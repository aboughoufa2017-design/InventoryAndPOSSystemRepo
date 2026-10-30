using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Pos;
using Warehouse.Application.Security;
using Warehouse.Application.Tenancy;
using Warehouse.Domain.Catalog;
using Warehouse.Domain.Inventory;
using Warehouse.Domain.Pos;
using Warehouse.Domain.Precision;
using Warehouse.Domain.Warehousing;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Infrastructure.Pos;

public sealed class PosCheckoutService(
    WarehouseDbContext db,
    ITenantContext tenantContext,
    ICurrentUser currentUser) : IPosCheckoutService
{
    private const string InventoryIssuePermission = "inventory.issue";

    public async Task<AppResult<PosSaleResponse>> FinalizeCashSaleAsync(
        FinalizePosCashSaleCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<PosSaleResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (command.ClientCommandId == Guid.Empty)
        {
            return AppResult<PosSaleResponse>.Failure(ErrorCodes.ValidationFailed, "ClientCommandId is required.");
        }

        var replay = await TryReplayAsync(tenantId, command.ClientCommandId, cancellationToken);
        if (replay is not null)
        {
            return replay;
        }

        if (command.Lines is null || command.Lines.Count == 0)
        {
            return AppResult<PosSaleResponse>.Failure(ErrorCodes.ValidationFailed, "At least one line is required.");
        }

        if (command.Lines.Any(l => l.Quantity <= 0m || l.DiscountAmount < 0m || l.UnitPrice is < 0m))
        {
            return AppResult<PosSaleResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "Line quantity must be positive; price and discount cannot be negative.");
        }

        if (command.PaymentMethod == PosPaymentMethodDto.Credit && command.CustomerId is null)
        {
            return AppResult<PosSaleResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "A customer is required for credit sales.");
        }

        var warehouse = await db.Warehouses.FirstOrDefaultAsync(w => w.Id == command.WarehouseId, cancellationToken);
        if (warehouse is null)
        {
            return AppResult<PosSaleResponse>.Failure(ErrorCodes.ValidationFailed, "Warehouse was not found.");
        }

        var locations = await db.WarehouseLocations
            .Where(l => l.WarehouseId == warehouse.Id)
            .ToListAsync(cancellationToken);
        var location = ResolveAvailableLocation(locations, command.LocationId);
        if (location is null)
        {
            return AppResult<PosSaleResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "Location was not found or is not available for sale.");
        }

        if (command.CustomerId is Guid customerId
            && !await db.Customers.AnyAsync(c => c.Id == customerId && c.IsActive, cancellationToken))
        {
            return AppResult<PosSaleResponse>.Failure(ErrorCodes.ValidationFailed, "Customer was not found or is inactive.");
        }

        if (command.CashShiftId is Guid shiftId)
        {
            var shiftOpen = await db.Set<CashShift>().AsNoTracking().AnyAsync(
                s => s.TenantId == tenantId
                     && s.Id == shiftId
                     && s.WarehouseId == warehouse.Id
                     && s.Status == CashShiftStatus.Open,
                cancellationToken);
            if (!shiftOpen)
            {
                return AppResult<PosSaleResponse>.Failure(
                    ErrorCodes.ValidationFailed,
                    "Cash shift was not found, is closed, or belongs to another warehouse.");
            }
        }

        var productIds = command.Lines.Select(l => l.ProductId).Distinct().ToList();
        var products = await db.Products
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToListAsync(cancellationToken);
        if (products.Count != productIds.Count)
        {
            return AppResult<PosSaleResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "One or more products were not found or inactive.");
        }

        var tenant = await db.Tenants.FirstAsync(t => t.Id == tenantId, cancellationToken);
        var allowNegative = currentUser.HasPermission(InventoryIssuePermission);
        var utcNow = DateTimeOffset.UtcNow;
        var saleId = Guid.NewGuid();
        var receiptNumber = BuildReceiptNumber(utcNow);

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var lines = new List<PosSaleLine>();
            decimal subtotal = 0m, discountTotal = 0m, taxTotal = 0m;
            foreach (var input in command.Lines)
            {
                var product = products.First(p => p.Id == input.ProductId);
                var unitPrice = Round(input.UnitPrice ?? product.SalePrice);
                var gross = Round(input.Quantity * unitPrice);
                var discount = Round(input.DiscountAmount);
                if (discount > gross)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return AppResult<PosSaleResponse>.Failure(
                        ErrorCodes.ValidationFailed,
                        $"Discount exceeds line amount for product {product.Sku}.");
                }

                var net = gross - discount;
                var tax = ProductTaxCalculator.CalculateTaxAmount(tenant, product, net);
                var lineTotal = net + tax;

                lines.Add(PosSaleLine.Create(
                    Guid.NewGuid(),
                    tenantId,
                    saleId,
                    product.Id,
                    product.Sku,
                    product.Name,
                    input.Quantity,
                    unitPrice,
                    discount,
                    tax,
                    lineTotal));

                subtotal += gross;
                discountTotal += discount;
                taxTotal += tax;
            }

            var grandTotal = subtotal - discountTotal + taxTotal;
            var currency = string.IsNullOrWhiteSpace(command.CurrencyCode) ? tenant.CurrencyCode : command.CurrencyCode;

            var sale = PosSale.CreateCompleted(
                saleId,
                tenantId,
                warehouse.Id,
                location.Id,
                command.ClientCommandId,
                receiptNumber,
                currency,
                subtotal,
                discountTotal,
                taxTotal,
                grandTotal,
                utcNow,
                command.DeviceId,
                currentUser.UserId,
                command.CustomerId,
                command.CashShiftId,
                command.Notes);

            var payment = PosPayment.Create(
                Guid.NewGuid(),
                tenantId,
                saleId,
                command.PaymentMethod == PosPaymentMethodDto.Credit ? PosPaymentMethod.Credit : PosPaymentMethod.Cash,
                sale.GrandTotal,
                null);

            db.Set<PosSale>().Add(sale);
            db.Set<PosSaleLine>().AddRange(lines);
            db.Set<PosPayment>().Add(payment);

            // Stock: tracked products only. Balances are cached so repeated products share one row.
            var balances = new Dictionary<Guid, StockBalance>();
            foreach (var line in lines)
            {
                var product = products.First(p => p.Id == line.ProductId);
                if (!product.TrackInventory)
                {
                    continue;
                }

                var balance = await GetOrCreateBalanceAsync(
                    balances, tenantId, product.Id, warehouse.Id, location.Id, utcNow, cancellationToken);
                var resulting = balance.OnHand - line.Quantity;
                if (!NegativeStockEvaluator.IsAllowed(tenant.NegativeStockPolicy, resulting, allowNegative))
                {
                    await tx.RollbackAsync(cancellationToken);
                    return AppResult<PosSaleResponse>.Failure(
                        ErrorCodes.InsufficientStock,
                        $"Insufficient stock for product {product.Sku}.");
                }

                balance.ApplyOnHandDelta(-line.Quantity, utcNow);

                db.StockMovements.Add(StockMovement.Create(
                    Guid.NewGuid(), tenantId, product.Id, warehouse.Id, location.Id,
                    -line.Quantity, StockMovementType.SalesShipment, SourceDocumentTypes.Sale,
                    saleId, null, currentUser.UserId, $"POS sale {receiptNumber}", utcNow));

                if (balance.OnHand < 0m)
                {
                    db.Set<PosInventoryConflict>().Add(PosInventoryConflict.Open(
                        Guid.NewGuid(),
                        tenantId,
                        warehouse.Id,
                        product.Id,
                        balance.OnHand,
                        $"POS sale {receiptNumber} drove {product.Sku} on-hand to {balance.OnHand}.",
                        utcNow,
                        command.ClientCommandId,
                        saleId));
                }
            }

            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return AppResult<PosSaleResponse>.Success(Map(sale, lines, [payment], false));
        }
        catch (ArgumentException ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return AppResult<PosSaleResponse>.Failure(ErrorCodes.ValidationFailed, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return AppResult<PosSaleResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync(cancellationToken);
            db.ChangeTracker.Clear();
            var raced = await TryReplayAsync(tenantId, command.ClientCommandId, cancellationToken);
            return raced ?? AppResult<PosSaleResponse>.Failure(ErrorCodes.InvalidOperation, "Duplicate operation.");
        }
    }

    public async Task<AppResult<PosSaleResponse>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<PosSaleResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        var sale = await db.Set<PosSale>().AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id, cancellationToken);
        if (sale is null)
        {
            return AppResult<PosSaleResponse>.Failure(ErrorCodes.InvalidOperation, "POS sale was not found.");
        }

        return AppResult<PosSaleResponse>.Success(await LoadResponseAsync(sale, false, cancellationToken));
    }

    public async Task<IReadOnlyList<PosSaleResponse>> ListAsync(
        Guid? warehouseId,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            throw new InvalidOperationException("Tenant context is required.");
        }

        var query = db.Set<PosSale>().AsNoTracking().Where(s => s.TenantId == tenantId);
        if (warehouseId is Guid wid)
        {
            query = query.Where(s => s.WarehouseId == wid);
        }

        if (fromUtc is DateTimeOffset from)
        {
            query = query.Where(s => s.SoldAtUtc >= from);
        }

        if (toUtc is DateTimeOffset to)
        {
            query = query.Where(s => s.SoldAtUtc <= to);
        }

        var sales = await query.OrderByDescending(s => s.SoldAtUtc).Take(200).ToListAsync(cancellationToken);
        var ids = sales.Select(s => s.Id).ToList();
        var lines = await db.Set<PosSaleLine>().AsNoTracking()
            .Where(l => l.TenantId == tenantId && ids.Contains(l.PosSaleId))
            .ToListAsync(cancellationToken);
        var payments = await db.Set<PosPayment>().AsNoTracking()
            .Where(p => p.TenantId == tenantId && ids.Contains(p.PosSaleId))
            .ToListAsync(cancellationToken);

        return sales
            .Select(s => Map(
                s,
                lines.Where(l => l.PosSaleId == s.Id).ToList(),
                payments.Where(p => p.PosSaleId == s.Id).ToList(),
                false))
            .ToList();
    }

    public async Task<AppResult<PosSaleResponse>> VoidAsync(
        VoidPosSaleCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<PosSaleResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        var sale = await db.Set<PosSale>()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == command.PosSaleId, cancellationToken);
        if (sale is null)
        {
            return AppResult<PosSaleResponse>.Failure(ErrorCodes.InvalidOperation, "POS sale was not found.");
        }

        if (sale.Status == PosSaleStatus.Voided)
        {
            await tx.RollbackAsync(cancellationToken);
            return AppResult<PosSaleResponse>.Success(await LoadResponseAsync(sale, true, cancellationToken));
        }

        var utcNow = DateTimeOffset.UtcNow;
        try
        {
            sale.Void(utcNow);

            var shipments = await db.StockMovements
                .Where(m => m.SourceDocumentId == sale.Id
                            && m.SourceDocumentType == SourceDocumentTypes.Sale
                            && m.MovementType == StockMovementType.SalesShipment)
                .ToListAsync(cancellationToken);

            var balances = new Dictionary<Guid, StockBalance>();
            foreach (var movement in shipments)
            {
                var restore = -movement.QuantityChange;
                var balance = await GetOrCreateBalanceAsync(
                    balances, tenantId, movement.ProductId, movement.WarehouseId, movement.LocationId,
                    utcNow, cancellationToken);
                balance.ApplyOnHandDelta(restore, utcNow);

                db.StockMovements.Add(StockMovement.Create(
                    Guid.NewGuid(), tenantId, movement.ProductId, movement.WarehouseId, movement.LocationId,
                    restore, StockMovementType.SalesReturn, SourceDocumentTypes.Sale,
                    sale.Id, null, currentUser.UserId, $"POS void {sale.ReceiptNumber}", utcNow));
            }

            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return AppResult<PosSaleResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }
        catch (ArgumentException ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return AppResult<PosSaleResponse>.Failure(ErrorCodes.ValidationFailed, ex.Message);
        }

        return AppResult<PosSaleResponse>.Success(await LoadResponseAsync(sale, false, cancellationToken));
    }

    public async Task<AppResult<PosSaleResponse>> RefundAsync(
        RefundPosSaleCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<PosSaleResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (command.ClientCommandId == Guid.Empty)
        {
            return AppResult<PosSaleResponse>.Failure(ErrorCodes.ValidationFailed, "ClientCommandId is required.");
        }

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        var sale = await db.Set<PosSale>()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == command.PosSaleId, cancellationToken);
        if (sale is null)
        {
            return AppResult<PosSaleResponse>.Failure(ErrorCodes.InvalidOperation, "POS sale was not found.");
        }

        if (sale.Status == PosSaleStatus.Refunded)
        {
            await tx.RollbackAsync(cancellationToken);
            return AppResult<PosSaleResponse>.Success(await LoadResponseAsync(sale, true, cancellationToken));
        }

        var saleLines = await db.Set<PosSaleLine>()
            .Where(l => l.TenantId == tenantId && l.PosSaleId == sale.Id)
            .ToListAsync(cancellationToken);

        // Empty refund line list means refund every line in full.
        var requested = command.Lines is { Count: > 0 }
            ? command.Lines
                .GroupBy(l => l.PosSaleLineId)
                .Select(g => new PosRefundLineInput(g.Key, g.Sum(x => x.Quantity)))
                .ToList()
            : saleLines.Select(l => new PosRefundLineInput(l.Id, l.Quantity)).ToList();

        var tenantProductIds = saleLines.Select(l => l.ProductId).Distinct().ToList();
        var tracked = await db.Products.AsNoTracking()
            .Where(p => tenantProductIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.TrackInventory, cancellationToken);

        var utcNow = DateTimeOffset.UtcNow;
        try
        {
            sale.MarkRefunded(utcNow);

            var balances = new Dictionary<Guid, StockBalance>();
            foreach (var refund in requested)
            {
                var line = saleLines.FirstOrDefault(l => l.Id == refund.PosSaleLineId);
                if (line is null)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return AppResult<PosSaleResponse>.Failure(ErrorCodes.ValidationFailed, "POS sale line was not found.");
                }

                if (refund.Quantity <= 0m || refund.Quantity > line.Quantity)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return AppResult<PosSaleResponse>.Failure(
                        ErrorCodes.ValidationFailed,
                        $"Invalid refund quantity for line {line.SkuSnapshot}.");
                }

                if (!tracked.GetValueOrDefault(line.ProductId))
                {
                    continue;
                }

                var balance = await GetOrCreateBalanceAsync(
                    balances, tenantId, line.ProductId, sale.WarehouseId, sale.LocationId, utcNow, cancellationToken);
                balance.ApplyOnHandDelta(refund.Quantity, utcNow);

                db.StockMovements.Add(StockMovement.Create(
                    Guid.NewGuid(), tenantId, line.ProductId, sale.WarehouseId, sale.LocationId,
                    refund.Quantity, StockMovementType.SalesReturn, SourceDocumentTypes.Sale,
                    sale.Id, null, currentUser.UserId, $"POS refund {sale.ReceiptNumber}", utcNow));
            }

            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return AppResult<PosSaleResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }
        catch (ArgumentException ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return AppResult<PosSaleResponse>.Failure(ErrorCodes.ValidationFailed, ex.Message);
        }

        return AppResult<PosSaleResponse>.Success(await LoadResponseAsync(sale, false, cancellationToken));
    }

    private async Task<AppResult<PosSaleResponse>?> TryReplayAsync(
        Guid tenantId,
        Guid clientCommandId,
        CancellationToken cancellationToken)
    {
        var existing = await db.Set<PosSale>().AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.ClientCommandId == clientCommandId, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        return AppResult<PosSaleResponse>.Success(await LoadResponseAsync(existing, true, cancellationToken));
    }

    private async Task<PosSaleResponse> LoadResponseAsync(
        PosSale sale,
        bool replay,
        CancellationToken cancellationToken)
    {
        var lines = await db.Set<PosSaleLine>().AsNoTracking()
            .Where(l => l.TenantId == sale.TenantId && l.PosSaleId == sale.Id)
            .ToListAsync(cancellationToken);
        var payments = await db.Set<PosPayment>().AsNoTracking()
            .Where(p => p.TenantId == sale.TenantId && p.PosSaleId == sale.Id)
            .ToListAsync(cancellationToken);
        return Map(sale, lines, payments, replay);
    }

    private async Task<StockBalance> GetOrCreateBalanceAsync(
        Dictionary<Guid, StockBalance> cache,
        Guid tenantId,
        Guid productId,
        Guid warehouseId,
        Guid locationId,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken)
    {
        // Keyed by product only: a single operation always uses one warehouse/location per product line
        // except void, where movements for the same product share the sale's location.
        if (cache.TryGetValue(productId, out var cached)
            && cached.WarehouseId == warehouseId
            && cached.LocationId == locationId)
        {
            return cached;
        }

        var balance = await db.StockBalances.FirstOrDefaultAsync(
            b => b.ProductId == productId && b.WarehouseId == warehouseId && b.LocationId == locationId,
            cancellationToken);
        if (balance is null)
        {
            balance = StockBalance.CreateEmpty(Guid.NewGuid(), tenantId, productId, warehouseId, locationId, utcNow);
            db.StockBalances.Add(balance);
        }

        cache[productId] = balance;
        return balance;
    }

    private static string BuildReceiptNumber(DateTimeOffset utcNow) =>
        $"POS-{utcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

    private static decimal Round(decimal value) =>
        Math.Round(value, MoneyPrecision.Scale, MidpointRounding.AwayFromZero);

    private static WarehouseLocation? ResolveAvailableLocation(
        IReadOnlyList<WarehouseLocation> locations,
        Guid? requestedId)
    {
        WarehouseLocation? location;
        if (requestedId is Guid id)
        {
            location = locations.FirstOrDefault(l => l.Id == id);
        }
        else
        {
            location = locations.FirstOrDefault(l => l.Code == WarehouseLocationCodes.General)
                       ?? locations.FirstOrDefault(l => l.IsDefault);
        }

        return location is { IsAvailableForSaleOrPick: true } ? location : null;
    }

    private static PosSaleResponse Map(
        PosSale sale,
        IReadOnlyList<PosSaleLine> lines,
        IReadOnlyList<PosPayment> payments,
        bool wasIdempotentReplay) =>
        new(
            sale.Id,
            sale.WarehouseId,
            sale.LocationId,
            sale.DeviceId,
            sale.CustomerId,
            sale.CashShiftId,
            sale.ClientCommandId,
            sale.ReceiptNumber,
            sale.Status.ToString(),
            sale.CurrencyCode,
            sale.Subtotal,
            sale.DiscountTotal,
            sale.TaxTotal,
            sale.GrandTotal,
            sale.SoldAtUtc,
            sale.Notes,
            sale.RowVersion,
            wasIdempotentReplay,
            lines.Select(l => new PosSaleLineResponse(
                l.Id,
                l.ProductId,
                l.SkuSnapshot,
                l.NameSnapshot,
                l.Quantity,
                l.UnitPrice,
                l.DiscountAmount,
                l.TaxAmount,
                l.LineTotal)).ToList(),
            payments.Select(p => new PosPaymentResponse(
                p.Id,
                p.Method.ToString(),
                p.Amount,
                p.Status,
                p.Reference)).ToList());
}

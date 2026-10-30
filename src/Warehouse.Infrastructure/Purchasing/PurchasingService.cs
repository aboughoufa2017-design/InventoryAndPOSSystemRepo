using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Purchasing;
using Warehouse.Application.Security;
using Warehouse.Application.Tenancy;
using Warehouse.Domain.Catalog;
using Warehouse.Domain.Inventory;
using Warehouse.Domain.Purchasing;
using Warehouse.Domain.Tenancy;
using Warehouse.Domain.Warehousing;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Infrastructure.Purchasing;

public sealed class PurchasingService(
    WarehouseDbContext db,
    ITenantContext tenantContext,
    ICurrentUser currentUser) : IPurchasingService
{
    public async Task<AppResult<PurchaseOrderResponse>> CreateAsync(
        CreatePurchaseOrderCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (command.Lines is null || command.Lines.Count == 0)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.ValidationFailed, "At least one line is required.");
        }

        var supplier = await db.Suppliers.FirstOrDefaultAsync(
            s => s.Id == command.SupplierId && s.IsActive,
            cancellationToken);
        if (supplier is null)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.ValidationFailed, "Supplier was not found or is inactive.");
        }

        var warehouse = await db.Warehouses.FirstOrDefaultAsync(w => w.Id == command.WarehouseId, cancellationToken);
        if (warehouse is null)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.ValidationFailed, "Warehouse was not found.");
        }

        var locations = await db.WarehouseLocations
            .Where(l => l.WarehouseId == warehouse.Id)
            .ToListAsync(cancellationToken);
        var location = ResolveAvailableLocation(locations, command.LocationId);
        if (location is null)
        {
            return AppResult<PurchaseOrderResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "Location was not found or is not available for put-away.");
        }

        var productIds = command.Lines.Select(l => l.ProductId).Distinct().ToList();
        if (productIds.Count != command.Lines.Count)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.ValidationFailed, "Duplicate products are not allowed.");
        }

        var products = await db.Products
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToListAsync(cancellationToken);
        if (products.Count != productIds.Count)
        {
            return AppResult<PurchaseOrderResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "One or more products were not found or inactive.");
        }

        var tenant = await db.Tenants.FirstAsync(t => t.Id == tenantId, cancellationToken);
        var utcNow = DateTimeOffset.UtcNow;

        PurchaseOrder order;
        List<PurchaseOrderLine> lines;
        try
        {
            order = PurchaseOrder.Create(
                Guid.NewGuid(),
                tenantId,
                supplier.Id,
                warehouse.Id,
                location.Id,
                command.Reference,
                command.Notes,
                currentUser.UserId,
                utcNow);

            lines = [];
            foreach (var request in command.Lines)
            {
                var product = products.First(p => p.Id == request.ProductId);
                var unitPrice = request.UnitPrice ?? product.PurchasePrice;
                var taxRate = ProductTaxCalculator.ResolveApplicableRate(tenant, product);
                lines.Add(PurchaseOrderLine.Create(
                    Guid.NewGuid(),
                    tenantId,
                    order.Id,
                    product.Id,
                    request.Quantity,
                    unitPrice,
                    taxRate,
                    request.Notes));
            }

            order.SetTotals(lines.Sum(l => l.LineNet), lines.Sum(l => l.LineTax), utcNow);
        }
        catch (ArgumentException ex)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.ValidationFailed, ex.Message);
        }

        db.PurchaseOrders.Add(order);
        db.PurchaseOrderLines.AddRange(lines);
        await db.SaveChangesAsync(cancellationToken);
        return AppResult<PurchaseOrderResponse>.Success(Map(order, lines));
    }

    public async Task<AppResult<PurchaseOrderResponse>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        EnsureTenant();
        var order = await db.PurchaseOrders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (order is null)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.InvalidOperation, "Purchase order was not found.");
        }

        var lines = await db.PurchaseOrderLines.AsNoTracking()
            .Where(l => l.PurchaseOrderId == id)
            .ToListAsync(cancellationToken);
        return AppResult<PurchaseOrderResponse>.Success(Map(order, lines));
    }

    public async Task<IReadOnlyList<PurchaseOrderResponse>> ListAsync(string? status, CancellationToken cancellationToken)
    {
        EnsureTenant();
        var query = db.PurchaseOrders.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PurchaseOrderStatus>(status, true, out var parsed))
        {
            query = query.Where(o => o.Status == parsed);
        }

        var orders = await query.OrderByDescending(o => o.CreatedAtUtc).Take(100).ToListAsync(cancellationToken);
        var ids = orders.Select(o => o.Id).ToList();
        var lines = await db.PurchaseOrderLines.AsNoTracking()
            .Where(l => ids.Contains(l.PurchaseOrderId))
            .ToListAsync(cancellationToken);
        return orders.Select(o => Map(o, lines.Where(l => l.PurchaseOrderId == o.Id).ToList())).ToList();
    }

    public async Task<AppResult<PurchaseOrderResponse>> ConfirmAsync(
        ConfirmPurchaseOrderCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (command.OperationId is Guid opId)
        {
            var replay = await TryReplayAsync(opId, cancellationToken);
            if (replay is not null)
            {
                return replay;
            }
        }

        var order = await db.PurchaseOrders.FirstOrDefaultAsync(o => o.Id == command.PurchaseOrderId, cancellationToken);
        if (order is null)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.InvalidOperation, "Purchase order was not found.");
        }

        try
        {
            order.EnsureRowVersion(command.ExpectedRowVersion);
        }
        catch (InvalidOperationException)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.SyncConflict, "Purchase order was modified by another request.");
        }

        var lines = await db.PurchaseOrderLines.Where(l => l.PurchaseOrderId == order.Id).ToListAsync(cancellationToken);
        if (lines.Count == 0)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.ValidationFailed, "Purchase order has no lines.");
        }

        var utcNow = DateTimeOffset.UtcNow;
        try
        {
            order.Confirm(utcNow);
            var response = Map(order, lines);
            StoreOperation(command.OperationId, tenantId, order.Id, PurchaseOperationActions.Confirm, response, utcNow);
            await db.SaveChangesAsync(cancellationToken);
            return AppResult<PurchaseOrderResponse>.Success(response);
        }
        catch (InvalidOperationException ex)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }
        catch (DbUpdateException) when (command.OperationId is not null)
        {
            var replay = await TryReplayAsync(command.OperationId.Value, cancellationToken);
            return replay ?? AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.InvalidOperation, "Duplicate operation.");
        }
    }

    public async Task<AppResult<PurchaseOrderResponse>> ReceiveAsync(
        ReceivePurchaseOrderCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (command.OperationId is Guid opId)
        {
            var replay = await TryReplayAsync(opId, cancellationToken);
            if (replay is not null)
            {
                return replay;
            }
        }

        if (command.Lines is null || command.Lines.Count == 0)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.ValidationFailed, "At least one receive line is required.");
        }

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        var order = await db.PurchaseOrders.FirstOrDefaultAsync(o => o.Id == command.PurchaseOrderId, cancellationToken);
        if (order is null)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.InvalidOperation, "Purchase order was not found.");
        }

        try
        {
            order.EnsureRowVersion(command.ExpectedRowVersion);
        }
        catch (InvalidOperationException)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.SyncConflict, "Purchase order was modified by another request.");
        }

        if (order.Status is not (PurchaseOrderStatus.Confirmed or PurchaseOrderStatus.PartiallyReceived))
        {
            return AppResult<PurchaseOrderResponse>.Failure(
                ErrorCodes.InvalidOperation,
                $"Purchase order is {order.Status} and cannot receive.");
        }

        var warehouseId = command.WarehouseId ?? order.WarehouseId;
        var locations = await db.WarehouseLocations.Where(l => l.WarehouseId == warehouseId).ToListAsync(cancellationToken);
        var locationIdHint = command.LocationId ?? (warehouseId == order.WarehouseId ? order.LocationId : null);
        var location = ResolveAvailableLocation(locations, locationIdHint);
        if (location is null || !await db.Warehouses.AnyAsync(w => w.Id == warehouseId, cancellationToken))
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.ValidationFailed, "Receive warehouse/location was not found.");
        }

        var lines = await db.PurchaseOrderLines.Where(l => l.PurchaseOrderId == order.Id).ToListAsync(cancellationToken);
        var utcNow = DateTimeOffset.UtcNow;
        var receipt = PurchaseReceipt.Create(
            Guid.NewGuid(),
            tenantId,
            order.Id,
            warehouseId,
            location.Id,
            command.Notes,
            currentUser.UserId,
            command.OperationId,
            utcNow);
        db.PurchaseReceipts.Add(receipt);

        try
        {
            foreach (var receiveLine in command.Lines)
            {
                var line = lines.FirstOrDefault(l => l.Id == receiveLine.PurchaseOrderLineId);
                if (line is null)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.ValidationFailed, "Purchase order line was not found.");
                }

                line.Receive(receiveLine.Quantity);

                var stock = await ApplyOnHandChangeAsync(
                    tenantId, line.ProductId, warehouseId, location.Id, receiveLine.Quantity,
                    NegativeStockPolicy.Allow, true, utcNow, cancellationToken);
                if (!stock.IsSuccess)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return AppResult<PurchaseOrderResponse>.Failure(stock.ErrorCode!, stock.ErrorMessage!);
                }

                db.PurchaseReceiptLines.Add(PurchaseReceiptLine.Create(
                    Guid.NewGuid(), tenantId, receipt.Id, line.Id, line.ProductId, receiveLine.Quantity));

                db.StockMovements.Add(StockMovement.Create(
                    Guid.NewGuid(), tenantId, line.ProductId, warehouseId, location.Id,
                    receiveLine.Quantity, StockMovementType.PurchaseReceipt, SourceDocumentTypes.Purchase,
                    receipt.Id, null, currentUser.UserId, "Purchase receipt", utcNow));
            }

            order.ApplyReceiveProgress(lines.Any(l => l.QuantityRemainingToReceive > 0m), utcNow);
            var response = Map(order, lines);
            StoreOperation(command.OperationId, tenantId, order.Id, PurchaseOperationActions.Receive, response, utcNow);
            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return AppResult<PurchaseOrderResponse>.Success(response);
        }
        catch (ArgumentException ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.ValidationFailed, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }
        catch (DbUpdateException) when (command.OperationId is not null)
        {
            await tx.RollbackAsync(cancellationToken);
            var replay = await TryReplayAsync(command.OperationId.Value, cancellationToken);
            return replay ?? AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.InvalidOperation, "Duplicate operation.");
        }
    }

    public async Task<AppResult<PurchaseOrderResponse>> ReturnAsync(
        ReturnPurchaseOrderCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (command.OperationId is Guid opId)
        {
            var replay = await TryReplayAsync(opId, cancellationToken);
            if (replay is not null)
            {
                return replay;
            }
        }

        if (command.Lines is null || command.Lines.Count == 0)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.ValidationFailed, "At least one return line is required.");
        }

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        var order = await db.PurchaseOrders.FirstOrDefaultAsync(o => o.Id == command.PurchaseOrderId, cancellationToken);
        if (order is null)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.InvalidOperation, "Purchase order was not found.");
        }

        try
        {
            order.EnsureRowVersion(command.ExpectedRowVersion);
        }
        catch (InvalidOperationException)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.SyncConflict, "Purchase order was modified by another request.");
        }

        var warehouseId = command.WarehouseId ?? order.WarehouseId;
        var locations = await db.WarehouseLocations.Where(l => l.WarehouseId == warehouseId).ToListAsync(cancellationToken);
        var locationIdHint = command.LocationId ?? (warehouseId == order.WarehouseId ? order.LocationId : null);
        var location = ResolveAvailableLocation(locations, locationIdHint);
        if (location is null)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.ValidationFailed, "Return location was not found.");
        }

        var lines = await db.PurchaseOrderLines.Where(l => l.PurchaseOrderId == order.Id).ToListAsync(cancellationToken);
        var tenant = await db.Tenants.FirstAsync(t => t.Id == tenantId, cancellationToken);
        var allowNegative = currentUser.HasPermission("purchases.approve");
        var utcNow = DateTimeOffset.UtcNow;
        var purchaseReturn = PurchaseReturn.Create(
            Guid.NewGuid(),
            tenantId,
            order.Id,
            warehouseId,
            location.Id,
            command.Notes,
            currentUser.UserId,
            command.OperationId,
            utcNow);
        db.PurchaseReturns.Add(purchaseReturn);

        try
        {
            order.RegisterReturn(utcNow);

            foreach (var returnLine in command.Lines)
            {
                var line = lines.FirstOrDefault(l => l.Id == returnLine.PurchaseOrderLineId);
                if (line is null)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.ValidationFailed, "Purchase order line was not found.");
                }

                line.Return(returnLine.Quantity);

                var stock = await ApplyOnHandChangeAsync(
                    tenantId, line.ProductId, warehouseId, location.Id, -returnLine.Quantity,
                    tenant.NegativeStockPolicy, allowNegative, utcNow, cancellationToken);
                if (!stock.IsSuccess)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return AppResult<PurchaseOrderResponse>.Failure(stock.ErrorCode!, stock.ErrorMessage!);
                }

                db.PurchaseReturnLines.Add(PurchaseReturnLine.Create(
                    Guid.NewGuid(), tenantId, purchaseReturn.Id, line.Id, line.ProductId, returnLine.Quantity));

                db.StockMovements.Add(StockMovement.Create(
                    Guid.NewGuid(), tenantId, line.ProductId, warehouseId, location.Id,
                    -returnLine.Quantity, StockMovementType.PurchaseReturn, SourceDocumentTypes.Purchase,
                    purchaseReturn.Id, null, currentUser.UserId, "Purchase return", utcNow));
            }

            var response = Map(order, lines);
            StoreOperation(command.OperationId, tenantId, order.Id, PurchaseOperationActions.Return, response, utcNow);
            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return AppResult<PurchaseOrderResponse>.Success(response);
        }
        catch (ArgumentException ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.ValidationFailed, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }
        catch (DbUpdateException) when (command.OperationId is not null)
        {
            await tx.RollbackAsync(cancellationToken);
            var replay = await TryReplayAsync(command.OperationId.Value, cancellationToken);
            return replay ?? AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.InvalidOperation, "Duplicate operation.");
        }
    }

    public async Task<AppResult<PurchaseOrderResponse>> CloseAsync(
        ClosePurchaseOrderCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (command.OperationId is Guid opId)
        {
            var replay = await TryReplayAsync(opId, cancellationToken);
            if (replay is not null)
            {
                return replay;
            }
        }

        var order = await db.PurchaseOrders.FirstOrDefaultAsync(o => o.Id == command.PurchaseOrderId, cancellationToken);
        if (order is null)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.InvalidOperation, "Purchase order was not found.");
        }

        try
        {
            order.EnsureRowVersion(command.ExpectedRowVersion);
        }
        catch (InvalidOperationException)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.SyncConflict, "Purchase order was modified by another request.");
        }

        var lines = await db.PurchaseOrderLines.Where(l => l.PurchaseOrderId == order.Id).ToListAsync(cancellationToken);
        var utcNow = DateTimeOffset.UtcNow;
        try
        {
            order.Close(utcNow);
            var response = Map(order, lines);
            StoreOperation(command.OperationId, tenantId, order.Id, PurchaseOperationActions.Close, response, utcNow);
            await db.SaveChangesAsync(cancellationToken);
            return AppResult<PurchaseOrderResponse>.Success(response);
        }
        catch (InvalidOperationException ex)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }
    }

    public async Task<AppResult<PurchaseOrderResponse>> CancelAsync(
        CancelPurchaseOrderCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (command.OperationId is Guid opId)
        {
            var replay = await TryReplayAsync(opId, cancellationToken);
            if (replay is not null)
            {
                return replay;
            }
        }

        var order = await db.PurchaseOrders.FirstOrDefaultAsync(o => o.Id == command.PurchaseOrderId, cancellationToken);
        if (order is null)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.InvalidOperation, "Purchase order was not found.");
        }

        try
        {
            order.EnsureRowVersion(command.ExpectedRowVersion);
        }
        catch (InvalidOperationException)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.SyncConflict, "Purchase order was modified by another request.");
        }

        var lines = await db.PurchaseOrderLines.Where(l => l.PurchaseOrderId == order.Id).ToListAsync(cancellationToken);
        if (lines.Any(l => l.QuantityReceived > 0m))
        {
            return AppResult<PurchaseOrderResponse>.Failure(
                ErrorCodes.InvalidOperation,
                "Cannot cancel a purchase order that has received stock. Create a return instead.");
        }

        var utcNow = DateTimeOffset.UtcNow;
        try
        {
            order.Cancel(utcNow);
            var response = Map(order, lines);
            StoreOperation(command.OperationId, tenantId, order.Id, PurchaseOperationActions.Cancel, response, utcNow);
            await db.SaveChangesAsync(cancellationToken);
            return AppResult<PurchaseOrderResponse>.Success(response);
        }
        catch (InvalidOperationException ex)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }
    }

    public async Task<AppResult<PurchaseOrderResponse>> UpdatePaymentAsync(
        UpdatePurchasePaymentCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (command.OperationId is Guid opId)
        {
            var replay = await TryReplayAsync(opId, cancellationToken);
            if (replay is not null)
            {
                return replay;
            }
        }

        var order = await db.PurchaseOrders.FirstOrDefaultAsync(o => o.Id == command.PurchaseOrderId, cancellationToken);
        if (order is null)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.InvalidOperation, "Purchase order was not found.");
        }

        try
        {
            order.EnsureRowVersion(command.ExpectedRowVersion);
        }
        catch (InvalidOperationException)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.SyncConflict, "Purchase order was modified by another request.");
        }

        var lines = await db.PurchaseOrderLines.Where(l => l.PurchaseOrderId == order.Id).ToListAsync(cancellationToken);
        var utcNow = DateTimeOffset.UtcNow;
        try
        {
            order.ApplyPayment(command.PaidAmount, utcNow);
            var response = Map(order, lines);
            StoreOperation(command.OperationId, tenantId, order.Id, PurchaseOperationActions.Payment, response, utcNow);
            await db.SaveChangesAsync(cancellationToken);
            return AppResult<PurchaseOrderResponse>.Success(response);
        }
        catch (ArgumentException ex)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.ValidationFailed, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }
    }

    private void StoreOperation(
        Guid? operationId,
        Guid tenantId,
        Guid purchaseOrderId,
        string action,
        PurchaseOrderResponse response,
        DateTimeOffset utcNow)
    {
        if (operationId is not Guid id)
        {
            return;
        }

        db.PurchaseOperations.Add(PurchaseOperation.Create(
            id,
            tenantId,
            purchaseOrderId,
            action,
            JsonSerializer.Serialize(response, PurchasingJson.Options),
            utcNow));
    }

    private async Task<AppResult<PurchaseOrderResponse>?> TryReplayAsync(Guid operationId, CancellationToken cancellationToken)
    {
        var existing = await db.PurchaseOperations.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == operationId, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        var payload = JsonSerializer.Deserialize<PurchaseOrderResponse>(existing.ResultPayloadJson, PurchasingJson.Options);
        if (payload is null)
        {
            return AppResult<PurchaseOrderResponse>.Failure(ErrorCodes.InternalError, "Idempotent payload could not be read.");
        }

        return AppResult<PurchaseOrderResponse>.Success(payload with { WasIdempotentReplay = true });
    }

    private async Task<AppResult> ApplyOnHandChangeAsync(
        Guid tenantId,
        Guid productId,
        Guid warehouseId,
        Guid locationId,
        decimal quantityChange,
        NegativeStockPolicy policy,
        bool allowNegativePermission,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken)
    {
        var balance = await db.StockBalances.FirstOrDefaultAsync(
            b => b.ProductId == productId && b.WarehouseId == warehouseId && b.LocationId == locationId,
            cancellationToken);

        if (balance is null)
        {
            balance = StockBalance.CreateEmpty(Guid.NewGuid(), tenantId, productId, warehouseId, locationId, utcNow);
            db.StockBalances.Add(balance);
        }

        var resulting = balance.OnHand + quantityChange;
        if (!NegativeStockEvaluator.IsAllowed(policy, resulting, allowNegativePermission))
        {
            return AppResult.Failure(ErrorCodes.InsufficientStock, "Insufficient stock for purchase return.");
        }

        balance.ApplyOnHandDelta(quantityChange, utcNow);
        return AppResult.Success();
    }

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

        if (location is null || !location.IsAvailableForSaleOrPick)
        {
            return null;
        }

        return location;
    }

    private static PurchaseOrderResponse Map(PurchaseOrder order, IReadOnlyList<PurchaseOrderLine> lines) =>
        new(
            order.Id,
            order.SupplierId,
            order.WarehouseId,
            order.LocationId,
            order.Status.ToString(),
            order.PaymentStatus.ToString(),
            order.Reference,
            order.Notes,
            order.Subtotal,
            order.TaxTotal,
            order.GrandTotal,
            order.PaidAmount,
            order.CreatedAtUtc,
            order.ConfirmedAtUtc,
            order.ClosedAtUtc,
            order.RowVersion,
            lines.Select(l => new PurchaseOrderLineResponse(
                l.Id,
                l.ProductId,
                l.QuantityOrdered,
                l.QuantityReceived,
                l.QuantityReturned,
                l.QuantityRemainingToReceive,
                l.UnitPrice,
                l.TaxRate,
                l.LineNet,
                l.LineTax,
                l.LineTotal,
                l.Notes)).ToList());

    private void EnsureTenant()
    {
        if (tenantContext.TenantId is null)
        {
            throw new InvalidOperationException("Tenant context is required.");
        }
    }
}

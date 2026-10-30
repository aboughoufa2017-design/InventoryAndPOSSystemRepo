using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Sales;
using Warehouse.Application.Security;
using Warehouse.Application.Tenancy;
using Warehouse.Domain.Catalog;
using Warehouse.Domain.Inventory;
using Warehouse.Domain.Sales;
using Warehouse.Domain.Tenancy;
using Warehouse.Domain.Warehousing;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Infrastructure.Sales;

public sealed class SalesService(
    WarehouseDbContext db,
    ITenantContext tenantContext,
    ICurrentUser currentUser) : ISalesService
{
    public async Task<AppResult<SalesOrderResponse>> CreateAsync(
        CreateSalesOrderCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (command.Lines is null || command.Lines.Count == 0)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.ValidationFailed, "At least one line is required.");
        }

        var customer = await db.Customers.FirstOrDefaultAsync(
            c => c.Id == command.CustomerId && c.IsActive,
            cancellationToken);
        if (customer is null)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.ValidationFailed, "Customer was not found or is inactive.");
        }

        var warehouse = await db.Warehouses.FirstOrDefaultAsync(w => w.Id == command.WarehouseId, cancellationToken);
        if (warehouse is null)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.ValidationFailed, "Warehouse was not found.");
        }

        var locations = await db.WarehouseLocations
            .Where(l => l.WarehouseId == warehouse.Id)
            .ToListAsync(cancellationToken);
        var location = ResolveAvailableLocation(locations, command.LocationId);
        if (location is null)
        {
            return AppResult<SalesOrderResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "Location was not found or is not available for pick.");
        }

        var productIds = command.Lines.Select(l => l.ProductId).Distinct().ToList();
        if (productIds.Count != command.Lines.Count)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.ValidationFailed, "Duplicate products are not allowed.");
        }

        var products = await db.Products
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToListAsync(cancellationToken);
        if (products.Count != productIds.Count)
        {
            return AppResult<SalesOrderResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "One or more products were not found or inactive.");
        }

        var tenant = await db.Tenants.FirstAsync(t => t.Id == tenantId, cancellationToken);
        var utcNow = DateTimeOffset.UtcNow;

        SalesOrder order;
        List<SalesOrderLine> lines;
        try
        {
            order = SalesOrder.Create(
                Guid.NewGuid(),
                tenantId,
                customer.Id,
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
                var unitPrice = request.UnitPrice ?? product.SalePrice;
                var taxRate = ProductTaxCalculator.ResolveApplicableRate(tenant, product);
                lines.Add(SalesOrderLine.Create(
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
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.ValidationFailed, ex.Message);
        }

        db.SalesOrders.Add(order);
        db.SalesOrderLines.AddRange(lines);
        await db.SaveChangesAsync(cancellationToken);
        return AppResult<SalesOrderResponse>.Success(Map(order, lines));
    }

    public async Task<AppResult<SalesOrderResponse>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        EnsureTenant();
        var order = await db.SalesOrders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (order is null)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.InvalidOperation, "Sales order was not found.");
        }

        var lines = await db.SalesOrderLines.AsNoTracking()
            .Where(l => l.SalesOrderId == id)
            .ToListAsync(cancellationToken);
        return AppResult<SalesOrderResponse>.Success(Map(order, lines));
    }

    public async Task<IReadOnlyList<SalesOrderResponse>> ListAsync(string? status, CancellationToken cancellationToken)
    {
        EnsureTenant();
        var query = db.SalesOrders.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<SalesOrderStatus>(status, true, out var parsed))
        {
            query = query.Where(o => o.Status == parsed);
        }

        var orders = await query.OrderByDescending(o => o.CreatedAtUtc).Take(100).ToListAsync(cancellationToken);
        var ids = orders.Select(o => o.Id).ToList();
        var lines = await db.SalesOrderLines.AsNoTracking()
            .Where(l => ids.Contains(l.SalesOrderId))
            .ToListAsync(cancellationToken);
        return orders.Select(o => Map(o, lines.Where(l => l.SalesOrderId == o.Id).ToList())).ToList();
    }

    public async Task<AppResult<SalesOrderResponse>> ConfirmAsync(
        ConfirmSalesOrderCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (command.OperationId is Guid opId)
        {
            var replay = await TryReplayAsync(opId, cancellationToken);
            if (replay is not null)
            {
                return replay;
            }
        }

        var order = await db.SalesOrders.FirstOrDefaultAsync(o => o.Id == command.SalesOrderId, cancellationToken);
        if (order is null)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.InvalidOperation, "Sales order was not found.");
        }

        try
        {
            order.EnsureRowVersion(command.ExpectedRowVersion);
        }
        catch (InvalidOperationException)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.SyncConflict, "Sales order was modified by another request.");
        }

        var lines = await db.SalesOrderLines.Where(l => l.SalesOrderId == order.Id).ToListAsync(cancellationToken);
        if (lines.Count == 0)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.ValidationFailed, "Sales order has no lines.");
        }

        var utcNow = DateTimeOffset.UtcNow;
        try
        {
            order.Confirm(utcNow);
            var response = Map(order, lines);
            StoreOperation(command.OperationId, tenantId, order.Id, SalesOperationActions.Confirm, response, utcNow);
            await db.SaveChangesAsync(cancellationToken);
            return AppResult<SalesOrderResponse>.Success(response);
        }
        catch (InvalidOperationException ex)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }
        catch (DbUpdateException) when (command.OperationId is not null)
        {
            var replay = await TryReplayAsync(command.OperationId.Value, cancellationToken);
            return replay ?? AppResult<SalesOrderResponse>.Failure(ErrorCodes.InvalidOperation, "Duplicate operation.");
        }
    }

    public async Task<AppResult<SalesOrderResponse>> ShipAsync(
        ShipSalesOrderCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
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
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.ValidationFailed, "At least one ship line is required.");
        }

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        var order = await db.SalesOrders.FirstOrDefaultAsync(o => o.Id == command.SalesOrderId, cancellationToken);
        if (order is null)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.InvalidOperation, "Sales order was not found.");
        }

        try
        {
            order.EnsureRowVersion(command.ExpectedRowVersion);
        }
        catch (InvalidOperationException)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.SyncConflict, "Sales order was modified by another request.");
        }

        if (order.Status is not (SalesOrderStatus.Confirmed or SalesOrderStatus.PartiallyFulfilled))
        {
            return AppResult<SalesOrderResponse>.Failure(
                ErrorCodes.InvalidOperation,
                $"Sales order is {order.Status} and cannot ship.");
        }

        var warehouseId = command.WarehouseId ?? order.WarehouseId;
        var locations = await db.WarehouseLocations.Where(l => l.WarehouseId == warehouseId).ToListAsync(cancellationToken);
        var locationIdHint = command.LocationId ?? (warehouseId == order.WarehouseId ? order.LocationId : null);
        var location = ResolveAvailableLocation(locations, locationIdHint);
        if (location is null || !await db.Warehouses.AnyAsync(w => w.Id == warehouseId, cancellationToken))
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.ValidationFailed, "Ship warehouse/location was not found.");
        }

        var lines = await db.SalesOrderLines.Where(l => l.SalesOrderId == order.Id).ToListAsync(cancellationToken);
        var tenant = await db.Tenants.FirstAsync(t => t.Id == tenantId, cancellationToken);
        var allowNegative = currentUser.HasPermission("inventory.issue");
        var utcNow = DateTimeOffset.UtcNow;
        var shipment = SalesShipment.Create(
            Guid.NewGuid(),
            tenantId,
            order.Id,
            warehouseId,
            location.Id,
            command.Notes,
            currentUser.UserId,
            command.OperationId,
            utcNow);
        db.SalesShipments.Add(shipment);

        try
        {
            foreach (var shipLine in command.Lines)
            {
                var line = lines.FirstOrDefault(l => l.Id == shipLine.SalesOrderLineId);
                if (line is null)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return AppResult<SalesOrderResponse>.Failure(ErrorCodes.ValidationFailed, "Sales order line was not found.");
                }

                line.Ship(shipLine.Quantity);

                var stock = await ApplyOnHandChangeAsync(
                    tenantId, line.ProductId, warehouseId, location.Id, -shipLine.Quantity,
                    tenant.NegativeStockPolicy, allowNegative, utcNow, cancellationToken);
                if (!stock.IsSuccess)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return AppResult<SalesOrderResponse>.Failure(stock.ErrorCode!, stock.ErrorMessage!);
                }

                db.SalesShipmentLines.Add(SalesShipmentLine.Create(
                    Guid.NewGuid(), tenantId, shipment.Id, line.Id, line.ProductId, shipLine.Quantity));

                db.StockMovements.Add(StockMovement.Create(
                    Guid.NewGuid(), tenantId, line.ProductId, warehouseId, location.Id,
                    -shipLine.Quantity, StockMovementType.SalesShipment, SourceDocumentTypes.Sale,
                    shipment.Id, null, currentUser.UserId, "Sales shipment", utcNow));
            }

            order.ApplyShipProgress(lines.Any(l => l.QuantityRemainingToShip > 0m), utcNow);
            var response = Map(order, lines);
            StoreOperation(command.OperationId, tenantId, order.Id, SalesOperationActions.Ship, response, utcNow);
            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return AppResult<SalesOrderResponse>.Success(response);
        }
        catch (ArgumentException ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.ValidationFailed, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }
        catch (DbUpdateException) when (command.OperationId is not null)
        {
            await tx.RollbackAsync(cancellationToken);
            var replay = await TryReplayAsync(command.OperationId.Value, cancellationToken);
            return replay ?? AppResult<SalesOrderResponse>.Failure(ErrorCodes.InvalidOperation, "Duplicate operation.");
        }
    }

    public async Task<AppResult<SalesOrderResponse>> ReturnAsync(
        ReturnSalesOrderCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
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
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.ValidationFailed, "At least one return line is required.");
        }

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        var order = await db.SalesOrders.FirstOrDefaultAsync(o => o.Id == command.SalesOrderId, cancellationToken);
        if (order is null)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.InvalidOperation, "Sales order was not found.");
        }

        try
        {
            order.EnsureRowVersion(command.ExpectedRowVersion);
        }
        catch (InvalidOperationException)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.SyncConflict, "Sales order was modified by another request.");
        }

        var warehouseId = command.WarehouseId ?? order.WarehouseId;
        var locations = await db.WarehouseLocations.Where(l => l.WarehouseId == warehouseId).ToListAsync(cancellationToken);
        var locationIdHint = command.LocationId ?? (warehouseId == order.WarehouseId ? order.LocationId : null);
        var location = ResolveAvailableLocation(locations, locationIdHint);
        if (location is null)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.ValidationFailed, "Return location was not found.");
        }

        var lines = await db.SalesOrderLines.Where(l => l.SalesOrderId == order.Id).ToListAsync(cancellationToken);
        var utcNow = DateTimeOffset.UtcNow;
        var salesReturn = SalesReturn.Create(
            Guid.NewGuid(),
            tenantId,
            order.Id,
            warehouseId,
            location.Id,
            command.Notes,
            currentUser.UserId,
            command.OperationId,
            utcNow);
        db.SalesReturns.Add(salesReturn);

        try
        {
            order.RegisterReturn(utcNow);

            foreach (var returnLine in command.Lines)
            {
                var line = lines.FirstOrDefault(l => l.Id == returnLine.SalesOrderLineId);
                if (line is null)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return AppResult<SalesOrderResponse>.Failure(ErrorCodes.ValidationFailed, "Sales order line was not found.");
                }

                line.Return(returnLine.Quantity);

                var stock = await ApplyOnHandChangeAsync(
                    tenantId, line.ProductId, warehouseId, location.Id, returnLine.Quantity,
                    NegativeStockPolicy.Allow, true, utcNow, cancellationToken);
                if (!stock.IsSuccess)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return AppResult<SalesOrderResponse>.Failure(stock.ErrorCode!, stock.ErrorMessage!);
                }

                db.SalesReturnLines.Add(SalesReturnLine.Create(
                    Guid.NewGuid(), tenantId, salesReturn.Id, line.Id, line.ProductId, returnLine.Quantity));

                db.StockMovements.Add(StockMovement.Create(
                    Guid.NewGuid(), tenantId, line.ProductId, warehouseId, location.Id,
                    returnLine.Quantity, StockMovementType.SalesReturn, SourceDocumentTypes.Sale,
                    salesReturn.Id, null, currentUser.UserId, "Sales return", utcNow));
            }

            var response = Map(order, lines);
            StoreOperation(command.OperationId, tenantId, order.Id, SalesOperationActions.Return, response, utcNow);
            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return AppResult<SalesOrderResponse>.Success(response);
        }
        catch (ArgumentException ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.ValidationFailed, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }
        catch (DbUpdateException) when (command.OperationId is not null)
        {
            await tx.RollbackAsync(cancellationToken);
            var replay = await TryReplayAsync(command.OperationId.Value, cancellationToken);
            return replay ?? AppResult<SalesOrderResponse>.Failure(ErrorCodes.InvalidOperation, "Duplicate operation.");
        }
    }

    public async Task<AppResult<SalesOrderResponse>> CloseAsync(
        CloseSalesOrderCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (command.OperationId is Guid opId)
        {
            var replay = await TryReplayAsync(opId, cancellationToken);
            if (replay is not null)
            {
                return replay;
            }
        }

        var order = await db.SalesOrders.FirstOrDefaultAsync(o => o.Id == command.SalesOrderId, cancellationToken);
        if (order is null)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.InvalidOperation, "Sales order was not found.");
        }

        try
        {
            order.EnsureRowVersion(command.ExpectedRowVersion);
        }
        catch (InvalidOperationException)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.SyncConflict, "Sales order was modified by another request.");
        }

        var lines = await db.SalesOrderLines.Where(l => l.SalesOrderId == order.Id).ToListAsync(cancellationToken);
        var utcNow = DateTimeOffset.UtcNow;
        try
        {
            order.Close(utcNow);
            var response = Map(order, lines);
            StoreOperation(command.OperationId, tenantId, order.Id, SalesOperationActions.Close, response, utcNow);
            await db.SaveChangesAsync(cancellationToken);
            return AppResult<SalesOrderResponse>.Success(response);
        }
        catch (InvalidOperationException ex)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }
    }

    public async Task<AppResult<SalesOrderResponse>> CancelAsync(
        CancelSalesOrderCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (command.OperationId is Guid opId)
        {
            var replay = await TryReplayAsync(opId, cancellationToken);
            if (replay is not null)
            {
                return replay;
            }
        }

        var order = await db.SalesOrders.FirstOrDefaultAsync(o => o.Id == command.SalesOrderId, cancellationToken);
        if (order is null)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.InvalidOperation, "Sales order was not found.");
        }

        try
        {
            order.EnsureRowVersion(command.ExpectedRowVersion);
        }
        catch (InvalidOperationException)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.SyncConflict, "Sales order was modified by another request.");
        }

        var lines = await db.SalesOrderLines.Where(l => l.SalesOrderId == order.Id).ToListAsync(cancellationToken);
        if (lines.Any(l => l.QuantityShipped > 0m))
        {
            return AppResult<SalesOrderResponse>.Failure(
                ErrorCodes.InvalidOperation,
                "Cannot cancel a sales order that has shipped stock. Create a return instead.");
        }

        var utcNow = DateTimeOffset.UtcNow;
        try
        {
            order.Cancel(utcNow);
            var response = Map(order, lines);
            StoreOperation(command.OperationId, tenantId, order.Id, SalesOperationActions.Cancel, response, utcNow);
            await db.SaveChangesAsync(cancellationToken);
            return AppResult<SalesOrderResponse>.Success(response);
        }
        catch (InvalidOperationException ex)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }
    }

    public async Task<AppResult<SalesOrderResponse>> UpdatePaymentAsync(
        UpdateSalesPaymentCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (command.OperationId is Guid opId)
        {
            var replay = await TryReplayAsync(opId, cancellationToken);
            if (replay is not null)
            {
                return replay;
            }
        }

        var order = await db.SalesOrders.FirstOrDefaultAsync(o => o.Id == command.SalesOrderId, cancellationToken);
        if (order is null)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.InvalidOperation, "Sales order was not found.");
        }

        try
        {
            order.EnsureRowVersion(command.ExpectedRowVersion);
        }
        catch (InvalidOperationException)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.SyncConflict, "Sales order was modified by another request.");
        }

        var lines = await db.SalesOrderLines.Where(l => l.SalesOrderId == order.Id).ToListAsync(cancellationToken);
        var utcNow = DateTimeOffset.UtcNow;
        try
        {
            order.ApplyPayment(command.PaidAmount, utcNow);
            var response = Map(order, lines);
            StoreOperation(command.OperationId, tenantId, order.Id, SalesOperationActions.Payment, response, utcNow);
            await db.SaveChangesAsync(cancellationToken);
            return AppResult<SalesOrderResponse>.Success(response);
        }
        catch (ArgumentException ex)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.ValidationFailed, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }
    }

    private void StoreOperation(
        Guid? operationId,
        Guid tenantId,
        Guid salesOrderId,
        string action,
        SalesOrderResponse response,
        DateTimeOffset utcNow)
    {
        if (operationId is not Guid id)
        {
            return;
        }

        db.SalesOperations.Add(SalesOperation.Create(
            id,
            tenantId,
            salesOrderId,
            action,
            JsonSerializer.Serialize(response, SalesJson.Options),
            utcNow));
    }

    private async Task<AppResult<SalesOrderResponse>?> TryReplayAsync(Guid operationId, CancellationToken cancellationToken)
    {
        var existing = await db.SalesOperations.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == operationId, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        var payload = JsonSerializer.Deserialize<SalesOrderResponse>(existing.ResultPayloadJson, SalesJson.Options);
        if (payload is null)
        {
            return AppResult<SalesOrderResponse>.Failure(ErrorCodes.InternalError, "Idempotent payload could not be read.");
        }

        return AppResult<SalesOrderResponse>.Success(payload with { WasIdempotentReplay = true });
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
            return AppResult.Failure(ErrorCodes.InsufficientStock, "Insufficient stock for sales shipment.");
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

    private static SalesOrderResponse Map(SalesOrder order, IReadOnlyList<SalesOrderLine> lines) =>
        new(
            order.Id,
            order.CustomerId,
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
            lines.Select(l => new SalesOrderLineResponse(
                l.Id,
                l.ProductId,
                l.QuantityOrdered,
                l.QuantityShipped,
                l.QuantityReturned,
                l.QuantityRemainingToShip,
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

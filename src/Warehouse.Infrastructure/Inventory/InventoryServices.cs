using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Inventory;
using Warehouse.Application.Security;
using Warehouse.Application.Tenancy;
using Warehouse.Domain.Inventory;
using Warehouse.Domain.Warehousing;
using Warehouse.Infrastructure.Persistence;
using WarehouseEntity = Warehouse.Domain.Warehousing.Warehouse;

namespace Warehouse.Infrastructure.Inventory;

public sealed class WarehouseService(
    WarehouseDbContext db,
    ITenantContext tenantContext) : IWarehouseService
{
    public async Task<AppResult<WarehouseResponse>> CreateAsync(
        CreateWarehouseCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<WarehouseResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (string.IsNullOrWhiteSpace(command.Code) || string.IsNullOrWhiteSpace(command.Name))
        {
            return AppResult<WarehouseResponse>.Failure(ErrorCodes.ValidationFailed, "Code and name are required.");
        }

        var code = command.Code.Trim().ToUpperInvariant();
        if (await db.Warehouses.AnyAsync(w => w.Code == code, cancellationToken))
        {
            return AppResult<WarehouseResponse>.Failure(ErrorCodes.InvalidOperation, "Warehouse code already exists.");
        }

        var utcNow = DateTimeOffset.UtcNow;
        var warehouseId = Guid.NewGuid();
        var warehouse = WarehouseEntity.Create(warehouseId, tenantId, code, command.Name, utcNow);
        var (general, inTransit) = WarehouseLocationFactory.CreateRequiredLocations(tenantId, warehouseId, utcNow);

        db.Warehouses.Add(warehouse);
        db.WarehouseLocations.Add(general);
        db.WarehouseLocations.Add(inTransit);
        await db.SaveChangesAsync(cancellationToken);

        return AppResult<WarehouseResponse>.Success(
            new WarehouseResponse(warehouse.Id, warehouse.Code, warehouse.Name, warehouse.IsActive, general.Id, inTransit.Id));
    }

    public async Task<AppResult<WarehouseResponse>> UpdateAsync(
        UpdateWarehouseCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return AppResult<WarehouseResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return AppResult<WarehouseResponse>.Failure(ErrorCodes.ValidationFailed, "Name is required.");
        }

        var warehouse = await db.Warehouses.FirstOrDefaultAsync(w => w.Id == command.Id, cancellationToken);
        if (warehouse is null)
        {
            return AppResult<WarehouseResponse>.Failure(ErrorCodes.InvalidOperation, "Warehouse not found.");
        }

        warehouse.Update(command.Name, command.IsActive, DateTimeOffset.UtcNow);
        await db.SaveChangesAsync(cancellationToken);

        var locations = await db.WarehouseLocations.AsNoTracking()
            .Where(l => l.WarehouseId == warehouse.Id)
            .ToListAsync(cancellationToken);
        var general = locations.First(l => l.Code == WarehouseLocationCodes.General);
        var inTransit = locations.First(l => l.Code == WarehouseLocationCodes.InTransit);

        return AppResult<WarehouseResponse>.Success(
            new WarehouseResponse(warehouse.Id, warehouse.Code, warehouse.Name, warehouse.IsActive, general.Id, inTransit.Id));
    }

    public async Task<IReadOnlyList<WarehouseResponse>> ListAsync(CancellationToken cancellationToken)
    {
        EnsureTenant();
        var warehouses = await db.Warehouses.AsNoTracking().OrderBy(w => w.Code).ToListAsync(cancellationToken);
        var result = new List<WarehouseResponse>();
        foreach (var warehouse in warehouses)
        {
            var locations = await db.WarehouseLocations.AsNoTracking()
                .Where(l => l.WarehouseId == warehouse.Id)
                .ToListAsync(cancellationToken);
            var general = locations.First(l => l.Code == WarehouseLocationCodes.General);
            var inTransit = locations.First(l => l.Code == WarehouseLocationCodes.InTransit);
            result.Add(new WarehouseResponse(
                warehouse.Id, warehouse.Code, warehouse.Name, warehouse.IsActive, general.Id, inTransit.Id));
        }

        return result;
    }

    public async Task<IReadOnlyList<WarehouseLocationResponse>> ListLocationsAsync(
        Guid warehouseId,
        CancellationToken cancellationToken)
    {
        EnsureTenant();
        return await db.WarehouseLocations.AsNoTracking()
            .Where(l => l.WarehouseId == warehouseId)
            .OrderBy(l => l.Code)
            .Select(l => new WarehouseLocationResponse(
                l.Id, l.WarehouseId, l.Code, l.Name, l.Kind.ToString(), l.IsDefault, l.IsActive))
            .ToListAsync(cancellationToken);
    }

    private void EnsureTenant()
    {
        if (tenantContext.TenantId is null)
        {
            throw new InvalidOperationException("Tenant context is required.");
        }
    }
}

public sealed class InventoryService(
    WarehouseDbContext db,
    ITenantContext tenantContext,
    ICurrentUser currentUser) : IInventoryService
{
    public async Task<IReadOnlyList<StockBalanceResponse>> ListBalancesAsync(
        Guid? warehouseId,
        Guid? productId,
        CancellationToken cancellationToken)
    {
        EnsureTenant();
        var query = db.StockBalances.AsNoTracking().AsQueryable();
        if (warehouseId is Guid wid)
        {
            query = query.Where(b => b.WarehouseId == wid);
        }

        if (productId is Guid pid)
        {
            query = query.Where(b => b.ProductId == pid);
        }

        return await query
            .OrderBy(b => b.WarehouseId)
            .ThenBy(b => b.ProductId)
            .Take(500)
            .Select(b => new StockBalanceResponse(
                b.Id, b.ProductId, b.WarehouseId, b.LocationId, b.OnHand, b.Reserved, b.OnHand - b.Reserved, b.RowVersion))
            .ToListAsync(cancellationToken);
    }

    public async Task<AppResult<StockBalanceResponse>> GetBalanceAsync(
        Guid productId,
        Guid warehouseId,
        Guid locationId,
        CancellationToken cancellationToken)
    {
        EnsureTenant();
        var balance = await db.StockBalances.AsNoTracking()
            .FirstOrDefaultAsync(
                b => b.ProductId == productId && b.WarehouseId == warehouseId && b.LocationId == locationId,
                cancellationToken);

        if (balance is null)
        {
            return AppResult<StockBalanceResponse>.Success(
                new StockBalanceResponse(Guid.Empty, productId, warehouseId, locationId, 0m, 0m, 0m, 0));
        }

        return AppResult<StockBalanceResponse>.Success(MapBalance(balance));
    }

    public async Task<IReadOnlyList<StockMovementResponse>> ListMovementsAsync(
        Guid? warehouseId,
        Guid? productId,
        int take,
        CancellationToken cancellationToken)
    {
        EnsureTenant();
        take = Math.Clamp(take <= 0 ? 50 : take, 1, 200);
        var query = db.StockMovements.AsNoTracking().AsQueryable();
        if (warehouseId is Guid wid)
        {
            query = query.Where(m => m.WarehouseId == wid);
        }

        if (productId is Guid pid)
        {
            query = query.Where(m => m.ProductId == pid);
        }

        return await query
            .OrderByDescending(m => m.OccurredAtUtc)
            .Take(take)
            .Select(m => new StockMovementResponse(
                m.Id, m.ProductId, m.WarehouseId, m.LocationId, m.QuantityChange,
                m.MovementType.ToString(), m.SourceDocumentType, m.SourceDocumentId, m.TransferLineId,
                m.Notes, m.OccurredAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<AppResult<StockAdjustmentResponse>> CreateAdjustmentAsync(
        CreateStockAdjustmentCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<StockAdjustmentResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (!Enum.TryParse<StockAdjustmentKind>(command.Kind, ignoreCase: true, out var kind))
        {
            return AppResult<StockAdjustmentResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "Kind must be OpeningBalance or Correction.");
        }

        if (command.Lines is null || command.Lines.Count == 0)
        {
            return AppResult<StockAdjustmentResponse>.Failure(ErrorCodes.ValidationFailed, "At least one line is required.");
        }

        var warehouse = await db.Warehouses.FirstOrDefaultAsync(w => w.Id == command.WarehouseId, cancellationToken);
        if (warehouse is null)
        {
            return AppResult<StockAdjustmentResponse>.Failure(ErrorCodes.ValidationFailed, "Warehouse was not found.");
        }

        var locationIds = command.Lines.Select(l => l.LocationId).Distinct().ToList();
        var locations = await db.WarehouseLocations
            .Where(l => l.WarehouseId == warehouse.Id && locationIds.Contains(l.Id))
            .ToListAsync(cancellationToken);
        if (locations.Count != locationIds.Count)
        {
            return AppResult<StockAdjustmentResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "One or more locations do not belong to the warehouse.");
        }

        var productIds = command.Lines.Select(l => l.ProductId).Distinct().ToList();
        var products = await db.Products.Where(p => productIds.Contains(p.Id) && p.IsActive && p.TrackInventory)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);
        if (products.Count != productIds.Count)
        {
            return AppResult<StockAdjustmentResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "One or more products were not found, inactive, or do not track inventory.");
        }

        var utcNow = DateTimeOffset.UtcNow;
        var adjustment = StockAdjustment.Create(
            Guid.NewGuid(),
            tenantId,
            warehouse.Id,
            kind,
            command.Reason,
            currentUser.UserId,
            utcNow);

        var lines = new List<StockAdjustmentLine>();
        try
        {
            foreach (var line in command.Lines)
            {
                lines.Add(StockAdjustmentLine.Create(
                    Guid.NewGuid(),
                    tenantId,
                    adjustment.Id,
                    line.ProductId,
                    line.LocationId,
                    line.QuantityChange,
                    line.Notes));
            }
        }
        catch (ArgumentException ex)
        {
            return AppResult<StockAdjustmentResponse>.Failure(ErrorCodes.ValidationFailed, ex.Message);
        }

        if (lines.GroupBy(l => new { l.ProductId, l.LocationId }).Any(g => g.Count() > 1))
        {
            return AppResult<StockAdjustmentResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "Duplicate product/location lines are not allowed.");
        }

        db.StockAdjustments.Add(adjustment);
        db.StockAdjustmentLines.AddRange(lines);
        await db.SaveChangesAsync(cancellationToken);
        return AppResult<StockAdjustmentResponse>.Success(MapAdjustment(adjustment, lines));
    }

    public async Task<AppResult<StockAdjustmentResponse>> GetAdjustmentAsync(Guid id, CancellationToken cancellationToken)
    {
        EnsureTenant();
        var adjustment = await db.StockAdjustments.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (adjustment is null)
        {
            return AppResult<StockAdjustmentResponse>.Failure(ErrorCodes.InvalidOperation, "Adjustment was not found.");
        }

        var lines = await db.StockAdjustmentLines.AsNoTracking()
            .Where(l => l.StockAdjustmentId == id)
            .ToListAsync(cancellationToken);
        return AppResult<StockAdjustmentResponse>.Success(MapAdjustment(adjustment, lines));
    }

    public async Task<AppResult<StockAdjustmentResponse>> PostAdjustmentAsync(Guid id, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<StockAdjustmentResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

        var adjustment = await db.StockAdjustments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (adjustment is null)
        {
            return AppResult<StockAdjustmentResponse>.Failure(ErrorCodes.InvalidOperation, "Adjustment was not found.");
        }

        var lines = await db.StockAdjustmentLines.Where(l => l.StockAdjustmentId == id).ToListAsync(cancellationToken);
        if (lines.Count == 0)
        {
            return AppResult<StockAdjustmentResponse>.Failure(ErrorCodes.ValidationFailed, "Cannot post an adjustment with no lines.");
        }

        var tenant = await db.Tenants.FirstAsync(t => t.Id == tenantId, cancellationToken);
        var allowNegativePermission = currentUser.HasPermission("inventory.adjust");
        var utcNow = DateTimeOffset.UtcNow;
        var movementType = adjustment.Kind == StockAdjustmentKind.OpeningBalance
            ? StockMovementType.OpeningBalance
            : StockMovementType.Adjustment;

        try
        {
            foreach (var line in lines)
            {
                var balance = await db.StockBalances.FirstOrDefaultAsync(
                    b => b.ProductId == line.ProductId
                         && b.WarehouseId == adjustment.WarehouseId
                         && b.LocationId == line.LocationId,
                    cancellationToken);

                if (balance is null)
                {
                    balance = StockBalance.CreateEmpty(
                        Guid.NewGuid(),
                        tenantId,
                        line.ProductId,
                        adjustment.WarehouseId,
                        line.LocationId,
                        utcNow);
                    db.StockBalances.Add(balance);
                }

                var resultingOnHand = balance.OnHand + line.QuantityChange;
                if (!NegativeStockEvaluator.IsAllowed(
                        tenant.NegativeStockPolicy,
                        resultingOnHand,
                        allowNegativePermission))
                {
                    return AppResult<StockAdjustmentResponse>.Failure(
                        ErrorCodes.InsufficientStock,
                        $"Negative stock is not allowed for product {line.ProductId} at the selected location.");
                }

                balance.ApplyOnHandDelta(line.QuantityChange, utcNow);
                db.StockMovements.Add(StockMovement.Create(
                    Guid.NewGuid(),
                    tenantId,
                    line.ProductId,
                    adjustment.WarehouseId,
                    line.LocationId,
                    line.QuantityChange,
                    movementType,
                    SourceDocumentTypes.StockAdjustment,
                    adjustment.Id,
                    transferLineId: null,
                    currentUser.UserId,
                    line.Notes ?? adjustment.Reason,
                    utcNow));
            }

            adjustment.MarkPosted(utcNow);
            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return AppResult<StockAdjustmentResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }

        return AppResult<StockAdjustmentResponse>.Success(MapAdjustment(adjustment, lines));
    }

    public async Task<AppResult<StockAdjustmentResponse>> CancelAdjustmentAsync(Guid id, CancellationToken cancellationToken)
    {
        EnsureTenant();
        var adjustment = await db.StockAdjustments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (adjustment is null)
        {
            return AppResult<StockAdjustmentResponse>.Failure(ErrorCodes.InvalidOperation, "Adjustment was not found.");
        }

        try
        {
            adjustment.Cancel(DateTimeOffset.UtcNow);
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return AppResult<StockAdjustmentResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }

        var lines = await db.StockAdjustmentLines.AsNoTracking()
            .Where(l => l.StockAdjustmentId == id)
            .ToListAsync(cancellationToken);
        return AppResult<StockAdjustmentResponse>.Success(MapAdjustment(adjustment, lines));
    }

    private static StockBalanceResponse MapBalance(StockBalance balance) =>
        new(balance.Id, balance.ProductId, balance.WarehouseId, balance.LocationId,
            balance.OnHand, balance.Reserved, balance.Available, balance.RowVersion);

    private static StockAdjustmentResponse MapAdjustment(
        StockAdjustment adjustment,
        IReadOnlyList<StockAdjustmentLine> lines) =>
        new(
            adjustment.Id,
            adjustment.WarehouseId,
            adjustment.Kind.ToString(),
            adjustment.Status.ToString(),
            adjustment.Reason,
            adjustment.CreatedAtUtc,
            adjustment.PostedAtUtc,
            adjustment.RowVersion,
            lines.Select(l => new StockAdjustmentLineResponse(
                l.Id, l.ProductId, l.LocationId, l.QuantityChange, l.Notes)).ToList());

    private void EnsureTenant()
    {
        if (tenantContext.TenantId is null)
        {
            throw new InvalidOperationException("Tenant context is required.");
        }
    }
}

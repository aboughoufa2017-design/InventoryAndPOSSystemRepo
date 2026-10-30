using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Inventory;
using Warehouse.Application.Tenancy;
using Warehouse.Domain.Inventory;
using Warehouse.Domain.Warehousing;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Infrastructure.Inventory;

public sealed class StockCountService(
    WarehouseDbContext db,
    ITenantContext tenantContext,
    IInventoryService inventory) : IStockCountService
{
    public async Task<IReadOnlyList<StockCountResponse>> ListAsync(CancellationToken cancellationToken)
    {
        EnsureTenant();
        var counts = await db.StockCounts.AsNoTracking()
            .OrderByDescending(c => c.CreatedAtUtc)
            .Take(100)
            .ToListAsync(cancellationToken);
        var ids = counts.Select(c => c.Id).ToList();
        var lines = await db.StockCountLines.AsNoTracking()
            .Where(l => ids.Contains(l.StockCountId))
            .ToListAsync(cancellationToken);
        return counts.Select(c => Map(c, lines.Where(l => l.StockCountId == c.Id).ToList())).ToList();
    }

    public async Task<AppResult<StockCountResponse>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        EnsureTenant();
        var count = await db.StockCounts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (count is null)
        {
            return AppResult<StockCountResponse>.Failure(ErrorCodes.InvalidOperation, "Stock count was not found.");
        }

        var lines = await db.StockCountLines.AsNoTracking()
            .Where(l => l.StockCountId == id)
            .ToListAsync(cancellationToken);
        return AppResult<StockCountResponse>.Success(Map(count, lines));
    }

    public async Task<AppResult<StockCountResponse>> CreateAsync(
        CreateStockCountCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<StockCountResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (command.WarehouseId == Guid.Empty)
        {
            return AppResult<StockCountResponse>.Failure(ErrorCodes.ValidationFailed, "Warehouse is required.");
        }

        var warehouse = await db.Warehouses.FirstOrDefaultAsync(w => w.Id == command.WarehouseId, cancellationToken);
        if (warehouse is null)
        {
            return AppResult<StockCountResponse>.Failure(ErrorCodes.ValidationFailed, "Warehouse was not found.");
        }

        var locations = await db.WarehouseLocations
            .Where(l => l.WarehouseId == warehouse.Id)
            .ToListAsync(cancellationToken);

        WarehouseLocation? location;
        if (command.LocationId is Guid locationId)
        {
            location = locations.FirstOrDefault(l => l.Id == locationId);
            if (location is null)
            {
                return AppResult<StockCountResponse>.Failure(
                    ErrorCodes.ValidationFailed,
                    "Location was not found in the warehouse.");
            }
        }
        else
        {
            location = locations.FirstOrDefault(l => l.Code == WarehouseLocationCodes.General);
            if (location is null)
            {
                return AppResult<StockCountResponse>.Failure(
                    ErrorCodes.InvalidOperation,
                    "Warehouse is missing a GENERAL location.");
            }
        }

        var products = await db.Products.AsNoTracking()
            .Where(p => p.IsActive && p.TrackInventory)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var balances = await db.StockBalances.AsNoTracking()
            .Where(b => b.WarehouseId == warehouse.Id && b.LocationId == location.Id)
            .ToListAsync(cancellationToken);
        var onHandByProduct = balances.ToDictionary(b => b.ProductId, b => b.OnHand);

        var utcNow = DateTimeOffset.UtcNow;
        StockCount count;
        try
        {
            count = StockCount.Create(
                Guid.NewGuid(),
                tenantId,
                warehouse.Id,
                location.Id,
                command.Notes,
                utcNow);
        }
        catch (ArgumentException ex)
        {
            return AppResult<StockCountResponse>.Failure(ErrorCodes.ValidationFailed, ex.Message);
        }

        var lines = products.Select(productId =>
            StockCountLine.Create(
                Guid.NewGuid(),
                tenantId,
                count.Id,
                productId,
                onHandByProduct.GetValueOrDefault(productId, 0m),
                notes: null)).ToList();

        db.StockCounts.Add(count);
        db.StockCountLines.AddRange(lines);
        await db.SaveChangesAsync(cancellationToken);
        return AppResult<StockCountResponse>.Success(Map(count, lines));
    }

    public async Task<AppResult<StockCountResponse>> SetLinesAsync(
        SetStockCountLinesCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<StockCountResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (command.Lines is null || command.Lines.Count == 0)
        {
            return AppResult<StockCountResponse>.Failure(ErrorCodes.ValidationFailed, "At least one line is required.");
        }

        var count = await db.StockCounts.FirstOrDefaultAsync(c => c.Id == command.StockCountId, cancellationToken);
        if (count is null)
        {
            return AppResult<StockCountResponse>.Failure(ErrorCodes.InvalidOperation, "Stock count was not found.");
        }

        if (count.RowVersion != command.ExpectedRowVersion)
        {
            return AppResult<StockCountResponse>.Failure(
                ErrorCodes.SyncConflict,
                "Stock count was modified by another request.");
        }

        try
        {
            count.EnsureEditable();
        }
        catch (InvalidOperationException ex)
        {
            return AppResult<StockCountResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }

        if (command.Lines.GroupBy(l => l.ProductId).Any(g => g.Count() > 1))
        {
            return AppResult<StockCountResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "Duplicate products are not allowed.");
        }

        var existingLines = await db.StockCountLines
            .Where(l => l.StockCountId == count.Id)
            .ToListAsync(cancellationToken);
        var byProduct = existingLines.ToDictionary(l => l.ProductId);

        var productIds = command.Lines.Select(l => l.ProductId).Distinct().ToList();
        var missingProductIds = productIds.Where(id => !byProduct.ContainsKey(id)).ToList();
        Dictionary<Guid, decimal>? onHandByProduct = null;
        if (missingProductIds.Count > 0)
        {
            var validProducts = await db.Products
                .Where(p => missingProductIds.Contains(p.Id) && p.IsActive && p.TrackInventory)
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);
            if (validProducts.Count != missingProductIds.Count)
            {
                return AppResult<StockCountResponse>.Failure(
                    ErrorCodes.ValidationFailed,
                    "One or more products were not found, inactive, or do not track inventory.");
            }

            var balances = await db.StockBalances.AsNoTracking()
                .Where(b => b.WarehouseId == count.WarehouseId
                            && b.LocationId == count.LocationId
                            && missingProductIds.Contains(b.ProductId))
                .ToListAsync(cancellationToken);
            onHandByProduct = balances.ToDictionary(b => b.ProductId, b => b.OnHand);
        }

        foreach (var input in command.Lines)
        {
            if (byProduct.TryGetValue(input.ProductId, out var line))
            {
                line.SetCountedQuantity(input.CountedQuantity);
                continue;
            }

            line = StockCountLine.Create(
                Guid.NewGuid(),
                tenantId,
                count.Id,
                input.ProductId,
                onHandByProduct!.GetValueOrDefault(input.ProductId, 0m),
                input.Notes);
            line.SetCountedQuantity(input.CountedQuantity);
            db.StockCountLines.Add(line);
            byProduct[input.ProductId] = line;
            existingLines.Add(line);
        }

        var utcNow = DateTimeOffset.UtcNow;
        try
        {
            count.MarkCounted(utcNow);
        }
        catch (InvalidOperationException ex)
        {
            return AppResult<StockCountResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }

        await db.SaveChangesAsync(cancellationToken);
        return AppResult<StockCountResponse>.Success(Map(count, existingLines));
    }

    public async Task<AppResult<StockCountResponse>> PostAsync(
        PostStockCountCommand command,
        CancellationToken cancellationToken)
    {
        EnsureTenant();

        var count = await db.StockCounts.FirstOrDefaultAsync(c => c.Id == command.StockCountId, cancellationToken);
        if (count is null)
        {
            return AppResult<StockCountResponse>.Failure(ErrorCodes.InvalidOperation, "Stock count was not found.");
        }

        if (count.RowVersion != command.ExpectedRowVersion)
        {
            return AppResult<StockCountResponse>.Failure(
                ErrorCodes.SyncConflict,
                "Stock count was modified by another request.");
        }

        if (count.Status != StockCountStatus.Counted)
        {
            return AppResult<StockCountResponse>.Failure(
                ErrorCodes.InvalidOperation,
                "Only counted stock counts can be posted.");
        }

        var lines = await db.StockCountLines
            .Where(l => l.StockCountId == count.Id)
            .ToListAsync(cancellationToken);

        var varianceLines = lines
            .Where(l => l.CountedQuantity is not null && l.Variance != 0m)
            .ToList();

        Guid? adjustmentId = null;
        if (varianceLines.Count > 0)
        {
            var adjustmentLines = varianceLines.Select(l =>
                new StockAdjustmentLineRequest(
                    l.ProductId,
                    count.LocationId,
                    l.CountedQuantity!.Value - l.SystemQuantity,
                    l.Notes)).ToList();

            var createResult = await inventory.CreateAdjustmentAsync(
                new CreateStockAdjustmentCommand(
                    count.WarehouseId,
                    nameof(StockAdjustmentKind.Correction),
                    $"Stock count {count.Id}",
                    adjustmentLines),
                cancellationToken);
            if (!createResult.IsSuccess)
            {
                return AppResult<StockCountResponse>.Failure(createResult.ErrorCode!, createResult.ErrorMessage!);
            }

            var postResult = await inventory.PostAdjustmentAsync(createResult.Value!.Id, cancellationToken);
            if (!postResult.IsSuccess)
            {
                return AppResult<StockCountResponse>.Failure(postResult.ErrorCode!, postResult.ErrorMessage!);
            }

            adjustmentId = createResult.Value.Id;
        }

        var utcNow = DateTimeOffset.UtcNow;
        try
        {
            count.MarkPosted(adjustmentId, utcNow);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return AppResult<StockCountResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }

        await db.SaveChangesAsync(cancellationToken);
        return AppResult<StockCountResponse>.Success(Map(count, lines));
    }

    public async Task<AppResult<StockCountResponse>> CancelAsync(
        CancelStockCountCommand command,
        CancellationToken cancellationToken)
    {
        EnsureTenant();

        var count = await db.StockCounts.FirstOrDefaultAsync(c => c.Id == command.StockCountId, cancellationToken);
        if (count is null)
        {
            return AppResult<StockCountResponse>.Failure(ErrorCodes.InvalidOperation, "Stock count was not found.");
        }

        if (count.RowVersion != command.ExpectedRowVersion)
        {
            return AppResult<StockCountResponse>.Failure(
                ErrorCodes.SyncConflict,
                "Stock count was modified by another request.");
        }

        try
        {
            count.Cancel(DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            return AppResult<StockCountResponse>.Failure(ErrorCodes.InvalidOperation, ex.Message);
        }

        await db.SaveChangesAsync(cancellationToken);
        var lines = await db.StockCountLines.AsNoTracking()
            .Where(l => l.StockCountId == count.Id)
            .ToListAsync(cancellationToken);
        return AppResult<StockCountResponse>.Success(Map(count, lines));
    }

    private static StockCountResponse Map(StockCount count, IReadOnlyList<StockCountLine> lines) =>
        new(
            count.Id,
            count.WarehouseId,
            count.LocationId,
            count.Status.ToString(),
            count.Notes,
            count.CreatedAtUtc,
            count.UpdatedAtUtc,
            count.PostedAtUtc,
            count.PostedAdjustmentId,
            count.RowVersion,
            lines.Select(l => new StockCountLineResponse(
                l.Id,
                l.ProductId,
                l.SystemQuantity,
                l.CountedQuantity,
                l.Variance,
                l.Notes)).ToList());

    private void EnsureTenant()
    {
        if (tenantContext.TenantId is null)
        {
            throw new InvalidOperationException("Tenant context is required.");
        }
    }
}

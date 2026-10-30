using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Reporting;
using Warehouse.Application.Tenancy;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Infrastructure.Reporting;

public sealed class ReportService(
    WarehouseDbContext db,
    ITenantContext tenantContext) : IReportService
{
    public async Task<AppResult<IReadOnlyList<StockOnHandRow>>> StockOnHandAsync(
        Guid? warehouseId,
        CancellationToken cancellationToken)
    {
        EnsureTenant();

        var query =
            from balance in db.StockBalances.AsNoTracking()
            join product in db.Products.AsNoTracking() on balance.ProductId equals product.Id
            join warehouse in db.Warehouses.AsNoTracking() on balance.WarehouseId equals warehouse.Id
            join location in db.WarehouseLocations.AsNoTracking() on balance.LocationId equals location.Id
            select new { balance, product, warehouse, location };

        if (warehouseId is Guid wid)
        {
            query = query.Where(x => x.balance.WarehouseId == wid);
        }

        var rows = await query
            .OrderBy(x => x.warehouse.Code)
            .ThenBy(x => x.product.Sku)
            .ThenBy(x => x.location.Code)
            .Take(2000)
            .Select(x => new StockOnHandRow(
                x.product.Id,
                x.product.Sku,
                x.product.Name,
                x.warehouse.Id,
                x.warehouse.Code,
                x.location.Id,
                x.location.Code,
                x.balance.OnHand,
                x.balance.Reserved,
                x.balance.OnHand - x.balance.Reserved))
            .ToListAsync(cancellationToken);

        return AppResult<IReadOnlyList<StockOnHandRow>>.Success(rows);
    }

    public async Task<AppResult<IReadOnlyList<MovementReportRow>>> MovementsAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        Guid? warehouseId,
        CancellationToken cancellationToken)
    {
        EnsureTenant();

        if (toUtc < fromUtc)
        {
            return AppResult<IReadOnlyList<MovementReportRow>>.Failure(
                ErrorCodes.ValidationFailed,
                "toUtc must be greater than or equal to fromUtc.");
        }

        var query = db.StockMovements.AsNoTracking()
            .Where(m => m.OccurredAtUtc >= fromUtc && m.OccurredAtUtc <= toUtc);
        if (warehouseId is Guid wid)
        {
            query = query.Where(m => m.WarehouseId == wid);
        }

        var rows = await query
            .OrderByDescending(m => m.OccurredAtUtc)
            .Take(2000)
            .Select(m => new MovementReportRow(
                m.Id,
                m.OccurredAtUtc,
                m.MovementType.ToString(),
                m.ProductId,
                m.WarehouseId,
                m.LocationId,
                m.QuantityChange,
                m.SourceDocumentType,
                m.SourceDocumentId))
            .ToListAsync(cancellationToken);

        return AppResult<IReadOnlyList<MovementReportRow>>.Success(rows);
    }

    public async Task<AppResult<IReadOnlyList<DocumentSummaryRow>>> PurchaseSummaryAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken)
    {
        EnsureTenant();

        if (toUtc < fromUtc)
        {
            return AppResult<IReadOnlyList<DocumentSummaryRow>>.Failure(
                ErrorCodes.ValidationFailed,
                "toUtc must be greater than or equal to fromUtc.");
        }

        var rows = await db.PurchaseOrders.AsNoTracking()
            .Where(o => o.CreatedAtUtc >= fromUtc && o.CreatedAtUtc <= toUtc)
            .OrderByDescending(o => o.CreatedAtUtc)
            .Take(500)
            .Select(o => new DocumentSummaryRow(
                o.Id,
                "PurchaseOrder",
                o.Status.ToString(),
                o.GrandTotal,
                o.CreatedAtUtc,
                o.Reference))
            .ToListAsync(cancellationToken);

        return AppResult<IReadOnlyList<DocumentSummaryRow>>.Success(rows);
    }

    public async Task<AppResult<IReadOnlyList<DocumentSummaryRow>>> SalesSummaryAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken)
    {
        EnsureTenant();

        if (toUtc < fromUtc)
        {
            return AppResult<IReadOnlyList<DocumentSummaryRow>>.Failure(
                ErrorCodes.ValidationFailed,
                "toUtc must be greater than or equal to fromUtc.");
        }

        var rows = await db.SalesOrders.AsNoTracking()
            .Where(o => o.CreatedAtUtc >= fromUtc && o.CreatedAtUtc <= toUtc)
            .OrderByDescending(o => o.CreatedAtUtc)
            .Take(500)
            .Select(o => new DocumentSummaryRow(
                o.Id,
                "SalesOrder",
                o.Status.ToString(),
                o.GrandTotal,
                o.CreatedAtUtc,
                o.Reference))
            .ToListAsync(cancellationToken);

        return AppResult<IReadOnlyList<DocumentSummaryRow>>.Success(rows);
    }

    private void EnsureTenant()
    {
        if (tenantContext.TenantId is null)
        {
            throw new InvalidOperationException("Tenant context is required.");
        }
    }
}

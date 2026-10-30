using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Pos;
using Warehouse.Application.Tenancy;
using Warehouse.Domain.Pos;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Infrastructure.Pos;

public sealed class PosConflictService(
    WarehouseDbContext db,
    ITenantContext tenantContext) : IPosConflictService
{
    public async Task<IReadOnlyList<PosConflictResponse>> ListOpenAsync(CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            throw new InvalidOperationException("Tenant context is required.");
        }

        var conflicts = await db.Set<PosInventoryConflict>().AsNoTracking()
            .Where(c => c.TenantId == tenantId && c.Status == PosConflictStatus.Open)
            .OrderByDescending(c => c.CreatedAtUtc)
            .Take(200)
            .ToListAsync(cancellationToken);

        return conflicts
            .Select(c => new PosConflictResponse(
                c.Id,
                c.WarehouseId,
                c.ProductId,
                c.PosSaleId,
                c.SourceCommandId,
                c.ResultingOnHand,
                c.Message,
                c.Status.ToString(),
                c.CreatedAtUtc))
            .ToList();
    }

    public async Task<AppResult> ResolveAsync(Guid conflictId, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        var conflict = await db.Set<PosInventoryConflict>()
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == conflictId, cancellationToken);
        if (conflict is null)
        {
            return AppResult.Failure(ErrorCodes.InvalidOperation, "POS conflict was not found.");
        }

        if (conflict.Status == PosConflictStatus.Resolved)
        {
            return AppResult.Success();
        }

        conflict.Resolve(DateTimeOffset.UtcNow);
        await db.SaveChangesAsync(cancellationToken);
        return AppResult.Success();
    }
}

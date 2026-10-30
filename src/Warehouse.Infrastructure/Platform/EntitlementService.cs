using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Platform;
using Warehouse.Application.Tenancy;
using Warehouse.Domain.Subscription;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Infrastructure.Platform;

public sealed class EntitlementService(WarehouseDbContext db, ITenantContext tenantContext) : IEntitlementService
{
    public async Task<bool> IsFeatureEnabledAsync(string featureCode, CancellationToken cancellationToken)
    {
        var features = await GetEnabledFeaturesAsync(cancellationToken);
        return features.Contains(featureCode.Trim(), StringComparer.OrdinalIgnoreCase);
    }

    public async Task<IReadOnlyList<string>> GetEnabledFeaturesAsync(CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Array.Empty<string>();
        }

        var tenantId = tenantContext.TenantId.Value;
        var tenant = await db.Tenants.AsNoTracking()
            .SingleOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
        if (tenant is null || !tenant.IsActive || tenant.IsSuspended)
        {
            return Array.Empty<string>();
        }

        var sub = await db.TenantSubscriptions.AsNoTracking()
            .SingleOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);
        if (sub is null || !sub.GrantsEntitlements)
        {
            return Array.Empty<string>();
        }

        var plan = await db.Plans.AsNoTracking().SingleOrDefaultAsync(p => p.Id == sub.PlanId, cancellationToken);
        if (plan is null || !plan.IsActive)
        {
            return Array.Empty<string>();
        }

        return plan.FeatureCodes;
    }
}

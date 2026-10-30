using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Identity;
using Warehouse.Application.Platform;
using Warehouse.Domain.Subscription;
using Warehouse.Infrastructure.Auth;
using Warehouse.Infrastructure.Identity;
using Warehouse.Infrastructure.Platform;
using Warehouse.Infrastructure.Persistence;
using Warehouse.Infrastructure.Tenancy;

namespace Warehouse.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class PlatformSaaSTests
{
    private readonly PostgreSqlCollectionFixture _collection;

    public PlatformSaaSTests(PostgreSqlCollectionFixture collection)
    {
        _collection = collection;
    }

    [SkippableFact]
    public async Task Platform_operator_provisions_tenant_with_trial_and_can_suspend()
    {
        Skip.If(!_collection.IsAvailable, PostgreSqlCollectionFixture.UnavailableReason);

        await using var migrate = _collection.CreateContext(NullTenantContext.Instance);
        await migrate.Database.MigrateAsync();

        var ambient = new AmbientTenantContext();
        await using var db = _collection.CreateContext(ambient);
        await PlatformBootstrap.EnsurePlansAsync(db, CancellationToken.None);

        var hasher = new AspNetPasswordHasher();
        var op = Domain.Platform.PlatformOperator.Create(
            Guid.NewGuid(),
            "ops@platform.test",
            hasher.HashPassword("Platform-Pass-123!"),
            "Ops",
            DateTimeOffset.UtcNow);
        db.PlatformOperators.Add(op);
        await db.SaveChangesAsync();

        ambient.SetPlatformAuthenticated(op.Id, op.Email);
        var provisioning = new TenantProvisioningService(db, hasher, ambient);
        var audit = new AuditWriter(db, ambient, ambient);
        var platform = new PlatformTenantService(db, provisioning, ambient, ambient, audit);

        var provision = await platform.ProvisionAsync(
            new ProvisionTenantPlatformCommand(
                "PLATCO",
                "Platform Co",
                "LYD",
                "Africa/Tripoli",
                "owner@platco.test",
                "Owner-Pass-123!",
                "Owner",
                SystemPlans.TrialCode),
            CancellationToken.None);

        Assert.True(provision.IsSuccess, provision.ErrorMessage);
        Assert.False(provision.Value!.IsSuspended);
        Assert.Equal("LYD", provision.Value.CurrencyCode);

        ambient.SetTenantScope(provision.Value.Id, isPlatformOperator: true);
        var sub = await db.TenantSubscriptions.SingleAsync(s => s.TenantId == provision.Value.Id);
        Assert.Equal(SubscriptionStatus.Trial, sub.Status);
        Assert.Equal(SystemPlans.TrialPlanId, sub.PlanId);

        var entitlements = new EntitlementService(db, ambient);
        Assert.True(await entitlements.IsFeatureEnabledAsync(SubscriptionFeatures.Catalog, CancellationToken.None));

        var suspended = await platform.SuspendAsync(provision.Value.Id, CancellationToken.None);
        Assert.True(suspended.IsSuccess, suspended.ErrorMessage);
        Assert.True(suspended.Value!.IsSuspended);

        Assert.False(await entitlements.IsFeatureEnabledAsync(SubscriptionFeatures.Catalog, CancellationToken.None));

        var usage = await platform.GetUsageAsync(provision.Value.Id, CancellationToken.None);
        Assert.True(usage.IsSuccess, usage.ErrorMessage);
        Assert.Equal(1, usage.Value!.UserCount);

        var readiness = await platform.GetReadinessAsync(provision.Value.Id, CancellationToken.None);
        Assert.True(readiness.IsSuccess, readiness.ErrorMessage);
        Assert.True(readiness.Value!.HasOwnerUser);
        Assert.Contains("unsuspend", readiness.Value.MissingSteps);
        Assert.Contains("warehouse", readiness.Value.MissingSteps);
    }
}

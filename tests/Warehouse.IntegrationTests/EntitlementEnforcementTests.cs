using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Identity;
using Warehouse.Application.Platform;
using Warehouse.Domain.Subscription;
using Warehouse.Infrastructure.Auth;
using Warehouse.Infrastructure.Identity;
using Warehouse.Infrastructure.Persistence;
using Warehouse.Infrastructure.Platform;
using Warehouse.Infrastructure.Tenancy;

namespace Warehouse.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class EntitlementEnforcementTests
{
    private readonly PostgreSqlCollectionFixture _collection;

    public EntitlementEnforcementTests(PostgreSqlCollectionFixture collection)
    {
        _collection = collection;
    }

    [SkippableFact]
    public async Task Cancelled_subscription_and_plan_without_sales_deny_sales_feature()
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
            "entops@platform.test",
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
                "ENTCO",
                "Entitlement Co",
                "USD",
                "UTC",
                "owner@entco.test",
                "Owner-Pass-123!",
                "Owner",
                SystemPlans.TrialCode),
            CancellationToken.None);
        Assert.True(provision.IsSuccess, provision.ErrorMessage);

        ambient.SetTenantScope(provision.Value!.Id, isPlatformOperator: true);
        var entitlements = new EntitlementService(db, ambient);
        Assert.True(await entitlements.IsFeatureEnabledAsync(SubscriptionFeatures.Sales, CancellationToken.None));

        var limitedPlan = Plan.Create(
            Guid.NewGuid(),
            "NOSALES",
            "No Sales",
            [
                SubscriptionFeatures.Catalog,
                SubscriptionFeatures.Inventory,
                SubscriptionFeatures.Purchasing,
                SubscriptionFeatures.Transfers,
                SubscriptionFeatures.Sync,
                SubscriptionFeatures.Reports,
            ],
            maxUsers: 10,
            maxWarehouses: 5,
            maxDevices: 5,
            maxProducts: 500,
            DateTimeOffset.UtcNow);
        db.Plans.Add(limitedPlan);

        var sub = await db.TenantSubscriptions.SingleAsync(s => s.TenantId == provision.Value.Id);
        sub.AssignPlan(limitedPlan.Id, DateTimeOffset.UtcNow);
        await db.SaveChangesAsync();

        Assert.False(await entitlements.IsFeatureEnabledAsync(SubscriptionFeatures.Sales, CancellationToken.None));
        Assert.True(await entitlements.IsFeatureEnabledAsync(SubscriptionFeatures.Inventory, CancellationToken.None));

        sub.SetStatus(SubscriptionStatus.Cancelled, DateTimeOffset.UtcNow);
        await db.SaveChangesAsync();

        Assert.False(await entitlements.IsFeatureEnabledAsync(SubscriptionFeatures.Inventory, CancellationToken.None));
        Assert.False(await entitlements.IsFeatureEnabledAsync(SubscriptionFeatures.Sales, CancellationToken.None));
    }
}

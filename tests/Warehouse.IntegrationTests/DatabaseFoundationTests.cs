using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Warehouse.Domain.Audit;
using Warehouse.Domain.Devices;
using Warehouse.Domain.Identity;
using Warehouse.Domain.Sync;
using Warehouse.Domain.Tenancy;
using Warehouse.Domain.Warehousing;
using Warehouse.Infrastructure.Persistence;
using Warehouse.Infrastructure.Tenancy;
using WarehouseEntity = Warehouse.Domain.Warehousing.Warehouse;

namespace Warehouse.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public class DatabaseFoundationTests
{
    private readonly PostgreSqlCollectionFixture _collection;

    public DatabaseFoundationTests(PostgreSqlCollectionFixture collection)
    {
        _collection = collection;
    }

    [SkippableFact]
    public async Task Migration_applies_and_can_be_rolled_back()
    {
        Skip.If(!_collection.IsAvailable, PostgreSqlCollectionFixture.UnavailableReason);

        await using var context = _collection.CreateContext(NullTenantContext.Instance);
        await context.Database.MigrateAsync();
        Assert.True(await context.Database.CanConnectAsync());

        var migrator = context.Database.GetService<IMigrator>();
        await migrator.MigrateAsync("0");
        await context.Database.MigrateAsync();
    }

    [SkippableFact]
    public async Task Tenant_isolation_hides_other_tenant_rows()
    {
        Skip.If(!_collection.IsAvailable, PostgreSqlCollectionFixture.UnavailableReason);

        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        await using (var setup = _collection.CreateContext(NullTenantContext.Instance))
        {
            await setup.Database.MigrateAsync();
            setup.Tenants.Add(Tenant.Create(tenantA, "TENA", "Tenant A", "USD", "UTC", now));
            setup.Tenants.Add(Tenant.Create(tenantB, "TENB", "Tenant B", "EUR", "UTC", now));
            await setup.SaveChangesAsync();
        }

        await using (var writer = _collection.CreateContext(new FixedTenantContext(tenantA)))
        {
            writer.Users.Add(User.Create(Guid.NewGuid(), tenantA, "a@example.com", "hash-a", "User A", now));
            await writer.SaveChangesAsync();
        }

        await using (var writer = _collection.CreateContext(new FixedTenantContext(tenantB)))
        {
            writer.Users.Add(User.Create(Guid.NewGuid(), tenantB, "b@example.com", "hash-b", "User B", now));
            await writer.SaveChangesAsync();
        }

        await using var readerA = _collection.CreateContext(new FixedTenantContext(tenantA));
        var users = await readerA.Users.ToListAsync();
        Assert.Single(users);
        Assert.All(users, u => Assert.Equal(tenantA, u.TenantId));
    }

    [SkippableFact]
    public async Task Duplicate_sync_operation_id_is_rejected()
    {
        Skip.If(!_collection.IsAvailable, PostgreSqlCollectionFixture.UnavailableReason);

        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        await using var context = _collection.CreateContext(new FixedTenantContext(tenantId));
        await context.Database.MigrateAsync();

        context.Tenants.Add(Tenant.Create(tenantId, "SYNC", "Sync Tenant", "USD", "UTC", now));
        context.Users.Add(User.Create(userId, tenantId, "sync@example.com", "hash", "Sync User", now));
        context.Devices.Add(Device.Register(deviceId, tenantId, "device-key-1", "Scanner", userId, "1.0.0", now));
        await context.SaveChangesAsync();

        context.ProcessedSyncOperations.Add(ProcessedSyncOperation.CreateSucceeded(
            operationId,
            tenantId,
            deviceId,
            userId,
            "Sale",
            Guid.NewGuid(),
            "PostSale",
            """{"ok":true}""",
            now));
        await context.SaveChangesAsync();

        context.ProcessedSyncOperations.Add(ProcessedSyncOperation.CreateSucceeded(
            operationId,
            tenantId,
            deviceId,
            userId,
            "Sale",
            Guid.NewGuid(),
            "PostSale",
            """{"ok":true}""",
            now));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [SkippableFact]
    public async Task Warehouse_can_have_general_and_in_transit_locations()
    {
        Skip.If(!_collection.IsAvailable, PostgreSqlCollectionFixture.UnavailableReason);

        var tenantId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        await using var context = _collection.CreateContext(new FixedTenantContext(tenantId));
        await context.Database.MigrateAsync();

        context.Tenants.Add(Tenant.Create(tenantId, "WH1", "Warehouse Tenant", "LYD", "Africa/Tripoli", now));
        context.Warehouses.Add(WarehouseEntity.Create(warehouseId, tenantId, "MAIN", "Main Warehouse", now));
        var (general, inTransit) = WarehouseLocationFactory.CreateRequiredLocations(tenantId, warehouseId, now);
        context.WarehouseLocations.AddRange(general, inTransit);
        await context.SaveChangesAsync();

        var locations = await context.WarehouseLocations.Where(l => l.WarehouseId == warehouseId).ToListAsync();
        Assert.Contains(locations, l => l.Code == WarehouseLocationCodes.General && l.IsDefault && l.Kind == WarehouseLocationKind.System);
        Assert.Contains(locations, l => l.Code == WarehouseLocationCodes.InTransit && l.Kind == WarehouseLocationKind.InTransit);
        Assert.All(locations, l => Assert.NotEqual(Guid.Empty, l.Id));
        Assert.False(inTransit.IsAvailableForSaleOrPick);
    }

    [SkippableFact]
    public async Task Device_belongs_to_tenant_and_audit_log_persists_metadata()
    {
        Skip.If(!_collection.IsAvailable, PostgreSqlCollectionFixture.UnavailableReason);

        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        await using var context = _collection.CreateContext(new FixedTenantContext(tenantId));
        await context.Database.MigrateAsync();

        context.Tenants.Add(Tenant.Create(tenantId, "DEV", "Device Tenant", "USD", "UTC", now));
        context.Users.Add(User.Create(userId, tenantId, "device@example.com", "hash", "Device User", now));
        context.Devices.Add(Device.Register(deviceId, tenantId, "device-abc", "Phone", userId, "2.0.0", now));
        context.AuditLogs.Add(AuditLog.Create(
            Guid.NewGuid(),
            tenantId,
            userId,
            deviceId,
            "device.registered",
            "Device",
            deviceId,
            now,
            "corr-1",
            "127.0.0.1",
            """{"before":null,"after":{"status":"Active"}}"""));
        await context.SaveChangesAsync();

        var device = await context.Devices.SingleAsync();
        Assert.Equal(tenantId, device.TenantId);
        Assert.Equal(userId, device.UserId);

        var audit = await context.AuditLogs.SingleAsync();
        Assert.Equal("device.registered", audit.Action);
        Assert.Contains("after", audit.MetadataJson, StringComparison.Ordinal);
    }

    [SkippableFact]
    public async Task Tenant_scoped_email_uniqueness_is_enforced()
    {
        Skip.If(!_collection.IsAvailable, PostgreSqlCollectionFixture.UnavailableReason);

        var tenantId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        await using var context = _collection.CreateContext(new FixedTenantContext(tenantId));
        await context.Database.MigrateAsync();
        context.Tenants.Add(Tenant.Create(tenantId, "UNQ", "Unique Tenant", "USD", "UTC", now));
        context.Users.Add(User.Create(Guid.NewGuid(), tenantId, "same@example.com", "hash1", "One", now));
        await context.SaveChangesAsync();

        context.Users.Add(User.Create(Guid.NewGuid(), tenantId, "same@example.com", "hash2", "Two", now));
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [SkippableFact]
    public async Task Timestamps_are_stored_as_utc_offsets()
    {
        Skip.If(!_collection.IsAvailable, PostgreSqlCollectionFixture.UnavailableReason);

        var tenantId = Guid.NewGuid();
        var now = DateTimeOffset.Parse("2026-09-26T12:00:00Z");

        await using var context = _collection.CreateContext(new FixedTenantContext(tenantId));
        await context.Database.MigrateAsync();
        context.Tenants.Add(Tenant.Create(tenantId, "UTC", "UTC Tenant", "USD", "UTC", now));
        await context.SaveChangesAsync();

        var tenant = await context.Tenants.SingleAsync();
        Assert.Equal(TimeSpan.Zero, tenant.CreatedAtUtc.Offset);
        Assert.Equal(now, tenant.CreatedAtUtc);
    }

    [SkippableFact]
    public async Task System_permissions_are_seeded()
    {
        Skip.If(!_collection.IsAvailable, PostgreSqlCollectionFixture.UnavailableReason);

        await using var context = _collection.CreateContext(NullTenantContext.Instance);
        await context.Database.MigrateAsync();
        var permissions = await context.Permissions.ToListAsync();
        Assert.Equal(SystemPermissions.Catalog.Count, permissions.Count);
        Assert.Contains(permissions, p => p.Code == "products.read");
    }
}

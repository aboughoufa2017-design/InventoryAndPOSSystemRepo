using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Catalog;
using Warehouse.Application.Identity;
using Warehouse.Application.Sync;
using Warehouse.Domain.Sync;
using Warehouse.Infrastructure.Auth;
using Warehouse.Infrastructure.Catalog;
using Warehouse.Infrastructure.Identity;
using Warehouse.Infrastructure.Persistence;
using Warehouse.Infrastructure.Sync;
using Warehouse.Infrastructure.Sync.Handlers;
using Warehouse.Infrastructure.Tenancy;

namespace Warehouse.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class SyncFlowTests
{
    private readonly PostgreSqlCollectionFixture _collection;

    public SyncFlowTests(PostgreSqlCollectionFixture collection)
    {
        _collection = collection;
    }

    [SkippableFact]
    public async Task Push_product_is_idempotent_and_pull_returns_change_feed()
    {
        Skip.If(!_collection.IsAvailable, PostgreSqlCollectionFixture.UnavailableReason);

        await using var migrate = _collection.CreateContext(NullTenantContext.Instance);
        await migrate.Database.MigrateAsync();

        var ambient = new AmbientTenantContext();
        await using var db = _collection.CreateContext(ambient);
        var provisioning = new TenantProvisioningService(db, new AspNetPasswordHasher(), ambient);
        var tenantId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        Assert.True((await provisioning.ProvisionAsync(
            new ProvisionTenantCommand(
                tenantId, "SYNCO", "Sync Co", "USD", "UTC", ownerId,
                "owner@synco.test", "Owner-Pass-123!", "Owner"),
            CancellationToken.None)).IsSuccess);

        var deviceId = Guid.NewGuid();
        db.Devices.Add(Domain.Devices.Device.Register(
            deviceId, tenantId, "device-key-1", "Phone", ownerId, "1.0", DateTimeOffset.UtcNow));
        await db.SaveChangesAsync(CancellationToken.None);

        ambient.SetAuthenticated(
            tenantId, ownerId, "owner@synco.test", deviceId, false,
            ["inventory.read", "products.create", "products.update", "products.read"]);

        var catalog = new CatalogService(db, ambient);
        var unit = (await catalog.ListUnitsAsync(CancellationToken.None)).First(u => u.Code == "PIECE");

        var changeLog = new ChangeLogWriter(db, ambient, ambient);
        ISyncOperationHandler[] handlers =
        [
            new ProductUpsertSyncHandler(db, ambient, ambient),
            new ProductDeactivateSyncHandler(db, ambient),
        ];
        var sync = new SyncService(db, ambient, ambient, changeLog, handlers);

        var productId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        var payload = JsonSerializer.Serialize(new
        {
            sku = "SKU-SYNC",
            name = "Synced Product",
            unitOfMeasureId = unit.Id,
            purchasePrice = 1m,
            salePrice = 2m,
            isTaxable = false,
            trackInventory = true,
        }, SyncJson.Options);

        var push = await sync.PushAsync(
            new SyncPushRequest(
            [
                new SyncPushOperationRequest(
                    operationId,
                    SyncEntityTypes.Product,
                    productId,
                    SyncOperationTypes.Upsert,
                    payload,
                    DateTimeOffset.UtcNow,
                    null),
            ]),
            CancellationToken.None);
        Assert.True(push.IsSuccess, push.ErrorMessage);
        Assert.Equal(SyncPushOutcomes.Applied, push.Value!.Results[0].Outcome);
        Assert.True(push.Value.Results[0].ChangeVersion is > 0);

        var replay = await sync.PushAsync(
            new SyncPushRequest(
            [
                new SyncPushOperationRequest(
                    operationId,
                    SyncEntityTypes.Product,
                    productId,
                    SyncOperationTypes.Upsert,
                    payload,
                    DateTimeOffset.UtcNow,
                    null),
            ]),
            CancellationToken.None);
        Assert.True(replay.IsSuccess);
        Assert.Equal(SyncPushOutcomes.AlreadyProcessed, replay.Value!.Results[0].Outcome);

        var pull = await sync.PullAsync(new SyncPullRequest(0, 50), CancellationToken.None);
        Assert.True(pull.IsSuccess, pull.ErrorMessage);
        Assert.Contains(pull.Value!.Changes, c => c.EntityId == productId && c.Operation == "Upsert");

        var deactivateOp = Guid.NewGuid();
        var tombstone = await sync.PushAsync(
            new SyncPushRequest(
            [
                new SyncPushOperationRequest(
                    deactivateOp,
                    SyncEntityTypes.Product,
                    productId,
                    SyncOperationTypes.Deactivate,
                    "{}",
                    DateTimeOffset.UtcNow,
                    null),
            ]),
            CancellationToken.None);
        Assert.True(tombstone.IsSuccess, tombstone.ErrorMessage);
        Assert.Equal(SyncPushOutcomes.Applied, tombstone.Value!.Results[0].Outcome);

        var pull2 = await sync.PullAsync(new SyncPullRequest(push.Value.Results[0].ChangeVersion!.Value, 50), CancellationToken.None);
        Assert.Contains(pull2.Value!.Changes, c => c.EntityId == productId && c.Operation == "Deactivate");
    }
}

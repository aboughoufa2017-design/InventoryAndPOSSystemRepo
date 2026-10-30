using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Catalog;
using Warehouse.Application.Identity;
using Warehouse.Application.Inventory;
using Warehouse.Infrastructure.Auth;
using Warehouse.Infrastructure.Catalog;
using Warehouse.Infrastructure.Identity;
using Warehouse.Infrastructure.Inventory;
using Warehouse.Infrastructure.Persistence;
using Warehouse.Infrastructure.Tenancy;

namespace Warehouse.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class TransferFlowTests
{
    private readonly PostgreSqlCollectionFixture _collection;

    public TransferFlowTests(PostgreSqlCollectionFixture collection)
    {
        _collection = collection;
    }

    [SkippableFact]
    public async Task Dispatch_receive_partial_and_idempotent_dispatch()
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
                tenantId, "TRFCO", "Transfer Co", "USD", "UTC", ownerId,
                "owner@trfco.test", "Owner-Pass-123!", "Owner"),
            CancellationToken.None)).IsSuccess);

        ambient.SetAuthenticated(
            tenantId, ownerId, "owner@trfco.test", null, false,
            ["inventory.read", "inventory.transfer", "inventory.adjust", "products.create", "products.read"]);

        var warehouses = new WarehouseService(db, ambient);
        var source = await warehouses.CreateAsync(new CreateWarehouseCommand("SRC", "Source"), CancellationToken.None);
        var dest = await warehouses.CreateAsync(new CreateWarehouseCommand("DST", "Dest"), CancellationToken.None);
        Assert.True(source.IsSuccess && dest.IsSuccess);

        var catalog = new CatalogService(db, ambient);
        var unit = (await catalog.ListUnitsAsync(CancellationToken.None)).First(u => u.Code == "PIECE");
        var product = await catalog.CreateProductAsync(
            new CreateProductCommand("SKU-T", "Transfer Item", unit.Id, 1m, 2m, false, true, null, null, null, null, null, null, null),
            CancellationToken.None);
        Assert.True(product.IsSuccess);

        var inventory = new InventoryService(db, ambient, ambient);
        var opening = await inventory.CreateAdjustmentAsync(
            new CreateStockAdjustmentCommand(
                source.Value!.Id,
                "OpeningBalance",
                null,
                [new StockAdjustmentLineRequest(product.Value!.Id, source.Value.GeneralLocationId, 20m, null)]),
            CancellationToken.None);
        Assert.True((await inventory.PostAdjustmentAsync(opening.Value!.Id, CancellationToken.None)).IsSuccess);

        var transfers = new TransferService(db, ambient, ambient);
        var created = await transfers.CreateAsync(
            new CreateTransferCommand(
                source.Value.Id,
                dest.Value!.Id,
                source.Value.GeneralLocationId,
                dest.Value.GeneralLocationId,
                "Move stock",
                [new TransferLineRequest(product.Value.Id, 10m, null)]),
            CancellationToken.None);
        Assert.True(created.IsSuccess, created.ErrorMessage);

        var operationId = Guid.NewGuid();
        var dispatched = await transfers.DispatchAsync(
            new DispatchTransferCommand(created.Value!.Id, created.Value.RowVersion, operationId),
            CancellationToken.None);
        Assert.True(dispatched.IsSuccess, dispatched.ErrorMessage);
        Assert.Equal("Dispatched", dispatched.Value!.Status);

        var replay = await transfers.DispatchAsync(
            new DispatchTransferCommand(created.Value.Id, created.Value.RowVersion, operationId),
            CancellationToken.None);
        Assert.True(replay.IsSuccess);
        Assert.True(replay.Value!.WasIdempotentReplay);

        var lineId = dispatched.Value.Lines[0].Id;
        var partial = await transfers.ReceiveAsync(
            new ReceiveTransferCommand(
                dispatched.Value.Id,
                dispatched.Value.RowVersion,
                Guid.NewGuid(),
                [new ReceiveTransferLineRequest(lineId, 4m)]),
            CancellationToken.None);
        Assert.True(partial.IsSuccess, partial.ErrorMessage);
        Assert.Equal("PartiallyReceived", partial.Value!.Status);
        Assert.Equal(6m, partial.Value.Lines[0].QuantityInTransit);

        var complete = await transfers.ReceiveAsync(
            new ReceiveTransferCommand(
                partial.Value.Id,
                partial.Value.RowVersion,
                Guid.NewGuid(),
                [new ReceiveTransferLineRequest(lineId, 6m)]),
            CancellationToken.None);
        Assert.True(complete.IsSuccess, complete.ErrorMessage);
        Assert.Equal("Completed", complete.Value!.Status);

        var destBalance = await inventory.GetBalanceAsync(
            product.Value.Id, dest.Value.Id, dest.Value.GeneralLocationId, CancellationToken.None);
        Assert.Equal(10m, destBalance.Value!.OnHand);
    }
}

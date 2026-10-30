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
public sealed class StockCountFlowTests
{
    private readonly PostgreSqlCollectionFixture _collection;

    public StockCountFlowTests(PostgreSqlCollectionFixture collection)
    {
        _collection = collection;
    }

    [SkippableFact]
    public async Task Count_post_updates_on_hand_via_correction_adjustment()
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
                tenantId, "CNTCO", "Count Co", "USD", "UTC", ownerId,
                "owner@cntco.test", "Owner-Pass-123!", "Owner"),
            CancellationToken.None)).IsSuccess);

        ambient.SetAuthenticated(
            tenantId, ownerId, "owner@cntco.test", null, false,
            ["inventory.adjust", "inventory.read", "products.create", "products.read"]);

        var warehouses = new WarehouseService(db, ambient);
        var warehouse = await warehouses.CreateAsync(new CreateWarehouseCommand("WH1", "Main"), CancellationToken.None);
        Assert.True(warehouse.IsSuccess);

        var catalog = new CatalogService(db, ambient);
        var unit = (await catalog.ListUnitsAsync(CancellationToken.None)).First(u => u.Code == "PIECE");
        var product = await catalog.CreateProductAsync(
            new CreateProductCommand("SKU-C", "Count Item", unit.Id, 1m, 2m, false, true, null, null, null, null, null, null, null),
            CancellationToken.None);
        Assert.True(product.IsSuccess);

        var inventory = new InventoryService(db, ambient, ambient);
        var opening = await inventory.CreateAdjustmentAsync(
            new CreateStockAdjustmentCommand(
                warehouse.Value!.Id,
                "OpeningBalance",
                null,
                [new StockAdjustmentLineRequest(product.Value!.Id, warehouse.Value.GeneralLocationId, 10m, null)]),
            CancellationToken.None);
        Assert.True((await inventory.PostAdjustmentAsync(opening.Value!.Id, CancellationToken.None)).IsSuccess);

        var counts = new StockCountService(db, ambient, inventory);
        var created = await counts.CreateAsync(
            new CreateStockCountCommand(warehouse.Value.Id, null, null),
            CancellationToken.None);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        Assert.Contains(created.Value!.Lines, l => l.ProductId == product.Value!.Id && l.SystemQuantity == 10m);

        var setLines = await counts.SetLinesAsync(
            new SetStockCountLinesCommand(
                created.Value.Id,
                created.Value.RowVersion,
                [new StockCountLineInput(product.Value!.Id, 8m, "short")]),
            CancellationToken.None);
        Assert.True(setLines.IsSuccess, setLines.ErrorMessage);

        var posted = await counts.PostAsync(
            new PostStockCountCommand(setLines.Value!.Id, setLines.Value.RowVersion),
            CancellationToken.None);
        Assert.True(posted.IsSuccess, posted.ErrorMessage);
        Assert.Equal("Posted", posted.Value!.Status);
        Assert.NotNull(posted.Value.PostedAdjustmentId);

        var balance = await db.StockBalances.SingleAsync(b =>
            b.ProductId == product.Value!.Id && b.WarehouseId == warehouse.Value.Id);
        Assert.Equal(8m, balance.OnHand);
    }
}

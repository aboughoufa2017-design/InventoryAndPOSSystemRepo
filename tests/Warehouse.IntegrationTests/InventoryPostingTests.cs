using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Catalog;
using Warehouse.Application.Identity;
using Warehouse.Application.Inventory;
using Warehouse.Domain.Tenancy;
using Warehouse.Infrastructure.Auth;
using Warehouse.Infrastructure.Catalog;
using Warehouse.Infrastructure.Identity;
using Warehouse.Infrastructure.Inventory;
using Warehouse.Infrastructure.Persistence;
using Warehouse.Infrastructure.Tenancy;

namespace Warehouse.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class InventoryPostingTests
{
    private readonly PostgreSqlCollectionFixture _collection;

    public InventoryPostingTests(PostgreSqlCollectionFixture collection)
    {
        _collection = collection;
    }

    [SkippableFact]
    public async Task Opening_balance_posts_movements_and_prevent_blocks_negative()
    {
        Skip.If(!_collection.IsAvailable, PostgreSqlCollectionFixture.UnavailableReason);

        await using var migrate = _collection.CreateContext(NullTenantContext.Instance);
        await migrate.Database.MigrateAsync();

        var ambient = new AmbientTenantContext();
        await using var db = _collection.CreateContext(ambient);
        var provisioning = new TenantProvisioningService(db, new AspNetPasswordHasher(), ambient);
        var tenantId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var provision = await provisioning.ProvisionAsync(
            new ProvisionTenantCommand(
                tenantId, "INVCO", "Inv Co", "USD", "UTC", ownerId,
                "owner@invco.test", "Owner-Pass-123!", "Owner"),
            CancellationToken.None);
        Assert.True(provision.IsSuccess, provision.ErrorMessage);

        ambient.SetAuthenticated(tenantId, ownerId, "owner@invco.test", null, false, ["inventory.read", "inventory.adjust", "products.create", "products.read"]);

        var warehouses = new WarehouseService(db, ambient);
        var warehouse = await warehouses.CreateAsync(new CreateWarehouseCommand("MAIN", "Main"), CancellationToken.None);
        Assert.True(warehouse.IsSuccess, warehouse.ErrorMessage);

        var catalog = new CatalogService(db, ambient);
        var unit = (await catalog.ListUnitsAsync(CancellationToken.None)).First(u => u.Code == "PIECE");
        var product = await catalog.CreateProductAsync(
            new CreateProductCommand(
                "ITEM-1", "Item", unit.Id, 1m, 2m, false, true, null, null, null, null, null, null, null),
            CancellationToken.None);
        Assert.True(product.IsSuccess, product.ErrorMessage);

        var inventory = new InventoryService(db, ambient, ambient);
        var opening = await inventory.CreateAdjustmentAsync(
            new CreateStockAdjustmentCommand(
                warehouse.Value!.Id,
                "OpeningBalance",
                "Initial",
                [new StockAdjustmentLineRequest(product.Value!.Id, warehouse.Value.GeneralLocationId, 10m, null)]),
            CancellationToken.None);
        Assert.True(opening.IsSuccess, opening.ErrorMessage);

        var posted = await inventory.PostAdjustmentAsync(opening.Value!.Id, CancellationToken.None);
        Assert.True(posted.IsSuccess, posted.ErrorMessage);
        Assert.Equal("Posted", posted.Value!.Status);

        var balance = await inventory.GetBalanceAsync(
            product.Value.Id, warehouse.Value.Id, warehouse.Value.GeneralLocationId, CancellationToken.None);
        Assert.Equal(10m, balance.Value!.OnHand);
        Assert.Equal(10m, balance.Value.Available);

        var movements = await inventory.ListMovementsAsync(warehouse.Value.Id, product.Value.Id, 10, CancellationToken.None);
        Assert.Single(movements);
        Assert.Equal(10m, movements[0].QuantityChange);

        var negative = await inventory.CreateAdjustmentAsync(
            new CreateStockAdjustmentCommand(
                warehouse.Value.Id,
                "Correction",
                "Too much",
                [new StockAdjustmentLineRequest(product.Value.Id, warehouse.Value.GeneralLocationId, -20m, null)]),
            CancellationToken.None);
        Assert.True(negative.IsSuccess, negative.ErrorMessage);
        var blocked = await inventory.PostAdjustmentAsync(negative.Value!.Id, CancellationToken.None);
        Assert.False(blocked.IsSuccess);
        Assert.Equal("INSUFFICIENT_STOCK", blocked.ErrorCode);
    }
}

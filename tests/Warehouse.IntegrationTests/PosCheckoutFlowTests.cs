using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Catalog;
using Warehouse.Application.Identity;
using Warehouse.Application.Inventory;
using Warehouse.Application.Pos;
using Warehouse.Infrastructure.Auth;
using Warehouse.Infrastructure.Catalog;
using Warehouse.Infrastructure.Identity;
using Warehouse.Infrastructure.Inventory;
using Warehouse.Infrastructure.Persistence;
using Warehouse.Infrastructure.Pos;
using Warehouse.Infrastructure.Tenancy;

namespace Warehouse.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class PosCheckoutFlowTests
{
    private readonly PostgreSqlCollectionFixture _collection;

    public PosCheckoutFlowTests(PostgreSqlCollectionFixture collection)
    {
        _collection = collection;
    }

    [SkippableFact]
    public async Task Finalize_decreases_on_hand_and_client_command_is_idempotent()
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
                tenantId, "POSCO", "POS Co", "LYD", "Africa/Tripoli", ownerId,
                "owner@posco.test", "Owner-Pass-123!", "Owner"),
            CancellationToken.None)).IsSuccess);

        ambient.SetAuthenticated(
            tenantId, ownerId, "owner@posco.test", null, false,
            ["pos.sale", "inventory.issue", "products.create", "products.read", "inventory.adjust", "inventory.read"]);

        var warehouses = new WarehouseService(db, ambient);
        var warehouse = await warehouses.CreateAsync(new CreateWarehouseCommand("WH1", "Store"), CancellationToken.None);
        Assert.True(warehouse.IsSuccess);

        var catalog = new CatalogService(db, ambient);
        var unit = (await catalog.ListUnitsAsync(CancellationToken.None)).First(u => u.Code == "PIECE");
        var product = await catalog.CreateProductAsync(
            new CreateProductCommand("SKU-P", "POS Item", unit.Id, 1m, 5m, false, true, null, null, null, null, null, null, null),
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

        var pos = new PosCheckoutService(db, ambient, ambient);
        var commandId = Guid.NewGuid();
        var sale = await pos.FinalizeCashSaleAsync(
            new FinalizePosCashSaleCommand(
                commandId,
                warehouse.Value.Id,
                null,
                null,
                null,
                null,
                "LYD",
                null,
                PosPaymentMethodDto.Cash,
                [new PosSaleLineInput(product.Value.Id, 2m, null, 0m)]),
            CancellationToken.None);
        Assert.True(sale.IsSuccess, sale.ErrorMessage);
        Assert.False(sale.Value!.WasIdempotentReplay);
        Assert.Equal(10m, sale.Value.GrandTotal);

        var balance = await db.StockBalances.SingleAsync(b =>
            b.ProductId == product.Value.Id && b.WarehouseId == warehouse.Value.Id);
        Assert.Equal(8m, balance.OnHand);

        var replay = await pos.FinalizeCashSaleAsync(
            new FinalizePosCashSaleCommand(
                commandId,
                warehouse.Value.Id,
                null,
                null,
                null,
                null,
                "LYD",
                null,
                PosPaymentMethodDto.Cash,
                [new PosSaleLineInput(product.Value.Id, 2m, null, 0m)]),
            CancellationToken.None);
        Assert.True(replay.IsSuccess, replay.ErrorMessage);
        Assert.True(replay.Value!.WasIdempotentReplay);

        balance = await db.StockBalances.SingleAsync(b =>
            b.ProductId == product.Value.Id && b.WarehouseId == warehouse.Value.Id);
        Assert.Equal(8m, balance.OnHand);
    }
}

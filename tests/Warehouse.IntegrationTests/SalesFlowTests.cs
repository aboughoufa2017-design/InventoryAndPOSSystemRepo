using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Catalog;
using Warehouse.Application.Identity;
using Warehouse.Application.Inventory;
using Warehouse.Application.Sales;
using Warehouse.Infrastructure.Auth;
using Warehouse.Infrastructure.Catalog;
using Warehouse.Infrastructure.Identity;
using Warehouse.Infrastructure.Inventory;
using Warehouse.Infrastructure.Persistence;
using Warehouse.Infrastructure.Sales;
using Warehouse.Infrastructure.Tenancy;

namespace Warehouse.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class SalesFlowTests
{
    private readonly PostgreSqlCollectionFixture _collection;

    public SalesFlowTests(PostgreSqlCollectionFixture collection)
    {
        _collection = collection;
    }

    [SkippableFact]
    public async Task Confirm_does_not_stock_ship_partial_return_and_payment()
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
                tenantId, "SALCO", "Sales Co", "USD", "UTC", ownerId,
                "owner@salco.test", "Owner-Pass-123!", "Owner"),
            CancellationToken.None)).IsSuccess);

        ambient.SetAuthenticated(
            tenantId, ownerId, "owner@salco.test", null, false,
            [
                "sales.create", "sales.cancel", "inventory.issue", "inventory.read",
                "inventory.adjust", "products.create", "products.read",
            ]);

        var warehouses = new WarehouseService(db, ambient);
        var warehouse = await warehouses.CreateAsync(new CreateWarehouseCommand("WH1", "Main"), CancellationToken.None);
        Assert.True(warehouse.IsSuccess);

        var catalog = new CatalogService(db, ambient);
        var unit = (await catalog.ListUnitsAsync(CancellationToken.None)).First(u => u.Code == "PIECE");
        var product = await catalog.CreateProductAsync(
            new CreateProductCommand("SKU-S", "Sale Item", unit.Id, 8m, 20m, false, true, null, null, null, null, null, null, null),
            CancellationToken.None);
        Assert.True(product.IsSuccess);

        var inventory = new InventoryService(db, ambient, ambient);
        var opening = await inventory.CreateAdjustmentAsync(
            new CreateStockAdjustmentCommand(
                warehouse.Value!.Id,
                "OpeningBalance",
                null,
                [new StockAdjustmentLineRequest(product.Value!.Id, warehouse.Value.GeneralLocationId, 20m, null)]),
            CancellationToken.None);
        Assert.True((await inventory.PostAdjustmentAsync(opening.Value!.Id, CancellationToken.None)).IsSuccess);

        var customers = new CustomerService(db, ambient);
        var customer = await customers.CreateAsync(
            new CreateCustomerCommand("CUST1", "Acme Buyer", null, null, null, null),
            CancellationToken.None);
        Assert.True(customer.IsSuccess, customer.ErrorMessage);

        var sales = new SalesService(db, ambient, ambient);
        var created = await sales.CreateAsync(
            new CreateSalesOrderCommand(
                customer.Value!.Id,
                warehouse.Value.Id,
                warehouse.Value.GeneralLocationId,
                "SO-100",
                null,
                [new SalesOrderLineRequest(product.Value.Id, 10m, 20m, null)]),
            CancellationToken.None);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        Assert.Equal("Draft", created.Value!.Status);
        Assert.Equal(200m, created.Value.GrandTotal);

        var confirmed = await sales.ConfirmAsync(
            new ConfirmSalesOrderCommand(created.Value.Id, created.Value.RowVersion, Guid.NewGuid()),
            CancellationToken.None);
        Assert.True(confirmed.IsSuccess, confirmed.ErrorMessage);
        Assert.Equal("Confirmed", confirmed.Value!.Status);
        Assert.Equal(20m, (await inventory.ListBalancesAsync(warehouse.Value.Id, product.Value.Id, CancellationToken.None)).Sum(b => b.OnHand));

        var lineId = confirmed.Value.Lines[0].Id;
        var shipOp = Guid.NewGuid();
        var partial = await sales.ShipAsync(
            new ShipSalesOrderCommand(
                confirmed.Value.Id,
                confirmed.Value.RowVersion,
                shipOp,
                null,
                null,
                null,
                [new ShipSalesLineRequest(lineId, 4m)]),
            CancellationToken.None);
        Assert.True(partial.IsSuccess, partial.ErrorMessage);
        Assert.Equal("PartiallyFulfilled", partial.Value!.Status);

        var replay = await sales.ShipAsync(
            new ShipSalesOrderCommand(
                confirmed.Value.Id,
                partial.Value.RowVersion,
                shipOp,
                null,
                null,
                null,
                [new ShipSalesLineRequest(lineId, 4m)]),
            CancellationToken.None);
        Assert.True(replay.IsSuccess);
        Assert.True(replay.Value!.WasIdempotentReplay);

        Assert.Equal(16m, (await inventory.ListBalancesAsync(warehouse.Value.Id, product.Value.Id, CancellationToken.None)).Sum(b => b.OnHand));

        var returned = await sales.ReturnAsync(
            new ReturnSalesOrderCommand(
                partial.Value.Id,
                partial.Value.RowVersion,
                Guid.NewGuid(),
                null,
                null,
                null,
                [new ReturnSalesLineRequest(lineId, 1m)]),
            CancellationToken.None);
        Assert.True(returned.IsSuccess, returned.ErrorMessage);
        Assert.Equal(17m, (await inventory.ListBalancesAsync(warehouse.Value.Id, product.Value.Id, CancellationToken.None)).Sum(b => b.OnHand));

        var paid = await sales.UpdatePaymentAsync(
            new UpdateSalesPaymentCommand(returned.Value!.Id, 80m, returned.Value.RowVersion, null),
            CancellationToken.None);
        Assert.True(paid.IsSuccess, paid.ErrorMessage);
        Assert.Equal("PartiallyPaid", paid.Value!.PaymentStatus);
    }
}

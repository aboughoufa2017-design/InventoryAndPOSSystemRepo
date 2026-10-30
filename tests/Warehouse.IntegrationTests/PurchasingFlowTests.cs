using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Catalog;
using Warehouse.Application.Identity;
using Warehouse.Application.Inventory;
using Warehouse.Application.Purchasing;
using Warehouse.Infrastructure.Auth;
using Warehouse.Infrastructure.Catalog;
using Warehouse.Infrastructure.Identity;
using Warehouse.Infrastructure.Inventory;
using Warehouse.Infrastructure.Persistence;
using Warehouse.Infrastructure.Purchasing;
using Warehouse.Infrastructure.Tenancy;

namespace Warehouse.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class PurchasingFlowTests
{
    private readonly PostgreSqlCollectionFixture _collection;

    public PurchasingFlowTests(PostgreSqlCollectionFixture collection)
    {
        _collection = collection;
    }

    [SkippableFact]
    public async Task Confirm_does_not_stock_receive_partial_return_and_payment()
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
                tenantId, "PURCO", "Purchase Co", "USD", "UTC", ownerId,
                "owner@purco.test", "Owner-Pass-123!", "Owner"),
            CancellationToken.None)).IsSuccess);

        ambient.SetAuthenticated(
            tenantId, ownerId, "owner@purco.test", null, false,
            [
                "purchases.create", "purchases.approve", "inventory.receive", "inventory.read",
                "inventory.adjust", "products.create", "products.read",
            ]);

        var warehouses = new WarehouseService(db, ambient);
        var warehouse = await warehouses.CreateAsync(new CreateWarehouseCommand("WH1", "Main"), CancellationToken.None);
        Assert.True(warehouse.IsSuccess);

        var catalog = new CatalogService(db, ambient);
        var unit = (await catalog.ListUnitsAsync(CancellationToken.None)).First(u => u.Code == "PIECE");
        var product = await catalog.CreateProductAsync(
            new CreateProductCommand("SKU-P", "Purchase Item", unit.Id, 10m, 15m, false, true, null, null, null, null, null, null, null),
            CancellationToken.None);
        Assert.True(product.IsSuccess);

        var suppliers = new SupplierService(db, ambient);
        var supplier = await suppliers.CreateAsync(
            new CreateSupplierCommand("SUP1", "Acme Supply", null, null, null, null),
            CancellationToken.None);
        Assert.True(supplier.IsSuccess, supplier.ErrorMessage);

        var purchasing = new PurchasingService(db, ambient, ambient);
        var created = await purchasing.CreateAsync(
            new CreatePurchaseOrderCommand(
                supplier.Value!.Id,
                warehouse.Value!.Id,
                warehouse.Value.GeneralLocationId,
                "PO-100",
                null,
                [new PurchaseOrderLineRequest(product.Value!.Id, 10m, 10m, null)]),
            CancellationToken.None);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        Assert.Equal("Draft", created.Value!.Status);
        Assert.Equal(100m, created.Value.GrandTotal);

        var inventory = new InventoryService(db, ambient, ambient);
        Assert.Empty(await inventory.ListBalancesAsync(warehouse.Value.Id, product.Value.Id, CancellationToken.None));

        var confirmed = await purchasing.ConfirmAsync(
            new ConfirmPurchaseOrderCommand(created.Value.Id, created.Value.RowVersion, Guid.NewGuid()),
            CancellationToken.None);
        Assert.True(confirmed.IsSuccess, confirmed.ErrorMessage);
        Assert.Equal("Confirmed", confirmed.Value!.Status);
        Assert.Empty(await inventory.ListBalancesAsync(warehouse.Value.Id, product.Value.Id, CancellationToken.None));

        var lineId = confirmed.Value.Lines[0].Id;
        var receiveOp = Guid.NewGuid();
        var partial = await purchasing.ReceiveAsync(
            new ReceivePurchaseOrderCommand(
                confirmed.Value.Id,
                confirmed.Value.RowVersion,
                receiveOp,
                null,
                null,
                null,
                [new ReceivePurchaseLineRequest(lineId, 4m)]),
            CancellationToken.None);
        Assert.True(partial.IsSuccess, partial.ErrorMessage);
        Assert.Equal("PartiallyReceived", partial.Value!.Status);

        var replay = await purchasing.ReceiveAsync(
            new ReceivePurchaseOrderCommand(
                confirmed.Value.Id,
                partial.Value.RowVersion,
                receiveOp,
                null,
                null,
                null,
                [new ReceivePurchaseLineRequest(lineId, 4m)]),
            CancellationToken.None);
        Assert.True(replay.IsSuccess);
        Assert.True(replay.Value!.WasIdempotentReplay);

        var balances = await inventory.ListBalancesAsync(warehouse.Value.Id, product.Value.Id, CancellationToken.None);
        Assert.Equal(4m, balances.Sum(b => b.OnHand));

        var returned = await purchasing.ReturnAsync(
            new ReturnPurchaseOrderCommand(
                partial.Value.Id,
                partial.Value.RowVersion,
                Guid.NewGuid(),
                null,
                null,
                null,
                [new ReturnPurchaseLineRequest(lineId, 1m)]),
            CancellationToken.None);
        Assert.True(returned.IsSuccess, returned.ErrorMessage);
        balances = await inventory.ListBalancesAsync(warehouse.Value.Id, product.Value.Id, CancellationToken.None);
        Assert.Equal(3m, balances.Sum(b => b.OnHand));

        var paid = await purchasing.UpdatePaymentAsync(
            new UpdatePurchasePaymentCommand(returned.Value!.Id, 50m, returned.Value.RowVersion, null),
            CancellationToken.None);
        Assert.True(paid.IsSuccess, paid.ErrorMessage);
        Assert.Equal("PartiallyPaid", paid.Value!.PaymentStatus);
    }
}

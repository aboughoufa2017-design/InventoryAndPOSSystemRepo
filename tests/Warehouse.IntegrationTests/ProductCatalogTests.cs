using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Catalog;
using Warehouse.Application.Identity;
using Warehouse.Domain.Catalog;
using Warehouse.Infrastructure.Auth;
using Warehouse.Infrastructure.Catalog;
using Warehouse.Infrastructure.Identity;
using Warehouse.Infrastructure.Persistence;
using Warehouse.Infrastructure.Tenancy;

namespace Warehouse.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class ProductCatalogTests
{
    private readonly PostgreSqlCollectionFixture _collection;

    public ProductCatalogTests(PostgreSqlCollectionFixture collection)
    {
        _collection = collection;
    }

    [SkippableFact]
    public async Task Provisioned_tenant_gets_system_units_and_can_create_product()
    {
        Skip.If(!_collection.IsAvailable, PostgreSqlCollectionFixture.UnavailableReason);

        await using var migrate = _collection.CreateContext(NullTenantContext.Instance);
        await migrate.Database.MigrateAsync();

        var ambient = new AmbientTenantContext();
        await using var db = _collection.CreateContext(ambient);
        var provisioning = new TenantProvisioningService(db, new AspNetPasswordHasher(), ambient);

        var tenantId = Guid.NewGuid();
        var provision = await provisioning.ProvisionAsync(
            new ProvisionTenantCommand(
                tenantId,
                "CATCO",
                "Catalog Co",
                "USD",
                "UTC",
                Guid.NewGuid(),
                "owner@catco.test",
                "Owner-Pass-123!",
                "Owner"),
            CancellationToken.None);
        Assert.True(provision.IsSuccess, provision.ErrorMessage);

        ambient.SetTenantScope(tenantId);
        var units = await db.UnitsOfMeasure.Where(u => u.IsSystem).ToListAsync();
        Assert.Equal(SystemUnits.Catalog.Count, units.Count);

        var catalog = new CatalogService(db, ambient);
        var piece = units.Single(u => u.Code == "PIECE");

        var created = await catalog.CreateProductAsync(
            new CreateProductCommand(
                "WIDGET-1",
                "Widget",
                piece.Id,
                5.5m,
                9.99m,
                IsTaxable: true,
                TrackInventory: true,
                CategoryId: null,
                Description: "A widget",
                Brand: "Acme",
                MinimumStock: 1,
                MaximumStock: 100,
                ReorderPoint: 5,
                Barcodes: ["111", "222"]),
            CancellationToken.None);

        Assert.True(created.IsSuccess, created.ErrorMessage);
        Assert.Equal("WIDGET-1", created.Value!.Sku);
        Assert.Equal(2, created.Value.Barcodes.Count);
        Assert.Equal(0m, created.Value.ApplicableTaxRate);

        var byBarcode = await catalog.GetProductByBarcodeAsync("111", CancellationToken.None);
        Assert.True(byBarcode.IsSuccess);
        Assert.Equal(created.Value.Id, byBarcode.Value!.Id);
    }
}

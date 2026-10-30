using Warehouse.Domain.Catalog;
using Warehouse.Domain.Tenancy;

namespace Warehouse.UnitTests.Catalog;

public class ProductTests
{
    [Fact]
    public void Create_normalizes_sku_and_rejects_negative_prices()
    {
        var product = Product.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            " sku-1 ",
            "Widget",
            Guid.NewGuid(),
            10.12345m,
            20m,
            isTaxable: true,
            trackInventory: true,
            categoryId: null,
            description: null,
            brand: "Acme",
            minimumStock: 1,
            maximumStock: 10,
            reorderPoint: 2,
            DateTimeOffset.UtcNow);

        Assert.Equal("SKU-1", product.Sku);
        Assert.Equal(10.1235m, product.PurchasePrice);
        Assert.Throws<ArgumentOutOfRangeException>(() => Product.Create(
            Guid.NewGuid(), Guid.NewGuid(), "X", "Y", Guid.NewGuid(),
            -1m, 0m, false, true, null, null, null, null, null, null, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Deactivate_sets_inactive_without_delete()
    {
        var product = Product.Create(
            Guid.NewGuid(), Guid.NewGuid(), "A", "Name", Guid.NewGuid(),
            0m, 0m, false, true, null, null, null, null, null, null, DateTimeOffset.UtcNow);
        product.SetActive(false, DateTimeOffset.UtcNow);
        Assert.False(product.IsActive);
    }
}

public class ProductTaxCalculatorTests
{
    [Fact]
    public void Tax_is_zero_when_tenant_tax_disabled()
    {
        var tenant = Tenant.Create(Guid.NewGuid(), "T1", "T", "USD", "UTC", DateTimeOffset.UtcNow);
        var product = Product.Create(
            Guid.NewGuid(), tenant.Id, "P1", "P", Guid.NewGuid(),
            100m, 100m, isTaxable: true, true, null, null, null, null, null, null, DateTimeOffset.UtcNow);

        Assert.Equal(0m, ProductTaxCalculator.ResolveApplicableRate(tenant, product));
        Assert.Equal(0m, ProductTaxCalculator.CalculateTaxAmount(tenant, product, 100m));
    }

    [Fact]
    public void Tax_applies_tenant_rate_when_enabled_and_product_taxable()
    {
        var tenant = Tenant.Create(Guid.NewGuid(), "T2", "T", "USD", "UTC", DateTimeOffset.UtcNow);
        tenant.ConfigureTax(true, "VAT", 0.15m, DateTimeOffset.UtcNow);
        var product = Product.Create(
            Guid.NewGuid(), tenant.Id, "P2", "P", Guid.NewGuid(),
            100m, 100m, isTaxable: true, true, null, null, null, null, null, null, DateTimeOffset.UtcNow);

        Assert.Equal(0.15m, ProductTaxCalculator.ResolveApplicableRate(tenant, product));
        Assert.Equal(15.0000m, ProductTaxCalculator.CalculateTaxAmount(tenant, product, 100m));
    }

    [Fact]
    public void Non_taxable_product_ignores_enabled_tenant_tax()
    {
        var tenant = Tenant.Create(Guid.NewGuid(), "T3", "T", "USD", "UTC", DateTimeOffset.UtcNow);
        tenant.ConfigureTax(true, "VAT", 0.15m, DateTimeOffset.UtcNow);
        var product = Product.Create(
            Guid.NewGuid(), tenant.Id, "P3", "P", Guid.NewGuid(),
            100m, 100m, isTaxable: false, true, null, null, null, null, null, null, DateTimeOffset.UtcNow);

        Assert.Equal(0m, ProductTaxCalculator.ResolveApplicableRate(tenant, product));
    }
}

public class ProductBarcodeTests
{
    [Fact]
    public void Create_trims_barcode()
    {
        var barcode = ProductBarcode.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), " 123 ", true, DateTimeOffset.UtcNow);
        Assert.Equal("123", barcode.Barcode);
        Assert.True(barcode.IsPrimary);
    }
}

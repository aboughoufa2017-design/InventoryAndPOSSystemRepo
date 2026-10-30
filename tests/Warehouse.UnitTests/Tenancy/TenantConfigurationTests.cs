using Warehouse.Domain.Tenancy;

namespace Warehouse.UnitTests.Tenancy;

public class TenantConfigurationTests
{
    [Fact]
    public void Requires_uuid_identifier()
    {
        Assert.Throws<ArgumentException>(() =>
            Tenant.Create(Guid.Empty, "ACME", "Acme", "USD", "UTC", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Currency_is_tenant_configurable_and_not_hard_coded_in_factory_beyond_parameter()
    {
        var lyd = Tenant.Create(Guid.NewGuid(), "ALM", "Almadar", "LYD", "Africa/Tripoli", DateTimeOffset.UtcNow);
        var eur = Tenant.Create(Guid.NewGuid(), "EU1", "Europe Co", "EUR", "Europe/Berlin", DateTimeOffset.UtcNow);

        Assert.Equal("LYD", lyd.CurrencyCode);
        Assert.Equal("EUR", eur.CurrencyCode);
    }

    [Fact]
    public void Tax_defaults_to_disabled()
    {
        var tenant = Tenant.Create(Guid.NewGuid(), "TAX", "Tax Tenant", "USD", "UTC", DateTimeOffset.UtcNow);
        Assert.False(tenant.TaxEnabled);
    }
}

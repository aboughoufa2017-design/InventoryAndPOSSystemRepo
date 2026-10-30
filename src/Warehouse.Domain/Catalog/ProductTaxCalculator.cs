using Warehouse.Domain.Tenancy;

namespace Warehouse.Domain.Catalog;

/// <summary>
/// Resolves effective tax for a product line using tenant configuration.
/// Inventory posting never applies tax; commercial documents use this helper.
/// </summary>
public static class ProductTaxCalculator
{
    public static decimal ResolveApplicableRate(Tenant tenant, Product product)
    {
        ArgumentNullException.ThrowIfNull(tenant);
        ArgumentNullException.ThrowIfNull(product);

        if (!tenant.TaxEnabled || !product.IsTaxable)
        {
            return 0m;
        }

        return tenant.TaxRate ?? 0m;
    }

    public static decimal CalculateTaxAmount(Tenant tenant, Product product, decimal netAmount)
    {
        if (netAmount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(netAmount), "Net amount cannot be negative.");
        }

        var rate = ResolveApplicableRate(tenant, product);
        if (rate == 0m)
        {
            return 0m;
        }

        // rate is a fraction (e.g. 0.15 = 15%) or percent stored as decimal — Tenant.TaxRate uses numeric(9,6).
        // Treat as fraction of 1 (0.15 = 15%).
        var tax = netAmount * rate;
        return Math.Round(tax, Precision.MoneyPrecision.Scale, MidpointRounding.AwayFromZero);
    }
}

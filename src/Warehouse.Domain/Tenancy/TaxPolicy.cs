namespace Warehouse.Domain.Tenancy;

/// <summary>
/// Tax is disabled by default and is not part of inventory posting.
/// </summary>
public sealed class TaxPolicy
{
    public static TaxPolicy Disabled { get; } = new(enabled: false);

    public TaxPolicy(bool enabled)
    {
        Enabled = enabled;
    }

    public bool Enabled { get; }

    public static TaxPolicy CreateDefault() => Disabled;
}

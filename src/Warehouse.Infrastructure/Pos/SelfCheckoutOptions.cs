namespace Warehouse.Infrastructure.Pos;

public sealed class SelfCheckoutOptions
{
    public const string SectionName = "SelfCheckout";

    /// <summary>
    /// Guest / Scan &amp; Go cart lifetime in hours. Override with SelfCheckout__SessionHours or SELF_CHECKOUT_SESSION_HOURS.
    /// </summary>
    public int SessionHours { get; set; } = 3;
}

namespace Warehouse.Domain.Subscription;

/// <summary>
/// Subscription lifecycle. Billing provider status maps into these values; no provider is integrated in v1.
/// </summary>
public enum SubscriptionStatus
{
    Trial = 0,
    Active = 1,
    PastDue = 2,
    Cancelled = 3,
}

/// <summary>
/// Well-known feature codes checked via <c>IEntitlementService</c>.
/// </summary>
public static class SubscriptionFeatures
{
    public const string Catalog = "catalog";
    public const string Inventory = "inventory";
    public const string Purchasing = "purchasing";
    public const string Sales = "sales";
    public const string Transfers = "transfers";
    public const string Sync = "sync";
    public const string Reports = "reports";
    public const string Offline = "offline";

    public static readonly IReadOnlyList<string> All =
    [
        Catalog, Inventory, Purchasing, Sales, Transfers, Sync, Reports, Offline,
    ];
}

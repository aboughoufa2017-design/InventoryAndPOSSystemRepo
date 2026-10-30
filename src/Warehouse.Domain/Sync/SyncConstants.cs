namespace Warehouse.Domain.Sync;

/// <summary>
/// Well-known sync entity and operation type strings for push routing.
/// </summary>
public static class SyncEntityTypes
{
    public const string Product = "Product";
    public const string StockAdjustment = "StockAdjustment";
    public const string Transfer = "Transfer";
    public const string PosSale = "PosSale";
}

public static class SyncOperationTypes
{
    public const string Upsert = "Upsert";
    public const string Deactivate = "Deactivate";
    public const string Post = "Post";
    public const string Dispatch = "Dispatch";
    public const string Receive = "Receive";
    public const string Cancel = "Cancel";
    public const string Finalize = "Finalize";
}

/// <summary>
/// Explicit push outcomes. Never silently overwrite on conflict.
/// </summary>
public static class SyncPushOutcomes
{
    public const string Applied = "Applied";
    public const string AlreadyProcessed = "AlreadyProcessed";
    public const string Conflict = "Conflict";
    public const string Rejected = "Rejected";
    public const string ValidationFailed = "ValidationFailed";
}

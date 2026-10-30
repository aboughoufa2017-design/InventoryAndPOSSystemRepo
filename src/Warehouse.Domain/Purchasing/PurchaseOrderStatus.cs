namespace Warehouse.Domain.Purchasing;

/// <summary>
/// Module-specific purchase order lifecycle. Distinct from generic DocumentLifecycleStatus.
/// </summary>
public enum PurchaseOrderStatus
{
    Draft = 0,
    Confirmed = 1,
    PartiallyReceived = 2,
    FullyReceived = 3,
    Closed = 4,
    Cancelled = 5,
}

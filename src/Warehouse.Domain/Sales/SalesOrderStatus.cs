namespace Warehouse.Domain.Sales;

/// <summary>
/// Module-specific sales order lifecycle. Distinct from generic DocumentLifecycleStatus.
/// </summary>
public enum SalesOrderStatus
{
    Draft = 0,
    Confirmed = 1,
    PartiallyFulfilled = 2,
    Fulfilled = 3,
    Closed = 4,
    Cancelled = 5,
}

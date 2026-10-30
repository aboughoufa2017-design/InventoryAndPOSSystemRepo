namespace Warehouse.Domain.Commercial;

/// <summary>
/// Payment tracking for commercial documents in v1. Not a general ledger.
/// </summary>
public enum PaymentStatus
{
    Unpaid = 0,
    PartiallyPaid = 1,
    Paid = 2,
}

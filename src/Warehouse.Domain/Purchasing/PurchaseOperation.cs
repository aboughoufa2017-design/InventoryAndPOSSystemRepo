using Warehouse.Domain.Common;

namespace Warehouse.Domain.Purchasing;

/// <summary>
/// TENANT-OWNED idempotency record for purchase confirm/receive/return/close/cancel/payment.
/// OperationId is the primary key (client-supplied).
/// </summary>
public sealed class PurchaseOperation : TenantOwnedEntity
{
    private PurchaseOperation()
    {
    }

    private PurchaseOperation(Guid operationId, Guid tenantId)
        : base(operationId, tenantId)
    {
    }

    public Guid OperationId => Id;

    public Guid PurchaseOrderId { get; private set; }

    public string Action { get; private set; } = string.Empty;

    public string ResultPayloadJson { get; private set; } = "{}";

    public DateTimeOffset ProcessedAtUtc { get; private set; }

    public static PurchaseOperation Create(
        Guid operationId,
        Guid tenantId,
        Guid purchaseOrderId,
        string action,
        string resultPayloadJson,
        DateTimeOffset utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(resultPayloadJson);
        if (purchaseOrderId == Guid.Empty)
        {
            throw new ArgumentException("Purchase order id is required.", nameof(purchaseOrderId));
        }

        return new PurchaseOperation(operationId, tenantId)
        {
            PurchaseOrderId = purchaseOrderId,
            Action = action.Trim(),
            ResultPayloadJson = resultPayloadJson,
            ProcessedAtUtc = utcNow,
        };
    }
}

public static class PurchaseOperationActions
{
    public const string Confirm = "Confirm";
    public const string Receive = "Receive";
    public const string Return = "Return";
    public const string Close = "Close";
    public const string Cancel = "Cancel";
    public const string Payment = "Payment";
}

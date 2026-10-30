using Warehouse.Domain.Common;

namespace Warehouse.Domain.Sales;

/// <summary>
/// TENANT-OWNED idempotency record for sales confirm/ship/return/close/cancel/payment.
/// OperationId is the primary key (client-supplied).
/// </summary>
public sealed class SalesOperation : TenantOwnedEntity
{
    private SalesOperation()
    {
    }

    private SalesOperation(Guid operationId, Guid tenantId)
        : base(operationId, tenantId)
    {
    }

    public Guid OperationId => Id;

    public Guid SalesOrderId { get; private set; }

    public string Action { get; private set; } = string.Empty;

    public string ResultPayloadJson { get; private set; } = "{}";

    public DateTimeOffset ProcessedAtUtc { get; private set; }

    public static SalesOperation Create(
        Guid operationId,
        Guid tenantId,
        Guid salesOrderId,
        string action,
        string resultPayloadJson,
        DateTimeOffset utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(resultPayloadJson);
        if (salesOrderId == Guid.Empty)
        {
            throw new ArgumentException("Sales order id is required.", nameof(salesOrderId));
        }

        return new SalesOperation(operationId, tenantId)
        {
            SalesOrderId = salesOrderId,
            Action = action.Trim(),
            ResultPayloadJson = resultPayloadJson,
            ProcessedAtUtc = utcNow,
        };
    }
}

public static class SalesOperationActions
{
    public const string Confirm = "Confirm";
    public const string Ship = "Ship";
    public const string Return = "Return";
    public const string Close = "Close";
    public const string Cancel = "Cancel";
    public const string Payment = "Payment";
}

using Warehouse.Domain.Common;

namespace Warehouse.Domain.Inventory;

/// <summary>
/// TENANT-OWNED idempotency record for transfer dispatch/receive operations.
/// OperationId is the primary key (client-supplied).
/// </summary>
public sealed class TransferOperation : TenantOwnedEntity
{
    private TransferOperation()
    {
    }

    private TransferOperation(Guid operationId, Guid tenantId)
        : base(operationId, tenantId)
    {
    }

    public Guid OperationId => Id;

    public Guid TransferId { get; private set; }

    public string Action { get; private set; } = string.Empty;

    public string ResultPayloadJson { get; private set; } = "{}";

    public DateTimeOffset ProcessedAtUtc { get; private set; }

    public static TransferOperation Create(
        Guid operationId,
        Guid tenantId,
        Guid transferId,
        string action,
        string resultPayloadJson,
        DateTimeOffset utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(resultPayloadJson);
        if (transferId == Guid.Empty)
        {
            throw new ArgumentException("Transfer id is required.", nameof(transferId));
        }

        return new TransferOperation(operationId, tenantId)
        {
            TransferId = transferId,
            Action = action.Trim(),
            ResultPayloadJson = resultPayloadJson,
            ProcessedAtUtc = utcNow,
        };
    }
}

public static class TransferOperationActions
{
    public const string Dispatch = "Dispatch";
    public const string Receive = "Receive";
    public const string Cancel = "Cancel";
}

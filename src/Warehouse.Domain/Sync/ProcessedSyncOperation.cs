using Warehouse.Domain.Common;

namespace Warehouse.Domain.Sync;

/// <summary>
/// TENANT-OWNED idempotency store for client sync operations.
/// OperationId is globally unique and is the primary key.
/// </summary>
public sealed class ProcessedSyncOperation : TenantOwnedEntity
{
    private ProcessedSyncOperation()
    {
    }

    private ProcessedSyncOperation(Guid operationId, Guid tenantId)
        : base(operationId, tenantId)
    {
    }

    public Guid OperationId => Id;

    public Guid DeviceId { get; private set; }

    public Guid UserId { get; private set; }

    public string EntityType { get; private set; } = string.Empty;

    public Guid EntityId { get; private set; }

    public string OperationType { get; private set; } = string.Empty;

    public SyncOperationProcessingStatus Status { get; private set; }

    public string ResultPayloadJson { get; private set; } = "{}";

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset ProcessedAtUtc { get; private set; }

    public static ProcessedSyncOperation CreateSucceeded(
        Guid operationId,
        Guid tenantId,
        Guid deviceId,
        Guid userId,
        string entityType,
        Guid entityId,
        string operationType,
        string resultPayloadJson,
        DateTimeOffset utcNow)
    {
        return Create(
            operationId,
            tenantId,
            deviceId,
            userId,
            entityType,
            entityId,
            operationType,
            SyncOperationProcessingStatus.Succeeded,
            resultPayloadJson,
            utcNow);
    }

    public static ProcessedSyncOperation CreateRejected(
        Guid operationId,
        Guid tenantId,
        Guid deviceId,
        Guid userId,
        string entityType,
        Guid entityId,
        string operationType,
        string resultPayloadJson,
        DateTimeOffset utcNow)
    {
        return Create(
            operationId,
            tenantId,
            deviceId,
            userId,
            entityType,
            entityId,
            operationType,
            SyncOperationProcessingStatus.Rejected,
            resultPayloadJson,
            utcNow);
    }

    private static ProcessedSyncOperation Create(
        Guid operationId,
        Guid tenantId,
        Guid deviceId,
        Guid userId,
        string entityType,
        Guid entityId,
        string operationType,
        SyncOperationProcessingStatus status,
        string resultPayloadJson,
        DateTimeOffset utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationType);
        ArgumentException.ThrowIfNullOrWhiteSpace(resultPayloadJson);

        if (deviceId == Guid.Empty)
        {
            throw new ArgumentException("Device id is required.", nameof(deviceId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        if (entityId == Guid.Empty)
        {
            throw new ArgumentException("Entity id is required.", nameof(entityId));
        }

        return new ProcessedSyncOperation(operationId, tenantId)
        {
            DeviceId = deviceId,
            UserId = userId,
            EntityType = entityType.Trim(),
            EntityId = entityId,
            OperationType = operationType.Trim(),
            Status = status,
            ResultPayloadJson = resultPayloadJson,
            CreatedAtUtc = utcNow,
            ProcessedAtUtc = utcNow,
        };
    }
}

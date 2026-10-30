using Warehouse.Domain.Common;

namespace Warehouse.Domain.Sync;

/// <summary>
/// TENANT-OWNED pull change-feed row. ChangeVersion is monotonic per tenant.
/// </summary>
public sealed class ChangeLogEntry : TenantOwnedEntity
{
    private ChangeLogEntry()
    {
    }

    private ChangeLogEntry(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public long ChangeVersion { get; private set; }

    public string EntityType { get; private set; } = string.Empty;

    public Guid EntityId { get; private set; }

    public ChangeLogOperation Operation { get; private set; }

    public string PayloadJson { get; private set; } = "{}";

    public DateTimeOffset OccurredAtUtc { get; private set; }

    public Guid? DeviceId { get; private set; }

    public Guid? UserId { get; private set; }

    public static ChangeLogEntry Create(
        Guid id,
        Guid tenantId,
        long changeVersion,
        string entityType,
        Guid entityId,
        ChangeLogOperation operation,
        string payloadJson,
        DateTimeOffset utcNow,
        Guid? deviceId,
        Guid? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        ArgumentException.ThrowIfNullOrWhiteSpace(payloadJson);
        if (changeVersion <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(changeVersion), "Change version must be positive.");
        }

        if (entityId == Guid.Empty)
        {
            throw new ArgumentException("Entity id is required.", nameof(entityId));
        }

        return new ChangeLogEntry(id, tenantId)
        {
            ChangeVersion = changeVersion,
            EntityType = entityType.Trim(),
            EntityId = entityId,
            Operation = operation,
            PayloadJson = payloadJson,
            OccurredAtUtc = utcNow,
            DeviceId = deviceId,
            UserId = userId,
        };
    }
}

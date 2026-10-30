using Warehouse.Domain.Common;

namespace Warehouse.Domain.Audit;

/// <summary>
/// Append-only audit record. Application code must not update or delete rows.
/// Retention/deletion is out of scope for v1.
/// TenantId is required for tenant actions; platform-only events may omit it only when no tenant is in scope.
/// </summary>
public sealed class AuditLog : Entity
{
    private AuditLog()
    {
    }

    private AuditLog(Guid id)
        : base(id)
    {
    }

    public Guid? TenantId { get; private set; }

    public Guid? UserId { get; private set; }

    public Guid? DeviceId { get; private set; }

    public string Action { get; private set; } = string.Empty;

    public string EntityType { get; private set; } = string.Empty;

    public Guid? EntityId { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }

    public string? CorrelationId { get; private set; }

    public string? IpAddress { get; private set; }

    /// <summary>
    /// Structured JSON metadata, optionally including before/after snapshots.
    /// </summary>
    public string? MetadataJson { get; private set; }

    public static AuditLog Create(
        Guid id,
        Guid? tenantId,
        Guid? userId,
        Guid? deviceId,
        string action,
        string entityType,
        Guid? entityId,
        DateTimeOffset occurredAtUtc,
        string? correlationId,
        string? ipAddress,
        string? metadataJson)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);

        return new AuditLog(id)
        {
            TenantId = tenantId,
            UserId = userId,
            DeviceId = deviceId,
            Action = action.Trim(),
            EntityType = entityType.Trim(),
            EntityId = entityId,
            OccurredAtUtc = occurredAtUtc,
            CorrelationId = string.IsNullOrWhiteSpace(correlationId) ? null : correlationId.Trim(),
            IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? null : ipAddress.Trim(),
            MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? null : metadataJson,
        };
    }
}

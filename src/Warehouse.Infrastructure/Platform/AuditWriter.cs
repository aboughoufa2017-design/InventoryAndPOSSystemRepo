using System.Text.Json;
using Warehouse.Application.Platform;
using Warehouse.Application.Security;
using Warehouse.Application.Tenancy;
using Warehouse.Domain.Audit;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Infrastructure.Platform;

public sealed class AuditWriter(
    WarehouseDbContext db,
    ICurrentUser currentUser,
    ITenantContext tenantContext) : IAuditWriter
{
    public async Task WriteAsync(
        Guid? tenantId,
        string action,
        string entityType,
        Guid? entityId,
        string? metadataJson,
        CancellationToken cancellationToken)
    {
        // Platform operators are not rows in users; FK audit_logs.user_id → users forbids their ids.
        Guid? userId = null;
        string? enrichedMetadata = metadataJson;

        if (tenantContext.IsPlatformOperator)
        {
            var meta = string.IsNullOrWhiteSpace(metadataJson)
                ? new Dictionary<string, object?>()
                : JsonSerializer.Deserialize<Dictionary<string, object?>>(metadataJson) ?? new Dictionary<string, object?>();
            if (currentUser.UserId is Guid operatorId)
            {
                meta["platformOperatorId"] = operatorId;
            }
            if (!string.IsNullOrWhiteSpace(currentUser.Email))
            {
                meta["platformOperatorEmail"] = currentUser.Email;
            }
            enrichedMetadata = JsonSerializer.Serialize(meta);
        }
        else
        {
            userId = currentUser.UserId;
        }

        db.AuditLogs.Add(AuditLog.Create(
            Guid.NewGuid(),
            tenantId,
            userId,
            deviceId: tenantContext.IsPlatformOperator ? null : currentUser.DeviceId,
            action,
            entityType,
            entityId,
            DateTimeOffset.UtcNow,
            correlationId: null,
            ipAddress: null,
            enrichedMetadata));

        await db.SaveChangesAsync(cancellationToken);
    }
}

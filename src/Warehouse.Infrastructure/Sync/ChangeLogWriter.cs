using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Security;
using Warehouse.Application.Sync;
using Warehouse.Application.Tenancy;
using Warehouse.Domain.Sync;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Infrastructure.Sync;

public sealed class ChangeLogWriter(
    WarehouseDbContext db,
    ITenantContext tenantContext,
    ICurrentUser currentUser) : IChangeLogWriter
{
    public async Task<long> AppendAsync(
        string entityType,
        Guid entityId,
        ChangeLogOperation operation,
        string payloadJson,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            throw new InvalidOperationException("Tenant context is required to append change log.");
        }

        var counter = await db.TenantChangeCounters.FirstOrDefaultAsync(c => c.TenantId == tenantId, cancellationToken);
        if (counter is null)
        {
            counter = TenantChangeCounter.Create(tenantId);
            db.TenantChangeCounters.Add(counter);
        }

        var version = counter.AllocateNext();
        db.ChangeLogEntries.Add(ChangeLogEntry.Create(
            Guid.NewGuid(),
            tenantId,
            version,
            entityType,
            entityId,
            operation,
            string.IsNullOrWhiteSpace(payloadJson) ? "{}" : payloadJson,
            DateTimeOffset.UtcNow,
            currentUser.DeviceId,
            currentUser.UserId));

        return version;
    }
}

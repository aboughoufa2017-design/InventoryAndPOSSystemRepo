namespace Warehouse.Domain.Sync;

/// <summary>
/// Per-tenant monotonic change-version allocator for the pull feed.
/// TenantId is the primary key.
/// </summary>
public sealed class TenantChangeCounter
{
    private TenantChangeCounter()
    {
    }

    private TenantChangeCounter(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant id is required.", nameof(tenantId));
        }

        TenantId = tenantId;
    }

    public Guid TenantId { get; private set; }

    /// <summary>Last allocated change version. Next allocation returns this + 1.</summary>
    public long LastChangeVersion { get; private set; }

    public static TenantChangeCounter Create(Guid tenantId) => new(tenantId);

    public long AllocateNext()
    {
        LastChangeVersion++;
        return LastChangeVersion;
    }
}

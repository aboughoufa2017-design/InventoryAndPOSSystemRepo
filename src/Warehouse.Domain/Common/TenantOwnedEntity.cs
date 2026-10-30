namespace Warehouse.Domain.Common;

/// <summary>
/// Entity whose rows belong to exactly one tenant.
/// TenantId is part of the identity for isolation. It is assigned at creation and is not a client-editable field.
/// </summary>
public abstract class TenantOwnedEntity : Entity, ITenantOwned
{
    /// <summary>
    /// Required by EF Core materialization. Application code must use the Guid constructor.
    /// </summary>
    protected TenantOwnedEntity()
    {
    }

    protected TenantOwnedEntity(Guid id, Guid tenantId)
        : base(id)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant id must be a non-empty UUID.", nameof(tenantId));
        }

        TenantId = tenantId;
    }

    public Guid TenantId { get; protected set; }
}

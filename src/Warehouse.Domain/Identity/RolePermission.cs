using Warehouse.Domain.Common;

namespace Warehouse.Domain.Identity;

/// <summary>
/// TENANT-OWNED join between role and permission.
/// </summary>
public sealed class RolePermission : TenantOwnedEntity
{
    private RolePermission()
    {
    }

    private RolePermission(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid RoleId { get; private set; }

    public Guid PermissionId { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static RolePermission Create(
        Guid id,
        Guid tenantId,
        Guid roleId,
        Guid permissionId,
        DateTimeOffset utcNow)
    {
        if (roleId == Guid.Empty)
        {
            throw new ArgumentException("Role id is required.", nameof(roleId));
        }

        if (permissionId == Guid.Empty)
        {
            throw new ArgumentException("Permission id is required.", nameof(permissionId));
        }

        return new RolePermission(id, tenantId)
        {
            RoleId = roleId,
            PermissionId = permissionId,
            CreatedAtUtc = utcNow,
        };
    }
}

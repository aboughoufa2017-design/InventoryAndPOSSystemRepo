using Warehouse.Domain.Common;

namespace Warehouse.Domain.Identity;

/// <summary>
/// TENANT-OWNED join between user and role.
/// </summary>
public sealed class UserRole : TenantOwnedEntity
{
    private UserRole()
    {
    }

    private UserRole(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid UserId { get; private set; }

    public Guid RoleId { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static UserRole Create(
        Guid id,
        Guid tenantId,
        Guid userId,
        Guid roleId,
        DateTimeOffset utcNow)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        if (roleId == Guid.Empty)
        {
            throw new ArgumentException("Role id is required.", nameof(roleId));
        }

        return new UserRole(id, tenantId)
        {
            UserId = userId,
            RoleId = roleId,
            CreatedAtUtc = utcNow,
        };
    }
}

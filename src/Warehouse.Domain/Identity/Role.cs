using Warehouse.Domain.Common;

namespace Warehouse.Domain.Identity;

/// <summary>
/// TENANT-OWNED role. System role codes (Owner, Administrator, ...) are seeded per tenant when identity is implemented.
/// </summary>
public sealed class Role : TenantOwnedEntity
{
    private Role()
    {
    }

    private Role(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public bool IsSystem { get; private set; }

    public bool IsActive { get; private set; } = true;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public int RowVersion { get; private set; } = 1;

    public static Role Create(
        Guid id,
        Guid tenantId,
        string code,
        string name,
        bool isSystem,
        DateTimeOffset utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Role(id, tenantId)
        {
            Code = code.Trim(),
            Name = name.Trim(),
            IsSystem = isSystem,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
        };
    }
}

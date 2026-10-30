using Warehouse.Domain.Common;

namespace Warehouse.Domain.Warehousing;

/// <summary>
/// TENANT-OWNED warehouse. Every warehouse must have at least GENERAL and IN_TRANSIT locations.
/// </summary>
public sealed class Warehouse : TenantOwnedEntity
{
    private Warehouse()
    {
    }

    private Warehouse(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public bool IsActive { get; private set; } = true;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public int RowVersion { get; private set; } = 1;

    public static Warehouse Create(
        Guid id,
        Guid tenantId,
        string code,
        string name,
        DateTimeOffset utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Warehouse(id, tenantId)
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
        };
    }

    public void Update(string name, bool isActive, DateTimeOffset utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        IsActive = isActive;
        UpdatedAtUtc = utcNow;
        RowVersion++;
    }
}

using Warehouse.Domain.Common;

namespace Warehouse.Domain.Catalog;

/// <summary>
/// TENANT-OWNED unit of measure (piece, box, kg, …).
/// </summary>
public sealed class UnitOfMeasure : TenantOwnedEntity
{
    private UnitOfMeasure()
    {
    }

    private UnitOfMeasure(Guid id, Guid tenantId)
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

    public static UnitOfMeasure Create(
        Guid id,
        Guid tenantId,
        string code,
        string name,
        bool isSystem,
        DateTimeOffset utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new UnitOfMeasure(id, tenantId)
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            IsSystem = isSystem,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
        };
    }

    public void Rename(string name, DateTimeOffset utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        Touch(utcNow);
    }

    public void SetActive(bool isActive, DateTimeOffset utcNow)
    {
        IsActive = isActive;
        Touch(utcNow);
    }

    private void Touch(DateTimeOffset utcNow)
    {
        UpdatedAtUtc = utcNow;
        RowVersion++;
    }
}

/// <summary>
/// Well-known unit codes seeded per tenant.
/// </summary>
public static class SystemUnits
{
    public static readonly IReadOnlyList<(string Code, string Name)> Catalog =
    [
        ("PIECE", "Piece"),
        ("BOX", "Box"),
        ("CARTON", "Carton"),
        ("KG", "Kilogram"),
        ("G", "Gram"),
        ("L", "Liter"),
        ("M", "Meter"),
    ];
}

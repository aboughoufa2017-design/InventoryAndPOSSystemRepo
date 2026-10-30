using Warehouse.Domain.Common;

namespace Warehouse.Domain.Warehousing;

/// <summary>
/// TENANT-OWNED location/bin inside a warehouse.
/// Future StockBalance rows require a non-null LocationId pointing here.
/// </summary>
public sealed class WarehouseLocation : TenantOwnedEntity
{
    private WarehouseLocation()
    {
    }

    private WarehouseLocation(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid WarehouseId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public WarehouseLocationKind Kind { get; private set; }

    public bool IsDefault { get; private set; }

    public bool IsActive { get; private set; } = true;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public int RowVersion { get; private set; } = 1;

    public static WarehouseLocation Create(
        Guid id,
        Guid tenantId,
        Guid warehouseId,
        string code,
        string name,
        WarehouseLocationKind kind,
        bool isDefault,
        DateTimeOffset utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (warehouseId == Guid.Empty)
        {
            throw new ArgumentException("Warehouse id is required.", nameof(warehouseId));
        }

        if (kind == WarehouseLocationKind.InTransit && isDefault)
        {
            throw new ArgumentException("In-transit locations cannot be the default available location.");
        }

        return new WarehouseLocation(id, tenantId)
        {
            WarehouseId = warehouseId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Kind = kind,
            IsDefault = isDefault,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
        };
    }

    public static WarehouseLocation CreateGeneral(Guid id, Guid tenantId, Guid warehouseId, DateTimeOffset utcNow) =>
        Create(
            id,
            tenantId,
            warehouseId,
            WarehouseLocationCodes.General,
            "General",
            WarehouseLocationKind.System,
            isDefault: true,
            utcNow);

    public static WarehouseLocation CreateInTransit(Guid id, Guid tenantId, Guid warehouseId, DateTimeOffset utcNow) =>
        Create(
            id,
            tenantId,
            warehouseId,
            WarehouseLocationCodes.InTransit,
            "In Transit",
            WarehouseLocationKind.InTransit,
            isDefault: false,
            utcNow);

    public bool IsAvailableForSaleOrPick => Kind != WarehouseLocationKind.InTransit;
}

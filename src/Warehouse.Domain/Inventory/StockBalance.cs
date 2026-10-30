using Warehouse.Domain.Common;
using Warehouse.Domain.Precision;

namespace Warehouse.Domain.Inventory;

/// <summary>
/// TENANT-OWNED current stock projection per product + warehouse + location.
/// Available is never persisted: Available = OnHand - Reserved.
/// </summary>
public sealed class StockBalance : TenantOwnedEntity
{
    private StockBalance()
    {
    }

    private StockBalance(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid ProductId { get; private set; }

    public Guid WarehouseId { get; private set; }

    public Guid LocationId { get; private set; }

    public decimal OnHand { get; private set; }

    public decimal Reserved { get; private set; }

    /// <summary>Calculated only — not mapped to the database.</summary>
    public decimal Available => OnHand - Reserved;

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public int RowVersion { get; private set; } = 1;

    public static StockBalance CreateEmpty(
        Guid id,
        Guid tenantId,
        Guid productId,
        Guid warehouseId,
        Guid locationId,
        DateTimeOffset utcNow)
    {
        if (productId == Guid.Empty || warehouseId == Guid.Empty || locationId == Guid.Empty)
        {
            throw new ArgumentException("Product, warehouse, and location ids are required.");
        }

        return new StockBalance(id, tenantId)
        {
            ProductId = productId,
            WarehouseId = warehouseId,
            LocationId = locationId,
            OnHand = 0m,
            Reserved = 0m,
            UpdatedAtUtc = utcNow,
        };
    }

    public void ApplyOnHandDelta(decimal quantityChange, DateTimeOffset utcNow)
    {
        var next = Round(OnHand + quantityChange);
        OnHand = next;
        UpdatedAtUtc = utcNow;
        RowVersion++;
    }

    public void ApplyReservedDelta(decimal quantityChange, DateTimeOffset utcNow)
    {
        var next = Round(Reserved + quantityChange);
        if (next < 0)
        {
            throw new InvalidOperationException("Reserved quantity cannot be negative.");
        }

        Reserved = next;
        UpdatedAtUtc = utcNow;
        RowVersion++;
    }

    private static decimal Round(decimal value) =>
        Math.Round(value, QuantityPrecision.Scale, MidpointRounding.AwayFromZero);
}

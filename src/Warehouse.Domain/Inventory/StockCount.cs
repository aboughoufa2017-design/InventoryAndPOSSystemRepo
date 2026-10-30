using Warehouse.Domain.Common;

namespace Warehouse.Domain.Inventory;

public enum StockCountStatus
{
    Draft = 0,
    Counted = 1,
    Posted = 2,
    Cancelled = 3,
}

/// <summary>
/// TENANT-OWNED physical stock count. Posting creates a correction adjustment via the inventory engine.
/// </summary>
public sealed class StockCount : TenantOwnedEntity
{
    private StockCount()
    {
    }

    private StockCount(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid WarehouseId { get; private set; }

    public Guid LocationId { get; private set; }

    public StockCountStatus Status { get; private set; } = StockCountStatus.Draft;

    public string? Notes { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public DateTimeOffset? PostedAtUtc { get; private set; }

    public Guid? PostedAdjustmentId { get; private set; }

    public int RowVersion { get; private set; } = 1;

    public static StockCount Create(
        Guid id,
        Guid tenantId,
        Guid warehouseId,
        Guid locationId,
        string? notes,
        DateTimeOffset utcNow)
    {
        if (warehouseId == Guid.Empty || locationId == Guid.Empty)
        {
            throw new ArgumentException("Warehouse and location are required.");
        }

        return new StockCount(id, tenantId)
        {
            WarehouseId = warehouseId,
            LocationId = locationId,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
        };
    }

    public void MarkCounted(DateTimeOffset utcNow)
    {
        if (Status is not (StockCountStatus.Draft or StockCountStatus.Counted))
        {
            throw new InvalidOperationException($"Cannot mark counted when status is {Status}.");
        }

        Status = StockCountStatus.Counted;
        Touch(utcNow);
    }

    public void MarkPosted(Guid? adjustmentId, DateTimeOffset utcNow)
    {
        if (Status != StockCountStatus.Counted)
        {
            throw new InvalidOperationException("Only counted stock counts can be posted.");
        }

        if (adjustmentId == Guid.Empty)
        {
            throw new ArgumentException("Adjustment id cannot be empty.", nameof(adjustmentId));
        }

        Status = StockCountStatus.Posted;
        PostedAdjustmentId = adjustmentId;
        PostedAtUtc = utcNow;
        Touch(utcNow);
    }

    public void Cancel(DateTimeOffset utcNow)
    {
        if (Status is StockCountStatus.Posted or StockCountStatus.Cancelled)
        {
            throw new InvalidOperationException($"Cannot cancel when status is {Status}.");
        }

        Status = StockCountStatus.Cancelled;
        Touch(utcNow);
    }

    public void EnsureEditable()
    {
        if (Status is not (StockCountStatus.Draft or StockCountStatus.Counted))
        {
            throw new InvalidOperationException($"Stock count is {Status} and cannot be edited.");
        }
    }

    private void Touch(DateTimeOffset utcNow)
    {
        UpdatedAtUtc = utcNow;
        RowVersion++;
    }
}

public sealed class StockCountLine : TenantOwnedEntity
{
    private StockCountLine()
    {
    }

    private StockCountLine(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid StockCountId { get; private set; }

    public Guid ProductId { get; private set; }

    public decimal SystemQuantity { get; private set; }

    public decimal? CountedQuantity { get; private set; }

    public string? Notes { get; private set; }

    public static StockCountLine Create(
        Guid id,
        Guid tenantId,
        Guid stockCountId,
        Guid productId,
        decimal systemQuantity,
        string? notes)
    {
        if (stockCountId == Guid.Empty || productId == Guid.Empty)
        {
            throw new ArgumentException("Stock count and product are required.");
        }

        return new StockCountLine(id, tenantId)
        {
            StockCountId = stockCountId,
            ProductId = productId,
            SystemQuantity = decimal.Round(systemQuantity, 4),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
        };
    }

    public void SetCountedQuantity(decimal counted)
    {
        CountedQuantity = decimal.Round(counted, 4);
    }

    public decimal Variance => (CountedQuantity ?? SystemQuantity) - SystemQuantity;
}

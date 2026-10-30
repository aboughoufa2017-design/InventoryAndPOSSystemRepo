using Warehouse.Domain.Common;

namespace Warehouse.Domain.Inventory;

public enum StockAdjustmentKind
{
    OpeningBalance = 0,
    Correction = 1,
}

/// <summary>
/// TENANT-OWNED stock adjustment / opening-balance document.
/// Stock changes only when status becomes Posted.
/// </summary>
public sealed class StockAdjustment : TenantOwnedEntity
{
    private StockAdjustment()
    {
    }

    private StockAdjustment(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid WarehouseId { get; private set; }

    public StockAdjustmentKind Kind { get; private set; }

    public DocumentLifecycleStatus Status { get; private set; } = DocumentLifecycleStatus.Draft;

    public string? Reason { get; private set; }

    public Guid? CreatedByUserId { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public DateTimeOffset? PostedAtUtc { get; private set; }

    public int RowVersion { get; private set; } = 1;

    public static StockAdjustment Create(
        Guid id,
        Guid tenantId,
        Guid warehouseId,
        StockAdjustmentKind kind,
        string? reason,
        Guid? createdByUserId,
        DateTimeOffset utcNow)
    {
        if (warehouseId == Guid.Empty)
        {
            throw new ArgumentException("Warehouse id is required.", nameof(warehouseId));
        }

        return new StockAdjustment(id, tenantId)
        {
            WarehouseId = warehouseId,
            Kind = kind,
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
        };
    }

    public void MarkPosted(DateTimeOffset utcNow)
    {
        EnsureDraft();
        Status = DocumentLifecycleStatus.Posted;
        PostedAtUtc = utcNow;
        Touch(utcNow);
    }

    public void Cancel(DateTimeOffset utcNow)
    {
        EnsureDraft();
        Status = DocumentLifecycleStatus.Cancelled;
        Touch(utcNow);
    }

    private void EnsureDraft()
    {
        if (Status != DocumentLifecycleStatus.Draft)
        {
            throw new InvalidOperationException($"Adjustment is {Status} and cannot be modified.");
        }
    }

    private void Touch(DateTimeOffset utcNow)
    {
        UpdatedAtUtc = utcNow;
        RowVersion++;
    }
}

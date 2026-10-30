using Warehouse.Domain.Common;

namespace Warehouse.Domain.Purchasing;

/// <summary>
/// TENANT-OWNED goods receipt against a confirmed (or partially received) purchase order.
/// </summary>
public sealed class PurchaseReceipt : TenantOwnedEntity
{
    private PurchaseReceipt()
    {
    }

    private PurchaseReceipt(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid PurchaseOrderId { get; private set; }

    public Guid WarehouseId { get; private set; }

    public Guid LocationId { get; private set; }

    public string? Notes { get; private set; }

    public Guid? CreatedByUserId { get; private set; }

    public Guid? OperationId { get; private set; }

    public DateTimeOffset ReceivedAtUtc { get; private set; }

    public static PurchaseReceipt Create(
        Guid id,
        Guid tenantId,
        Guid purchaseOrderId,
        Guid warehouseId,
        Guid locationId,
        string? notes,
        Guid? createdByUserId,
        Guid? operationId,
        DateTimeOffset utcNow)
    {
        if (purchaseOrderId == Guid.Empty || warehouseId == Guid.Empty || locationId == Guid.Empty)
        {
            throw new ArgumentException("Purchase order, warehouse, and location are required.");
        }

        return new PurchaseReceipt(id, tenantId)
        {
            PurchaseOrderId = purchaseOrderId,
            WarehouseId = warehouseId,
            LocationId = locationId,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            CreatedByUserId = createdByUserId,
            OperationId = operationId,
            ReceivedAtUtc = utcNow,
        };
    }
}

using Warehouse.Domain.Common;
using Warehouse.Domain.Precision;

namespace Warehouse.Domain.Purchasing;

/// <summary>
/// TENANT-OWNED receipt line linked to a purchase order line.
/// </summary>
public sealed class PurchaseReceiptLine : TenantOwnedEntity
{
    private PurchaseReceiptLine()
    {
    }

    private PurchaseReceiptLine(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid PurchaseReceiptId { get; private set; }

    public Guid PurchaseOrderLineId { get; private set; }

    public Guid ProductId { get; private set; }

    public decimal Quantity { get; private set; }

    public static PurchaseReceiptLine Create(
        Guid id,
        Guid tenantId,
        Guid purchaseReceiptId,
        Guid purchaseOrderLineId,
        Guid productId,
        decimal quantity)
    {
        if (purchaseReceiptId == Guid.Empty || purchaseOrderLineId == Guid.Empty || productId == Guid.Empty)
        {
            throw new ArgumentException("Receipt, order line, and product ids are required.");
        }

        if (quantity <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        return new PurchaseReceiptLine(id, tenantId)
        {
            PurchaseReceiptId = purchaseReceiptId,
            PurchaseOrderLineId = purchaseOrderLineId,
            ProductId = productId,
            Quantity = Math.Round(quantity, QuantityPrecision.Scale, MidpointRounding.AwayFromZero),
        };
    }
}

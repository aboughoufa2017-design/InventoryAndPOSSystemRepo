using Warehouse.Domain.Common;
using Warehouse.Domain.Precision;

namespace Warehouse.Domain.Purchasing;

/// <summary>
/// TENANT-OWNED purchase return line linked to a purchase order line.
/// </summary>
public sealed class PurchaseReturnLine : TenantOwnedEntity
{
    private PurchaseReturnLine()
    {
    }

    private PurchaseReturnLine(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid PurchaseReturnId { get; private set; }

    public Guid PurchaseOrderLineId { get; private set; }

    public Guid ProductId { get; private set; }

    public decimal Quantity { get; private set; }

    public static PurchaseReturnLine Create(
        Guid id,
        Guid tenantId,
        Guid purchaseReturnId,
        Guid purchaseOrderLineId,
        Guid productId,
        decimal quantity)
    {
        if (purchaseReturnId == Guid.Empty || purchaseOrderLineId == Guid.Empty || productId == Guid.Empty)
        {
            throw new ArgumentException("Return, order line, and product ids are required.");
        }

        if (quantity <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        return new PurchaseReturnLine(id, tenantId)
        {
            PurchaseReturnId = purchaseReturnId,
            PurchaseOrderLineId = purchaseOrderLineId,
            ProductId = productId,
            Quantity = Math.Round(quantity, QuantityPrecision.Scale, MidpointRounding.AwayFromZero),
        };
    }
}

using Warehouse.Domain.Common;
using Warehouse.Domain.Precision;

namespace Warehouse.Domain.Purchasing;

/// <summary>
/// TENANT-OWNED purchase order line. Ordered/received/returned quantities are tracked explicitly.
/// </summary>
public sealed class PurchaseOrderLine : TenantOwnedEntity
{
    private PurchaseOrderLine()
    {
    }

    private PurchaseOrderLine(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid PurchaseOrderId { get; private set; }

    public Guid ProductId { get; private set; }

    public decimal QuantityOrdered { get; private set; }

    public decimal QuantityReceived { get; private set; }

    public decimal QuantityReturned { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal TaxRate { get; private set; }

    public decimal LineNet { get; private set; }

    public decimal LineTax { get; private set; }

    public decimal LineTotal { get; private set; }

    public string? Notes { get; private set; }

    public decimal QuantityRemainingToReceive => QuantityOrdered - QuantityReceived;

    public decimal QuantityReturnable => QuantityReceived - QuantityReturned;

    public static PurchaseOrderLine Create(
        Guid id,
        Guid tenantId,
        Guid purchaseOrderId,
        Guid productId,
        decimal quantityOrdered,
        decimal unitPrice,
        decimal taxRate,
        string? notes)
    {
        if (purchaseOrderId == Guid.Empty || productId == Guid.Empty)
        {
            throw new ArgumentException("Purchase order and product ids are required.");
        }

        if (quantityOrdered <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(quantityOrdered), "Quantity must be greater than zero.");
        }

        if (unitPrice < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price cannot be negative.");
        }

        if (taxRate < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(taxRate), "Tax rate cannot be negative.");
        }

        var qty = RoundQty(quantityOrdered);
        var price = RoundMoney(unitPrice);
        var rate = taxRate;
        var net = RoundMoney(qty * price);
        var tax = RoundMoney(net * rate);

        return new PurchaseOrderLine(id, tenantId)
        {
            PurchaseOrderId = purchaseOrderId,
            ProductId = productId,
            QuantityOrdered = qty,
            QuantityReceived = 0m,
            QuantityReturned = 0m,
            UnitPrice = price,
            TaxRate = rate,
            LineNet = net,
            LineTax = tax,
            LineTotal = RoundMoney(net + tax),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
        };
    }

    public void Receive(decimal quantity)
    {
        if (quantity <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Receive quantity must be greater than zero.");
        }

        var rounded = RoundQty(quantity);
        if (rounded > QuantityRemainingToReceive)
        {
            throw new InvalidOperationException("Cannot receive more than the remaining ordered quantity.");
        }

        QuantityReceived = RoundQty(QuantityReceived + rounded);
    }

    public void Return(decimal quantity)
    {
        if (quantity <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Return quantity must be greater than zero.");
        }

        var rounded = RoundQty(quantity);
        if (rounded > QuantityReturnable)
        {
            throw new InvalidOperationException("Cannot return more than the net received quantity.");
        }

        QuantityReturned = RoundQty(QuantityReturned + rounded);
    }

    private static decimal RoundQty(decimal value) =>
        Math.Round(value, QuantityPrecision.Scale, MidpointRounding.AwayFromZero);

    private static decimal RoundMoney(decimal value) =>
        Math.Round(value, MoneyPrecision.Scale, MidpointRounding.AwayFromZero);
}

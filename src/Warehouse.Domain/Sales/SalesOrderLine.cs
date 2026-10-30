using Warehouse.Domain.Common;
using Warehouse.Domain.Precision;

namespace Warehouse.Domain.Sales;

/// <summary>
/// TENANT-OWNED sales order line. Ordered/shipped/returned quantities are tracked explicitly.
/// </summary>
public sealed class SalesOrderLine : TenantOwnedEntity
{
    private SalesOrderLine()
    {
    }

    private SalesOrderLine(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid SalesOrderId { get; private set; }

    public Guid ProductId { get; private set; }

    public decimal QuantityOrdered { get; private set; }

    public decimal QuantityShipped { get; private set; }

    public decimal QuantityReturned { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal TaxRate { get; private set; }

    public decimal LineNet { get; private set; }

    public decimal LineTax { get; private set; }

    public decimal LineTotal { get; private set; }

    public string? Notes { get; private set; }

    public decimal QuantityRemainingToShip => QuantityOrdered - QuantityShipped;

    public decimal QuantityReturnable => QuantityShipped - QuantityReturned;

    public static SalesOrderLine Create(
        Guid id,
        Guid tenantId,
        Guid salesOrderId,
        Guid productId,
        decimal quantityOrdered,
        decimal unitPrice,
        decimal taxRate,
        string? notes)
    {
        if (salesOrderId == Guid.Empty || productId == Guid.Empty)
        {
            throw new ArgumentException("Sales order and product ids are required.");
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

        return new SalesOrderLine(id, tenantId)
        {
            SalesOrderId = salesOrderId,
            ProductId = productId,
            QuantityOrdered = qty,
            QuantityShipped = 0m,
            QuantityReturned = 0m,
            UnitPrice = price,
            TaxRate = rate,
            LineNet = net,
            LineTax = tax,
            LineTotal = RoundMoney(net + tax),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
        };
    }

    public void Ship(decimal quantity)
    {
        if (quantity <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Ship quantity must be greater than zero.");
        }

        var rounded = RoundQty(quantity);
        if (rounded > QuantityRemainingToShip)
        {
            throw new InvalidOperationException("Cannot ship more than the remaining ordered quantity.");
        }

        QuantityShipped = RoundQty(QuantityShipped + rounded);
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
            throw new InvalidOperationException("Cannot return more than the net shipped quantity.");
        }

        QuantityReturned = RoundQty(QuantityReturned + rounded);
    }

    private static decimal RoundQty(decimal value) =>
        Math.Round(value, QuantityPrecision.Scale, MidpointRounding.AwayFromZero);

    private static decimal RoundMoney(decimal value) =>
        Math.Round(value, MoneyPrecision.Scale, MidpointRounding.AwayFromZero);
}

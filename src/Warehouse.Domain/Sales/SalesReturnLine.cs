using Warehouse.Domain.Common;
using Warehouse.Domain.Precision;

namespace Warehouse.Domain.Sales;

/// <summary>
/// TENANT-OWNED sales return line linked to a sales order line.
/// </summary>
public sealed class SalesReturnLine : TenantOwnedEntity
{
    private SalesReturnLine()
    {
    }

    private SalesReturnLine(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid SalesReturnId { get; private set; }

    public Guid SalesOrderLineId { get; private set; }

    public Guid ProductId { get; private set; }

    public decimal Quantity { get; private set; }

    public static SalesReturnLine Create(
        Guid id,
        Guid tenantId,
        Guid salesReturnId,
        Guid salesOrderLineId,
        Guid productId,
        decimal quantity)
    {
        if (salesReturnId == Guid.Empty || salesOrderLineId == Guid.Empty || productId == Guid.Empty)
        {
            throw new ArgumentException("Return, order line, and product ids are required.");
        }

        if (quantity <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        return new SalesReturnLine(id, tenantId)
        {
            SalesReturnId = salesReturnId,
            SalesOrderLineId = salesOrderLineId,
            ProductId = productId,
            Quantity = Math.Round(quantity, QuantityPrecision.Scale, MidpointRounding.AwayFromZero),
        };
    }
}

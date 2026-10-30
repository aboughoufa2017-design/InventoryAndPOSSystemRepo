using Warehouse.Domain.Common;
using Warehouse.Domain.Precision;

namespace Warehouse.Domain.Sales;

/// <summary>
/// TENANT-OWNED shipment line linked to a sales order line.
/// </summary>
public sealed class SalesShipmentLine : TenantOwnedEntity
{
    private SalesShipmentLine()
    {
    }

    private SalesShipmentLine(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid SalesShipmentId { get; private set; }

    public Guid SalesOrderLineId { get; private set; }

    public Guid ProductId { get; private set; }

    public decimal Quantity { get; private set; }

    public static SalesShipmentLine Create(
        Guid id,
        Guid tenantId,
        Guid salesShipmentId,
        Guid salesOrderLineId,
        Guid productId,
        decimal quantity)
    {
        if (salesShipmentId == Guid.Empty || salesOrderLineId == Guid.Empty || productId == Guid.Empty)
        {
            throw new ArgumentException("Shipment, order line, and product ids are required.");
        }

        if (quantity <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        return new SalesShipmentLine(id, tenantId)
        {
            SalesShipmentId = salesShipmentId,
            SalesOrderLineId = salesOrderLineId,
            ProductId = productId,
            Quantity = Math.Round(quantity, QuantityPrecision.Scale, MidpointRounding.AwayFromZero),
        };
    }
}

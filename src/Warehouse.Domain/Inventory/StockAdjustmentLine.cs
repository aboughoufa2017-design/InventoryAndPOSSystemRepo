using Warehouse.Domain.Common;
using Warehouse.Domain.Precision;

namespace Warehouse.Domain.Inventory;

public sealed class StockAdjustmentLine : TenantOwnedEntity
{
    private StockAdjustmentLine()
    {
    }

    private StockAdjustmentLine(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid StockAdjustmentId { get; private set; }

    public Guid ProductId { get; private set; }

    public Guid LocationId { get; private set; }

    public decimal QuantityChange { get; private set; }

    public string? Notes { get; private set; }

    public static StockAdjustmentLine Create(
        Guid id,
        Guid tenantId,
        Guid stockAdjustmentId,
        Guid productId,
        Guid locationId,
        decimal quantityChange,
        string? notes)
    {
        if (productId == Guid.Empty || locationId == Guid.Empty || stockAdjustmentId == Guid.Empty)
        {
            throw new ArgumentException("Adjustment, product, and location ids are required.");
        }

        if (quantityChange == 0m)
        {
            throw new ArgumentException("Quantity change cannot be zero.", nameof(quantityChange));
        }

        return new StockAdjustmentLine(id, tenantId)
        {
            StockAdjustmentId = stockAdjustmentId,
            ProductId = productId,
            LocationId = locationId,
            QuantityChange = Math.Round(quantityChange, QuantityPrecision.Scale, MidpointRounding.AwayFromZero),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
        };
    }
}

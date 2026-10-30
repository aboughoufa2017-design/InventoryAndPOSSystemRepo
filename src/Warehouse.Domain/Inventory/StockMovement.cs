using Warehouse.Domain.Common;
using Warehouse.Domain.Precision;

namespace Warehouse.Domain.Inventory;

/// <summary>
/// TENANT-OWNED immutable stock ledger entry. Corrections are compensating movements, never edits.
/// </summary>
public sealed class StockMovement : TenantOwnedEntity
{
    private StockMovement()
    {
    }

    private StockMovement(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid ProductId { get; private set; }

    public Guid WarehouseId { get; private set; }

    public Guid LocationId { get; private set; }

    /// <summary>Signed OnHand change. Positive increases OnHand; negative decreases.</summary>
    public decimal QuantityChange { get; private set; }

    public StockMovementType MovementType { get; private set; }

    public string SourceDocumentType { get; private set; } = string.Empty;

    public Guid? SourceDocumentId { get; private set; }

    /// <summary>Optional link for transfer lines (Phase 5).</summary>
    public Guid? TransferLineId { get; private set; }

    public Guid? CreatedByUserId { get; private set; }

    public string? Notes { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static StockMovement Create(
        Guid id,
        Guid tenantId,
        Guid productId,
        Guid warehouseId,
        Guid locationId,
        decimal quantityChange,
        StockMovementType movementType,
        string sourceDocumentType,
        Guid? sourceDocumentId,
        Guid? transferLineId,
        Guid? createdByUserId,
        string? notes,
        DateTimeOffset occurredAtUtc)
    {
        if (productId == Guid.Empty || warehouseId == Guid.Empty || locationId == Guid.Empty)
        {
            throw new ArgumentException("Product, warehouse, and location ids are required.");
        }

        if (quantityChange == 0m)
        {
            throw new ArgumentException("Quantity change cannot be zero.", nameof(quantityChange));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(sourceDocumentType);

        return new StockMovement(id, tenantId)
        {
            ProductId = productId,
            WarehouseId = warehouseId,
            LocationId = locationId,
            QuantityChange = Math.Round(quantityChange, QuantityPrecision.Scale, MidpointRounding.AwayFromZero),
            MovementType = movementType,
            SourceDocumentType = sourceDocumentType.Trim(),
            SourceDocumentId = sourceDocumentId,
            TransferLineId = transferLineId,
            CreatedByUserId = createdByUserId,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            OccurredAtUtc = occurredAtUtc,
            CreatedAtUtc = occurredAtUtc,
        };
    }
}

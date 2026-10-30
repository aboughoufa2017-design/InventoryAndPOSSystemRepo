using Warehouse.Domain.Common;
using Warehouse.Domain.Precision;

namespace Warehouse.Domain.Inventory;

/// <summary>
/// TENANT-OWNED transfer line. QuantityRequested is the dispatched quantity.
/// QuantityReceived accumulates partial receives. Remaining in transit = Requested - Received.
/// </summary>
public sealed class TransferLine : TenantOwnedEntity
{
    private TransferLine()
    {
    }

    private TransferLine(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid TransferId { get; private set; }

    public Guid ProductId { get; private set; }

    public decimal QuantityRequested { get; private set; }

    public decimal QuantityReceived { get; private set; }

    public string? Notes { get; private set; }

    public decimal QuantityInTransit => QuantityRequested - QuantityReceived;

    public static TransferLine Create(
        Guid id,
        Guid tenantId,
        Guid transferId,
        Guid productId,
        decimal quantityRequested,
        string? notes)
    {
        if (transferId == Guid.Empty || productId == Guid.Empty)
        {
            throw new ArgumentException("Transfer and product ids are required.");
        }

        if (quantityRequested <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(quantityRequested), "Quantity must be greater than zero.");
        }

        return new TransferLine(id, tenantId)
        {
            TransferId = transferId,
            ProductId = productId,
            QuantityRequested = Round(quantityRequested),
            QuantityReceived = 0m,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
        };
    }

    public void Receive(decimal quantity)
    {
        if (quantity <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Receive quantity must be greater than zero.");
        }

        var rounded = Round(quantity);
        if (rounded > QuantityInTransit)
        {
            throw new InvalidOperationException("Cannot receive more than the remaining in-transit quantity.");
        }

        QuantityReceived = Round(QuantityReceived + rounded);
    }

    public void ReturnAllInTransitToSource()
    {
        // Cancellation returns remaining in-transit; received qty stays at destination.
        QuantityRequested = QuantityReceived;
    }

    private static decimal Round(decimal value) =>
        Math.Round(value, QuantityPrecision.Scale, MidpointRounding.AwayFromZero);
}

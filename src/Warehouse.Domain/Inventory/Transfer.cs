using Warehouse.Domain.Common;

namespace Warehouse.Domain.Inventory;

/// <summary>
/// Module-specific transfer lifecycle. Distinct from generic DocumentLifecycleStatus.
/// </summary>
public enum TransferStatus
{
    Draft = 0,
    Dispatched = 1,
    PartiallyReceived = 2,
    Completed = 3,
    Cancelled = 4,
}

/// <summary>
/// TENANT-OWNED warehouse-to-warehouse transfer header.
/// Inventory effects: Dispatch (source → source IN_TRANSIT), Receive (IN_TRANSIT → destination).
/// </summary>
public sealed class Transfer : TenantOwnedEntity
{
    private Transfer()
    {
    }

    private Transfer(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid SourceWarehouseId { get; private set; }

    public Guid DestinationWarehouseId { get; private set; }

    public Guid SourceLocationId { get; private set; }

    public Guid DestinationLocationId { get; private set; }

    public TransferStatus Status { get; private set; } = TransferStatus.Draft;

    public string? Notes { get; private set; }

    public Guid? CreatedByUserId { get; private set; }

    public Guid? DispatchOperationId { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public DateTimeOffset? DispatchedAtUtc { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public int RowVersion { get; private set; } = 1;

    public static Transfer Create(
        Guid id,
        Guid tenantId,
        Guid sourceWarehouseId,
        Guid destinationWarehouseId,
        Guid sourceLocationId,
        Guid destinationLocationId,
        string? notes,
        Guid? createdByUserId,
        DateTimeOffset utcNow)
    {
        if (sourceWarehouseId == Guid.Empty || destinationWarehouseId == Guid.Empty)
        {
            throw new ArgumentException("Source and destination warehouses are required.");
        }

        if (sourceWarehouseId == destinationWarehouseId)
        {
            throw new ArgumentException("Source and destination warehouses must differ.");
        }

        if (sourceLocationId == Guid.Empty || destinationLocationId == Guid.Empty)
        {
            throw new ArgumentException("Source and destination locations are required.");
        }

        return new Transfer(id, tenantId)
        {
            SourceWarehouseId = sourceWarehouseId,
            DestinationWarehouseId = destinationWarehouseId,
            SourceLocationId = sourceLocationId,
            DestinationLocationId = destinationLocationId,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
        };
    }

    public void MarkDispatched(Guid? operationId, DateTimeOffset utcNow)
    {
        if (Status != TransferStatus.Draft)
        {
            throw new InvalidOperationException($"Transfer is {Status} and cannot be dispatched.");
        }

        Status = TransferStatus.Dispatched;
        DispatchOperationId = operationId;
        DispatchedAtUtc = utcNow;
        Touch(utcNow);
    }

    public void ApplyReceiveProgress(bool anyRemainingInTransit, DateTimeOffset utcNow)
    {
        if (Status is not (TransferStatus.Dispatched or TransferStatus.PartiallyReceived))
        {
            throw new InvalidOperationException($"Transfer is {Status} and cannot receive.");
        }

        if (anyRemainingInTransit)
        {
            Status = TransferStatus.PartiallyReceived;
        }
        else
        {
            Status = TransferStatus.Completed;
            CompletedAtUtc = utcNow;
        }

        Touch(utcNow);
    }

    public void CancelDraft(DateTimeOffset utcNow)
    {
        if (Status != TransferStatus.Draft)
        {
            throw new InvalidOperationException("Only draft transfers can be cancelled without stock compensation.");
        }

        Status = TransferStatus.Cancelled;
        Touch(utcNow);
    }

    public void CancelInTransit(DateTimeOffset utcNow)
    {
        if (Status is not (TransferStatus.Dispatched or TransferStatus.PartiallyReceived))
        {
            throw new InvalidOperationException($"Transfer is {Status} and cannot be cancelled with stock return.");
        }

        Status = TransferStatus.Cancelled;
        Touch(utcNow);
    }

    public void EnsureRowVersion(int expectedRowVersion)
    {
        if (RowVersion != expectedRowVersion)
        {
            throw new InvalidOperationException("SYNC_CONFLICT");
        }
    }

    private void Touch(DateTimeOffset utcNow)
    {
        UpdatedAtUtc = utcNow;
        RowVersion++;
    }
}

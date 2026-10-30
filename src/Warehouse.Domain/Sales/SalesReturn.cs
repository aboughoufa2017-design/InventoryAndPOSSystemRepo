using Warehouse.Domain.Common;

namespace Warehouse.Domain.Sales;

/// <summary>
/// TENANT-OWNED sales return against previously shipped quantities.
/// </summary>
public sealed class SalesReturn : TenantOwnedEntity
{
    private SalesReturn()
    {
    }

    private SalesReturn(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid SalesOrderId { get; private set; }

    public Guid WarehouseId { get; private set; }

    public Guid LocationId { get; private set; }

    public string? Notes { get; private set; }

    public Guid? CreatedByUserId { get; private set; }

    public Guid? OperationId { get; private set; }

    public DateTimeOffset ReturnedAtUtc { get; private set; }

    public static SalesReturn Create(
        Guid id,
        Guid tenantId,
        Guid salesOrderId,
        Guid warehouseId,
        Guid locationId,
        string? notes,
        Guid? createdByUserId,
        Guid? operationId,
        DateTimeOffset utcNow)
    {
        if (salesOrderId == Guid.Empty || warehouseId == Guid.Empty || locationId == Guid.Empty)
        {
            throw new ArgumentException("Sales order, warehouse, and location are required.");
        }

        return new SalesReturn(id, tenantId)
        {
            SalesOrderId = salesOrderId,
            WarehouseId = warehouseId,
            LocationId = locationId,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            CreatedByUserId = createdByUserId,
            OperationId = operationId,
            ReturnedAtUtc = utcNow,
        };
    }
}

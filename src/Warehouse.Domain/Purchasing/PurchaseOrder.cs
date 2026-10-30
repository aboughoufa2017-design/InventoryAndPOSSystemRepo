using Warehouse.Domain.Commercial;
using Warehouse.Domain.Common;
using Warehouse.Domain.Precision;

namespace Warehouse.Domain.Purchasing;

/// <summary>
/// TENANT-OWNED purchase order header.
/// Confirmation does not change stock; receiving posts StockMovements.
/// </summary>
public sealed class PurchaseOrder : TenantOwnedEntity
{
    private PurchaseOrder()
    {
    }

    private PurchaseOrder(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid SupplierId { get; private set; }

    public Guid WarehouseId { get; private set; }

    public Guid LocationId { get; private set; }

    public PurchaseOrderStatus Status { get; private set; } = PurchaseOrderStatus.Draft;

    public PaymentStatus PaymentStatus { get; private set; } = PaymentStatus.Unpaid;

    public string? Reference { get; private set; }

    public string? Notes { get; private set; }

    public decimal Subtotal { get; private set; }

    public decimal TaxTotal { get; private set; }

    public decimal GrandTotal { get; private set; }

    public decimal PaidAmount { get; private set; }

    public Guid? CreatedByUserId { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public DateTimeOffset? ConfirmedAtUtc { get; private set; }

    public DateTimeOffset? ClosedAtUtc { get; private set; }

    public int RowVersion { get; private set; } = 1;

    public static PurchaseOrder Create(
        Guid id,
        Guid tenantId,
        Guid supplierId,
        Guid warehouseId,
        Guid locationId,
        string? reference,
        string? notes,
        Guid? createdByUserId,
        DateTimeOffset utcNow)
    {
        if (supplierId == Guid.Empty || warehouseId == Guid.Empty || locationId == Guid.Empty)
        {
            throw new ArgumentException("Supplier, warehouse, and location are required.");
        }

        return new PurchaseOrder(id, tenantId)
        {
            SupplierId = supplierId,
            WarehouseId = warehouseId,
            LocationId = locationId,
            Reference = string.IsNullOrWhiteSpace(reference) ? null : reference.Trim(),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
        };
    }

    public void SetTotals(decimal subtotal, decimal taxTotal, DateTimeOffset utcNow)
    {
        if (Status != PurchaseOrderStatus.Draft)
        {
            throw new InvalidOperationException("Totals can only be set while the order is Draft.");
        }

        if (subtotal < 0m || taxTotal < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(subtotal), "Totals cannot be negative.");
        }

        Subtotal = RoundMoney(subtotal);
        TaxTotal = RoundMoney(taxTotal);
        GrandTotal = RoundMoney(Subtotal + TaxTotal);
        Touch(utcNow);
    }

    public void Confirm(DateTimeOffset utcNow)
    {
        if (Status != PurchaseOrderStatus.Draft)
        {
            throw new InvalidOperationException($"Purchase order is {Status} and cannot be confirmed.");
        }

        Status = PurchaseOrderStatus.Confirmed;
        ConfirmedAtUtc = utcNow;
        Touch(utcNow);
    }

    public void ApplyReceiveProgress(bool anyRemainingToReceive, DateTimeOffset utcNow)
    {
        if (Status is not (PurchaseOrderStatus.Confirmed or PurchaseOrderStatus.PartiallyReceived or PurchaseOrderStatus.FullyReceived))
        {
            throw new InvalidOperationException($"Purchase order is {Status} and cannot receive.");
        }

        Status = anyRemainingToReceive
            ? PurchaseOrderStatus.PartiallyReceived
            : PurchaseOrderStatus.FullyReceived;
        Touch(utcNow);
    }

    public void RegisterReturn(DateTimeOffset utcNow)
    {
        if (Status is PurchaseOrderStatus.Draft or PurchaseOrderStatus.Cancelled or PurchaseOrderStatus.Confirmed)
        {
            throw new InvalidOperationException($"Purchase order is {Status} and cannot return stock.");
        }

        Touch(utcNow);
    }

    public void Close(DateTimeOffset utcNow)
    {
        if (Status != PurchaseOrderStatus.FullyReceived)
        {
            throw new InvalidOperationException("Only fully received purchase orders can be closed.");
        }

        Status = PurchaseOrderStatus.Closed;
        ClosedAtUtc = utcNow;
        Touch(utcNow);
    }

    public void Cancel(DateTimeOffset utcNow)
    {
        if (Status is not (PurchaseOrderStatus.Draft or PurchaseOrderStatus.Confirmed))
        {
            throw new InvalidOperationException($"Purchase order is {Status} and cannot be cancelled.");
        }

        Status = PurchaseOrderStatus.Cancelled;
        Touch(utcNow);
    }

    public void ApplyPayment(decimal paidAmount, DateTimeOffset utcNow)
    {
        if (Status == PurchaseOrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Cancelled purchase orders cannot accept payment updates.");
        }

        if (paidAmount < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(paidAmount), "Paid amount cannot be negative.");
        }

        var rounded = RoundMoney(paidAmount);
        if (rounded > GrandTotal)
        {
            throw new InvalidOperationException("Paid amount cannot exceed grand total.");
        }

        PaidAmount = rounded;
        if (PaidAmount == 0m)
        {
            PaymentStatus = PaymentStatus.Unpaid;
        }
        else if (PaidAmount >= GrandTotal)
        {
            PaymentStatus = PaymentStatus.Paid;
        }
        else
        {
            PaymentStatus = PaymentStatus.PartiallyPaid;
        }

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

    private static decimal RoundMoney(decimal value) =>
        Math.Round(value, MoneyPrecision.Scale, MidpointRounding.AwayFromZero);
}

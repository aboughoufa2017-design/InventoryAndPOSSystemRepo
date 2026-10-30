using Warehouse.Domain.Commercial;
using Warehouse.Domain.Common;
using Warehouse.Domain.Precision;

namespace Warehouse.Domain.Sales;

/// <summary>
/// TENANT-OWNED sales order header.
/// Confirmation does not change stock; shipping posts StockMovements.
/// </summary>
public sealed class SalesOrder : TenantOwnedEntity
{
    private SalesOrder()
    {
    }

    private SalesOrder(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid CustomerId { get; private set; }

    public Guid WarehouseId { get; private set; }

    public Guid LocationId { get; private set; }

    public SalesOrderStatus Status { get; private set; } = SalesOrderStatus.Draft;

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

    public static SalesOrder Create(
        Guid id,
        Guid tenantId,
        Guid customerId,
        Guid warehouseId,
        Guid locationId,
        string? reference,
        string? notes,
        Guid? createdByUserId,
        DateTimeOffset utcNow)
    {
        if (customerId == Guid.Empty || warehouseId == Guid.Empty || locationId == Guid.Empty)
        {
            throw new ArgumentException("Customer, warehouse, and location are required.");
        }

        return new SalesOrder(id, tenantId)
        {
            CustomerId = customerId,
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
        if (Status != SalesOrderStatus.Draft)
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
        if (Status != SalesOrderStatus.Draft)
        {
            throw new InvalidOperationException($"Sales order is {Status} and cannot be confirmed.");
        }

        Status = SalesOrderStatus.Confirmed;
        ConfirmedAtUtc = utcNow;
        Touch(utcNow);
    }

    public void ApplyShipProgress(bool anyRemainingToShip, DateTimeOffset utcNow)
    {
        if (Status is not (SalesOrderStatus.Confirmed or SalesOrderStatus.PartiallyFulfilled or SalesOrderStatus.Fulfilled))
        {
            throw new InvalidOperationException($"Sales order is {Status} and cannot ship.");
        }

        Status = anyRemainingToShip
            ? SalesOrderStatus.PartiallyFulfilled
            : SalesOrderStatus.Fulfilled;
        Touch(utcNow);
    }

    public void RegisterReturn(DateTimeOffset utcNow)
    {
        if (Status is SalesOrderStatus.Draft or SalesOrderStatus.Cancelled or SalesOrderStatus.Confirmed)
        {
            throw new InvalidOperationException($"Sales order is {Status} and cannot return stock.");
        }

        Touch(utcNow);
    }

    public void Close(DateTimeOffset utcNow)
    {
        if (Status != SalesOrderStatus.Fulfilled)
        {
            throw new InvalidOperationException("Only fulfilled sales orders can be closed.");
        }

        Status = SalesOrderStatus.Closed;
        ClosedAtUtc = utcNow;
        Touch(utcNow);
    }

    public void Cancel(DateTimeOffset utcNow)
    {
        if (Status is not (SalesOrderStatus.Draft or SalesOrderStatus.Confirmed))
        {
            throw new InvalidOperationException($"Sales order is {Status} and cannot be cancelled.");
        }

        Status = SalesOrderStatus.Cancelled;
        Touch(utcNow);
    }

    public void ApplyPayment(decimal paidAmount, DateTimeOffset utcNow)
    {
        if (Status == SalesOrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Cancelled sales orders cannot accept payment updates.");
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

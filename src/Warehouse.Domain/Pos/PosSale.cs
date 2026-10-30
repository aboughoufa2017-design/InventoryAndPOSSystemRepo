using Warehouse.Domain.Common;
using Warehouse.Domain.Precision;

namespace Warehouse.Domain.Pos;

public enum PosSaleStatus
{
    Completed = 0,
    Voided = 1,
    Refunded = 2,
}

public enum PosPaymentMethod
{
    Cash = 0,
    Credit = 1,
}

/// <summary>
/// TENANT-OWNED retail POS ticket. Distinct from B2B <c>SalesOrder</c>.
/// Branch = WarehouseId. Receipt-critical fields are snapshotted on lines.
/// </summary>
public sealed class PosSale : TenantOwnedEntity
{
    private PosSale()
    {
    }

    private PosSale(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid WarehouseId { get; private set; }

    public Guid LocationId { get; private set; }

    public Guid? DeviceId { get; private set; }

    public Guid? CashierUserId { get; private set; }

    public Guid? CustomerId { get; private set; }

    public Guid? CashShiftId { get; private set; }

    /// <summary>Client-generated idempotency key (sync / retry).</summary>
    public Guid ClientCommandId { get; private set; }

    public string ReceiptNumber { get; private set; } = string.Empty;

    public PosSaleStatus Status { get; private set; } = PosSaleStatus.Completed;

    public string CurrencyCode { get; private set; } = "LYD";

    public decimal Subtotal { get; private set; }

    public decimal DiscountTotal { get; private set; }

    public decimal TaxTotal { get; private set; }

    public decimal GrandTotal { get; private set; }

    public DateTimeOffset SoldAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public DateTimeOffset? VoidedAtUtc { get; private set; }

    public string? Notes { get; private set; }

    public int RowVersion { get; private set; } = 1;

    public static PosSale CreateCompleted(
        Guid id,
        Guid tenantId,
        Guid warehouseId,
        Guid locationId,
        Guid clientCommandId,
        string receiptNumber,
        string currencyCode,
        decimal subtotal,
        decimal discountTotal,
        decimal taxTotal,
        decimal grandTotal,
        DateTimeOffset soldAtUtc,
        Guid? deviceId,
        Guid? cashierUserId,
        Guid? customerId,
        Guid? cashShiftId,
        string? notes)
    {
        if (warehouseId == Guid.Empty || locationId == Guid.Empty || clientCommandId == Guid.Empty)
        {
            throw new ArgumentException("Warehouse, location, and client command id are required.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(receiptNumber);

        return new PosSale(id, tenantId)
        {
            WarehouseId = warehouseId,
            LocationId = locationId,
            DeviceId = deviceId,
            CashierUserId = cashierUserId,
            CustomerId = customerId,
            CashShiftId = cashShiftId,
            ClientCommandId = clientCommandId,
            ReceiptNumber = receiptNumber.Trim(),
            CurrencyCode = string.IsNullOrWhiteSpace(currencyCode) ? "LYD" : currencyCode.Trim().ToUpperInvariant(),
            Subtotal = RoundMoney(subtotal),
            DiscountTotal = RoundMoney(discountTotal),
            TaxTotal = RoundMoney(taxTotal),
            GrandTotal = RoundMoney(grandTotal),
            SoldAtUtc = soldAtUtc,
            CreatedAtUtc = soldAtUtc,
            UpdatedAtUtc = soldAtUtc,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            Status = PosSaleStatus.Completed,
        };
    }

    public void Void(DateTimeOffset utcNow)
    {
        if (Status != PosSaleStatus.Completed)
        {
            throw new InvalidOperationException($"Cannot void sale in status {Status}.");
        }

        Status = PosSaleStatus.Voided;
        VoidedAtUtc = utcNow;
        Touch(utcNow);
    }

    public void MarkRefunded(DateTimeOffset utcNow)
    {
        if (Status != PosSaleStatus.Completed)
        {
            throw new InvalidOperationException($"Cannot refund sale in status {Status}.");
        }

        Status = PosSaleStatus.Refunded;
        Touch(utcNow);
    }

    private void Touch(DateTimeOffset utcNow)
    {
        UpdatedAtUtc = utcNow;
        RowVersion++;
    }

    private static decimal RoundMoney(decimal value) =>
        Math.Round(value, MoneyPrecision.Scale, MidpointRounding.AwayFromZero);
}

public sealed class PosSaleLine : TenantOwnedEntity
{
    private PosSaleLine()
    {
    }

    private PosSaleLine(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid PosSaleId { get; private set; }

    public Guid ProductId { get; private set; }

    public string SkuSnapshot { get; private set; } = string.Empty;

    public string NameSnapshot { get; private set; } = string.Empty;

    public decimal Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal DiscountAmount { get; private set; }

    public decimal TaxAmount { get; private set; }

    public decimal LineTotal { get; private set; }

    public static PosSaleLine Create(
        Guid id,
        Guid tenantId,
        Guid posSaleId,
        Guid productId,
        string skuSnapshot,
        string nameSnapshot,
        decimal quantity,
        decimal unitPrice,
        decimal discountAmount,
        decimal taxAmount,
        decimal lineTotal)
    {
        if (posSaleId == Guid.Empty || productId == Guid.Empty)
        {
            throw new ArgumentException("Sale and product are required.");
        }

        if (quantity <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }

        return new PosSaleLine(id, tenantId)
        {
            PosSaleId = posSaleId,
            ProductId = productId,
            SkuSnapshot = skuSnapshot.Trim(),
            NameSnapshot = nameSnapshot.Trim(),
            Quantity = Math.Round(quantity, QuantityPrecision.Scale, MidpointRounding.AwayFromZero),
            UnitPrice = Math.Round(unitPrice, MoneyPrecision.Scale, MidpointRounding.AwayFromZero),
            DiscountAmount = Math.Round(discountAmount, MoneyPrecision.Scale, MidpointRounding.AwayFromZero),
            TaxAmount = Math.Round(taxAmount, MoneyPrecision.Scale, MidpointRounding.AwayFromZero),
            LineTotal = Math.Round(lineTotal, MoneyPrecision.Scale, MidpointRounding.AwayFromZero),
        };
    }
}

public sealed class PosPayment : TenantOwnedEntity
{
    private PosPayment()
    {
    }

    private PosPayment(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid PosSaleId { get; private set; }

    public PosPaymentMethod Method { get; private set; }

    public decimal Amount { get; private set; }

    public string Status { get; private set; } = "Captured";

    public string? Reference { get; private set; }

    public static PosPayment Create(
        Guid id,
        Guid tenantId,
        Guid posSaleId,
        PosPaymentMethod method,
        decimal amount,
        string? reference)
    {
        if (posSaleId == Guid.Empty)
        {
            throw new ArgumentException("Sale is required.");
        }

        if (amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        return new PosPayment(id, tenantId)
        {
            PosSaleId = posSaleId,
            Method = method,
            Amount = Math.Round(amount, MoneyPrecision.Scale, MidpointRounding.AwayFromZero),
            Reference = string.IsNullOrWhiteSpace(reference) ? null : reference.Trim(),
        };
    }
}

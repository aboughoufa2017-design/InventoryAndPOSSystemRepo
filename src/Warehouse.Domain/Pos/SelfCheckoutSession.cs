using Warehouse.Domain.Common;

namespace Warehouse.Domain.Pos;

public enum SelfCheckoutSessionStatus
{
    Open = 0,
    Transferred = 1,
    Auditing = 2,
    ReadyForPayment = 3,
    Closed = 4,
    Expired = 5,
}

/// <summary>
/// Guest / customer Scan &amp; Go cart that can be handed to a cashier via QR code.
/// </summary>
public sealed class SelfCheckoutSession : TenantOwnedEntity
{
    private SelfCheckoutSession()
    {
    }

    private SelfCheckoutSession(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid WarehouseId { get; private set; }

    /// <summary>Short public code embedded in the transfer QR (e.g. MTSC:ABCD1234).</summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>Secret token for guest API calls (not shown in QR).</summary>
    public string AccessToken { get; private set; } = string.Empty;

    public bool IsGuest { get; private set; }

    public SelfCheckoutSessionStatus Status { get; private set; }

    public string CurrencyCode { get; private set; } = "LYD";

    public decimal Subtotal { get; private set; }

    public decimal TaxTotal { get; private set; }

    public decimal GrandTotal { get; private set; }

    public bool AuditRequired { get; private set; }

    public int AuditItemCount { get; private set; }

    public string? AuditTargetBarcodesJson { get; private set; }

    public bool AuditCompleted { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static SelfCheckoutSession Create(
        Guid id,
        Guid tenantId,
        Guid warehouseId,
        string code,
        string accessToken,
        bool isGuest,
        string currencyCode,
        DateTimeOffset utcNow,
        TimeSpan lifetime)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(currencyCode);

        return new SelfCheckoutSession(id, tenantId)
        {
            WarehouseId = warehouseId,
            Code = code.Trim().ToUpperInvariant(),
            AccessToken = accessToken.Trim(),
            IsGuest = isGuest,
            Status = SelfCheckoutSessionStatus.Open,
            CurrencyCode = currencyCode.Trim().ToUpperInvariant(),
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
            ExpiresAtUtc = utcNow.Add(lifetime),
        };
    }

    public void RecalculateTotals(decimal subtotal, decimal taxTotal, DateTimeOffset utcNow)
    {
        Subtotal = subtotal;
        TaxTotal = taxTotal;
        GrandTotal = subtotal + taxTotal;
        UpdatedAtUtc = utcNow;
    }

    public void MarkTransferred(bool auditRequired, int auditItemCount, string? auditTargetsJson, DateTimeOffset utcNow)
    {
        EnsureOpenOrTransferred();
        Status = SelfCheckoutSessionStatus.Transferred;
        AuditRequired = auditRequired;
        AuditItemCount = auditItemCount;
        AuditTargetBarcodesJson = auditTargetsJson;
        AuditCompleted = !auditRequired;
        if (!auditRequired)
        {
            Status = SelfCheckoutSessionStatus.ReadyForPayment;
        }

        UpdatedAtUtc = utcNow;
    }

    /// <summary>
    /// Guest returns to shopping after showing the transfer QR (before cashier audit/payment).
    /// </summary>
    public void ReopenForEditing(DateTimeOffset utcNow)
    {
        if (Status is not SelfCheckoutSessionStatus.Transferred
            and not SelfCheckoutSessionStatus.ReadyForPayment)
        {
            throw new InvalidOperationException(
                $"Cannot reopen session for editing in status {Status}.");
        }

        Status = SelfCheckoutSessionStatus.Open;
        AuditRequired = false;
        AuditItemCount = 0;
        AuditTargetBarcodesJson = null;
        AuditCompleted = false;
        UpdatedAtUtc = utcNow;
    }

    public void StartAudit(DateTimeOffset utcNow)
    {
        if (Status is not SelfCheckoutSessionStatus.Transferred and not SelfCheckoutSessionStatus.Auditing)
        {
            throw new InvalidOperationException("Session is not awaiting audit.");
        }

        Status = SelfCheckoutSessionStatus.Auditing;
        UpdatedAtUtc = utcNow;
    }

    public void CompleteAudit(DateTimeOffset utcNow)
    {
        AuditCompleted = true;
        Status = SelfCheckoutSessionStatus.ReadyForPayment;
        UpdatedAtUtc = utcNow;
    }

    public void Close(DateTimeOffset utcNow)
    {
        Status = SelfCheckoutSessionStatus.Closed;
        UpdatedAtUtc = utcNow;
    }

    public void EnsureUsable(DateTimeOffset utcNow)
    {
        if (utcNow > ExpiresAtUtc)
        {
            Status = SelfCheckoutSessionStatus.Expired;
            throw new InvalidOperationException("Self-checkout session expired.");
        }

        if (Status is SelfCheckoutSessionStatus.Closed or SelfCheckoutSessionStatus.Expired)
        {
            throw new InvalidOperationException("Self-checkout session is closed.");
        }
    }

    private void EnsureOpenOrTransferred()
    {
        if (Status is not SelfCheckoutSessionStatus.Open and not SelfCheckoutSessionStatus.Transferred)
        {
            throw new InvalidOperationException($"Cannot transfer session in status {Status}.");
        }
    }
}

public sealed class SelfCheckoutLine : TenantOwnedEntity
{
    private SelfCheckoutLine()
    {
    }

    private SelfCheckoutLine(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid SessionId { get; private set; }

    public Guid ProductId { get; private set; }

    public string SkuSnapshot { get; private set; } = string.Empty;

    public string NameSnapshot { get; private set; } = string.Empty;

    public string? BarcodeSnapshot { get; private set; }

    public decimal Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal LineTotal { get; private set; }

    public static SelfCheckoutLine Create(
        Guid id,
        Guid tenantId,
        Guid sessionId,
        Guid productId,
        string sku,
        string name,
        string? barcode,
        decimal quantity,
        decimal unitPrice)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }

        return new SelfCheckoutLine(id, tenantId)
        {
            SessionId = sessionId,
            ProductId = productId,
            SkuSnapshot = sku.Trim(),
            NameSnapshot = name.Trim(),
            BarcodeSnapshot = string.IsNullOrWhiteSpace(barcode) ? null : barcode.Trim(),
            Quantity = quantity,
            UnitPrice = unitPrice,
            LineTotal = decimal.Round(quantity * unitPrice, 2, MidpointRounding.AwayFromZero),
        };
    }

    public void SetQuantity(decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }

        Quantity = quantity;
        LineTotal = decimal.Round(quantity * UnitPrice, 2, MidpointRounding.AwayFromZero);
    }
}

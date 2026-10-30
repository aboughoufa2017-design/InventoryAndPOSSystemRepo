using Warehouse.Domain.Common;

namespace Warehouse.Domain.Tenancy;

/// <summary>
/// GLOBAL root entity for a company using the SaaS. Not tenant-owned.
/// Holds tenant configuration including currency, tax (disabled by default), and offline policy defaults.
/// </summary>
public sealed class Tenant : Entity
{
    private Tenant()
    {
    }

    private Tenant(Guid id)
        : base(id)
    {
    }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public bool IsActive { get; private set; } = true;

    public bool IsSuspended { get; private set; }

    /// <summary>
    /// ISO 4217 currency code. Tenant-configurable; never hard-coded as a single product currency.
    /// </summary>
    public string CurrencyCode { get; private set; } = "USD";

    public string TimeZoneId { get; private set; } = "UTC";

    public NegativeStockPolicy NegativeStockPolicy { get; private set; } = NegativeStockPolicy.Prevent;

    public bool TaxEnabled { get; private set; }

    public string? TaxName { get; private set; }

    public decimal? TaxRate { get; private set; }

    /// <summary>
    /// Tenant offline window in days. Default is <see cref="Security.OfflineSessionPolicy.DefaultMaxOfflineDays"/>.
    /// Null is allowed only when a tenant explicitly disables the time-based offline stop.
    /// </summary>
    public int? MaxOfflineDays { get; private set; }

    public bool RequireOnlineRevalidation { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public int RowVersion { get; private set; } = 1;

    public static Tenant Create(
        Guid id,
        string code,
        string name,
        string currencyCode,
        string timeZoneId,
        DateTimeOffset utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(currencyCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZoneId);

        if (currencyCode.Length is not 3)
        {
            throw new ArgumentException("CurrencyCode must be a 3-letter ISO 4217 code.", nameof(currencyCode));
        }

        return new Tenant(id)
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            CurrencyCode = currencyCode.Trim().ToUpperInvariant(),
            TimeZoneId = timeZoneId.Trim(),
            TaxEnabled = false,
            MaxOfflineDays = Security.OfflineSessionPolicy.DefaultMaxOfflineDays,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
        };
    }

    public void UpdateProfile(string name, string currencyCode, string timeZoneId, DateTimeOffset utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(currencyCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZoneId);

        if (currencyCode.Length is not 3)
        {
            throw new ArgumentException("CurrencyCode must be a 3-letter ISO 4217 code.", nameof(currencyCode));
        }

        Name = name.Trim();
        CurrencyCode = currencyCode.Trim().ToUpperInvariant();
        TimeZoneId = timeZoneId.Trim();
        Touch(utcNow);
    }

    public void SetNegativeStockPolicy(NegativeStockPolicy policy, DateTimeOffset utcNow)
    {
        NegativeStockPolicy = policy;
        Touch(utcNow);
    }

    public void SetActive(bool isActive, DateTimeOffset utcNow)
    {
        IsActive = isActive;
        Touch(utcNow);
    }

    public void Suspend(DateTimeOffset utcNow)
    {
        IsSuspended = true;
        Touch(utcNow);
    }

    public void Resume(DateTimeOffset utcNow)
    {
        IsSuspended = false;
        Touch(utcNow);
    }

    public void ConfigureOfflinePolicy(int? maxOfflineDays, bool requireOnlineRevalidation, DateTimeOffset utcNow)
    {
        if (maxOfflineDays is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxOfflineDays));
        }

        MaxOfflineDays = maxOfflineDays;
        RequireOnlineRevalidation = requireOnlineRevalidation;
        Touch(utcNow);
    }

    public void ConfigureTax(bool enabled, string? taxName, decimal? taxRate, DateTimeOffset utcNow)
    {
        if (enabled)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(taxName);
            if (taxRate is null or < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(taxRate));
            }
        }

        TaxEnabled = enabled;
        TaxName = enabled ? taxName!.Trim() : null;
        TaxRate = enabled ? taxRate : null;
        Touch(utcNow);
    }

    private void Touch(DateTimeOffset utcNow)
    {
        UpdatedAtUtc = utcNow;
        RowVersion++;
    }
}

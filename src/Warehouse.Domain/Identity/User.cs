using Warehouse.Domain.Common;

namespace Warehouse.Domain.Identity;

/// <summary>
/// TENANT-OWNED user. Passwords are stored only as a hash.
/// </summary>
public sealed class User : TenantOwnedEntity
{
    private User()
    {
    }

    private User(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Normalized phone digits (optional). Unique per tenant when set.
    /// </summary>
    public string? PhoneNumber { get; private set; }

    public string PasswordHash { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public bool IsActive { get; private set; } = true;

    public bool IsDisabled { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public int RowVersion { get; private set; } = 1;

    public static User Create(
        Guid id,
        Guid tenantId,
        string email,
        string passwordHash,
        string displayName,
        DateTimeOffset utcNow,
        string? phoneNumber = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        return new User(id, tenantId)
        {
            Email = email.Trim().ToLowerInvariant(),
            PhoneNumber = NormalizePhone(phoneNumber),
            PasswordHash = passwordHash,
            DisplayName = displayName.Trim(),
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
        };
    }

    public void SetPhoneNumber(string? phoneNumber, DateTimeOffset utcNow)
    {
        PhoneNumber = NormalizePhone(phoneNumber);
        UpdatedAtUtc = utcNow;
        RowVersion++;
    }

    public static string? NormalizePhone(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return null;
        }

        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
        return digits.Length == 0 ? null : digits;
    }

    public void Disable(DateTimeOffset utcNow)
    {
        IsDisabled = true;
        IsActive = false;
        UpdatedAtUtc = utcNow;
        RowVersion++;
    }

    public void Enable(DateTimeOffset utcNow)
    {
        IsDisabled = false;
        IsActive = true;
        UpdatedAtUtc = utcNow;
        RowVersion++;
    }

    public void UpdateDisplayName(string displayName, DateTimeOffset utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        DisplayName = displayName.Trim();
        UpdatedAtUtc = utcNow;
        RowVersion++;
    }

    public void SetPasswordHash(string passwordHash, DateTimeOffset utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        PasswordHash = passwordHash;
        UpdatedAtUtc = utcNow;
        RowVersion++;
    }
}

using Warehouse.Domain.Common;

namespace Warehouse.Domain.Platform;

/// <summary>
/// GLOBAL SaaS operator account. Separate from tenant Users. Never receives tenant roles.
/// </summary>
public sealed class PlatformOperator : Entity
{
    private PlatformOperator()
    {
    }

    private PlatformOperator(Guid id)
        : base(id)
    {
    }

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public bool IsActive { get; private set; } = true;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static PlatformOperator Create(
        Guid id,
        string email,
        string passwordHash,
        string displayName,
        DateTimeOffset utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        return new PlatformOperator(id)
        {
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            DisplayName = displayName.Trim(),
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
        };
    }

    public void Disable(DateTimeOffset utcNow)
    {
        IsActive = false;
        Touch(utcNow);
    }

    public void Enable(DateTimeOffset utcNow)
    {
        IsActive = true;
        Touch(utcNow);
    }

    private void Touch(DateTimeOffset utcNow) => UpdatedAtUtc = utcNow;
}

using Warehouse.Domain.Common;

namespace Warehouse.Domain.Identity;

/// <summary>
/// TENANT-OWNED refresh token. Only the SHA-256 hash is persisted — never the raw token.
/// </summary>
public sealed class RefreshToken : TenantOwnedEntity
{
    private RefreshToken()
    {
    }

    private RefreshToken(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid UserId { get; private set; }

    public Guid? DeviceId { get; private set; }

    /// <summary>
    /// Rotation family. Reuse of a rotated token revokes the entire family.
    /// </summary>
    public Guid FamilyId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public Guid? ReplacedByTokenId { get; private set; }

    public bool IsRevoked => RevokedAtUtc is not null;

    public bool IsActive(DateTimeOffset utcNow) =>
        !IsRevoked && ExpiresAtUtc > utcNow;

    public static RefreshToken Create(
        Guid id,
        Guid tenantId,
        Guid userId,
        Guid? deviceId,
        Guid familyId,
        string tokenHash,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id must be a non-empty UUID.", nameof(userId));
        }

        if (familyId == Guid.Empty)
        {
            throw new ArgumentException("Family id must be a non-empty UUID.", nameof(familyId));
        }

        if (expiresAtUtc <= createdAtUtc)
        {
            throw new ArgumentException("ExpiresAtUtc must be after CreatedAtUtc.", nameof(expiresAtUtc));
        }

        return new RefreshToken(id, tenantId)
        {
            UserId = userId,
            DeviceId = deviceId,
            FamilyId = familyId,
            TokenHash = tokenHash.Trim().ToLowerInvariant(),
            CreatedAtUtc = createdAtUtc,
            ExpiresAtUtc = expiresAtUtc,
        };
    }

    public void Revoke(DateTimeOffset utcNow, Guid? replacedByTokenId = null)
    {
        if (IsRevoked)
        {
            return;
        }

        RevokedAtUtc = utcNow;
        ReplacedByTokenId = replacedByTokenId;
    }
}

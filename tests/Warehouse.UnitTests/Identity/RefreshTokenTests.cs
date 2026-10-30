using Warehouse.Domain.Identity;

namespace Warehouse.UnitTests.Identity;

public class RefreshTokenTests
{
    [Fact]
    public void Create_stores_hash_and_is_active_until_expiry()
    {
        var now = DateTimeOffset.UtcNow;
        var token = RefreshToken.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            deviceId: null,
            Guid.NewGuid(),
            "abc123hash",
            now,
            now.AddDays(14));

        Assert.Equal("abc123hash", token.TokenHash);
        Assert.True(token.IsActive(now.AddDays(1)));
        Assert.False(token.IsRevoked);
    }

    [Fact]
    public void Revoke_marks_token_inactive_and_links_replacement()
    {
        var now = DateTimeOffset.UtcNow;
        var token = RefreshToken.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            Guid.NewGuid(),
            "hash",
            now,
            now.AddDays(14));

        var replacementId = Guid.NewGuid();
        token.Revoke(now, replacementId);

        Assert.True(token.IsRevoked);
        Assert.False(token.IsActive(now));
        Assert.Equal(replacementId, token.ReplacedByTokenId);
    }
}

public class SystemRolesTests
{
    [Fact]
    public void Templates_include_owner_with_all_permissions_and_is_system_intended()
    {
        Assert.Contains(SystemRoles.Templates, t => t.Code == SystemRoles.Owner);
        var owner = SystemRoles.Templates.Single(t => t.Code == SystemRoles.Owner);
        Assert.Equal(SystemPermissions.Catalog.Count, owner.PermissionCodes.Count);
        Assert.Contains(SystemRoles.Templates, t => t.Code == SystemRoles.Cashier);
        Assert.Equal(9, SystemRoles.Templates.Count);
    }
}

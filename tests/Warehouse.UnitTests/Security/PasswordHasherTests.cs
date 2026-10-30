using Warehouse.Infrastructure.Auth;

namespace Warehouse.UnitTests.Security;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_and_verify_round_trip()
    {
        var hasher = new AspNetPasswordHasher();
        var hash = hasher.HashPassword("S3cure-Pass!");
        Assert.True(hasher.VerifyHashedPassword(hash, "S3cure-Pass!"));
        Assert.False(hasher.VerifyHashedPassword(hash, "wrong"));
    }
}

public class RefreshTokenHasherTests
{
    [Fact]
    public void Hash_is_deterministic_sha256_hex()
    {
        var a = RefreshTokenHasher.Hash("raw-token");
        var b = RefreshTokenHasher.Hash("raw-token");
        Assert.Equal(a, b);
        Assert.Equal(64, a.Length);
        Assert.NotEqual(RefreshTokenHasher.Hash("other"), a);
    }
}

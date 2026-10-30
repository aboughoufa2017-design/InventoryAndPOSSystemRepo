using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Warehouse.Application.Security;
using Warehouse.Domain.Identity;

namespace Warehouse.Infrastructure.Auth;

public sealed class AspNetPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return _hasher.HashPassword(user: null!, password);
    }

    public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hashedPassword);
        ArgumentException.ThrowIfNullOrWhiteSpace(providedPassword);

        var result = _hasher.VerifyHashedPassword(user: null!, hashedPassword, providedPassword);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}

public static class RefreshTokenHasher
{
    public static string Hash(string rawToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawToken);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static string GenerateRawToken()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Warehouse.Infrastructure.Auth;

public interface IJwtTokenService
{
    (string Token, DateTimeOffset ExpiresAtUtc) CreateAccessToken(
        Guid userId,
        Guid tenantId,
        string email,
        string displayName,
        Guid? deviceId,
        IEnumerable<string> permissions,
        IEnumerable<string> roles,
        bool isPlatformOperator);

    (string Token, DateTimeOffset ExpiresAtUtc) CreatePlatformAccessToken(
        Guid operatorId,
        string email,
        string displayName);
}

public sealed class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
{
    private readonly JwtOptions _options = options.Value;

    public (string Token, DateTimeOffset ExpiresAtUtc) CreateAccessToken(
        Guid userId,
        Guid tenantId,
        string email,
        string displayName,
        Guid? deviceId,
        IEnumerable<string> permissions,
        IEnumerable<string> roles,
        bool isPlatformOperator)
    {
        EnsureSigningKeyConfigured();

        var now = DateTimeOffset.UtcNow;
        var expires = now.AddMinutes(_options.AccessTokenMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.UniqueName, displayName),
            new(AuthClaimTypes.TenantId, tenantId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        if (deviceId is Guid did)
        {
            claims.Add(new Claim(AuthClaimTypes.DeviceId, did.ToString()));
        }

        if (isPlatformOperator)
        {
            claims.Add(new Claim(AuthClaimTypes.IsPlatformOperator, "true"));
        }

        AddPermissionAndRoleClaims(claims, permissions, roles);
        return WriteToken(claims, now, expires);
    }

    public (string Token, DateTimeOffset ExpiresAtUtc) CreatePlatformAccessToken(
        Guid operatorId,
        string email,
        string displayName)
    {
        EnsureSigningKeyConfigured();

        var now = DateTimeOffset.UtcNow;
        var expires = now.AddMinutes(_options.AccessTokenMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, operatorId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.UniqueName, displayName),
            new(AuthClaimTypes.IsPlatformOperator, "true"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        return WriteToken(claims, now, expires);
    }

    private static void AddPermissionAndRoleClaims(
        List<Claim> claims,
        IEnumerable<string> permissions,
        IEnumerable<string> roles)
    {
        foreach (var permission in permissions.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            claims.Add(new Claim(AuthClaimTypes.Permission, permission));
        }

        foreach (var role in roles.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            claims.Add(new Claim(AuthClaimTypes.Role, role));
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
    }

    private (string Token, DateTimeOffset ExpiresAtUtc) WriteToken(
        List<Claim> claims,
        DateTimeOffset now,
        DateTimeOffset expires)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    private void EnsureSigningKeyConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.SigningKey) || _options.SigningKey.Length < 32)
        {
            throw new InvalidOperationException(
                "Jwt:SigningKey must be configured and at least 32 characters. Set Jwt__SigningKey or JWT_SIGNING_KEY.");
        }
    }
}

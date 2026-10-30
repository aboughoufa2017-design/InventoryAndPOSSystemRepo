namespace Warehouse.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "warehouse";

    public string Audience { get; set; } = "warehouse";

    /// <summary>
    /// Symmetric signing key. Prefer environment variable Jwt__SigningKey or JWT_SIGNING_KEY.
    /// </summary>
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>
    /// Access JWT lifetime in minutes. Override with Jwt__AccessTokenMinutes or JWT_ACCESS_TOKEN_MINUTES.
    /// </summary>
    public int AccessTokenMinutes { get; set; } = 180;

    /// <summary>
    /// Sliding refresh-token lifetime in days. Override with Jwt__RefreshTokenDays or JWT_REFRESH_TOKEN_DAYS.
    /// </summary>
    public int RefreshTokenDays { get; set; } = 14;

    public int RefreshTokenAbsoluteDays { get; set; } = 90;
}

public static class AuthClaimTypes
{
    public const string TenantId = "tenant_id";
    public const string DeviceId = "device_id";
    public const string Permission = "permission";
    public const string Role = "role";
    public const string IsPlatformOperator = "platform_operator";
}

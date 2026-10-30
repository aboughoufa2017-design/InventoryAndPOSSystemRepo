namespace Warehouse.Domain.Security;

/// <summary>
/// Configurable offline session and device security policy.
/// Default MaxOfflineDays is <see cref="DefaultMaxOfflineDays"/> (tenant-configurable; not a hard-coded business rule).
/// DeviceRevoked, UserDisabled, and TenantSuspended take effect when the device next connects.
/// </summary>
public sealed class OfflineSessionPolicy
{
    /// <summary>
    /// Default tenant offline window in days. Tenants may configure a different value.
    /// Do not scatter this literal in business handlers; read it from tenant configuration.
    /// </summary>
    public const int DefaultMaxOfflineDays = 7;

    public OfflineSessionPolicy(
        int? maxOfflineDays,
        DateTimeOffset? lastSuccessfulOnlineAuthentication,
        bool requireOnlineRevalidation,
        bool deviceRevoked,
        bool userDisabled,
        bool tenantSuspended)
    {
        if (maxOfflineDays is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxOfflineDays), "MaxOfflineDays cannot be negative.");
        }

        MaxOfflineDays = maxOfflineDays;
        LastSuccessfulOnlineAuthentication = lastSuccessfulOnlineAuthentication;
        RequireOnlineRevalidation = requireOnlineRevalidation;
        DeviceRevoked = deviceRevoked;
        UserDisabled = userDisabled;
        TenantSuspended = tenantSuspended;
    }

    public int? MaxOfflineDays { get; }

    public DateTimeOffset? LastSuccessfulOnlineAuthentication { get; }

    public bool RequireOnlineRevalidation { get; }

    public bool DeviceRevoked { get; }

    public bool UserDisabled { get; }

    public bool TenantSuspended { get; }

    public bool BlocksServerApply => DeviceRevoked || UserDisabled || TenantSuspended;

    public static OfflineSessionPolicy CreateDefault() =>
        new(
            maxOfflineDays: DefaultMaxOfflineDays,
            lastSuccessfulOnlineAuthentication: null,
            requireOnlineRevalidation: false,
            deviceRevoked: false,
            userDisabled: false,
            tenantSuspended: false);

    /// <summary>
    /// Whether the configured max-offline-days limit requires online revalidation at the given UTC time.
    /// When MaxOfflineDays is unset, this returns false.
    /// </summary>
    public bool IsMaxOfflineDaysExceeded(DateTimeOffset utcNow)
    {
        if (MaxOfflineDays is null)
        {
            return false;
        }

        if (LastSuccessfulOnlineAuthentication is null)
        {
            return true;
        }

        var limit = LastSuccessfulOnlineAuthentication.Value.AddDays(MaxOfflineDays.Value);
        return utcNow >= limit;
    }

    public bool MustStopCreatingLocalOperations(DateTimeOffset utcNow) =>
        RequireOnlineRevalidation
        || BlocksServerApply
        || IsMaxOfflineDaysExceeded(utcNow);
}

/// <summary>
/// Alias retained so documentation and future modules can refer to DeviceSecurityPolicy
/// as the same conceptual model as OfflineSessionPolicy.
/// </summary>
public sealed class DeviceSecurityPolicy
{
    public DeviceSecurityPolicy(OfflineSessionPolicy sessionPolicy)
    {
        SessionPolicy = sessionPolicy ?? throw new ArgumentNullException(nameof(sessionPolicy));
    }

    public OfflineSessionPolicy SessionPolicy { get; }

    public int? MaxOfflineDays => SessionPolicy.MaxOfflineDays;

    public DateTimeOffset? LastSuccessfulOnlineAuthentication => SessionPolicy.LastSuccessfulOnlineAuthentication;

    public bool RequireOnlineRevalidation => SessionPolicy.RequireOnlineRevalidation;

    public bool DeviceRevoked => SessionPolicy.DeviceRevoked;

    public bool UserDisabled => SessionPolicy.UserDisabled;

    public bool TenantSuspended => SessionPolicy.TenantSuspended;

    public static DeviceSecurityPolicy CreateDefault() => new(OfflineSessionPolicy.CreateDefault());
}

using Warehouse.Domain.Security;

namespace Warehouse.UnitTests.Security;

public class OfflineSessionPolicyTests
{
    [Fact]
    public void Default_uses_seven_day_configurable_window()
    {
        var policy = OfflineSessionPolicy.CreateDefault();
        Assert.Equal(7, policy.MaxOfflineDays);
        Assert.Equal(OfflineSessionPolicy.DefaultMaxOfflineDays, policy.MaxOfflineDays);
    }

    [Fact]
    public void Default_blocks_after_seven_days_without_online_auth()
    {
        var lastAuth = DateTimeOffset.Parse("2026-01-01T00:00:00Z");
        var policy = new OfflineSessionPolicy(
            maxOfflineDays: OfflineSessionPolicy.DefaultMaxOfflineDays,
            lastSuccessfulOnlineAuthentication: lastAuth,
            requireOnlineRevalidation: false,
            deviceRevoked: false,
            userDisabled: false,
            tenantSuspended: false);

        Assert.False(policy.IsMaxOfflineDaysExceeded(lastAuth.AddDays(6)));
        Assert.True(policy.IsMaxOfflineDaysExceeded(lastAuth.AddDays(7)));
        Assert.True(policy.MustStopCreatingLocalOperations(lastAuth.AddDays(7)));
    }

    [Fact]
    public void Configurable_max_offline_days_can_differ_from_default()
    {
        var lastAuth = DateTimeOffset.Parse("2026-01-01T00:00:00Z");
        var policy = new OfflineSessionPolicy(
            maxOfflineDays: 10,
            lastSuccessfulOnlineAuthentication: lastAuth,
            requireOnlineRevalidation: false,
            deviceRevoked: false,
            userDisabled: false,
            tenantSuspended: false);

        Assert.False(policy.IsMaxOfflineDaysExceeded(lastAuth.AddDays(9)));
        Assert.True(policy.IsMaxOfflineDaysExceeded(lastAuth.AddDays(10)));
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    public void Security_blocks_stop_server_apply(bool deviceRevoked, bool userDisabled, bool tenantSuspended)
    {
        var policy = new OfflineSessionPolicy(
            maxOfflineDays: OfflineSessionPolicy.DefaultMaxOfflineDays,
            lastSuccessfulOnlineAuthentication: DateTimeOffset.UtcNow,
            requireOnlineRevalidation: false,
            deviceRevoked: deviceRevoked,
            userDisabled: userDisabled,
            tenantSuspended: tenantSuspended);

        Assert.True(policy.BlocksServerApply);
        Assert.True(policy.MustStopCreatingLocalOperations(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Rejects_negative_max_offline_days()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new OfflineSessionPolicy(
                maxOfflineDays: -1,
                lastSuccessfulOnlineAuthentication: null,
                requireOnlineRevalidation: false,
                deviceRevoked: false,
                userDisabled: false,
                tenantSuspended: false));
    }
}

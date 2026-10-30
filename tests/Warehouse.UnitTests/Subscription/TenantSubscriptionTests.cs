using Warehouse.Domain.Subscription;

namespace Warehouse.UnitTests.Subscription;

public sealed class TenantSubscriptionTests
{
    [Fact]
    public void Trial_and_active_and_past_due_grant_entitlements_cancelled_does_not()
    {
        var now = DateTimeOffset.UtcNow;
        var trial = TenantSubscription.StartTrial(Guid.NewGuid(), Guid.NewGuid(), SystemPlans.TrialPlanId, now, TimeSpan.FromDays(14));
        Assert.True(trial.GrantsEntitlements);
        Assert.Equal(SubscriptionStatus.Trial, trial.Status);

        trial.SetStatus(SubscriptionStatus.Active, now);
        Assert.True(trial.GrantsEntitlements);

        trial.SetStatus(SubscriptionStatus.PastDue, now);
        Assert.True(trial.GrantsEntitlements);

        trial.SetStatus(SubscriptionStatus.Cancelled, now);
        Assert.False(trial.GrantsEntitlements);
    }

    [Fact]
    public void Plan_has_feature_is_case_insensitive()
    {
        var plan = Plan.Create(
            Guid.NewGuid(),
            "TEST",
            "Test",
            ["Catalog", "inventory"],
            10,
            2,
            5,
            100,
            DateTimeOffset.UtcNow);

        Assert.True(plan.HasFeature("catalog"));
        Assert.True(plan.HasFeature("INVENTORY"));
        Assert.False(plan.HasFeature("sales"));
    }
}

using Warehouse.Domain.Common;

namespace Warehouse.Domain.Subscription;

/// <summary>
/// TENANT-OWNED subscription row (one per tenant). ExternalReference is for a future billing provider.
/// </summary>
public sealed class TenantSubscription : TenantOwnedEntity
{
    private TenantSubscription()
    {
    }

    private TenantSubscription(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid PlanId { get; private set; }

    public SubscriptionStatus Status { get; private set; }

    public DateTimeOffset? TrialEndsAtUtc { get; private set; }

    public DateTimeOffset? CurrentPeriodStartUtc { get; private set; }

    public DateTimeOffset? CurrentPeriodEndUtc { get; private set; }

    /// <summary>
    /// Opaque billing-provider reference (Stripe subscription id, etc.). Unused until a provider is integrated.
    /// </summary>
    public string? ExternalReference { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public int RowVersion { get; private set; } = 1;

    public static TenantSubscription StartTrial(
        Guid id,
        Guid tenantId,
        Guid planId,
        DateTimeOffset utcNow,
        TimeSpan trialLength)
    {
        if (trialLength <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(trialLength));
        }

        return new TenantSubscription(id, tenantId)
        {
            PlanId = planId,
            Status = SubscriptionStatus.Trial,
            TrialEndsAtUtc = utcNow.Add(trialLength),
            CurrentPeriodStartUtc = utcNow,
            CurrentPeriodEndUtc = utcNow.Add(trialLength),
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
        };
    }

    public static TenantSubscription Create(
        Guid id,
        Guid tenantId,
        Guid planId,
        SubscriptionStatus status,
        DateTimeOffset? trialEndsAtUtc,
        DateTimeOffset? periodStartUtc,
        DateTimeOffset? periodEndUtc,
        string? externalReference,
        DateTimeOffset utcNow)
    {
        return new TenantSubscription(id, tenantId)
        {
            PlanId = planId,
            Status = status,
            TrialEndsAtUtc = trialEndsAtUtc,
            CurrentPeriodStartUtc = periodStartUtc,
            CurrentPeriodEndUtc = periodEndUtc,
            ExternalReference = string.IsNullOrWhiteSpace(externalReference) ? null : externalReference.Trim(),
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
        };
    }

    public void AssignPlan(Guid planId, DateTimeOffset utcNow)
    {
        if (planId == Guid.Empty)
        {
            throw new ArgumentException("Plan id must be a non-empty UUID.", nameof(planId));
        }

        PlanId = planId;
        Touch(utcNow);
    }

    public void SetStatus(SubscriptionStatus status, DateTimeOffset utcNow)
    {
        Status = status;
        Touch(utcNow);
    }

    public void SetPeriod(DateTimeOffset? startUtc, DateTimeOffset? endUtc, DateTimeOffset utcNow)
    {
        CurrentPeriodStartUtc = startUtc;
        CurrentPeriodEndUtc = endUtc;
        Touch(utcNow);
    }

    public void SetExternalReference(string? externalReference, DateTimeOffset utcNow)
    {
        ExternalReference = string.IsNullOrWhiteSpace(externalReference) ? null : externalReference.Trim();
        Touch(utcNow);
    }

    public void SetTrialEndsAt(DateTimeOffset? trialEndsAtUtc, DateTimeOffset utcNow)
    {
        TrialEndsAtUtc = trialEndsAtUtc;
        Touch(utcNow);
    }

    /// <summary>
    /// Entitlements are usable while Trial or Active (and PastDue still allows grace access).
    /// Cancelled subscriptions deny features.
    /// </summary>
    public bool GrantsEntitlements =>
        Status is SubscriptionStatus.Trial or SubscriptionStatus.Active or SubscriptionStatus.PastDue;

    private void Touch(DateTimeOffset utcNow)
    {
        UpdatedAtUtc = utcNow;
        RowVersion++;
    }
}

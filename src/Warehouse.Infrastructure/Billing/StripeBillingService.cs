using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Billing;
using Warehouse.Application.Tenancy;
using Warehouse.Domain.Subscription;
using Warehouse.Infrastructure.Persistence;
using StripeOptions = Warehouse.Application.Billing.StripeOptions;

namespace Warehouse.Infrastructure.Billing;

public sealed class StripeBillingService(
    WarehouseDbContext db,
    ITenantContext tenantContext,
    IOptions<StripeOptions> options) : IBillingService
{
    public async Task<AppResult<CheckoutSessionResponse>> CreateCheckoutSessionAsync(
        string planCode,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<CheckoutSessionResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        var opts = options.Value;
        if (string.IsNullOrWhiteSpace(opts.SecretKey))
        {
            return AppResult<CheckoutSessionResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "Stripe is not configured (Stripe:SecretKey).");
        }

        if (string.IsNullOrWhiteSpace(planCode)
            || !opts.PriceIds.TryGetValue(planCode.Trim(), out var priceId)
            || string.IsNullOrWhiteSpace(priceId))
        {
            return AppResult<CheckoutSessionResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "Unknown plan code or missing Stripe PriceIds mapping.");
        }

        var plan = await db.Plans.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Code == planCode.Trim().ToUpperInvariant() && p.IsActive, cancellationToken);
        if (plan is null)
        {
            return AppResult<CheckoutSessionResponse>.Failure(ErrorCodes.ValidationFailed, "Plan was not found.");
        }

        StripeConfiguration.ApiKey = opts.SecretKey;
        var sessionService = new SessionService();
        var session = await sessionService.CreateAsync(
            new SessionCreateOptions
            {
                Mode = "subscription",
                SuccessUrl = opts.SuccessUrl,
                CancelUrl = opts.CancelUrl,
                LineItems =
                [
                    new SessionLineItemOptions { Price = priceId, Quantity = 1 },
                ],
                ClientReferenceId = tenantId.ToString("D"),
                Metadata = new Dictionary<string, string>
                {
                    ["tenant_id"] = tenantId.ToString("D"),
                    ["plan_code"] = plan.Code,
                    ["plan_id"] = plan.Id.ToString("D"),
                },
                SubscriptionData = new SessionSubscriptionDataOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        ["tenant_id"] = tenantId.ToString("D"),
                        ["plan_id"] = plan.Id.ToString("D"),
                    },
                },
            },
            cancellationToken: cancellationToken);

        return AppResult<CheckoutSessionResponse>.Success(
            new CheckoutSessionResponse(session.Id, session.Url));
    }

    public async Task<AppResult<BillingPortalResponse>> CreatePortalSessionAsync(CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<BillingPortalResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        var opts = options.Value;
        if (string.IsNullOrWhiteSpace(opts.SecretKey))
        {
            return AppResult<BillingPortalResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "Stripe is not configured (Stripe:SecretKey).");
        }

        var sub = await db.TenantSubscriptions.AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);
        if (sub?.ExternalReference is null)
        {
            return AppResult<BillingPortalResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "No Stripe subscription reference on this tenant.");
        }

        StripeConfiguration.ApiKey = opts.SecretKey;
        var stripeSub = await new SubscriptionService().GetAsync(
            sub.ExternalReference,
            cancellationToken: cancellationToken);

        var portal = await new Stripe.BillingPortal.SessionService().CreateAsync(
            new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = stripeSub.CustomerId,
                ReturnUrl = opts.SuccessUrl,
            },
            cancellationToken: cancellationToken);

        return AppResult<BillingPortalResponse>.Success(new BillingPortalResponse(portal.Url));
    }

    public async Task<AppResult> HandleWebhookAsync(
        string json,
        string signatureHeader,
        CancellationToken cancellationToken)
    {
        var opts = options.Value;
        if (string.IsNullOrWhiteSpace(opts.WebhookSecret))
        {
            return AppResult.Failure(ErrorCodes.ValidationFailed, "Stripe:WebhookSecret is not configured.");
        }

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, opts.WebhookSecret);
        }
        catch (Exception ex)
        {
            return AppResult.Failure(ErrorCodes.ValidationFailed, $"Invalid Stripe signature: {ex.Message}");
        }

        var utcNow = DateTimeOffset.UtcNow;

        switch (stripeEvent.Type)
        {
            case EventTypes.CheckoutSessionCompleted:
            {
                if (stripeEvent.Data.Object is not Session session)
                {
                    break;
                }

                if (!session.Metadata.TryGetValue("tenant_id", out var tenantIdRaw)
                    || !Guid.TryParse(tenantIdRaw, out var tenantId)
                    || !session.Metadata.TryGetValue("plan_id", out var planIdRaw)
                    || !Guid.TryParse(planIdRaw, out var planId))
                {
                    break;
                }

                // Webhooks are platform-scoped — temporarily clear tenant filter by using ignore query filters.
                var sub = await db.TenantSubscriptions.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);
                if (sub is null)
                {
                    break;
                }

                sub.AssignPlan(planId, utcNow);
                sub.SetStatus(SubscriptionStatus.Active, utcNow);
                if (!string.IsNullOrWhiteSpace(session.SubscriptionId))
                {
                    sub.SetExternalReference(session.SubscriptionId, utcNow);
                }

                await db.SaveChangesAsync(cancellationToken);
                break;
            }
            case EventTypes.CustomerSubscriptionDeleted:
            case EventTypes.CustomerSubscriptionUpdated:
            {
                if (stripeEvent.Data.Object is not Subscription subscription)
                {
                    break;
                }

                var sub = await db.TenantSubscriptions.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(s => s.ExternalReference == subscription.Id, cancellationToken);
                if (sub is null)
                {
                    break;
                }

                var status = subscription.Status switch
                {
                    "active" or "trialing" => SubscriptionStatus.Active,
                    "past_due" or "unpaid" => SubscriptionStatus.PastDue,
                    "canceled" or "incomplete_expired" => SubscriptionStatus.Cancelled,
                    _ => sub.Status,
                };
                sub.SetStatus(status, utcNow);
                await db.SaveChangesAsync(cancellationToken);
                break;
            }
        }

        return AppResult.Success();
    }
}

namespace Warehouse.Application.Billing;

public sealed record StripeOptions
{
    public const string SectionName = "Stripe";

    public string SecretKey { get; set; } = string.Empty;

    public string WebhookSecret { get; set; } = string.Empty;

    public string PublishableKey { get; set; } = string.Empty;

    public string SuccessUrl { get; set; } = "http://localhost:5173/billing?success=1";

    public string CancelUrl { get; set; } = "http://localhost:5173/billing?cancelled=1";

    /// <summary>Map plan code → Stripe Price id.</summary>
    public Dictionary<string, string> PriceIds { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed record CheckoutSessionResponse(string SessionId, string Url);

public sealed record BillingPortalResponse(string Url);

public interface IBillingService
{
    Task<Abstractions.AppResult<CheckoutSessionResponse>> CreateCheckoutSessionAsync(
        string planCode,
        CancellationToken cancellationToken);

    Task<Abstractions.AppResult<BillingPortalResponse>> CreatePortalSessionAsync(CancellationToken cancellationToken);

    Task<Abstractions.AppResult> HandleWebhookAsync(string json, string signatureHeader, CancellationToken cancellationToken);
}

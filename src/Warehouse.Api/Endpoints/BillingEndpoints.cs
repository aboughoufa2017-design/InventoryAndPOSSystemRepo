using Warehouse.Api.Authorization;
using Warehouse.Api.Http;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Billing;
using Warehouse.Shared.Api;

namespace Warehouse.Api.Endpoints;

public static class BillingEndpoints
{
    public static IEndpointRouteBuilder MapBillingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/billing").WithTags("Billing").RequireAuthorization();
        group.MapPost("/checkout", CheckoutAsync)
            .RequirePermission("users.manage")
            .WithName("CreateBillingCheckout");
        group.MapPost("/portal", PortalAsync)
            .RequirePermission("users.manage")
            .WithName("CreateBillingPortal");

        endpoints.MapPost("/api/v1/billing/webhooks/stripe", WebhookAsync)
            .AllowAnonymous()
            .WithTags("Billing")
            .WithName("StripeWebhook");

        return endpoints;
    }

    private static async Task<IResult> CheckoutAsync(
        CheckoutRequest request,
        IBillingService billing,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(await billing.CreateCheckoutSessionAsync(request.PlanCode, ct), http);

    private static async Task<IResult> PortalAsync(
        IBillingService billing,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(await billing.CreatePortalSessionAsync(ct), http);

    private static async Task<IResult> WebhookAsync(
        HttpRequest request,
        IBillingService billing,
        HttpContext http,
        CancellationToken ct)
    {
        using var reader = new StreamReader(request.Body);
        var json = await reader.ReadToEndAsync(ct);
        var signature = request.Headers["Stripe-Signature"].ToString();
        var result = await billing.HandleWebhookAsync(json, signature, ct);
        if (!result.IsSuccess)
        {
            return Results.Json(
                new ApiErrorResponse(result.ErrorCode!, result.ErrorMessage ?? "Webhook failed.", CorrelationId(http)),
                statusCode: StatusCodes.Status400BadRequest);
        }

        return Results.Ok();
    }

    private static IResult ToHttp<T>(AppResult<T> result, HttpContext http)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        var status = result.ErrorCode == ApiErrorCodes.Unauthorized
            ? StatusCodes.Status401Unauthorized
            : StatusCodes.Status400BadRequest;
        return Results.Json(
            new ApiErrorResponse(result.ErrorCode!, result.ErrorMessage ?? "Request failed.", CorrelationId(http)),
            statusCode: status);
    }

    private static string CorrelationId(HttpContext http) =>
        http.Items[CorrelationIdMiddleware.ItemKey] as string ?? "unknown";

    public sealed record CheckoutRequest(string PlanCode);
}

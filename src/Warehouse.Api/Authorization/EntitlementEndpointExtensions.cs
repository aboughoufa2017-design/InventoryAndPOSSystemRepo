using Warehouse.Application.Platform;
using Warehouse.Domain.Subscription;
using Warehouse.Shared.Api;

namespace Warehouse.Api.Authorization;

public static class EntitlementEndpointExtensions
{
    public static RouteGroupBuilder RequireFeature(this RouteGroupBuilder group, string featureCode)
    {
        group.AddEndpointFilter(new EntitlementFeatureFilter(featureCode));
        return group;
    }

    public static RouteGroupBuilder RequireCatalog(this RouteGroupBuilder g) =>
        g.RequireFeature(SubscriptionFeatures.Catalog);

    public static RouteGroupBuilder RequireInventory(this RouteGroupBuilder g) =>
        g.RequireFeature(SubscriptionFeatures.Inventory);

    public static RouteGroupBuilder RequirePurchasing(this RouteGroupBuilder g) =>
        g.RequireFeature(SubscriptionFeatures.Purchasing);

    public static RouteGroupBuilder RequireSales(this RouteGroupBuilder g) =>
        g.RequireFeature(SubscriptionFeatures.Sales);

    public static RouteGroupBuilder RequireTransfers(this RouteGroupBuilder g) =>
        g.RequireFeature(SubscriptionFeatures.Transfers);

    public static RouteGroupBuilder RequireSync(this RouteGroupBuilder g) =>
        g.RequireFeature(SubscriptionFeatures.Sync);

    public static RouteGroupBuilder RequireReports(this RouteGroupBuilder g) =>
        g.RequireFeature(SubscriptionFeatures.Reports);
}

file sealed class EntitlementFeatureFilter(string featureCode) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var entitlements = context.HttpContext.RequestServices.GetRequiredService<IEntitlementService>();
        if (!await entitlements.IsFeatureEnabledAsync(featureCode, context.HttpContext.RequestAborted))
        {
            var correlationId = context.HttpContext.Items[Http.CorrelationIdMiddleware.ItemKey] as string
                ?? context.HttpContext.TraceIdentifier;
            return Results.Json(
                new ApiErrorResponse(
                    ApiErrorCodes.FeatureNotEntitled,
                    $"Feature '{featureCode}' is not enabled for this tenant subscription.",
                    correlationId),
                statusCode: StatusCodes.Status403Forbidden);
        }

        return await next(context);
    }
}

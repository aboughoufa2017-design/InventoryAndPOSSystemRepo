using Warehouse.Application.Abstractions;
using Warehouse.Application.Platform;
using Warehouse.Api.Authorization;
using Warehouse.Domain.Subscription;
using Warehouse.Shared.Api;

namespace Warehouse.Api.Endpoints;

public static class PlatformEndpoints
{
    public static IEndpointRouteBuilder MapPlatformEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var auth = endpoints.MapGroup("/api/v1/platform/auth").WithTags("Platform");
        auth.MapPost("/login", LoginAsync)
            .AllowAnonymous()
            .RequireRateLimiting(Hosting.ProductionHardeningExtensions.AuthRateLimitPolicy)
            .WithName("PlatformLogin")
            .Produces<PlatformAuthResponse>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);

        var group = endpoints.MapGroup("/api/v1/platform")
            .WithTags("Platform")
            .RequireAuthorization(AuthorizationExtensions.PlatformOperatorPolicy);

        group.MapGet("/plans", ListPlansAsync)
            .WithName("PlatformListPlans")
            .Produces<IReadOnlyList<PlanResponse>>(StatusCodes.Status200OK);

        group.MapGet("/tenants", ListTenantsAsync)
            .WithName("PlatformListTenants")
            .Produces<IReadOnlyList<PlatformTenantResponse>>(StatusCodes.Status200OK);

        group.MapPost("/tenants", ProvisionAsync)
            .WithName("PlatformProvisionTenant")
            .Produces<PlatformTenantResponse>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden);

        group.MapGet("/tenants/{tenantId:guid}", GetTenantAsync)
            .WithName("PlatformGetTenant")
            .Produces<PlatformTenantResponse>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/tenants/{tenantId:guid}/suspend", SuspendAsync)
            .WithName("PlatformSuspendTenant")
            .Produces<PlatformTenantResponse>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/tenants/{tenantId:guid}/resume", ResumeAsync)
            .WithName("PlatformResumeTenant")
            .Produces<PlatformTenantResponse>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPut("/tenants/{tenantId:guid}/settings", UpdateSettingsAsync)
            .WithName("PlatformUpdateTenantSettings")
            .Produces<PlatformTenantResponse>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        group.MapGet("/tenants/{tenantId:guid}/subscription", GetSubscriptionAsync)
            .WithName("PlatformGetSubscription")
            .Produces<SubscriptionResponse>(StatusCodes.Status200OK);

        group.MapPut("/tenants/{tenantId:guid}/subscription", UpdateSubscriptionAsync)
            .WithName("PlatformUpdateSubscription")
            .Produces<SubscriptionResponse>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        group.MapGet("/tenants/{tenantId:guid}/usage", GetUsageAsync)
            .WithName("PlatformGetUsage")
            .Produces<TenantUsageResponse>(StatusCodes.Status200OK);

        group.MapGet("/tenants/{tenantId:guid}/readiness", GetReadinessAsync)
            .WithName("PlatformGetReadiness")
            .Produces<TenantReadinessResponse>(StatusCodes.Status200OK);

        return endpoints;
    }

    private static async Task<IResult> LoginAsync(
        PlatformLoginRequest request,
        IPlatformAuthenticationService auth,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await auth.LoginAsync(new PlatformLoginCommand(request.Email, request.Password), cancellationToken);
        return ToHttp(result, httpContext);
    }

    private static async Task<IResult> ListPlansAsync(
        IPlatformTenantService platform,
        HttpContext httpContext,
        CancellationToken cancellationToken) =>
        ToHttp(await platform.ListPlansAsync(cancellationToken), httpContext);

    private static async Task<IResult> ListTenantsAsync(
        IPlatformTenantService platform,
        HttpContext httpContext,
        CancellationToken cancellationToken) =>
        ToHttp(await platform.ListTenantsAsync(cancellationToken), httpContext);

    private static async Task<IResult> ProvisionAsync(
        ProvisionTenantRequest request,
        IPlatformTenantService platform,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await platform.ProvisionAsync(
            new ProvisionTenantPlatformCommand(
                request.TenantCode,
                request.TenantName,
                request.CurrencyCode,
                request.TimeZoneId,
                request.OwnerEmail,
                request.OwnerPassword,
                request.OwnerDisplayName,
                request.PlanCode),
            cancellationToken);
        return ToHttp(result, httpContext);
    }

    private static async Task<IResult> GetTenantAsync(
        Guid tenantId,
        IPlatformTenantService platform,
        HttpContext httpContext,
        CancellationToken cancellationToken) =>
        ToHttp(await platform.GetTenantAsync(tenantId, cancellationToken), httpContext);

    private static async Task<IResult> SuspendAsync(
        Guid tenantId,
        IPlatformTenantService platform,
        HttpContext httpContext,
        CancellationToken cancellationToken) =>
        ToHttp(await platform.SuspendAsync(tenantId, cancellationToken), httpContext);

    private static async Task<IResult> ResumeAsync(
        Guid tenantId,
        IPlatformTenantService platform,
        HttpContext httpContext,
        CancellationToken cancellationToken) =>
        ToHttp(await platform.ResumeAsync(tenantId, cancellationToken), httpContext);

    private static async Task<IResult> UpdateSettingsAsync(
        Guid tenantId,
        UpdateTenantSettingsRequest request,
        IPlatformTenantService platform,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await platform.UpdateSettingsAsync(
            new UpdateTenantSettingsCommand(
                tenantId,
                request.Name,
                request.CurrencyCode,
                request.TimeZoneId,
                request.NegativeStockPolicy,
                request.TaxEnabled,
                request.TaxName,
                request.TaxRate,
                request.MaxOfflineDays,
                request.RequireOnlineRevalidation,
                request.ExpectedRowVersion),
            cancellationToken);
        return ToHttp(result, httpContext);
    }

    private static async Task<IResult> GetSubscriptionAsync(
        Guid tenantId,
        IPlatformTenantService platform,
        HttpContext httpContext,
        CancellationToken cancellationToken) =>
        ToHttp(await platform.GetSubscriptionAsync(tenantId, cancellationToken), httpContext);

    private static async Task<IResult> UpdateSubscriptionAsync(
        Guid tenantId,
        UpdateSubscriptionRequest request,
        IPlatformTenantService platform,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await platform.UpdateSubscriptionAsync(
            new UpdateSubscriptionCommand(
                tenantId,
                request.PlanCode,
                request.Status,
                request.TrialEndsAtUtc,
                request.CurrentPeriodStartUtc,
                request.CurrentPeriodEndUtc,
                request.ExternalReference,
                request.ExpectedRowVersion),
            cancellationToken);
        return ToHttp(result, httpContext);
    }

    private static async Task<IResult> GetUsageAsync(
        Guid tenantId,
        IPlatformTenantService platform,
        HttpContext httpContext,
        CancellationToken cancellationToken) =>
        ToHttp(await platform.GetUsageAsync(tenantId, cancellationToken), httpContext);

    private static async Task<IResult> GetReadinessAsync(
        Guid tenantId,
        IPlatformTenantService platform,
        HttpContext httpContext,
        CancellationToken cancellationToken) =>
        ToHttp(await platform.GetReadinessAsync(tenantId, cancellationToken), httpContext);

    private static IResult ToHttp<T>(AppResult<T> result, HttpContext httpContext)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        return Results.Json(
            new ApiErrorResponse(result.ErrorCode!, result.ErrorMessage!, CorrelationId(httpContext)),
            statusCode: StatusFromCode(result.ErrorCode!));
    }

    private static string CorrelationId(HttpContext httpContext) =>
        httpContext.Items[Http.CorrelationIdMiddleware.ItemKey] as string ?? "unknown";

    private static int StatusFromCode(string code) => code switch
    {
        ApiErrorCodes.Unauthorized => StatusCodes.Status401Unauthorized,
        ApiErrorCodes.Forbidden => StatusCodes.Status403Forbidden,
        ApiErrorCodes.TenantNotFound => StatusCodes.Status404NotFound,
        ApiErrorCodes.SyncConflict => StatusCodes.Status409Conflict,
        ApiErrorCodes.ValidationFailed => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status400BadRequest,
    };

    public sealed record PlatformLoginRequest(string Email, string Password);

    public sealed record ProvisionTenantRequest(
        string TenantCode,
        string TenantName,
        string CurrencyCode,
        string TimeZoneId,
        string OwnerEmail,
        string OwnerPassword,
        string OwnerDisplayName,
        string? PlanCode);

    public sealed record UpdateTenantSettingsRequest(
        string Name,
        string CurrencyCode,
        string TimeZoneId,
        NegativeStockPolicyDto NegativeStockPolicy,
        bool TaxEnabled,
        string? TaxName,
        decimal? TaxRate,
        int? MaxOfflineDays,
        bool RequireOnlineRevalidation,
        int ExpectedRowVersion);

    public sealed record UpdateSubscriptionRequest(
        string PlanCode,
        SubscriptionStatus Status,
        DateTimeOffset? TrialEndsAtUtc,
        DateTimeOffset? CurrentPeriodStartUtc,
        DateTimeOffset? CurrentPeriodEndUtc,
        string? ExternalReference,
        int ExpectedRowVersion);
}

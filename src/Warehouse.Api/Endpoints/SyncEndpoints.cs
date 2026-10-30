using Warehouse.Api.Authorization;
using Warehouse.Api.Http;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Sync;
using Warehouse.Shared.Api;

namespace Warehouse.Api.Endpoints;

public static class SyncEndpoints
{
    public static IEndpointRouteBuilder MapSyncEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/sync").WithTags("Sync").RequireAuthorization().RequireSync();
        group.MapPost("/push", PushAsync).RequirePermission("inventory.read").WithName("SyncPush");
        group.MapPost("/pull", PullAsync).RequirePermission("inventory.read").WithName("SyncPull");
        return endpoints;
    }

    private static async Task<IResult> PushAsync(
        SyncPushHttpRequest request,
        ISyncService sync,
        HttpContext http,
        CancellationToken ct)
    {
        var operations = (request.Operations ?? [])
            .Select(o => new SyncPushOperationRequest(
                o.OperationId,
                o.EntityType,
                o.EntityId,
                o.OperationType,
                o.PayloadJson ?? "{}",
                o.ClientTimestampUtc,
                o.BaseVersion))
            .ToList();

        return ToHttp(await sync.PushAsync(new SyncPushRequest(operations), ct), http);
    }

    private static async Task<IResult> PullAsync(
        SyncPullHttpRequest request,
        ISyncService sync,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(await sync.PullAsync(new SyncPullRequest(request.AfterChangeVersion, request.Limit), ct), http);

    private static IResult ToHttp<T>(AppResult<T> result, HttpContext http)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        return Results.Json(
            new ApiErrorResponse(result.ErrorCode!, result.ErrorMessage!, CorrelationId(http)),
            statusCode: StatusFromCode(result.ErrorCode!));
    }

    private static string CorrelationId(HttpContext http) =>
        http.Items[CorrelationIdMiddleware.ItemKey] as string ?? "unknown";

    private static int StatusFromCode(string code) => code switch
    {
        ApiErrorCodes.Unauthorized => StatusCodes.Status401Unauthorized,
        ApiErrorCodes.Forbidden => StatusCodes.Status403Forbidden,
        ApiErrorCodes.InsufficientStock => StatusCodes.Status409Conflict,
        ApiErrorCodes.SyncConflict => StatusCodes.Status409Conflict,
        ApiErrorCodes.ValidationFailed => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status400BadRequest,
    };

    public sealed record SyncPushHttpRequest(IReadOnlyList<SyncPushOpDto>? Operations);

    public sealed record SyncPushOpDto(
        Guid OperationId,
        string EntityType,
        Guid EntityId,
        string OperationType,
        string? PayloadJson,
        DateTimeOffset? ClientTimestampUtc,
        int? BaseVersion);

    public sealed record SyncPullHttpRequest(long AfterChangeVersion, int Limit = 100);
}

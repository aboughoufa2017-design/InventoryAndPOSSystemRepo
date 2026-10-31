using Warehouse.Api.Authorization;
using Warehouse.Api.Http;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Pos;
using Warehouse.Shared.Api;

namespace Warehouse.Api.Endpoints;

public static class SelfCheckoutEndpoints
{
    public static IEndpointRouteBuilder MapSelfCheckoutEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Guest Scan & Go — no JWT; tenant resolved via tenantCode / access token.
        var guest = endpoints.MapGroup("/api/v1/pos/self-checkout")
            .WithTags("SelfCheckout")
            .AllowAnonymous();

        guest.MapPost("/sessions", StartSessionAsync).WithName("SelfCheckoutStart");
        guest.MapGet("/sessions/current", GetCurrentAsync).WithName("SelfCheckoutGetCurrent");
        guest.MapPost("/sessions/current/lines", AddLineAsync).WithName("SelfCheckoutAddLine");
        guest.MapPut("/sessions/current/lines/{lineId:guid}", SetLineAsync).WithName("SelfCheckoutSetLine");
        guest.MapDelete("/sessions/current/lines/{lineId:guid}", RemoveLineAsync).WithName("SelfCheckoutRemoveLine");
        guest.MapPost("/sessions/current/transfer", TransferAsync).WithName("SelfCheckoutTransfer");
        guest.MapPost("/sessions/current/reopen", ReopenAsync).WithName("SelfCheckoutReopen");

        // Cashier handoff + random audit (authenticated POS).
        var staff = endpoints.MapGroup("/api/v1/pos/self-checkout")
            .WithTags("SelfCheckout")
            .RequireAuthorization()
            .RequireSales();

        staff.MapGet("/waiting", ListWaitingAsync)
            .RequirePermission("pos.sale")
            .WithName("SelfCheckoutListWaiting");
        staff.MapPost("/import", ImportAsync)
            .RequirePermission("pos.sale")
            .WithName("SelfCheckoutImport");
        staff.MapPost("/sessions/{id:guid}/audit/start", StartAuditAsync)
            .RequirePermission("pos.sale")
            .WithName("SelfCheckoutStartAudit");
        staff.MapPost("/sessions/{id:guid}/audit/complete", CompleteAuditAsync)
            .RequirePermission("pos.sale")
            .WithName("SelfCheckoutCompleteAudit");
        staff.MapPost("/sessions/{id:guid}/close", CloseAsync)
            .RequirePermission("pos.sale")
            .WithName("SelfCheckoutClose");

        return endpoints;
    }

    private static async Task<IResult> StartSessionAsync(
        StartSelfCheckoutRequest request,
        ISelfCheckoutService service,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(
            await service.StartAsync(
                new StartSelfCheckoutCommand(request.TenantCode, request.WarehouseId, request.IsGuest),
                ct),
            http);

    private static async Task<IResult> GetCurrentAsync(
        string accessToken,
        ISelfCheckoutService service,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(await service.GetByAccessTokenAsync(accessToken, ct), http);

    private static async Task<IResult> AddLineAsync(
        AddSelfCheckoutLineRequest request,
        ISelfCheckoutService service,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(
            await service.AddLineByBarcodeAsync(
                request.AccessToken,
                request.Barcode,
                request.Quantity <= 0 ? 1m : request.Quantity,
                ct),
            http);

    private static async Task<IResult> SetLineAsync(
        Guid lineId,
        SetSelfCheckoutLineRequest request,
        ISelfCheckoutService service,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(
            await service.SetLineQuantityAsync(request.AccessToken, lineId, request.Quantity, ct),
            http);

    private static async Task<IResult> RemoveLineAsync(
        Guid lineId,
        string accessToken,
        ISelfCheckoutService service,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(await service.RemoveLineAsync(accessToken, lineId, ct), http);

    private static async Task<IResult> TransferAsync(
        AccessTokenRequest request,
        ISelfCheckoutService service,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(await service.TransferAsync(request.AccessToken, ct), http);

    private static async Task<IResult> ReopenAsync(
        AccessTokenRequest request,
        ISelfCheckoutService service,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(await service.ReopenAsync(request.AccessToken, ct), http);

    private static async Task<IResult> ListWaitingAsync(
        Guid warehouseId,
        ISelfCheckoutService service,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(await service.ListWaitingAsync(warehouseId, ct), http);

    private static async Task<IResult> ImportAsync(
        ImportSelfCheckoutRequest request,
        ISelfCheckoutService service,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(await service.ImportByCodeAsync(request.CodeOrQrPayload, ct), http);

    private static async Task<IResult> StartAuditAsync(
        Guid id,
        ISelfCheckoutService service,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(await service.StartAuditAsync(id, ct), http);

    private static async Task<IResult> CompleteAuditAsync(
        Guid id,
        CompleteAuditRequest request,
        ISelfCheckoutService service,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(await service.CompleteAuditAsync(id, request.ScannedBarcodes ?? [], ct), http);

    private static async Task<IResult> CloseAsync(
        Guid id,
        ISelfCheckoutService service,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(await service.MarkClosedAsync(id, ct), http);

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
        ApiErrorCodes.ValidationFailed => StatusCodes.Status400BadRequest,
        ApiErrorCodes.SyncConflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status400BadRequest,
    };

    public sealed record StartSelfCheckoutRequest(string TenantCode, Guid? WarehouseId, bool IsGuest = true);

    public sealed record AccessTokenRequest(string AccessToken);

    public sealed record AddSelfCheckoutLineRequest(string AccessToken, string Barcode, decimal Quantity = 1);

    public sealed record SetSelfCheckoutLineRequest(string AccessToken, decimal Quantity);

    public sealed record ImportSelfCheckoutRequest(string CodeOrQrPayload);

    public sealed record CompleteAuditRequest(IReadOnlyList<string>? ScannedBarcodes);
}

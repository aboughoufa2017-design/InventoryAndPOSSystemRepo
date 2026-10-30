using Warehouse.Api.Authorization;
using Warehouse.Api.Http;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Inventory;
using Warehouse.Shared.Api;

namespace Warehouse.Api.Endpoints;

public static class TransferEndpoints
{
    public static IEndpointRouteBuilder MapTransferEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/transfers").WithTags("Transfers").RequireAuthorization().RequireTransfers();

        group.MapGet("/", ListAsync).RequirePermission("inventory.read").WithName("ListTransfers");
        group.MapGet("/{id:guid}", GetAsync).RequirePermission("inventory.read").WithName("GetTransfer");
        group.MapPost("/", CreateAsync).RequirePermission("inventory.transfer").WithName("CreateTransfer");
        group.MapPost("/{id:guid}/dispatch", DispatchAsync).RequirePermission("inventory.transfer").WithName("DispatchTransfer");
        group.MapPost("/{id:guid}/receive", ReceiveAsync).RequirePermission("inventory.transfer").WithName("ReceiveTransfer");
        group.MapPost("/{id:guid}/cancel", CancelAsync).RequirePermission("inventory.transfer").WithName("CancelTransfer");

        return endpoints;
    }

    private static async Task<IResult> ListAsync(ITransferService transfers, string? status, CancellationToken ct) =>
        Results.Ok(await transfers.ListAsync(status, ct));

    private static async Task<IResult> GetAsync(Guid id, ITransferService transfers, HttpContext http, CancellationToken ct) =>
        ToHttp(await transfers.GetAsync(id, ct), http);

    private static async Task<IResult> CreateAsync(
        CreateTransferRequest request,
        ITransferService transfers,
        HttpContext http,
        CancellationToken ct)
    {
        var lines = request.Lines.Select(l => new TransferLineRequest(l.ProductId, l.Quantity, l.Notes)).ToList();
        return ToHttp(
            await transfers.CreateAsync(
                new CreateTransferCommand(
                    request.SourceWarehouseId,
                    request.DestinationWarehouseId,
                    request.SourceLocationId,
                    request.DestinationLocationId,
                    request.Notes,
                    lines),
                ct),
            http);
    }

    private static async Task<IResult> DispatchAsync(
        Guid id,
        DispatchTransferRequest request,
        ITransferService transfers,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(
            await transfers.DispatchAsync(
                new DispatchTransferCommand(id, request.ExpectedRowVersion, request.OperationId),
                ct),
            http);

    private static async Task<IResult> ReceiveAsync(
        Guid id,
        ReceiveTransferRequest request,
        ITransferService transfers,
        HttpContext http,
        CancellationToken ct)
    {
        var lines = request.Lines.Select(l => new ReceiveTransferLineRequest(l.TransferLineId, l.Quantity)).ToList();
        return ToHttp(
            await transfers.ReceiveAsync(
                new ReceiveTransferCommand(id, request.ExpectedRowVersion, request.OperationId, lines),
                ct),
            http);
    }

    private static async Task<IResult> CancelAsync(
        Guid id,
        CancelTransferRequest request,
        ITransferService transfers,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(
            await transfers.CancelAsync(
                new CancelTransferCommand(id, request.ExpectedRowVersion, request.OperationId),
                ct),
            http);

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
        ApiErrorCodes.OperationAlreadyProcessed => StatusCodes.Status200OK,
        ApiErrorCodes.ValidationFailed => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status400BadRequest,
    };

    public sealed record CreateTransferRequest(
        Guid SourceWarehouseId,
        Guid DestinationWarehouseId,
        Guid? SourceLocationId,
        Guid? DestinationLocationId,
        string? Notes,
        IReadOnlyList<TransferLineDto> Lines);

    public sealed record TransferLineDto(Guid ProductId, decimal Quantity, string? Notes);

    public sealed record DispatchTransferRequest(int ExpectedRowVersion, Guid? OperationId);

    public sealed record ReceiveTransferRequest(
        int ExpectedRowVersion,
        Guid? OperationId,
        IReadOnlyList<ReceiveLineDto> Lines);

    public sealed record ReceiveLineDto(Guid TransferLineId, decimal Quantity);

    public sealed record CancelTransferRequest(int ExpectedRowVersion, Guid? OperationId);
}

using Warehouse.Api.Authorization;
using Warehouse.Api.Http;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Inventory;
using Warehouse.Shared.Api;

namespace Warehouse.Api.Endpoints;

public static class StockCountEndpoints
{
    public static IEndpointRouteBuilder MapStockCountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/stock-counts")
            .WithTags("StockCounts")
            .RequireAuthorization()
            .RequireInventory();

        group.MapGet("/", ListAsync).RequirePermission("inventory.adjust").WithName("ListStockCounts");
        group.MapGet("/{id:guid}", GetAsync).RequirePermission("inventory.adjust").WithName("GetStockCount");
        group.MapPost("/", CreateAsync).RequirePermission("inventory.adjust").WithName("CreateStockCount");
        group.MapPut("/{id:guid}/lines", SetLinesAsync).RequirePermission("inventory.adjust").WithName("SetStockCountLines");
        group.MapPost("/{id:guid}/post", PostAsync).RequirePermission("inventory.adjust").WithName("PostStockCount");
        group.MapPost("/{id:guid}/cancel", CancelAsync).RequirePermission("inventory.adjust").WithName("CancelStockCount");

        return endpoints;
    }

    private static async Task<IResult> ListAsync(IStockCountService stockCounts, CancellationToken ct) =>
        Results.Ok(await stockCounts.ListAsync(ct));

    private static async Task<IResult> GetAsync(
        Guid id,
        IStockCountService stockCounts,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(await stockCounts.GetAsync(id, ct), http);

    private static async Task<IResult> CreateAsync(
        CreateStockCountRequest request,
        IStockCountService stockCounts,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(
            await stockCounts.CreateAsync(
                new CreateStockCountCommand(request.WarehouseId, request.LocationId, request.Notes),
                ct),
            http);

    private static async Task<IResult> SetLinesAsync(
        Guid id,
        SetStockCountLinesRequest request,
        IStockCountService stockCounts,
        HttpContext http,
        CancellationToken ct)
    {
        var lines = request.Lines
            .Select(l => new StockCountLineInput(l.ProductId, l.CountedQuantity, l.Notes))
            .ToList();
        return ToHttp(
            await stockCounts.SetLinesAsync(
                new SetStockCountLinesCommand(id, request.ExpectedRowVersion, lines),
                ct),
            http);
    }

    private static async Task<IResult> PostAsync(
        Guid id,
        PostStockCountRequest request,
        IStockCountService stockCounts,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(
            await stockCounts.PostAsync(new PostStockCountCommand(id, request.ExpectedRowVersion), ct),
            http);

    private static async Task<IResult> CancelAsync(
        Guid id,
        CancelStockCountRequest request,
        IStockCountService stockCounts,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(
            await stockCounts.CancelAsync(new CancelStockCountCommand(id, request.ExpectedRowVersion), ct),
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
        ApiErrorCodes.ValidationFailed => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status400BadRequest,
    };

    public sealed record CreateStockCountRequest(Guid WarehouseId, Guid? LocationId, string? Notes);

    public sealed record SetStockCountLinesRequest(
        int ExpectedRowVersion,
        IReadOnlyList<StockCountLineDto> Lines);

    public sealed record StockCountLineDto(Guid ProductId, decimal CountedQuantity, string? Notes);

    public sealed record PostStockCountRequest(int ExpectedRowVersion);

    public sealed record CancelStockCountRequest(int ExpectedRowVersion);
}

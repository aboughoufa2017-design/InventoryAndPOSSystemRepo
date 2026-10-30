using Warehouse.Api.Authorization;
using Warehouse.Api.Http;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Inventory;
using Warehouse.Shared.Api;

namespace Warehouse.Api.Endpoints;

public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var warehouses = endpoints.MapGroup("/api/v1/warehouses").WithTags("Warehouses").RequireAuthorization().RequireInventory();
        warehouses.MapGet("/", ListWarehousesAsync).RequirePermission("inventory.read").WithName("ListWarehouses");
        warehouses.MapPost("/", CreateWarehouseAsync).RequirePermission("inventory.adjust").WithName("CreateWarehouse");
        warehouses.MapPut("/{id:guid}", UpdateWarehouseAsync).RequirePermission("inventory.adjust").WithName("UpdateWarehouse");
        warehouses.MapGet("/{id:guid}/locations", ListLocationsAsync)
            .RequirePermission("inventory.read")
            .WithName("ListWarehouseLocations");

        var inventory = endpoints.MapGroup("/api/v1/inventory").WithTags("Inventory").RequireAuthorization().RequireInventory();
        inventory.MapGet("/balances", ListBalancesAsync).RequirePermission("inventory.read").WithName("ListStockBalances");
        inventory.MapGet("/balances/{productId:guid}/{warehouseId:guid}/{locationId:guid}", GetBalanceAsync)
            .RequirePermission("inventory.read")
            .WithName("GetStockBalance");
        inventory.MapGet("/movements", ListMovementsAsync).RequirePermission("inventory.read").WithName("ListStockMovements");

        inventory.MapPost("/adjustments", CreateAdjustmentAsync)
            .RequirePermission("inventory.adjust")
            .WithName("CreateStockAdjustment");
        inventory.MapGet("/adjustments/{id:guid}", GetAdjustmentAsync)
            .RequirePermission("inventory.read")
            .WithName("GetStockAdjustment");
        inventory.MapPost("/adjustments/{id:guid}/post", PostAdjustmentAsync)
            .RequirePermission("inventory.adjust")
            .WithName("PostStockAdjustment");
        inventory.MapPost("/adjustments/{id:guid}/cancel", CancelAdjustmentAsync)
            .RequirePermission("inventory.adjust")
            .WithName("CancelStockAdjustment");

        return endpoints;
    }

    private static async Task<IResult> ListWarehousesAsync(IWarehouseService warehouses, CancellationToken ct) =>
        Results.Ok(await warehouses.ListAsync(ct));

    private static async Task<IResult> CreateWarehouseAsync(
        CreateWarehouseRequest request,
        IWarehouseService warehouses,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(await warehouses.CreateAsync(new CreateWarehouseCommand(request.Code, request.Name), ct), http);

    private static async Task<IResult> UpdateWarehouseAsync(
        Guid id,
        UpdateWarehouseRequest request,
        IWarehouseService warehouses,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(
            await warehouses.UpdateAsync(new UpdateWarehouseCommand(id, request.Name, request.IsActive), ct),
            http);

    private static async Task<IResult> ListLocationsAsync(
        Guid id,
        IWarehouseService warehouses,
        CancellationToken ct) =>
        Results.Ok(await warehouses.ListLocationsAsync(id, ct));

    private static async Task<IResult> ListBalancesAsync(
        IInventoryService inventory,
        Guid? warehouseId,
        Guid? productId,
        CancellationToken ct) =>
        Results.Ok(await inventory.ListBalancesAsync(warehouseId, productId, ct));

    private static async Task<IResult> GetBalanceAsync(
        Guid productId,
        Guid warehouseId,
        Guid locationId,
        IInventoryService inventory,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(await inventory.GetBalanceAsync(productId, warehouseId, locationId, ct), http);

    private static async Task<IResult> ListMovementsAsync(
        IInventoryService inventory,
        Guid? warehouseId,
        Guid? productId,
        int? take,
        CancellationToken ct) =>
        Results.Ok(await inventory.ListMovementsAsync(warehouseId, productId, take ?? 50, ct));

    private static async Task<IResult> CreateAdjustmentAsync(
        CreateAdjustmentRequest request,
        IInventoryService inventory,
        HttpContext http,
        CancellationToken ct)
    {
        var lines = request.Lines.Select(l =>
            new StockAdjustmentLineRequest(l.ProductId, l.LocationId, l.QuantityChange, l.Notes)).ToList();
        return ToHttp(
            await inventory.CreateAdjustmentAsync(
                new CreateStockAdjustmentCommand(request.WarehouseId, request.Kind, request.Reason, lines),
                ct),
            http);
    }

    private static async Task<IResult> GetAdjustmentAsync(
        Guid id,
        IInventoryService inventory,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(await inventory.GetAdjustmentAsync(id, ct), http);

    private static async Task<IResult> PostAdjustmentAsync(
        Guid id,
        IInventoryService inventory,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(await inventory.PostAdjustmentAsync(id, ct), http);

    private static async Task<IResult> CancelAdjustmentAsync(
        Guid id,
        IInventoryService inventory,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(await inventory.CancelAdjustmentAsync(id, ct), http);

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

    public sealed record CreateWarehouseRequest(string Code, string Name);

    public sealed record UpdateWarehouseRequest(string Name, bool IsActive = true);

    public sealed record CreateAdjustmentRequest(
        Guid WarehouseId,
        string Kind,
        string? Reason,
        IReadOnlyList<AdjustmentLineRequest> Lines);

    public sealed record AdjustmentLineRequest(
        Guid ProductId,
        Guid LocationId,
        decimal QuantityChange,
        string? Notes);
}

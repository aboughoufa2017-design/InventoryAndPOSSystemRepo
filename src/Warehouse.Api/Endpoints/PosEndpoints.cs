using Warehouse.Api.Authorization;
using Warehouse.Api.Http;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Catalog;
using Warehouse.Application.Pos;
using Warehouse.Shared.Api;

namespace Warehouse.Api.Endpoints;

public static class PosEndpoints
{
    public static IEndpointRouteBuilder MapPosEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var pos = endpoints.MapGroup("/api/v1/pos")
            .WithTags("POS")
            .RequireAuthorization()
            .RequireSales();

        pos.MapGet("/products/lookup", LookupProductAsync)
            .RequirePermission("pos.sale")
            .WithName("PosLookupProduct");
        pos.MapGet("/sales", ListSalesAsync)
            .RequirePermission("pos.sale")
            .WithName("PosListSales");
        pos.MapGet("/sales/{id:guid}", GetSaleAsync)
            .RequirePermission("pos.sale")
            .WithName("PosGetSale");
        pos.MapPost("/sales", FinalizeSaleAsync)
            .RequirePermission("pos.sale")
            .WithName("PosFinalizeSale");
        pos.MapPost("/sales/{id:guid}/void", VoidSaleAsync)
            .RequirePermission("pos.void")
            .WithName("PosVoidSale");
        pos.MapPost("/sales/{id:guid}/refund", RefundSaleAsync)
            .RequirePermission("pos.refund")
            .WithName("PosRefundSale");

        pos.MapGet("/shifts", ListShiftsAsync)
            .RequirePermission("pos.shift")
            .WithName("PosListShifts");
        pos.MapPost("/shifts", OpenShiftAsync)
            .RequirePermission("pos.shift")
            .WithName("PosOpenShift");
        pos.MapPost("/shifts/{id:guid}/close", CloseShiftAsync)
            .RequirePermission("pos.shift")
            .WithName("PosCloseShift");

        pos.MapGet("/conflicts", ListConflictsAsync)
            .RequirePermission("pos.void")
            .WithName("PosListConflicts");
        pos.MapPost("/conflicts/{id:guid}/resolve", ResolveConflictAsync)
            .RequirePermission("pos.void")
            .WithName("PosResolveConflict");

        pos.MapGet("/price-lists", ListPriceListsAsync)
            .RequirePermission("pos.sale")
            .WithName("PosListPriceLists");
        pos.MapPost("/price-lists", CreatePriceListAsync)
            .RequirePermission("pos.sale")
            .WithName("PosCreatePriceList");
        pos.MapPost("/price-lists/{id:guid}/prices", SetProductPriceAsync)
            .RequirePermission("pos.sale")
            .WithName("PosSetProductPrice");

        pos.MapGet("/restaurants/tables", ListTablesAsync)
            .RequirePermission("pos.sale")
            .WithName("PosListTables");
        pos.MapPost("/restaurants/tables", CreateTableAsync)
            .RequirePermission("pos.sale")
            .WithName("PosCreateTable");
        pos.MapGet("/restaurants/orders", ListOpenOrdersAsync)
            .RequirePermission("pos.sale")
            .WithName("PosListOpenOrders");
        pos.MapPost("/restaurants/orders", OpenOrderAsync)
            .RequirePermission("pos.sale")
            .WithName("PosOpenRestaurantOrder");
        pos.MapPost("/restaurants/orders/{id:guid}/cancel", CancelOrderAsync)
            .RequirePermission("pos.sale")
            .WithName("PosCancelRestaurantOrder");

        return endpoints;
    }

    private static async Task<IResult> LookupProductAsync(
        string? barcode,
        string? sku,
        ICatalogService catalog,
        HttpContext http,
        CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(barcode))
        {
            return ToHttp(await catalog.GetProductByBarcodeAsync(barcode, ct), http);
        }

        if (!string.IsNullOrWhiteSpace(sku))
        {
            return ToHttp(await catalog.GetProductBySkuAsync(sku, ct), http);
        }

        return Results.Json(
            new ApiErrorResponse(ApiErrorCodes.ValidationFailed, "barcode or sku is required.", CorrelationId(http)),
            statusCode: StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> ListSalesAsync(
        IPosCheckoutService pos,
        Guid? warehouseId,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        CancellationToken ct) =>
        Results.Ok(await pos.ListAsync(warehouseId, fromUtc, toUtc, ct));

    private static async Task<IResult> GetSaleAsync(
        Guid id,
        IPosCheckoutService pos,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(await pos.GetAsync(id, ct), http);

    private static async Task<IResult> FinalizeSaleAsync(
        FinalizePosSaleRequest request,
        IPosCheckoutService pos,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(
            await pos.FinalizeCashSaleAsync(
                new FinalizePosCashSaleCommand(
                    request.ClientCommandId,
                    request.WarehouseId,
                    request.LocationId,
                    request.DeviceId,
                    request.CustomerId,
                    request.CashShiftId,
                    request.CurrencyCode,
                    request.Notes,
                    request.PaymentMethod,
                    request.Lines ?? []),
                ct),
            http);

    private static async Task<IResult> VoidSaleAsync(
        Guid id,
        VoidPosSaleRequest? request,
        IPosCheckoutService pos,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(
            await pos.VoidAsync(new VoidPosSaleCommand(id, request?.ClientCommandId), ct),
            http);

    private static async Task<IResult> RefundSaleAsync(
        Guid id,
        RefundPosSaleRequest request,
        IPosCheckoutService pos,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(
            await pos.RefundAsync(
                new RefundPosSaleCommand(id, request.ClientCommandId, request.Lines ?? []),
                ct),
            http);

    private static async Task<IResult> ListShiftsAsync(
        ICashShiftService shifts,
        Guid? warehouseId,
        CancellationToken ct) =>
        Results.Ok(await shifts.ListAsync(warehouseId, ct));

    private static async Task<IResult> OpenShiftAsync(
        OpenShiftRequest request,
        ICashShiftService shifts,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(
            await shifts.OpenAsync(new OpenCashShiftCommand(request.WarehouseId, request.DeviceId, request.OpeningFloat), ct),
            http);

    private static async Task<IResult> CloseShiftAsync(
        Guid id,
        CloseShiftRequest request,
        ICashShiftService shifts,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(await shifts.CloseAsync(new CloseCashShiftCommand(id, request.CountedCash), ct), http);

    private static async Task<IResult> ListConflictsAsync(IPosConflictService conflicts, CancellationToken ct) =>
        Results.Ok(await conflicts.ListOpenAsync(ct));

    private static async Task<IResult> ResolveConflictAsync(
        Guid id,
        IPosConflictService conflicts,
        HttpContext http,
        CancellationToken ct)
    {
        var result = await conflicts.ResolveAsync(id, ct);
        if (result.IsSuccess)
        {
            return Results.Ok();
        }

        return Results.Json(
            new ApiErrorResponse(result.ErrorCode!, result.ErrorMessage!, CorrelationId(http)),
            statusCode: StatusFromCode(result.ErrorCode!));
    }

    private static async Task<IResult> ListPriceListsAsync(IPosPricingService pricing, CancellationToken ct) =>
        Results.Ok(await pricing.ListPriceListsAsync(ct));

    private static async Task<IResult> CreatePriceListAsync(
        CreatePriceListRequest request,
        IPosPricingService pricing,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(
            await pricing.CreatePriceListAsync(
                new CreatePriceListCommand(request.Code, request.Name, request.CurrencyCode),
                ct),
            http);

    private static async Task<IResult> SetProductPriceAsync(
        Guid id,
        SetProductPriceRequest request,
        IPosPricingService pricing,
        HttpContext http,
        CancellationToken ct)
    {
        var result = await pricing.SetProductPriceAsync(
            new SetProductPriceCommand(id, request.ProductId, request.UnitPrice, request.MinQuantity),
            ct);
        if (result.IsSuccess)
        {
            return Results.Ok();
        }

        return Results.Json(
            new ApiErrorResponse(result.ErrorCode!, result.ErrorMessage!, CorrelationId(http)),
            statusCode: StatusFromCode(result.ErrorCode!));
    }

    private static async Task<IResult> ListTablesAsync(
        Guid warehouseId,
        IRestaurantPosService restaurant,
        CancellationToken ct) =>
        Results.Ok(await restaurant.ListTablesAsync(warehouseId, ct));

    private static async Task<IResult> CreateTableAsync(
        CreateTableRequest request,
        IRestaurantPosService restaurant,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(
            await restaurant.CreateTableAsync(
                new CreateRestaurantTableCommand(request.WarehouseId, request.Code, request.Name),
                ct),
            http);

    private static async Task<IResult> ListOpenOrdersAsync(
        Guid warehouseId,
        IRestaurantPosService restaurant,
        CancellationToken ct) =>
        Results.Ok(await restaurant.ListOpenOrdersAsync(warehouseId, ct));

    private static async Task<IResult> OpenOrderAsync(
        OpenRestaurantOrderRequest request,
        IRestaurantPosService restaurant,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(
            await restaurant.OpenOrderAsync(
                new OpenRestaurantOrderCommand(request.WarehouseId, request.TableId, request.Notes),
                ct),
            http);

    private static async Task<IResult> CancelOrderAsync(
        Guid id,
        IRestaurantPosService restaurant,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(await restaurant.CancelOrderAsync(id, ct), http);

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

    public sealed record FinalizePosSaleRequest(
        Guid ClientCommandId,
        Guid WarehouseId,
        Guid? LocationId,
        Guid? DeviceId,
        Guid? CustomerId,
        Guid? CashShiftId,
        string? CurrencyCode,
        string? Notes,
        PosPaymentMethodDto PaymentMethod,
        IReadOnlyList<PosSaleLineInput>? Lines);

    public sealed record VoidPosSaleRequest(Guid? ClientCommandId);

    public sealed record RefundPosSaleRequest(Guid ClientCommandId, IReadOnlyList<PosRefundLineInput>? Lines);

    public sealed record OpenShiftRequest(Guid WarehouseId, Guid? DeviceId, decimal OpeningFloat);

    public sealed record CloseShiftRequest(decimal CountedCash);

    public sealed record CreatePriceListRequest(string Code, string Name, string? CurrencyCode);

    public sealed record SetProductPriceRequest(Guid ProductId, decimal UnitPrice, decimal? MinQuantity);

    public sealed record CreateTableRequest(Guid WarehouseId, string Code, string Name);

    public sealed record OpenRestaurantOrderRequest(Guid WarehouseId, Guid TableId, string? Notes);
}

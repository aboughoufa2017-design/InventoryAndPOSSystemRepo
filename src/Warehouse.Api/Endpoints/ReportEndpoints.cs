using Warehouse.Api.Authorization;
using Warehouse.Api.Http;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Reporting;
using Warehouse.Shared.Api;

namespace Warehouse.Api.Endpoints;

public static class ReportEndpoints
{
    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/reports")
            .WithTags("Reports")
            .RequireAuthorization()
            .RequireReports();

        group.MapGet("/stock-on-hand", StockOnHandAsync)
            .RequirePermission("reports.read")
            .WithName("ReportStockOnHand");
        group.MapGet("/movements", MovementsAsync)
            .RequirePermission("reports.read")
            .WithName("ReportMovements");
        group.MapGet("/purchases", PurchaseSummaryAsync)
            .RequirePermission("reports.read")
            .WithName("ReportPurchaseSummary");
        group.MapGet("/sales", SalesSummaryAsync)
            .RequirePermission("reports.read")
            .WithName("ReportSalesSummary");

        return endpoints;
    }

    private static async Task<IResult> StockOnHandAsync(
        IReportService reports,
        Guid? warehouseId,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(await reports.StockOnHandAsync(warehouseId, ct), http);

    private static async Task<IResult> MovementsAsync(
        IReportService reports,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        Guid? warehouseId,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(await reports.MovementsAsync(fromUtc, toUtc, warehouseId, ct), http);

    private static async Task<IResult> PurchaseSummaryAsync(
        IReportService reports,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(await reports.PurchaseSummaryAsync(fromUtc, toUtc, ct), http);

    private static async Task<IResult> SalesSummaryAsync(
        IReportService reports,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(await reports.SalesSummaryAsync(fromUtc, toUtc, ct), http);

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
        _ => StatusCodes.Status400BadRequest,
    };
}

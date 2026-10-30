using Warehouse.Api.Authorization;
using Warehouse.Api.Http;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Sales;
using Warehouse.Shared.Api;

namespace Warehouse.Api.Endpoints;

public static class SalesEndpoints
{
    public static IEndpointRouteBuilder MapSalesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var customers = endpoints.MapGroup("/api/v1/customers").WithTags("Customers").RequireAuthorization().RequireSales();
        customers.MapGet("/", ListCustomersAsync).RequirePermission("sales.create").WithName("ListCustomers");
        customers.MapGet("/{id:guid}", GetCustomerAsync).RequirePermission("sales.create").WithName("GetCustomer");
        customers.MapPost("/", CreateCustomerAsync).RequirePermission("sales.create").WithName("CreateCustomer");
        customers.MapPut("/{id:guid}", UpdateCustomerAsync).RequirePermission("sales.create").WithName("UpdateCustomer");

        var sales = endpoints.MapGroup("/api/v1/sales").WithTags("Sales").RequireAuthorization().RequireSales();
        sales.MapGet("/", ListSalesAsync).RequirePermission("sales.create").WithName("ListSalesOrders");
        sales.MapGet("/{id:guid}", GetSaleAsync).RequirePermission("sales.create").WithName("GetSalesOrder");
        sales.MapPost("/", CreateSaleAsync).RequirePermission("sales.create").WithName("CreateSalesOrder");
        sales.MapPost("/{id:guid}/confirm", ConfirmAsync).RequirePermission("sales.create").WithName("ConfirmSalesOrder");
        sales.MapPost("/{id:guid}/ship", ShipAsync).RequirePermission("inventory.issue").WithName("ShipSalesOrder");
        sales.MapPost("/{id:guid}/returns", ReturnAsync).RequirePermission("sales.create").WithName("ReturnSalesOrder");
        sales.MapPost("/{id:guid}/close", CloseAsync).RequirePermission("sales.create").WithName("CloseSalesOrder");
        sales.MapPost("/{id:guid}/cancel", CancelAsync).RequirePermission("sales.cancel").WithName("CancelSalesOrder");
        sales.MapPut("/{id:guid}/payment", UpdatePaymentAsync).RequirePermission("sales.create").WithName("UpdateSalesPayment");

        return endpoints;
    }

    private static async Task<IResult> ListCustomersAsync(ICustomerService customers, CancellationToken ct) =>
        Results.Ok(await customers.ListAsync(ct));

    private static async Task<IResult> GetCustomerAsync(Guid id, ICustomerService customers, HttpContext http, CancellationToken ct) =>
        ToHttp(await customers.GetAsync(id, ct), http);

    private static async Task<IResult> CreateCustomerAsync(
        CreateCustomerRequest request,
        ICustomerService customers,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(
            await customers.CreateAsync(
                new CreateCustomerCommand(
                    request.Code,
                    request.Name,
                    request.ContactName,
                    request.Email,
                    request.Phone,
                    request.Notes),
                ct),
            http);

    private static async Task<IResult> UpdateCustomerAsync(
        Guid id,
        UpdateCustomerRequest request,
        ICustomerService customers,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(
            await customers.UpdateAsync(
                new UpdateCustomerCommand(
                    id,
                    request.Name,
                    request.ContactName,
                    request.Email,
                    request.Phone,
                    request.Notes,
                    request.IsActive,
                    request.ExpectedRowVersion),
                ct),
            http);

    private static async Task<IResult> ListSalesAsync(ISalesService sales, string? status, CancellationToken ct) =>
        Results.Ok(await sales.ListAsync(status, ct));

    private static async Task<IResult> GetSaleAsync(Guid id, ISalesService sales, HttpContext http, CancellationToken ct) =>
        ToHttp(await sales.GetAsync(id, ct), http);

    private static async Task<IResult> CreateSaleAsync(
        CreateSalesOrderRequest request,
        ISalesService sales,
        HttpContext http,
        CancellationToken ct)
    {
        var lines = request.Lines.Select(l => new SalesOrderLineRequest(l.ProductId, l.Quantity, l.UnitPrice, l.Notes)).ToList();
        return ToHttp(
            await sales.CreateAsync(
                new CreateSalesOrderCommand(
                    request.CustomerId,
                    request.WarehouseId,
                    request.LocationId,
                    request.Reference,
                    request.Notes,
                    lines),
                ct),
            http);
    }

    private static async Task<IResult> ConfirmAsync(
        Guid id,
        MutatingSalesRequest request,
        ISalesService sales,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(
            await sales.ConfirmAsync(
                new ConfirmSalesOrderCommand(id, request.ExpectedRowVersion, request.OperationId),
                ct),
            http);

    private static async Task<IResult> ShipAsync(
        Guid id,
        ShipSalesRequest request,
        ISalesService sales,
        HttpContext http,
        CancellationToken ct)
    {
        var lines = request.Lines.Select(l => new ShipSalesLineRequest(l.SalesOrderLineId, l.Quantity)).ToList();
        return ToHttp(
            await sales.ShipAsync(
                new ShipSalesOrderCommand(
                    id,
                    request.ExpectedRowVersion,
                    request.OperationId,
                    request.WarehouseId,
                    request.LocationId,
                    request.Notes,
                    lines),
                ct),
            http);
    }

    private static async Task<IResult> ReturnAsync(
        Guid id,
        ReturnSalesRequest request,
        ISalesService sales,
        HttpContext http,
        CancellationToken ct)
    {
        var lines = request.Lines.Select(l => new ReturnSalesLineRequest(l.SalesOrderLineId, l.Quantity)).ToList();
        return ToHttp(
            await sales.ReturnAsync(
                new ReturnSalesOrderCommand(
                    id,
                    request.ExpectedRowVersion,
                    request.OperationId,
                    request.WarehouseId,
                    request.LocationId,
                    request.Notes,
                    lines),
                ct),
            http);
    }

    private static async Task<IResult> CloseAsync(
        Guid id,
        MutatingSalesRequest request,
        ISalesService sales,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(
            await sales.CloseAsync(
                new CloseSalesOrderCommand(id, request.ExpectedRowVersion, request.OperationId),
                ct),
            http);

    private static async Task<IResult> CancelAsync(
        Guid id,
        MutatingSalesRequest request,
        ISalesService sales,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(
            await sales.CancelAsync(
                new CancelSalesOrderCommand(id, request.ExpectedRowVersion, request.OperationId),
                ct),
            http);

    private static async Task<IResult> UpdatePaymentAsync(
        Guid id,
        UpdateSalesPaymentRequest request,
        ISalesService sales,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(
            await sales.UpdatePaymentAsync(
                new UpdateSalesPaymentCommand(id, request.PaidAmount, request.ExpectedRowVersion, request.OperationId),
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

    public sealed record CreateCustomerRequest(
        string Code,
        string Name,
        string? ContactName,
        string? Email,
        string? Phone,
        string? Notes);

    public sealed record UpdateCustomerRequest(
        string Name,
        string? ContactName,
        string? Email,
        string? Phone,
        string? Notes,
        bool IsActive,
        int ExpectedRowVersion);

    public sealed record CreateSalesOrderRequest(
        Guid CustomerId,
        Guid WarehouseId,
        Guid? LocationId,
        string? Reference,
        string? Notes,
        IReadOnlyList<SalesLineDto> Lines);

    public sealed record SalesLineDto(Guid ProductId, decimal Quantity, decimal? UnitPrice, string? Notes);

    public sealed record MutatingSalesRequest(int ExpectedRowVersion, Guid? OperationId);

    public sealed record ShipSalesRequest(
        int ExpectedRowVersion,
        Guid? OperationId,
        Guid? WarehouseId,
        Guid? LocationId,
        string? Notes,
        IReadOnlyList<ShipSalesLineDto> Lines);

    public sealed record ShipSalesLineDto(Guid SalesOrderLineId, decimal Quantity);

    public sealed record ReturnSalesRequest(
        int ExpectedRowVersion,
        Guid? OperationId,
        Guid? WarehouseId,
        Guid? LocationId,
        string? Notes,
        IReadOnlyList<ReturnSalesLineDto> Lines);

    public sealed record ReturnSalesLineDto(Guid SalesOrderLineId, decimal Quantity);

    public sealed record UpdateSalesPaymentRequest(decimal PaidAmount, int ExpectedRowVersion, Guid? OperationId);
}

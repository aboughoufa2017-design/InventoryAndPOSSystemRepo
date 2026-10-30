using Warehouse.Api.Authorization;
using Warehouse.Api.Http;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Purchasing;
using Warehouse.Shared.Api;

namespace Warehouse.Api.Endpoints;

public static class PurchasingEndpoints
{
    public static IEndpointRouteBuilder MapPurchasingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var suppliers = endpoints.MapGroup("/api/v1/suppliers").WithTags("Suppliers").RequireAuthorization().RequirePurchasing();
        suppliers.MapGet("/", ListSuppliersAsync).RequirePermission("purchases.create").WithName("ListSuppliers");
        suppliers.MapGet("/{id:guid}", GetSupplierAsync).RequirePermission("purchases.create").WithName("GetSupplier");
        suppliers.MapPost("/", CreateSupplierAsync).RequirePermission("purchases.create").WithName("CreateSupplier");
        suppliers.MapPut("/{id:guid}", UpdateSupplierAsync).RequirePermission("purchases.create").WithName("UpdateSupplier");

        var purchases = endpoints.MapGroup("/api/v1/purchases").WithTags("Purchases").RequireAuthorization().RequirePurchasing();
        purchases.MapGet("/", ListPurchasesAsync).RequirePermission("purchases.create").WithName("ListPurchaseOrders");
        purchases.MapGet("/{id:guid}", GetPurchaseAsync).RequirePermission("purchases.create").WithName("GetPurchaseOrder");
        purchases.MapPost("/", CreatePurchaseAsync).RequirePermission("purchases.create").WithName("CreatePurchaseOrder");
        purchases.MapPost("/{id:guid}/confirm", ConfirmAsync).RequirePermission("purchases.approve").WithName("ConfirmPurchaseOrder");
        purchases.MapPost("/{id:guid}/receive", ReceiveAsync).RequirePermission("inventory.receive").WithName("ReceivePurchaseOrder");
        purchases.MapPost("/{id:guid}/returns", ReturnAsync).RequirePermission("purchases.approve").WithName("ReturnPurchaseOrder");
        purchases.MapPost("/{id:guid}/close", CloseAsync).RequirePermission("purchases.approve").WithName("ClosePurchaseOrder");
        purchases.MapPost("/{id:guid}/cancel", CancelAsync).RequirePermission("purchases.approve").WithName("CancelPurchaseOrder");
        purchases.MapPut("/{id:guid}/payment", UpdatePaymentAsync).RequirePermission("purchases.approve").WithName("UpdatePurchasePayment");

        return endpoints;
    }

    private static async Task<IResult> ListSuppliersAsync(ISupplierService suppliers, CancellationToken ct) =>
        Results.Ok(await suppliers.ListAsync(ct));

    private static async Task<IResult> GetSupplierAsync(Guid id, ISupplierService suppliers, HttpContext http, CancellationToken ct) =>
        ToHttp(await suppliers.GetAsync(id, ct), http);

    private static async Task<IResult> CreateSupplierAsync(
        CreateSupplierRequest request,
        ISupplierService suppliers,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(
            await suppliers.CreateAsync(
                new CreateSupplierCommand(
                    request.Code,
                    request.Name,
                    request.ContactName,
                    request.Email,
                    request.Phone,
                    request.Notes),
                ct),
            http);

    private static async Task<IResult> UpdateSupplierAsync(
        Guid id,
        UpdateSupplierRequest request,
        ISupplierService suppliers,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(
            await suppliers.UpdateAsync(
                new UpdateSupplierCommand(
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

    private static async Task<IResult> ListPurchasesAsync(IPurchasingService purchasing, string? status, CancellationToken ct) =>
        Results.Ok(await purchasing.ListAsync(status, ct));

    private static async Task<IResult> GetPurchaseAsync(Guid id, IPurchasingService purchasing, HttpContext http, CancellationToken ct) =>
        ToHttp(await purchasing.GetAsync(id, ct), http);

    private static async Task<IResult> CreatePurchaseAsync(
        CreatePurchaseOrderRequest request,
        IPurchasingService purchasing,
        HttpContext http,
        CancellationToken ct)
    {
        var lines = request.Lines.Select(l => new PurchaseOrderLineRequest(l.ProductId, l.Quantity, l.UnitPrice, l.Notes)).ToList();
        return ToHttp(
            await purchasing.CreateAsync(
                new CreatePurchaseOrderCommand(
                    request.SupplierId,
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
        MutatingPurchaseRequest request,
        IPurchasingService purchasing,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(
            await purchasing.ConfirmAsync(
                new ConfirmPurchaseOrderCommand(id, request.ExpectedRowVersion, request.OperationId),
                ct),
            http);

    private static async Task<IResult> ReceiveAsync(
        Guid id,
        ReceivePurchaseRequest request,
        IPurchasingService purchasing,
        HttpContext http,
        CancellationToken ct)
    {
        var lines = request.Lines.Select(l => new ReceivePurchaseLineRequest(l.PurchaseOrderLineId, l.Quantity)).ToList();
        return ToHttp(
            await purchasing.ReceiveAsync(
                new ReceivePurchaseOrderCommand(
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
        ReturnPurchaseRequest request,
        IPurchasingService purchasing,
        HttpContext http,
        CancellationToken ct)
    {
        var lines = request.Lines.Select(l => new ReturnPurchaseLineRequest(l.PurchaseOrderLineId, l.Quantity)).ToList();
        return ToHttp(
            await purchasing.ReturnAsync(
                new ReturnPurchaseOrderCommand(
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
        MutatingPurchaseRequest request,
        IPurchasingService purchasing,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(
            await purchasing.CloseAsync(
                new ClosePurchaseOrderCommand(id, request.ExpectedRowVersion, request.OperationId),
                ct),
            http);

    private static async Task<IResult> CancelAsync(
        Guid id,
        MutatingPurchaseRequest request,
        IPurchasingService purchasing,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(
            await purchasing.CancelAsync(
                new CancelPurchaseOrderCommand(id, request.ExpectedRowVersion, request.OperationId),
                ct),
            http);

    private static async Task<IResult> UpdatePaymentAsync(
        Guid id,
        UpdatePaymentRequest request,
        IPurchasingService purchasing,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(
            await purchasing.UpdatePaymentAsync(
                new UpdatePurchasePaymentCommand(id, request.PaidAmount, request.ExpectedRowVersion, request.OperationId),
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

    public sealed record CreateSupplierRequest(
        string Code,
        string Name,
        string? ContactName,
        string? Email,
        string? Phone,
        string? Notes);

    public sealed record UpdateSupplierRequest(
        string Name,
        string? ContactName,
        string? Email,
        string? Phone,
        string? Notes,
        bool IsActive,
        int ExpectedRowVersion);

    public sealed record CreatePurchaseOrderRequest(
        Guid SupplierId,
        Guid WarehouseId,
        Guid? LocationId,
        string? Reference,
        string? Notes,
        IReadOnlyList<PurchaseLineDto> Lines);

    public sealed record PurchaseLineDto(Guid ProductId, decimal Quantity, decimal? UnitPrice, string? Notes);

    public sealed record MutatingPurchaseRequest(int ExpectedRowVersion, Guid? OperationId);

    public sealed record ReceivePurchaseRequest(
        int ExpectedRowVersion,
        Guid? OperationId,
        Guid? WarehouseId,
        Guid? LocationId,
        string? Notes,
        IReadOnlyList<ReceivePurchaseLineDto> Lines);

    public sealed record ReceivePurchaseLineDto(Guid PurchaseOrderLineId, decimal Quantity);

    public sealed record ReturnPurchaseRequest(
        int ExpectedRowVersion,
        Guid? OperationId,
        Guid? WarehouseId,
        Guid? LocationId,
        string? Notes,
        IReadOnlyList<ReturnPurchaseLineDto> Lines);

    public sealed record ReturnPurchaseLineDto(Guid PurchaseOrderLineId, decimal Quantity);

    public sealed record UpdatePaymentRequest(decimal PaidAmount, int ExpectedRowVersion, Guid? OperationId);
}

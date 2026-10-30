using Warehouse.Api.Authorization;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Catalog;
using Warehouse.Api.Http;
using Warehouse.Shared.Api;

namespace Warehouse.Api.Endpoints;

public static class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var units = endpoints.MapGroup("/api/v1/units").WithTags("Units").RequireAuthorization().RequireCatalog();
        units.MapGet("/", ListUnitsAsync).RequirePermission("products.read").WithName("ListUnits");
        units.MapPost("/", CreateUnitAsync).RequirePermission("products.create").WithName("CreateUnit");

        var categories = endpoints.MapGroup("/api/v1/categories").WithTags("Categories").RequireAuthorization().RequireCatalog();
        categories.MapGet("/", ListCategoriesAsync).RequirePermission("products.read").WithName("ListCategories");
        categories.MapPost("/", CreateCategoryAsync).RequirePermission("products.create").WithName("CreateCategory");
        categories.MapPut("/{id:guid}", UpdateCategoryAsync).RequirePermission("products.update").WithName("UpdateCategory");

        var products = endpoints.MapGroup("/api/v1/products").WithTags("Products").RequireAuthorization().RequireCatalog();
        products.MapGet("/", ListProductsAsync).RequirePermission("products.read").WithName("ListProducts");
        products.MapGet("/{id:guid}", GetProductAsync).RequirePermission("products.read").WithName("GetProduct");
        products.MapGet("/by-sku/{sku}", GetProductBySkuAsync).RequirePermission("products.read").WithName("GetProductBySku");
        products.MapGet("/by-barcode/{barcode}", GetProductByBarcodeAsync).RequirePermission("products.read").WithName("GetProductByBarcode");
        products.MapPost("/", CreateProductAsync).RequirePermission("products.create").WithName("CreateProduct");
        products.MapPut("/{id:guid}", UpdateProductAsync).RequirePermission("products.update").WithName("UpdateProduct");
        products.MapPost("/{id:guid}/barcodes", AddBarcodeAsync).RequirePermission("products.update").WithName("AddProductBarcode");
        products.MapDelete("/{id:guid}/barcodes/{barcodeId:guid}", RemoveBarcodeAsync)
            .RequirePermission("products.update")
            .WithName("RemoveProductBarcode");

        return endpoints;
    }

    private static async Task<IResult> ListUnitsAsync(ICatalogService catalog, CancellationToken ct) =>
        Results.Ok(await catalog.ListUnitsAsync(ct));

    private static async Task<IResult> CreateUnitAsync(
        CreateUnitRequest request,
        ICatalogService catalog,
        HttpContext http,
        CancellationToken ct)
    {
        var result = await catalog.CreateUnitAsync(new CreateUnitOfMeasureCommand(request.Code, request.Name), ct);
        return ToHttp(result, http);
    }

    private static async Task<IResult> ListCategoriesAsync(ICatalogService catalog, CancellationToken ct) =>
        Results.Ok(await catalog.ListCategoriesAsync(ct));

    private static async Task<IResult> CreateCategoryAsync(
        CreateCategoryRequest request,
        ICatalogService catalog,
        HttpContext http,
        CancellationToken ct)
    {
        var result = await catalog.CreateCategoryAsync(
            new CreateProductCategoryCommand(request.Code, request.Name, request.ParentCategoryId), ct);
        return ToHttp(result, http);
    }

    private static async Task<IResult> UpdateCategoryAsync(
        Guid id,
        UpdateCategoryRequest request,
        ICatalogService catalog,
        HttpContext http,
        CancellationToken ct)
    {
        var result = await catalog.UpdateCategoryAsync(
            new UpdateProductCategoryCommand(id, request.Name, request.ParentCategoryId, request.IsActive), ct);
        return ToHttp(result, http);
    }

    private static async Task<IResult> ListProductsAsync(
        ICatalogService catalog,
        string? search,
        bool? activeOnly,
        CancellationToken ct) =>
        Results.Ok(await catalog.ListProductsAsync(search, activeOnly, ct));

    private static async Task<IResult> GetProductAsync(
        Guid id,
        ICatalogService catalog,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(await catalog.GetProductAsync(id, ct), http);

    private static async Task<IResult> GetProductBySkuAsync(
        string sku,
        ICatalogService catalog,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(await catalog.GetProductBySkuAsync(sku, ct), http);

    private static async Task<IResult> GetProductByBarcodeAsync(
        string barcode,
        ICatalogService catalog,
        HttpContext http,
        CancellationToken ct) =>
        ToHttp(await catalog.GetProductByBarcodeAsync(barcode, ct), http);

    private static async Task<IResult> CreateProductAsync(
        CreateProductRequest request,
        ICatalogService catalog,
        HttpContext http,
        CancellationToken ct)
    {
        var result = await catalog.CreateProductAsync(
            new CreateProductCommand(
                request.Sku,
                request.Name,
                request.UnitOfMeasureId,
                request.PurchasePrice,
                request.SalePrice,
                request.IsTaxable,
                request.TrackInventory,
                request.CategoryId,
                request.Description,
                request.Brand,
                request.MinimumStock,
                request.MaximumStock,
                request.ReorderPoint,
                request.Barcodes),
            ct);
        return ToHttp(result, http);
    }

    private static async Task<IResult> UpdateProductAsync(
        Guid id,
        UpdateProductRequest request,
        ICatalogService catalog,
        HttpContext http,
        CancellationToken ct)
    {
        var result = await catalog.UpdateProductAsync(
            new UpdateProductCommand(
                id,
                request.Name,
                request.UnitOfMeasureId,
                request.PurchasePrice,
                request.SalePrice,
                request.IsTaxable,
                request.TrackInventory,
                request.IsActive,
                request.CategoryId,
                request.Description,
                request.Brand,
                request.MinimumStock,
                request.MaximumStock,
                request.ReorderPoint,
                request.ExpectedRowVersion),
            ct);
        return ToHttp(result, http);
    }

    private static async Task<IResult> AddBarcodeAsync(
        Guid id,
        AddBarcodeRequest request,
        ICatalogService catalog,
        HttpContext http,
        CancellationToken ct)
    {
        var result = await catalog.AddBarcodeAsync(
            new AddProductBarcodeCommand(id, request.Barcode, request.IsPrimary), ct);
        return ToHttp(result, http);
    }

    private static async Task<IResult> RemoveBarcodeAsync(
        Guid id,
        Guid barcodeId,
        ICatalogService catalog,
        HttpContext http,
        CancellationToken ct)
    {
        var result = await catalog.RemoveBarcodeAsync(id, barcodeId, ct);
        if (!result.IsSuccess)
        {
            return Results.Json(
                new ApiErrorResponse(result.ErrorCode!, result.ErrorMessage!, CorrelationId(http)),
                statusCode: StatusFromCode(result.ErrorCode!));
        }

        return Results.NoContent();
    }

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
        httpContext.Items[CorrelationIdMiddleware.ItemKey] as string ?? "unknown";

    private static int StatusFromCode(string code) => code switch
    {
        ApiErrorCodes.Unauthorized => StatusCodes.Status401Unauthorized,
        ApiErrorCodes.Forbidden => StatusCodes.Status403Forbidden,
        ApiErrorCodes.ProductNotFound => StatusCodes.Status404NotFound,
        ApiErrorCodes.SyncConflict => StatusCodes.Status409Conflict,
        ApiErrorCodes.ValidationFailed => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status400BadRequest,
    };

    public sealed record CreateUnitRequest(string Code, string Name);

    public sealed record CreateCategoryRequest(string Code, string Name, Guid? ParentCategoryId);

    public sealed record UpdateCategoryRequest(string Name, Guid? ParentCategoryId, bool IsActive);

    public sealed record CreateProductRequest(
        string Sku,
        string Name,
        Guid UnitOfMeasureId,
        decimal PurchasePrice,
        decimal SalePrice,
        bool IsTaxable,
        bool TrackInventory,
        Guid? CategoryId,
        string? Description,
        string? Brand,
        decimal? MinimumStock,
        decimal? MaximumStock,
        decimal? ReorderPoint,
        IReadOnlyList<string>? Barcodes);

    public sealed record UpdateProductRequest(
        string Name,
        Guid UnitOfMeasureId,
        decimal PurchasePrice,
        decimal SalePrice,
        bool IsTaxable,
        bool TrackInventory,
        bool IsActive,
        Guid? CategoryId,
        string? Description,
        string? Brand,
        decimal? MinimumStock,
        decimal? MaximumStock,
        decimal? ReorderPoint,
        int ExpectedRowVersion);

    public sealed record AddBarcodeRequest(string Barcode, bool IsPrimary);
}

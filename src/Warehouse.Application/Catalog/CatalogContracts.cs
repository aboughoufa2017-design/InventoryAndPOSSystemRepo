using Warehouse.Application.Abstractions;

namespace Warehouse.Application.Catalog;

public sealed record UnitOfMeasureResponse(Guid Id, string Code, string Name, bool IsSystem, bool IsActive);

public sealed record ProductCategoryResponse(
    Guid Id,
    string Code,
    string Name,
    Guid? ParentCategoryId,
    bool IsActive);

public sealed record ProductBarcodeResponse(Guid Id, string Barcode, bool IsPrimary);

public sealed record ProductResponse(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    string? Brand,
    Guid? CategoryId,
    Guid UnitOfMeasureId,
    string UnitOfMeasureCode,
    decimal PurchasePrice,
    decimal SalePrice,
    bool IsTaxable,
    bool IsActive,
    bool TrackInventory,
    decimal? MinimumStock,
    decimal? MaximumStock,
    decimal? ReorderPoint,
    decimal ApplicableTaxRate,
    IReadOnlyList<ProductBarcodeResponse> Barcodes,
    int RowVersion);

public sealed record CreateUnitOfMeasureCommand(string Code, string Name);

public sealed record CreateProductCategoryCommand(string Code, string Name, Guid? ParentCategoryId);

public sealed record UpdateProductCategoryCommand(Guid Id, string Name, Guid? ParentCategoryId, bool IsActive);

public sealed record CreateProductCommand(
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

public sealed record UpdateProductCommand(
    Guid Id,
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

public sealed record AddProductBarcodeCommand(Guid ProductId, string Barcode, bool IsPrimary);

public interface ICatalogService
{
    Task<IReadOnlyList<UnitOfMeasureResponse>> ListUnitsAsync(CancellationToken cancellationToken);

    Task<AppResult<UnitOfMeasureResponse>> CreateUnitAsync(
        CreateUnitOfMeasureCommand command,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductCategoryResponse>> ListCategoriesAsync(CancellationToken cancellationToken);

    Task<AppResult<ProductCategoryResponse>> CreateCategoryAsync(
        CreateProductCategoryCommand command,
        CancellationToken cancellationToken);

    Task<AppResult<ProductCategoryResponse>> UpdateCategoryAsync(
        UpdateProductCategoryCommand command,
        CancellationToken cancellationToken);

    Task<AppResult<ProductResponse>> GetProductAsync(Guid productId, CancellationToken cancellationToken);

    Task<AppResult<ProductResponse>> GetProductBySkuAsync(string sku, CancellationToken cancellationToken);

    Task<AppResult<ProductResponse>> GetProductByBarcodeAsync(string barcode, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductResponse>> ListProductsAsync(string? search, bool? activeOnly, CancellationToken cancellationToken);

    Task<AppResult<ProductResponse>> CreateProductAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken);

    Task<AppResult<ProductResponse>> UpdateProductAsync(
        UpdateProductCommand command,
        CancellationToken cancellationToken);

    Task<AppResult<ProductBarcodeResponse>> AddBarcodeAsync(
        AddProductBarcodeCommand command,
        CancellationToken cancellationToken);

    Task<AppResult> RemoveBarcodeAsync(Guid productId, Guid barcodeId, CancellationToken cancellationToken);
}

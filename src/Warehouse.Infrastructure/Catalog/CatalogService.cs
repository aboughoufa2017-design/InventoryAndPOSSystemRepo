using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Catalog;
using Warehouse.Application.Tenancy;
using Warehouse.Domain.Catalog;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Infrastructure.Catalog;

public sealed class CatalogService(WarehouseDbContext db, ITenantContext tenantContext) : ICatalogService
{
    public async Task<IReadOnlyList<UnitOfMeasureResponse>> ListUnitsAsync(CancellationToken cancellationToken)
    {
        EnsureTenant();
        return await db.UnitsOfMeasure
            .AsNoTracking()
            .OrderBy(u => u.Code)
            .Select(u => new UnitOfMeasureResponse(u.Id, u.Code, u.Name, u.IsSystem, u.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<AppResult<UnitOfMeasureResponse>> CreateUnitAsync(
        CreateUnitOfMeasureCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<UnitOfMeasureResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (string.IsNullOrWhiteSpace(command.Code) || string.IsNullOrWhiteSpace(command.Name))
        {
            return AppResult<UnitOfMeasureResponse>.Failure(ErrorCodes.ValidationFailed, "Code and name are required.");
        }

        var code = command.Code.Trim().ToUpperInvariant();
        if (await db.UnitsOfMeasure.AnyAsync(u => u.Code == code, cancellationToken))
        {
            return AppResult<UnitOfMeasureResponse>.Failure(ErrorCodes.InvalidOperation, "Unit code already exists.");
        }

        var unit = UnitOfMeasure.Create(Guid.NewGuid(), tenantId, code, command.Name, isSystem: false, DateTimeOffset.UtcNow);
        db.UnitsOfMeasure.Add(unit);
        await db.SaveChangesAsync(cancellationToken);
        return AppResult<UnitOfMeasureResponse>.Success(
            new UnitOfMeasureResponse(unit.Id, unit.Code, unit.Name, unit.IsSystem, unit.IsActive));
    }

    public async Task<IReadOnlyList<ProductCategoryResponse>> ListCategoriesAsync(CancellationToken cancellationToken)
    {
        EnsureTenant();
        return await db.ProductCategories
            .AsNoTracking()
            .OrderBy(c => c.Code)
            .Select(c => new ProductCategoryResponse(c.Id, c.Code, c.Name, c.ParentCategoryId, c.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<AppResult<ProductCategoryResponse>> CreateCategoryAsync(
        CreateProductCategoryCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<ProductCategoryResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (string.IsNullOrWhiteSpace(command.Code) || string.IsNullOrWhiteSpace(command.Name))
        {
            return AppResult<ProductCategoryResponse>.Failure(ErrorCodes.ValidationFailed, "Code and name are required.");
        }

        if (command.ParentCategoryId is Guid parentId
            && !await db.ProductCategories.AnyAsync(c => c.Id == parentId, cancellationToken))
        {
            return AppResult<ProductCategoryResponse>.Failure(ErrorCodes.ValidationFailed, "Parent category was not found.");
        }

        var code = command.Code.Trim().ToUpperInvariant();
        if (await db.ProductCategories.AnyAsync(c => c.Code == code, cancellationToken))
        {
            return AppResult<ProductCategoryResponse>.Failure(ErrorCodes.InvalidOperation, "Category code already exists.");
        }

        var category = ProductCategory.Create(
            Guid.NewGuid(), tenantId, code, command.Name, command.ParentCategoryId, DateTimeOffset.UtcNow);
        db.ProductCategories.Add(category);
        await db.SaveChangesAsync(cancellationToken);
        return AppResult<ProductCategoryResponse>.Success(
            new ProductCategoryResponse(category.Id, category.Code, category.Name, category.ParentCategoryId, category.IsActive));
    }

    public async Task<AppResult<ProductCategoryResponse>> UpdateCategoryAsync(
        UpdateProductCategoryCommand command,
        CancellationToken cancellationToken)
    {
        EnsureTenant();
        var category = await db.ProductCategories.FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);
        if (category is null)
        {
            return AppResult<ProductCategoryResponse>.Failure(ErrorCodes.ProductNotFound, "Category was not found.");
        }

        if (command.ParentCategoryId is Guid parentId)
        {
            if (parentId == category.Id)
            {
                return AppResult<ProductCategoryResponse>.Failure(ErrorCodes.ValidationFailed, "Category cannot be its own parent.");
            }

            if (!await db.ProductCategories.AnyAsync(c => c.Id == parentId, cancellationToken))
            {
                return AppResult<ProductCategoryResponse>.Failure(ErrorCodes.ValidationFailed, "Parent category was not found.");
            }
        }

        try
        {
            category.Update(command.Name, command.ParentCategoryId, DateTimeOffset.UtcNow);
            category.SetActive(command.IsActive, DateTimeOffset.UtcNow);
        }
        catch (ArgumentException ex)
        {
            return AppResult<ProductCategoryResponse>.Failure(ErrorCodes.ValidationFailed, ex.Message);
        }

        await db.SaveChangesAsync(cancellationToken);
        return AppResult<ProductCategoryResponse>.Success(
            new ProductCategoryResponse(category.Id, category.Code, category.Name, category.ParentCategoryId, category.IsActive));
    }

    public async Task<AppResult<ProductResponse>> GetProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        EnsureTenant();
        var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product is null)
        {
            return AppResult<ProductResponse>.Failure(ErrorCodes.ProductNotFound, "Product was not found.");
        }

        return AppResult<ProductResponse>.Success(await MapProductAsync(product, cancellationToken));
    }

    public async Task<AppResult<ProductResponse>> GetProductBySkuAsync(string sku, CancellationToken cancellationToken)
    {
        EnsureTenant();
        if (string.IsNullOrWhiteSpace(sku))
        {
            return AppResult<ProductResponse>.Failure(ErrorCodes.ValidationFailed, "SKU is required.");
        }

        var normalized = sku.Trim().ToUpperInvariant();
        var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Sku == normalized, cancellationToken);
        if (product is null)
        {
            return AppResult<ProductResponse>.Failure(ErrorCodes.ProductNotFound, "Product was not found.");
        }

        return AppResult<ProductResponse>.Success(await MapProductAsync(product, cancellationToken));
    }

    public async Task<AppResult<ProductResponse>> GetProductByBarcodeAsync(string barcode, CancellationToken cancellationToken)
    {
        EnsureTenant();
        if (string.IsNullOrWhiteSpace(barcode))
        {
            return AppResult<ProductResponse>.Failure(ErrorCodes.ValidationFailed, "Barcode is required.");
        }

        var normalized = barcode.Trim();
        var link = await db.ProductBarcodes.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Barcode == normalized, cancellationToken);
        if (link is null)
        {
            return AppResult<ProductResponse>.Failure(ErrorCodes.ProductNotFound, "Product was not found.");
        }

        var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == link.ProductId, cancellationToken);
        if (product is null)
        {
            return AppResult<ProductResponse>.Failure(ErrorCodes.ProductNotFound, "Product was not found.");
        }

        return AppResult<ProductResponse>.Success(await MapProductAsync(product, cancellationToken));
    }

    public async Task<IReadOnlyList<ProductResponse>> ListProductsAsync(
        string? search,
        bool? activeOnly,
        CancellationToken cancellationToken)
    {
        EnsureTenant();
        var query = db.Products.AsNoTracking().AsQueryable();
        if (activeOnly == true)
        {
            query = query.Where(p => p.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToUpperInvariant();
            var matchingProductIds = await db.ProductBarcodes.AsNoTracking()
                .Where(b => b.Barcode.Contains(search.Trim()))
                .Select(b => b.ProductId)
                .ToListAsync(cancellationToken);

            query = query.Where(p =>
                p.Sku.Contains(term)
                || p.Name.ToUpper().Contains(term)
                || (p.Brand != null && p.Brand.ToUpper().Contains(term))
                || matchingProductIds.Contains(p.Id));
        }

        var products = await query.OrderBy(p => p.Sku).Take(200).ToListAsync(cancellationToken);
        var result = new List<ProductResponse>(products.Count);
        foreach (var product in products)
        {
            result.Add(await MapProductAsync(product, cancellationToken));
        }

        return result;
    }

    public async Task<AppResult<ProductResponse>> CreateProductAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<ProductResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (string.IsNullOrWhiteSpace(command.Sku) || string.IsNullOrWhiteSpace(command.Name))
        {
            return AppResult<ProductResponse>.Failure(ErrorCodes.ValidationFailed, "SKU and name are required.");
        }

        var sku = command.Sku.Trim().ToUpperInvariant();
        if (await db.Products.AnyAsync(p => p.Sku == sku, cancellationToken))
        {
            return AppResult<ProductResponse>.Failure(ErrorCodes.InvalidOperation, "SKU already exists.");
        }

        var unit = await db.UnitsOfMeasure.FirstOrDefaultAsync(u => u.Id == command.UnitOfMeasureId && u.IsActive, cancellationToken);
        if (unit is null)
        {
            return AppResult<ProductResponse>.Failure(ErrorCodes.ValidationFailed, "Unit of measure was not found or is inactive.");
        }

        if (command.CategoryId is Guid categoryId
            && !await db.ProductCategories.AnyAsync(c => c.Id == categoryId && c.IsActive, cancellationToken))
        {
            return AppResult<ProductResponse>.Failure(ErrorCodes.ValidationFailed, "Category was not found or is inactive.");
        }

        var barcodes = NormalizeBarcodes(command.Barcodes);
        foreach (var barcode in barcodes)
        {
            if (await db.ProductBarcodes.AnyAsync(b => b.Barcode == barcode, cancellationToken))
            {
                return AppResult<ProductResponse>.Failure(ErrorCodes.InvalidOperation, $"Barcode '{barcode}' already exists.");
            }
        }

        Product product;
        try
        {
            product = Product.Create(
                Guid.NewGuid(),
                tenantId,
                sku,
                command.Name,
                command.UnitOfMeasureId,
                command.PurchasePrice,
                command.SalePrice,
                command.IsTaxable,
                command.TrackInventory,
                command.CategoryId,
                command.Description,
                command.Brand,
                command.MinimumStock,
                command.MaximumStock,
                command.ReorderPoint,
                DateTimeOffset.UtcNow);
        }
        catch (ArgumentException ex)
        {
            return AppResult<ProductResponse>.Failure(ErrorCodes.ValidationFailed, ex.Message);
        }

        db.Products.Add(product);
        for (var i = 0; i < barcodes.Count; i++)
        {
            db.ProductBarcodes.Add(ProductBarcode.Create(
                Guid.NewGuid(),
                tenantId,
                product.Id,
                barcodes[i],
                isPrimary: i == 0,
                DateTimeOffset.UtcNow));
        }

        await db.SaveChangesAsync(cancellationToken);
        return AppResult<ProductResponse>.Success(await MapProductAsync(product, cancellationToken));
    }

    public async Task<AppResult<ProductResponse>> UpdateProductAsync(
        UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        EnsureTenant();
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);
        if (product is null)
        {
            return AppResult<ProductResponse>.Failure(ErrorCodes.ProductNotFound, "Product was not found.");
        }

        if (product.RowVersion != command.ExpectedRowVersion)
        {
            return AppResult<ProductResponse>.Failure(ErrorCodes.SyncConflict, "Product was modified by another request.");
        }

        var unit = await db.UnitsOfMeasure.FirstOrDefaultAsync(u => u.Id == command.UnitOfMeasureId && u.IsActive, cancellationToken);
        if (unit is null)
        {
            return AppResult<ProductResponse>.Failure(ErrorCodes.ValidationFailed, "Unit of measure was not found or is inactive.");
        }

        if (command.CategoryId is Guid categoryId
            && !await db.ProductCategories.AnyAsync(c => c.Id == categoryId, cancellationToken))
        {
            return AppResult<ProductResponse>.Failure(ErrorCodes.ValidationFailed, "Category was not found.");
        }

        try
        {
            product.Update(
                command.Name,
                command.UnitOfMeasureId,
                command.PurchasePrice,
                command.SalePrice,
                command.IsTaxable,
                command.TrackInventory,
                command.CategoryId,
                command.Description,
                command.Brand,
                command.MinimumStock,
                command.MaximumStock,
                command.ReorderPoint,
                DateTimeOffset.UtcNow);
            product.SetActive(command.IsActive, DateTimeOffset.UtcNow);
        }
        catch (ArgumentException ex)
        {
            return AppResult<ProductResponse>.Failure(ErrorCodes.ValidationFailed, ex.Message);
        }

        await db.SaveChangesAsync(cancellationToken);
        return AppResult<ProductResponse>.Success(await MapProductAsync(product, cancellationToken));
    }

    public async Task<AppResult<ProductBarcodeResponse>> AddBarcodeAsync(
        AddProductBarcodeCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<ProductBarcodeResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (string.IsNullOrWhiteSpace(command.Barcode))
        {
            return AppResult<ProductBarcodeResponse>.Failure(ErrorCodes.ValidationFailed, "Barcode is required.");
        }

        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == command.ProductId, cancellationToken);
        if (product is null)
        {
            return AppResult<ProductBarcodeResponse>.Failure(ErrorCodes.ProductNotFound, "Product was not found.");
        }

        var barcode = command.Barcode.Trim();
        if (await db.ProductBarcodes.AnyAsync(b => b.Barcode == barcode, cancellationToken))
        {
            return AppResult<ProductBarcodeResponse>.Failure(ErrorCodes.InvalidOperation, "Barcode already exists.");
        }

        if (command.IsPrimary)
        {
            var existing = await db.ProductBarcodes.Where(b => b.ProductId == product.Id).ToListAsync(cancellationToken);
            foreach (var item in existing)
            {
                item.SetPrimary(false);
            }
        }

        var entity = ProductBarcode.Create(
            Guid.NewGuid(), tenantId, product.Id, barcode, command.IsPrimary, DateTimeOffset.UtcNow);
        db.ProductBarcodes.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return AppResult<ProductBarcodeResponse>.Success(
            new ProductBarcodeResponse(entity.Id, entity.Barcode, entity.IsPrimary));
    }

    public async Task<AppResult> RemoveBarcodeAsync(Guid productId, Guid barcodeId, CancellationToken cancellationToken)
    {
        EnsureTenant();
        var entity = await db.ProductBarcodes
            .FirstOrDefaultAsync(b => b.Id == barcodeId && b.ProductId == productId, cancellationToken);
        if (entity is null)
        {
            return AppResult.Failure(ErrorCodes.ProductNotFound, "Barcode was not found.");
        }

        db.ProductBarcodes.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return AppResult.Success();
    }

    private async Task<ProductResponse> MapProductAsync(Product product, CancellationToken cancellationToken)
    {
        var unitCode = await db.UnitsOfMeasure.AsNoTracking()
            .Where(u => u.Id == product.UnitOfMeasureId)
            .Select(u => u.Code)
            .FirstAsync(cancellationToken);

        var barcodes = await db.ProductBarcodes.AsNoTracking()
            .Where(b => b.ProductId == product.Id)
            .OrderByDescending(b => b.IsPrimary)
            .ThenBy(b => b.Barcode)
            .Select(b => new ProductBarcodeResponse(b.Id, b.Barcode, b.IsPrimary))
            .ToListAsync(cancellationToken);

        var tenant = await db.Tenants.AsNoTracking()
            .FirstAsync(t => t.Id == product.TenantId, cancellationToken);

        return new ProductResponse(
            product.Id,
            product.Sku,
            product.Name,
            product.Description,
            product.Brand,
            product.CategoryId,
            product.UnitOfMeasureId,
            unitCode,
            product.PurchasePrice,
            product.SalePrice,
            product.IsTaxable,
            product.IsActive,
            product.TrackInventory,
            product.MinimumStock,
            product.MaximumStock,
            product.ReorderPoint,
            ProductTaxCalculator.ResolveApplicableRate(tenant, product),
            barcodes,
            product.RowVersion);
    }

    private void EnsureTenant()
    {
        if (tenantContext.TenantId is null)
        {
            throw new InvalidOperationException("Tenant context is required.");
        }
    }

    private static IReadOnlyList<string> NormalizeBarcodes(IReadOnlyList<string>? barcodes)
    {
        if (barcodes is null || barcodes.Count == 0)
        {
            return Array.Empty<string>();
        }

        return barcodes
            .Where(b => !string.IsNullOrWhiteSpace(b))
            .Select(b => b.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }
}

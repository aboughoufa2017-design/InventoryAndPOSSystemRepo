using Warehouse.Domain.Common;
using Warehouse.Domain.Precision;

namespace Warehouse.Domain.Catalog;

/// <summary>
/// TENANT-OWNED sellable/stockable product. One SKU per product in v1.
/// Soft-deactivate only; do not hard-delete when history exists.
/// </summary>
public sealed class Product : TenantOwnedEntity
{
    private Product()
    {
    }

    private Product(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    /// <summary>Tenant-unique SKU / product code.</summary>
    public string Sku { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? Brand { get; private set; }

    public Guid? CategoryId { get; private set; }

    public Guid UnitOfMeasureId { get; private set; }

    /// <summary>Purchase / cost price. Mapped to numeric(19,4).</summary>
    public decimal PurchasePrice { get; private set; }

    /// <summary>Sale price. Mapped to numeric(19,4).</summary>
    public decimal SalePrice { get; private set; }

    /// <summary>
    /// When true and tenant tax is enabled, tenant tax rate applies.
    /// When tenant tax is disabled, tax is always inactive regardless of this flag.
    /// </summary>
    public bool IsTaxable { get; private set; }

    public bool IsActive { get; private set; } = true;

    public bool TrackInventory { get; private set; } = true;

    public decimal? MinimumStock { get; private set; }

    public decimal? MaximumStock { get; private set; }

    public decimal? ReorderPoint { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public int RowVersion { get; private set; } = 1;

    public static Product Create(
        Guid id,
        Guid tenantId,
        string sku,
        string name,
        Guid unitOfMeasureId,
        decimal purchasePrice,
        decimal salePrice,
        bool isTaxable,
        bool trackInventory,
        Guid? categoryId,
        string? description,
        string? brand,
        decimal? minimumStock,
        decimal? maximumStock,
        decimal? reorderPoint,
        DateTimeOffset utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (unitOfMeasureId == Guid.Empty)
        {
            throw new ArgumentException("Unit of measure is required.", nameof(unitOfMeasureId));
        }

        ValidateMoney(purchasePrice, nameof(purchasePrice));
        ValidateMoney(salePrice, nameof(salePrice));
        ValidateOptionalQuantity(minimumStock, nameof(minimumStock));
        ValidateOptionalQuantity(maximumStock, nameof(maximumStock));
        ValidateOptionalQuantity(reorderPoint, nameof(reorderPoint));
        ValidateStockBounds(minimumStock, maximumStock, reorderPoint);

        return new Product(id, tenantId)
        {
            Sku = sku.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = NormalizeOptional(description),
            Brand = NormalizeOptional(brand),
            CategoryId = categoryId,
            UnitOfMeasureId = unitOfMeasureId,
            PurchasePrice = RoundMoney(purchasePrice),
            SalePrice = RoundMoney(salePrice),
            IsTaxable = isTaxable,
            TrackInventory = trackInventory,
            MinimumStock = minimumStock is null ? null : RoundQuantity(minimumStock.Value),
            MaximumStock = maximumStock is null ? null : RoundQuantity(maximumStock.Value),
            ReorderPoint = reorderPoint is null ? null : RoundQuantity(reorderPoint.Value),
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
        };
    }

    public void Update(
        string name,
        Guid unitOfMeasureId,
        decimal purchasePrice,
        decimal salePrice,
        bool isTaxable,
        bool trackInventory,
        Guid? categoryId,
        string? description,
        string? brand,
        decimal? minimumStock,
        decimal? maximumStock,
        decimal? reorderPoint,
        DateTimeOffset utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (unitOfMeasureId == Guid.Empty)
        {
            throw new ArgumentException("Unit of measure is required.", nameof(unitOfMeasureId));
        }

        ValidateMoney(purchasePrice, nameof(purchasePrice));
        ValidateMoney(salePrice, nameof(salePrice));
        ValidateOptionalQuantity(minimumStock, nameof(minimumStock));
        ValidateOptionalQuantity(maximumStock, nameof(maximumStock));
        ValidateOptionalQuantity(reorderPoint, nameof(reorderPoint));
        ValidateStockBounds(minimumStock, maximumStock, reorderPoint);

        Name = name.Trim();
        Description = NormalizeOptional(description);
        Brand = NormalizeOptional(brand);
        CategoryId = categoryId;
        UnitOfMeasureId = unitOfMeasureId;
        PurchasePrice = RoundMoney(purchasePrice);
        SalePrice = RoundMoney(salePrice);
        IsTaxable = isTaxable;
        TrackInventory = trackInventory;
        MinimumStock = minimumStock is null ? null : RoundQuantity(minimumStock.Value);
        MaximumStock = maximumStock is null ? null : RoundQuantity(maximumStock.Value);
        ReorderPoint = reorderPoint is null ? null : RoundQuantity(reorderPoint.Value);
        Touch(utcNow);
    }

    public void SetActive(bool isActive, DateTimeOffset utcNow)
    {
        IsActive = isActive;
        Touch(utcNow);
    }

    private void Touch(DateTimeOffset utcNow)
    {
        UpdatedAtUtc = utcNow;
        RowVersion++;
    }

    private static void ValidateMoney(decimal value, string paramName)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "Money amounts cannot be negative.");
        }
    }

    private static void ValidateOptionalQuantity(decimal? value, string paramName)
    {
        if (value is < 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "Quantities cannot be negative.");
        }
    }

    private static void ValidateStockBounds(decimal? minimum, decimal? maximum, decimal? reorder)
    {
        if (minimum is not null && maximum is not null && minimum > maximum)
        {
            throw new ArgumentException("Minimum stock cannot exceed maximum stock.");
        }

        if (reorder is not null && maximum is not null && reorder > maximum)
        {
            throw new ArgumentException("Reorder point cannot exceed maximum stock.");
        }
    }

    private static decimal RoundMoney(decimal value) =>
        Math.Round(value, MoneyPrecision.Scale, MidpointRounding.AwayFromZero);

    private static decimal RoundQuantity(decimal value) =>
        Math.Round(value, QuantityPrecision.Scale, MidpointRounding.AwayFromZero);

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

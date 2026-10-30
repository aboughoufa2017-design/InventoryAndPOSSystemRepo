using Warehouse.Domain.Common;

namespace Warehouse.Domain.Catalog;

/// <summary>
/// TENANT-OWNED product category.
/// </summary>
public sealed class ProductCategory : TenantOwnedEntity
{
    private ProductCategory()
    {
    }

    private ProductCategory(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public Guid? ParentCategoryId { get; private set; }

    public bool IsActive { get; private set; } = true;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public int RowVersion { get; private set; } = 1;

    public static ProductCategory Create(
        Guid id,
        Guid tenantId,
        string code,
        string name,
        Guid? parentCategoryId,
        DateTimeOffset utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new ProductCategory(id, tenantId)
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            ParentCategoryId = parentCategoryId,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
        };
    }

    public void Update(string name, Guid? parentCategoryId, DateTimeOffset utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (parentCategoryId == Id)
        {
            throw new ArgumentException("Category cannot be its own parent.", nameof(parentCategoryId));
        }

        Name = name.Trim();
        ParentCategoryId = parentCategoryId;
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
}

using Warehouse.Domain.Common;

namespace Warehouse.Domain.Catalog;

/// <summary>
/// TENANT-OWNED barcode linked to a product. Multiple barcodes per product are allowed.
/// Barcode value is unique per tenant.
/// </summary>
public sealed class ProductBarcode : TenantOwnedEntity
{
    private ProductBarcode()
    {
    }

    private ProductBarcode(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public Guid ProductId { get; private set; }

    public string Barcode { get; private set; } = string.Empty;

    public bool IsPrimary { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static ProductBarcode Create(
        Guid id,
        Guid tenantId,
        Guid productId,
        string barcode,
        bool isPrimary,
        DateTimeOffset utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(barcode);
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("Product id is required.", nameof(productId));
        }

        return new ProductBarcode(id, tenantId)
        {
            ProductId = productId,
            Barcode = barcode.Trim(),
            IsPrimary = isPrimary,
            CreatedAtUtc = utcNow,
        };
    }

    public void SetPrimary(bool isPrimary)
    {
        IsPrimary = isPrimary;
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Catalog;
using Warehouse.Domain.Tenancy;

namespace Warehouse.Infrastructure.Persistence.Configurations;

internal sealed class UnitOfMeasureConfiguration : IEntityTypeConfiguration<UnitOfMeasure>
{
    public void Configure(EntityTypeBuilder<UnitOfMeasure> builder)
    {
        builder.ToTable("units_of_measure");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();
        builder.Property(x => x.RowVersion).IsConcurrencyToken();

        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.IsActive });
        builder.HasIndex(x => x.TenantId);

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("product_categories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();
        builder.Property(x => x.RowVersion).IsConcurrencyToken();

        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.IsActive });
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.ParentCategoryId);

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ProductCategory>()
            .WithMany()
            .HasForeignKey(x => x.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products", t =>
        {
            t.HasCheckConstraint("ck_products_purchase_price_non_negative", "purchase_price >= 0");
            t.HasCheckConstraint("ck_products_sale_price_non_negative", "sale_price >= 0");
            t.HasCheckConstraint("ck_products_minimum_stock_non_negative", "minimum_stock IS NULL OR minimum_stock >= 0");
            t.HasCheckConstraint("ck_products_maximum_stock_non_negative", "maximum_stock IS NULL OR maximum_stock >= 0");
            t.HasCheckConstraint("ck_products_reorder_point_non_negative", "reorder_point IS NULL OR reorder_point >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Sku).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Brand).HasMaxLength(128);
        builder.Property(x => x.PurchasePrice).HasPrecision(19, 4).IsRequired();
        builder.Property(x => x.SalePrice).HasPrecision(19, 4).IsRequired();
        builder.Property(x => x.MinimumStock).HasPrecision(18, 6);
        builder.Property(x => x.MaximumStock).HasPrecision(18, 6);
        builder.Property(x => x.ReorderPoint).HasPrecision(18, 6);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();
        builder.Property(x => x.RowVersion).IsConcurrencyToken();

        builder.HasIndex(x => new { x.TenantId, x.Sku }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.Name });
        builder.HasIndex(x => new { x.TenantId, x.IsActive });
        builder.HasIndex(x => new { x.TenantId, x.CategoryId });
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.UnitOfMeasureId);

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ProductCategory>()
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<UnitOfMeasure>()
            .WithMany()
            .HasForeignKey(x => x.UnitOfMeasureId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ProductBarcodeConfiguration : IEntityTypeConfiguration<ProductBarcode>
{
    public void Configure(EntityTypeBuilder<ProductBarcode> builder)
    {
        builder.ToTable("product_barcodes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Barcode).HasMaxLength(128).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.Barcode }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.ProductId });
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.ProductId);

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

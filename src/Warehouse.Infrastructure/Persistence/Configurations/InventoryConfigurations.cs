using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Catalog;
using Warehouse.Domain.Identity;
using Warehouse.Domain.Inventory;
using Warehouse.Domain.Tenancy;
using Warehouse.Domain.Warehousing;
using WarehouseEntity = Warehouse.Domain.Warehousing.Warehouse;

namespace Warehouse.Infrastructure.Persistence.Configurations;

internal sealed class StockBalanceConfiguration : IEntityTypeConfiguration<StockBalance>
{
    public void Configure(EntityTypeBuilder<StockBalance> builder)
    {
        builder.ToTable("stock_balances");
        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.Available);
        builder.Property(x => x.OnHand).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.Reserved).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();
        builder.Property(x => x.RowVersion).IsConcurrencyToken();

        builder.HasIndex(x => new { x.TenantId, x.ProductId, x.WarehouseId, x.LocationId }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.WarehouseId, x.ProductId });
        builder.HasIndex(x => x.TenantId);

        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WarehouseEntity>().WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WarehouseLocation>().WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("stock_movements");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.QuantityChange).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.MovementType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.SourceDocumentType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(512);
        builder.Property(x => x.OccurredAtUtc).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.ProductId, x.OccurredAtUtc });
        builder.HasIndex(x => new { x.TenantId, x.SourceDocumentType, x.SourceDocumentId });
        builder.HasIndex(x => new { x.TenantId, x.WarehouseId, x.LocationId });
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.TransferLineId);

        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WarehouseEntity>().WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WarehouseLocation>().WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.SetNull);
    }
}

internal sealed class StockAdjustmentConfiguration : IEntityTypeConfiguration<StockAdjustment>
{
    public void Configure(EntityTypeBuilder<StockAdjustment> builder)
    {
        builder.ToTable("stock_adjustments", t =>
        {
            t.HasCheckConstraint("ck_stock_adjustments_kind", "kind IN ('OpeningBalance', 'Correction')");
            t.HasCheckConstraint("ck_stock_adjustments_status", "status IN ('Draft', 'Approved', 'Posted', 'Cancelled')");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Kind).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(512);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();
        builder.Property(x => x.RowVersion).IsConcurrencyToken();

        builder.HasIndex(x => new { x.TenantId, x.WarehouseId, x.Status });
        builder.HasIndex(x => x.TenantId);

        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WarehouseEntity>().WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.SetNull);
    }
}

internal sealed class StockAdjustmentLineConfiguration : IEntityTypeConfiguration<StockAdjustmentLine>
{
    public void Configure(EntityTypeBuilder<StockAdjustmentLine> builder)
    {
        builder.ToTable("stock_adjustment_lines", t =>
            t.HasCheckConstraint("ck_stock_adjustment_lines_quantity_change_nonzero", "quantity_change <> 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.QuantityChange).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(512);

        builder.HasIndex(x => new { x.StockAdjustmentId, x.ProductId, x.LocationId }).IsUnique();
        builder.HasIndex(x => x.TenantId);

        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<StockAdjustment>().WithMany().HasForeignKey(x => x.StockAdjustmentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WarehouseLocation>().WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class StockCountConfiguration : IEntityTypeConfiguration<StockCount>
{
    public void Configure(EntityTypeBuilder<StockCount> builder)
    {
        builder.ToTable("stock_counts", t =>
            t.HasCheckConstraint("ck_stock_counts_status", "status IN ('Draft', 'Counted', 'Posted', 'Cancelled')"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(512);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();
        builder.Property(x => x.RowVersion).IsConcurrencyToken();

        builder.HasIndex(x => new { x.TenantId, x.WarehouseId, x.Status });
        builder.HasIndex(x => x.TenantId);

        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WarehouseEntity>().WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WarehouseLocation>().WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class StockCountLineConfiguration : IEntityTypeConfiguration<StockCountLine>
{
    public void Configure(EntityTypeBuilder<StockCountLine> builder)
    {
        builder.ToTable("stock_count_lines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SystemQuantity).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.CountedQuantity).HasPrecision(18, 6);
        builder.Property(x => x.Notes).HasMaxLength(512);

        builder.HasIndex(x => new { x.StockCountId, x.ProductId }).IsUnique();
        builder.HasIndex(x => x.TenantId);

        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<StockCount>().WithMany().HasForeignKey(x => x.StockCountId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}

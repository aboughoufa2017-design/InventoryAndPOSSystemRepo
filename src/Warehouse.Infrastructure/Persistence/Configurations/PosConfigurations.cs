using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Pos;
using Warehouse.Domain.Precision;
using Warehouse.Domain.Tenancy;

namespace Warehouse.Infrastructure.Persistence.Configurations;

internal sealed class PosSaleConfiguration : IEntityTypeConfiguration<PosSale>
{
    public void Configure(EntityTypeBuilder<PosSale> builder)
    {
        builder.ToTable("pos_sales");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ReceiptNumber).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Subtotal).HasPrecision(MoneyPrecision.Precision, MoneyPrecision.Scale);
        builder.Property(x => x.DiscountTotal).HasPrecision(MoneyPrecision.Precision, MoneyPrecision.Scale);
        builder.Property(x => x.TaxTotal).HasPrecision(MoneyPrecision.Precision, MoneyPrecision.Scale);
        builder.Property(x => x.GrandTotal).HasPrecision(MoneyPrecision.Precision, MoneyPrecision.Scale);
        builder.Property(x => x.Notes).HasMaxLength(1024);
        builder.Property(x => x.RowVersion).IsConcurrencyToken();
        builder.HasIndex(x => new { x.TenantId, x.ClientCommandId }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.WarehouseId, x.SoldAtUtc });
        builder.HasIndex(x => new { x.TenantId, x.ReceiptNumber });
        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PosSaleLineConfiguration : IEntityTypeConfiguration<PosSaleLine>
{
    public void Configure(EntityTypeBuilder<PosSaleLine> builder)
    {
        builder.ToTable("pos_sale_lines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SkuSnapshot).HasMaxLength(64).IsRequired();
        builder.Property(x => x.NameSnapshot).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Quantity).HasPrecision(QuantityPrecision.Precision, QuantityPrecision.Scale);
        builder.Property(x => x.UnitPrice).HasPrecision(MoneyPrecision.Precision, MoneyPrecision.Scale);
        builder.Property(x => x.DiscountAmount).HasPrecision(MoneyPrecision.Precision, MoneyPrecision.Scale);
        builder.Property(x => x.TaxAmount).HasPrecision(MoneyPrecision.Precision, MoneyPrecision.Scale);
        builder.Property(x => x.LineTotal).HasPrecision(MoneyPrecision.Precision, MoneyPrecision.Scale);
        builder.HasIndex(x => new { x.TenantId, x.PosSaleId });
        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PosSale>().WithMany().HasForeignKey(x => x.PosSaleId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class PosPaymentConfiguration : IEntityTypeConfiguration<PosPayment>
{
    public void Configure(EntityTypeBuilder<PosPayment> builder)
    {
        builder.ToTable("pos_payments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Method).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(MoneyPrecision.Precision, MoneyPrecision.Scale);
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Reference).HasMaxLength(128);
        builder.HasIndex(x => new { x.TenantId, x.PosSaleId });
        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PosSale>().WithMany().HasForeignKey(x => x.PosSaleId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class CashShiftConfiguration : IEntityTypeConfiguration<CashShift>
{
    public void Configure(EntityTypeBuilder<CashShift> builder)
    {
        builder.ToTable("cash_shifts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.OpeningFloat).HasPrecision(MoneyPrecision.Precision, MoneyPrecision.Scale);
        builder.Property(x => x.ClosingCountedCash).HasPrecision(MoneyPrecision.Precision, MoneyPrecision.Scale);
        builder.Property(x => x.RowVersion).IsConcurrencyToken();
        builder.HasIndex(x => new { x.TenantId, x.WarehouseId, x.Status });
        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class CashMovementConfiguration : IEntityTypeConfiguration<CashMovement>
{
    public void Configure(EntityTypeBuilder<CashMovement> builder)
    {
        builder.ToTable("cash_movements");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(MoneyPrecision.Precision, MoneyPrecision.Scale);
        builder.Property(x => x.Reason).HasMaxLength(512);
        builder.HasIndex(x => new { x.TenantId, x.CashShiftId });
        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CashShift>().WithMany().HasForeignKey(x => x.CashShiftId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class PosInventoryConflictConfiguration : IEntityTypeConfiguration<PosInventoryConflict>
{
    public void Configure(EntityTypeBuilder<PosInventoryConflict> builder)
    {
        builder.ToTable("pos_inventory_conflicts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Message).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.ResultingOnHand).HasPrecision(QuantityPrecision.Precision, QuantityPrecision.Scale);
        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PriceListConfiguration : IEntityTypeConfiguration<PriceList>
{
    public void Configure(EntityTypeBuilder<PriceList> builder)
    {
        builder.ToTable("price_lists");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ProductPriceConfiguration : IEntityTypeConfiguration<ProductPrice>
{
    public void Configure(EntityTypeBuilder<ProductPrice> builder)
    {
        builder.ToTable("product_prices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UnitPrice).HasPrecision(MoneyPrecision.Precision, MoneyPrecision.Scale);
        builder.Property(x => x.MinQuantity).HasPrecision(QuantityPrecision.Precision, QuantityPrecision.Scale);
        builder.HasIndex(x => new { x.TenantId, x.PriceListId, x.ProductId, x.MinQuantity });
        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PriceList>().WithMany().HasForeignKey(x => x.PriceListId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class RestaurantTableConfiguration : IEntityTypeConfiguration<RestaurantTable>
{
    public void Configure(EntityTypeBuilder<RestaurantTable> builder)
    {
        builder.ToTable("restaurant_tables");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.WarehouseId, x.Code }).IsUnique();
        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class RestaurantOpenOrderConfiguration : IEntityTypeConfiguration<RestaurantOpenOrder>
{
    public void Configure(EntityTypeBuilder<RestaurantOpenOrder> builder)
    {
        builder.ToTable("restaurant_open_orders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1024);
        builder.HasIndex(x => new { x.TenantId, x.WarehouseId, x.Status });
        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<RestaurantTable>().WithMany().HasForeignKey(x => x.TableId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class SelfCheckoutSessionConfiguration : IEntityTypeConfiguration<SelfCheckoutSession>
{
    public void Configure(EntityTypeBuilder<SelfCheckoutSession> builder)
    {
        builder.ToTable("self_checkout_sessions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(16).IsRequired();
        builder.Property(x => x.AccessToken).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Subtotal).HasPrecision(MoneyPrecision.Precision, MoneyPrecision.Scale);
        builder.Property(x => x.TaxTotal).HasPrecision(MoneyPrecision.Precision, MoneyPrecision.Scale);
        builder.Property(x => x.GrandTotal).HasPrecision(MoneyPrecision.Precision, MoneyPrecision.Scale);
        builder.Property(x => x.AuditTargetBarcodesJson).HasColumnType("text");
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        builder.HasIndex(x => x.AccessToken).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.Status, x.ExpiresAtUtc });
        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class SelfCheckoutLineConfiguration : IEntityTypeConfiguration<SelfCheckoutLine>
{
    public void Configure(EntityTypeBuilder<SelfCheckoutLine> builder)
    {
        builder.ToTable("self_checkout_lines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SkuSnapshot).HasMaxLength(64).IsRequired();
        builder.Property(x => x.NameSnapshot).HasMaxLength(256).IsRequired();
        builder.Property(x => x.BarcodeSnapshot).HasMaxLength(64);
        builder.Property(x => x.Quantity).HasPrecision(QuantityPrecision.Precision, QuantityPrecision.Scale);
        builder.Property(x => x.UnitPrice).HasPrecision(MoneyPrecision.Precision, MoneyPrecision.Scale);
        builder.Property(x => x.LineTotal).HasPrecision(MoneyPrecision.Precision, MoneyPrecision.Scale);
        builder.HasIndex(x => new { x.TenantId, x.SessionId });
        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SelfCheckoutSession>().WithMany().HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Cascade);
    }
}

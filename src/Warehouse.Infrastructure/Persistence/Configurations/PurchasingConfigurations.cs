using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Catalog;
using Warehouse.Domain.Identity;
using Warehouse.Domain.Purchasing;
using Warehouse.Domain.Tenancy;
using Warehouse.Domain.Warehousing;
using WarehouseEntity = Warehouse.Domain.Warehousing.Warehouse;

namespace Warehouse.Infrastructure.Persistence.Configurations;

internal sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("suppliers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.ContactName).HasMaxLength(256);
        builder.Property(x => x.Email).HasMaxLength(256);
        builder.Property(x => x.Phone).HasMaxLength(64);
        builder.Property(x => x.Notes).HasMaxLength(512);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();
        builder.Property(x => x.RowVersion).IsConcurrencyToken();

        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.IsActive });
        builder.HasIndex(x => x.TenantId);

        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("purchase_orders", t =>
        {
            t.HasCheckConstraint(
                "ck_purchase_orders_status",
                "status IN ('Draft', 'Confirmed', 'PartiallyReceived', 'FullyReceived', 'Closed', 'Cancelled')");
            t.HasCheckConstraint(
                "ck_purchase_orders_payment_status",
                "payment_status IN ('Unpaid', 'PartiallyPaid', 'Paid')");
            t.HasCheckConstraint("ck_purchase_orders_paid_non_negative", "paid_amount >= 0");
            t.HasCheckConstraint("ck_purchase_orders_totals_non_negative", "subtotal >= 0 AND tax_total >= 0 AND grand_total >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.PaymentStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Reference).HasMaxLength(128);
        builder.Property(x => x.Notes).HasMaxLength(512);
        builder.Property(x => x.Subtotal).HasPrecision(19, 4).IsRequired();
        builder.Property(x => x.TaxTotal).HasPrecision(19, 4).IsRequired();
        builder.Property(x => x.GrandTotal).HasPrecision(19, 4).IsRequired();
        builder.Property(x => x.PaidAmount).HasPrecision(19, 4).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();
        builder.Property(x => x.RowVersion).IsConcurrencyToken();

        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => new { x.TenantId, x.SupplierId });
        builder.HasIndex(x => x.TenantId);

        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Supplier>().WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WarehouseEntity>().WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WarehouseLocation>().WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.SetNull);
    }
}

internal sealed class PurchaseOrderLineConfiguration : IEntityTypeConfiguration<PurchaseOrderLine>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderLine> builder)
    {
        builder.ToTable("purchase_order_lines", t =>
        {
            t.HasCheckConstraint("ck_purchase_order_lines_qty_ordered_positive", "quantity_ordered > 0");
            t.HasCheckConstraint("ck_purchase_order_lines_qty_received_non_negative", "quantity_received >= 0");
            t.HasCheckConstraint("ck_purchase_order_lines_qty_returned_non_negative", "quantity_returned >= 0");
            t.HasCheckConstraint(
                "ck_purchase_order_lines_received_not_over_ordered",
                "quantity_received <= quantity_ordered");
            t.HasCheckConstraint(
                "ck_purchase_order_lines_returned_not_over_received",
                "quantity_returned <= quantity_received");
            t.HasCheckConstraint("ck_purchase_order_lines_unit_price_non_negative", "unit_price >= 0");
            t.HasCheckConstraint("ck_purchase_order_lines_tax_rate_non_negative", "tax_rate >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.QuantityRemainingToReceive);
        builder.Ignore(x => x.QuantityReturnable);
        builder.Property(x => x.QuantityOrdered).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.QuantityReceived).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.QuantityReturned).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.UnitPrice).HasPrecision(19, 4).IsRequired();
        builder.Property(x => x.TaxRate).HasPrecision(9, 6).IsRequired();
        builder.Property(x => x.LineNet).HasPrecision(19, 4).IsRequired();
        builder.Property(x => x.LineTax).HasPrecision(19, 4).IsRequired();
        builder.Property(x => x.LineTotal).HasPrecision(19, 4).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(512);

        builder.HasIndex(x => new { x.PurchaseOrderId, x.ProductId }).IsUnique();
        builder.HasIndex(x => x.TenantId);

        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PurchaseOrder>().WithMany().HasForeignKey(x => x.PurchaseOrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PurchaseReceiptConfiguration : IEntityTypeConfiguration<PurchaseReceipt>
{
    public void Configure(EntityTypeBuilder<PurchaseReceipt> builder)
    {
        builder.ToTable("purchase_receipts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Notes).HasMaxLength(512);
        builder.Property(x => x.ReceivedAtUtc).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.PurchaseOrderId });
        builder.HasIndex(x => x.OperationId).IsUnique();
        builder.HasIndex(x => x.TenantId);

        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PurchaseOrder>().WithMany().HasForeignKey(x => x.PurchaseOrderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WarehouseEntity>().WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WarehouseLocation>().WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.SetNull);
    }
}

internal sealed class PurchaseReceiptLineConfiguration : IEntityTypeConfiguration<PurchaseReceiptLine>
{
    public void Configure(EntityTypeBuilder<PurchaseReceiptLine> builder)
    {
        builder.ToTable("purchase_receipt_lines", t =>
        {
            t.HasCheckConstraint("ck_purchase_receipt_lines_quantity_positive", "quantity > 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasPrecision(18, 6).IsRequired();

        builder.HasIndex(x => new { x.PurchaseReceiptId, x.PurchaseOrderLineId }).IsUnique();
        builder.HasIndex(x => x.TenantId);

        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PurchaseReceipt>().WithMany().HasForeignKey(x => x.PurchaseReceiptId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<PurchaseOrderLine>().WithMany().HasForeignKey(x => x.PurchaseOrderLineId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PurchaseReturnConfiguration : IEntityTypeConfiguration<PurchaseReturn>
{
    public void Configure(EntityTypeBuilder<PurchaseReturn> builder)
    {
        builder.ToTable("purchase_returns");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Notes).HasMaxLength(512);
        builder.Property(x => x.ReturnedAtUtc).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.PurchaseOrderId });
        builder.HasIndex(x => x.OperationId).IsUnique();
        builder.HasIndex(x => x.TenantId);

        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PurchaseOrder>().WithMany().HasForeignKey(x => x.PurchaseOrderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WarehouseEntity>().WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WarehouseLocation>().WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.SetNull);
    }
}

internal sealed class PurchaseReturnLineConfiguration : IEntityTypeConfiguration<PurchaseReturnLine>
{
    public void Configure(EntityTypeBuilder<PurchaseReturnLine> builder)
    {
        builder.ToTable("purchase_return_lines", t =>
        {
            t.HasCheckConstraint("ck_purchase_return_lines_quantity_positive", "quantity > 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasPrecision(18, 6).IsRequired();

        builder.HasIndex(x => new { x.PurchaseReturnId, x.PurchaseOrderLineId }).IsUnique();
        builder.HasIndex(x => x.TenantId);

        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PurchaseReturn>().WithMany().HasForeignKey(x => x.PurchaseReturnId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<PurchaseOrderLine>().WithMany().HasForeignKey(x => x.PurchaseOrderLineId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PurchaseOperationConfiguration : IEntityTypeConfiguration<PurchaseOperation>
{
    public void Configure(EntityTypeBuilder<PurchaseOperation> builder)
    {
        builder.ToTable("purchase_operations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Action).HasMaxLength(32).IsRequired();
        builder.Property(x => x.ResultPayloadJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.ProcessedAtUtc).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.PurchaseOrderId });
        builder.HasIndex(x => x.TenantId);

        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PurchaseOrder>().WithMany().HasForeignKey(x => x.PurchaseOrderId).OnDelete(DeleteBehavior.Cascade);
    }
}

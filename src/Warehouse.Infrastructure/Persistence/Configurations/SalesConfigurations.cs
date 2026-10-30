using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Catalog;
using Warehouse.Domain.Identity;
using Warehouse.Domain.Sales;
using Warehouse.Domain.Tenancy;
using Warehouse.Domain.Warehousing;
using WarehouseEntity = Warehouse.Domain.Warehousing.Warehouse;

namespace Warehouse.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");
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

internal sealed class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("sales_orders", t =>
        {
            t.HasCheckConstraint(
                "ck_sales_orders_status",
                "status IN ('Draft', 'Confirmed', 'PartiallyFulfilled', 'Fulfilled', 'Closed', 'Cancelled')");
            t.HasCheckConstraint(
                "ck_sales_orders_payment_status",
                "payment_status IN ('Unpaid', 'PartiallyPaid', 'Paid')");
            t.HasCheckConstraint("ck_sales_orders_paid_non_negative", "paid_amount >= 0");
            t.HasCheckConstraint("ck_sales_orders_totals_non_negative", "subtotal >= 0 AND tax_total >= 0 AND grand_total >= 0");
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
        builder.HasIndex(x => new { x.TenantId, x.CustomerId });
        builder.HasIndex(x => x.TenantId);

        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WarehouseEntity>().WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WarehouseLocation>().WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.SetNull);
    }
}

internal sealed class SalesOrderLineConfiguration : IEntityTypeConfiguration<SalesOrderLine>
{
    public void Configure(EntityTypeBuilder<SalesOrderLine> builder)
    {
        builder.ToTable("sales_order_lines", t =>
        {
            t.HasCheckConstraint("ck_sales_order_lines_qty_ordered_positive", "quantity_ordered > 0");
            t.HasCheckConstraint("ck_sales_order_lines_qty_shipped_non_negative", "quantity_shipped >= 0");
            t.HasCheckConstraint("ck_sales_order_lines_qty_returned_non_negative", "quantity_returned >= 0");
            t.HasCheckConstraint(
                "ck_sales_order_lines_shipped_not_over_ordered",
                "quantity_shipped <= quantity_ordered");
            t.HasCheckConstraint(
                "ck_sales_order_lines_returned_not_over_shipped",
                "quantity_returned <= quantity_shipped");
            t.HasCheckConstraint("ck_sales_order_lines_unit_price_non_negative", "unit_price >= 0");
            t.HasCheckConstraint("ck_sales_order_lines_tax_rate_non_negative", "tax_rate >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.QuantityRemainingToShip);
        builder.Ignore(x => x.QuantityReturnable);
        builder.Property(x => x.QuantityOrdered).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.QuantityShipped).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.QuantityReturned).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.UnitPrice).HasPrecision(19, 4).IsRequired();
        builder.Property(x => x.TaxRate).HasPrecision(9, 6).IsRequired();
        builder.Property(x => x.LineNet).HasPrecision(19, 4).IsRequired();
        builder.Property(x => x.LineTax).HasPrecision(19, 4).IsRequired();
        builder.Property(x => x.LineTotal).HasPrecision(19, 4).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(512);

        builder.HasIndex(x => new { x.SalesOrderId, x.ProductId }).IsUnique();
        builder.HasIndex(x => x.TenantId);

        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SalesOrder>().WithMany().HasForeignKey(x => x.SalesOrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class SalesShipmentConfiguration : IEntityTypeConfiguration<SalesShipment>
{
    public void Configure(EntityTypeBuilder<SalesShipment> builder)
    {
        builder.ToTable("sales_shipments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Notes).HasMaxLength(512);
        builder.Property(x => x.ShippedAtUtc).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.SalesOrderId });
        builder.HasIndex(x => x.OperationId).IsUnique();
        builder.HasIndex(x => x.TenantId);

        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SalesOrder>().WithMany().HasForeignKey(x => x.SalesOrderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WarehouseEntity>().WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WarehouseLocation>().WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.SetNull);
    }
}

internal sealed class SalesShipmentLineConfiguration : IEntityTypeConfiguration<SalesShipmentLine>
{
    public void Configure(EntityTypeBuilder<SalesShipmentLine> builder)
    {
        builder.ToTable("sales_shipment_lines", t =>
        {
            t.HasCheckConstraint("ck_sales_shipment_lines_quantity_positive", "quantity > 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasPrecision(18, 6).IsRequired();

        builder.HasIndex(x => new { x.SalesShipmentId, x.SalesOrderLineId }).IsUnique();
        builder.HasIndex(x => x.TenantId);

        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SalesShipment>().WithMany().HasForeignKey(x => x.SalesShipmentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<SalesOrderLine>().WithMany().HasForeignKey(x => x.SalesOrderLineId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class SalesReturnConfiguration : IEntityTypeConfiguration<SalesReturn>
{
    public void Configure(EntityTypeBuilder<SalesReturn> builder)
    {
        builder.ToTable("sales_returns");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Notes).HasMaxLength(512);
        builder.Property(x => x.ReturnedAtUtc).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.SalesOrderId });
        builder.HasIndex(x => x.OperationId).IsUnique();
        builder.HasIndex(x => x.TenantId);

        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SalesOrder>().WithMany().HasForeignKey(x => x.SalesOrderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WarehouseEntity>().WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WarehouseLocation>().WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.SetNull);
    }
}

internal sealed class SalesReturnLineConfiguration : IEntityTypeConfiguration<SalesReturnLine>
{
    public void Configure(EntityTypeBuilder<SalesReturnLine> builder)
    {
        builder.ToTable("sales_return_lines", t =>
        {
            t.HasCheckConstraint("ck_sales_return_lines_quantity_positive", "quantity > 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasPrecision(18, 6).IsRequired();

        builder.HasIndex(x => new { x.SalesReturnId, x.SalesOrderLineId }).IsUnique();
        builder.HasIndex(x => x.TenantId);

        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SalesReturn>().WithMany().HasForeignKey(x => x.SalesReturnId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<SalesOrderLine>().WithMany().HasForeignKey(x => x.SalesOrderLineId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class SalesOperationConfiguration : IEntityTypeConfiguration<SalesOperation>
{
    public void Configure(EntityTypeBuilder<SalesOperation> builder)
    {
        builder.ToTable("sales_operations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Action).HasMaxLength(32).IsRequired();
        builder.Property(x => x.ResultPayloadJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.ProcessedAtUtc).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.SalesOrderId });
        builder.HasIndex(x => x.TenantId);

        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SalesOrder>().WithMany().HasForeignKey(x => x.SalesOrderId).OnDelete(DeleteBehavior.Cascade);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Catalog;
using Warehouse.Domain.Identity;
using Warehouse.Domain.Inventory;
using Warehouse.Domain.Tenancy;
using Warehouse.Domain.Warehousing;
using WarehouseEntity = Warehouse.Domain.Warehousing.Warehouse;

namespace Warehouse.Infrastructure.Persistence.Configurations;

internal sealed class TransferConfiguration : IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> builder)
    {
        builder.ToTable("transfers", t =>
        {
            t.HasCheckConstraint(
                "ck_transfers_status",
                "status IN ('Draft', 'Dispatched', 'PartiallyReceived', 'Completed', 'Cancelled')");
            t.HasCheckConstraint(
                "ck_transfers_warehouses_differ",
                "source_warehouse_id <> destination_warehouse_id");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(512);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();
        builder.Property(x => x.RowVersion).IsConcurrencyToken();

        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => new { x.TenantId, x.SourceWarehouseId });
        builder.HasIndex(x => new { x.TenantId, x.DestinationWarehouseId });
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.DispatchOperationId).IsUnique();

        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WarehouseEntity>().WithMany().HasForeignKey(x => x.SourceWarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WarehouseEntity>().WithMany().HasForeignKey(x => x.DestinationWarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WarehouseLocation>().WithMany().HasForeignKey(x => x.SourceLocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WarehouseLocation>().WithMany().HasForeignKey(x => x.DestinationLocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.SetNull);
    }
}

internal sealed class TransferLineConfiguration : IEntityTypeConfiguration<TransferLine>
{
    public void Configure(EntityTypeBuilder<TransferLine> builder)
    {
        builder.ToTable("transfer_lines", t =>
        {
            t.HasCheckConstraint("ck_transfer_lines_quantity_requested_positive", "quantity_requested > 0");
            t.HasCheckConstraint("ck_transfer_lines_quantity_received_non_negative", "quantity_received >= 0");
            t.HasCheckConstraint(
                "ck_transfer_lines_received_not_over_requested",
                "quantity_received <= quantity_requested");
        });
        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.QuantityInTransit);
        builder.Property(x => x.QuantityRequested).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.QuantityReceived).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(512);

        builder.HasIndex(x => new { x.TransferId, x.ProductId }).IsUnique();
        builder.HasIndex(x => x.TenantId);

        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Transfer>().WithMany().HasForeignKey(x => x.TransferId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class TransferOperationConfiguration : IEntityTypeConfiguration<TransferOperation>
{
    public void Configure(EntityTypeBuilder<TransferOperation> builder)
    {
        builder.ToTable("transfer_operations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Action).HasMaxLength(32).IsRequired();
        builder.Property(x => x.ResultPayloadJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.ProcessedAtUtc).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.TransferId });
        builder.HasIndex(x => x.TenantId);

        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Transfer>().WithMany().HasForeignKey(x => x.TransferId).OnDelete(DeleteBehavior.Cascade);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Tenancy;
using Warehouse.Domain.Warehousing;
using WarehouseEntity = Warehouse.Domain.Warehousing.Warehouse;

namespace Warehouse.Infrastructure.Persistence.Configurations;

internal sealed class WarehouseLocationConfiguration : IEntityTypeConfiguration<WarehouseLocation>
{
    public void Configure(EntityTypeBuilder<WarehouseLocation> builder)
    {
        builder.ToTable("warehouse_locations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Kind).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();
        builder.Property(x => x.RowVersion).IsConcurrencyToken();

        builder.HasIndex(x => new { x.TenantId, x.WarehouseId, x.Code }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.WarehouseId, x.Kind });
        builder.HasIndex(x => new { x.TenantId, x.WarehouseId });
        builder.HasIndex(x => x.TenantId);

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<WarehouseEntity>()
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "ck_warehouse_locations_kind",
                "kind IN ('Normal', 'System', 'InTransit')");
            t.HasCheckConstraint(
                "ck_warehouse_locations_in_transit_not_default",
                "kind <> 'InTransit' OR is_default = false");
        });
    }
}

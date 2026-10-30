using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Tenancy;
using WarehouseEntity = Warehouse.Domain.Warehousing.Warehouse;

namespace Warehouse.Infrastructure.Persistence.Configurations;

internal sealed class WarehouseConfiguration : IEntityTypeConfiguration<WarehouseEntity>
{
    public void Configure(EntityTypeBuilder<WarehouseEntity> builder)
    {
        builder.ToTable("warehouses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();
        builder.Property(x => x.RowVersion).IsConcurrencyToken();

        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.Name });
        builder.HasIndex(x => new { x.TenantId, x.IsActive });
        builder.HasIndex(x => x.TenantId);

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

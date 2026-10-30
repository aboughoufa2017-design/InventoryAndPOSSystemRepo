using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Devices;
using Warehouse.Domain.Identity;
using Warehouse.Domain.Sync;
using Warehouse.Domain.Tenancy;

namespace Warehouse.Infrastructure.Persistence.Configurations;

internal sealed class ProcessedSyncOperationConfiguration : IEntityTypeConfiguration<ProcessedSyncOperation>
{
    public void Configure(EntityTypeBuilder<ProcessedSyncOperation> builder)
    {
        builder.ToTable("processed_sync_operations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityType).HasMaxLength(128).IsRequired();
        builder.Property(x => x.OperationType).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.ResultPayloadJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.ProcessedAtUtc).IsRequired();

        // OperationId is the primary key (Id). Global uniqueness is guaranteed by the PK.
        builder.HasIndex(x => new { x.TenantId, x.CreatedAtUtc });
        builder.HasIndex(x => new { x.TenantId, x.DeviceId });
        builder.HasIndex(x => new { x.TenantId, x.EntityType, x.EntityId });
        builder.HasIndex(x => x.TenantId);

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Device>()
            .WithMany()
            .HasForeignKey(x => x.DeviceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t =>
            t.HasCheckConstraint(
                "ck_processed_sync_operations_status",
                "status IN ('Succeeded', 'Rejected')"));

        builder.Ignore(x => x.OperationId);
    }
}

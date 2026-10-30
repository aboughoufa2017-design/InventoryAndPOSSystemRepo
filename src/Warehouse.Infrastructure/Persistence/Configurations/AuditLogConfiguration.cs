using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Audit;
using Warehouse.Domain.Devices;
using Warehouse.Domain.Identity;
using Warehouse.Domain.Tenancy;

namespace Warehouse.Infrastructure.Persistence.Configurations;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Action).HasMaxLength(128).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(128).IsRequired();
        builder.Property(x => x.OccurredAtUtc).IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(64);
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.MetadataJson).HasColumnType("jsonb");

        builder.HasIndex(x => new { x.TenantId, x.OccurredAtUtc });
        builder.HasIndex(x => new { x.TenantId, x.EntityType, x.EntityId });
        builder.HasIndex(x => new { x.TenantId, x.UserId });
        builder.HasIndex(x => x.CorrelationId);
        builder.HasIndex(x => x.TenantId);

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<Device>()
            .WithMany()
            .HasForeignKey(x => x.DeviceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

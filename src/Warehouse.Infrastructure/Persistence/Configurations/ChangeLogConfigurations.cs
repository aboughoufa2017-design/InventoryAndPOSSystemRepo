using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Devices;
using Warehouse.Domain.Identity;
using Warehouse.Domain.Sync;
using Warehouse.Domain.Tenancy;

namespace Warehouse.Infrastructure.Persistence.Configurations;

internal sealed class ChangeLogEntryConfiguration : IEntityTypeConfiguration<ChangeLogEntry>
{
    public void Configure(EntityTypeBuilder<ChangeLogEntry> builder)
    {
        builder.ToTable("change_log", t =>
        {
            t.HasCheckConstraint(
                "ck_change_log_operation",
                "operation IN ('Upsert', 'Deactivate')");
            t.HasCheckConstraint("ck_change_log_change_version_positive", "change_version > 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ChangeVersion).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Operation).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.OccurredAtUtc).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.ChangeVersion }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.EntityType, x.EntityId });
        builder.HasIndex(x => x.TenantId);

        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Device>().WithMany().HasForeignKey(x => x.DeviceId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
    }
}

internal sealed class TenantChangeCounterConfiguration : IEntityTypeConfiguration<TenantChangeCounter>
{
    public void Configure(EntityTypeBuilder<TenantChangeCounter> builder)
    {
        builder.ToTable("tenant_change_counters");
        builder.HasKey(x => x.TenantId);
        builder.Property(x => x.LastChangeVersion).IsRequired();

        builder.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
    }
}

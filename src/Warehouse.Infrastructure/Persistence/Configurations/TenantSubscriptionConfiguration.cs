using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Subscription;

namespace Warehouse.Infrastructure.Persistence.Configurations;

internal sealed class TenantSubscriptionConfiguration : IEntityTypeConfiguration<TenantSubscription>
{
    public void Configure(EntityTypeBuilder<TenantSubscription> builder)
    {
        builder.ToTable("tenant_subscriptions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.PlanId).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.ExternalReference).HasMaxLength(256);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();
        builder.Property(x => x.RowVersion).IsRequired().IsConcurrencyToken();

        builder.HasIndex(x => x.TenantId).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => x.PlanId);

        builder.HasOne<Plan>()
            .WithMany()
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "ck_tenant_subscriptions_status",
                "status IN ('Trial', 'Active', 'PastDue', 'Cancelled')");
        });
    }
}

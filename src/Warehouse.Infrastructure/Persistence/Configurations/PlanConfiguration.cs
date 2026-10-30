using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Subscription;

namespace Warehouse.Infrastructure.Persistence.Configurations;

internal sealed class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("plans");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.PrimitiveCollection(x => x.FeatureCodes).ElementType().HasMaxLength(64);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();
    }
}

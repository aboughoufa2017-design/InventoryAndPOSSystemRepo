using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Platform;

namespace Warehouse.Infrastructure.Persistence.Configurations;

internal sealed class PlatformOperatorConfiguration : IEntityTypeConfiguration<PlatformOperator>
{
    public void Configure(EntityTypeBuilder<PlatformOperator> builder)
    {
        builder.ToTable("platform_operators");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();

        builder.HasIndex(x => x.Email).IsUnique();
    }
}

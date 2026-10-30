using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Identity;

namespace Warehouse.Infrastructure.Persistence.Configurations;

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(512);
        builder.HasIndex(x => x.Code).IsUnique();
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Identity;
using Warehouse.Domain.Tenancy;

namespace Warehouse.Infrastructure.Persistence.Configurations;

internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => new { x.RoleId, x.PermissionId }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.RoleId });
        builder.HasIndex(x => x.TenantId);

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Permission>()
            .WithMany()
            .HasForeignKey(x => x.PermissionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

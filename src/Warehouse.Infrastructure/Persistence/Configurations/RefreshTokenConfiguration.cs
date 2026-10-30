using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Devices;
using Warehouse.Domain.Identity;
using Warehouse.Domain.Tenancy;

namespace Warehouse.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.ExpiresAtUtc).IsRequired();

        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.UserId });
        builder.HasIndex(x => x.FamilyId);
        builder.HasIndex(x => x.TenantId);

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Device>()
            .WithMany()
            .HasForeignKey(x => x.DeviceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<RefreshToken>()
            .WithMany()
            .HasForeignKey(x => x.ReplacedByTokenId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

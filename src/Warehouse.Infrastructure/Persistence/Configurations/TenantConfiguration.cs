using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Tenancy;

namespace Warehouse.Infrastructure.Persistence.Configurations;

internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.TimeZoneId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.NegativeStockPolicy).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.TaxName).HasMaxLength(128);
        builder.Property(x => x.TaxRate).HasPrecision(9, 6);
        builder.Property(x => x.AllowStaffSelfRegistration).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();
        builder.Property(x => x.RowVersion).IsRequired().IsConcurrencyToken();

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Name);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_tenants_currency_code_length", "char_length(currency_code) = 3");
            t.HasCheckConstraint("ck_tenants_tax_rate_non_negative", "tax_rate IS NULL OR tax_rate >= 0");
            t.HasCheckConstraint("ck_tenants_max_offline_days_non_negative", "max_offline_days IS NULL OR max_offline_days >= 0");
            t.HasCheckConstraint(
                "ck_tenants_negative_stock_policy",
                "negative_stock_policy IN ('Prevent', 'Allow', 'AllowWithPermission')");
            t.HasCheckConstraint(
                "ck_tenants_tax_configuration",
                "(tax_enabled = false AND tax_name IS NULL AND tax_rate IS NULL) OR (tax_enabled = true AND tax_name IS NOT NULL AND tax_rate IS NOT NULL)");
        });
    }
}

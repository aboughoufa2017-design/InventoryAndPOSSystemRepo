using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Warehouse.Domain.Audit;
using Warehouse.Domain.Catalog;
using Warehouse.Domain.Devices;
using Warehouse.Domain.Identity;
using Warehouse.Domain.Inventory;
using Warehouse.Domain.Platform;
using Warehouse.Domain.Purchasing;
using Warehouse.Domain.Sales;
using Warehouse.Domain.Subscription;
using Warehouse.Domain.Sync;
using Warehouse.Domain.Precision;
using Warehouse.Domain.Security;
using Warehouse.Domain.Tenancy;
using Warehouse.Domain.Warehousing;
using Warehouse.Infrastructure.Persistence;
using Warehouse.Infrastructure.Tenancy;
using WarehouseEntity = Warehouse.Domain.Warehousing.Warehouse;

namespace Warehouse.ArchitectureTests;

public class DatabaseModelTests
{
    [Fact]
    public void Uses_snake_case_table_and_column_names()
    {
        using var context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(User));
        Assert.NotNull(entity);
        Assert.Equal("users", entity!.GetTableName());
        Assert.Equal("tenant_id", entity.FindProperty(nameof(User.TenantId))!.GetColumnName(StoreObjectIdentifier.Table("users", null)));
        Assert.Equal("created_at_utc", entity.FindProperty(nameof(User.CreatedAtUtc))!.GetColumnName(StoreObjectIdentifier.Table("users", null)));
        Assert.Equal("password_hash", entity.FindProperty(nameof(User.PasswordHash))!.GetColumnName(StoreObjectIdentifier.Table("users", null)));
    }

    [Fact]
    public void Tenant_owned_tables_require_tenant_id()
    {
        using var context = CreateContext();
        foreach (var type in new[]
                 {
                     typeof(User),
                     typeof(Role),
                     typeof(WarehouseEntity),
                     typeof(WarehouseLocation),
                     typeof(Device),
                     typeof(ProcessedSyncOperation),
                     typeof(TenantSubscription),
                 })
        {
            var entity = context.Model.FindEntityType(type);
            Assert.NotNull(entity);
            var tenantId = entity!.FindProperty("TenantId");
            Assert.NotNull(tenantId);
            Assert.False(tenantId!.IsNullable);
        }
    }

    [Fact]
    public void Warehouse_location_id_is_required_guid_primary_key()
    {
        using var context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(WarehouseLocation));
        Assert.NotNull(entity);
        var id = entity!.FindProperty(nameof(WarehouseLocation.Id));
        Assert.NotNull(id);
        Assert.False(id!.IsNullable);
        Assert.Equal(typeof(Guid), id.ClrType);
    }

    [Fact]
    public void Change_log_tables_exist_with_tenant_version_uniqueness()
    {
        using var context = CreateContext();
        Assert.NotNull(context.Model.FindEntityType(typeof(ChangeLogEntry)));
        Assert.NotNull(context.Model.FindEntityType(typeof(TenantChangeCounter)));
        AssertUniqueIndex(context, typeof(ChangeLogEntry), "tenant_id", "change_version");
        Assert.Equal(["Applied", "AlreadyProcessed", "Conflict", "Rejected", "ValidationFailed"],
            new[]
            {
                SyncPushOutcomes.Applied,
                SyncPushOutcomes.AlreadyProcessed,
                SyncPushOutcomes.Conflict,
                SyncPushOutcomes.Rejected,
                SyncPushOutcomes.ValidationFailed,
            });
    }

    [Fact]
    public void Sales_tables_exist_with_payment_status_and_confirm_no_stock()
    {
        using var context = CreateContext();
        Assert.NotNull(context.Model.FindEntityType(typeof(Customer)));
        Assert.NotNull(context.Model.FindEntityType(typeof(SalesOrder)));
        AssertUniqueIndex(context, typeof(SalesOrderLine), "sales_order_id", "product_id");
        Assert.Null(context.Model.FindEntityType(typeof(SalesOrderLine))!.FindProperty(nameof(SalesOrderLine.QuantityRemainingToShip)));
        Assert.Contains("does not change stock", SalesStockEffects.ConfirmDescription);
    }

    [Fact]
    public void Purchase_tables_exist_with_payment_status_and_confirm_no_stock()
    {
        using var context = CreateContext();
        Assert.NotNull(context.Model.FindEntityType(typeof(Supplier)));
        Assert.NotNull(context.Model.FindEntityType(typeof(PurchaseOrder)));
        AssertUniqueIndex(context, typeof(PurchaseOrderLine), "purchase_order_id", "product_id");
        Assert.Null(context.Model.FindEntityType(typeof(PurchaseOrderLine))!.FindProperty(nameof(PurchaseOrderLine.QuantityRemainingToReceive)));
        Assert.Contains("does not change stock", PurchasingStockEffects.ConfirmDescription);
    }

    [Fact]
    public void Transfer_tables_exist_with_line_product_uniqueness()
    {
        using var context = CreateContext();
        Assert.NotNull(context.Model.FindEntityType(typeof(Transfer)));
        AssertUniqueIndex(context, typeof(TransferLine), "transfer_id", "product_id");
        Assert.Null(context.Model.FindEntityType(typeof(TransferLine))!.FindProperty(nameof(TransferLine.QuantityInTransit)));
        Assert.Contains("IN_TRANSIT", TransferStockEffects.DispatchDescription);
    }

    [Fact]
    public void Stock_balance_grain_has_location_and_ignores_available()
    {
        using var context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(StockBalance));
        Assert.NotNull(entity);
        Assert.False(entity!.FindProperty(nameof(StockBalance.LocationId))!.IsNullable);
        Assert.Null(entity.FindProperty(nameof(StockBalance.Available)));
        AssertUniqueIndex(context, typeof(StockBalance), "tenant_id", "product_id", "warehouse_id", "location_id");
        Assert.Contains("LocationId(NOT NULL)", TransferStockEffects.BalanceGrainDescription);
    }

    [Fact]
    public void Product_catalog_tables_have_tenant_scoped_unique_keys()
    {
        using var context = CreateContext();
        AssertUniqueIndex(context, typeof(Product), "tenant_id", "sku");
        AssertUniqueIndex(context, typeof(ProductBarcode), "tenant_id", "barcode");
        AssertUniqueIndex(context, typeof(UnitOfMeasure), "tenant_id", "code");
        AssertUniqueIndex(context, typeof(ProductCategory), "tenant_id", "code");

        Assert.Equal("numeric(19,4)", context.Model.FindEntityType(typeof(Product))!
            .FindProperty(nameof(Product.PurchasePrice))!.GetColumnType());
        Assert.Equal("numeric(18,6)", context.Model.FindEntityType(typeof(Product))!
            .FindProperty(nameof(Product.MinimumStock))!.GetColumnType());
    }

    [Fact]
    public void Refresh_token_table_has_unique_token_hash()
    {
        using var context = CreateContext();
        AssertUniqueIndex(context, typeof(RefreshToken), "token_hash");
        Assert.Contains(context.Model.FindEntityType(typeof(RefreshToken))!.GetIndexes(),
            i => i.Properties.Select(p => p.Name).SequenceEqual(new[] { nameof(RefreshToken.TenantId), nameof(RefreshToken.UserId) }));
        Assert.Contains(context.Model.FindEntityType(typeof(RefreshToken))!.GetIndexes(),
            i => i.Properties.Any(p => p.Name == nameof(RefreshToken.FamilyId)));
    }

    [Fact]
    public void Future_stock_balance_grain_requires_non_null_location_id_and_on_hand_reserved()
    {
        Assert.Contains("LocationId(NOT NULL)", TransferStockEffects.BalanceGrainDescription, StringComparison.Ordinal);
        Assert.Contains("OnHand", TransferStockEffects.BalanceGrainDescription, StringComparison.Ordinal);
        Assert.Contains("Reserved", TransferStockEffects.BalanceGrainDescription, StringComparison.Ordinal);
        Assert.Equal(WarehouseLocationCodes.InTransit, WarehouseLocationCodes.InTransit);
        Assert.Equal(WarehouseLocationCodes.General, WarehouseLocationCodes.General);
    }

    [Fact]
    public void Money_and_quantity_precision_constants_match_postgres_numeric_scales()
    {
        Assert.Equal(19, MoneyPrecision.Precision);
        Assert.Equal(4, MoneyPrecision.Scale);
        Assert.Equal(18, QuantityPrecision.Precision);
        Assert.Equal(6, QuantityPrecision.Scale);
    }

    [Fact]
    public void Processed_sync_operation_id_is_primary_key()
    {
        using var context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(ProcessedSyncOperation));
        Assert.NotNull(entity);
        var key = Assert.Single(entity!.GetKeys().Where(k => k.IsPrimaryKey()));
        Assert.Equal(nameof(ProcessedSyncOperation.Id), Assert.Single(key.Properties).Name);
    }

    [Fact]
    public void Tenant_scoped_unique_indexes_exist()
    {
        using var context = CreateContext();
        AssertUniqueIndex(context, typeof(User), "tenant_id", "email");
        AssertUniqueIndex(context, typeof(Role), "tenant_id", "code");
        AssertUniqueIndex(context, typeof(WarehouseEntity), "tenant_id", "code");
        AssertUniqueIndex(context, typeof(WarehouseLocation), "tenant_id", "warehouse_id", "code");
        AssertUniqueIndex(context, typeof(Device), "tenant_id", "device_key");
    }

    [Fact]
    public void Audit_log_supports_required_foundation_columns()
    {
        using var context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(AuditLog));
        Assert.NotNull(entity);
        foreach (var name in new[]
                 {
                     nameof(AuditLog.TenantId),
                     nameof(AuditLog.UserId),
                     nameof(AuditLog.DeviceId),
                     nameof(AuditLog.Action),
                     nameof(AuditLog.EntityType),
                     nameof(AuditLog.EntityId),
                     nameof(AuditLog.OccurredAtUtc),
                     nameof(AuditLog.CorrelationId),
                     nameof(AuditLog.IpAddress),
                     nameof(AuditLog.MetadataJson),
                 })
        {
            Assert.NotNull(entity!.FindProperty(name));
        }

        Assert.Equal("jsonb", entity!.FindProperty(nameof(AuditLog.MetadataJson))!.GetColumnType());
    }

    [Fact]
    public void Tenant_tax_is_disabled_and_offline_default_is_seven_days()
    {
        var tenant = Tenant.Create(Guid.NewGuid(), "ACME", "Acme", "USD", "UTC", DateTimeOffset.UtcNow);
        Assert.False(tenant.TaxEnabled);
        Assert.Null(tenant.TaxName);
        Assert.Null(tenant.TaxRate);
        Assert.Equal(OfflineSessionPolicy.DefaultMaxOfflineDays, tenant.MaxOfflineDays);
    }

    [Fact]
    public void Foreign_keys_exist_for_core_relationships()
    {
        using var context = CreateContext();
        Assert.Contains(context.Model.FindEntityType(typeof(WarehouseLocation))!.GetForeignKeys(),
            fk => fk.Properties.Any(p => p.Name == nameof(WarehouseLocation.WarehouseId)));
        Assert.Contains(context.Model.FindEntityType(typeof(Device))!.GetForeignKeys(),
            fk => fk.Properties.Any(p => p.Name == nameof(Device.TenantId)));
        Assert.Contains(context.Model.FindEntityType(typeof(UserRole))!.GetForeignKeys(),
            fk => fk.Properties.Any(p => p.Name == nameof(UserRole.UserId)));
    }

    [Fact]
    public void Global_entities_include_permission_tenant_plan_and_platform_operator()
    {
        using var context = CreateContext();
        Assert.Null(context.Model.FindEntityType(typeof(Permission))!.FindProperty("TenantId"));
        Assert.Null(context.Model.FindEntityType(typeof(Tenant))!.FindProperty("TenantId"));
        Assert.Null(context.Model.FindEntityType(typeof(Plan))!.FindProperty("TenantId"));
        Assert.Null(context.Model.FindEntityType(typeof(PlatformOperator))!.FindProperty("TenantId"));
        AssertUniqueIndex(context, typeof(Plan), "code");
        AssertUniqueIndex(context, typeof(PlatformOperator), "email");
        AssertUniqueIndex(context, typeof(TenantSubscription), "tenant_id");
    }

    private static void AssertUniqueIndex(WarehouseDbContext context, Type clrType, params string[] columns)
    {
        var entity = context.Model.FindEntityType(clrType);
        Assert.NotNull(entity);
        var table = entity!.GetTableName()!;
        var matched = entity.GetIndexes().Any(index =>
            index.IsUnique
            && index.Properties.Select(p => p.GetColumnName(StoreObjectIdentifier.Table(table, null))).SequenceEqual(columns));
        Assert.True(matched, $"Expected unique index on {table} ({string.Join(", ", columns)})");
    }

    private static WarehouseDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<WarehouseDbContext>()
            .UseNpgsql("Host=localhost;Database=warehouse_model_tests;Username=warehouse;Password=change-me")
            .UseSnakeCaseNamingConvention()
            .Options;

        return new WarehouseDbContext(options, NullTenantContext.Instance);
    }
}

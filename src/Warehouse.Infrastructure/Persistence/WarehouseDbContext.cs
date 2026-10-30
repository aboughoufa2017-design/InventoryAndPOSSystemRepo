using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Tenancy;
using Warehouse.Domain.Audit;
using Warehouse.Domain.Catalog;
using Warehouse.Domain.Devices;
using Warehouse.Domain.Identity;
using Warehouse.Domain.Inventory;
using Warehouse.Domain.Platform;
using Warehouse.Domain.Pos;
using Warehouse.Domain.Purchasing;
using Warehouse.Domain.Sales;
using Warehouse.Domain.Subscription;
using Warehouse.Domain.Sync;
using Warehouse.Domain.Tenancy;
using Warehouse.Domain.Warehousing;
using WarehouseEntity = Warehouse.Domain.Warehousing.Warehouse;

namespace Warehouse.Infrastructure.Persistence;

public sealed class WarehouseDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<User> Users => Set<User>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();

    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<ProductBarcode> ProductBarcodes => Set<ProductBarcode>();

    public DbSet<StockBalance> StockBalances => Set<StockBalance>();

    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();

    public DbSet<StockAdjustmentLine> StockAdjustmentLines => Set<StockAdjustmentLine>();

    public DbSet<StockCount> StockCounts => Set<StockCount>();

    public DbSet<StockCountLine> StockCountLines => Set<StockCountLine>();

    public DbSet<Transfer> Transfers => Set<Transfer>();

    public DbSet<TransferLine> TransferLines => Set<TransferLine>();

    public DbSet<TransferOperation> TransferOperations => Set<TransferOperation>();

    public DbSet<Supplier> Suppliers => Set<Supplier>();

    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();

    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();

    public DbSet<PurchaseReceipt> PurchaseReceipts => Set<PurchaseReceipt>();

    public DbSet<PurchaseReceiptLine> PurchaseReceiptLines => Set<PurchaseReceiptLine>();

    public DbSet<PurchaseReturn> PurchaseReturns => Set<PurchaseReturn>();

    public DbSet<PurchaseReturnLine> PurchaseReturnLines => Set<PurchaseReturnLine>();

    public DbSet<PurchaseOperation> PurchaseOperations => Set<PurchaseOperation>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();

    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();

    public DbSet<SalesShipment> SalesShipments => Set<SalesShipment>();

    public DbSet<SalesShipmentLine> SalesShipmentLines => Set<SalesShipmentLine>();

    public DbSet<SalesReturn> SalesReturns => Set<SalesReturn>();

    public DbSet<SalesReturnLine> SalesReturnLines => Set<SalesReturnLine>();

    public DbSet<SalesOperation> SalesOperations => Set<SalesOperation>();

    public DbSet<WarehouseEntity> Warehouses => Set<WarehouseEntity>();

    public DbSet<WarehouseLocation> WarehouseLocations => Set<WarehouseLocation>();

    public DbSet<Device> Devices => Set<Device>();

    public DbSet<PosSale> PosSales => Set<PosSale>();

    public DbSet<PosSaleLine> PosSaleLines => Set<PosSaleLine>();

    public DbSet<PosPayment> PosPayments => Set<PosPayment>();

    public DbSet<CashShift> CashShifts => Set<CashShift>();

    public DbSet<CashMovement> CashMovements => Set<CashMovement>();

    public DbSet<PosInventoryConflict> PosInventoryConflicts => Set<PosInventoryConflict>();

    public DbSet<PriceList> PriceLists => Set<PriceList>();

    public DbSet<ProductPrice> ProductPrices => Set<ProductPrice>();

    public DbSet<RestaurantTable> RestaurantTables => Set<RestaurantTable>();

    public DbSet<RestaurantOpenOrder> RestaurantOpenOrders => Set<RestaurantOpenOrder>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<ProcessedSyncOperation> ProcessedSyncOperations => Set<ProcessedSyncOperation>();

    public DbSet<ChangeLogEntry> ChangeLogEntries => Set<ChangeLogEntry>();

    public DbSet<TenantChangeCounter> TenantChangeCounters => Set<TenantChangeCounter>();

    public DbSet<PlatformOperator> PlatformOperators => Set<PlatformOperator>();

    public DbSet<Plan> Plans => Set<Plan>();

    public DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();

    public Guid? CurrentTenantId => _tenantContext.TenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WarehouseDbContext).Assembly);
        ApplyTenantQueryFilters(modelBuilder);
        SystemPermissionSeed.Seed(modelBuilder);
    }

    private void ApplyTenantQueryFilters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<User>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<RolePermission>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<UserRole>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<RefreshToken>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<UnitOfMeasure>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<ProductCategory>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<Product>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<ProductBarcode>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<StockBalance>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<StockMovement>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<StockAdjustment>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<StockAdjustmentLine>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<StockCount>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<StockCountLine>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<Transfer>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<TransferLine>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<TransferOperation>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<Supplier>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<PurchaseOrder>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<PurchaseOrderLine>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<PurchaseReceipt>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<PurchaseReceiptLine>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<PurchaseReturn>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<PurchaseReturnLine>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<PurchaseOperation>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<Customer>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<SalesOrder>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<SalesOrderLine>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<SalesShipment>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<SalesShipmentLine>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<SalesReturn>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<SalesReturnLine>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<SalesOperation>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<WarehouseEntity>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<WarehouseLocation>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<Device>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<PosSale>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<PosSaleLine>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<PosPayment>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<CashShift>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<CashMovement>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<PosInventoryConflict>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<PriceList>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<ProductPrice>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<RestaurantTable>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<RestaurantOpenOrder>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<ProcessedSyncOperation>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<ChangeLogEntry>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<TenantChangeCounter>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<TenantSubscription>().HasQueryFilter(e => _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<AuditLog>().HasQueryFilter(e =>
            _tenantContext.TenantId != null && e.TenantId == _tenantContext.TenantId);
    }
}

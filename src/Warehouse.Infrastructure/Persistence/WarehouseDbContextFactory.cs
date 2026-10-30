using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Warehouse.Infrastructure.Tenancy;

namespace Warehouse.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core migrations (`dotnet ef`).
/// </summary>
public sealed class WarehouseDbContextFactory : IDesignTimeDbContextFactory<WarehouseDbContext>
{
    public WarehouseDbContext CreateDbContext(string[] args)
    {
        var connectionString = PostgresConnectionString.Normalize(
            Environment.GetEnvironmentVariable("WAREHOUSE_DB_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=warehouse;Username=warehouse;Password=change-me");

        var options = new DbContextOptionsBuilder<WarehouseDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new WarehouseDbContext(options, NullTenantContext.Instance);
    }
}

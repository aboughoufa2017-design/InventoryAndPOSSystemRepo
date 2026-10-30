using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Warehouse.Application.Tenancy;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.IntegrationTests;

[CollectionDefinition(PostgreSqlCollection.Name)]
public sealed class PostgreSqlCollection : ICollectionFixture<PostgreSqlCollectionFixture>
{
    public const string Name = "PostgreSql";
}

public sealed class PostgreSqlCollectionFixture : IAsyncLifetime
{
    public const string UnavailableReason =
        "Docker is not available on this machine. PostgreSQL integration tests were skipped.";

    private PostgreSqlContainer? _container;

    public bool IsAvailable { get; private set; }

    public string? ConnectionString { get; private set; }

    public async Task InitializeAsync()
    {
        try
        {
            _container = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .WithDatabase("warehouse")
                .WithUsername("warehouse")
                .WithPassword("change-me")
                .Build();

            await _container.StartAsync();
            ConnectionString = _container.GetConnectionString();
            IsAvailable = true;
        }
        catch
        {
            IsAvailable = false;
            ConnectionString = null;
            if (_container is not null)
            {
                await _container.DisposeAsync();
                _container = null;
            }
        }
    }

    public WarehouseDbContext CreateContext(ITenantContext tenantContext)
    {
        if (!IsAvailable || ConnectionString is null)
        {
            throw new InvalidOperationException(UnavailableReason);
        }

        var options = new DbContextOptionsBuilder<WarehouseDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new WarehouseDbContext(options, tenantContext);
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}

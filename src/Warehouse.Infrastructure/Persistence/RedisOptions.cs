namespace Warehouse.Infrastructure.Persistence;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    /// <summary>StackExchange.Redis connection string, e.g. localhost:6379.</summary>
    public string ConnectionString { get; set; } = string.Empty;
}

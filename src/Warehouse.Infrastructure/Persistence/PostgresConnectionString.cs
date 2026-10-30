using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Warehouse.Infrastructure.Persistence;

/// <summary>
/// Resolves and normalizes Postgres connection strings for local keyword format
/// and Render-style <c>postgres(ql)://</c> URIs.
/// </summary>
public static class PostgresConnectionString
{
    public static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }

    public static string Resolve(IConfiguration configuration)
    {
        var raw = FirstNonEmpty(
            configuration["Database:ConnectionString"],
            configuration["WAREHOUSE_DB_CONNECTION"],
            Environment.GetEnvironmentVariable("WAREHOUSE_DB_CONNECTION"));

        if (raw is null)
        {
            throw new InvalidOperationException(
                "Database:ConnectionString (or WAREHOUSE_DB_CONNECTION) is not configured.");
        }

        return Normalize(raw);
    }

    public static string Normalize(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        connectionString = connectionString.Trim();

        if (!connectionString.Contains("://", StringComparison.Ordinal))
        {
            return connectionString;
        }

        // Uri requires a known scheme; postgres:// and postgresql:// are equivalent.
        if (connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
        {
            connectionString = "postgresql://" + connectionString["postgres://".Length..];
        }

        if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var uri)
            || !uri.Scheme.Equals("postgresql", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Database connection string URI must use postgres:// or postgresql://.");
        }

        var userInfo = uri.UserInfo.Split(':', 2);
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "",
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "",
            Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
        };

        ApplySslFromQuery(uri.Query, builder);
        return builder.ConnectionString;
    }

    private static void ApplySslFromQuery(string query, NpgsqlConnectionStringBuilder builder)
    {
        if (string.IsNullOrEmpty(query))
        {
            return;
        }

        var trimmed = query.TrimStart('?');
        foreach (var part in trimmed.Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var pair = part.Split('=', 2);
            if (pair.Length != 2)
            {
                continue;
            }

            if (!pair[0].Equals("sslmode", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var mode = Uri.UnescapeDataString(pair[1]);
            builder.SslMode = mode.ToLowerInvariant() switch
            {
                "disable" => SslMode.Disable,
                "allow" => SslMode.Allow,
                "prefer" => SslMode.Prefer,
                "require" => SslMode.Require,
                "verify-ca" => SslMode.VerifyCA,
                "verify-full" => SslMode.VerifyFull,
                _ => builder.SslMode,
            };
        }
    }
}

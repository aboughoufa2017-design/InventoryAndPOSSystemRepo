using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Api.Hosting;

public static class ProductionStartupValidation
{
    public static void EnsureProductionSecrets(WebApplicationBuilder builder)
    {
        if (!builder.Environment.IsProduction())
        {
            return;
        }

        var errors = new List<string>();

        // appsettings leave ConnectionString/SigningKey as "" — treat blank as unset so
        // env vars (Database__*, Redis__*, JWT_SIGNING_KEY) can supply Production secrets.
        var db = FirstNonEmpty(
            builder.Configuration["Database:ConnectionString"],
            builder.Configuration["WAREHOUSE_DB_CONNECTION"]);
        if (db is null)
        {
            errors.Add("Database:ConnectionString (or WAREHOUSE_DB_CONNECTION) is required in Production.");
        }

        var redis = FirstNonEmpty(builder.Configuration["Redis:ConnectionString"]);
        if (redis is null)
        {
            errors.Add("Redis:ConnectionString is required in Production.");
        }

        var jwt = FirstNonEmpty(
            builder.Configuration["Jwt:SigningKey"],
            builder.Configuration["JWT_SIGNING_KEY"],
            Environment.GetEnvironmentVariable("JWT_SIGNING_KEY"));
        if (jwt is null || jwt.Length < 32)
        {
            errors.Add("Jwt:SigningKey / JWT_SIGNING_KEY must be at least 32 characters in Production.");
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                "Production configuration invalid:" + Environment.NewLine + string.Join(Environment.NewLine, errors));
        }
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }
}

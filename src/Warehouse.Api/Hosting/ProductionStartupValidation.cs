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

        var db = builder.Configuration["Database:ConnectionString"]
            ?? builder.Configuration["WAREHOUSE_DB_CONNECTION"];
        if (string.IsNullOrWhiteSpace(db))
        {
            errors.Add("Database:ConnectionString (or WAREHOUSE_DB_CONNECTION) is required in Production.");
        }

        var redis = builder.Configuration["Redis:ConnectionString"];
        if (string.IsNullOrWhiteSpace(redis))
        {
            errors.Add("Redis:ConnectionString is required in Production.");
        }

        var jwt = builder.Configuration["Jwt:SigningKey"]
            ?? builder.Configuration["JWT_SIGNING_KEY"]
            ?? Environment.GetEnvironmentVariable("JWT_SIGNING_KEY");
        if (string.IsNullOrWhiteSpace(jwt) || jwt.Length < 32)
        {
            errors.Add("Jwt:SigningKey / JWT_SIGNING_KEY must be at least 32 characters in Production.");
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                "Production configuration invalid:" + Environment.NewLine + string.Join(Environment.NewLine, errors));
        }
    }
}

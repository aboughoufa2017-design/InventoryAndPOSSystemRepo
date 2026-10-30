using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Warehouse.Infrastructure.Persistence;
using Warehouse.Shared.Api;

namespace Warehouse.Api.Hosting;

public static class ProductionHardeningExtensions
{
    public const string AuthRateLimitPolicy = "auth";

    public static WebApplicationBuilder AddWarehouseObservability(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "Warehouse.Api")
            .WriteTo.Console(new Serilog.Formatting.Compact.RenderedCompactJsonFormatter())
            .CreateLogger();

        builder.Host.UseSerilog();

        // Quiet by default. Set OTEL_EXPORTER_CONSOLE=true to dump spans/metrics to stdout.
        var exportConsole = string.Equals(
            builder.Configuration["OTEL_EXPORTER_CONSOLE"]
                ?? Environment.GetEnvironmentVariable("OTEL_EXPORTER_CONSOLE"),
            "true",
            StringComparison.OrdinalIgnoreCase);

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                serviceName: "Warehouse.Api",
                serviceVersion: typeof(ProductionHardeningExtensions).Assembly.GetName().Version?.ToString() ?? "0.0.0"))
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation();
                tracing.AddHttpClientInstrumentation();
                if (exportConsole)
                {
                    tracing.AddConsoleExporter();
                }
            })
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation();
                metrics.AddRuntimeInstrumentation();
                if (exportConsole)
                {
                    metrics.AddConsoleExporter();
                }
            });

        return builder;
    }

    public static IServiceCollection AddWarehouseRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
            {
                var correlationId = context.HttpContext.Items[Http.CorrelationIdMiddleware.ItemKey] as string
                    ?? context.HttpContext.TraceIdentifier;
                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsJsonAsync(
                    new ApiErrorResponse(
                        "RATE_LIMITED",
                        "Too many requests. Try again later.",
                        correlationId),
                    cancellationToken);
            };

            options.AddPolicy(AuthRateLimitPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 20,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true,
                    }));

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var path = httpContext.Request.Path.Value ?? string.Empty;
                if (path.StartsWith("/health", StringComparison.OrdinalIgnoreCase)
                    || path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
                {
                    return RateLimitPartition.GetNoLimiter("health");
                }

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 300,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true,
                    });
            });
        });

        return services;
    }

    public static IHealthChecksBuilder AddWarehouseReadyChecks(
        this IHealthChecksBuilder healthChecks,
        IConfiguration configuration)
    {
        var redis = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>()
            ?? new RedisOptions();

        string? postgresConnectionString = null;
        try
        {
            postgresConnectionString = PostgresConnectionString.Resolve(configuration);
        }
        catch (InvalidOperationException)
        {
            // Reported as unhealthy check below.
        }

        if (!string.IsNullOrWhiteSpace(postgresConnectionString))
        {
            healthChecks.AddNpgSql(
                postgresConnectionString,
                name: "postgres",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready"]);
        }
        else
        {
            healthChecks.AddCheck(
                "postgres",
                () => HealthCheckResult.Unhealthy("Database:ConnectionString is not configured."),
                tags: ["ready"]);
        }

        if (!string.IsNullOrWhiteSpace(redis.ConnectionString))
        {
            healthChecks.AddRedis(
                redis.ConnectionString,
                name: "redis",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready"]);
        }
        else
        {
            healthChecks.AddCheck(
                "redis",
                () => HealthCheckResult.Unhealthy("Redis:ConnectionString is not configured."),
                tags: ["ready"]);
        }

        return healthChecks;
    }
}

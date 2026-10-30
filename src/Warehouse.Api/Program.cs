using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Warehouse.Api.Authorization;
using Warehouse.Api.Endpoints;
using Warehouse.Api.Hosting;
using Warehouse.Api.Http;
using Warehouse.Infrastructure.Persistence;
using Warehouse.Shared.Api;
using CorsOptions = Warehouse.Api.Hosting.CorsOptions;

if (args.Contains("--migrate-only", StringComparer.OrdinalIgnoreCase))
{
    var migrateBuilder = WebApplication.CreateBuilder(args);
    ProductionStartupValidation.EnsureProductionSecrets(migrateBuilder);
    migrateBuilder.Services.AddWarehousePersistence(migrateBuilder.Configuration);
    await using var migrateApp = migrateBuilder.Build();
    await using var scope = migrateApp.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<Warehouse.Infrastructure.Persistence.WarehouseDbContext>();
    await db.Database.MigrateAsync();
    await Warehouse.Infrastructure.Platform.PlatformBootstrap.EnsurePlansAsync(db, CancellationToken.None);
    Console.WriteLine("Migrations applied.");
    return;
}

var builder = WebApplication.CreateBuilder(args);

ProductionStartupValidation.EnsureProductionSecrets(builder);
builder.AddWarehouseObservability();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddExceptionHandler<UnhandledExceptionHandler>();
builder.Services.AddProblemDetails();

var healthChecks = builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"]);

if (!builder.Environment.IsEnvironment("Testing"))
{
    healthChecks.AddWarehouseReadyChecks(builder.Configuration);
    builder.Services.AddWarehouseRateLimiting();
    builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection(RedisOptions.SectionName));
    builder.Services.AddWarehousePersistence(builder.Configuration);
    builder.Services.AddWarehouseAuthentication(builder.Configuration);
    builder.Services.AddWarehouseAuthorizationPolicies();
}

builder.Services.Configure<CorsOptions>(builder.Configuration.GetSection(CorsOptions.SectionName));
builder.Services.AddCors(options =>
{
    options.AddPolicy("WebDev", policy =>
        policy.WithOrigins(
                "http://localhost:5173",
                "http://127.0.0.1:5173",
                "http://localhost:5174",
                "http://127.0.0.1:5174")
            .AllowAnyHeader()
            .AllowAnyMethod());

    options.AddPolicy("ProductionWeb", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
        if (origins.Length == 0)
        {
            policy.SetIsOriginAllowed(_ => false);
        }
        else
        {
            policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
        }
    });
});

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseSerilogRequestLogging();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("WebDev");
}
else if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseCors("ProductionWeb");
}

if (!app.Environment.IsEnvironment("Testing"))
{
    if (!app.Environment.IsProduction())
    {
        app.UseHttpsRedirection();
    }

    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseMiddleware<TenantContextMiddleware>();
    app.UseAuthorization();
}

app.MapGet("/health/live", async (HealthCheckService healthChecksService, CancellationToken cancellationToken) =>
    {
        var report = await healthChecksService.CheckHealthAsync(
            registration => registration.Tags.Contains("live"),
            cancellationToken);

        var body = new LiveHealthResponse(report.Status.ToString());
        return report.Status == HealthStatus.Healthy
            ? Results.Ok(body)
            : Results.Json(body, statusCode: StatusCodes.Status503ServiceUnavailable);
    })
    .WithName("GetLiveHealth")
    .WithTags("Health")
    .Produces<LiveHealthResponse>(StatusCodes.Status200OK)
    .Produces<LiveHealthResponse>(StatusCodes.Status503ServiceUnavailable)
    .AllowAnonymous();

app.MapGet("/health/ready", async (HealthCheckService healthChecksService, CancellationToken cancellationToken) =>
    {
        var report = await healthChecksService.CheckHealthAsync(
            registration => registration.Tags.Contains("ready"),
            cancellationToken);

        var checks = report.Entries.ToDictionary(
            entry => entry.Key,
            entry => entry.Value.Status.ToString(),
            StringComparer.OrdinalIgnoreCase);

        var body = new ReadyHealthResponse(report.Status.ToString(), checks);
        return report.Status == HealthStatus.Healthy
            ? Results.Ok(body)
            : Results.Json(body, statusCode: StatusCodes.Status503ServiceUnavailable);
    })
    .WithName("GetReadyHealth")
    .WithTags("Health")
    .Produces<ReadyHealthResponse>(StatusCodes.Status200OK)
    .Produces<ReadyHealthResponse>(StatusCodes.Status503ServiceUnavailable)
    .AllowAnonymous();

if (!app.Environment.IsEnvironment("Testing"))
{
    app.MapAuthEndpoints();
    app.MapDeviceEndpoints();
    app.MapIdentityEndpoints();
    app.MapCatalogEndpoints();
    app.MapInventoryEndpoints();
    app.MapTransferEndpoints();
    app.MapPurchasingEndpoints();
    app.MapSalesEndpoints();
    app.MapPosEndpoints();
    app.MapSyncEndpoints();
    app.MapPlatformEndpoints();
    app.MapStockCountEndpoints();
    app.MapReportEndpoints();
    app.MapBillingEndpoints();

    await Warehouse.Infrastructure.Platform.PlatformBootstrap.EnsureAsync(app.Services);
}

try
{
    app.Run();
}
finally
{
    await Log.CloseAndFlushAsync();
}

public partial class Program;

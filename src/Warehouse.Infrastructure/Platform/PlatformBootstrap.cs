using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Warehouse.Application.Security;
using Warehouse.Domain.Identity;
using Warehouse.Domain.Platform;
using Warehouse.Domain.Subscription;
using Warehouse.Infrastructure.Persistence;
using Warehouse.Infrastructure.Tenancy;

namespace Warehouse.Infrastructure.Platform;

public sealed class PlatformOptions
{
    public const string SectionName = "Platform";

    public string? BootstrapEmail { get; set; }

    public string? BootstrapPassword { get; set; }

    public string BootstrapDisplayName { get; set; } = "Platform Admin";
}

public static class PlatformBootstrap
{
    public static async Task EnsureAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("PlatformBootstrap");

        try
        {
            var environment = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Hosting.IHostEnvironment>();
            var configuration = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
            var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var options = scope.ServiceProvider.GetRequiredService<IOptions<PlatformOptions>>().Value;

            var restored = false;
            try
            {
                restored = await StartupDatabaseRestore.TryRestoreIfEmptyAsync(
                    configuration,
                    logger,
                    cancellationToken);
            }
            catch (Exception restoreEx)
            {
                // Seed restore must not block migrate + platform admin bootstrap.
                logger.LogError(
                    restoreEx,
                    "Startup seed restore failed; continuing with migrate/bootstrap.");
            }

            var migrateOnStart = string.Equals(
                configuration["WAREHOUSE_MIGRATE_ON_START"],
                "true",
                StringComparison.OrdinalIgnoreCase);

            if (environment.IsProduction() && !restored && !migrateOnStart)
            {
                // Production migrations normally run via migrate job — unless we just restored seed
                // data or WAREHOUSE_MIGRATE_ON_START=true (Render free tier).
                if (!await db.Database.CanConnectAsync(cancellationToken))
                {
                    throw new InvalidOperationException("Cannot connect to the database.");
                }

                logger.LogInformation("Production bootstrap: skipping MigrateAsync (use migrate job or set WAREHOUSE_MIGRATE_ON_START=true).");
            }
            else
            {
                await db.Database.MigrateAsync(cancellationToken);
            }

            await EnsurePlansAsync(db, cancellationToken);
            await EnsureSystemRolePermissionsAsync(db, logger, cancellationToken);
            await EnsureBootstrapOperatorAsync(db, hasher, options, logger, cancellationToken);
        }
        catch (Exception ex)
        {
            // Do not prevent the host from listening when local Postgres is down or misconfigured.
            logger.LogError(
                ex,
                "Platform bootstrap failed (migrations / plans / operator). API is up; fix Database:ConnectionString and retry.");
        }
    }

    public static async Task EnsurePlansAsync(WarehouseDbContext db, CancellationToken cancellationToken)
    {
        var utcNow = DateTimeOffset.UtcNow;
        foreach (var seed in SystemPlans.Catalog)
        {
            var existing = await db.Plans.SingleOrDefaultAsync(p => p.Id == seed.Id, cancellationToken);
            if (existing is null)
            {
                db.Plans.Add(Plan.Create(
                    seed.Id,
                    seed.Code,
                    seed.Name,
                    seed.FeatureCodes,
                    seed.MaxUsers,
                    seed.MaxWarehouses,
                    seed.MaxDevices,
                    seed.MaxProducts,
                    utcNow));
            }
            else
            {
                existing.ReplaceFeatures(seed.FeatureCodes);
                if (!existing.IsActive)
                {
                    existing.Activate(utcNow);
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Backfills missing template permissions onto existing system roles (e.g. pos.* after upgrade).
    /// Also creates missing system roles such as Cashier.
    /// </summary>
    public static async Task EnsureSystemRolePermissionsAsync(
        WarehouseDbContext db,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTimeOffset.UtcNow;
        var permissionByCode = await db.Permissions.AsNoTracking()
            .ToDictionaryAsync(p => p.Code, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var tenants = await db.Tenants.AsNoTracking().Select(t => t.Id).ToListAsync(cancellationToken);
        var added = 0;

        foreach (var tenantId in tenants)
        {
            var roles = await db.Roles.IgnoreQueryFilters()
                .Where(r => r.TenantId == tenantId && r.IsSystem)
                .ToListAsync(cancellationToken);

            var roleByCode = roles.ToDictionary(r => r.Code, StringComparer.OrdinalIgnoreCase);

            foreach (var template in SystemRoles.Templates)
            {
                if (!roleByCode.TryGetValue(template.Code, out var role))
                {
                    role = Role.Create(Guid.NewGuid(), tenantId, template.Code, template.Name, isSystem: true, utcNow);
                    db.Roles.Add(role);
                    roleByCode[template.Code] = role;
                    added++;
                }

                var existingPermissionIds = await db.RolePermissions.IgnoreQueryFilters()
                    .Where(rp => rp.TenantId == tenantId && rp.RoleId == role.Id)
                    .Select(rp => rp.PermissionId)
                    .ToListAsync(cancellationToken);
                var existingSet = existingPermissionIds.ToHashSet();

                foreach (var code in template.PermissionCodes)
                {
                    if (!permissionByCode.TryGetValue(code, out var permission))
                    {
                        continue;
                    }

                    if (existingSet.Contains(permission.Id))
                    {
                        continue;
                    }

                    db.RolePermissions.Add(RolePermission.Create(
                        Guid.NewGuid(),
                        tenantId,
                        role.Id,
                        permission.Id,
                        utcNow));
                    existingSet.Add(permission.Id);
                    added++;
                }
            }
        }

        if (added > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Synced {Count} system role permission/role rows for existing tenants.", added);
        }
    }

    private static async Task EnsureBootstrapOperatorAsync(
        WarehouseDbContext db,
        IPasswordHasher hasher,
        PlatformOptions options,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.BootstrapEmail)
            || string.IsNullOrWhiteSpace(options.BootstrapPassword))
        {
            return;
        }

        var email = options.BootstrapEmail.Trim().ToLowerInvariant();
        var exists = await db.PlatformOperators.AnyAsync(o => o.Email == email, cancellationToken);
        if (exists)
        {
            return;
        }

        db.PlatformOperators.Add(PlatformOperator.Create(
            Guid.NewGuid(),
            email,
            hasher.HashPassword(options.BootstrapPassword),
            options.BootstrapDisplayName,
            DateTimeOffset.UtcNow));
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Bootstrapped platform operator {Email}", email);
    }
}

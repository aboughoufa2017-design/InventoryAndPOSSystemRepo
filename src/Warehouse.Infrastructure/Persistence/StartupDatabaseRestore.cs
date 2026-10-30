using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Warehouse.Infrastructure.Persistence;

/// <summary>
/// Optionally restores a pg_dump -Fc file into an empty database on API start
/// (used to ship local seed data to Render). Skips when the DB already has tables.
/// </summary>
public static class StartupDatabaseRestore
{
    public const string DefaultDumpPath = "/app/seed/warehouse.dump";

    /// <returns>True when a restore was executed.</returns>
    public static async Task<bool> TryRestoreIfEmptyAsync(
        IConfiguration configuration,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var dumpPath = PostgresConnectionString.FirstNonEmpty(
            configuration["WAREHOUSE_RESTORE_DUMP_PATH"],
            configuration["Database:RestoreDumpPath"],
            DefaultDumpPath) ?? DefaultDumpPath;

        var force = string.Equals(
            configuration["WAREHOUSE_RESTORE_ON_START"],
            "true",
            StringComparison.OrdinalIgnoreCase);

        if (!File.Exists(dumpPath))
        {
            logger.LogInformation("Startup restore skipped: dump not found at {DumpPath}", dumpPath);
            return false;
        }

        string connectionString;
        try
        {
            connectionString = PostgresConnectionString.Resolve(configuration);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Startup restore skipped: database connection string not configured.");
            return false;
        }

        await using (var connection = new NpgsqlConnection(connectionString))
        {
            await connection.OpenAsync(cancellationToken);
            var shouldRestore = force || await IsSeedRestoreNeededAsync(connection, cancellationToken);
            if (!shouldRestore)
            {
                logger.LogInformation("Startup restore skipped: database already has application data.");
                return false;
            }

            if (force)
            {
                logger.LogWarning(
                    "WAREHOUSE_RESTORE_ON_START=true — restoring {DumpPath} over existing data.",
                    dumpPath);
            }
        }

        logger.LogInformation("Restoring database from {DumpPath}…", dumpPath);
        RunPgRestore(connectionString, dumpPath, logger);
        logger.LogInformation("Database restore from {DumpPath} completed.", dumpPath);
        return true;
    }

    /// <summary>
    /// Restore when schema is empty, or when migrations exist but there is no tenant data yet
    /// (typical after a fresh migrate-only on Render).
    /// </summary>
    private static async Task<bool> IsSeedRestoreNeededAsync(
        NpgsqlConnection connection,
        CancellationToken cancellationToken)
    {
        await using (var tablesCmd = new NpgsqlCommand(
                           """
                           SELECT NOT EXISTS (
                               SELECT 1
                               FROM information_schema.tables
                               WHERE table_schema = 'public'
                                 AND table_type = 'BASE TABLE'
                           );
                           """,
                           connection))
        {
            if (await tablesCmd.ExecuteScalarAsync(cancellationToken) is true)
            {
                return true;
            }
        }

        await using var tenantsExistCmd = new NpgsqlCommand(
            """
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.tables
                WHERE table_schema = 'public'
                  AND table_name = 'tenants'
            );
            """,
            connection);
        if (await tenantsExistCmd.ExecuteScalarAsync(cancellationToken) is not true)
        {
            return true;
        }

        await using var tenantCountCmd = new NpgsqlCommand("SELECT COUNT(*)::int FROM tenants;", connection);
        var tenantCount = await tenantCountCmd.ExecuteScalarAsync(cancellationToken);
        return tenantCount is 0 or null;
    }

    private static void RunPgRestore(string connectionString, string dumpPath, ILogger logger)
    {
        var pgRestore = FindPgRestore();
        var startInfo = new ProcessStartInfo
        {
            FileName = pgRestore,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        startInfo.ArgumentList.Add("--clean");
        startInfo.ArgumentList.Add("--if-exists");
        startInfo.ArgumentList.Add("--no-owner");
        startInfo.ArgumentList.Add("--no-acl");
        startInfo.ArgumentList.Add("--dbname");
        startInfo.ArgumentList.Add(connectionString);
        startInfo.ArgumentList.Add(dumpPath);

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Failed to start {pgRestore}.");

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (!string.IsNullOrWhiteSpace(stdout))
        {
            logger.LogInformation("pg_restore stdout: {Output}", stdout.Trim());
        }

        if (!string.IsNullOrWhiteSpace(stderr))
        {
            // pg_restore writes notices to stderr; exit 1 is often non-fatal warnings.
            logger.LogWarning("pg_restore stderr: {Output}", stderr.Trim());
        }

        if (process.ExitCode > 1)
        {
            throw new InvalidOperationException($"pg_restore failed with exit code {process.ExitCode}.");
        }
    }

    private static string FindPgRestore()
    {
        var fromEnv = Environment.GetEnvironmentVariable("PG_RESTORE");
        if (!string.IsNullOrWhiteSpace(fromEnv) && File.Exists(fromEnv))
        {
            return fromEnv;
        }

        foreach (var candidate in new[]
                 {
                     "pg_restore",
                     @"C:\Program Files\PostgreSQL\18\bin\pg_restore.exe",
                     @"C:\Program Files\PostgreSQL\17\bin\pg_restore.exe",
                     @"C:\Program Files\PostgreSQL\16\bin\pg_restore.exe",
                 })
        {
            if (candidate is "pg_restore")
            {
                return candidate;
            }

            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return "pg_restore";
    }
}

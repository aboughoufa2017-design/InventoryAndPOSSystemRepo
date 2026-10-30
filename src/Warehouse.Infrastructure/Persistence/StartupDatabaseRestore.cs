using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Warehouse.Infrastructure.Persistence;

/// <summary>
/// Optionally restores a seed SQL dump into an empty database on API start
/// (used to ship local seed data to Render). Skips when the DB already has tenants.
/// Prefer plain SQL (<c>pg_dump -Fp</c>) so container <c>psql</c> need not match
/// the dump tool major version (custom -Fc archives are version-locked).
/// </summary>
public static class StartupDatabaseRestore
{
    public const string DefaultDumpPath = "/app/seed/warehouse.sql";

    /// <returns>True when a restore was executed.</returns>
    public static async Task<bool> TryRestoreIfEmptyAsync(
        IConfiguration configuration,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var dumpPath = ResolveDumpPath(configuration);

        var force = string.Equals(
            configuration["WAREHOUSE_RESTORE_ON_START"],
            "true",
            StringComparison.OrdinalIgnoreCase);

        if (dumpPath is null)
        {
            logger.LogInformation(
                "Startup restore skipped: no seed file (tried warehouse.sql / warehouse.dump under /app/seed).");
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
        if (dumpPath.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
        {
            RunPsqlFile(connectionString, dumpPath, logger);
        }
        else
        {
            RunPgRestore(connectionString, dumpPath, logger);
        }

        logger.LogInformation("Database restore from {DumpPath} completed.", dumpPath);
        return true;
    }

    private static string? ResolveDumpPath(IConfiguration configuration)
    {
        var configured = PostgresConnectionString.FirstNonEmpty(
            configuration["WAREHOUSE_RESTORE_DUMP_PATH"],
            configuration["Database:RestoreDumpPath"]);
        if (configured is not null && File.Exists(configured))
        {
            return configured;
        }

        foreach (var candidate in new[]
                 {
                     DefaultDumpPath,
                     "/app/seed/warehouse.dump",
                     Path.Combine(AppContext.BaseDirectory, "seed", "warehouse.sql"),
                     Path.Combine(AppContext.BaseDirectory, "seed", "warehouse.dump"),
                 })
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return configured is not null && File.Exists(configured) ? configured : null;
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

    private static void RunPsqlFile(string connectionString, string sqlPath, ILogger logger)
    {
        var psql = FindTool("psql", "PG_PSQL");
        var startInfo = new ProcessStartInfo
        {
            FileName = psql,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        startInfo.ArgumentList.Add("--dbname");
        startInfo.ArgumentList.Add(connectionString);
        startInfo.ArgumentList.Add("--set");
        startInfo.ArgumentList.Add("ON_ERROR_STOP=1");
        startInfo.ArgumentList.Add("--file");
        startInfo.ArgumentList.Add(sqlPath);

        RunProcess(startInfo, "psql", logger, fatalExitAbove: 0);
    }

    private static void RunPgRestore(string connectionString, string dumpPath, ILogger logger)
    {
        var pgRestore = FindTool("pg_restore", "PG_RESTORE");
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

        // pg_restore exit 1 is often non-fatal warnings.
        RunProcess(startInfo, "pg_restore", logger, fatalExitAbove: 1);
    }

    private static void RunProcess(
        ProcessStartInfo startInfo,
        string toolName,
        ILogger logger,
        int fatalExitAbove)
    {
        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Failed to start {toolName}.");

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (!string.IsNullOrWhiteSpace(stdout))
        {
            logger.LogInformation("{Tool} stdout: {Output}", toolName, stdout.Trim());
        }

        if (!string.IsNullOrWhiteSpace(stderr))
        {
            logger.LogWarning("{Tool} stderr: {Output}", toolName, stderr.Trim());
        }

        if (process.ExitCode > fatalExitAbove)
        {
            throw new InvalidOperationException($"{toolName} failed with exit code {process.ExitCode}.");
        }
    }

    private static string FindTool(string name, string envVar)
    {
        var fromEnv = Environment.GetEnvironmentVariable(envVar);
        if (!string.IsNullOrWhiteSpace(fromEnv) && File.Exists(fromEnv))
        {
            return fromEnv;
        }

        foreach (var candidate in new[]
                 {
                     name,
                     $@"C:\Program Files\PostgreSQL\18\bin\{name}.exe",
                     $@"C:\Program Files\PostgreSQL\17\bin\{name}.exe",
                     $@"C:\Program Files\PostgreSQL\16\bin\{name}.exe",
                 })
        {
            if (candidate == name)
            {
                return candidate;
            }

            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return name;
    }
}

using System.Runtime.CompilerServices;

namespace Warehouse.ArchitectureTests;

internal static class RepoRoot
{
    public static string Locate([CallerFilePath] string? sourceFile = null)
    {
        foreach (var start in CandidateStarts(sourceFile))
        {
            var directory = new DirectoryInfo(start);
            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "Warehouse.sln")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }
        }

        throw new InvalidOperationException(
            "Could not locate Warehouse.sln from the test assembly or source tree.");
    }

    private static IEnumerable<string> CandidateStarts(string? sourceFile)
    {
        var env = Environment.GetEnvironmentVariable("WAREHOUSE_REPO_ROOT");
        if (!string.IsNullOrWhiteSpace(env))
        {
            yield return env;
        }

        if (!string.IsNullOrWhiteSpace(sourceFile))
        {
            var sourceDir = Path.GetDirectoryName(sourceFile);
            if (!string.IsNullOrWhiteSpace(sourceDir))
            {
                yield return sourceDir;
            }
        }

        yield return AppContext.BaseDirectory;
    }
}

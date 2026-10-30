using Microsoft.EntityFrameworkCore;
using Warehouse.Domain.Identity;

namespace Warehouse.Infrastructure.Persistence;

internal static class SystemPermissionSeed
{
    // Stable ids so permission references stay constant across environments.
    private static readonly Guid[] Ids =
    [
        Guid.Parse("11111111-1111-1111-1111-111111111101"),
        Guid.Parse("11111111-1111-1111-1111-111111111102"),
        Guid.Parse("11111111-1111-1111-1111-111111111103"),
        Guid.Parse("11111111-1111-1111-1111-111111111104"),
        Guid.Parse("11111111-1111-1111-1111-111111111105"),
        Guid.Parse("11111111-1111-1111-1111-111111111106"),
        Guid.Parse("11111111-1111-1111-1111-111111111107"),
        Guid.Parse("11111111-1111-1111-1111-111111111108"),
        Guid.Parse("11111111-1111-1111-1111-111111111109"),
        Guid.Parse("11111111-1111-1111-1111-111111111110"),
        Guid.Parse("11111111-1111-1111-1111-111111111111"),
        Guid.Parse("11111111-1111-1111-1111-111111111112"),
        Guid.Parse("11111111-1111-1111-1111-111111111113"),
        Guid.Parse("11111111-1111-1111-1111-111111111114"),
        Guid.Parse("11111111-1111-1111-1111-111111111115"),
        Guid.Parse("11111111-1111-1111-1111-111111111116"),
        Guid.Parse("11111111-1111-1111-1111-111111111117"),
        Guid.Parse("11111111-1111-1111-1111-111111111118"),
        Guid.Parse("11111111-1111-1111-1111-111111111119"),
    ];

    public static void Seed(ModelBuilder modelBuilder)
    {
        var permissions = SystemPermissions.Catalog
            .Select((item, index) => Permission.Create(Ids[index], item.Code, item.Name))
            .ToArray();

        modelBuilder.Entity<Permission>().HasData(permissions);
    }
}

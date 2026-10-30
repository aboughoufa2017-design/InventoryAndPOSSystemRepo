using Warehouse.Domain.Common;

namespace Warehouse.Domain.Identity;

/// <summary>
/// Well-known permission codes seeded into the global permission catalog.
/// </summary>
public static class SystemPermissions
{
    public static readonly IReadOnlyList<(string Code, string Name)> Catalog =
    [
        ("products.read", "Read products"),
        ("products.create", "Create products"),
        ("products.update", "Update products"),
        ("products.delete", "Delete products"),
        ("inventory.read", "Read inventory"),
        ("inventory.receive", "Receive inventory"),
        ("inventory.issue", "Issue inventory"),
        ("inventory.adjust", "Adjust inventory"),
        ("inventory.transfer", "Transfer inventory"),
        ("purchases.create", "Create purchases"),
        ("purchases.approve", "Approve purchases"),
        ("sales.create", "Create sales"),
        ("sales.cancel", "Cancel sales"),
        ("reports.read", "Read reports"),
        ("users.manage", "Manage users"),
        ("pos.sale", "POS checkout"),
        ("pos.void", "Void POS sales"),
        ("pos.refund", "Refund POS sales"),
        ("pos.shift", "POS cash shifts"),
    ];
}

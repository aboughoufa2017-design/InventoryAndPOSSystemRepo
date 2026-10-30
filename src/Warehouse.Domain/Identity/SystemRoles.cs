namespace Warehouse.Domain.Identity;

/// <summary>
/// System role templates provisioned per tenant with <c>IsSystem = true</c>.
/// </summary>
public static class SystemRoles
{
    public const string Owner = "Owner";
    public const string Administrator = "Administrator";
    public const string WarehouseManager = "WarehouseManager";
    public const string WarehouseWorker = "WarehouseWorker";
    public const string SalesUser = "SalesUser";
    public const string Cashier = "Cashier";
    public const string PurchasingUser = "PurchasingUser";
    public const string Accountant = "Accountant";
    public const string Viewer = "Viewer";

    public static readonly IReadOnlyList<SystemRoleTemplate> Templates =
    [
        new(Owner, "Owner", AllPermissions),
        new(Administrator, "Administrator", AllPermissions),
        new(WarehouseManager, "Warehouse manager",
        [
            "products.read", "products.create", "products.update", "products.delete",
            "inventory.read", "inventory.receive", "inventory.issue", "inventory.adjust", "inventory.transfer",
            "pos.sale", "pos.void", "pos.refund", "pos.shift",
            "reports.read",
        ]),
        new(WarehouseWorker, "Warehouse worker",
        [
            "products.read",
            "inventory.read", "inventory.receive", "inventory.issue", "inventory.transfer",
        ]),
        new(SalesUser, "Sales user",
        [
            "products.read",
            "inventory.read",
            "sales.create", "sales.cancel",
            "pos.sale", "pos.void", "pos.refund", "pos.shift",
            "inventory.issue",
        ]),
        new(Cashier, "Cashier",
        [
            "products.read",
            "inventory.read",
            "pos.sale", "pos.shift",
            "inventory.issue",
        ]),
        new(PurchasingUser, "Purchasing user",
        [
            "products.read",
            "inventory.read", "inventory.receive",
            "purchases.create", "purchases.approve",
        ]),
        new(Accountant, "Accountant",
        [
            "products.read",
            "inventory.read",
            "reports.read",
        ]),
        new(Viewer, "Viewer",
        [
            "products.read",
            "inventory.read",
            "reports.read",
        ]),
    ];

    private static IReadOnlyList<string> AllPermissions =>
        SystemPermissions.Catalog.Select(p => p.Code).ToArray();
}

public sealed record SystemRoleTemplate(
    string Code,
    string Name,
    IReadOnlyList<string> PermissionCodes);

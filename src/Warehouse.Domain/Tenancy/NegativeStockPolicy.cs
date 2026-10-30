namespace Warehouse.Domain.Tenancy;

/// <summary>
/// Negative stock behavior configured per tenant. Default is Prevent.
/// </summary>
public enum NegativeStockPolicy
{
    Prevent = 0,
    Allow = 1,
    AllowWithPermission = 2,
}

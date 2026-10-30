using Warehouse.Domain.Tenancy;

namespace Warehouse.Domain.Inventory;

/// <summary>
/// Evaluates whether a resulting OnHand quantity is allowed under the tenant policy.
/// Warn is not a supported policy.
/// </summary>
public static class NegativeStockEvaluator
{
    public static bool IsAllowed(
        NegativeStockPolicy policy,
        decimal resultingOnHand,
        bool callerHasNegativeStockPermission)
    {
        if (resultingOnHand >= 0m)
        {
            return true;
        }

        return policy switch
        {
            NegativeStockPolicy.Allow => true,
            NegativeStockPolicy.AllowWithPermission => callerHasNegativeStockPermission,
            NegativeStockPolicy.Prevent => false,
            _ => false,
        };
    }
}

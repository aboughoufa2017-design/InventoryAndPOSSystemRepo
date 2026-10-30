namespace Warehouse.Domain.Purchasing;

/// <summary>
/// Purchasing inventory effects are documented for architecture tests and module docs.
/// Confirmation alone never changes OnHand; receiving and returns post StockMovements.
/// </summary>
public static class PurchasingStockEffects
{
    public const string ConfirmDescription = "Confirm does not change stock.";

    public const string ReceiveDescription = "Receive increases OnHand at the destination location.";

    public const string ReturnDescription = "Return decreases OnHand at the return location.";
}

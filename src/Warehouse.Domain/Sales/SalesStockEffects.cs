namespace Warehouse.Domain.Sales;

/// <summary>
/// Sales inventory effects for architecture tests and module docs.
/// Confirmation alone never changes OnHand; shipping and returns post StockMovements.
/// </summary>
public static class SalesStockEffects
{
    public const string ConfirmDescription = "Confirm does not change stock.";

    public const string ShipDescription = "Ship decreases OnHand at the source location.";

    public const string ReturnDescription = "Return increases OnHand at the return location.";
}

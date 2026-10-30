namespace Warehouse.Domain.Inventory;

/// <summary>
/// Semantic labels only. Physical storage is always by WarehouseLocation.
/// Available = non-in-transit location OnHand usable after subtracting Reserved.
/// InTransit = OnHand on a location with kind InTransit (code IN_TRANSIT).
/// </summary>
public enum StockBalanceKind
{
    Available = 0,
    InTransit = 1,
}

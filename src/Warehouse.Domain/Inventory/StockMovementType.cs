namespace Warehouse.Domain.Inventory;

public enum StockMovementType
{
    OpeningBalance = 0,
    Adjustment = 1,
    TransferDispatch = 2,
    TransferReceive = 3,
    PurchaseReceipt = 4,
    PurchaseReturn = 5,
    SalesShipment = 6,
    SalesReturn = 7,
}

/// <summary>
/// Stable string codes for StockMovement.SourceDocumentType.
/// </summary>
public static class SourceDocumentTypes
{
    public const string StockAdjustment = "StockAdjustment";
    public const string Transfer = "Transfer";
    public const string Purchase = "Purchase";
    public const string Sale = "Sale";
}

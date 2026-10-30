namespace Warehouse.Domain.Inventory;

/// <summary>
/// Documents future StockBalance grain and transfer effects without implementing inventory tables.
/// StockBalance stores OnHand and Reserved at a Location (LocationId NOT NULL).
/// Available = OnHand - Reserved (calculated; never persisted).
/// In-transit stock is OnHand on the warehouse IN_TRANSIT location — not a separate balance concept.
/// </summary>
public static class TransferStockEffects
{
    public const string DispatchDescription =
        "Dispatch decreases source location OnHand and increases the warehouse IN_TRANSIT location OnHand.";

    public const string ReceiveDescription =
        "Receive decreases IN_TRANSIT location OnHand and increases destination location OnHand.";

    public const string PartialReceiveDescription =
        "Partial receiving is supported until in-transit OnHand for the transfer is fully drained.";

    public const string AtomicIdempotentDescription =
        "Transfer dispatch and receive operations must be atomic and idempotent.";

    public const string BalanceGrainDescription =
        "StockBalance is Tenant + Product + Warehouse + LocationId(NOT NULL) + OnHand + Reserved; Available = OnHand - Reserved.";

    public const string TraceabilityDescription =
        "StockMovement may reference TransferLineId (nullable) plus SourceDocumentType/SourceDocumentId.";

    /// <summary>
    /// Semantic label for stock on non-in-transit locations.
    /// </summary>
    public static StockBalanceKind DispatchSourceKind => StockBalanceKind.Available;

    /// <summary>
    /// Semantic label for stock on the IN_TRANSIT location.
    /// </summary>
    public static StockBalanceKind DispatchDestinationKind => StockBalanceKind.InTransit;

    public static StockBalanceKind ReceiveSourceKind => StockBalanceKind.InTransit;

    public static StockBalanceKind ReceiveDestinationKind => StockBalanceKind.Available;
}

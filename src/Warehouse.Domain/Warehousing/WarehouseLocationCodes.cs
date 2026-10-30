namespace Warehouse.Domain.Warehousing;

public static class WarehouseLocationCodes
{
    /// <summary>
    /// Default system location every warehouse must have.
    /// </summary>
    public const string General = "GENERAL";

    /// <summary>
    /// Dedicated in-transit bucket for stock dispatched and not yet received.
    /// Not available for sale, pick, or normal allocation.
    /// </summary>
    public const string InTransit = "IN_TRANSIT";
}

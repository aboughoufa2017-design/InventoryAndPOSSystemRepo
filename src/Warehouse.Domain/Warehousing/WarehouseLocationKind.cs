namespace Warehouse.Domain.Warehousing;

/// <summary>
/// Distinguishes normal physical bins from system and in-transit buckets.
/// In-transit stock must never be treated as available inventory.
/// </summary>
public enum WarehouseLocationKind
{
    Normal = 0,
    System = 1,
    InTransit = 2,
}

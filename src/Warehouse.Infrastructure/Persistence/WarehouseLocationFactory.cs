using Warehouse.Domain.Warehousing;

namespace Warehouse.Infrastructure.Persistence;

/// <summary>
/// Creates the mandatory GENERAL and IN_TRANSIT locations for a warehouse.
/// Used by tests and by future warehouse creation workflows.
/// </summary>
public static class WarehouseLocationFactory
{
    public static (WarehouseLocation General, WarehouseLocation InTransit) CreateRequiredLocations(
        Guid tenantId,
        Guid warehouseId,
        DateTimeOffset utcNow,
        Guid? generalLocationId = null,
        Guid? inTransitLocationId = null)
    {
        var general = WarehouseLocation.CreateGeneral(
            generalLocationId ?? Guid.NewGuid(),
            tenantId,
            warehouseId,
            utcNow);

        var inTransit = WarehouseLocation.CreateInTransit(
            inTransitLocationId ?? Guid.NewGuid(),
            tenantId,
            warehouseId,
            utcNow);

        return (general, inTransit);
    }
}

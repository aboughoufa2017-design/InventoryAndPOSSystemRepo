using Warehouse.Domain.Warehousing;

namespace Warehouse.UnitTests.Warehousing;

public class WarehouseLocationTests
{
    [Fact]
    public void Creates_general_system_default_location()
    {
        var location = WarehouseLocation.CreateGeneral(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        Assert.Equal(WarehouseLocationCodes.General, location.Code);
        Assert.Equal(WarehouseLocationKind.System, location.Kind);
        Assert.True(location.IsDefault);
        Assert.True(location.IsAvailableForSaleOrPick);
    }

    [Fact]
    public void Creates_in_transit_location_that_is_not_available()
    {
        var location = WarehouseLocation.CreateInTransit(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        Assert.Equal(WarehouseLocationCodes.InTransit, location.Code);
        Assert.Equal(WarehouseLocationKind.InTransit, location.Kind);
        Assert.False(location.IsDefault);
        Assert.False(location.IsAvailableForSaleOrPick);
    }

    [Fact]
    public void Rejects_in_transit_as_default_location()
    {
        Assert.Throws<ArgumentException>(() =>
            WarehouseLocation.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                WarehouseLocationCodes.InTransit,
                "In Transit",
                WarehouseLocationKind.InTransit,
                isDefault: true,
                DateTimeOffset.UtcNow));
    }
}

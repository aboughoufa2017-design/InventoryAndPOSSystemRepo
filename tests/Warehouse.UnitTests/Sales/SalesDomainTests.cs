using Warehouse.Domain.Commercial;
using Warehouse.Domain.Sales;

namespace Warehouse.UnitTests.Sales;

public class SalesDomainTests
{
    [Fact]
    public void Confirm_does_not_ship_and_ship_progresses_lifecycle()
    {
        var order = SalesOrder.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "SO-1", null, null, DateTimeOffset.UtcNow);
        order.SetTotals(100m, 15m, DateTimeOffset.UtcNow);
        Assert.Equal(SalesOrderStatus.Draft, order.Status);
        Assert.Equal(PaymentStatus.Unpaid, order.PaymentStatus);

        order.Confirm(DateTimeOffset.UtcNow);
        Assert.Equal(SalesOrderStatus.Confirmed, order.Status);
        Assert.Equal(SalesStockEffects.ConfirmDescription, SalesStockEffects.ConfirmDescription);

        order.ApplyShipProgress(anyRemainingToShip: true, DateTimeOffset.UtcNow);
        Assert.Equal(SalesOrderStatus.PartiallyFulfilled, order.Status);

        order.ApplyShipProgress(anyRemainingToShip: false, DateTimeOffset.UtcNow);
        Assert.Equal(SalesOrderStatus.Fulfilled, order.Status);

        order.Close(DateTimeOffset.UtcNow);
        Assert.Equal(SalesOrderStatus.Closed, order.Status);
    }

    [Fact]
    public void Line_tracks_ship_and_return_limits()
    {
        var line = SalesOrderLine.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            10m, 5m, 0.15m, null);
        Assert.Equal(10m, line.QuantityRemainingToShip);
        line.Ship(4m);
        Assert.Equal(6m, line.QuantityRemainingToShip);
        Assert.Equal(4m, line.QuantityReturnable);
        Assert.Throws<InvalidOperationException>(() => line.Ship(7m));
        line.Return(2m);
        Assert.Equal(2m, line.QuantityReturnable);
        Assert.Throws<InvalidOperationException>(() => line.Return(3m));
    }

    [Fact]
    public void Payment_status_derives_from_paid_amount()
    {
        var order = SalesOrder.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            null, null, null, DateTimeOffset.UtcNow);
        order.SetTotals(100m, 0m, DateTimeOffset.UtcNow);
        order.Confirm(DateTimeOffset.UtcNow);
        order.ApplyPayment(40m, DateTimeOffset.UtcNow);
        Assert.Equal(PaymentStatus.PartiallyPaid, order.PaymentStatus);
        order.ApplyPayment(100m, DateTimeOffset.UtcNow);
        Assert.Equal(PaymentStatus.Paid, order.PaymentStatus);
    }

    [Fact]
    public void Row_version_mismatch_throws_sync_conflict_marker()
    {
        var order = SalesOrder.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            null, null, null, DateTimeOffset.UtcNow);
        var ex = Assert.Throws<InvalidOperationException>(() => order.EnsureRowVersion(99));
        Assert.Equal("SYNC_CONFLICT", ex.Message);
    }
}

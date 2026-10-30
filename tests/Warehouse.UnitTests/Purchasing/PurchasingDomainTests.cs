using Warehouse.Domain.Commercial;
using Warehouse.Domain.Purchasing;

namespace Warehouse.UnitTests.Purchasing;

public class PurchasingDomainTests
{
    [Fact]
    public void Confirm_does_not_change_payment_and_receive_progresses_lifecycle()
    {
        var order = PurchaseOrder.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "PO-1", null, null, DateTimeOffset.UtcNow);
        order.SetTotals(100m, 15m, DateTimeOffset.UtcNow);
        Assert.Equal(PurchaseOrderStatus.Draft, order.Status);
        Assert.Equal(PaymentStatus.Unpaid, order.PaymentStatus);

        order.Confirm(DateTimeOffset.UtcNow);
        Assert.Equal(PurchaseOrderStatus.Confirmed, order.Status);
        Assert.Equal(PurchasingStockEffects.ConfirmDescription, PurchasingStockEffects.ConfirmDescription);

        order.ApplyReceiveProgress(anyRemainingToReceive: true, DateTimeOffset.UtcNow);
        Assert.Equal(PurchaseOrderStatus.PartiallyReceived, order.Status);

        order.ApplyReceiveProgress(anyRemainingToReceive: false, DateTimeOffset.UtcNow);
        Assert.Equal(PurchaseOrderStatus.FullyReceived, order.Status);

        order.Close(DateTimeOffset.UtcNow);
        Assert.Equal(PurchaseOrderStatus.Closed, order.Status);
    }

    [Fact]
    public void Line_tracks_receive_and_return_limits()
    {
        var line = PurchaseOrderLine.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            10m, 5m, 0.15m, null);
        Assert.Equal(10m, line.QuantityRemainingToReceive);
        line.Receive(4m);
        Assert.Equal(6m, line.QuantityRemainingToReceive);
        Assert.Equal(4m, line.QuantityReturnable);
        Assert.Throws<InvalidOperationException>(() => line.Receive(7m));
        line.Return(2m);
        Assert.Equal(2m, line.QuantityReturnable);
        Assert.Throws<InvalidOperationException>(() => line.Return(3m));
    }

    [Fact]
    public void Payment_status_derives_from_paid_amount()
    {
        var order = PurchaseOrder.Create(
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
        var order = PurchaseOrder.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            null, null, null, DateTimeOffset.UtcNow);
        var ex = Assert.Throws<InvalidOperationException>(() => order.EnsureRowVersion(99));
        Assert.Equal("SYNC_CONFLICT", ex.Message);
    }
}

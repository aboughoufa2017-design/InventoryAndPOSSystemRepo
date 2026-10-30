using Warehouse.Domain.Inventory;

namespace Warehouse.UnitTests.Inventory;

public class TransferDomainTests
{
    [Fact]
    public void Dispatch_and_partial_receive_progress_to_completed()
    {
        var transfer = Transfer.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), null, null, DateTimeOffset.UtcNow);

        Assert.Equal(TransferStatus.Draft, transfer.Status);
        transfer.MarkDispatched(Guid.NewGuid(), DateTimeOffset.UtcNow);
        Assert.Equal(TransferStatus.Dispatched, transfer.Status);

        transfer.ApplyReceiveProgress(anyRemainingInTransit: true, DateTimeOffset.UtcNow);
        Assert.Equal(TransferStatus.PartiallyReceived, transfer.Status);

        transfer.ApplyReceiveProgress(anyRemainingInTransit: false, DateTimeOffset.UtcNow);
        Assert.Equal(TransferStatus.Completed, transfer.Status);
    }

    [Fact]
    public void Transfer_line_tracks_in_transit_and_rejects_over_receive()
    {
        var line = TransferLine.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10m, null);
        Assert.Equal(10m, line.QuantityInTransit);
        line.Receive(4m);
        Assert.Equal(6m, line.QuantityInTransit);
        Assert.Throws<InvalidOperationException>(() => line.Receive(7m));
    }

    [Fact]
    public void Row_version_mismatch_throws_sync_conflict_marker()
    {
        var transfer = Transfer.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), null, null, DateTimeOffset.UtcNow);
        var ex = Assert.Throws<InvalidOperationException>(() => transfer.EnsureRowVersion(99));
        Assert.Equal("SYNC_CONFLICT", ex.Message);
    }
}

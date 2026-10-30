using Warehouse.Domain.Inventory;

namespace Warehouse.UnitTests.Inventory;

public sealed class StockCountDomainTests
{
    [Fact]
    public void Lifecycle_draft_counted_posted_with_row_version()
    {
        var utc = DateTimeOffset.UtcNow;
        var count = StockCount.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "cycle", utc);
        Assert.Equal(StockCountStatus.Draft, count.Status);
        Assert.Equal(1, count.RowVersion);

        count.MarkCounted(utc.AddMinutes(1));
        Assert.Equal(StockCountStatus.Counted, count.Status);
        Assert.Equal(2, count.RowVersion);

        var adjustmentId = Guid.NewGuid();
        count.MarkPosted(adjustmentId, utc.AddMinutes(2));
        Assert.Equal(StockCountStatus.Posted, count.Status);
        Assert.Equal(adjustmentId, count.PostedAdjustmentId);
        Assert.Equal(3, count.RowVersion);
    }

    [Fact]
    public void Cancel_from_draft_succeeds_and_posted_cannot_cancel()
    {
        var utc = DateTimeOffset.UtcNow;
        var count = StockCount.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, utc);
        count.Cancel(utc.AddMinutes(1));
        Assert.Equal(StockCountStatus.Cancelled, count.Status);

        var posted = StockCount.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, utc);
        posted.MarkCounted(utc);
        posted.MarkPosted(null, utc);
        Assert.Throws<InvalidOperationException>(() => posted.Cancel(utc));
    }

    [Fact]
    public void Line_variance_is_counted_minus_system()
    {
        var line = StockCountLine.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            systemQuantity: 10m,
            notes: null);
        Assert.Equal(0m, line.Variance);

        line.SetCountedQuantity(7m);
        Assert.Equal(-3m, line.Variance);
    }
}

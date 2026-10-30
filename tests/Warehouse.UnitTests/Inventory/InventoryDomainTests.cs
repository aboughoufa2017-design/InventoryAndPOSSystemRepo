using Warehouse.Domain.Common;
using Warehouse.Domain.Inventory;
using Warehouse.Domain.Tenancy;

namespace Warehouse.UnitTests.Inventory;

public class NegativeStockEvaluatorTests
{
    [Theory]
    [InlineData(NegativeStockPolicy.Prevent, -1, true, false)]
    [InlineData(NegativeStockPolicy.Prevent, 0, true, true)]
    [InlineData(NegativeStockPolicy.Allow, -5, false, true)]
    [InlineData(NegativeStockPolicy.AllowWithPermission, -1, false, false)]
    [InlineData(NegativeStockPolicy.AllowWithPermission, -1, true, true)]
    public void Evaluates_policy(NegativeStockPolicy policy, decimal onHand, bool hasPermission, bool expected)
    {
        Assert.Equal(expected, NegativeStockEvaluator.IsAllowed(policy, onHand, hasPermission));
    }
}

public class StockBalanceTests
{
    [Fact]
    public void Available_is_on_hand_minus_reserved_and_not_persisted_conceptually()
    {
        var balance = StockBalance.CreateEmpty(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
        balance.ApplyOnHandDelta(10m, DateTimeOffset.UtcNow);
        balance.ApplyReservedDelta(3m, DateTimeOffset.UtcNow);
        Assert.Equal(10m, balance.OnHand);
        Assert.Equal(3m, balance.Reserved);
        Assert.Equal(7m, balance.Available);
    }
}

public class StockAdjustmentTests
{
    [Fact]
    public void Draft_posts_and_cannot_post_twice()
    {
        var adj = StockAdjustment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            StockAdjustmentKind.OpeningBalance, "open", null, DateTimeOffset.UtcNow);
        Assert.Equal(DocumentLifecycleStatus.Draft, adj.Status);
        adj.MarkPosted(DateTimeOffset.UtcNow);
        Assert.Equal(DocumentLifecycleStatus.Posted, adj.Status);
        Assert.Throws<InvalidOperationException>(() => adj.MarkPosted(DateTimeOffset.UtcNow));
    }
}

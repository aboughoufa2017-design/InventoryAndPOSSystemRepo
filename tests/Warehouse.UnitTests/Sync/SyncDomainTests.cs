using Warehouse.Domain.Sync;

namespace Warehouse.UnitTests.Sync;

public class SyncDomainTests
{
    [Fact]
    public void Tenant_change_counter_allocates_monotonic_versions()
    {
        var counter = TenantChangeCounter.Create(Guid.NewGuid());
        Assert.Equal(1, counter.AllocateNext());
        Assert.Equal(2, counter.AllocateNext());
        Assert.Equal(2, counter.LastChangeVersion);
    }

    [Fact]
    public void Change_log_entry_rejects_non_positive_version()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ChangeLogEntry.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            0,
            SyncEntityTypes.Product,
            Guid.NewGuid(),
            ChangeLogOperation.Upsert,
            "{}",
            DateTimeOffset.UtcNow,
            null,
            null));
    }

    [Fact]
    public void Push_outcomes_are_explicit()
    {
        Assert.Equal("Applied", SyncPushOutcomes.Applied);
        Assert.Equal("AlreadyProcessed", SyncPushOutcomes.AlreadyProcessed);
        Assert.Equal("Conflict", SyncPushOutcomes.Conflict);
        Assert.Equal("Rejected", SyncPushOutcomes.Rejected);
        Assert.Equal("ValidationFailed", SyncPushOutcomes.ValidationFailed);
    }

    [Fact]
    public void Deactivate_is_tombstone_operation()
    {
        var entry = ChangeLogEntry.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            1,
            SyncEntityTypes.Product,
            Guid.NewGuid(),
            ChangeLogOperation.Deactivate,
            """{"isActive":false}""",
            DateTimeOffset.UtcNow,
            null,
            null);
        Assert.Equal(ChangeLogOperation.Deactivate, entry.Operation);
    }
}

using Warehouse.Domain.Common;

namespace Warehouse.UnitTests.Common;

public class TenantOwnedEntityTests
{
    [Fact]
    public void Retains_client_generated_ids()
    {
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var entity = new SampleTenantEntity(id, tenantId);

        Assert.Equal(id, entity.Id);
        Assert.Equal(tenantId, entity.TenantId);
    }

    [Fact]
    public void Rejects_empty_entity_id()
    {
        var exception = Assert.Throws<ArgumentException>(() => new SampleTenantEntity(Guid.Empty, Guid.NewGuid()));

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void Rejects_empty_tenant_id()
    {
        var exception = Assert.Throws<ArgumentException>(() => new SampleTenantEntity(Guid.NewGuid(), Guid.Empty));

        Assert.Equal("tenantId", exception.ParamName);
    }

    private sealed class SampleTenantEntity : TenantOwnedEntity
    {
        public SampleTenantEntity(Guid id, Guid tenantId)
            : base(id, tenantId)
        {
        }
    }
}

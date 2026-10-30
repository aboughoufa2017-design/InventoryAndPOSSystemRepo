namespace Warehouse.Domain.Common;

/// <summary>
/// Marker for entities that belong to exactly one tenant and must be filtered by TenantId.
/// </summary>
public interface ITenantOwned
{
    Guid TenantId { get; }
}

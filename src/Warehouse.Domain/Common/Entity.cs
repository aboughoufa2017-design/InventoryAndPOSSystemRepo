namespace Warehouse.Domain.Common;

/// <summary>
/// Identity base for aggregates and entities that may be created offline.
/// Identifiers are client-generated UUIDs and stay unchanged after synchronization.
/// </summary>
public abstract class Entity
{
    /// <summary>
    /// Required by EF Core materialization. Application code must use the Guid constructor.
    /// </summary>
    protected Entity()
    {
    }

    protected Entity(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Entity id must be a non-empty UUID.", nameof(id));
        }

        Id = id;
    }

    public Guid Id { get; protected set; }
}

namespace Warehouse.Domain.Sync;

/// <summary>
/// Change-feed operation kinds for pull. Tombstones use <see cref="Deactivate"/>.
/// </summary>
public enum ChangeLogOperation
{
    Upsert = 0,
    Deactivate = 1,
}

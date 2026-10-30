namespace Warehouse.Domain.Common;

/// <summary>
/// Document lifecycle for future commercial and inventory documents.
/// Inventory must not change while a document is Draft.
/// </summary>
public enum DocumentLifecycleStatus
{
    Draft = 0,
    Approved = 1,
    Posted = 2,
    Cancelled = 3,
}

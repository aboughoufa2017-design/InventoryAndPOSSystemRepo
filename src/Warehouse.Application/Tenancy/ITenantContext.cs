namespace Warehouse.Application.Tenancy;

/// <summary>
/// Tenant scope for the current operation. The value comes from the authenticated identity.
/// A tenant id supplied in a request body, query string, or header is never a source for this value.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Tenant of the caller, or null when the caller is not acting inside a tenant.
    /// </summary>
    Guid? TenantId { get; }

    /// <summary>
    /// True for a SaaS platform operator. This flag does not grant ambient access to every tenant.
    /// Cross-tenant work uses platform endpoints and an explicit tenant id that is authorized and audited.
    /// </summary>
    bool IsPlatformOperator { get; }
}

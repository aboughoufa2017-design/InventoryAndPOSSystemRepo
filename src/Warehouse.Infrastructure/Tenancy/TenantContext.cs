using Warehouse.Application.Tenancy;

namespace Warehouse.Infrastructure.Tenancy;

/// <summary>
/// Default tenant context before authentication is implemented.
/// Yields no tenant, so global query filters hide all tenant-owned rows.
/// </summary>
public sealed class NullTenantContext : ITenantContext
{
    public static NullTenantContext Instance { get; } = new();

    public Guid? TenantId => null;

    public bool IsPlatformOperator => false;
}

/// <summary>
/// Explicit tenant scope for tests and platform units of work.
/// </summary>
public sealed class FixedTenantContext(Guid? tenantId, bool isPlatformOperator = false) : ITenantContext
{
    public Guid? TenantId { get; } = tenantId;

    public bool IsPlatformOperator { get; } = isPlatformOperator;
}

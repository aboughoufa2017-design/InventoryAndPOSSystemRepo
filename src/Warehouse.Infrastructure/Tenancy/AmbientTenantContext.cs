using Warehouse.Application.Security;
using Warehouse.Application.Tenancy;

namespace Warehouse.Infrastructure.Tenancy;

/// <summary>
/// Mutable per-request tenant and user context. Populated from JWT claims by middleware.
/// </summary>
public sealed class AmbientTenantContext : ITenantContext, ICurrentUser
{
    private readonly HashSet<string> _permissions = new(StringComparer.OrdinalIgnoreCase);

    public Guid? TenantId { get; private set; }

    public bool IsPlatformOperator { get; private set; }

    public bool IsAuthenticated { get; private set; }

    public Guid? UserId { get; private set; }

    public Guid? DeviceId { get; private set; }

    public string? Email { get; private set; }

    public IReadOnlyCollection<string> Permissions => _permissions;

    public void SetUnauthenticated()
    {
        TenantId = null;
        IsPlatformOperator = false;
        IsAuthenticated = false;
        UserId = null;
        DeviceId = null;
        Email = null;
        _permissions.Clear();
    }

    public void SetAuthenticated(
        Guid tenantId,
        Guid userId,
        string email,
        Guid? deviceId,
        bool isPlatformOperator,
        IEnumerable<string> permissions)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant id must be a non-empty UUID.", nameof(tenantId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id must be a non-empty UUID.", nameof(userId));
        }

        TenantId = tenantId;
        UserId = userId;
        Email = email;
        DeviceId = deviceId;
        IsPlatformOperator = isPlatformOperator;
        IsAuthenticated = true;
        ReplacePermissions(permissions);
    }

    /// <summary>
    /// Platform operator principal with no ambient tenant (filters stay closed until SetTenantScope).
    /// </summary>
    public void SetPlatformAuthenticated(Guid operatorId, string email, IEnumerable<string>? permissions = null)
    {
        if (operatorId == Guid.Empty)
        {
            throw new ArgumentException("Operator id must be a non-empty UUID.", nameof(operatorId));
        }

        TenantId = null;
        UserId = operatorId;
        Email = email;
        DeviceId = null;
        IsPlatformOperator = true;
        IsAuthenticated = true;
        ReplacePermissions(permissions ?? Array.Empty<string>());
    }

    /// <summary>
    /// Sets tenant scope without a full user principal (login bootstrap, platform UoW).
    /// Preserves existing UserId / IsPlatformOperator / authentication flags when already set.
    /// </summary>
    public void SetTenantScope(Guid tenantId, bool isPlatformOperator = false)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant id must be a non-empty UUID.", nameof(tenantId));
        }

        TenantId = tenantId;
        IsPlatformOperator = isPlatformOperator || IsPlatformOperator;
    }

    private void ReplacePermissions(IEnumerable<string> permissions)
    {
        _permissions.Clear();
        foreach (var permission in permissions)
        {
            if (!string.IsNullOrWhiteSpace(permission))
            {
                _permissions.Add(permission.Trim());
            }
        }
    }

    public bool HasPermission(string permissionCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permissionCode);
        return _permissions.Contains(permissionCode.Trim());
    }
}

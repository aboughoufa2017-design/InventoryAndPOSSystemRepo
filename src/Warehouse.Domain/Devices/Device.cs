using Warehouse.Domain.Common;

namespace Warehouse.Domain.Devices;

/// <summary>
/// TENANT-OWNED registered client device for offline sync and security policy evaluation.
/// </summary>
public sealed class Device : TenantOwnedEntity
{
    private Device()
    {
    }

    private Device(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    /// <summary>
    /// Stable client-generated device key. Unique per tenant.
    /// </summary>
    public string DeviceKey { get; private set; } = string.Empty;

    public Guid? UserId { get; private set; }

    /// <summary>Optional POS branch binding (maps to warehouse id).</summary>
    public Guid? WarehouseId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public DeviceStatus Status { get; private set; } = DeviceStatus.Active;

    public string? ApplicationVersion { get; private set; }

    public DateTimeOffset? LastSuccessfulSyncAtUtc { get; private set; }

    public DateTimeOffset? LastSuccessfulOnlineAuthenticationAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public int RowVersion { get; private set; } = 1;

    public bool IsRevoked => Status == DeviceStatus.Revoked;

    public static Device Register(
        Guid id,
        Guid tenantId,
        string deviceKey,
        string name,
        Guid? userId,
        string? applicationVersion,
        DateTimeOffset utcNow,
        Guid? warehouseId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Device(id, tenantId)
        {
            DeviceKey = deviceKey.Trim(),
            Name = name.Trim(),
            UserId = userId,
            WarehouseId = warehouseId == Guid.Empty ? null : warehouseId,
            ApplicationVersion = string.IsNullOrWhiteSpace(applicationVersion) ? null : applicationVersion.Trim(),
            LastSuccessfulOnlineAuthenticationAtUtc = utcNow,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
        };
    }

    public void BindWarehouse(Guid? warehouseId, DateTimeOffset utcNow)
    {
        WarehouseId = warehouseId == Guid.Empty ? null : warehouseId;
        UpdatedAtUtc = utcNow;
        RowVersion++;
    }

    public void Revoke(DateTimeOffset utcNow)
    {
        Status = DeviceStatus.Revoked;
        UpdatedAtUtc = utcNow;
        RowVersion++;
    }

    public void RecordOnlineAuthentication(DateTimeOffset utcNow, string? applicationVersion = null)
    {
        LastSuccessfulOnlineAuthenticationAtUtc = utcNow;
        if (!string.IsNullOrWhiteSpace(applicationVersion))
        {
            ApplicationVersion = applicationVersion.Trim();
        }

        UpdatedAtUtc = utcNow;
        RowVersion++;
    }

    public void RecordSuccessfulSync(DateTimeOffset utcNow, string? applicationVersion)
    {
        LastSuccessfulSyncAtUtc = utcNow;
        if (!string.IsNullOrWhiteSpace(applicationVersion))
        {
            ApplicationVersion = applicationVersion.Trim();
        }

        UpdatedAtUtc = utcNow;
        RowVersion++;
    }
}

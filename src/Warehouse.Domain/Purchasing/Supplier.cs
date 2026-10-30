using Warehouse.Domain.Common;

namespace Warehouse.Domain.Purchasing;

/// <summary>
/// TENANT-OWNED supplier master data.
/// </summary>
public sealed class Supplier : TenantOwnedEntity
{
    private Supplier()
    {
    }

    private Supplier(Guid id, Guid tenantId)
        : base(id, tenantId)
    {
    }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? ContactName { get; private set; }

    public string? Email { get; private set; }

    public string? Phone { get; private set; }

    public string? Notes { get; private set; }

    public bool IsActive { get; private set; } = true;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public int RowVersion { get; private set; } = 1;

    public static Supplier Create(
        Guid id,
        Guid tenantId,
        string code,
        string name,
        string? contactName,
        string? email,
        string? phone,
        string? notes,
        DateTimeOffset utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Supplier(id, tenantId)
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            ContactName = Normalize(contactName),
            Email = Normalize(email),
            Phone = Normalize(phone),
            Notes = Normalize(notes),
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
        };
    }

    public void Update(
        string name,
        string? contactName,
        string? email,
        string? phone,
        string? notes,
        bool isActive,
        DateTimeOffset utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        ContactName = Normalize(contactName);
        Email = Normalize(email);
        Phone = Normalize(phone);
        Notes = Normalize(notes);
        IsActive = isActive;
        Touch(utcNow);
    }

    public void EnsureRowVersion(int expectedRowVersion)
    {
        if (RowVersion != expectedRowVersion)
        {
            throw new InvalidOperationException("SYNC_CONFLICT");
        }
    }

    private void Touch(DateTimeOffset utcNow)
    {
        UpdatedAtUtc = utcNow;
        RowVersion++;
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

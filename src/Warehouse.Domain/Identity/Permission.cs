using Warehouse.Domain.Common;

namespace Warehouse.Domain.Identity;

/// <summary>
/// GLOBAL permission catalog. Codes are unique across the platform.
/// </summary>
public sealed class Permission : Entity
{
    private Permission()
    {
    }

    private Permission(Guid id)
        : base(id)
    {
    }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public static Permission Create(Guid id, string code, string name, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Permission(id)
        {
            Code = code.Trim().ToLowerInvariant(),
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
        };
    }
}

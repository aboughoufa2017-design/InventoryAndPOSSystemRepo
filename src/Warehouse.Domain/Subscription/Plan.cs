using Warehouse.Domain.Common;

namespace Warehouse.Domain.Subscription;

/// <summary>
/// GLOBAL commercial plan. Feature codes and soft limits; no payment-provider coupling.
/// </summary>
public sealed class Plan : Entity
{
    private Plan()
    {
    }

    private Plan(Guid id)
        : base(id)
    {
    }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public bool IsActive { get; private set; } = true;

    /// <summary>Null means unlimited.</summary>
    public int? MaxUsers { get; private set; }

    /// <summary>Null means unlimited.</summary>
    public int? MaxWarehouses { get; private set; }

    /// <summary>Null means unlimited.</summary>
    public int? MaxDevices { get; private set; }

    /// <summary>Null means unlimited.</summary>
    public int? MaxProducts { get; private set; }

    /// <summary>Lowercase feature codes enabled on this plan.</summary>
    public List<string> FeatureCodes { get; private set; } = [];

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static Plan Create(
        Guid id,
        string code,
        string name,
        IEnumerable<string> featureCodes,
        int? maxUsers,
        int? maxWarehouses,
        int? maxDevices,
        int? maxProducts,
        DateTimeOffset utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var plan = new Plan(id)
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            MaxUsers = maxUsers,
            MaxWarehouses = maxWarehouses,
            MaxDevices = maxDevices,
            MaxProducts = maxProducts,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
        };

        plan.ReplaceFeatures(featureCodes);
        return plan;
    }

    public void ReplaceFeatures(IEnumerable<string> featureCodes)
    {
        FeatureCodes = featureCodes
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim().ToLowerInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public bool HasFeature(string featureCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(featureCode);
        return FeatureCodes.Contains(featureCode.Trim(), StringComparer.OrdinalIgnoreCase);
    }

    public void Deactivate(DateTimeOffset utcNow)
    {
        IsActive = false;
        Touch(utcNow);
    }

    public void Activate(DateTimeOffset utcNow)
    {
        IsActive = true;
        Touch(utcNow);
    }

    private void Touch(DateTimeOffset utcNow) => UpdatedAtUtc = utcNow;
}

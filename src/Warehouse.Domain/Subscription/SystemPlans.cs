namespace Warehouse.Domain.Subscription;

/// <summary>
/// Seeded commercial plans (stable ids for migrations and provisioning defaults).
/// </summary>
public static class SystemPlans
{
    public static readonly Guid TrialPlanId = Guid.Parse("22222222-2222-2222-2222-222222222201");
    public static readonly Guid StandardPlanId = Guid.Parse("22222222-2222-2222-2222-222222222202");
    public static readonly Guid EnterprisePlanId = Guid.Parse("22222222-2222-2222-2222-222222222203");

    public const string TrialCode = "TRIAL";
    public const string StandardCode = "STANDARD";
    public const string EnterpriseCode = "ENTERPRISE";

    public static readonly TimeSpan DefaultTrialLength = TimeSpan.FromDays(30);

    public static IReadOnlyList<PlanSeed> Catalog { get; } =
    [
        new(
            TrialPlanId,
            TrialCode,
            "Trial",
            SubscriptionFeatures.All,
            MaxUsers: 5,
            MaxWarehouses: 2,
            MaxDevices: 5,
            MaxProducts: 100),
        new(
            StandardPlanId,
            StandardCode,
            "Standard",
            SubscriptionFeatures.All,
            MaxUsers: 50,
            MaxWarehouses: 20,
            MaxDevices: 50,
            MaxProducts: 10_000),
        new(
            EnterprisePlanId,
            EnterpriseCode,
            "Enterprise",
            SubscriptionFeatures.All,
            MaxUsers: null,
            MaxWarehouses: null,
            MaxDevices: null,
            MaxProducts: null),
    ];

    public sealed record PlanSeed(
        Guid Id,
        string Code,
        string Name,
        IReadOnlyList<string> FeatureCodes,
        int? MaxUsers,
        int? MaxWarehouses,
        int? MaxDevices,
        int? MaxProducts);
}

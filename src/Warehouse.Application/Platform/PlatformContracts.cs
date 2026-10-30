using Warehouse.Application.Abstractions;
using Warehouse.Domain.Subscription;

namespace Warehouse.Application.Platform;

public sealed record PlatformLoginCommand(string Email, string Password);

public sealed record PlatformAuthResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    Guid OperatorId,
    string Email,
    string DisplayName);

public sealed record ProvisionTenantPlatformCommand(
    string TenantCode,
    string TenantName,
    string CurrencyCode,
    string TimeZoneId,
    string OwnerEmail,
    string OwnerPassword,
    string OwnerDisplayName,
    string? PlanCode);

public sealed record PlatformTenantResponse(
    Guid Id,
    string Code,
    string Name,
    bool IsActive,
    bool IsSuspended,
    string CurrencyCode,
    string TimeZoneId,
    bool TaxEnabled,
    string? TaxName,
    decimal? TaxRate,
    int? MaxOfflineDays,
    bool RequireOnlineRevalidation,
    NegativeStockPolicyDto NegativeStockPolicy,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    int RowVersion);

public enum NegativeStockPolicyDto
{
    Prevent = 0,
    Allow = 1,
    AllowWithPermission = 2,
}

public sealed record UpdateTenantSettingsCommand(
    Guid TenantId,
    string Name,
    string CurrencyCode,
    string TimeZoneId,
    NegativeStockPolicyDto NegativeStockPolicy,
    bool TaxEnabled,
    string? TaxName,
    decimal? TaxRate,
    int? MaxOfflineDays,
    bool RequireOnlineRevalidation,
    int ExpectedRowVersion);

public sealed record PlanResponse(
    Guid Id,
    string Code,
    string Name,
    bool IsActive,
    int? MaxUsers,
    int? MaxWarehouses,
    int? MaxDevices,
    int? MaxProducts,
    IReadOnlyList<string> FeatureCodes);

public sealed record SubscriptionResponse(
    Guid Id,
    Guid TenantId,
    Guid PlanId,
    string PlanCode,
    string PlanName,
    SubscriptionStatus Status,
    DateTimeOffset? TrialEndsAtUtc,
    DateTimeOffset? CurrentPeriodStartUtc,
    DateTimeOffset? CurrentPeriodEndUtc,
    string? ExternalReference,
    int RowVersion);

public sealed record UpdateSubscriptionCommand(
    Guid TenantId,
    string PlanCode,
    SubscriptionStatus Status,
    DateTimeOffset? TrialEndsAtUtc,
    DateTimeOffset? CurrentPeriodStartUtc,
    DateTimeOffset? CurrentPeriodEndUtc,
    string? ExternalReference,
    int ExpectedRowVersion);

public sealed record TenantUsageResponse(
    Guid TenantId,
    int UserCount,
    int WarehouseCount,
    int DeviceCount,
    int ActiveDeviceCount,
    int ProductCount,
    int? PlanMaxUsers,
    int? PlanMaxWarehouses,
    int? PlanMaxDevices,
    int? PlanMaxProducts);

public sealed record TenantReadinessResponse(
    Guid TenantId,
    bool HasOwnerUser,
    bool HasWarehouse,
    bool HasProduct,
    bool HasActiveSubscription,
    bool IsSuspended,
    IReadOnlyList<string> MissingSteps);

public interface IPlatformAuthenticationService
{
    Task<AppResult<PlatformAuthResponse>> LoginAsync(PlatformLoginCommand command, CancellationToken cancellationToken);
}

public interface IPlatformTenantService
{
    Task<AppResult<PlatformTenantResponse>> ProvisionAsync(
        ProvisionTenantPlatformCommand command,
        CancellationToken cancellationToken);

    Task<AppResult<IReadOnlyList<PlatformTenantResponse>>> ListTenantsAsync(CancellationToken cancellationToken);

    Task<AppResult<PlatformTenantResponse>> GetTenantAsync(Guid tenantId, CancellationToken cancellationToken);

    Task<AppResult<PlatformTenantResponse>> SuspendAsync(Guid tenantId, CancellationToken cancellationToken);

    Task<AppResult<PlatformTenantResponse>> ResumeAsync(Guid tenantId, CancellationToken cancellationToken);

    Task<AppResult<PlatformTenantResponse>> UpdateSettingsAsync(
        UpdateTenantSettingsCommand command,
        CancellationToken cancellationToken);

    Task<AppResult<SubscriptionResponse>> GetSubscriptionAsync(Guid tenantId, CancellationToken cancellationToken);

    Task<AppResult<SubscriptionResponse>> UpdateSubscriptionAsync(
        UpdateSubscriptionCommand command,
        CancellationToken cancellationToken);

    Task<AppResult<TenantUsageResponse>> GetUsageAsync(Guid tenantId, CancellationToken cancellationToken);

    Task<AppResult<TenantReadinessResponse>> GetReadinessAsync(Guid tenantId, CancellationToken cancellationToken);

    Task<AppResult<IReadOnlyList<PlanResponse>>> ListPlansAsync(CancellationToken cancellationToken);
}

public interface IEntitlementService
{
    Task<bool> IsFeatureEnabledAsync(string featureCode, CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> GetEnabledFeaturesAsync(CancellationToken cancellationToken);
}

public interface IAuditWriter
{
    Task WriteAsync(
        Guid? tenantId,
        string action,
        string entityType,
        Guid? entityId,
        string? metadataJson,
        CancellationToken cancellationToken);
}

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Identity;
using Warehouse.Application.Platform;
using Warehouse.Application.Security;
using Warehouse.Domain.Devices;
using Warehouse.Domain.Subscription;
using Warehouse.Domain.Tenancy;
using Warehouse.Infrastructure.Persistence;
using Warehouse.Infrastructure.Tenancy;

namespace Warehouse.Infrastructure.Platform;

public sealed class PlatformTenantService(
    WarehouseDbContext db,
    ITenantProvisioningService provisioning,
    AmbientTenantContext ambient,
    ICurrentUser currentUser,
    IAuditWriter auditWriter) : IPlatformTenantService
{
    public async Task<AppResult<PlatformTenantResponse>> ProvisionAsync(
        ProvisionTenantPlatformCommand command,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || !ambient.IsPlatformOperator)
        {
            return AppResult<PlatformTenantResponse>.Failure(ErrorCodes.Forbidden, "Platform operator required.");
        }

        var planCode = string.IsNullOrWhiteSpace(command.PlanCode)
            ? SystemPlans.TrialCode
            : command.PlanCode.Trim().ToUpperInvariant();

        var plan = await db.Plans.AsNoTracking()
            .SingleOrDefaultAsync(p => p.Code == planCode && p.IsActive, cancellationToken);
        if (plan is null)
        {
            return AppResult<PlatformTenantResponse>.Failure(ErrorCodes.ValidationFailed, $"Unknown plan '{planCode}'.");
        }

        var tenantId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var provision = await provisioning.ProvisionAsync(
            new ProvisionTenantCommand(
                tenantId,
                command.TenantCode,
                command.TenantName,
                command.CurrencyCode,
                command.TimeZoneId,
                ownerId,
                command.OwnerEmail,
                command.OwnerPassword,
                command.OwnerDisplayName),
            cancellationToken);

        if (!provision.IsSuccess)
        {
            return AppResult<PlatformTenantResponse>.Failure(provision.ErrorCode!, provision.ErrorMessage!);
        }

        var utcNow = DateTimeOffset.UtcNow;
        TenantSubscription subscription = plan.Code == SystemPlans.TrialCode
            ? TenantSubscription.StartTrial(Guid.NewGuid(), tenantId, plan.Id, utcNow, SystemPlans.DefaultTrialLength)
            : TenantSubscription.Create(
                Guid.NewGuid(),
                tenantId,
                plan.Id,
                SubscriptionStatus.Active,
                trialEndsAtUtc: null,
                periodStartUtc: utcNow,
                periodEndUtc: utcNow.AddDays(30),
                externalReference: null,
                utcNow);

        // Subscription is tenant-owned; set scope for insert filters are not applied on insert.
        ambient.SetTenantScope(tenantId, isPlatformOperator: true);
        db.TenantSubscriptions.Add(subscription);
        await db.SaveChangesAsync(cancellationToken);

        await auditWriter.WriteAsync(
            tenantId,
            "platform.tenant.provisioned",
            nameof(Tenant),
            tenantId,
            JsonSerializer.Serialize(new { plan.Code, OwnerEmail = command.OwnerEmail }),
            cancellationToken);

        var tenant = await db.Tenants.AsNoTracking().SingleAsync(t => t.Id == tenantId, cancellationToken);
        return AppResult<PlatformTenantResponse>.Success(MapTenant(tenant));
    }

    public async Task<AppResult<IReadOnlyList<PlatformTenantResponse>>> ListTenantsAsync(
        CancellationToken cancellationToken)
    {
        if (!EnsurePlatform(out var code, out var message))
        {
            return AppResult<IReadOnlyList<PlatformTenantResponse>>.Failure(code!, message!);
        }

        var tenants = await db.Tenants.AsNoTracking().OrderBy(t => t.Code).ToListAsync(cancellationToken);
        await auditWriter.WriteAsync(null, "platform.tenants.listed", nameof(Tenant), null, null, cancellationToken);
        return AppResult<IReadOnlyList<PlatformTenantResponse>>.Success(tenants.Select(MapTenant).ToList());
    }

    public async Task<AppResult<PlatformTenantResponse>> GetTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        if (!EnsurePlatform(out var code, out var message))
        {
            return AppResult<PlatformTenantResponse>.Failure(code!, message!);
        }

        var tenant = await db.Tenants.AsNoTracking().SingleOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
        if (tenant is null)
        {
            return AppResult<PlatformTenantResponse>.Failure(ErrorCodes.TenantNotFound, "Tenant not found.");
        }

        await auditWriter.WriteAsync(
            tenantId, "platform.tenant.viewed", nameof(Tenant), tenantId, null, cancellationToken);
        return AppResult<PlatformTenantResponse>.Success(MapTenant(tenant));
    }

    public async Task<AppResult<PlatformTenantResponse>> SuspendAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        if (!EnsurePlatform(out var code, out var message))
        {
            return AppResult<PlatformTenantResponse>.Failure(code!, message!);
        }

        var tenant = await db.Tenants.SingleOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
        if (tenant is null)
        {
            return AppResult<PlatformTenantResponse>.Failure(ErrorCodes.TenantNotFound, "Tenant not found.");
        }

        tenant.Suspend(DateTimeOffset.UtcNow);
        await db.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(
            tenantId, "platform.tenant.suspended", nameof(Tenant), tenantId, null, cancellationToken);
        return AppResult<PlatformTenantResponse>.Success(MapTenant(tenant));
    }

    public async Task<AppResult<PlatformTenantResponse>> ResumeAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        if (!EnsurePlatform(out var code, out var message))
        {
            return AppResult<PlatformTenantResponse>.Failure(code!, message!);
        }

        var tenant = await db.Tenants.SingleOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
        if (tenant is null)
        {
            return AppResult<PlatformTenantResponse>.Failure(ErrorCodes.TenantNotFound, "Tenant not found.");
        }

        tenant.Resume(DateTimeOffset.UtcNow);
        await db.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(
            tenantId, "platform.tenant.resumed", nameof(Tenant), tenantId, null, cancellationToken);
        return AppResult<PlatformTenantResponse>.Success(MapTenant(tenant));
    }

    public async Task<AppResult<PlatformTenantResponse>> UpdateSettingsAsync(
        UpdateTenantSettingsCommand command,
        CancellationToken cancellationToken)
    {
        if (!EnsurePlatform(out var code, out var message))
        {
            return AppResult<PlatformTenantResponse>.Failure(code!, message!);
        }

        var tenant = await db.Tenants.SingleOrDefaultAsync(t => t.Id == command.TenantId, cancellationToken);
        if (tenant is null)
        {
            return AppResult<PlatformTenantResponse>.Failure(ErrorCodes.TenantNotFound, "Tenant not found.");
        }

        if (tenant.RowVersion != command.ExpectedRowVersion)
        {
            return AppResult<PlatformTenantResponse>.Failure(ErrorCodes.SyncConflict, "Tenant was modified.");
        }

        var utcNow = DateTimeOffset.UtcNow;
        try
        {
            tenant.UpdateProfile(command.Name, command.CurrencyCode, command.TimeZoneId, utcNow);
            tenant.SetNegativeStockPolicy(MapPolicy(command.NegativeStockPolicy), utcNow);
            tenant.ConfigureTax(command.TaxEnabled, command.TaxName, command.TaxRate, utcNow);
            tenant.ConfigureOfflinePolicy(command.MaxOfflineDays, command.RequireOnlineRevalidation, utcNow);
        }
        catch (ArgumentException ex)
        {
            return AppResult<PlatformTenantResponse>.Failure(ErrorCodes.ValidationFailed, ex.Message);
        }

        await db.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(
            command.TenantId, "platform.tenant.settings_updated", nameof(Tenant), command.TenantId, null, cancellationToken);
        return AppResult<PlatformTenantResponse>.Success(MapTenant(tenant));
    }

    public async Task<AppResult<SubscriptionResponse>> GetSubscriptionAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        if (!EnsurePlatform(out var code, out var message))
        {
            return AppResult<SubscriptionResponse>.Failure(code!, message!);
        }

        if (!await TenantExistsAsync(tenantId, cancellationToken))
        {
            return AppResult<SubscriptionResponse>.Failure(ErrorCodes.TenantNotFound, "Tenant not found.");
        }

        ambient.SetTenantScope(tenantId, isPlatformOperator: true);
        var sub = await db.TenantSubscriptions.AsNoTracking()
            .SingleOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);
        if (sub is null)
        {
            return AppResult<SubscriptionResponse>.Failure(ErrorCodes.InvalidOperation, "Subscription not found.");
        }

        var plan = await db.Plans.AsNoTracking().SingleAsync(p => p.Id == sub.PlanId, cancellationToken);
        await auditWriter.WriteAsync(
            tenantId, "platform.subscription.viewed", nameof(TenantSubscription), sub.Id, null, cancellationToken);
        return AppResult<SubscriptionResponse>.Success(MapSubscription(sub, plan));
    }

    public async Task<AppResult<SubscriptionResponse>> UpdateSubscriptionAsync(
        UpdateSubscriptionCommand command,
        CancellationToken cancellationToken)
    {
        if (!EnsurePlatform(out var code, out var message))
        {
            return AppResult<SubscriptionResponse>.Failure(code!, message!);
        }

        if (!await TenantExistsAsync(command.TenantId, cancellationToken))
        {
            return AppResult<SubscriptionResponse>.Failure(ErrorCodes.TenantNotFound, "Tenant not found.");
        }

        var planCode = command.PlanCode.Trim().ToUpperInvariant();
        var plan = await db.Plans.AsNoTracking()
            .SingleOrDefaultAsync(p => p.Code == planCode && p.IsActive, cancellationToken);
        if (plan is null)
        {
            return AppResult<SubscriptionResponse>.Failure(ErrorCodes.ValidationFailed, $"Unknown plan '{planCode}'.");
        }

        ambient.SetTenantScope(command.TenantId, isPlatformOperator: true);
        var sub = await db.TenantSubscriptions.SingleOrDefaultAsync(
            s => s.TenantId == command.TenantId, cancellationToken);
        if (sub is null)
        {
            return AppResult<SubscriptionResponse>.Failure(ErrorCodes.InvalidOperation, "Subscription not found.");
        }

        if (sub.RowVersion != command.ExpectedRowVersion)
        {
            return AppResult<SubscriptionResponse>.Failure(ErrorCodes.SyncConflict, "Subscription was modified.");
        }

        var utcNow = DateTimeOffset.UtcNow;
        sub.AssignPlan(plan.Id, utcNow);
        sub.SetStatus(command.Status, utcNow);
        sub.SetPeriod(command.CurrentPeriodStartUtc, command.CurrentPeriodEndUtc, utcNow);
        sub.SetExternalReference(command.ExternalReference, utcNow);
        sub.SetTrialEndsAt(command.TrialEndsAtUtc, utcNow);

        await db.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(
            command.TenantId,
            "platform.subscription.updated",
            nameof(TenantSubscription),
            sub.Id,
            JsonSerializer.Serialize(new { plan.Code, Status = command.Status.ToString() }),
            cancellationToken);

        return AppResult<SubscriptionResponse>.Success(MapSubscription(sub, plan));
    }

    public async Task<AppResult<TenantUsageResponse>> GetUsageAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        if (!EnsurePlatform(out var code, out var message))
        {
            return AppResult<TenantUsageResponse>.Failure(code!, message!);
        }

        if (!await TenantExistsAsync(tenantId, cancellationToken))
        {
            return AppResult<TenantUsageResponse>.Failure(ErrorCodes.TenantNotFound, "Tenant not found.");
        }

        ambient.SetTenantScope(tenantId, isPlatformOperator: true);
        var userCount = await db.Users.CountAsync(cancellationToken);
        var warehouseCount = await db.Warehouses.CountAsync(cancellationToken);
        var deviceCount = await db.Devices.CountAsync(cancellationToken);
        var activeDeviceCount = await db.Devices.CountAsync(
            d => d.Status != DeviceStatus.Revoked, cancellationToken);
        var productCount = await db.Products.CountAsync(cancellationToken);

        int? maxUsers = null, maxWarehouses = null, maxDevices = null, maxProducts = null;
        var sub = await db.TenantSubscriptions.AsNoTracking()
            .SingleOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);
        if (sub is not null)
        {
            var plan = await db.Plans.AsNoTracking().SingleAsync(p => p.Id == sub.PlanId, cancellationToken);
            maxUsers = plan.MaxUsers;
            maxWarehouses = plan.MaxWarehouses;
            maxDevices = plan.MaxDevices;
            maxProducts = plan.MaxProducts;
        }

        await auditWriter.WriteAsync(
            tenantId, "platform.usage.viewed", nameof(Tenant), tenantId, null, cancellationToken);

        return AppResult<TenantUsageResponse>.Success(new TenantUsageResponse(
            tenantId,
            userCount,
            warehouseCount,
            deviceCount,
            activeDeviceCount,
            productCount,
            maxUsers,
            maxWarehouses,
            maxDevices,
            maxProducts));
    }

    public async Task<AppResult<TenantReadinessResponse>> GetReadinessAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        if (!EnsurePlatform(out var code, out var message))
        {
            return AppResult<TenantReadinessResponse>.Failure(code!, message!);
        }

        var tenant = await db.Tenants.AsNoTracking().SingleOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
        if (tenant is null)
        {
            return AppResult<TenantReadinessResponse>.Failure(ErrorCodes.TenantNotFound, "Tenant not found.");
        }

        ambient.SetTenantScope(tenantId, isPlatformOperator: true);
        var hasOwner = await db.Users.AnyAsync(cancellationToken);
        var hasWarehouse = await db.Warehouses.AnyAsync(cancellationToken);
        var hasProduct = await db.Products.AnyAsync(cancellationToken);
        var sub = await db.TenantSubscriptions.AsNoTracking()
            .SingleOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);
        var hasActiveSub = sub is not null && sub.GrantsEntitlements;

        var missing = new List<string>();
        if (!hasOwner) missing.Add("owner_user");
        if (!hasWarehouse) missing.Add("warehouse");
        if (!hasProduct) missing.Add("product");
        if (!hasActiveSub) missing.Add("subscription");
        if (tenant.IsSuspended) missing.Add("unsuspend");

        await auditWriter.WriteAsync(
            tenantId, "platform.readiness.viewed", nameof(Tenant), tenantId, null, cancellationToken);

        return AppResult<TenantReadinessResponse>.Success(new TenantReadinessResponse(
            tenantId,
            hasOwner,
            hasWarehouse,
            hasProduct,
            hasActiveSub,
            tenant.IsSuspended,
            missing));
    }

    public async Task<AppResult<IReadOnlyList<PlanResponse>>> ListPlansAsync(CancellationToken cancellationToken)
    {
        if (!EnsurePlatform(out var code, out var message))
        {
            return AppResult<IReadOnlyList<PlanResponse>>.Failure(code!, message!);
        }

        var plans = await db.Plans.AsNoTracking().OrderBy(p => p.Code).ToListAsync(cancellationToken);
        return AppResult<IReadOnlyList<PlanResponse>>.Success(plans.Select(p => new PlanResponse(
            p.Id,
            p.Code,
            p.Name,
            p.IsActive,
            p.MaxUsers,
            p.MaxWarehouses,
            p.MaxDevices,
            p.MaxProducts,
            p.FeatureCodes)).ToList());
    }

    private bool EnsurePlatform(out string? errorCode, out string? errorMessage)
    {
        if (currentUser.IsAuthenticated && ambient.IsPlatformOperator)
        {
            errorCode = null;
            errorMessage = null;
            return true;
        }

        errorCode = ErrorCodes.Forbidden;
        errorMessage = "Platform operator required.";
        return false;
    }

    private async Task<bool> TenantExistsAsync(Guid tenantId, CancellationToken cancellationToken) =>
        await db.Tenants.AsNoTracking().AnyAsync(t => t.Id == tenantId, cancellationToken);

    private static PlatformTenantResponse MapTenant(Tenant tenant) =>
        new(
            tenant.Id,
            tenant.Code,
            tenant.Name,
            tenant.IsActive,
            tenant.IsSuspended,
            tenant.CurrencyCode,
            tenant.TimeZoneId,
            tenant.TaxEnabled,
            tenant.TaxName,
            tenant.TaxRate,
            tenant.MaxOfflineDays,
            tenant.RequireOnlineRevalidation,
            (NegativeStockPolicyDto)(int)tenant.NegativeStockPolicy,
            tenant.CreatedAtUtc,
            tenant.UpdatedAtUtc,
            tenant.RowVersion);

    private static SubscriptionResponse MapSubscription(TenantSubscription sub, Plan plan) =>
        new(
            sub.Id,
            sub.TenantId,
            sub.PlanId,
            plan.Code,
            plan.Name,
            sub.Status,
            sub.TrialEndsAtUtc,
            sub.CurrentPeriodStartUtc,
            sub.CurrentPeriodEndUtc,
            sub.ExternalReference,
            sub.RowVersion);

    private static NegativeStockPolicy MapPolicy(NegativeStockPolicyDto dto) =>
        dto switch
        {
            NegativeStockPolicyDto.Allow => NegativeStockPolicy.Allow,
            NegativeStockPolicyDto.AllowWithPermission => NegativeStockPolicy.AllowWithPermission,
            _ => NegativeStockPolicy.Prevent,
        };
}

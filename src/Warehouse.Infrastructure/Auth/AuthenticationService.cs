using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Auth;
using Warehouse.Application.Security;
using Warehouse.Application.Tenancy;
using Warehouse.Domain.Audit;
using Warehouse.Domain.Devices;
using Warehouse.Domain.Identity;
using Warehouse.Infrastructure.Persistence;
using Warehouse.Infrastructure.Tenancy;

namespace Warehouse.Infrastructure.Auth;

public sealed class AuthenticationService(
    WarehouseDbContext db,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IOptions<JwtOptions> jwtOptions,
    AmbientTenantContext ambientTenantContext,
    ITenantContext tenantContext) : IAuthenticationService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<AppResult<AuthTokenResponse>> LoginAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.TenantCode)
            || string.IsNullOrWhiteSpace(command.Email)
            || string.IsNullOrWhiteSpace(command.Password))
        {
            return AppResult<AuthTokenResponse>.Failure(ErrorCodes.ValidationFailed, "Tenant code, email, and password are required.");
        }

        var tenantCode = command.TenantCode.Trim().ToUpperInvariant();
        var email = command.Email.Trim().ToLowerInvariant();
        var utcNow = DateTimeOffset.UtcNow;

        var tenant = await db.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Code == tenantCode, cancellationToken);

        if (tenant is null || !tenant.IsActive)
        {
            return AppResult<AuthTokenResponse>.Failure(ErrorCodes.Unauthorized, "Invalid credentials.");
        }

        if (tenant.IsSuspended)
        {
            return AppResult<AuthTokenResponse>.Failure(ErrorCodes.Forbidden, "Tenant is suspended.");
        }

        ambientTenantContext.SetTenantScope(tenant.Id);

        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null || !passwordHasher.VerifyHashedPassword(user.PasswordHash, command.Password))
        {
            return AppResult<AuthTokenResponse>.Failure(ErrorCodes.Unauthorized, "Invalid credentials.");
        }

        if (!user.IsActive || user.IsDisabled)
        {
            return AppResult<AuthTokenResponse>.Failure(ErrorCodes.Forbidden, "User is disabled.");
        }

        Device? device = null;
        if (!string.IsNullOrWhiteSpace(command.DeviceKey))
        {
            var deviceResult = await ResolveDeviceForLoginAsync(
                tenant.Id,
                user.Id,
                command.DeviceKey,
                command.DeviceName,
                command.ApplicationVersion,
                utcNow,
                cancellationToken);
            if (!deviceResult.IsSuccess)
            {
                return AppResult<AuthTokenResponse>.Failure(deviceResult.ErrorCode!, deviceResult.ErrorMessage!);
            }

            device = deviceResult.Value;
        }

        var (permissions, roles) = await LoadPermissionsAndRolesAsync(user.Id, cancellationToken);
        var tokens = await IssueTokensAsync(tenant.Id, user, device?.Id, permissions, roles, utcNow, cancellationToken);

        db.AuditLogs.Add(AuditLog.Create(
            Guid.NewGuid(),
            tenant.Id,
            user.Id,
            device?.Id,
            "auth.login",
            nameof(User),
            user.Id,
            utcNow,
            correlationId: null,
            ipAddress: null,
            metadataJson: null));

        await db.SaveChangesAsync(cancellationToken);
        return AppResult<AuthTokenResponse>.Success(tokens);
    }

    public async Task<AppResult<AuthTokenResponse>> RefreshAsync(RefreshCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            return AppResult<AuthTokenResponse>.Failure(ErrorCodes.ValidationFailed, "Refresh token is required.");
        }

        var utcNow = DateTimeOffset.UtcNow;
        var hash = RefreshTokenHasher.Hash(command.RefreshToken);

        var existing = await db.RefreshTokens
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        if (existing is null)
        {
            return AppResult<AuthTokenResponse>.Failure(ErrorCodes.Unauthorized, "Invalid refresh token.");
        }

        ambientTenantContext.SetTenantScope(existing.TenantId);

        if (!existing.IsActive(utcNow))
        {
            if (existing.IsRevoked)
            {
                await RevokeFamilyAsync(existing.FamilyId, utcNow, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }

            return AppResult<AuthTokenResponse>.Failure(ErrorCodes.Unauthorized, "Invalid refresh token.");
        }

        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == existing.TenantId, cancellationToken);
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == existing.UserId, cancellationToken);

        if (tenant is null || !tenant.IsActive || tenant.IsSuspended
            || user is null || !user.IsActive || user.IsDisabled)
        {
            existing.Revoke(utcNow);
            await db.SaveChangesAsync(cancellationToken);
            return AppResult<AuthTokenResponse>.Failure(ErrorCodes.Forbidden, "Account is not allowed to refresh.");
        }

        Device? device = null;
        if (existing.DeviceId is Guid deviceId)
        {
            device = await db.Devices.FirstOrDefaultAsync(d => d.Id == deviceId, cancellationToken);
            if (device is null || device.IsRevoked)
            {
                existing.Revoke(utcNow);
                await db.SaveChangesAsync(cancellationToken);
                return AppResult<AuthTokenResponse>.Failure(ErrorCodes.Forbidden, "Device is revoked.");
            }

            device.RecordOnlineAuthentication(utcNow);
        }

        // Absolute refresh ceiling is anchored to the family's first token creation time.
        var familyCreatedAtUtc = await db.RefreshTokens
            .Where(t => t.FamilyId == existing.FamilyId)
            .MinAsync(t => t.CreatedAtUtc, cancellationToken);

        var (permissions, roles) = await LoadPermissionsAndRolesAsync(user.Id, cancellationToken);

        var newTokenId = Guid.NewGuid();
        var rawRefresh = RefreshTokenHasher.GenerateRawToken();
        var refreshHash = RefreshTokenHasher.Hash(rawRefresh);
        var refreshExpires = ComputeRefreshExpiry(utcNow, familyCreatedAtUtc);

        var replacement = RefreshToken.Create(
            newTokenId,
            existing.TenantId,
            existing.UserId,
            existing.DeviceId,
            existing.FamilyId,
            refreshHash,
            utcNow,
            refreshExpires);

        existing.Revoke(utcNow, newTokenId);
        db.RefreshTokens.Add(replacement);

        var (accessToken, accessExpires) = jwtTokenService.CreateAccessToken(
            user.Id,
            tenant.Id,
            user.Email,
            user.DisplayName,
            existing.DeviceId,
            permissions,
            roles,
            isPlatformOperator: false);

        await db.SaveChangesAsync(cancellationToken);

        return AppResult<AuthTokenResponse>.Success(new AuthTokenResponse(
            accessToken,
            accessExpires,
            rawRefresh,
            refreshExpires,
            user.Id,
            tenant.Id,
            user.Email,
            user.DisplayName,
            existing.DeviceId,
            permissions,
            roles));
    }

    public async Task<AppResult> LogoutAsync(LogoutCommand command, CancellationToken cancellationToken)
    {
        var utcNow = DateTimeOffset.UtcNow;

        if (!string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            var hash = RefreshTokenHasher.Hash(command.RefreshToken);
            var existing = await db.RefreshTokens
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

            if (existing is not null)
            {
                ambientTenantContext.SetTenantScope(existing.TenantId);
                await RevokeFamilyAsync(existing.FamilyId, utcNow, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }
        }
        else if (tenantContext.TenantId is Guid tenantId && ambientTenantContext.UserId is Guid userId)
        {
            var familyIds = await db.RefreshTokens
                .Where(t => t.UserId == userId && t.RevokedAtUtc == null)
                .Select(t => t.FamilyId)
                .Distinct()
                .ToListAsync(cancellationToken);

            foreach (var familyId in familyIds)
            {
                await RevokeFamilyAsync(familyId, utcNow, cancellationToken);
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        return AppResult.Success();
    }

    public async Task<AppResult<CurrentUserResponse>> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        if (!ambientTenantContext.IsAuthenticated
            || ambientTenantContext.UserId is not Guid userId
            || ambientTenantContext.TenantId is null)
        {
            return AppResult<CurrentUserResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
        {
            return AppResult<CurrentUserResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        var (permissions, roles) = await LoadPermissionsAndRolesAsync(user.Id, cancellationToken);
        return AppResult<CurrentUserResponse>.Success(new CurrentUserResponse(
            user.Id,
            user.TenantId,
            user.Email,
            user.DisplayName,
            ambientTenantContext.DeviceId,
            user.IsActive && !user.IsDisabled,
            permissions,
            roles));
    }

    private async Task<AppResult<Device>> ResolveDeviceForLoginAsync(
        Guid tenantId,
        Guid userId,
        string deviceKey,
        string? deviceName,
        string? applicationVersion,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken)
    {
        var key = deviceKey.Trim();
        var device = await db.Devices.FirstOrDefaultAsync(d => d.DeviceKey == key, cancellationToken);
        if (device is null)
        {
            if (string.IsNullOrWhiteSpace(deviceName))
            {
                return AppResult<Device>.Failure(ErrorCodes.ValidationFailed, "Device name is required when registering a new device.");
            }

            device = Device.Register(
                Guid.NewGuid(),
                tenantId,
                key,
                deviceName,
                userId,
                applicationVersion,
                utcNow);
            db.Devices.Add(device);
            return AppResult<Device>.Success(device);
        }

        if (device.IsRevoked)
        {
            return AppResult<Device>.Failure(ErrorCodes.Forbidden, "Device is revoked.");
        }

        device.RecordOnlineAuthentication(utcNow, applicationVersion);
        return AppResult<Device>.Success(device);
    }

    private async Task<(IReadOnlyList<string> Permissions, IReadOnlyList<string> Roles)> LoadPermissionsAndRolesAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var roleRows = await (
            from ur in db.UserRoles
            join r in db.Roles on ur.RoleId equals r.Id
            where ur.UserId == userId && r.IsActive
            select r.Code).Distinct().ToListAsync(cancellationToken);

        var permissionRows = await (
            from ur in db.UserRoles
            join rp in db.RolePermissions on ur.RoleId equals rp.RoleId
            join p in db.Permissions on rp.PermissionId equals p.Id
            where ur.UserId == userId
            select p.Code).Distinct().ToListAsync(cancellationToken);

        return (permissionRows, roleRows);
    }

    private async Task<AuthTokenResponse> IssueTokensAsync(
        Guid tenantId,
        User user,
        Guid? deviceId,
        IReadOnlyList<string> permissions,
        IReadOnlyList<string> roles,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken)
    {
        var familyId = Guid.NewGuid();
        var tokenId = Guid.NewGuid();
        var rawRefresh = RefreshTokenHasher.GenerateRawToken();
        var refreshHash = RefreshTokenHasher.Hash(rawRefresh);
        var refreshExpires = utcNow.AddDays(_jwtOptions.RefreshTokenDays);

        db.RefreshTokens.Add(RefreshToken.Create(
            tokenId,
            tenantId,
            user.Id,
            deviceId,
            familyId,
            refreshHash,
            utcNow,
            refreshExpires));

        var (accessToken, accessExpires) = jwtTokenService.CreateAccessToken(
            user.Id,
            tenantId,
            user.Email,
            user.DisplayName,
            deviceId,
            permissions,
            roles,
            isPlatformOperator: false);

        await Task.CompletedTask;
        return new AuthTokenResponse(
            accessToken,
            accessExpires,
            rawRefresh,
            refreshExpires,
            user.Id,
            tenantId,
            user.Email,
            user.DisplayName,
            deviceId,
            permissions,
            roles);
    }

    private DateTimeOffset ComputeRefreshExpiry(DateTimeOffset utcNow, DateTimeOffset familyCreatedAtUtc)
    {
        var sliding = utcNow.AddDays(_jwtOptions.RefreshTokenDays);
        var absolute = familyCreatedAtUtc.AddDays(_jwtOptions.RefreshTokenAbsoluteDays);
        return sliding < absolute ? sliding : absolute;
    }

    private async Task RevokeFamilyAsync(Guid familyId, DateTimeOffset utcNow, CancellationToken cancellationToken)
    {
        var tokens = await db.RefreshTokens
            .Where(t => t.FamilyId == familyId && t.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.Revoke(utcNow);
        }
    }
}

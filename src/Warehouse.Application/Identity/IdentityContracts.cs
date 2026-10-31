using Warehouse.Application.Abstractions;

namespace Warehouse.Application.Identity;

public sealed record RoleResponse(
    Guid Id,
    string Code,
    string Name,
    bool IsSystem,
    bool IsActive,
    IReadOnlyList<string> PermissionCodes);

public sealed record PermissionResponse(Guid Id, string Code, string Name, string? Description);

public sealed record UserRoleSummary(Guid RoleId, string RoleCode, string RoleName);

public sealed record UserResponse(
    Guid Id,
    string Email,
    string? PhoneNumber,
    string DisplayName,
    bool IsActive,
    bool IsDisabled,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    int RowVersion,
    IReadOnlyList<UserRoleSummary> Roles);

public sealed record CreateUserCommand(
    string Email,
    string Password,
    string DisplayName,
    IReadOnlyList<Guid> RoleIds,
    string? PhoneNumber = null);

public sealed record UpdateUserCommand(
    Guid UserId,
    string DisplayName,
    IReadOnlyList<Guid> RoleIds,
    int ExpectedRowVersion);

public sealed record SetUserActiveCommand(Guid UserId, bool IsActive, int ExpectedRowVersion);

public sealed record ResetUserPasswordCommand(Guid UserId, string NewPassword, int ExpectedRowVersion);

public interface IRoleQueryService
{
    Task<IReadOnlyList<RoleResponse>> ListRolesAsync(CancellationToken cancellationToken);
}

public interface IPermissionQueryService
{
    Task<IReadOnlyList<PermissionResponse>> ListPermissionsAsync(CancellationToken cancellationToken);
}

public interface IUserManagementService
{
    Task<IReadOnlyList<UserResponse>> ListUsersAsync(CancellationToken cancellationToken);

    Task<AppResult<UserResponse>> GetUserAsync(Guid userId, CancellationToken cancellationToken);

    Task<AppResult<UserResponse>> CreateUserAsync(CreateUserCommand command, CancellationToken cancellationToken);

    Task<AppResult<UserResponse>> UpdateUserAsync(UpdateUserCommand command, CancellationToken cancellationToken);

    Task<AppResult<UserResponse>> SetUserActiveAsync(SetUserActiveCommand command, CancellationToken cancellationToken);

    Task<AppResult<UserResponse>> ResetPasswordAsync(
        ResetUserPasswordCommand command,
        CancellationToken cancellationToken);
}

public sealed record ProvisionTenantCommand(
    Guid TenantId,
    string TenantCode,
    string TenantName,
    string CurrencyCode,
    string TimeZoneId,
    Guid OwnerUserId,
    string OwnerEmail,
    string OwnerPassword,
    string OwnerDisplayName);

public sealed record ProvisionTenantResponse(
    Guid TenantId,
    Guid OwnerUserId,
    IReadOnlyList<string> SystemRoleCodes);

/// <summary>
/// Creates a tenant, provisions system roles with permissions, and creates the owner user.
/// Used by platform admin flows and integration tests. Not a public self-signup endpoint.
/// </summary>
public interface ITenantProvisioningService
{
    Task<Abstractions.AppResult<ProvisionTenantResponse>> ProvisionAsync(
        ProvisionTenantCommand command,
        CancellationToken cancellationToken);
}

namespace Warehouse.Application.Auth;

public sealed record LoginCommand(
    string TenantCode,
    string EmailOrPhone,
    string Password,
    string? DeviceKey,
    string? DeviceName,
    string? ApplicationVersion);

public sealed record StaffRegisterCommand(
    string TenantCode,
    string Email,
    string PhoneNumber,
    string Password,
    string DisplayName,
    string? DeviceKey,
    string? DeviceName,
    string? ApplicationVersion);

public sealed record RefreshCommand(string RefreshToken);

public sealed record LogoutCommand(string? RefreshToken);

public sealed record AuthTokenResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc,
    Guid UserId,
    Guid TenantId,
    string Email,
    string? PhoneNumber,
    string DisplayName,
    Guid? DeviceId,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<string> Roles);

public sealed record CurrentUserResponse(
    Guid UserId,
    Guid TenantId,
    string Email,
    string? PhoneNumber,
    string DisplayName,
    Guid? DeviceId,
    bool IsActive,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<string> Roles);

public interface IAuthenticationService
{
    Task<Abstractions.AppResult<AuthTokenResponse>> LoginAsync(LoginCommand command, CancellationToken cancellationToken);

    Task<Abstractions.AppResult<AuthTokenResponse>> RegisterStaffAsync(
        StaffRegisterCommand command,
        CancellationToken cancellationToken);

    Task<Abstractions.AppResult<AuthTokenResponse>> RefreshAsync(RefreshCommand command, CancellationToken cancellationToken);

    Task<Abstractions.AppResult> LogoutAsync(LogoutCommand command, CancellationToken cancellationToken);

    Task<Abstractions.AppResult<CurrentUserResponse>> GetCurrentUserAsync(CancellationToken cancellationToken);
}

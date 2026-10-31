using Warehouse.Application.Abstractions;
using Warehouse.Application.Auth;
using Warehouse.Application.Devices;
using Warehouse.Application.Identity;
using Warehouse.Api.Authorization;
using Warehouse.Api.Http;
using Warehouse.Shared.Api;

namespace Warehouse.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/auth").WithTags("Auth");

        group.MapPost("/login", LoginAsync)
            .AllowAnonymous()
            .RequireRateLimiting(Hosting.ProductionHardeningExtensions.AuthRateLimitPolicy)
            .WithName("Login")
            .Produces<AuthTokenResponse>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapPost("/register", RegisterStaffAsync)
            .AllowAnonymous()
            .RequireRateLimiting(Hosting.ProductionHardeningExtensions.AuthRateLimitPolicy)
            .WithName("RegisterStaff")
            .Produces<AuthTokenResponse>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/refresh", RefreshAsync)
            .AllowAnonymous()
            .RequireRateLimiting(Hosting.ProductionHardeningExtensions.AuthRateLimitPolicy)
            .WithName("Refresh")
            .Produces<AuthTokenResponse>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized);

        group.MapPost("/logout", LogoutAsync)
            .RequireAuthorization()
            .WithName("Logout")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized);

        group.MapGet("/me", MeAsync)
            .RequireAuthorization()
            .WithName("GetCurrentUser")
            .Produces<CurrentUserResponse>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized);

        return endpoints;
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        IAuthenticationService auth,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var identifier = request.EmailOrPhone ?? request.Email;
        var result = await auth.LoginAsync(
            new LoginCommand(
                request.TenantCode,
                identifier ?? string.Empty,
                request.Password,
                request.DeviceKey,
                request.DeviceName,
                request.ApplicationVersion),
            cancellationToken);

        return ToHttp(result, httpContext);
    }

    private static async Task<IResult> RegisterStaffAsync(
        StaffRegisterRequest request,
        IAuthenticationService auth,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await auth.RegisterStaffAsync(
            new StaffRegisterCommand(
                request.TenantCode,
                request.Email,
                request.PhoneNumber,
                request.Password,
                request.DisplayName,
                request.DeviceKey,
                request.DeviceName,
                request.ApplicationVersion),
            cancellationToken);

        return ToHttp(result, httpContext);
    }

    private static async Task<IResult> RefreshAsync(
        RefreshRequest request,
        IAuthenticationService auth,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await auth.RefreshAsync(new RefreshCommand(request.RefreshToken), cancellationToken);
        return ToHttp(result, httpContext);
    }

    private static async Task<IResult> LogoutAsync(
        LogoutRequest? request,
        IAuthenticationService auth,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await auth.LogoutAsync(new LogoutCommand(request?.RefreshToken), cancellationToken);
        if (!result.IsSuccess)
        {
            return Results.Json(
                new ApiErrorResponse(result.ErrorCode!, result.ErrorMessage!, CorrelationId(httpContext)),
                statusCode: StatusFromCode(result.ErrorCode!));
        }

        return Results.NoContent();
    }

    private static async Task<IResult> MeAsync(
        IAuthenticationService auth,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await auth.GetCurrentUserAsync(cancellationToken);
        return ToHttp(result, httpContext);
    }

    private static IResult ToHttp<T>(AppResult<T> result, HttpContext httpContext)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        return Results.Json(
            new ApiErrorResponse(result.ErrorCode!, result.ErrorMessage!, CorrelationId(httpContext)),
            statusCode: StatusFromCode(result.ErrorCode!));
    }

    private static string CorrelationId(HttpContext httpContext) =>
        httpContext.Items[Http.CorrelationIdMiddleware.ItemKey] as string ?? "unknown";

    private static int StatusFromCode(string code) => code switch
    {
        ApiErrorCodes.Unauthorized => StatusCodes.Status401Unauthorized,
        ApiErrorCodes.Forbidden => StatusCodes.Status403Forbidden,
        ApiErrorCodes.ValidationFailed => StatusCodes.Status400BadRequest,
        ApiErrorCodes.TenantNotFound => StatusCodes.Status404NotFound,
        _ => StatusCodes.Status400BadRequest,
    };

    public sealed record LoginRequest(
        string TenantCode,
        string? Email,
        string? EmailOrPhone,
        string Password,
        string? DeviceKey,
        string? DeviceName,
        string? ApplicationVersion);

    public sealed record StaffRegisterRequest(
        string TenantCode,
        string Email,
        string PhoneNumber,
        string Password,
        string DisplayName,
        string? DeviceKey,
        string? DeviceName,
        string? ApplicationVersion);

    public sealed record RefreshRequest(string RefreshToken);

    public sealed record LogoutRequest(string? RefreshToken);
}

public static class DeviceEndpoints
{
    public static IEndpointRouteBuilder MapDeviceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/devices").WithTags("Devices").RequireAuthorization();

        group.MapPost("/register", RegisterAsync)
            .WithName("RegisterDevice")
            .Produces<DeviceResponse>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden);

        return endpoints;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterDeviceRequest request,
        IDeviceRegistrationService devices,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await devices.RegisterAsync(
            new RegisterDeviceCommand(request.DeviceKey, request.Name, request.ApplicationVersion, request.WarehouseId),
            cancellationToken);

        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        var status = result.ErrorCode switch
        {
            ApiErrorCodes.Unauthorized => StatusCodes.Status401Unauthorized,
            ApiErrorCodes.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status400BadRequest,
        };

        return Results.Json(
            new ApiErrorResponse(
                result.ErrorCode!,
                result.ErrorMessage!,
                httpContext.Items[Http.CorrelationIdMiddleware.ItemKey] as string ?? "unknown"),
            statusCode: status);
    }

    public sealed record RegisterDeviceRequest(
        string DeviceKey,
        string Name,
        string? ApplicationVersion,
        Guid? WarehouseId = null);
}

public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/roles", ListRolesAsync)
            .WithTags("Roles")
            .WithName("ListRoles")
            .RequireAuthorization()
            .RequirePermission("users.manage")
            .Produces<IReadOnlyList<RoleResponse>>(StatusCodes.Status200OK);

        endpoints.MapGet("/api/v1/permissions", ListPermissionsAsync)
            .WithTags("Permissions")
            .WithName("ListPermissions")
            .RequireAuthorization()
            .RequirePermission("users.manage")
            .Produces<IReadOnlyList<PermissionResponse>>(StatusCodes.Status200OK);

        var users = endpoints.MapGroup("/api/v1/users").WithTags("Users").RequireAuthorization();
        users.MapGet("/", ListUsersAsync)
            .RequirePermission("users.manage")
            .WithName("ListUsers")
            .Produces<IReadOnlyList<UserResponse>>(StatusCodes.Status200OK);
        users.MapGet("/{id:guid}", GetUserAsync)
            .RequirePermission("users.manage")
            .WithName("GetUser")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);
        users.MapPost("/", CreateUserAsync)
            .RequirePermission("users.manage")
            .WithName("CreateUser")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);
        users.MapPut("/{id:guid}", UpdateUserAsync)
            .RequirePermission("users.manage")
            .WithName("UpdateUser")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);
        users.MapPost("/{id:guid}/set-active", SetUserActiveAsync)
            .RequirePermission("users.manage")
            .WithName("SetUserActive")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);
        users.MapPost("/{id:guid}/reset-password", ResetUserPasswordAsync)
            .RequirePermission("users.manage")
            .WithName("ResetUserPassword")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);

        return endpoints;
    }

    private static async Task<IResult> ListRolesAsync(IRoleQueryService roles, CancellationToken cancellationToken)
    {
        var items = await roles.ListRolesAsync(cancellationToken);
        return Results.Ok(items);
    }

    private static async Task<IResult> ListPermissionsAsync(
        IPermissionQueryService permissions,
        CancellationToken cancellationToken)
    {
        var items = await permissions.ListPermissionsAsync(cancellationToken);
        return Results.Ok(items);
    }

    private static async Task<IResult> ListUsersAsync(IUserManagementService users, CancellationToken cancellationToken) =>
        Results.Ok(await users.ListUsersAsync(cancellationToken));

    private static async Task<IResult> GetUserAsync(
        Guid id,
        IUserManagementService users,
        HttpContext http,
        CancellationToken cancellationToken) =>
        ToHttp(await users.GetUserAsync(id, cancellationToken), http);

    private static async Task<IResult> CreateUserAsync(
        CreateUserRequest request,
        IUserManagementService users,
        HttpContext http,
        CancellationToken cancellationToken) =>
        ToHttp(
            await users.CreateUserAsync(
                new CreateUserCommand(
                    request.Email,
                    request.Password,
                    request.DisplayName,
                    request.RoleIds,
                    request.PhoneNumber),
                cancellationToken),
            http);

    private static async Task<IResult> UpdateUserAsync(
        Guid id,
        UpdateUserRequest request,
        IUserManagementService users,
        HttpContext http,
        CancellationToken cancellationToken) =>
        ToHttp(
            await users.UpdateUserAsync(
                new UpdateUserCommand(id, request.DisplayName, request.RoleIds, request.ExpectedRowVersion),
                cancellationToken),
            http);

    private static async Task<IResult> SetUserActiveAsync(
        Guid id,
        SetUserActiveRequest request,
        IUserManagementService users,
        HttpContext http,
        CancellationToken cancellationToken) =>
        ToHttp(
            await users.SetUserActiveAsync(
                new SetUserActiveCommand(id, request.IsActive, request.ExpectedRowVersion),
                cancellationToken),
            http);

    private static async Task<IResult> ResetUserPasswordAsync(
        Guid id,
        ResetUserPasswordRequest request,
        IUserManagementService users,
        HttpContext http,
        CancellationToken cancellationToken) =>
        ToHttp(
            await users.ResetPasswordAsync(
                new ResetUserPasswordCommand(id, request.NewPassword, request.ExpectedRowVersion),
                cancellationToken),
            http);

    private static IResult ToHttp<T>(AppResult<T> result, HttpContext http)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        var status = StatusFromCode(result.ErrorCode!);
        return Results.Json(
            new ApiErrorResponse(result.ErrorCode!, result.ErrorMessage ?? "Request failed.", CorrelationId(http)),
            statusCode: status);
    }

    private static string CorrelationId(HttpContext http) =>
        http.Items[CorrelationIdMiddleware.ItemKey] as string ?? "unknown";

    private static int StatusFromCode(string code) => code switch
    {
        ApiErrorCodes.Unauthorized => StatusCodes.Status401Unauthorized,
        ApiErrorCodes.Forbidden => StatusCodes.Status403Forbidden,
        ApiErrorCodes.SyncConflict => StatusCodes.Status409Conflict,
        ApiErrorCodes.ValidationFailed => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status400BadRequest,
    };

    public sealed record CreateUserRequest(
        string Email,
        string Password,
        string DisplayName,
        IReadOnlyList<Guid> RoleIds,
        string? PhoneNumber = null);

    public sealed record UpdateUserRequest(
        string DisplayName,
        IReadOnlyList<Guid> RoleIds,
        int ExpectedRowVersion);

    public sealed record SetUserActiveRequest(bool IsActive, int ExpectedRowVersion);

    public sealed record ResetUserPasswordRequest(string NewPassword, int ExpectedRowVersion);
}

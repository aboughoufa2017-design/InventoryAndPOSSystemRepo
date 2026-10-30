using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Warehouse.Infrastructure.Auth;

namespace Warehouse.Api.Authorization;

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}

/// <summary>
/// Singleton-safe: reads permission claims from the principal, not scoped services.
/// </summary>
public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated == true
            && context.User.HasClaim(AuthClaimTypes.Permission, requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public static class AuthorizationExtensions
{
    public const string PermissionPolicyPrefix = "perm:";
    public const string PlatformOperatorPolicy = "PlatformOperator";

    public static IServiceCollection AddWarehouseAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                PlatformOperatorPolicy,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireClaim(AuthClaimTypes.IsPlatformOperator, "true"));
        });
        return services;
    }

    public static RouteHandlerBuilder RequirePlatformOperator(this RouteHandlerBuilder builder) =>
        builder.RequireAuthorization(PlatformOperatorPolicy);

    public static RouteHandlerBuilder RequirePermission(this RouteHandlerBuilder builder, string permissionCode)
    {
        return builder.RequireAuthorization($"{PermissionPolicyPrefix}{permissionCode}");
    }
}

/// <summary>
/// Resolves policies named <c>perm:{code}</c> into permission requirements.
/// </summary>
public sealed class PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(AuthorizationExtensions.PermissionPolicyPrefix, StringComparison.Ordinal))
        {
            var permission = policyName[AuthorizationExtensions.PermissionPolicyPrefix.Length..];
            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(permission))
                .Build();
        }

        return await base.GetPolicyAsync(policyName);
    }
}

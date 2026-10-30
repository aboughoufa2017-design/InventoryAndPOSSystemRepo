using System.Security.Claims;
using Warehouse.Infrastructure.Auth;
using Warehouse.Infrastructure.Tenancy;

namespace Warehouse.Api.Http;

/// <summary>
/// Maps authenticated JWT claims into the ambient tenant/user context used by EF query filters.
/// </summary>
public sealed class TenantContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, AmbientTenantContext ambient)
    {
        ambient.SetUnauthenticated();

        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = context.User.FindFirstValue(AuthClaimTypes.TenantId);
            var userClaim = context.User.FindFirstValue("sub")
                ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = context.User.FindFirstValue(ClaimTypes.Email)
                ?? context.User.FindFirstValue("email")
                ?? string.Empty;

            Guid? deviceId = null;
            var deviceClaim = context.User.FindFirstValue(AuthClaimTypes.DeviceId);
            if (Guid.TryParse(deviceClaim, out var parsedDevice))
            {
                deviceId = parsedDevice;
            }

            var isPlatform = string.Equals(
                context.User.FindFirstValue(AuthClaimTypes.IsPlatformOperator),
                "true",
                StringComparison.OrdinalIgnoreCase);

            var permissions = context.User.FindAll(AuthClaimTypes.Permission).Select(c => c.Value);

            if (isPlatform && Guid.TryParse(userClaim, out var platformUserId))
            {
                ambient.SetPlatformAuthenticated(platformUserId, email, permissions);
            }
            else if (Guid.TryParse(tenantClaim, out var tenantId) && Guid.TryParse(userClaim, out var userId))
            {
                ambient.SetAuthenticated(tenantId, userId, email, deviceId, isPlatform, permissions);
            }
        }

        await next(context);
    }
}

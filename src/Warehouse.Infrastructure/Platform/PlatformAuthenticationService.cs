using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Platform;
using Warehouse.Application.Security;
using Warehouse.Domain.Platform;
using Warehouse.Infrastructure.Auth;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Infrastructure.Platform;

public sealed class PlatformAuthenticationService(
    WarehouseDbContext db,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwt) : IPlatformAuthenticationService
{
    public async Task<AppResult<PlatformAuthResponse>> LoginAsync(
        PlatformLoginCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Email) || string.IsNullOrWhiteSpace(command.Password))
        {
            return AppResult<PlatformAuthResponse>.Failure(ErrorCodes.ValidationFailed, "Email and password are required.");
        }

        var email = command.Email.Trim().ToLowerInvariant();
        var op = await db.PlatformOperators.SingleOrDefaultAsync(o => o.Email == email, cancellationToken);
        if (op is null || !op.IsActive || !passwordHasher.VerifyHashedPassword(op.PasswordHash, command.Password))
        {
            return AppResult<PlatformAuthResponse>.Failure(ErrorCodes.Unauthorized, "Invalid credentials.");
        }

        var (token, expires) = jwt.CreatePlatformAccessToken(op.Id, op.Email, op.DisplayName);
        return AppResult<PlatformAuthResponse>.Success(
            new PlatformAuthResponse(token, expires, op.Id, op.Email, op.DisplayName));
    }
}

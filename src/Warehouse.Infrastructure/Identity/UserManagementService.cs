using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Identity;
using Warehouse.Application.Security;
using Warehouse.Application.Tenancy;
using Warehouse.Domain.Identity;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Infrastructure.Identity;

public sealed class UserManagementService(
    WarehouseDbContext db,
    ITenantContext tenantContext,
    ICurrentUser currentUser,
    IPasswordHasher passwordHasher) : IUserManagementService
{
    public async Task<IReadOnlyList<UserResponse>> ListUsersAsync(CancellationToken cancellationToken)
    {
        EnsureTenant();
        var users = await db.Users.AsNoTracking().OrderBy(u => u.Email).ToListAsync(cancellationToken);
        return await MapManyAsync(users, cancellationToken);
    }

    public async Task<AppResult<UserResponse>> GetUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return AppResult<UserResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
        {
            return AppResult<UserResponse>.Failure(ErrorCodes.ValidationFailed, "User not found.");
        }

        return AppResult<UserResponse>.Success(await MapOneAsync(user, cancellationToken));
    }

    public async Task<AppResult<UserResponse>> CreateUserAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<UserResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (string.IsNullOrWhiteSpace(command.Email)
            || string.IsNullOrWhiteSpace(command.Password)
            || string.IsNullOrWhiteSpace(command.DisplayName))
        {
            return AppResult<UserResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "Email, password, and display name are required.");
        }

        if (command.Password.Trim().Length < 8)
        {
            return AppResult<UserResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "Password must be at least 8 characters.");
        }

        if (command.RoleIds is null || command.RoleIds.Count == 0)
        {
            return AppResult<UserResponse>.Failure(ErrorCodes.ValidationFailed, "At least one role is required.");
        }

        var email = command.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == email, cancellationToken))
        {
            return AppResult<UserResponse>.Failure(ErrorCodes.InvalidOperation, "Email already exists.");
        }

        var phone = User.NormalizePhone(command.PhoneNumber);
        if (phone is not null && await db.Users.AnyAsync(u => u.PhoneNumber == phone, cancellationToken))
        {
            return AppResult<UserResponse>.Failure(ErrorCodes.InvalidOperation, "Phone number already exists.");
        }

        var roles = await db.Roles
            .Where(r => command.RoleIds.Contains(r.Id) && r.IsActive)
            .ToListAsync(cancellationToken);

        if (roles.Count != command.RoleIds.Distinct().Count())
        {
            return AppResult<UserResponse>.Failure(ErrorCodes.ValidationFailed, "One or more roles are invalid.");
        }

        var utcNow = DateTimeOffset.UtcNow;
        var user = User.Create(
            Guid.NewGuid(),
            tenantId,
            email,
            passwordHasher.HashPassword(command.Password),
            command.DisplayName,
            utcNow,
            phone);

        db.Users.Add(user);
        foreach (var role in roles)
        {
            db.UserRoles.Add(UserRole.Create(Guid.NewGuid(), tenantId, user.Id, role.Id, utcNow));
        }

        await db.SaveChangesAsync(cancellationToken);
        return AppResult<UserResponse>.Success(await MapOneAsync(user, cancellationToken));
    }

    public async Task<AppResult<UserResponse>> UpdateUserAsync(
        UpdateUserCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return AppResult<UserResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (string.IsNullOrWhiteSpace(command.DisplayName))
        {
            return AppResult<UserResponse>.Failure(ErrorCodes.ValidationFailed, "Display name is required.");
        }

        if (command.RoleIds is null || command.RoleIds.Count == 0)
        {
            return AppResult<UserResponse>.Failure(ErrorCodes.ValidationFailed, "At least one role is required.");
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);
        if (user is null)
        {
            return AppResult<UserResponse>.Failure(ErrorCodes.ValidationFailed, "User not found.");
        }

        if (user.RowVersion != command.ExpectedRowVersion)
        {
            return AppResult<UserResponse>.Failure(ErrorCodes.SyncConflict, "User was modified by another request.");
        }

        var roles = await db.Roles
            .Where(r => command.RoleIds.Contains(r.Id) && r.IsActive)
            .ToListAsync(cancellationToken);

        if (roles.Count != command.RoleIds.Distinct().Count())
        {
            return AppResult<UserResponse>.Failure(ErrorCodes.ValidationFailed, "One or more roles are invalid.");
        }

        var existingLinks = await db.UserRoles.Where(ur => ur.UserId == user.Id).ToListAsync(cancellationToken);
        var removingOwner = await WouldRemoveLastOwnerAsync(user.Id, roles, existingLinks, cancellationToken);
        if (removingOwner)
        {
            return AppResult<UserResponse>.Failure(
                ErrorCodes.InvalidOperation,
                "Cannot remove the last active Owner role assignment.");
        }

        var utcNow = DateTimeOffset.UtcNow;
        user.UpdateDisplayName(command.DisplayName, utcNow);

        db.UserRoles.RemoveRange(existingLinks);
        foreach (var role in roles)
        {
            db.UserRoles.Add(UserRole.Create(Guid.NewGuid(), tenantId, user.Id, role.Id, utcNow));
        }

        await db.SaveChangesAsync(cancellationToken);
        return AppResult<UserResponse>.Success(await MapOneAsync(user, cancellationToken));
    }

    public async Task<AppResult<UserResponse>> SetUserActiveAsync(
        SetUserActiveCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return AppResult<UserResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (currentUser.UserId == command.UserId && !command.IsActive)
        {
            return AppResult<UserResponse>.Failure(ErrorCodes.InvalidOperation, "You cannot disable your own account.");
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);
        if (user is null)
        {
            return AppResult<UserResponse>.Failure(ErrorCodes.ValidationFailed, "User not found.");
        }

        if (user.RowVersion != command.ExpectedRowVersion)
        {
            return AppResult<UserResponse>.Failure(ErrorCodes.SyncConflict, "User was modified by another request.");
        }

        if (!command.IsActive)
        {
            var isOwner = await UserHasOwnerRoleAsync(user.Id, cancellationToken);
            if (isOwner)
            {
                var otherOwners = await CountOtherActiveOwnersAsync(user.Id, cancellationToken);
                if (otherOwners == 0)
                {
                    return AppResult<UserResponse>.Failure(
                        ErrorCodes.InvalidOperation,
                        "Cannot disable the last active Owner.");
                }
            }

            user.Disable(DateTimeOffset.UtcNow);
        }
        else
        {
            user.Enable(DateTimeOffset.UtcNow);
        }

        await db.SaveChangesAsync(cancellationToken);
        return AppResult<UserResponse>.Success(await MapOneAsync(user, cancellationToken));
    }

    public async Task<AppResult<UserResponse>> ResetPasswordAsync(
        ResetUserPasswordCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return AppResult<UserResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (string.IsNullOrWhiteSpace(command.NewPassword) || command.NewPassword.Trim().Length < 8)
        {
            return AppResult<UserResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "Password must be at least 8 characters.");
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);
        if (user is null)
        {
            return AppResult<UserResponse>.Failure(ErrorCodes.ValidationFailed, "User not found.");
        }

        if (user.RowVersion != command.ExpectedRowVersion)
        {
            return AppResult<UserResponse>.Failure(ErrorCodes.SyncConflict, "User was modified by another request.");
        }

        user.SetPasswordHash(passwordHasher.HashPassword(command.NewPassword), DateTimeOffset.UtcNow);
        await db.SaveChangesAsync(cancellationToken);
        return AppResult<UserResponse>.Success(await MapOneAsync(user, cancellationToken));
    }

    private async Task<bool> WouldRemoveLastOwnerAsync(
        Guid userId,
        IReadOnlyList<Role> newRoles,
        IReadOnlyList<UserRole> existingLinks,
        CancellationToken cancellationToken)
    {
        var ownerRoleId = await db.Roles
            .AsNoTracking()
            .Where(r => r.Code == SystemRoles.Owner)
            .Select(r => (Guid?)r.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (ownerRoleId is null)
        {
            return false;
        }

        var hadOwner = existingLinks.Any(l => l.RoleId == ownerRoleId.Value);
        var keepsOwner = newRoles.Any(r => r.Id == ownerRoleId.Value);
        if (!hadOwner || keepsOwner)
        {
            return false;
        }

        return await CountOtherActiveOwnersAsync(userId, cancellationToken) == 0;
    }

    private async Task<bool> UserHasOwnerRoleAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await (
            from ur in db.UserRoles.AsNoTracking()
            join r in db.Roles.AsNoTracking() on ur.RoleId equals r.Id
            where ur.UserId == userId && r.Code == SystemRoles.Owner
            select ur.Id).AnyAsync(cancellationToken);
    }

    private async Task<int> CountOtherActiveOwnersAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await (
            from ur in db.UserRoles.AsNoTracking()
            join r in db.Roles.AsNoTracking() on ur.RoleId equals r.Id
            join u in db.Users.AsNoTracking() on ur.UserId equals u.Id
            where r.Code == SystemRoles.Owner
                  && u.Id != userId
                  && u.IsActive
                  && !u.IsDisabled
            select u.Id).Distinct().CountAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<UserResponse>> MapManyAsync(
        IReadOnlyList<User> users,
        CancellationToken cancellationToken)
    {
        if (users.Count == 0)
        {
            return [];
        }

        var userIds = users.Select(u => u.Id).ToList();
        var roleRows = await (
            from ur in db.UserRoles.AsNoTracking()
            join r in db.Roles.AsNoTracking() on ur.RoleId equals r.Id
            where userIds.Contains(ur.UserId)
            select new { ur.UserId, Role = new UserRoleSummary(r.Id, r.Code, r.Name) }).ToListAsync(cancellationToken);

        var byUser = roleRows
            .GroupBy(x => x.UserId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<UserRoleSummary>)g.Select(x => x.Role).OrderBy(r => r.RoleCode).ToList());

        return users.Select(u => new UserResponse(
            u.Id,
            u.Email,
            u.PhoneNumber,
            u.DisplayName,
            u.IsActive,
            u.IsDisabled,
            u.CreatedAtUtc,
            u.UpdatedAtUtc,
            u.RowVersion,
            byUser.GetValueOrDefault(u.Id, Array.Empty<UserRoleSummary>()))).ToList();
    }

    private async Task<UserResponse> MapOneAsync(User user, CancellationToken cancellationToken)
    {
        var mapped = await MapManyAsync([user], cancellationToken);
        return mapped[0];
    }

    private void EnsureTenant()
    {
        if (tenantContext.TenantId is null)
        {
            throw new InvalidOperationException("Tenant scope is required.");
        }
    }
}

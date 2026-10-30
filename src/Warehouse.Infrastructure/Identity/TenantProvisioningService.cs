using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Identity;
using Warehouse.Application.Security;
using Warehouse.Domain.Catalog;
using Warehouse.Domain.Identity;
using Warehouse.Domain.Tenancy;
using Warehouse.Infrastructure.Persistence;
using Warehouse.Infrastructure.Tenancy;

namespace Warehouse.Infrastructure.Identity;

public sealed class TenantProvisioningService(
    WarehouseDbContext db,
    IPasswordHasher passwordHasher,
    AmbientTenantContext ambientTenantContext) : ITenantProvisioningService
{
    public async Task<AppResult<ProvisionTenantResponse>> ProvisionAsync(
        ProvisionTenantCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.TenantCode)
            || string.IsNullOrWhiteSpace(command.OwnerEmail)
            || string.IsNullOrWhiteSpace(command.OwnerPassword)
            || string.IsNullOrWhiteSpace(command.OwnerDisplayName))
        {
            return AppResult<ProvisionTenantResponse>.Failure(
                ErrorCodes.ValidationFailed,
                "Tenant code, owner email, password, and display name are required.");
        }

        var utcNow = DateTimeOffset.UtcNow;
        var code = command.TenantCode.Trim();

        var exists = await db.Tenants.AnyAsync(t => t.Code == code || t.Id == command.TenantId, cancellationToken);
        if (exists)
        {
            return AppResult<ProvisionTenantResponse>.Failure(ErrorCodes.InvalidOperation, "Tenant already exists.");
        }

        var tenant = Tenant.Create(
            command.TenantId,
            code,
            command.TenantName,
            command.CurrencyCode,
            command.TimeZoneId,
            utcNow);

        db.Tenants.Add(tenant);
        await db.SaveChangesAsync(cancellationToken);

        ambientTenantContext.SetTenantScope(tenant.Id);

        var permissionByCode = await db.Permissions
            .AsNoTracking()
            .ToDictionaryAsync(p => p.Code, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var roleCodes = new List<string>();
        Role? ownerRole = null;

        foreach (var template in SystemRoles.Templates)
        {
            var role = Role.Create(
                Guid.NewGuid(),
                tenant.Id,
                template.Code,
                template.Name,
                isSystem: true,
                utcNow);

            db.Roles.Add(role);
            roleCodes.Add(role.Code);

            if (template.Code == SystemRoles.Owner)
            {
                ownerRole = role;
            }

            foreach (var permissionCode in template.PermissionCodes)
            {
                if (!permissionByCode.TryGetValue(permissionCode, out var permission))
                {
                    continue;
                }

                db.RolePermissions.Add(RolePermission.Create(
                    Guid.NewGuid(),
                    tenant.Id,
                    role.Id,
                    permission.Id,
                    utcNow));
            }
        }

        var passwordHash = passwordHasher.HashPassword(command.OwnerPassword);
        var owner = User.Create(
            command.OwnerUserId,
            tenant.Id,
            command.OwnerEmail,
            passwordHash,
            command.OwnerDisplayName,
            utcNow);

        db.Users.Add(owner);

        if (ownerRole is null)
        {
            return AppResult<ProvisionTenantResponse>.Failure(
                ErrorCodes.InternalError,
                "Owner system role template is missing.");
        }

        db.UserRoles.Add(UserRole.Create(Guid.NewGuid(), tenant.Id, owner.Id, ownerRole.Id, utcNow));

        foreach (var unit in SystemUnits.Catalog)
        {
            db.UnitsOfMeasure.Add(UnitOfMeasure.Create(
                Guid.NewGuid(),
                tenant.Id,
                unit.Code,
                unit.Name,
                isSystem: true,
                utcNow));
        }

        await db.SaveChangesAsync(cancellationToken);

        return AppResult<ProvisionTenantResponse>.Success(
            new ProvisionTenantResponse(tenant.Id, owner.Id, roleCodes));
    }
}

public sealed class RoleQueryService(WarehouseDbContext db) : IRoleQueryService
{
    public async Task<IReadOnlyList<RoleResponse>> ListRolesAsync(CancellationToken cancellationToken)
    {
        var roles = await db.Roles.AsNoTracking().OrderBy(r => r.Code).ToListAsync(cancellationToken);
        var roleIds = roles.Select(r => r.Id).ToList();

        var permissionLinks = await (
            from rp in db.RolePermissions.AsNoTracking()
            join p in db.Permissions.AsNoTracking() on rp.PermissionId equals p.Id
            where roleIds.Contains(rp.RoleId)
            select new { rp.RoleId, p.Code }).ToListAsync(cancellationToken);

        var byRole = permissionLinks
            .GroupBy(x => x.RoleId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<string>)g.Select(x => x.Code).OrderBy(c => c).ToList());

        return roles.Select(r => new RoleResponse(
            r.Id,
            r.Code,
            r.Name,
            r.IsSystem,
            r.IsActive,
            byRole.GetValueOrDefault(r.Id, Array.Empty<string>()))).ToList();
    }
}

public sealed class PermissionQueryService(WarehouseDbContext db) : IPermissionQueryService
{
    public async Task<IReadOnlyList<PermissionResponse>> ListPermissionsAsync(CancellationToken cancellationToken)
    {
        return await db.Permissions
            .AsNoTracking()
            .OrderBy(p => p.Code)
            .Select(p => new PermissionResponse(p.Id, p.Code, p.Name, p.Description))
            .ToListAsync(cancellationToken);
    }
}

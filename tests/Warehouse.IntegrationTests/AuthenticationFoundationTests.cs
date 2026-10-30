using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Identity;
using Warehouse.Domain.Identity;
using Warehouse.Infrastructure.Auth;
using Warehouse.Infrastructure.Identity;
using Warehouse.Infrastructure.Persistence;
using Warehouse.Infrastructure.Tenancy;

namespace Warehouse.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class AuthenticationFoundationTests
{
    private readonly PostgreSqlCollectionFixture _collection;

    public AuthenticationFoundationTests(PostgreSqlCollectionFixture collection)
    {
        _collection = collection;
    }

    [SkippableFact]
    public async Task Provision_creates_system_roles_and_owner_can_login()
    {
        Skip.If(!_collection.IsAvailable, PostgreSqlCollectionFixture.UnavailableReason);

        await using var setup = _collection.CreateContext(NullTenantContext.Instance);
        await setup.Database.MigrateAsync();

        var ambient = new AmbientTenantContext();
        await using var db = _collection.CreateContext(ambient);
        var provisioning = new TenantProvisioningService(db, new AspNetPasswordHasher(), ambient);

        var tenantId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var provision = await provisioning.ProvisionAsync(
            new ProvisionTenantCommand(
                tenantId,
                "AUTHCO",
                "Auth Co",
                "USD",
                "UTC",
                ownerId,
                "owner@authco.test",
                "Owner-Pass-123!",
                "Owner"),
            CancellationToken.None);

        Assert.True(provision.IsSuccess, provision.ErrorMessage);
        Assert.Contains(SystemRoles.Owner, provision.Value!.SystemRoleCodes);
        Assert.Equal(SystemRoles.Templates.Count, provision.Value.SystemRoleCodes.Count);

        ambient.SetTenantScope(tenantId);
        var roles = await db.Roles.Where(r => r.IsSystem).ToListAsync();
        Assert.Equal(SystemRoles.Templates.Count, roles.Count);
        Assert.All(roles, r => Assert.True(r.IsSystem));

        var jwtOptions = Microsoft.Extensions.Options.Options.Create(new JwtOptions
        {
            Issuer = "warehouse-test",
            Audience = "warehouse-test",
            SigningKey = "test-signing-key-at-least-32-chars!!",
            AccessTokenMinutes = 180,
            RefreshTokenDays = 14,
            RefreshTokenAbsoluteDays = 90,
        });

        var auth = new AuthenticationService(
            db,
            new AspNetPasswordHasher(),
            new JwtTokenService(jwtOptions),
            jwtOptions,
            ambient,
            ambient);

        var login = await auth.LoginAsync(
            new Application.Auth.LoginCommand(
                "AUTHCO",
                "owner@authco.test",
                "Owner-Pass-123!",
                DeviceKey: "device-1",
                DeviceName: "Test Device",
                ApplicationVersion: "1.0.0"),
            CancellationToken.None);

        Assert.True(login.IsSuccess, login.ErrorMessage);
        Assert.False(string.IsNullOrWhiteSpace(login.Value!.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(login.Value.RefreshToken));
        Assert.Contains("users.manage", login.Value.Permissions);
        Assert.Contains(SystemRoles.Owner, login.Value.Roles);
        Assert.NotNull(login.Value.DeviceId);

        var refresh = await auth.RefreshAsync(
            new Application.Auth.RefreshCommand(login.Value.RefreshToken),
            CancellationToken.None);
        Assert.True(refresh.IsSuccess, refresh.ErrorMessage);
        Assert.NotEqual(login.Value.RefreshToken, refresh.Value!.RefreshToken);

        var reuse = await auth.RefreshAsync(
            new Application.Auth.RefreshCommand(login.Value.RefreshToken),
            CancellationToken.None);
        Assert.False(reuse.IsSuccess);
        Assert.Equal("UNAUTHORIZED", reuse.ErrorCode);

        ambient.SetAuthenticated(
            tenantId,
            ownerId,
            "owner@authco.test",
            login.Value.DeviceId,
            false,
            login.Value.Permissions);

        var logout = await auth.LogoutAsync(
            new Application.Auth.LogoutCommand(refresh.Value.RefreshToken),
            CancellationToken.None);
        Assert.True(logout.IsSuccess);

        var afterLogout = await auth.RefreshAsync(
            new Application.Auth.RefreshCommand(refresh.Value.RefreshToken),
            CancellationToken.None);
        Assert.False(afterLogout.IsSuccess);
    }

    [SkippableFact]
    public async Task Refresh_tokens_table_exists_after_migrate()
    {
        Skip.If(!_collection.IsAvailable, PostgreSqlCollectionFixture.UnavailableReason);

        await using var context = _collection.CreateContext(NullTenantContext.Instance);
        await context.Database.MigrateAsync();
        var entity = context.Model.FindEntityType(typeof(RefreshToken));
        Assert.NotNull(entity);
        Assert.Equal("refresh_tokens", entity!.GetTableName());
    }
}

using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Devices;
using Warehouse.Application.Security;
using Warehouse.Application.Tenancy;
using Warehouse.Domain.Devices;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Infrastructure.Devices;

public sealed class DeviceRegistrationService(
    WarehouseDbContext db,
    ITenantContext tenantContext,
    ICurrentUser currentUser) : IDeviceRegistrationService
{
    public async Task<AppResult<DeviceResponse>> RegisterAsync(
        RegisterDeviceCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId || currentUser.UserId is not Guid userId)
        {
            return AppResult<DeviceResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (string.IsNullOrWhiteSpace(command.DeviceKey) || string.IsNullOrWhiteSpace(command.Name))
        {
            return AppResult<DeviceResponse>.Failure(ErrorCodes.ValidationFailed, "Device key and name are required.");
        }

        var utcNow = DateTimeOffset.UtcNow;
        var key = command.DeviceKey.Trim();
        var existing = await db.Devices.FirstOrDefaultAsync(d => d.DeviceKey == key, cancellationToken);
        if (existing is not null)
        {
            if (existing.IsRevoked)
            {
                return AppResult<DeviceResponse>.Failure(ErrorCodes.Forbidden, "Device is revoked.");
            }

            existing.RecordOnlineAuthentication(utcNow, command.ApplicationVersion);
            if (command.WarehouseId is Guid warehouseId)
            {
                if (!await db.Warehouses.AnyAsync(w => w.Id == warehouseId, cancellationToken))
                {
                    return AppResult<DeviceResponse>.Failure(ErrorCodes.ValidationFailed, "Warehouse was not found.");
                }

                existing.BindWarehouse(warehouseId, utcNow);
            }

            await db.SaveChangesAsync(cancellationToken);
            return AppResult<DeviceResponse>.Success(ToResponse(existing));
        }

        if (command.WarehouseId is Guid bindWarehouseId
            && !await db.Warehouses.AnyAsync(w => w.Id == bindWarehouseId, cancellationToken))
        {
            return AppResult<DeviceResponse>.Failure(ErrorCodes.ValidationFailed, "Warehouse was not found.");
        }

        var device = Device.Register(
            Guid.NewGuid(),
            tenantId,
            key,
            command.Name,
            userId,
            command.ApplicationVersion,
            utcNow,
            command.WarehouseId);

        db.Devices.Add(device);
        await db.SaveChangesAsync(cancellationToken);
        return AppResult<DeviceResponse>.Success(ToResponse(device));
    }

    private static DeviceResponse ToResponse(Device device) =>
        new(
            device.Id,
            device.DeviceKey,
            device.Name,
            device.Status.ToString(),
            device.WarehouseId,
            device.ApplicationVersion,
            device.LastSuccessfulOnlineAuthenticationAtUtc,
            device.CreatedAtUtc);
}

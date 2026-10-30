namespace Warehouse.Application.Devices;

public sealed record RegisterDeviceCommand(
    string DeviceKey,
    string Name,
    string? ApplicationVersion,
    Guid? WarehouseId = null);

public sealed record DeviceResponse(
    Guid Id,
    string DeviceKey,
    string Name,
    string Status,
    Guid? WarehouseId,
    string? ApplicationVersion,
    DateTimeOffset? LastSuccessfulOnlineAuthenticationAtUtc,
    DateTimeOffset CreatedAtUtc);

public interface IDeviceRegistrationService
{
    Task<Abstractions.AppResult<DeviceResponse>> RegisterAsync(
        RegisterDeviceCommand command,
        CancellationToken cancellationToken);
}

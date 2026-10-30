namespace Warehouse.Application.Security;

/// <summary>
/// Authenticated caller for the current request. Values come from the access token, never from the body.
/// </summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    Guid? UserId { get; }

    Guid? DeviceId { get; }

    string? Email { get; }

    IReadOnlyCollection<string> Permissions { get; }

    bool HasPermission(string permissionCode);
}

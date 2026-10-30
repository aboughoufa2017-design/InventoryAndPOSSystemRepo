using System.Text.Json;
using Warehouse.Application.Abstractions;
using Warehouse.Domain.Sync;

namespace Warehouse.Application.Sync;

public sealed record SyncPushOperationRequest(
    Guid OperationId,
    string EntityType,
    Guid EntityId,
    string OperationType,
    string PayloadJson,
    DateTimeOffset? ClientTimestampUtc,
    int? BaseVersion);

public sealed record SyncPushOperationResult(
    Guid OperationId,
    string Outcome,
    string? ErrorCode,
    string? Message,
    string? ResultPayloadJson,
    long? ChangeVersion);

public sealed record SyncPushRequest(IReadOnlyList<SyncPushOperationRequest> Operations);

public sealed record SyncPushResponse(
    IReadOnlyList<SyncPushOperationResult> Results,
    DeviceSecuritySnapshot Security);

public sealed record SyncPullRequest(long AfterChangeVersion, int Limit = 100);

public sealed record SyncChangeItem(
    long ChangeVersion,
    string EntityType,
    Guid EntityId,
    string Operation,
    string PayloadJson,
    DateTimeOffset OccurredAtUtc);

public sealed record SyncPullResponse(
    IReadOnlyList<SyncChangeItem> Changes,
    long LatestChangeVersion,
    bool HasMore,
    DeviceSecuritySnapshot Security);

public sealed record DeviceSecuritySnapshot(
    bool DeviceRevoked,
    bool UserDisabled,
    bool TenantSuspended,
    bool RequireOnlineRevalidation,
    int? MaxOfflineDays,
    DateTimeOffset? LastSuccessfulOnlineAuthenticationAtUtc,
    bool BlocksApply);

public interface ISyncService
{
    Task<AppResult<SyncPushResponse>> PushAsync(SyncPushRequest request, CancellationToken cancellationToken);

    Task<AppResult<SyncPullResponse>> PullAsync(SyncPullRequest request, CancellationToken cancellationToken);
}

public interface IChangeLogWriter
{
    Task<long> AppendAsync(
        string entityType,
        Guid entityId,
        ChangeLogOperation operation,
        string payloadJson,
        CancellationToken cancellationToken);
}

/// <summary>
/// Applies one push operation. Registered per EntityType + OperationType.
/// </summary>
public interface ISyncOperationHandler
{
    string EntityType { get; }

    string OperationType { get; }

    Task<SyncHandlerResult> HandleAsync(SyncPushOperationRequest operation, CancellationToken cancellationToken);
}

public sealed record SyncHandlerResult(
    string Outcome,
    string? ErrorCode,
    string? Message,
    string ResultPayloadJson,
    ChangeLogOperation? ChangeLogOperation = null,
    string? ChangeLogPayloadJson = null);

public static class SyncJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}

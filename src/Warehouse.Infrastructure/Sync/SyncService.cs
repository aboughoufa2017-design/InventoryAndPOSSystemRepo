using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Security;
using Warehouse.Application.Sync;
using Warehouse.Application.Tenancy;
using Warehouse.Domain.Devices;
using Warehouse.Domain.Security;
using Warehouse.Domain.Sync;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Infrastructure.Sync;

public sealed class SyncService(
    WarehouseDbContext db,
    ITenantContext tenantContext,
    ICurrentUser currentUser,
    IChangeLogWriter changeLog,
    IEnumerable<ISyncOperationHandler> handlers) : ISyncService
{
    private readonly Dictionary<(string EntityType, string OperationType), ISyncOperationHandler> _handlers =
        handlers.ToDictionary(
            h => (h.EntityType, h.OperationType),
            h => h,
            new EntityOperationComparer());

    public async Task<AppResult<SyncPushResponse>> PushAsync(
        SyncPushRequest request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId || currentUser.UserId is not Guid userId)
        {
            return AppResult<SyncPushResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        if (currentUser.DeviceId is not Guid deviceId)
        {
            return AppResult<SyncPushResponse>.Failure(ErrorCodes.ValidationFailed, "Device id claim is required for sync push.");
        }

        var security = await BuildSecurityAsync(tenantId, userId, deviceId, cancellationToken);
        if (security.BlocksApply)
        {
            return AppResult<SyncPushResponse>.Failure(
                ErrorCodes.Forbidden,
                DescribeSecurityBlock(security));
        }

        if (request.Operations is null || request.Operations.Count == 0)
        {
            return AppResult<SyncPushResponse>.Success(new SyncPushResponse([], security));
        }

        var results = new List<SyncPushOperationResult>(request.Operations.Count);
        foreach (var operation in request.Operations)
        {
            results.Add(await ProcessOneAsync(tenantId, userId, deviceId, operation, cancellationToken));
        }

        var device = await db.Devices.FirstOrDefaultAsync(d => d.Id == deviceId, cancellationToken);
        device?.RecordSuccessfulSync(DateTimeOffset.UtcNow, null);
        await db.SaveChangesAsync(cancellationToken);

        return AppResult<SyncPushResponse>.Success(new SyncPushResponse(results, security));
    }

    public async Task<AppResult<SyncPullResponse>> PullAsync(
        SyncPullRequest request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId || currentUser.UserId is not Guid userId)
        {
            return AppResult<SyncPullResponse>.Failure(ErrorCodes.Unauthorized, "Not authenticated.");
        }

        var deviceId = currentUser.DeviceId;
        var security = await BuildSecurityAsync(tenantId, userId, deviceId, cancellationToken);
        if (security.DeviceRevoked || security.UserDisabled || security.TenantSuspended)
        {
            return AppResult<SyncPullResponse>.Failure(
                ErrorCodes.Forbidden,
                DescribeSecurityBlock(security));
        }

        if (request.AfterChangeVersion < 0)
        {
            return AppResult<SyncPullResponse>.Failure(ErrorCodes.ValidationFailed, "AfterChangeVersion cannot be negative.");
        }

        var limit = request.Limit is < 1 or > 200 ? 100 : request.Limit;
        var rows = await db.ChangeLogEntries.AsNoTracking()
            .Where(c => c.ChangeVersion > request.AfterChangeVersion)
            .OrderBy(c => c.ChangeVersion)
            .Take(limit + 1)
            .ToListAsync(cancellationToken);

        var hasMore = rows.Count > limit;
        if (hasMore)
        {
            rows = rows.Take(limit).ToList();
        }

        var latest = await db.TenantChangeCounters.AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .Select(c => (long?)c.LastChangeVersion)
            .FirstOrDefaultAsync(cancellationToken) ?? 0L;

        if (deviceId is Guid did)
        {
            var device = await db.Devices.FirstOrDefaultAsync(d => d.Id == did, cancellationToken);
            device?.RecordSuccessfulSync(DateTimeOffset.UtcNow, null);
            await db.SaveChangesAsync(cancellationToken);
        }

        return AppResult<SyncPullResponse>.Success(new SyncPullResponse(
            rows.Select(r => new SyncChangeItem(
                r.ChangeVersion,
                r.EntityType,
                r.EntityId,
                r.Operation.ToString(),
                r.PayloadJson,
                r.OccurredAtUtc)).ToList(),
            latest,
            hasMore,
            security));
    }

    private async Task<SyncPushOperationResult> ProcessOneAsync(
        Guid tenantId,
        Guid userId,
        Guid deviceId,
        SyncPushOperationRequest operation,
        CancellationToken cancellationToken)
    {
        if (operation.OperationId == Guid.Empty || operation.EntityId == Guid.Empty
            || string.IsNullOrWhiteSpace(operation.EntityType)
            || string.IsNullOrWhiteSpace(operation.OperationType))
        {
            return new SyncPushOperationResult(
                operation.OperationId,
                SyncPushOutcomes.ValidationFailed,
                ErrorCodes.ValidationFailed,
                "OperationId, EntityType, EntityId, and OperationType are required.",
                null,
                null);
        }

        var existing = await db.ProcessedSyncOperations.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == operation.OperationId, cancellationToken);
        if (existing is not null)
        {
            return new SyncPushOperationResult(
                operation.OperationId,
                SyncPushOutcomes.AlreadyProcessed,
                ErrorCodes.OperationAlreadyProcessed,
                "Operation was already processed.",
                existing.ResultPayloadJson,
                null);
        }

        if (!_handlers.TryGetValue((operation.EntityType.Trim(), operation.OperationType.Trim()), out var handler))
        {
            var unsupported = JsonSerializer.Serialize(
                new { code = ErrorCodes.InvalidOperation, message = "Unsupported sync operation." },
                SyncJson.Options);
            await StoreProcessedAsync(
                operation,
                tenantId,
                deviceId,
                userId,
                SyncOperationProcessingStatus.Rejected,
                unsupported,
                cancellationToken);
            return new SyncPushOperationResult(
                operation.OperationId,
                SyncPushOutcomes.Rejected,
                ErrorCodes.InvalidOperation,
                "Unsupported sync operation.",
                unsupported,
                null);
        }

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Re-check inside transaction for races
            if (await db.ProcessedSyncOperations.AnyAsync(o => o.Id == operation.OperationId, cancellationToken))
            {
                await tx.RollbackAsync(cancellationToken);
                var replay = await db.ProcessedSyncOperations.AsNoTracking()
                    .FirstAsync(o => o.Id == operation.OperationId, cancellationToken);
                return new SyncPushOperationResult(
                    operation.OperationId,
                    SyncPushOutcomes.AlreadyProcessed,
                    ErrorCodes.OperationAlreadyProcessed,
                    "Operation was already processed.",
                    replay.ResultPayloadJson,
                    null);
            }

            var handled = await handler.HandleAsync(operation, cancellationToken);
            long? changeVersion = null;
            if (handled.Outcome == SyncPushOutcomes.Applied
                && handled.ChangeLogOperation is ChangeLogOperation changeOp)
            {
                changeVersion = await changeLog.AppendAsync(
                    operation.EntityType,
                    operation.EntityId,
                    changeOp,
                    handled.ChangeLogPayloadJson ?? handled.ResultPayloadJson,
                    cancellationToken);
            }

            var status = handled.Outcome is SyncPushOutcomes.Applied or SyncPushOutcomes.AlreadyProcessed
                ? SyncOperationProcessingStatus.Succeeded
                : SyncOperationProcessingStatus.Rejected;

            db.ProcessedSyncOperations.Add(
                status == SyncOperationProcessingStatus.Succeeded
                    ? ProcessedSyncOperation.CreateSucceeded(
                        operation.OperationId,
                        tenantId,
                        deviceId,
                        userId,
                        operation.EntityType,
                        operation.EntityId,
                        operation.OperationType,
                        handled.ResultPayloadJson,
                        DateTimeOffset.UtcNow)
                    : ProcessedSyncOperation.CreateRejected(
                        operation.OperationId,
                        tenantId,
                        deviceId,
                        userId,
                        operation.EntityType,
                        operation.EntityId,
                        operation.OperationType,
                        handled.ResultPayloadJson,
                        DateTimeOffset.UtcNow));

            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            return new SyncPushOperationResult(
                operation.OperationId,
                handled.Outcome,
                handled.ErrorCode,
                handled.Message,
                handled.ResultPayloadJson,
                changeVersion);
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync(cancellationToken);
            var replay = await db.ProcessedSyncOperations.AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == operation.OperationId, cancellationToken);
            if (replay is not null)
            {
                return new SyncPushOperationResult(
                    operation.OperationId,
                    SyncPushOutcomes.AlreadyProcessed,
                    ErrorCodes.OperationAlreadyProcessed,
                    "Operation was already processed.",
                    replay.ResultPayloadJson,
                    null);
            }

            return new SyncPushOperationResult(
                operation.OperationId,
                SyncPushOutcomes.Rejected,
                ErrorCodes.InternalError,
                "Sync operation failed due to a persistence conflict.",
                null,
                null);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return new SyncPushOperationResult(
                operation.OperationId,
                SyncPushOutcomes.Rejected,
                ErrorCodes.InternalError,
                ex.Message,
                null,
                null);
        }
    }

    private async Task StoreProcessedAsync(
        SyncPushOperationRequest operation,
        Guid tenantId,
        Guid deviceId,
        Guid userId,
        SyncOperationProcessingStatus status,
        string payload,
        CancellationToken cancellationToken)
    {
        db.ProcessedSyncOperations.Add(
            status == SyncOperationProcessingStatus.Succeeded
                ? ProcessedSyncOperation.CreateSucceeded(
                    operation.OperationId, tenantId, deviceId, userId,
                    operation.EntityType, operation.EntityId, operation.OperationType, payload, DateTimeOffset.UtcNow)
                : ProcessedSyncOperation.CreateRejected(
                    operation.OperationId, tenantId, deviceId, userId,
                    operation.EntityType, operation.EntityId, operation.OperationType, payload, DateTimeOffset.UtcNow));
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<DeviceSecuritySnapshot> BuildSecurityAsync(
        Guid tenantId,
        Guid userId,
        Guid? deviceId,
        CancellationToken cancellationToken)
    {
        var tenant = await db.Tenants.AsNoTracking().FirstAsync(t => t.Id == tenantId, cancellationToken);
        var user = await db.Users.AsNoTracking().FirstAsync(u => u.Id == userId, cancellationToken);
        Device? device = null;
        if (deviceId is Guid did)
        {
            device = await db.Devices.AsNoTracking().FirstOrDefaultAsync(d => d.Id == did, cancellationToken);
        }

        var policy = new OfflineSessionPolicy(
            tenant.MaxOfflineDays ?? OfflineSessionPolicy.DefaultMaxOfflineDays,
            device?.LastSuccessfulOnlineAuthenticationAtUtc,
            tenant.RequireOnlineRevalidation,
            device?.IsRevoked ?? false,
            user.IsDisabled,
            tenant.IsSuspended);

        return new DeviceSecuritySnapshot(
            policy.DeviceRevoked,
            policy.UserDisabled,
            policy.TenantSuspended,
            policy.RequireOnlineRevalidation,
            policy.MaxOfflineDays,
            policy.LastSuccessfulOnlineAuthentication,
            policy.BlocksServerApply);
    }

    private static string DescribeSecurityBlock(DeviceSecuritySnapshot security)
    {
        if (security.DeviceRevoked)
        {
            return "Device is revoked.";
        }

        if (security.UserDisabled)
        {
            return "User is disabled.";
        }

        if (security.TenantSuspended)
        {
            return "Tenant is suspended.";
        }

        return "Sync apply is blocked by security policy.";
    }

    private sealed class EntityOperationComparer : IEqualityComparer<(string EntityType, string OperationType)>
    {
        public bool Equals((string EntityType, string OperationType) x, (string EntityType, string OperationType) y) =>
            string.Equals(x.EntityType, y.EntityType, StringComparison.OrdinalIgnoreCase)
            && string.Equals(x.OperationType, y.OperationType, StringComparison.OrdinalIgnoreCase);

        public int GetHashCode((string EntityType, string OperationType) obj) =>
            HashCode.Combine(
                StringComparer.OrdinalIgnoreCase.GetHashCode(obj.EntityType),
                StringComparer.OrdinalIgnoreCase.GetHashCode(obj.OperationType));
    }
}

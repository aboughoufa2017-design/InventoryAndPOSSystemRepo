using System.Text.Json;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Inventory;
using Warehouse.Application.Sync;
using Warehouse.Domain.Sync;

namespace Warehouse.Infrastructure.Sync.Handlers;

public sealed class StockAdjustmentPostSyncHandler(IInventoryService inventory) : ISyncOperationHandler
{
    public string EntityType => SyncEntityTypes.StockAdjustment;

    public string OperationType => SyncOperationTypes.Post;

    public async Task<SyncHandlerResult> HandleAsync(SyncPushOperationRequest operation, CancellationToken cancellationToken)
    {
        StockAdjustmentPostPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<StockAdjustmentPostPayload>(operation.PayloadJson, SyncJson.Options);
        }
        catch (JsonException)
        {
            return Fail(SyncPushOutcomes.ValidationFailed, ErrorCodes.ValidationFailed, "Invalid stock adjustment payload.");
        }

        if (payload is null || payload.Lines is null || payload.Lines.Count == 0)
        {
            return Fail(SyncPushOutcomes.ValidationFailed, ErrorCodes.ValidationFailed, "Adjustment lines are required.");
        }

        var create = await inventory.CreateAdjustmentAsync(
            new CreateStockAdjustmentCommand(
                payload.WarehouseId,
                payload.Kind,
                payload.Reason,
                payload.Lines.Select(l => new StockAdjustmentLineRequest(l.ProductId, l.LocationId, l.QuantityChange, l.Notes)).ToList()),
            cancellationToken);

        if (!create.IsSuccess)
        {
            return MapFailure(create.ErrorCode!, create.ErrorMessage!);
        }

        // Prefer client entity id: if created id differs, still post the created document.
        // Offline clients should send EntityId matching a create that uses that id — InventoryService generates ids.
        // For Phase 8 we post the created adjustment and include both ids in the result.
        var post = await inventory.PostAdjustmentAsync(create.Value!.Id, cancellationToken);
        if (!post.IsSuccess)
        {
            return MapFailure(post.ErrorCode!, post.ErrorMessage!);
        }

        var resultJson = JsonSerializer.Serialize(post.Value, SyncJson.Options);
        return new SyncHandlerResult(
            SyncPushOutcomes.Applied,
            null,
            null,
            resultJson,
            ChangeLogOperation.Upsert,
            resultJson);
    }

    private static SyncHandlerResult MapFailure(string code, string message)
    {
        var outcome = code switch
        {
            ErrorCodes.SyncConflict => SyncPushOutcomes.Conflict,
            ErrorCodes.InsufficientStock => SyncPushOutcomes.Rejected,
            ErrorCodes.ValidationFailed => SyncPushOutcomes.ValidationFailed,
            ErrorCodes.Forbidden or ErrorCodes.Unauthorized => SyncPushOutcomes.Rejected,
            _ => SyncPushOutcomes.Rejected,
        };
        return Fail(outcome, code, message);
    }

    private static SyncHandlerResult Fail(string outcome, string code, string message) =>
        new(outcome, code, message, JsonSerializer.Serialize(new { code, message }, SyncJson.Options));

    private sealed record StockAdjustmentPostPayload(
        Guid WarehouseId,
        string Kind,
        string? Reason,
        IReadOnlyList<StockAdjustmentLinePayload> Lines);

    private sealed record StockAdjustmentLinePayload(
        Guid ProductId,
        Guid LocationId,
        decimal QuantityChange,
        string? Notes);
}

public sealed class TransferDispatchSyncHandler(ITransferService transfers) : ISyncOperationHandler
{
    public string EntityType => SyncEntityTypes.Transfer;

    public string OperationType => SyncOperationTypes.Dispatch;

    public async Task<SyncHandlerResult> HandleAsync(SyncPushOperationRequest operation, CancellationToken cancellationToken)
    {
        TransferMutationPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<TransferMutationPayload>(operation.PayloadJson, SyncJson.Options);
        }
        catch (JsonException)
        {
            return Fail(SyncPushOutcomes.ValidationFailed, ErrorCodes.ValidationFailed, "Invalid transfer payload.");
        }

        if (payload is null)
        {
            return Fail(SyncPushOutcomes.ValidationFailed, ErrorCodes.ValidationFailed, "Transfer payload is required.");
        }

        var result = await transfers.DispatchAsync(
            new DispatchTransferCommand(operation.EntityId, payload.ExpectedRowVersion, operation.OperationId),
            cancellationToken);
        return MapTransfer(result);
    }

    private static SyncHandlerResult MapTransfer(AppResult<TransferResponse> result)
    {
        if (result.IsSuccess)
        {
            var json = JsonSerializer.Serialize(result.Value, SyncJson.Options);
            var outcome = result.Value!.WasIdempotentReplay
                ? SyncPushOutcomes.AlreadyProcessed
                : SyncPushOutcomes.Applied;
            return new SyncHandlerResult(
                outcome,
                null,
                null,
                json,
                ChangeLogOperation.Upsert,
                json);
        }

        return MapFailure(result.ErrorCode!, result.ErrorMessage!);
    }

    private static SyncHandlerResult MapFailure(string code, string message)
    {
        var outcome = code switch
        {
            ErrorCodes.SyncConflict => SyncPushOutcomes.Conflict,
            ErrorCodes.InsufficientStock => SyncPushOutcomes.Rejected,
            ErrorCodes.ValidationFailed => SyncPushOutcomes.ValidationFailed,
            _ => SyncPushOutcomes.Rejected,
        };
        return Fail(outcome, code, message);
    }

    private static SyncHandlerResult Fail(string outcome, string code, string message) =>
        new(outcome, code, message, JsonSerializer.Serialize(new { code, message }, SyncJson.Options));

    private sealed record TransferMutationPayload(int ExpectedRowVersion);
}

public sealed class TransferReceiveSyncHandler(ITransferService transfers) : ISyncOperationHandler
{
    public string EntityType => SyncEntityTypes.Transfer;

    public string OperationType => SyncOperationTypes.Receive;

    public async Task<SyncHandlerResult> HandleAsync(SyncPushOperationRequest operation, CancellationToken cancellationToken)
    {
        TransferReceivePayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<TransferReceivePayload>(operation.PayloadJson, SyncJson.Options);
        }
        catch (JsonException)
        {
            return Fail(SyncPushOutcomes.ValidationFailed, ErrorCodes.ValidationFailed, "Invalid transfer receive payload.");
        }

        if (payload?.Lines is null || payload.Lines.Count == 0)
        {
            return Fail(SyncPushOutcomes.ValidationFailed, ErrorCodes.ValidationFailed, "Receive lines are required.");
        }

        var result = await transfers.ReceiveAsync(
            new ReceiveTransferCommand(
                operation.EntityId,
                payload.ExpectedRowVersion,
                operation.OperationId,
                payload.Lines.Select(l => new ReceiveTransferLineRequest(l.TransferLineId, l.Quantity)).ToList()),
            cancellationToken);

        if (result.IsSuccess)
        {
            var json = JsonSerializer.Serialize(result.Value, SyncJson.Options);
            var outcome = result.Value!.WasIdempotentReplay
                ? SyncPushOutcomes.AlreadyProcessed
                : SyncPushOutcomes.Applied;
            return new SyncHandlerResult(
                outcome,
                null,
                null,
                json,
                ChangeLogOperation.Upsert,
                json);
        }

        var failOutcome = result.ErrorCode switch
        {
            ErrorCodes.SyncConflict => SyncPushOutcomes.Conflict,
            ErrorCodes.InsufficientStock => SyncPushOutcomes.Rejected,
            ErrorCodes.ValidationFailed => SyncPushOutcomes.ValidationFailed,
            _ => SyncPushOutcomes.Rejected,
        };
        return Fail(failOutcome, result.ErrorCode!, result.ErrorMessage!);
    }

    private static SyncHandlerResult Fail(string outcome, string code, string message) =>
        new(outcome, code, message, JsonSerializer.Serialize(new { code, message }, SyncJson.Options));

    private sealed record TransferReceivePayload(
        int ExpectedRowVersion,
        IReadOnlyList<TransferReceiveLinePayload> Lines);

    private sealed record TransferReceiveLinePayload(Guid TransferLineId, decimal Quantity);
}

public sealed class TransferCancelSyncHandler(ITransferService transfers) : ISyncOperationHandler
{
    public string EntityType => SyncEntityTypes.Transfer;

    public string OperationType => SyncOperationTypes.Cancel;

    public async Task<SyncHandlerResult> HandleAsync(SyncPushOperationRequest operation, CancellationToken cancellationToken)
    {
        TransferCancelPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<TransferCancelPayload>(operation.PayloadJson, SyncJson.Options);
        }
        catch (JsonException)
        {
            return Fail(SyncPushOutcomes.ValidationFailed, ErrorCodes.ValidationFailed, "Invalid transfer cancel payload.");
        }

        if (payload is null)
        {
            return Fail(SyncPushOutcomes.ValidationFailed, ErrorCodes.ValidationFailed, "Cancel payload is required.");
        }

        var result = await transfers.CancelAsync(
            new CancelTransferCommand(operation.EntityId, payload.ExpectedRowVersion, operation.OperationId),
            cancellationToken);

        if (result.IsSuccess)
        {
            var json = JsonSerializer.Serialize(result.Value, SyncJson.Options);
            var outcome = result.Value!.WasIdempotentReplay
                ? SyncPushOutcomes.AlreadyProcessed
                : SyncPushOutcomes.Applied;
            return new SyncHandlerResult(
                outcome,
                null,
                null,
                json,
                ChangeLogOperation.Upsert,
                json);
        }

        var failOutcome = result.ErrorCode switch
        {
            ErrorCodes.SyncConflict => SyncPushOutcomes.Conflict,
            ErrorCodes.InsufficientStock => SyncPushOutcomes.Rejected,
            ErrorCodes.ValidationFailed => SyncPushOutcomes.ValidationFailed,
            _ => SyncPushOutcomes.Rejected,
        };
        return Fail(failOutcome, result.ErrorCode!, result.ErrorMessage!);
    }

    private static SyncHandlerResult Fail(string outcome, string code, string message) =>
        new(outcome, code, message, JsonSerializer.Serialize(new { code, message }, SyncJson.Options));

    private sealed record TransferCancelPayload(int ExpectedRowVersion);
}

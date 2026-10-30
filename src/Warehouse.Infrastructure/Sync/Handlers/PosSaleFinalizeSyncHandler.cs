using System.Text.Json;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Pos;
using Warehouse.Application.Sync;
using Warehouse.Domain.Sync;

namespace Warehouse.Infrastructure.Sync.Handlers;

public sealed class PosSaleFinalizeSyncHandler(IPosCheckoutService checkout) : ISyncOperationHandler
{
    public string EntityType => SyncEntityTypes.PosSale;

    public string OperationType => SyncOperationTypes.Finalize;

    public async Task<SyncHandlerResult> HandleAsync(
        SyncPushOperationRequest operation,
        CancellationToken cancellationToken)
    {
        PosSaleFinalizePayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<PosSaleFinalizePayload>(operation.PayloadJson, SyncJson.Options);
        }
        catch (JsonException)
        {
            return Fail(SyncPushOutcomes.ValidationFailed, ErrorCodes.ValidationFailed, "Invalid POS sale payload.");
        }

        if (payload is null || payload.Lines is null || payload.Lines.Count == 0)
        {
            return Fail(SyncPushOutcomes.ValidationFailed, ErrorCodes.ValidationFailed, "Sale lines are required.");
        }

        var result = await checkout.FinalizeCashSaleAsync(
            new FinalizePosCashSaleCommand(
                operation.OperationId,
                payload.WarehouseId,
                payload.LocationId,
                payload.DeviceId,
                payload.CustomerId,
                payload.CashShiftId,
                payload.CurrencyCode,
                payload.Notes,
                payload.PaymentMethod,
                payload.Lines
                    .Select(l => new PosSaleLineInput(l.ProductId, l.Quantity, l.UnitPrice, l.DiscountAmount))
                    .ToList()),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return MapFailure(result.ErrorCode!, result.ErrorMessage!);
        }

        var json = JsonSerializer.Serialize(result.Value, SyncJson.Options);
        var outcome = result.Value!.WasIdempotentReplay
            ? SyncPushOutcomes.AlreadyProcessed
            : SyncPushOutcomes.Applied;
        return new SyncHandlerResult(outcome, null, null, json, ChangeLogOperation.Upsert, json);
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

    private sealed record PosSaleFinalizePayload(
        Guid WarehouseId,
        Guid? LocationId,
        Guid? DeviceId,
        Guid? CustomerId,
        Guid? CashShiftId,
        string? CurrencyCode,
        string? Notes,
        PosPaymentMethodDto PaymentMethod,
        IReadOnlyList<PosSaleFinalizeLine> Lines);

    private sealed record PosSaleFinalizeLine(
        Guid ProductId,
        decimal Quantity,
        decimal? UnitPrice,
        decimal DiscountAmount);
}

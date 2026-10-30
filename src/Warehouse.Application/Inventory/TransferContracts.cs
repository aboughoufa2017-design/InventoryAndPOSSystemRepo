using System.Text.Json;
using Warehouse.Application.Abstractions;

namespace Warehouse.Application.Inventory;

public sealed record TransferLineRequest(Guid ProductId, decimal Quantity, string? Notes);

public sealed record TransferLineResponse(
    Guid Id,
    Guid ProductId,
    decimal QuantityRequested,
    decimal QuantityReceived,
    decimal QuantityInTransit,
    string? Notes);

public sealed record TransferResponse(
    Guid Id,
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId,
    Guid SourceLocationId,
    Guid DestinationLocationId,
    string Status,
    string? Notes,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? DispatchedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    int RowVersion,
    IReadOnlyList<TransferLineResponse> Lines,
    bool WasIdempotentReplay = false);

public sealed record CreateTransferCommand(
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId,
    Guid? SourceLocationId,
    Guid? DestinationLocationId,
    string? Notes,
    IReadOnlyList<TransferLineRequest> Lines);

public sealed record ReceiveTransferLineRequest(Guid TransferLineId, decimal Quantity);

public sealed record DispatchTransferCommand(Guid TransferId, int ExpectedRowVersion, Guid? OperationId);

public sealed record ReceiveTransferCommand(
    Guid TransferId,
    int ExpectedRowVersion,
    Guid? OperationId,
    IReadOnlyList<ReceiveTransferLineRequest> Lines);

public sealed record CancelTransferCommand(Guid TransferId, int ExpectedRowVersion, Guid? OperationId);

public interface ITransferService
{
    Task<AppResult<TransferResponse>> CreateAsync(CreateTransferCommand command, CancellationToken cancellationToken);

    Task<AppResult<TransferResponse>> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<TransferResponse>> ListAsync(string? status, CancellationToken cancellationToken);

    Task<AppResult<TransferResponse>> DispatchAsync(DispatchTransferCommand command, CancellationToken cancellationToken);

    Task<AppResult<TransferResponse>> ReceiveAsync(ReceiveTransferCommand command, CancellationToken cancellationToken);

    Task<AppResult<TransferResponse>> CancelAsync(CancelTransferCommand command, CancellationToken cancellationToken);
}

public static class TransferJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}

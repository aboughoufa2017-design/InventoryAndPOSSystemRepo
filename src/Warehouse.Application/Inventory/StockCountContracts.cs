using Warehouse.Application.Abstractions;

namespace Warehouse.Application.Inventory;

public sealed record StockCountLineResponse(
    Guid Id,
    Guid ProductId,
    decimal SystemQuantity,
    decimal? CountedQuantity,
    decimal Variance,
    string? Notes);

public sealed record StockCountResponse(
    Guid Id,
    Guid WarehouseId,
    Guid LocationId,
    string Status,
    string? Notes,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? PostedAtUtc,
    Guid? PostedAdjustmentId,
    int RowVersion,
    IReadOnlyList<StockCountLineResponse> Lines);

public sealed record CreateStockCountCommand(Guid WarehouseId, Guid? LocationId, string? Notes);

public sealed record SetStockCountLinesCommand(
    Guid StockCountId,
    int ExpectedRowVersion,
    IReadOnlyList<StockCountLineInput> Lines);

public sealed record StockCountLineInput(Guid ProductId, decimal CountedQuantity, string? Notes);

public sealed record PostStockCountCommand(Guid StockCountId, int ExpectedRowVersion);

public sealed record CancelStockCountCommand(Guid StockCountId, int ExpectedRowVersion);

public interface IStockCountService
{
    Task<IReadOnlyList<StockCountResponse>> ListAsync(CancellationToken cancellationToken);

    Task<AppResult<StockCountResponse>> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<AppResult<StockCountResponse>> CreateAsync(CreateStockCountCommand command, CancellationToken cancellationToken);

    Task<AppResult<StockCountResponse>> SetLinesAsync(SetStockCountLinesCommand command, CancellationToken cancellationToken);

    Task<AppResult<StockCountResponse>> PostAsync(PostStockCountCommand command, CancellationToken cancellationToken);

    Task<AppResult<StockCountResponse>> CancelAsync(CancelStockCountCommand command, CancellationToken cancellationToken);
}

using Warehouse.Application.Abstractions;

namespace Warehouse.Application.Inventory;

public sealed record StockBalanceResponse(
    Guid Id,
    Guid ProductId,
    Guid WarehouseId,
    Guid LocationId,
    decimal OnHand,
    decimal Reserved,
    decimal Available,
    int RowVersion);

public sealed record StockMovementResponse(
    Guid Id,
    Guid ProductId,
    Guid WarehouseId,
    Guid LocationId,
    decimal QuantityChange,
    string MovementType,
    string SourceDocumentType,
    Guid? SourceDocumentId,
    Guid? TransferLineId,
    string? Notes,
    DateTimeOffset OccurredAtUtc);

public sealed record StockAdjustmentLineRequest(Guid ProductId, Guid LocationId, decimal QuantityChange, string? Notes);

public sealed record StockAdjustmentLineResponse(
    Guid Id,
    Guid ProductId,
    Guid LocationId,
    decimal QuantityChange,
    string? Notes);

public sealed record StockAdjustmentResponse(
    Guid Id,
    Guid WarehouseId,
    string Kind,
    string Status,
    string? Reason,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? PostedAtUtc,
    int RowVersion,
    IReadOnlyList<StockAdjustmentLineResponse> Lines);

public sealed record CreateStockAdjustmentCommand(
    Guid WarehouseId,
    string Kind,
    string? Reason,
    IReadOnlyList<StockAdjustmentLineRequest> Lines);

public sealed record WarehouseResponse(
    Guid Id,
    string Code,
    string Name,
    bool IsActive,
    Guid GeneralLocationId,
    Guid InTransitLocationId);

public sealed record CreateWarehouseCommand(string Code, string Name);

public sealed record UpdateWarehouseCommand(Guid Id, string Name, bool IsActive);

public sealed record WarehouseLocationResponse(
    Guid Id,
    Guid WarehouseId,
    string Code,
    string Name,
    string Kind,
    bool IsDefault,
    bool IsActive);

public interface IWarehouseService
{
    Task<AppResult<WarehouseResponse>> CreateAsync(CreateWarehouseCommand command, CancellationToken cancellationToken);

    Task<AppResult<WarehouseResponse>> UpdateAsync(UpdateWarehouseCommand command, CancellationToken cancellationToken);

    Task<IReadOnlyList<WarehouseResponse>> ListAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<WarehouseLocationResponse>> ListLocationsAsync(Guid warehouseId, CancellationToken cancellationToken);
}

public interface IInventoryService
{
    Task<IReadOnlyList<StockBalanceResponse>> ListBalancesAsync(
        Guid? warehouseId,
        Guid? productId,
        CancellationToken cancellationToken);

    Task<AppResult<StockBalanceResponse>> GetBalanceAsync(
        Guid productId,
        Guid warehouseId,
        Guid locationId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<StockMovementResponse>> ListMovementsAsync(
        Guid? warehouseId,
        Guid? productId,
        int take,
        CancellationToken cancellationToken);

    Task<AppResult<StockAdjustmentResponse>> CreateAdjustmentAsync(
        CreateStockAdjustmentCommand command,
        CancellationToken cancellationToken);

    Task<AppResult<StockAdjustmentResponse>> GetAdjustmentAsync(Guid id, CancellationToken cancellationToken);

    Task<AppResult<StockAdjustmentResponse>> PostAdjustmentAsync(Guid id, CancellationToken cancellationToken);

    Task<AppResult<StockAdjustmentResponse>> CancelAdjustmentAsync(Guid id, CancellationToken cancellationToken);
}

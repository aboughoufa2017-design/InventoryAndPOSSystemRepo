using Warehouse.Application.Abstractions;

namespace Warehouse.Application.Reporting;

public sealed record StockOnHandRow(
    Guid ProductId,
    string Sku,
    string ProductName,
    Guid WarehouseId,
    string WarehouseCode,
    Guid LocationId,
    string LocationCode,
    decimal OnHand,
    decimal Reserved,
    decimal Available);

public sealed record MovementReportRow(
    Guid Id,
    DateTimeOffset OccurredAtUtc,
    string MovementType,
    Guid ProductId,
    Guid WarehouseId,
    Guid LocationId,
    decimal Quantity,
    string? ReferenceType,
    Guid? ReferenceId);

public sealed record DocumentSummaryRow(
    Guid Id,
    string DocumentType,
    string Status,
    decimal GrandTotal,
    DateTimeOffset CreatedAtUtc,
    string? Reference);

public interface IReportService
{
    Task<AppResult<IReadOnlyList<StockOnHandRow>>> StockOnHandAsync(
        Guid? warehouseId,
        CancellationToken cancellationToken);

    Task<AppResult<IReadOnlyList<MovementReportRow>>> MovementsAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        Guid? warehouseId,
        CancellationToken cancellationToken);

    Task<AppResult<IReadOnlyList<DocumentSummaryRow>>> PurchaseSummaryAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken);

    Task<AppResult<IReadOnlyList<DocumentSummaryRow>>> SalesSummaryAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken);
}

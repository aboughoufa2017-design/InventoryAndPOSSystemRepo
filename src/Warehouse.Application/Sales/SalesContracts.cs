using System.Text.Json;
using Warehouse.Application.Abstractions;

namespace Warehouse.Application.Sales;

public sealed record CustomerResponse(
    Guid Id,
    string Code,
    string Name,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Notes,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    int RowVersion);

public sealed record CreateCustomerCommand(
    string Code,
    string Name,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Notes);

public sealed record UpdateCustomerCommand(
    Guid CustomerId,
    string Name,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Notes,
    bool IsActive,
    int ExpectedRowVersion);

public sealed record SalesOrderLineRequest(
    Guid ProductId,
    decimal Quantity,
    decimal? UnitPrice,
    string? Notes);

public sealed record SalesOrderLineResponse(
    Guid Id,
    Guid ProductId,
    decimal QuantityOrdered,
    decimal QuantityShipped,
    decimal QuantityReturned,
    decimal QuantityRemainingToShip,
    decimal UnitPrice,
    decimal TaxRate,
    decimal LineNet,
    decimal LineTax,
    decimal LineTotal,
    string? Notes);

public sealed record SalesOrderResponse(
    Guid Id,
    Guid CustomerId,
    Guid WarehouseId,
    Guid LocationId,
    string Status,
    string PaymentStatus,
    string? Reference,
    string? Notes,
    decimal Subtotal,
    decimal TaxTotal,
    decimal GrandTotal,
    decimal PaidAmount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ConfirmedAtUtc,
    DateTimeOffset? ClosedAtUtc,
    int RowVersion,
    IReadOnlyList<SalesOrderLineResponse> Lines,
    bool WasIdempotentReplay = false);

public sealed record CreateSalesOrderCommand(
    Guid CustomerId,
    Guid WarehouseId,
    Guid? LocationId,
    string? Reference,
    string? Notes,
    IReadOnlyList<SalesOrderLineRequest> Lines);

public sealed record ConfirmSalesOrderCommand(Guid SalesOrderId, int ExpectedRowVersion, Guid? OperationId);

public sealed record ShipSalesLineRequest(Guid SalesOrderLineId, decimal Quantity);

public sealed record ShipSalesOrderCommand(
    Guid SalesOrderId,
    int ExpectedRowVersion,
    Guid? OperationId,
    Guid? WarehouseId,
    Guid? LocationId,
    string? Notes,
    IReadOnlyList<ShipSalesLineRequest> Lines);

public sealed record ReturnSalesLineRequest(Guid SalesOrderLineId, decimal Quantity);

public sealed record ReturnSalesOrderCommand(
    Guid SalesOrderId,
    int ExpectedRowVersion,
    Guid? OperationId,
    Guid? WarehouseId,
    Guid? LocationId,
    string? Notes,
    IReadOnlyList<ReturnSalesLineRequest> Lines);

public sealed record CloseSalesOrderCommand(Guid SalesOrderId, int ExpectedRowVersion, Guid? OperationId);

public sealed record CancelSalesOrderCommand(Guid SalesOrderId, int ExpectedRowVersion, Guid? OperationId);

public sealed record UpdateSalesPaymentCommand(
    Guid SalesOrderId,
    decimal PaidAmount,
    int ExpectedRowVersion,
    Guid? OperationId);

public interface ICustomerService
{
    Task<AppResult<CustomerResponse>> CreateAsync(CreateCustomerCommand command, CancellationToken cancellationToken);

    Task<AppResult<CustomerResponse>> UpdateAsync(UpdateCustomerCommand command, CancellationToken cancellationToken);

    Task<AppResult<CustomerResponse>> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<CustomerResponse>> ListAsync(CancellationToken cancellationToken);
}

public interface ISalesService
{
    Task<AppResult<SalesOrderResponse>> CreateAsync(CreateSalesOrderCommand command, CancellationToken cancellationToken);

    Task<AppResult<SalesOrderResponse>> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<SalesOrderResponse>> ListAsync(string? status, CancellationToken cancellationToken);

    Task<AppResult<SalesOrderResponse>> ConfirmAsync(ConfirmSalesOrderCommand command, CancellationToken cancellationToken);

    Task<AppResult<SalesOrderResponse>> ShipAsync(ShipSalesOrderCommand command, CancellationToken cancellationToken);

    Task<AppResult<SalesOrderResponse>> ReturnAsync(ReturnSalesOrderCommand command, CancellationToken cancellationToken);

    Task<AppResult<SalesOrderResponse>> CloseAsync(CloseSalesOrderCommand command, CancellationToken cancellationToken);

    Task<AppResult<SalesOrderResponse>> CancelAsync(CancelSalesOrderCommand command, CancellationToken cancellationToken);

    Task<AppResult<SalesOrderResponse>> UpdatePaymentAsync(UpdateSalesPaymentCommand command, CancellationToken cancellationToken);
}

public static class SalesJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}

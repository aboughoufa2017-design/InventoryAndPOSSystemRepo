using System.Text.Json;
using Warehouse.Application.Abstractions;

namespace Warehouse.Application.Purchasing;

public sealed record SupplierResponse(
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

public sealed record CreateSupplierCommand(
    string Code,
    string Name,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Notes);

public sealed record UpdateSupplierCommand(
    Guid SupplierId,
    string Name,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Notes,
    bool IsActive,
    int ExpectedRowVersion);

public sealed record PurchaseOrderLineRequest(
    Guid ProductId,
    decimal Quantity,
    decimal? UnitPrice,
    string? Notes);

public sealed record PurchaseOrderLineResponse(
    Guid Id,
    Guid ProductId,
    decimal QuantityOrdered,
    decimal QuantityReceived,
    decimal QuantityReturned,
    decimal QuantityRemainingToReceive,
    decimal UnitPrice,
    decimal TaxRate,
    decimal LineNet,
    decimal LineTax,
    decimal LineTotal,
    string? Notes);

public sealed record PurchaseOrderResponse(
    Guid Id,
    Guid SupplierId,
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
    IReadOnlyList<PurchaseOrderLineResponse> Lines,
    bool WasIdempotentReplay = false);

public sealed record CreatePurchaseOrderCommand(
    Guid SupplierId,
    Guid WarehouseId,
    Guid? LocationId,
    string? Reference,
    string? Notes,
    IReadOnlyList<PurchaseOrderLineRequest> Lines);

public sealed record ConfirmPurchaseOrderCommand(Guid PurchaseOrderId, int ExpectedRowVersion, Guid? OperationId);

public sealed record ReceivePurchaseLineRequest(Guid PurchaseOrderLineId, decimal Quantity);

public sealed record ReceivePurchaseOrderCommand(
    Guid PurchaseOrderId,
    int ExpectedRowVersion,
    Guid? OperationId,
    Guid? WarehouseId,
    Guid? LocationId,
    string? Notes,
    IReadOnlyList<ReceivePurchaseLineRequest> Lines);

public sealed record ReturnPurchaseLineRequest(Guid PurchaseOrderLineId, decimal Quantity);

public sealed record ReturnPurchaseOrderCommand(
    Guid PurchaseOrderId,
    int ExpectedRowVersion,
    Guid? OperationId,
    Guid? WarehouseId,
    Guid? LocationId,
    string? Notes,
    IReadOnlyList<ReturnPurchaseLineRequest> Lines);

public sealed record ClosePurchaseOrderCommand(Guid PurchaseOrderId, int ExpectedRowVersion, Guid? OperationId);

public sealed record CancelPurchaseOrderCommand(Guid PurchaseOrderId, int ExpectedRowVersion, Guid? OperationId);

public sealed record UpdatePurchasePaymentCommand(
    Guid PurchaseOrderId,
    decimal PaidAmount,
    int ExpectedRowVersion,
    Guid? OperationId);

public interface ISupplierService
{
    Task<AppResult<SupplierResponse>> CreateAsync(CreateSupplierCommand command, CancellationToken cancellationToken);

    Task<AppResult<SupplierResponse>> UpdateAsync(UpdateSupplierCommand command, CancellationToken cancellationToken);

    Task<AppResult<SupplierResponse>> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<SupplierResponse>> ListAsync(CancellationToken cancellationToken);
}

public interface IPurchasingService
{
    Task<AppResult<PurchaseOrderResponse>> CreateAsync(CreatePurchaseOrderCommand command, CancellationToken cancellationToken);

    Task<AppResult<PurchaseOrderResponse>> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<PurchaseOrderResponse>> ListAsync(string? status, CancellationToken cancellationToken);

    Task<AppResult<PurchaseOrderResponse>> ConfirmAsync(ConfirmPurchaseOrderCommand command, CancellationToken cancellationToken);

    Task<AppResult<PurchaseOrderResponse>> ReceiveAsync(ReceivePurchaseOrderCommand command, CancellationToken cancellationToken);

    Task<AppResult<PurchaseOrderResponse>> ReturnAsync(ReturnPurchaseOrderCommand command, CancellationToken cancellationToken);

    Task<AppResult<PurchaseOrderResponse>> CloseAsync(ClosePurchaseOrderCommand command, CancellationToken cancellationToken);

    Task<AppResult<PurchaseOrderResponse>> CancelAsync(CancelPurchaseOrderCommand command, CancellationToken cancellationToken);

    Task<AppResult<PurchaseOrderResponse>> UpdatePaymentAsync(UpdatePurchasePaymentCommand command, CancellationToken cancellationToken);
}

public static class PurchasingJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}

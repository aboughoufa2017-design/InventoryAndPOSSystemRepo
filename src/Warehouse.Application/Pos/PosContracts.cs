namespace Warehouse.Application.Pos;

public sealed record PosSaleLineInput(
    Guid ProductId,
    decimal Quantity,
    decimal? UnitPrice,
    decimal DiscountAmount);

public sealed record FinalizePosCashSaleCommand(
    Guid ClientCommandId,
    Guid WarehouseId,
    Guid? LocationId,
    Guid? DeviceId,
    Guid? CustomerId,
    Guid? CashShiftId,
    string? CurrencyCode,
    string? Notes,
    PosPaymentMethodDto PaymentMethod,
    IReadOnlyList<PosSaleLineInput> Lines);

public enum PosPaymentMethodDto
{
    Cash = 0,
    Credit = 1,
}

public sealed record VoidPosSaleCommand(Guid PosSaleId, Guid? ClientCommandId);

public sealed record RefundPosSaleCommand(
    Guid PosSaleId,
    Guid ClientCommandId,
    IReadOnlyList<PosRefundLineInput> Lines);

public sealed record PosRefundLineInput(Guid PosSaleLineId, decimal Quantity);

public sealed record PosSaleLineResponse(
    Guid Id,
    Guid ProductId,
    string SkuSnapshot,
    string NameSnapshot,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal LineTotal);

public sealed record PosPaymentResponse(
    Guid Id,
    string Method,
    decimal Amount,
    string Status,
    string? Reference);

public sealed record PosSaleResponse(
    Guid Id,
    Guid WarehouseId,
    Guid LocationId,
    Guid? DeviceId,
    Guid? CustomerId,
    Guid? CashShiftId,
    Guid ClientCommandId,
    string ReceiptNumber,
    string Status,
    string CurrencyCode,
    decimal Subtotal,
    decimal DiscountTotal,
    decimal TaxTotal,
    decimal GrandTotal,
    DateTimeOffset SoldAtUtc,
    string? Notes,
    int RowVersion,
    bool WasIdempotentReplay,
    IReadOnlyList<PosSaleLineResponse> Lines,
    IReadOnlyList<PosPaymentResponse> Payments);

public sealed record OpenCashShiftCommand(Guid WarehouseId, Guid? DeviceId, decimal OpeningFloat);

public sealed record CloseCashShiftCommand(Guid CashShiftId, decimal CountedCash);

public sealed record CashShiftResponse(
    Guid Id,
    Guid WarehouseId,
    Guid? DeviceId,
    Guid CashierUserId,
    string Status,
    decimal OpeningFloat,
    decimal? ClosingCountedCash,
    DateTimeOffset OpenedAtUtc,
    DateTimeOffset? ClosedAtUtc,
    int RowVersion);

public sealed record PosConflictResponse(
    Guid Id,
    Guid WarehouseId,
    Guid ProductId,
    Guid? PosSaleId,
    Guid? SourceCommandId,
    decimal ResultingOnHand,
    string Message,
    string Status,
    DateTimeOffset CreatedAtUtc);

public sealed record CreatePriceListCommand(string Code, string Name, string? CurrencyCode);

public sealed record SetProductPriceCommand(
    Guid PriceListId,
    Guid ProductId,
    decimal UnitPrice,
    decimal? MinQuantity);

public sealed record PriceListResponse(Guid Id, string Code, string Name, string CurrencyCode, bool IsActive);

public sealed record CreateRestaurantTableCommand(Guid WarehouseId, string Code, string Name);

public sealed record OpenRestaurantOrderCommand(Guid WarehouseId, Guid TableId, string? Notes);

public sealed record RestaurantTableResponse(Guid Id, Guid WarehouseId, string Code, string Name, bool IsActive);

public sealed record RestaurantOrderResponse(
    Guid Id,
    Guid WarehouseId,
    Guid TableId,
    string Status,
    string? Notes,
    DateTimeOffset OpenedAtUtc,
    Guid? PosSaleId);

public interface IPosCheckoutService
{
    Task<Abstractions.AppResult<PosSaleResponse>> FinalizeCashSaleAsync(
        FinalizePosCashSaleCommand command,
        CancellationToken cancellationToken);

    Task<Abstractions.AppResult<PosSaleResponse>> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<PosSaleResponse>> ListAsync(
        Guid? warehouseId,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        CancellationToken cancellationToken);

    Task<Abstractions.AppResult<PosSaleResponse>> VoidAsync(
        VoidPosSaleCommand command,
        CancellationToken cancellationToken);

    Task<Abstractions.AppResult<PosSaleResponse>> RefundAsync(
        RefundPosSaleCommand command,
        CancellationToken cancellationToken);
}

public interface ICashShiftService
{
    Task<Abstractions.AppResult<CashShiftResponse>> OpenAsync(
        OpenCashShiftCommand command,
        CancellationToken cancellationToken);

    Task<Abstractions.AppResult<CashShiftResponse>> CloseAsync(
        CloseCashShiftCommand command,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<CashShiftResponse>> ListAsync(Guid? warehouseId, CancellationToken cancellationToken);
}

public interface IPosConflictService
{
    Task<IReadOnlyList<PosConflictResponse>> ListOpenAsync(CancellationToken cancellationToken);

    Task<Abstractions.AppResult> ResolveAsync(Guid conflictId, CancellationToken cancellationToken);
}

public interface IPosPricingService
{
    Task<Abstractions.AppResult<PriceListResponse>> CreatePriceListAsync(
        CreatePriceListCommand command,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<PriceListResponse>> ListPriceListsAsync(CancellationToken cancellationToken);

    Task<Abstractions.AppResult> SetProductPriceAsync(
        SetProductPriceCommand command,
        CancellationToken cancellationToken);

    Task<decimal?> ResolveUnitPriceAsync(
        Guid productId,
        Guid? priceListId,
        decimal quantity,
        CancellationToken cancellationToken);
}

public interface IRestaurantPosService
{
    Task<Abstractions.AppResult<RestaurantTableResponse>> CreateTableAsync(
        CreateRestaurantTableCommand command,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<RestaurantTableResponse>> ListTablesAsync(Guid warehouseId, CancellationToken cancellationToken);

    Task<Abstractions.AppResult<RestaurantOrderResponse>> OpenOrderAsync(
        OpenRestaurantOrderCommand command,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<RestaurantOrderResponse>> ListOpenOrdersAsync(
        Guid warehouseId,
        CancellationToken cancellationToken);

    Task<Abstractions.AppResult<RestaurantOrderResponse>> CancelOrderAsync(
        Guid orderId,
        CancellationToken cancellationToken);
}

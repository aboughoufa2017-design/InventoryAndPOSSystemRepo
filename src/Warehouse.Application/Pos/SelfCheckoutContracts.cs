using Warehouse.Application.Abstractions;

namespace Warehouse.Application.Pos;

public sealed record StartSelfCheckoutCommand(
    string TenantCode,
    Guid? WarehouseId,
    bool IsGuest);

public sealed record SelfCheckoutLineResponse(
    Guid Id,
    Guid ProductId,
    string Sku,
    string Name,
    string? Barcode,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public sealed record SelfCheckoutSessionResponse(
    Guid Id,
    Guid WarehouseId,
    string Code,
    string AccessToken,
    bool IsGuest,
    string Status,
    string CurrencyCode,
    decimal Subtotal,
    decimal TaxTotal,
    decimal GrandTotal,
    bool AuditRequired,
    int AuditItemCount,
    IReadOnlyList<string> AuditTargetBarcodes,
    bool AuditCompleted,
    DateTimeOffset ExpiresAtUtc,
    string QrPayload,
    IReadOnlyList<SelfCheckoutLineResponse> Lines);

public sealed record SelfCheckoutTransferResponse(
    Guid SessionId,
    string Code,
    string QrPayload,
    bool AuditRequired,
    int AuditItemCount,
    string Status,
    decimal GrandTotal,
    string CurrencyCode);

public sealed record SelfCheckoutWaitingItemResponse(
    Guid SessionId,
    string Code,
    string Status,
    decimal GrandTotal,
    string CurrencyCode,
    int LineCount,
    DateTimeOffset UpdatedAtUtc,
    bool AuditRequired,
    bool AuditCompleted);

public interface ISelfCheckoutService
{
    Task<AppResult<SelfCheckoutSessionResponse>> StartAsync(
        StartSelfCheckoutCommand command,
        CancellationToken cancellationToken);

    Task<AppResult<SelfCheckoutSessionResponse>> GetByAccessTokenAsync(
        string accessToken,
        CancellationToken cancellationToken);

    Task<AppResult<SelfCheckoutSessionResponse>> AddLineByBarcodeAsync(
        string accessToken,
        string barcode,
        decimal quantity,
        CancellationToken cancellationToken);

    Task<AppResult<SelfCheckoutSessionResponse>> SetLineQuantityAsync(
        string accessToken,
        Guid lineId,
        decimal quantity,
        CancellationToken cancellationToken);

    Task<AppResult<SelfCheckoutSessionResponse>> RemoveLineAsync(
        string accessToken,
        Guid lineId,
        CancellationToken cancellationToken);

    Task<AppResult<SelfCheckoutTransferResponse>> TransferAsync(
        string accessToken,
        CancellationToken cancellationToken);

    Task<AppResult<SelfCheckoutSessionResponse>> ReopenAsync(
        string accessToken,
        CancellationToken cancellationToken);

    Task<AppResult<IReadOnlyList<SelfCheckoutWaitingItemResponse>>> ListWaitingAsync(
        Guid warehouseId,
        CancellationToken cancellationToken);

    Task<AppResult<SelfCheckoutSessionResponse>> ImportByCodeAsync(
        string codeOrQrPayload,
        CancellationToken cancellationToken);

    Task<AppResult<SelfCheckoutSessionResponse>> StartAuditAsync(
        Guid sessionId,
        CancellationToken cancellationToken);

    Task<AppResult<SelfCheckoutSessionResponse>> CompleteAuditAsync(
        Guid sessionId,
        IReadOnlyList<string> scannedBarcodes,
        CancellationToken cancellationToken);

    Task<AppResult<SelfCheckoutSessionResponse>> MarkClosedAsync(
        Guid sessionId,
        CancellationToken cancellationToken);
}

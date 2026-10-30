namespace Warehouse.Shared.Api;

public static class ApiHeaders
{
    public const string CorrelationId = "X-Correlation-ID";
    public const string IdempotencyKey = "Idempotency-Key";
}

public static class ApiErrorCodes
{
    public const string ProductNotFound = "PRODUCT_NOT_FOUND";
    public const string InsufficientStock = "INSUFFICIENT_STOCK";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string TenantNotFound = "TENANT_NOT_FOUND";
    public const string InvalidOperation = "INVALID_OPERATION";
    public const string SyncConflict = "SYNC_CONFLICT";
    public const string OperationAlreadyProcessed = "OPERATION_ALREADY_PROCESSED";
    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string InternalError = "INTERNAL_ERROR";
    public const string FeatureNotEntitled = "FEATURE_NOT_ENTITLED";
}

public sealed record ApiErrorResponse(
    string Code,
    string Message,
    string CorrelationId,
    IReadOnlyList<ApiFieldError>? Errors = null);

public sealed record ApiFieldError(string Field, string Message);

public sealed record LiveHealthResponse(string Status);

public sealed record ReadyHealthResponse(
    string Status,
    IReadOnlyDictionary<string, string> Checks);

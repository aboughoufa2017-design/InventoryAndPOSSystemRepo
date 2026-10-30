namespace Warehouse.Application.Abstractions;

/// <summary>
/// Application-layer error codes. Values match <c>Warehouse.Shared.Api.ApiErrorCodes</c>.
/// </summary>
public static class ErrorCodes
{
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string TenantNotFound = "TENANT_NOT_FOUND";
    public const string InvalidOperation = "INVALID_OPERATION";
    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string InternalError = "INTERNAL_ERROR";
    public const string ProductNotFound = "PRODUCT_NOT_FOUND";
    public const string SyncConflict = "SYNC_CONFLICT";
    public const string InsufficientStock = "INSUFFICIENT_STOCK";
    public const string OperationAlreadyProcessed = "OPERATION_ALREADY_PROCESSED";
    public const string FeatureNotEntitled = "FEATURE_NOT_ENTITLED";
}

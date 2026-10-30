namespace Warehouse.Application.Abstractions;

public sealed class AppResult<T>
{
    private AppResult(bool isSuccess, T? value, string? errorCode, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public T? Value { get; }

    public string? ErrorCode { get; }

    public string? ErrorMessage { get; }

    public static AppResult<T> Success(T value) => new(true, value, null, null);

    public static AppResult<T> Failure(string errorCode, string errorMessage) =>
        new(false, default, errorCode, errorMessage);
}

public sealed class AppResult
{
    private AppResult(bool isSuccess, string? errorCode, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public string? ErrorCode { get; }

    public string? ErrorMessage { get; }

    public static AppResult Success() => new(true, null, null);

    public static AppResult Failure(string errorCode, string errorMessage) =>
        new(false, errorCode, errorMessage);
}

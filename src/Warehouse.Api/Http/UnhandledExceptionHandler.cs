using Microsoft.AspNetCore.Diagnostics;
using Warehouse.Shared.Api;

namespace Warehouse.Api.Http;

public sealed class UnhandledExceptionHandler(ILogger<UnhandledExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var correlationId = httpContext.Items[CorrelationIdMiddleware.ItemKey] as string ?? "unknown";
        logger.LogError(exception, "Unhandled exception. CorrelationId: {CorrelationId}", correlationId);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(
            new ApiErrorResponse(ApiErrorCodes.InternalError, "An unexpected error occurred.", correlationId),
            cancellationToken);

        return true;
    }
}

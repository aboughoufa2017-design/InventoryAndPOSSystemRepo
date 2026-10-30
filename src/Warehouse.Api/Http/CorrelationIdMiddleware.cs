using System.Diagnostics;
using Warehouse.Shared.Api;

namespace Warehouse.Api.Http;

public sealed class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    public const string ItemKey = "CorrelationId";

    public async Task InvokeAsync(HttpContext context)
    {
        var incoming = context.Request.Headers[ApiHeaders.CorrelationId].ToString();
        var correlationId = IsAcceptable(incoming) ? incoming : Guid.NewGuid().ToString("D");

        context.Items[ItemKey] = correlationId;
        context.Response.Headers[ApiHeaders.CorrelationId] = correlationId;
        Activity.Current?.SetTag("correlation.id", correlationId);

        using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await next(context);
        }
    }

    private static bool IsAcceptable(string value)
    {
        if (value.Length is < 1 or > 64)
        {
            return false;
        }

        foreach (var character in value)
        {
            var allowed = character is (>= 'a' and <= 'z')
                or (>= 'A' and <= 'Z')
                or (>= '0' and <= '9')
                or '-'
                or '_';
            if (!allowed)
            {
                return false;
            }
        }

        return true;
    }
}

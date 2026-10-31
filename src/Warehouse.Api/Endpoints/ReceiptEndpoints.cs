using Warehouse.Application.Messaging;
using Warehouse.Api.Authorization;
using Warehouse.Api.Http;
using Warehouse.Shared.Api;

namespace Warehouse.Api.Endpoints;

public static class ReceiptEndpoints
{
    public static IEndpointRouteBuilder MapReceiptEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/receipts")
            .WithTags("Receipts")
            .RequireAuthorization();

        group.MapPost("/send-email", SendEmailAsync)
            .RequirePermission("pos.sale")
            .WithName("SendReceiptEmail")
            .Produces(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapPost("/send-sms", SendSmsAsync)
            .RequirePermission("pos.sale")
            .WithName("SendReceiptSms")
            .Produces(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapPost("/send", SendBothAsync)
            .RequirePermission("pos.sale")
            .WithName("SendReceiptEmailAndSms")
            .Produces(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);

        return endpoints;
    }

    private static async Task<IResult> SendEmailAsync(
        SendReceiptRequest request,
        IReceiptNotificationService notifications,
        ILoggerFactory loggerFactory,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("ReceiptEndpoints");
        if (string.IsNullOrWhiteSpace(request.CustomerEmail))
        {
            return BadRequest(http, "customerEmail is required.");
        }

        try
        {
            await notifications.SendEmailAsync(
                request.CustomerEmail,
                ToDetails(request),
                cancellationToken);
            return Results.Ok(new { channel = "email", sent = true });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Receipt email failed for order {OrderId}", request.OrderId);
            return BadRequest(http, ex.Message);
        }
    }

    private static async Task<IResult> SendSmsAsync(
        SendReceiptRequest request,
        IReceiptNotificationService notifications,
        ILoggerFactory loggerFactory,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("ReceiptEndpoints");
        if (string.IsNullOrWhiteSpace(request.CustomerPhone))
        {
            return BadRequest(http, "customerPhone is required.");
        }

        try
        {
            await notifications.SendSmsAsync(
                request.CustomerPhone,
                ToDetails(request),
                cancellationToken);
            return Results.Ok(new { channel = "sms", sent = true });
        }
        catch (ResalaApiException ex)
        {
            logger.LogWarning(ex, "Receipt SMS Resala type={Type} order={OrderId}", ex.Type, request.OrderId);
            return Results.Json(
                new ApiErrorResponse(ex.Type, ex.Message, CorrelationId(http)),
                statusCode: StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Receipt SMS failed for order {OrderId}", request.OrderId);
            return BadRequest(http, ex.Message);
        }
    }

    private static async Task<IResult> SendBothAsync(
        SendReceiptRequest request,
        IReceiptNotificationService notifications,
        ILoggerFactory loggerFactory,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("ReceiptEndpoints");
        // Request CT is intentionally unused: email/SMS use independent timeouts so one channel
        // cannot cancel the other when SMTP hangs past the HTTP client/proxy limit.
        _ = cancellationToken;

        var emailRequested = !string.IsNullOrWhiteSpace(request.CustomerEmail);
        var smsRequested = !string.IsNullOrWhiteSpace(request.CustomerPhone);
        if (!emailRequested && !smsRequested)
        {
            return BadRequest(http, "Provide customerEmail and/or customerPhone.");
        }

        var details = ToDetails(request);
        string? emailError = null;
        string? smsError = null;
        var emailSent = false;
        var smsSent = false;

        // Independent per-channel timeouts so a hung SMTP connect does not cancel SMS (and vice versa).
        // Do not link to the HTTP request token — sibling channels must not share cancellation.
        Task? emailTask = null;
        Task? smsTask = null;

        if (emailRequested)
        {
            emailTask = SendEmailChannelAsync();
        }

        if (smsRequested)
        {
            smsTask = SendSmsChannelAsync();
        }

        if (emailTask is not null && smsTask is not null)
        {
            await Task.WhenAll(emailTask, smsTask);
        }
        else if (emailTask is not null)
        {
            await emailTask;
        }
        else if (smsTask is not null)
        {
            await smsTask;
        }

        if (!emailSent && !smsSent)
        {
            return BadRequest(http, string.Join(" | ", new[] { emailError, smsError }.Where(e => e is not null)));
        }

        return Results.Ok(new
        {
            emailSent,
            smsSent,
            emailError,
            smsError,
        });

        async Task SendEmailChannelAsync()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            try
            {
                await notifications.SendEmailAsync(request.CustomerEmail!, details, cts.Token);
                emailSent = true;
            }
            catch (OperationCanceledException ex)
            {
                logger.LogWarning(ex, "Receipt email timed out for order {OrderId}", request.OrderId);
                emailError = "SMTP connect/send timed out. Check SMTP_* settings or host outbound SMTP (Render often blocks SMTP).";
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Receipt email failed for order {OrderId}", request.OrderId);
                emailError = ex.Message;
            }
        }

        async Task SendSmsChannelAsync()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(25));
            try
            {
                await notifications.SendSmsAsync(request.CustomerPhone!, details, cts.Token);
                smsSent = true;
            }
            catch (ResalaApiException ex)
            {
                logger.LogWarning(ex, "Receipt SMS Resala type={Type} order={OrderId}", ex.Type, request.OrderId);
                smsError = $"{ex.Type}: {ex.Message}";
            }
            catch (OperationCanceledException ex)
            {
                logger.LogWarning(ex, "Receipt SMS timed out for order {OrderId}", request.OrderId);
                smsError = "SMS send timed out waiting for Resala.";
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Receipt SMS failed for order {OrderId}", request.OrderId);
                smsError = ex.Message;
            }
        }
    }

    private static OrderReceiptDetails ToDetails(SendReceiptRequest request)
    {
        var lines = (request.Lines ?? [])
            .Select(l => new ReceiptLineDto(
                l.Name ?? string.Empty,
                l.Sku ?? string.Empty,
                l.Quantity,
                l.UnitPrice))
            .ToList();

        return new OrderReceiptDetails(
            request.OrderId,
            request.OrderCode,
            string.IsNullOrWhiteSpace(request.CurrencyCode) ? "LYD" : request.CurrencyCode,
            request.GrandTotal,
            request.CreatedAtUtc == default ? DateTimeOffset.UtcNow : request.CreatedAtUtc,
            lines);
    }

    private static IResult BadRequest(HttpContext http, string message) =>
        Results.Json(
            new ApiErrorResponse(ApiErrorCodes.ValidationFailed, message, CorrelationId(http)),
            statusCode: StatusCodes.Status400BadRequest);

    private static string CorrelationId(HttpContext http) =>
        http.Items[CorrelationIdMiddleware.ItemKey] as string ?? "unknown";

    public sealed record SendReceiptRequest(
        string OrderId,
        string? OrderCode,
        string? CustomerEmail,
        string? CustomerPhone,
        string? CurrencyCode,
        decimal GrandTotal,
        DateTimeOffset CreatedAtUtc,
        IReadOnlyList<SendReceiptLineRequest>? Lines);

    public sealed record SendReceiptLineRequest(
        string? Name,
        string? Sku,
        decimal Quantity,
        decimal UnitPrice);
}

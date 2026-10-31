using System.Globalization;
using System.Net;
using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Warehouse.Application.Messaging;

namespace Warehouse.Infrastructure.Messaging;

public sealed class EmailService(
    IOptions<SmtpSettings> settings,
    ILogger<EmailService> logger) : IEmailService
{
    private readonly SmtpSettings _settings = settings.Value;

    public async Task SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        EnsureConfigured();

        if (string.IsNullOrWhiteSpace(message.To))
        {
            throw new ArgumentException("Recipient email is required.", nameof(message));
        }

        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        email.To.Add(MailboxAddress.Parse(message.To.Trim()));
        email.Subject = message.Subject;

        var builder = new BodyBuilder();
        if (message.IsHtml)
        {
            builder.HtmlBody = message.Body;
        }
        else
        {
            builder.TextBody = message.Body;
        }

        if (message.Attachments is { Count: > 0 })
        {
            foreach (var att in message.Attachments)
            {
                builder.Attachments.Add(
                    att.FileName,
                    att.Content,
                    ContentType.Parse(att.ContentType));
            }
        }

        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        // Cap connect/auth/send so a blocked outbound SMTP port does not hang the request indefinitely.
        smtp.Timeout = 12_000;
        var socketOptions = _settings.EnableSsl
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.None;

        try
        {
            await smtp.ConnectAsync(_settings.Server, _settings.Port, socketOptions, cancellationToken)
                .ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(_settings.Username))
            {
                await smtp.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken)
                    .ConfigureAwait(false);
            }

            await smtp.SendAsync(email, cancellationToken).ConfigureAwait(false);
            await smtp.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException(
                $"SMTP timed out connecting/sending via {_settings.Server}:{_settings.Port}. " +
                "Host outbound SMTP may be blocked (common on Render). Prefer smtp.gmail.com:587 with an app password.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (TimeoutException)
        {
            throw new TimeoutException(
                $"SMTP timed out connecting/sending via {_settings.Server}:{_settings.Port}. " +
                "Host outbound SMTP may be blocked (common on Render). Prefer smtp.gmail.com:587 with an app password.");
        }
        catch (Exception ex) when (ex is IOException or SmtpCommandException or SmtpProtocolException)
        {
            throw new InvalidOperationException(
                $"SMTP unreachable or rejected at {_settings.Server}:{_settings.Port}: {ex.Message}",
                ex);
        }

        logger.LogInformation("SMTP email sent to {To} subject {Subject}", message.To, message.Subject);
    }

    public async Task SendReceiptEmailAsync(
        string toEmail,
        string orderId,
        string? pdfReceiptPath = null,
        CancellationToken cancellationToken = default)
    {
        var safeOrderId = WebUtility.HtmlEncode(orderId);
        var htmlBody = $"""
            <div style='font-family: Arial, sans-serif; color: #0F172A;'>
              <h2 style='color: #0A42A0;'>Modern Tech POS - Payment Receipt</h2>
              <p>Thank you for shopping with us!</p>
              <p><strong>Order ID:</strong> {safeOrderId}</p>
              <p>Attached to this email is your digital receipt.</p>
              <hr style='border: 1px solid #E2E8F0;' />
              <p style='font-size: 12px; color: #64748B;'>Modern Tech Retail Systems</p>
            </div>
            """;

        var emailMessage = new EmailMessage
        {
            To = toEmail,
            Subject = $"Receipt for Order #{orderId} - Modern Tech",
            Body = htmlBody,
            IsHtml = true,
        };

        if (!string.IsNullOrWhiteSpace(pdfReceiptPath) && File.Exists(pdfReceiptPath))
        {
            var fileBytes = await File.ReadAllBytesAsync(pdfReceiptPath, cancellationToken)
                .ConfigureAwait(false);
            emailMessage.Attachments =
            [
                new EmailAttachment
                {
                    FileName = $"Receipt_{orderId}.pdf",
                    Content = fileBytes,
                    ContentType = "application/pdf",
                },
            ];
        }

        await SendEmailAsync(emailMessage, cancellationToken).ConfigureAwait(false);
    }

    public Task SendOrderDetailsEmailAsync(
        string toEmail,
        OrderReceiptDetails details,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(details);
        var culture = CultureInfo.InvariantCulture;
        var rows = new StringBuilder();
        foreach (var line in details.Lines)
        {
            var lineTotal = line.Quantity * line.UnitPrice;
            rows.Append("<tr>")
                .Append("<td style='padding:6px;border-bottom:1px solid #E2E8F0;'>")
                .Append(WebUtility.HtmlEncode(line.Name))
                .Append("</td>")
                .Append("<td style='padding:6px;border-bottom:1px solid #E2E8F0;'>")
                .Append(WebUtility.HtmlEncode(line.Sku))
                .Append("</td>")
                .Append("<td style='padding:6px;border-bottom:1px solid #E2E8F0;text-align:right;'>")
                .Append(line.Quantity.ToString("0.###", culture))
                .Append("</td>")
                .Append("<td style='padding:6px;border-bottom:1px solid #E2E8F0;text-align:right;'>")
                .Append(lineTotal.ToString("0.00", culture))
                .Append("</td>")
                .Append("</tr>");
        }

        var orderCode = string.IsNullOrWhiteSpace(details.OrderCode)
            ? ""
            : $"<p><strong>Order code:</strong> {WebUtility.HtmlEncode(details.OrderCode)}</p>";

        var html = $"""
            <div style='font-family: Arial, sans-serif; color: #0F172A;'>
              <h2 style='color: #0A42A0;'>Modern Tech — إيصال / Receipt</h2>
              <p><strong>Order ID:</strong> {WebUtility.HtmlEncode(details.OrderId)}</p>
              {orderCode}
              <p><strong>Date:</strong> {details.CreatedAtUtc.UtcDateTime:yyyy-MM-dd HH:mm} UTC</p>
              <table style='width:100%;border-collapse:collapse;margin-top:12px;'>
                <thead>
                  <tr>
                    <th style='text-align:left;padding:6px;border-bottom:2px solid #0A42A0;'>Item</th>
                    <th style='text-align:left;padding:6px;border-bottom:2px solid #0A42A0;'>SKU</th>
                    <th style='text-align:right;padding:6px;border-bottom:2px solid #0A42A0;'>Qty</th>
                    <th style='text-align:right;padding:6px;border-bottom:2px solid #0A42A0;'>Total</th>
                  </tr>
                </thead>
                <tbody>{rows}</tbody>
              </table>
              <p style='margin-top:16px;font-size:18px;font-weight:700;'>
                Total: {details.GrandTotal.ToString("0.00", culture)} {WebUtility.HtmlEncode(details.CurrencyCode)}
              </p>
              <hr style='border: 1px solid #E2E8F0;' />
              <p style='font-size: 12px; color: #64748B;'>Modern Tech Retail Systems</p>
            </div>
            """;

        return SendEmailAsync(
            new EmailMessage
            {
                To = toEmail,
                Subject = $"Receipt #{details.OrderId} — Modern Tech",
                Body = html,
                IsHtml = true,
            },
            cancellationToken);
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_settings.Server)
            || string.IsNullOrWhiteSpace(_settings.SenderEmail)
            || _settings.Port <= 0)
        {
            throw new InvalidOperationException(
                "SMTP is not configured. Set SmtpSettings (Server, Port, SenderEmail) or SMTP_* environment variables.");
        }
    }
}

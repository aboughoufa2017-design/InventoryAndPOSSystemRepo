using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Warehouse.Application.Messaging;

namespace Warehouse.Infrastructure.Messaging;

public sealed class ReceiptNotificationService(
    IEmailService email,
    IResalaSmsClient resala,
    IOptions<ResalaOptions> resalaOptions,
    ILogger<ReceiptNotificationService> logger) : IReceiptNotificationService
{
    public Task SendEmailAsync(
        string toEmail,
        OrderReceiptDetails details,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            throw new ArgumentException("Customer email is required.", nameof(toEmail));
        }

        return email.SendOrderDetailsEmailAsync(toEmail.Trim(), details, cancellationToken);
    }

    public async Task SendSmsAsync(
        string phone,
        OrderReceiptDetails details,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            throw new ArgumentException("Customer phone is required.", nameof(phone));
        }

        var normalized = ResalaPhone.NormalizeLibyanMobile(phone)
                         ?? throw new ArgumentException(
                             "Phone must be a Libyan mobile (091/092/093/094).",
                             nameof(phone));

        var opts = resalaOptions.Value;
        if (string.IsNullOrWhiteSpace(opts.ReceiptSmsTemplateId))
        {
            throw new InvalidOperationException(
                "Resala receipt SMS template is not configured (Resala:ReceiptSmsTemplateId / RESALA_RECEIPT_SMS_TEMPLATE_ID).");
        }

        var culture = CultureInfo.InvariantCulture;
        // Must match the approved Resala template placeholders $1..$4.
        var record = new Dictionary<string, object?>
        {
            ["phone"] = normalized,
            ["$1"] = !string.IsNullOrWhiteSpace(details.OrderCode)
                ? details.OrderCode!
                : details.OrderId,
            ["$2"] = $"{details.GrandTotal.ToString("0.00", culture)} {details.CurrencyCode}",
            ["$3"] = BuildLinesPlaceholder(details, culture),
            ["$4"] = details.CreatedAtUtc.UtcDateTime.ToString("yyyy-MM-dd HH:mm", culture) + " UTC",
        };

        try
        {
            var result = await resala.SendTemplateAsync(
                new SendTemplateRequest { Records = [record] },
                new SendTemplateQuery(
                    SmsTemplateId: opts.ReceiptSmsTemplateId!,
                    Sender: opts.DefaultSender),
                cancellationToken).ConfigureAwait(false);

            logger.LogInformation(
                "Resala receipt SMS succeeded={Succeeded} failed={Failed} invalid={InvalidCount} order={OrderId}",
                result.Succeeded,
                result.Failed,
                result.InvalidNumbers.Count,
                details.OrderId);

            if (result.Succeeded <= 0)
            {
                var invalid = string.Join(", ", result.InvalidNumbers);
                throw new InvalidOperationException(
                    result.InvalidNumbers.Count > 0
                        ? $"SMS not sent; invalid numbers: {invalid}"
                        : "SMS not sent; Resala reported zero succeeded messages.");
            }
        }
        catch (ResalaApiException ex)
        {
            // Do not retry — surface type for the API layer.
            logger.LogWarning(
                ex,
                "Resala receipt SMS failed type={Type} status={Status} order={OrderId}",
                ex.Type,
                ex.StatusCode,
                details.OrderId);
            throw;
        }
    }

    /// <summary>Line items for template <c>$3</c> (name×qty=lineTotal), truncated for SMS length.</summary>
    private static string BuildLinesPlaceholder(OrderReceiptDetails details, CultureInfo culture)
    {
        if (details.Lines.Count == 0)
        {
            return "-";
        }

        var sb = new StringBuilder();
        var written = 0;
        foreach (var line in details.Lines)
        {
            var lineTotal = (line.Quantity * line.UnitPrice).ToString("0.00", culture);
            var name = line.Name.Length <= 40 ? line.Name : line.Name[..37] + "…";
            var piece = $"{name}×{line.Quantity.ToString("0.###", culture)}={lineTotal}";

            var separatorLen = written > 0 ? 3 : 0;
            // Arabic SMS is UCS-2 (~70 chars/segment); keep $3 bounded.
            if (sb.Length + separatorLen + piece.Length > 280)
            {
                var remaining = details.Lines.Count - written;
                if (remaining > 0)
                {
                    var suffix = $" | +{remaining}";
                    if (sb.Length + suffix.Length <= 300)
                    {
                        sb.Append(suffix);
                    }
                    else
                    {
                        sb.Append('…');
                    }
                }

                break;
            }

            if (written > 0)
            {
                sb.Append(" | ");
            }

            sb.Append(piece);
            written++;
        }

        return sb.Length == 0 ? "-" : sb.ToString();
    }
}

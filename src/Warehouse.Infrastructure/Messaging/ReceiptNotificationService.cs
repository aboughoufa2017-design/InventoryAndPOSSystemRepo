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
        var summary = BuildSmsSummary(details, culture);
        var total = $"{details.GrandTotal.ToString("0.00", culture)} {details.CurrencyCode}";

        // Template placeholders: $1 = order id, $2 = total, $3 = line summary
        var record = new Dictionary<string, object?>
        {
            ["phone"] = normalized,
            ["$1"] = details.OrderId,
            ["$2"] = total,
            ["$3"] = summary,
        };
        if (!string.IsNullOrWhiteSpace(details.OrderCode))
        {
            record["$4"] = details.OrderCode;
        }

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

    private static string BuildSmsSummary(OrderReceiptDetails details, CultureInfo culture)
    {
        var sb = new StringBuilder();
        var count = 0;
        foreach (var line in details.Lines)
        {
            if (count >= 5)
            {
                sb.Append("…");
                break;
            }

            if (count > 0)
            {
                sb.Append(" | ");
            }

            sb.Append(line.Name)
                .Append('×')
                .Append(line.Quantity.ToString("0.###", culture));
            count++;
        }

        var text = sb.ToString();
        return text.Length <= 280 ? text : text[..277] + "…";
    }
}

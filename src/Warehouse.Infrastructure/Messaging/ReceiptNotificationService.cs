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
        // Resala receipt template uses a single placeholder, e.g.:
        // "اهلا بك هده تفاصيل الفاتوة $1"
        var record = new Dictionary<string, object?>
        {
            ["phone"] = normalized,
            ["$1"] = BuildReceiptPlaceholder(details, culture),
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

    /// <summary>Compact invoice text for the template's single <c>$1</c> placeholder.</summary>
    private static string BuildReceiptPlaceholder(OrderReceiptDetails details, CultureInfo culture)
    {
        var sb = new StringBuilder();
        var label = !string.IsNullOrWhiteSpace(details.OrderCode)
            ? details.OrderCode
            : details.OrderId;
        sb.Append('#').Append(label)
            .Append(' ')
            .Append(details.GrandTotal.ToString("0.00", culture))
            .Append(' ')
            .Append(details.CurrencyCode);

        var count = 0;
        foreach (var line in details.Lines)
        {
            if (count >= 5)
            {
                sb.Append(" …");
                break;
            }

            sb.Append(" | ")
                .Append(line.Name)
                .Append('×')
                .Append(line.Quantity.ToString("0.###", culture));
            count++;
        }

        var text = sb.ToString();
        // Keep well under a single SMS segment budget for the variable part.
        return text.Length <= 320 ? text : text[..317] + "…";
    }
}

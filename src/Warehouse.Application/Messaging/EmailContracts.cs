namespace Warehouse.Application.Messaging;

public sealed class SmtpSettings
{
    public const string SectionName = "SmtpSettings";

    public string Server { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public string SenderName { get; set; } = "Modern Tech POS";

    public string SenderEmail { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool EnableSsl { get; set; } = true;
}

public sealed class EmailMessage
{
    public string To { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public bool IsHtml { get; set; } = true;

    public List<EmailAttachment>? Attachments { get; set; }
}

public sealed class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;

    public byte[] Content { get; set; } = [];

    public string ContentType { get; set; } = "application/octet-stream";
}

public sealed record ReceiptLineDto(
    string Name,
    string Sku,
    decimal Quantity,
    decimal UnitPrice);

public sealed record OrderReceiptDetails(
    string OrderId,
    string? OrderCode,
    string CurrencyCode,
    decimal GrandTotal,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyList<ReceiptLineDto> Lines);

public interface IEmailService
{
    Task SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default);

    /// <summary>Sends a branded receipt email; attaches PDF when <paramref name="pdfReceiptPath"/> exists.</summary>
    Task SendReceiptEmailAsync(
        string toEmail,
        string orderId,
        string? pdfReceiptPath = null,
        CancellationToken cancellationToken = default);

    /// <summary>Sends order line details as HTML (no PDF required).</summary>
    Task SendOrderDetailsEmailAsync(
        string toEmail,
        OrderReceiptDetails details,
        CancellationToken cancellationToken = default);
}

public interface IReceiptNotificationService
{
    Task SendEmailAsync(string toEmail, OrderReceiptDetails details, CancellationToken cancellationToken = default);

    Task SendSmsAsync(string phone, OrderReceiptDetails details, CancellationToken cancellationToken = default);
}

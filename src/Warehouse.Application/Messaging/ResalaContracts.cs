using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Warehouse.Application.Messaging;

public sealed class ResalaOptions
{
    public const string SectionName = "Resala";

    /// <summary>API key starting with <c>rsl_</c>. Prefer env <c>RESALA_API_KEY</c> / <c>Resala__ApiKey</c>.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Base URL without trailing slash. Default is the Resala dev host.</summary>
    public string BaseUrl { get; set; } = "https://dev.resala.ly";

    /// <summary>Optional default sender name (must match an active registered name exactly).</summary>
    public string? DefaultSender { get; set; }

    /// <summary>
    /// Approved Resala SMS template id for order receipts.
    /// Placeholders: <c>$1</c> order, <c>$2</c> total, <c>$3</c> lines, <c>$4</c> time (UTC).
    /// </summary>
    public string? ReceiptSmsTemplateId { get; set; }
}

/// <summary>Resala error <c>type</c> values — branch on these, never on localized <c>message</c>.</summary>
public static class ResalaErrorTypes
{
    public const string AccountExpired = "AccountExpired";
    public const string InsufficientCredit = "InsufficientCredit";
    public const string BadRequest = "BadRequest";
    public const string Unauthorized = "Unauthorized";
    public const string TokenExpired = "TokenExpired";
    public const string Forbidden = "Forbidden";
    public const string NotFound = "NotFound";
    public const string InputValidation = "InputValidation";
}

public sealed class ResalaApiException : Exception
{
    public ResalaApiException(
        int statusCode,
        string type,
        string? message,
        IReadOnlyDictionary<string, string[]>? errors = null)
        : base(message ?? type)
    {
        StatusCode = statusCode;
        Type = type;
        Errors = errors ?? new ReadOnlyDictionary<string, string[]>(new Dictionary<string, string[]>());
    }

    public int StatusCode { get; }

    /// <summary>Resala <c>type</c> discriminator.</summary>
    public string Type { get; }

    /// <summary>Present for <see cref="ResalaErrorTypes.InputValidation"/> (422).</summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public bool Is(string type) =>
        string.Equals(Type, type, StringComparison.Ordinal);
}

public sealed record CreatePinRequest(string Phone);

public sealed record CreatePinQuery(
    string? ServiceName = null,
    bool? Test = null,
    int? Len = null,
    bool? Autofill = null,
    string? Sender = null);

public sealed class ResalaPinResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("pin")]
    public string Pin { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("region")]
    public string Region { get; set; } = string.Empty;

    [JsonPropertyName("number")]
    public string Number { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("hit_count")]
    public int HitCount { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }

    [JsonPropertyName("sender_name")]
    public string? SenderName { get; set; }
}

public sealed class SendTemplateRecord : Dictionary<string, object?>
{
    public SendTemplateRecord(string phone)
    {
        this["phone"] = phone;
    }

    public SendTemplateRecord Set(string placeholder, object? value)
    {
        this[placeholder] = value;
        return this;
    }
}

public sealed record SendTemplateQuery(
    string SmsTemplateId,
    bool? Test = null,
    string? Sender = null,
    string? ScheduledAt = null);

public sealed class SendTemplateRequest
{
    [JsonPropertyName("records")]
    public List<Dictionary<string, object?>> Records { get; set; } = [];
}

public sealed class ResalaSendTemplateResponse
{
    [JsonPropertyName("is_prod")]
    public bool IsProd { get; set; }

    [JsonPropertyName("total_messages")]
    public int TotalMessages { get; set; }

    [JsonPropertyName("total_free_messages")]
    public int TotalFreeMessages { get; set; }

    [JsonPropertyName("total_sent_free_messages")]
    public int TotalSentFreeMessages { get; set; }

    [JsonPropertyName("total_cost")]
    public decimal TotalCost { get; set; }

    [JsonPropertyName("succeeded")]
    public int Succeeded { get; set; }

    [JsonPropertyName("failed")]
    public int Failed { get; set; }

    [JsonPropertyName("failed_numbers")]
    public List<string> FailedNumbers { get; set; } = [];

    [JsonPropertyName("invalid_numbers")]
    public List<string> InvalidNumbers { get; set; } = [];

    [JsonPropertyName("sender_names")]
    public Dictionary<string, string>? SenderNames { get; set; }

    [JsonPropertyName("sms_template_version")]
    public ResalaSmsTemplateVersion? SmsTemplateVersion { get; set; }
}

public sealed class ResalaSmsTemplateVersion
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("version_number")]
    public int VersionNumber { get; set; }

    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

public sealed record SentViewQuery(
    string? Filters = null,
    string? Q = null,
    string? Sorts = null,
    int? Page = null,
    int? Paginate = null);

public sealed class ResalaSentViewResponse
{
    [JsonPropertyName("data")]
    public List<ResalaSentMessage> Data { get; set; } = [];

    [JsonPropertyName("meta")]
    public ResalaPaginationMeta Meta { get; set; } = new();
}

public sealed class ResalaSentMessage
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("region")]
    public string Region { get; set; } = string.Empty;

    [JsonPropertyName("number")]
    public string Number { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("pages")]
    public int Pages { get; set; }

    [JsonPropertyName("env")]
    public string Env { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class ResalaPaginationMeta
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("per_page")]
    public int PerPage { get; set; }

    [JsonPropertyName("current_page")]
    public int CurrentPage { get; set; }

    [JsonPropertyName("last_page")]
    public int LastPage { get; set; }
}

/// <summary>
/// Typed Resala SMS/OTP HTTP client.
/// API failures throw <see cref="ResalaApiException"/> (branch on <see cref="ResalaApiException.Type"/>).
/// Do not auto-retry failed sends.
/// </summary>
public interface IResalaSmsClient
{
    /// <summary>POST /api/v1/pins — OTP / verification code. International numbers allowed.</summary>
    Task<ResalaPinResponse> CreatePinAsync(
        CreatePinRequest request,
        CreatePinQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>POST /api/v1/messages/send-template — Libyan mobiles only (invalid rows skipped).</summary>
    Task<ResalaSendTemplateResponse> SendTemplateAsync(
        SendTemplateRequest request,
        SendTemplateQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>GET /api/v1/sent-view — paginated sent messages.</summary>
    Task<ResalaSentViewResponse> GetSentViewAsync(
        SentViewQuery? query = null,
        CancellationToken cancellationToken = default);
}

/// <summary>Libyan MSISDN helpers (091/092/093/094 + 7 digits, optional 218 prefix).</summary>
public static class ResalaPhone
{
    /// <summary>True for Libyan mobile numbers (local or with 218).</summary>
    public static bool IsLibyanMobile(string? phone)
    {
        var digits = DigitsOnly(phone);
        if (digits is null)
        {
            return false;
        }

        if (digits.Length == 10 && digits[0] == '0' && IsCarrierPrefix(digits.AsSpan(1, 2)))
        {
            return true;
        }

        if (digits.Length == 12 && digits.StartsWith("218", StringComparison.Ordinal)
            && IsCarrierPrefix(digits.AsSpan(3, 2)))
        {
            return true;
        }

        return false;
    }

    /// <summary>Normalize to <c>2189xxxxxxxx</c> when the number is a Libyan mobile; otherwise null.</summary>
    public static string? NormalizeLibyanMobile(string? phone)
    {
        var digits = DigitsOnly(phone);
        if (digits is null || !IsLibyanMobile(digits))
        {
            return null;
        }

        if (digits.Length == 10 && digits[0] == '0')
        {
            return "218" + digits[1..];
        }

        return digits;
    }

    private static string? DigitsOnly(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return null;
        }

        var digits = new string(phone.Where(char.IsDigit).ToArray());
        return digits.Length == 0 ? null : digits;
    }

    private static bool IsCarrierPrefix(ReadOnlySpan<char> two)
        => two is "91" or "92" or "93" or "94";
}

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Warehouse.Application.Messaging;

namespace Warehouse.Infrastructure.Messaging;

/// <summary>
/// Typed Resala SMS/OTP client. Authenticates with Bearer API key from configuration.
/// Failed sends are never retried automatically — catch <see cref="ResalaApiException"/> and branch on <c>Type</c>.
/// </summary>
public sealed class ResalaSmsClient : IResalaSmsClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly HttpClient _http;
    private readonly ResalaOptions _options;

    public ResalaSmsClient(HttpClient http, IOptions<ResalaOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public async Task<ResalaPinResponse> CreatePinAsync(
        CreatePinRequest request,
        CreatePinQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Phone))
        {
            throw new ArgumentException("Phone is required.", nameof(request));
        }

        var path = AppendQuery("/api/v1/pins", BuildPinQuery(query));
        return await SendAsync<ResalaPinResponse>(
            HttpMethod.Post,
            path,
            new { phone = request.Phone.Trim() },
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<ResalaSendTemplateResponse> SendTemplateAsync(
        SendTemplateRequest request,
        SendTemplateQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(query);
        if (string.IsNullOrWhiteSpace(query.SmsTemplateId))
        {
            throw new ArgumentException("sms_template_id is required.", nameof(query));
        }

        if (request.Records is null || request.Records.Count == 0)
        {
            throw new ArgumentException("At least one template record is required.", nameof(request));
        }

        var path = AppendQuery("/api/v1/messages/send-template", BuildSendTemplateQuery(query));
        return await SendAsync<ResalaSendTemplateResponse>(
            HttpMethod.Post,
            path,
            new { records = request.Records },
            cancellationToken).ConfigureAwait(false);
    }

    public Task<ResalaSentViewResponse> GetSentViewAsync(
        SentViewQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        var path = AppendQuery("/api/v1/sent-view", BuildSentViewQuery(query));
        return SendAsync<ResalaSentViewResponse>(HttpMethod.Get, path, body: null, cancellationToken);
    }

    private async Task<T> SendAsync<T>(
        HttpMethod method,
        string pathAndQuery,
        object? body,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException(
                "Resala API key is not configured (Resala__ApiKey / RESALA_API_KEY).");
        }

        using var request = new HttpRequestMessage(method, pathAndQuery);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey.Trim());
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        // Single attempt only — do not retry (message may already have been accepted).
        using var response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var status = (int)response.StatusCode;
        var raw = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                throw new ResalaApiException(status, "EmptyBody", "Resala returned an empty success body.");
            }

            try
            {
                var parsed = JsonSerializer.Deserialize<T>(raw, JsonOptions);
                if (parsed is null)
                {
                    throw new ResalaApiException(status, "EmptyBody", "Resala returned an unreadable success body.");
                }

                return parsed;
            }
            catch (JsonException ex)
            {
                throw new ResalaApiException(
                    status,
                    "ParseError",
                    $"Resala success body could not be parsed: {ex.Message}");
            }
        }

        var apiError = TryParseError(raw);
        var type = !string.IsNullOrWhiteSpace(apiError?.Type)
            ? apiError!.Type!
            : MapStatusToType(status);
        var message = apiError?.Message ?? $"Resala request failed ({status}).";

        throw new ResalaApiException(status, type, message, apiError?.Errors);
    }

    private static string MapStatusToType(int status) => status switch
    {
        400 => ResalaErrorTypes.BadRequest,
        401 => ResalaErrorTypes.Unauthorized,
        403 => ResalaErrorTypes.Forbidden,
        404 => ResalaErrorTypes.NotFound,
        422 => ResalaErrorTypes.InputValidation,
        _ => "HttpError",
    };

    private static ResalaErrorBody? TryParseError(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<ResalaErrorBody>(raw, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private List<KeyValuePair<string, string?>> BuildPinQuery(CreatePinQuery? query)
    {
        var q = query ?? new CreatePinQuery(Sender: _options.DefaultSender);
        var sender = q.Sender ?? _options.DefaultSender;
        return
        [
            new("service_name", q.ServiceName),
            new("test", BoolQuery(q.Test)),
            new("len", q.Len?.ToString()),
            new("autofill", BoolQuery(q.Autofill)),
            new("sender", sender),
        ];
    }

    private List<KeyValuePair<string, string?>> BuildSendTemplateQuery(SendTemplateQuery query)
    {
        var sender = query.Sender ?? _options.DefaultSender;
        return
        [
            new("sms_template_id", query.SmsTemplateId),
            new("test", BoolQuery(query.Test)),
            new("sender", sender),
            new("scheduled_at", query.ScheduledAt),
        ];
    }

    private static List<KeyValuePair<string, string?>> BuildSentViewQuery(SentViewQuery? query)
    {
        query ??= new SentViewQuery();
        return
        [
            new("filters", query.Filters),
            new("q", query.Q),
            new("sorts", query.Sorts),
            new("page", query.Page?.ToString()),
            new("paginate", query.Paginate?.ToString()),
        ];
    }

    private static string? BoolQuery(bool? value) => value switch
    {
        true => "true",
        false => "false",
        null => null,
    };

    private static string AppendQuery(string path, IEnumerable<KeyValuePair<string, string?>> pairs)
    {
        var sb = new StringBuilder(path);
        var first = true;
        foreach (var (key, value) in pairs)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            sb.Append(first ? '?' : '&');
            first = false;
            sb.Append(Uri.EscapeDataString(key));
            sb.Append('=');
            sb.Append(Uri.EscapeDataString(value));
        }

        return sb.ToString();
    }

    private sealed class ResalaErrorBody
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("errors")]
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}

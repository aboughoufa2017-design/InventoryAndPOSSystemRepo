using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Warehouse.Api.Http;
using Warehouse.Shared.Api;

namespace Warehouse.IntegrationTests;

public class ApiHostTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public ApiHostTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Live_health_returns_healthy()
    {
        var response = await _client.GetAsync("/health/live");
        var body = await response.Content.ReadFromJsonAsync<LiveHealthResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal("Healthy", body.Status);
    }

    [Fact]
    public async Task Live_health_echoes_a_valid_correlation_id()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        request.Headers.TryAddWithoutValidation(ApiHeaders.CorrelationId, "device-sync-01");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues(ApiHeaders.CorrelationId, out var values));
        Assert.Equal("device-sync-01", Assert.Single(values));
    }

    [Fact]
    public async Task Live_health_replaces_an_invalid_correlation_id()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        request.Headers.TryAddWithoutValidation(ApiHeaders.CorrelationId, "bad id");

        var response = await _client.SendAsync(request);

        Assert.True(response.Headers.TryGetValues(ApiHeaders.CorrelationId, out var values));
        var correlationId = Assert.Single(values);
        Assert.NotEqual("bad id", correlationId);
        Assert.True(Guid.TryParse(correlationId, out _));
    }
}

public class UnhandledExceptionHandlerTests
{
    [Fact]
    public async Task Hides_exception_details_and_keeps_the_correlation_id()
    {
        var context = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider(),
            Response = { Body = new MemoryStream() },
        };
        context.Items[CorrelationIdMiddleware.ItemKey] = "corr-1";

        var handler = new UnhandledExceptionHandler(NullLogger<UnhandledExceptionHandler>.Instance);
        var handled = await handler.TryHandleAsync(
            context,
            new InvalidOperationException("password=super-secret"),
            CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);

        context.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(context.Response.Body);
        var root = document.RootElement;

        Assert.Equal(ApiErrorCodes.InternalError, root.GetProperty("code").GetString());
        Assert.Equal("An unexpected error occurred.", root.GetProperty("message").GetString());
        Assert.Equal("corr-1", root.GetProperty("correlationId").GetString());
        Assert.DoesNotContain("super-secret", root.ToString(), StringComparison.Ordinal);
    }
}

using Microsoft.Extensions.DependencyInjection;
using Wigo4it.MultiTenant.MultiTenantHttpClient;

namespace Wigo4it.MultiTenant.Tests.MultiTenantHttpClient;

[TestFixture]
public class HttpClientServiceCollectionExtensionsTests
{
    [Test]
    public async Task AddMultiTenantHttpClient_with_named_client_adds_multitenant_headers()
    {
        var services = new ServiceCollection();
        var recordingHandler = new RecordingHandler();

        services.Configure<Wigo4itTenantOptions>(options =>
        {
            options.TenantCode = "9446";
            options.EnvironmentName = "dev";
            options.GemeenteCode = "0363";
        });

        services.AddMultiTenantHttpClient("tenant-aware").ConfigurePrimaryHttpMessageHandler(() => recordingHandler);

        await using var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("tenant-aware");

        await client.GetAsync("https://example.test");

        Assert.That(recordingHandler.LastRequest, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(
                GetSingleHeaderValue(recordingHandler.LastRequest!, MultitenancyIdentifiers.HttpHeaders.WegwijzerTenantCode),
                Is.EqualTo("9446")
            );
            Assert.That(
                GetSingleHeaderValue(recordingHandler.LastRequest!, MultitenancyIdentifiers.HttpHeaders.WegwijzerEnvironmentName),
                Is.EqualTo("dev")
            );
            Assert.That(
                GetSingleHeaderValue(recordingHandler.LastRequest!, MultitenancyIdentifiers.HttpHeaders.GemeenteCode),
                Is.EqualTo("0363")
            );
        }
    }

    [Test]
    public async Task AddMultiTenantHttpClient_with_typed_client_adds_multitenant_headers()
    {
        var services = new ServiceCollection();
        var recordingHandler = new RecordingHandler();

        services.Configure<Wigo4itTenantOptions>(options =>
        {
            options.TenantCode = "1234";
            options.EnvironmentName = "acc";
            options.GemeenteCode = "0599";
        });

        services.AddMultiTenantHttpClient<TestTypedClient>().ConfigurePrimaryHttpMessageHandler(() => recordingHandler);

        await using var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<TestTypedClient>();

        await client.HttpClient.GetAsync("https://example.test");

        Assert.That(recordingHandler.LastRequest, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(
                GetSingleHeaderValue(recordingHandler.LastRequest!, MultitenancyIdentifiers.HttpHeaders.WegwijzerTenantCode),
                Is.EqualTo("1234")
            );
            Assert.That(
                GetSingleHeaderValue(recordingHandler.LastRequest!, MultitenancyIdentifiers.HttpHeaders.WegwijzerEnvironmentName),
                Is.EqualTo("acc")
            );
            Assert.That(
                GetSingleHeaderValue(recordingHandler.LastRequest!, MultitenancyIdentifiers.HttpHeaders.GemeenteCode),
                Is.EqualTo("0599")
            );
        }
    }

    private static string GetSingleHeaderValue(HttpRequestMessage request, string headerName)
    {
        return request.Headers.TryGetValues(headerName, out var values)
            ? values.Single()
            : throw new AssertionException($"Header '{headerName}' was not found on the outgoing request.");
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        }
    }

    private class TestTypedClient(HttpClient httpClient)
    {
        public HttpClient HttpClient { get; } = httpClient;
    }
}

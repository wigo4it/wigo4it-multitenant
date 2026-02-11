using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus.Extensions.IntegrationTesting;
using Wigo4it.MultiTenant.NServiceBus.Sample;

namespace Wigo4it.MultiTenant.NServiceBus.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory for integration testing the multi-tenant NServiceBus sample application.
/// This factory configures the test server with appropriate settings for integration testing.
/// </summary>
public class TestWebApplicationFactory(Action<IConfigurationBuilder>? configureConfiguration = null)
    : WebApplicationFactory<Program>
{
    internal readonly TestLoggerProvider Logger = new();

    protected override IHostBuilder CreateHostBuilder()
    {
        var builder = Host.CreateDefaultBuilder()
            .UseNServiceBus(ctx =>
            {
                var endpoint = SampleEndpointConfiguration.Create(ctx);

                // Configure the endpoint with test-friendly defaults
                endpoint.ConfigureTestEndpoint();

                // Re-set storage directory after ConfigureTestEndpoint replaces the transport
                var transport = endpoint.UseTransport<LearningTransport>();
                transport.StorageDirectory(Path.Combine(ctx.HostingEnvironment.ContentRootPath, ".nsbtransport"));

                return endpoint;
            })
            .ConfigureLogging(l =>
            {
                l.ClearProviders(); // `Host.CreateDefaultBuilder` logt ook naar de Windows event log, dit leidt soms tot error in de CI runs, en doen we toch niets mee.
                l.AddProvider(Logger);
            })
            .ConfigureAppConfiguration(c => configureConfiguration?.Invoke(c))
            .ConfigureServices(services =>
            {
                services.ConfigureSampleServices();
            })
            .ConfigureWebHostDefaults(b => b.Configure(_ => { }));

        return builder;
    }
}

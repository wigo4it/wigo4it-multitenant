using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus.Extensions.IntegrationTesting;
using NServiceBus.Logging;
using Wigo4it.MultiTenant.NServiceBus.Sample;

namespace Wigo4it.MultiTenant.NServiceBus.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory for integration testing the multi-tenant NServiceBus sample application.
/// This factory configures the test server with appropriate settings for integration testing.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly Action<IConfigurationBuilder>? _configureConfiguration;

    internal readonly TestLoggerProvider Logger = new();

    public TestWebApplicationFactory(Action<IConfigurationBuilder>? configureConfiguration = null)
    {
        _configureConfiguration = configureConfiguration;
    }

    public TestWebApplicationFactory WithConfiguration(Action<IConfigurationBuilder> configureConfiguration)
    {
        return new TestWebApplicationFactory(configureConfiguration);
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        var builder = Host.CreateDefaultBuilder()
            .UseNServiceBus(ctx =>
            {
                var endpoint = SampleEndpointConfiguration.Create(ctx);

                // Configure the endpoint with test-friendly defaults
                endpoint.ConfigureTestEndpoint();

                return endpoint;
            })
            .ConfigureLogging(l =>
            {
                l.AddProvider(Logger);
            })
            .ConfigureAppConfiguration(c => _configureConfiguration?.Invoke(c))
            .ConfigureServices(services =>
            {
                services.ConfigureSampleServices();
            })
            .ConfigureWebHostDefaults(b => b.Configure(_ => { }));

        return builder;
    }

    // protected override void ConfigureWebHost(IWebHostBuilder builder)
    // {
    //     // Explicitly load the Testing appsettings to ensure test-specific configuration is applied
    //     builder.ConfigureAppConfiguration((_, config) =>
    //     {
    //         // Apply per-test configuration customizations last so they override defaults
    //         _configureConfiguration?.Invoke(config);
    //     });
    //
    //     builder.ConfigureServices(services =>
    //     {
    //         // Add any test-specific service overrides here
    //         // For example, you might want to replace database contexts with in-memory versions
    //         // or mock certain services for testing
    //         _configureServices?.Invoke(services);
    //     });
    //}
}

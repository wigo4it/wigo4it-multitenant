using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus.Extensions.IntegrationTesting;
using Wigo4it.MultiTenant.NServiceBus;

namespace Wigo4it.MultiTenant.AspNetCore.IntegrationTests;

public class TestWebApplicationFactory : WebApplicationFactory<TestProgram>
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

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            _configureConfiguration?.Invoke(config);
        });
        
        builder.ConfigureServices(services =>
        {
            services.AddLogging(l => l.AddProvider(Logger));
        });
    }
}

public class TestProgram
{
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseNServiceBus(ctx =>
            {
                var endpoint = new EndpointConfiguration("TestEndpoint");
                endpoint.UseTransport<LearningTransport>();
                endpoint.UseSerialization<SystemJsonSerializer>();
                endpoint.ConfigureTestEndpoint();
                endpoint.UseWigo4itMultiTenant();
                return endpoint;
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureServices(services =>
                {
                    services.AddWigo4itMultiTenantAspNetCore<Wigo4itTenantInfo>();
                    services.ConfigurePerTenant<TestOptions, Wigo4itTenantInfo>((options, tenant) =>
                    {
                        options.CustomSetting = tenant.ConnectionString ?? "Default";
                    });
                });
                
                webBuilder.Configure(app =>
                {
                    app.UseWigo4itMultiTenant();
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapPost("/send-message", async (IMessageSession messageSession) =>
                        {
                            await messageSession.SendLocal(new TestMessage { Content = "Hello from HTTP" });
                            return Results.Accepted();
                        });
                    });
                });
            });
}

public class TestOptions
{
    public string CustomSetting { get; set; } = string.Empty;
}

public class TestMessage : IMessage
{
    public string Content { get; set; } = string.Empty;
}

public class TestMessageHandler : IHandleMessages<TestMessage>
{
    private readonly ILogger<TestMessageHandler> _logger;

    public TestMessageHandler(ILogger<TestMessageHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(TestMessage message, IMessageHandlerContext context)
    {
        _logger.LogInformation("Received message: {Content}", message.Content);
        return Task.CompletedTask;
    }
}

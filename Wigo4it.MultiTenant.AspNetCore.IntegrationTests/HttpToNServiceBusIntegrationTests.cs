using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static NServiceBus.Extensions.IntegrationTesting.EndpointFixture;

namespace Wigo4it.MultiTenant.AspNetCore.IntegrationTests;

/// <summary>
/// Integration tests for multi-tenant HTTP to NServiceBus message forwarding.
/// Tests that HTTP headers are properly captured and forwarded to NServiceBus messages.
/// </summary>
public class HttpToNServiceBusIntegrationTests
{
    private TestWebApplicationFactory _factory = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new TestWebApplicationFactory();
    }

    [TearDown]
    public void TearDown()
    {
        _factory.Dispose();
    }

    [Test]
    public async Task ShouldForwardHttpHeadersToNServiceBusMessages()
    {
        const string tenantCode = "9446";
        const string environmentName = "dev";
        const string gemeenteCode = "0599";
        
        // Configure tenant
        _factory = _factory.WithConfiguration(cfg =>
        {
            var settings = new Dictionary<string, string?>
            {
                { $"Tenants:{tenantCode}:Environments:{environmentName}:Gemeenten:{gemeenteCode}:ConnectionString", "TestConnection" }
            };
            cfg.AddInMemoryCollection(settings);
        });
        
        var client = _factory.CreateClient();
        
        // Add multi-tenancy headers
        client.DefaultRequestHeaders.Add(MultitenancyHeaders.WegwijzerTenantCode, tenantCode);
        client.DefaultRequestHeaders.Add(MultitenancyHeaders.WegwijzerEnvironmentName, environmentName);
        client.DefaultRequestHeaders.Add(MultitenancyHeaders.GemeenteCode, gemeenteCode);
        
        // Send HTTP request that triggers NServiceBus message
        var messageSession = _factory.Services.GetRequiredService<IMessageSession>();
        await ExecuteAndWaitForHandled<TestMessage>(() => 
            client.PostAsync("/send-message", null));
        
        // Verify that message was handled
        var logs = _factory.Logger.Logs
            .Where(l => l.Category == typeof(TestMessageHandler).FullName)
            .ToList();
        
        Assert.That(logs, Is.Not.Empty, "No logs from TestMessageHandler found");
        Assert.That(logs, Has.Exactly(1).Matches<LogEntry>(l => 
            l.Message.Contains("Received message: Hello from HTTP")));
    }

    [Test]
    [TestCase("9446", "dev", "0599")]
    [TestCase("0518", "prod", "0001")]
    public async Task ShouldResolveTenantFromHttpHeaders(
        string tenantCode, 
        string environmentName, 
        string gemeenteCode)
    {
        // Configure tenants
        _factory = _factory.WithConfiguration(cfg =>
        {
            var settings = new Dictionary<string, string?>
            {
                { $"Tenants:{tenantCode}:Environments:{environmentName}:Gemeenten:{gemeenteCode}:ConnectionString", $"Connection-{gemeenteCode}" }
            };
            cfg.AddInMemoryCollection(settings);
        });
        
        var client = _factory.CreateClient();
        
        // Add multi-tenancy headers
        client.DefaultRequestHeaders.Add(MultitenancyHeaders.WegwijzerTenantCode, tenantCode);
        client.DefaultRequestHeaders.Add(MultitenancyHeaders.WegwijzerEnvironmentName, environmentName);
        client.DefaultRequestHeaders.Add(MultitenancyHeaders.GemeenteCode, gemeenteCode);
        
        // Send HTTP request
        var messageSession = _factory.Services.GetRequiredService<IMessageSession>();
        await ExecuteAndWaitForHandled<TestMessage>(() => 
            client.PostAsync("/send-message", null));
        
        // Verify tenant was resolved correctly
        var logs = _factory.Logger.Logs
            .Where(l => l.Category == typeof(TestMessageHandler).FullName)
            .ToList();
        
        Assert.That(logs, Is.Not.Empty, $"No logs from TestMessageHandler found for tenant {tenantCode}");
    }
}

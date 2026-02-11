using Microsoft.Extensions.Configuration;
using Wigo4it.MultiTenant.NServiceBus.Sample;
using static NServiceBus.Extensions.IntegrationTesting.EndpointFixture;

namespace Wigo4it.MultiTenant.NServiceBus.IntegrationTests;

/// <summary>
/// Integratietests voor multi-tenant berichtenafhandeling met NServiceBus.
/// Test tenantresolutie en context propagatie door de berichtenpijplijn.
/// </summary>
public class MultiTenantMessageTests
{
    private TestWebApplicationFactory? factory;

    [TearDown]
    public void TearDown()
    {
        factory?.Dispose();
    }

    [Test]
    public async Task ShouldReturnDefaultIfNoOverride()
    {
        const string tenantCode = "9446";
        const string environmentName = "dev";
        const string gemeenteCode = "0599";
        const string expectedSettingValue = "Default setting at Environment level";

        factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();
        await ExecuteAndWaitForHandled<SampleMessage>(() =>
            client.PostAsync($"/send/{tenantCode}-{environmentName}-{gemeenteCode}", null)
        );

        // Controleer dat de aangepaste instellingswaarde is geregistreerd
        var sampleMessageHandlerLogs = factory
            .Logger.Logs.Where(l => l.Category == typeof(SampleMessageHandler).FullName)
            .ToList();

        Assert.That(sampleMessageHandlerLogs, Is.Not.Empty, "Geen logs van SampleMessageHandler gevonden");

        Assert.That(
            sampleMessageHandlerLogs,
            Has.Exactly(1).Matches<LogEntry>(l => l.Message.Contains($"Custom setting: {expectedSettingValue}"))
        );
    }

    [Test]
    [TestCase("9446", "dev", "0599", "Test setting: Default")]
    [TestCase("9446", "dev", "0518", "Test setting: Specific")]
    public async Task ShouldReturnOverrides(string tenantCode, string environmentName, string gemeenteCode, string settingValue)
    {
        // Zet gemeente-specifieke configuratie.
        // Hier via InMemoryCollection, maar elke .Net ConfigurationProvider zou werken.
        factory = new TestWebApplicationFactory(cfg =>
        {
            var overrides = new Dictionary<string, string?>
            {
                { $"Tenants:{tenantCode}:Environments:{environmentName}:Gemeenten:{gemeenteCode}:CustomSetting", settingValue },
            };
            cfg.AddInMemoryCollection(overrides);
        });

        var message = new SampleMessage
        {
            Content = $"Sample message for {tenantCode}-{environmentName}-{gemeenteCode}",
            CreatedAtUtc = DateTime.UtcNow,
        };

        var request = factory.Server.CreateRequest($"/send/{tenantCode}-{environmentName}-{gemeenteCode}");
        await ExecuteAndWaitForHandled<SampleMessage>(request.PostAsync);

        // Controleer dat de aangepaste instellingswaarde is geregistreerd
        var sampleMessageHandlerLogs = factory
            .Logger.Logs.Where(l => l.Category == typeof(SampleMessageHandler).FullName)
            .ToList();

        Assert.That(sampleMessageHandlerLogs, Is.Not.Empty, "Geen logs van SampleMessageHandler gevonden");

        Assert.That(
            sampleMessageHandlerLogs,
            Has.Exactly(1).Matches<LogEntry>(l => l.Message.Contains($"Custom setting: {settingValue}"))
        );
    }
}

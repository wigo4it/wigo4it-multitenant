using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Wigo4it.MultiTenant.Tests;

public class MultiLevelConfigurationTests
{
    [Test]
    public async Task KanWaardenOphalen()
    {
        IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["MyOptions:Level1"] = "L1 value",
                ["Tenants:9446:MyOptions:Level2"] = "L2 value",
                ["Tenants:9446:Environments:dev:MyOptions:Level3"] = "L3 value",
                ["Tenants:9446:Environments:dev:Gemeenten:0363:MyOptions:Level4"] = "L4 value",
            }
        );

        // Arrange
        var store = new DictionaryConfigurationStore(configurationBuilder.Build());

        // Act
        var stage = await store.GetAsync("9446-dev-0363");

        var myOptions = stage!.Configuration.GetSection("MyOptions").Get<MyOptions>()!;

        // Assert
        Assert.That(myOptions.Level1, Is.EqualTo("L1 value"));
        Assert.That(myOptions.Level2, Is.EqualTo("L2 value"));
        Assert.That(myOptions.Level3, Is.EqualTo("L3 value"));
        Assert.That(myOptions.Level4, Is.EqualTo("L4 value"));
    }

    [Test]
    public async Task LagerNiveauOverschrijftHogerNiveau()
    {
        IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                // Rootniveau stelt Shared in op "root"
                ["MyOptions:Shared"] = "root",
                // Omgevingniveau overschrijft Shared naar "environment"
                ["Tenants:9446:Environments:dev:MyOptions:Shared"] = "environment",
                // Gemeenteniveau overschrijft Shared naar "gemeente"
                ["Tenants:9446:Environments:dev:Gemeenten:0363:MyOptions:Shared"] = "gemeente",
                ["Tenants:9446:Environments:dev:Gemeenten:0363:Identifier"] = "9446-dev-0363",
            }
        );

        var store = new DictionaryConfigurationStore(configurationBuilder.Build());

        var tenant = await store.GetAsync("9446-dev-0363");

        var myOptions = new MyOptions();
        tenant!.Configuration.Bind("MyOptions", myOptions);

        // Het meest specifieke (laagste) niveau wint
        Assert.That(myOptions.Shared, Is.EqualTo("gemeente"));
    }

    [Test]
    public async Task OmgevingOverschrijftRoot()
    {
        IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                // Rootniveau stelt Shared in op "root"
                ["MyOptions:Shared"] = "root",
                // Omgevingniveau overschrijft Shared naar "environment"
                ["Tenants:9446:Environments:dev:MyOptions:Shared"] = "environment",
                // Gemeente overschrijft Shared NIET
                ["Tenants:9446:Environments:dev:Gemeenten:0363"] = null,
            }
        );

        var store = new DictionaryConfigurationStore(configurationBuilder.Build());

        var tenant = await store.GetAsync("9446-dev-0363");

        var myOptions = new MyOptions();
        tenant!.Configuration.Bind("MyOptions", myOptions);

        // Omgeving moet root overschrijven wanneer gemeente geen waarde specificeert
        Assert.That(myOptions.Shared, Is.EqualTo("environment"));
    }

    [Test]
    public async Task KanOptionsOphalenViaFinbuckle()
    {
        // Arrange: bouw configuratie met waarden op alle drie niveaus
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["MyOptions:Level1"] = "L1 value",
                    ["Tenants:9446:Environments:dev:MyOptions:Level2"] = "L2 value",
                    ["Tenants:9446:Environments:dev:MyOptions:Shared"] = "environment",
                    ["Tenants:9446:Environments:dev:Gemeenten:0363:MyOptions:Level3"] = "L3 value",
                    ["Tenants:9446:Environments:dev:Gemeenten:0363:MyOptions:Shared"] = "gemeente",
                    ["Tenants:9446:Environments:dev:Gemeenten:0363:Identifier"] = "9446-dev-0363",
                }
            )
            .Build();

        // Arrange: zet services op met Finbuckle en de DictionaryConfigurationStore
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        services
            .AddMultiTenant<Wigo4itTenantInfo>()
            .WithStore<DictionaryConfigurationStore>(ServiceLifetime.Singleton)
            .WithStaticStrategy("9446-dev-0363");

        services.AddOptions<MyOptions>().BindConfigurationPerTenant("MyOptions");

        await using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        // Act: los de tenant op via Finbuckle's resolver en zet de context
        var tenantResolver = scope.ServiceProvider.GetRequiredService<ITenantResolver>();
        var tenantContext = await tenantResolver.ResolveAsync(new object());
        Assert.That(tenantContext.IsResolved, Is.True, "Tenant moet worden opgelost door de statische strategie");

        var contextSetter = scope.ServiceProvider.GetRequiredService<IMultiTenantContextSetter>();
        contextSetter.MultiTenantContext = tenantContext;

        var resolvedOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<MyOptions>>().Value;

        // Assert: waarden van alle drie niveaus zijn aanwezig, lager overschrijft hoger
        Assert.That(resolvedOptions.Level1, Is.EqualTo("L1 value"));
        Assert.That(resolvedOptions.Level2, Is.EqualTo("L2 value"));
        Assert.That(resolvedOptions.Level3, Is.EqualTo("L3 value"));
        Assert.That(resolvedOptions.Shared, Is.EqualTo("gemeente"));
    }

    [Test]
    public async Task ConfiguratieBevatGeenWaardenVanAndereTenants()
    {
        IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                // Globale root-level instelling
                ["MyOptions:RootSetting"] = "root",

                // Tenant 9446, omgeving dev, gemeente 0363 (ons doel)
                ["Tenants:9446:Environments:dev:MyOptions:EnvSetting"] = "dev-setting",
                ["Tenants:9446:Environments:dev:Gemeenten:0363:Identifier"] = "9446-dev-0363",
                ["Tenants:9446:Environments:dev:Gemeenten:0363:MyOptions:GemeenteSetting"] = "gemeente-0363",

                // Andere gemeente in dezelfde omgeving — mag NIET lekken
                ["Tenants:9446:Environments:dev:Gemeenten:0599:Identifier"] = "should NOT leak 1",
                ["Tenants:9446:Environments:dev:Gemeenten:0599:MyOptions:GemeenteSetting"] = "should NOT leak",

                // Andere omgeving in dezelfde tenant — mag NIET lekken
                ["Tenants:9446:Environments:acc:MyOptions:EnvSetting"] = "should NOT leak",
                ["Tenants:9446:Environments:acc:Gemeenten:0363:Identifier"] = "should NOT leak 2",
                ["Tenants:9446:Environments:acc:Gemeenten:0363:MyOptions:GemeenteSetting"] = "should NOT leak",

                // Volledig andere tenant — mag NIET lekken
                ["Tenants:1234:Environments:dev:MyOptions:EnvSetting"] = "should NOT leak",
                ["Tenants:1234:Environments:dev:Gemeenten:0001:Identifier"] = "should NOT leak 3",
                ["Tenants:1234:Environments:dev:Gemeenten:0001:MyOptions:GemeenteSetting"] = "should NOT leak",
            }
        );

        var store = new DictionaryConfigurationStore(configurationBuilder.Build());

        var tenant = await store.GetAsync("9446-dev-0363");
        Assert.That(tenant, Is.Not.Null);

        // Verzamel alle sleutel-waardeparen uit de tenant-configuratie
        var allValues = tenant!
            .Configuration.AsEnumerable()
            .Where(kvp => kvp.Value is not null)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // Moet eigen waarden van alle drie niveaus bevatten
        Assert.That(allValues, Contains.Key("MyOptions:RootSetting"));
        Assert.That(allValues, Contains.Key("Tenants:9446:Environments:dev:MyOptions:EnvSetting"));
        Assert.That(allValues, Contains.Key("Tenants:9446:Environments:dev:Gemeenten:0363:MyOptions:GemeenteSetting"));

        // Mag GEEN waarden van de andere gemeente, omgeving of tenant bevatten
        var leakedValues = allValues.Where(kvp => kvp.Value!.StartsWith("should NOT leak")).ToList();

        Assert.That(
            leakedValues,
            Is.Empty,
            $"Tenant-configuratie bevat gelekte waarden van andere tenants/omgevingen/gemeenten: "
                + $"{string.Join(", ", leakedValues.Select(kvp => $"{kvp.Key}={kvp.Value}"))}"
        );
    }

    class MyOptions
    {
        public string? Level1 { get; init; }
        public string? Level2 { get; init; }
        public string? Level3 { get; init; }
        public string? Level4 { get; init; }
        public string? Shared { get; init; }
    }
}

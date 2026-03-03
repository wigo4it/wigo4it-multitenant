using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Wigo4it.MultiTenant.Tests;

public class FlatteningDictionaryConfigurationStoreTests
{
    [Test]
    public async Task KanWaardenOphalen()
    {
        IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["MyOptions:Level1"] = "L1 value",
            ["Tenants:9446:Environments:dev:MyOptions:Level2"] = "L2 value",
            ["Tenants:9446:Environments:dev:Gemeenten:0363:MyOptions:Level3"] = "L3 value",
            ["Tenants:9446:Environments:dev:Gemeenten:0363:Identifier"] = "9446-dev-0559",
        });

        // Arrange
        var store = new FlatteningDictionaryConfigurationStore(configurationBuilder.Build());

        // Act
        var stage = await store.GetAsync("9446-dev-0559");


        var myOptions = new MyOptions();
        stage.Configuration.Bind("MyOptions", myOptions);
        Assert.That(myOptions.Level1, Is.EqualTo("L1 value"));
        Assert.That(myOptions.Level2, Is.EqualTo("L2 value"));
        Assert.That(myOptions.Level3, Is.EqualTo("L3 value"));

        // Assert
    }

    [Test]
    public async Task LagerNiveauOverschrijftHogerNiveau()
    {
        IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            // Root level sets Shared to "root"
            ["MyOptions:Shared"] = "root",
            // Environment level overrides Shared to "environment"
            ["Tenants:9446:Environments:dev:MyOptions:Shared"] = "environment",
            // Gemeente level overrides Shared to "gemeente"
            ["Tenants:9446:Environments:dev:Gemeenten:0363:MyOptions:Shared"] = "gemeente",
            ["Tenants:9446:Environments:dev:Gemeenten:0363:Identifier"] = "9446-dev-0363",
        });

        var store = new FlatteningDictionaryConfigurationStore(configurationBuilder.Build());

        var tenant = await store.GetAsync("9446-dev-0363");

        var myOptions = new MyOptions();
        tenant!.Configuration.Bind("MyOptions", myOptions);

        // The most specific (lowest) level should win
        Assert.That(myOptions.Shared, Is.EqualTo("gemeente"));
    }

    [Test]
    public async Task OmgevingOverschrijftRoot()
    {
        IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            // Root level sets Shared to "root"
            ["MyOptions:Shared"] = "root",
            // Environment level overrides Shared to "environment"
            ["Tenants:9446:Environments:dev:MyOptions:Shared"] = "environment",
            // Gemeente does NOT override Shared
            ["Tenants:9446:Environments:dev:Gemeenten:0363:Identifier"] = "9446-dev-0363",
        });

        var store = new FlatteningDictionaryConfigurationStore(configurationBuilder.Build());

        var tenant = await store.GetAsync("9446-dev-0363");

        var myOptions = new MyOptions();
        tenant!.Configuration.Bind("MyOptions", myOptions);

        // Environment should override root when gemeente doesn't specify a value
        Assert.That(myOptions.Shared, Is.EqualTo("environment"));
    }
    
    [Test]
    public async Task KanOptionsOphalenViaFinbuckle()
    {
        // Arrange: build configuration with values at all three levels
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MyOptions:Level1"] = "L1 value",
                ["Tenants:9446:Environments:dev:MyOptions:Level2"] = "L2 value",
                ["Tenants:9446:Environments:dev:MyOptions:Shared"] = "environment",
                ["Tenants:9446:Environments:dev:Gemeenten:0363:MyOptions:Level3"] = "L3 value",
                ["Tenants:9446:Environments:dev:Gemeenten:0363:MyOptions:Shared"] = "gemeente",
                ["Tenants:9446:Environments:dev:Gemeenten:0363:Identifier"] = "9446-dev-0363",
            })
            .Build();

        // Arrange: wire up services with Finbuckle and the FlatteningDictionaryConfigurationStore
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        services
            .AddMultiTenant<FlattendConfigTennantInfo>()
            .WithStore<FlatteningDictionaryConfigurationStore>(ServiceLifetime.Singleton)
            .WithStaticStrategy("9446-dev-0363");

        services.ConfigurePerTenant<MyOptions, FlattendConfigTennantInfo>(
            (options, tenant) =>
            {
                tenant.Configuration.Bind("MyOptions", options);
            }
        );

        await using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        // Act: resolve the tenant via Finbuckle's resolver and set the context
        var tenantResolver = scope.ServiceProvider.GetRequiredService<ITenantResolver>();
        var tenantContext = await tenantResolver.ResolveAsync(new object());
        Assert.That(tenantContext.IsResolved, Is.True, "Tenant should be resolved by the static strategy");

        var contextSetter = scope.ServiceProvider.GetRequiredService<IMultiTenantContextSetter>();
        contextSetter.MultiTenantContext = tenantContext;

        var resolvedOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<MyOptions>>().Value;

        // Assert: values from all three levels are present, lower overrides higher
        Assert.That(resolvedOptions.Level1, Is.EqualTo("L1 value"));
        Assert.That(resolvedOptions.Level2, Is.EqualTo("L2 value"));
        Assert.That(resolvedOptions.Level3, Is.EqualTo("L3 value"));
        Assert.That(resolvedOptions.Shared, Is.EqualTo("gemeente"));
    }

    [Test]
    public async Task ConfiguratieBevatGeenWaardenVanAndereTenants()
    {
        IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            // Global root-level setting
            ["MyOptions:RootSetting"] = "root",

            // Tenant 9446, environment dev, gemeente 0363 (our target)
            ["Tenants:9446:Environments:dev:MyOptions:EnvSetting"] = "dev-setting",
            ["Tenants:9446:Environments:dev:Gemeenten:0363:Identifier"] = "9446-dev-0363",
            ["Tenants:9446:Environments:dev:Gemeenten:0363:MyOptions:GemeenteSetting"] = "gemeente-0363",

            // Other gemeente in same environment — should NOT leak
            ["Tenants:9446:Environments:dev:Gemeenten:0599:Identifier"] = "should NOT leak 1",
            ["Tenants:9446:Environments:dev:Gemeenten:0599:MyOptions:GemeenteSetting"] = "should NOT leak",

            // Other environment in same tenant — should NOT leak
            ["Tenants:9446:Environments:acc:MyOptions:EnvSetting"] = "should NOT leak",
            ["Tenants:9446:Environments:acc:Gemeenten:0363:Identifier"] = "should NOT leak 2",
            ["Tenants:9446:Environments:acc:Gemeenten:0363:MyOptions:GemeenteSetting"] = "should NOT leak",

            // Completely different tenant — should NOT leak
            ["Tenants:1234:Environments:dev:MyOptions:EnvSetting"] = "should NOT leak",
            ["Tenants:1234:Environments:dev:Gemeenten:0001:Identifier"] = "should NOT leak 3",
            ["Tenants:1234:Environments:dev:Gemeenten:0001:MyOptions:GemeenteSetting"] = "should NOT leak",
        });

        var store = new FlatteningDictionaryConfigurationStore(configurationBuilder.Build());

        var tenant = await store.GetAsync("9446-dev-0363");
        Assert.That(tenant, Is.Not.Null);

        // Collect all key-value pairs from the tenant's configuration
        var allValues = tenant!.Configuration.AsEnumerable()
            .Where(kvp => kvp.Value is not null)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // Should contain own values from all three levels
        Assert.That(allValues, Contains.Key("MyOptions:RootSetting"));
        Assert.That(allValues, Contains.Key("MyOptions:EnvSetting"));
        Assert.That(allValues["MyOptions:EnvSetting"], Is.EqualTo("dev-setting"));
        Assert.That(allValues, Contains.Key("MyOptions:GemeenteSetting"));
        Assert.That(allValues["MyOptions:GemeenteSetting"], Is.EqualTo("gemeente-0363"));

        // Should NOT contain values from the other gemeente, environment, or tenant
        var leakedValues = allValues.Where(kvp => kvp.Value.StartsWith("should NOT leak")).ToList();

        Assert.That(leakedValues, Is.Empty,
            $"Tenant configuration contains leaked values from other tenants/environments/gemeenten: " +
            $"{string.Join(", ", leakedValues.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
    }

    class MyOptions
    {
        public string? Level1 { get; set; }
        public string? Level2 { get; set; }
        public string? Level3 { get; set; }
        public string? Shared { get; set; }
    }
}
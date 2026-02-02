using System.Collections.Concurrent;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NServiceBus.Testing;
using Wigo4it.MultiTenant.NServiceBus;

namespace Wigo4it.MultiTenant.NserviceBus.Tests;

[TestFixture]
public class RaceConditionTests
{
    private ServiceProvider? _services;

    private readonly List<Wigo4itTenantInfo> _expectedValues =
    [
        new()
        {
            Name = "Tenant 0599",
            Hoofdgemeente = "H0599",
            GemeenteCode = "0599",
            TenantCode = "9446",
            EnvironmentName = "xyz",
        },
        new()
        {
            Name = "Tenant 0518",
            Hoofdgemeente = "H0518",
            GemeenteCode = "0518",
            TenantCode = "9446",
            EnvironmentName = "xyz",
        },
    ];

    [SetUp]
    public void RaceConditionTestSetup()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["Tenants:9446:Environments:xyz:Defaults:tenantcode"] = "9446",
            ["Tenants:9446:Environments:xyz:Defaults:environmentname"] = "xyz",
            ["Tenants:9446:Environments:xyz:Defaults:kcm:isenabled"] = "false",
            ["Tenants:9446:Environments:xyz:Gemeenten:0599:identifier"] = "9446-xyz-0599",
            ["Tenants:9446:Environments:xyz:Gemeenten:0599:name"] = "Tenant 0599",
            ["Tenants:9446:Environments:xyz:Gemeenten:0599:gemeentecode"] = "0599",
            ["Tenants:9446:Environments:xyz:Gemeenten:0599:hoofdgemeente"] = "H0599",
            ["Tenants:9446:Environments:xyz:Gemeenten:0518:identifier"] = "9446-xyz-0518",
            ["Tenants:9446:Environments:xyz:Gemeenten:0518:name"] = "Tenant 0518",
            ["Tenants:9446:Environments:xyz:Gemeenten:0518:hoofdgemeente"] = "H0518",
            ["Tenants:9446:Environments:xyz:Gemeenten:0518:gemeentecode"] = "0518",
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IConfiguration>(configuration);
        serviceCollection.AddWigo4itMultiTenant(NServiceBusTenantIdResolver.DetermineTenantIdentifier)
            .ConfigurePerTenant<TestOptions, Wigo4itTenantInfo>((opt, tenant) =>
            {
                opt.Name = tenant.Name;
                opt.Identifier = tenant.Identifier;
                opt.Hoofdgemeente = tenant.Hoofdgemeente;
                opt.GemeenteCode = tenant.GemeenteCode;
            });
        
        _services = serviceCollection.BuildServiceProvider();
    }

    [TearDown]
    public void TearDown()
    {
        _services?.Dispose();
    }

    [Test]
    public async Task Options_should_reflect_current_tenant_under_high_concurrency_with_IOptions()
    {
        await Test_concurrency<IOptions<TestOptions>>(omgevingOptions => omgevingOptions.Value.Hoofdgemeente);
    }

    [Test]
    public async Task Options_should_reflect_current_tenant_under_high_concurrency_with_IOptionsSnapshot()
    {
        await Test_concurrency<IOptionsSnapshot<TestOptions>>(omgevingOptions => omgevingOptions.Value.Hoofdgemeente);
    }

    [Test]
    public async Task Options_should_reflect_current_tenant_under_high_concurrency_with_IOptionsMonitor()
    {
        await Test_concurrency<IOptionsMonitor<TestOptions>>(omgevingOptions => omgevingOptions.CurrentValue.Hoofdgemeente);
    }

    private async Task Test_concurrency<TOptions>(Func<TOptions, string?> tester)
        where TOptions : notnull
    {
        var results = new ConcurrentBag<(string hoofdgemeente, string? value)>();
        const int concurrencyLevel = 100;

        await Parallel.ForAsync(
            0,
            concurrencyLevel,
            new ParallelOptions { MaxDegreeOfParallelism = concurrencyLevel },
            async (i, _) =>
            {
                var tenant = _expectedValues[i % _expectedValues.Count];
                await ResolveTenantAndExecute(
                    tenant,
                    () =>
                    {
                        using var scope = _services!.CreateScope();
                        var options = scope.ServiceProvider.GetRequiredService<TOptions>();

                        var value = tester(options);

                        results.Add((tenant.Hoofdgemeente, value));
                        return Task.CompletedTask;
                    }
                );
            }
        );

        var mismatches = results.Where(r => r.value != r.hoofdgemeente).ToList();
        if (mismatches.Any())
        {
            var message =
                $"Found {mismatches.Count} mismatches out of {results.Count} results.\n"
                + string.Join("\n", mismatches.Select(m => $"Tenant: {m.hoofdgemeente}, Value: {m.value}"));
            Assert.Fail(message);
        }
    }

    private async Task ResolveTenantAndExecute(Wigo4itTenantInfo tenant, Func<Task> next)
    {
        var incomingContext = new MessageContextWithServiceProvider(_services!);

        incomingContext.Message.Headers.Add(MultitenancyHeaders.WegwijzerTenantCode, tenant.TenantCode);
        incomingContext.Message.Headers.Add(MultitenancyHeaders.WegwijzerEnvironmentName, tenant.EnvironmentName);
        incomingContext.Message.Headers.Add(MultitenancyHeaders.GemeenteCode, tenant.GemeenteCode);

        await new MultiTenantBehavior(_ => { }).Invoke(incomingContext, next);
    }

    private class MessageContextWithServiceProvider(IServiceProvider serviceProvider) : TestableIncomingPhysicalMessageContext
    {
        protected override IServiceProvider GetBuilder()
        {
            return serviceProvider;
        }
    }
}

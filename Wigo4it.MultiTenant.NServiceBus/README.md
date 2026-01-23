# Wigo4it.MultiTenant.NServiceBus

NServiceBus-integratie voor Wigo4it.MultiTenant. De library resolveert tenants uit berichtheaders, zet de tenantcontext voor handlers en stuurt headers standaard door naar uitgaande berichten.

## Installatie

```bash
dotnet add package Wigo4it.MultiTenant.NServiceBus
```

(Voor web/worker services zonder NServiceBus gebruik je alleen `Wigo4it.MultiTenant`.)

## Setup in een endpoint

1. **Registreren van multi-tenant services**

```csharp
using Wigo4it.MultiTenant;
using Wigo4it.MultiTenant.NServiceBus;

builder.Services.AddWigo4itMultiTenant<MyTenantInfo>(
    NServiceBusTenantIdResolver.DetermineTenantIdentifier
);

builder.Services.ConfigurePerTenant<MyTenantOptions, MyTenantInfo>((options, tenant) =>
{
    options.CustomSetting = tenant.CustomSetting;
    options.FeatureEnabled = tenant.FeatureEnabled;
});
```

2. **Pipeline behavior activeren**

```csharp
public static EndpointConfiguration CreateEndpointConfiguration(HostBuilderContext context)
{
    var endpointConfiguration = new EndpointConfiguration("MyEndpoint");

    // overige endpoint configuratie ...
    endpointConfiguration.Pipeline.RegisterWigo4ItMultiTenantBehavior();

    return endpointConfiguration;
}

builder.Host.UseNServiceBus(context => CreateEndpointConfiguration(context));
```

3. **Optioneel: callback voor logging/telemetrie**

```csharp
endpointConfiguration.Pipeline.RegisterWigo4ItMultiTenantBehavior(tenantContext =>
{
    var tenantInfo = tenantContext.TenantInfo;
    Console.WriteLine($"Processing message for tenant: {tenantInfo.Identifier}");
});
```

## Handler met tenant context

```csharp
public class MyMessageHandler : IHandleMessages<MyMessage>
{
    private readonly ILogger<MyMessageHandler> _logger;
    private readonly IMultiTenantContextAccessor<MyTenantInfo> _tenantContextAccessor;
    private readonly MyTenantOptions _options;

    public MyMessageHandler(
        ILogger<MyMessageHandler> logger,
        IMultiTenantContextAccessor<MyTenantInfo> tenantContextAccessor,
        IOptions<MyTenantOptions> options)
    {
        _logger = logger;
        _tenantContextAccessor = tenantContextAccessor;
        _options = options.Value;
    }

    public Task Handle(MyMessage message, IMessageHandlerContext context)
    {
        var tenantInfo = _tenantContextAccessor.MultiTenantContext?.TenantInfo;

        _logger.LogInformation(
            "Processing message for tenant {TenantId} with setting {Setting}",
            tenantInfo?.Identifier,
            _options.CustomSetting);

        // Gebruik tenant-specifieke configuratie
        var connectionString = tenantInfo?.ConnectionString;

        return Task.CompletedTask;
    }
}
```

## Berichten versturen met headers

**Vanuit een ASP.NET Core endpoint**

```csharp
app.MapPost("/send/{tenantCode}/{environmentName}/{gemeenteCode}",
    async (string tenantCode, string environmentName, string gemeenteCode,
           IMessageSession messageSession) =>
    {
        var sendOptions = new SendOptions();
        sendOptions.RouteToThisEndpoint();

        sendOptions.SetHeader(MultitenancyHeaders.WegwijzerTenantCode, tenantCode);
        sendOptions.SetHeader(MultitenancyHeaders.WegwijzerEnvironmentName, environmentName);
        sendOptions.SetHeader(MultitenancyHeaders.GemeenteCode, gemeenteCode);

        await messageSession.Send(new MyMessage { /* payload */ }, sendOptions);
        return Results.Accepted();
    });
```

**Vanuit een handler** (headers worden standaard gekopieerd, overschrijf ze voor een andere tenant):

```csharp
public async Task Handle(MyMessage message, IMessageHandlerContext context)
{
    var sendOptions = new SendOptions();
    sendOptions.SetDestination("AnotherQueue");

    sendOptions.SetHeader(MultitenancyHeaders.WegwijzerTenantCode, "9999");
    sendOptions.SetHeader(MultitenancyHeaders.WegwijzerEnvironmentName, "prod");
    sendOptions.SetHeader(MultitenancyHeaders.GemeenteCode, "1234");

    await context.Send(new AnotherMessage { /* payload */ }, sendOptions);
}
```

## Voorbeeldapp draaien

```bash
cd Wigo4it.MultiTenant.NServiceBus.Sample
dotnet run

# In een andere terminal
curl -X POST http://localhost:5000/send/9446/dev/0599
```

De handler verwerkt het bericht in de context van tenant `9446-dev-0599`.

## Best practices

- Gebruik `ConfigurePerTenant` om alleen de benodigde properties naar options te mappen; injecteer `IOptions<T>` in handlers/services.
- Registreer de pipeline behavior één keer per endpoint.
- Log de geresolveerde tenant (bijvoorbeeld via de callback) voor diagnostiek.
- Zet tenant headers altijd op uitgaande berichten als je naar een andere tenant routeert.

## Troubleshooting

- **`InvalidOperationException: Tenant could not be resolved`**: controleer dat alle drie de headers aanwezig zijn en dat de identifier in configuratie bestaat.
- **Options bevatten unexpected waarden**: check de mapping en lowercase propertynamen in JSON; vermijd `IOptionsMonitor<T>` voor scoped services.
- **Race conditions**: gebruik `IOptions<T>` of `IOptionsSnapshot<T>` en vermijd singletons die tenant-specifieke state bijhouden.

## Tests en referenties

- Race-condition tests: [Wigo4it.MultiTenant.NserviceBus.Tests](../Wigo4it.MultiTenant.NserviceBus.Tests).
- Integratietests: [Wigo4it.MultiTenant.NServiceBus.IntegrationTests](../Wigo4it.MultiTenant.NServiceBus.IntegrationTests).
- Voorbeeldapp: [Wigo4it.MultiTenant.NServiceBus.Sample](../Wigo4it.MultiTenant.NServiceBus.Sample).

# Wigo4it.MultiTenant.NServiceBus

NServiceBus-integratie voor Wigo4it.MultiTenant. De library resolved tenants uit berichtheaders, zet de tenantcontext voor handlers en stuurt headers standaard door naar uitgaande berichten.

## Installatie

```bash
dotnet add package Wigo4it.MultiTenant.NServiceBus
```

## Setup in een endpoint

1. **Registreren van multi-tenant services**

```csharp
using Wigo4it.MultiTenant;
using Wigo4it.MultiTenant.NServiceBus;

builder.Services.AddWigo4itMultiTenant<MyTenantInfo>(
    NServiceBusTenantIdResolver.DetermineTenantIdentifier
);
builder.Services.AddWigo4itMultiTenantNServiceBus();

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
    endpointConfiguration.UseWigo4itMultiTenant();

    return endpointConfiguration;
}

builder.Host.UseNServiceBus(context => CreateEndpointConfiguration(context));
```

1. **Optioneel: callback**

```csharp
endpointConfiguration.UseWigo4itMultiTenant(tenantContext =>
{
    var tenantInfo = tenantContext.TenantInfo;
    Console.WriteLine($"Processing message for tenant: {tenantInfo.Identifier}");

    // Of bijvoorbeeld om de globale configuratie van Socrates.Core.Configuratie te zetten:
    global::Socrates.Core.Configuratie.OmgevingContext.Naam = tenantContext.TenantInfo!.Identifier;
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

Let op: gebruik bij voorkeur `IOptions<>` om tenant-specifieke configuratie in je handler te benaderen in plaats van `IMultiTenantContextAccessor<>` zodat handlers niet afhankelijk worden van multi-tenant configuratiecode. `IMultiTenantContextAccessor<>` is wel beschikbaar, mocht het toch nodig zijn.

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

**Vanuit een handler** (headers worden standaard gekopieerd vanaf het inkomende message, dus hoef je niet expliciet te zetten):

```csharp
public async Task Handle(MyMessage message, IMessageHandlerContext context)
{
    await context.SendLocal(new AnotherMessage { /* payload */ });
}
```
## Best practices

- Gebruik `ConfigurePerTenant` om alleen de benodigde properties naar options te mappen; injecteer `IOptions<T>` in handlers/services.
- Log de resolved tenant (bijvoorbeeld via de callback) voor diagnostiek.

## Troubleshooting

- **`InvalidOperationException: Tenant could not be resolved`**: controleer dat alle drie de headers aanwezig zijn en dat de identifier in configuratie bestaat.
- **Options bevatten unexpected waarden**: check de mapping en lowercase propertynamen in JSON; vermijd `IOptionsMonitor<T>` voor scoped services.
- **Race conditions**: gebruik `IOptions<T>` of `IOptionsSnapshot<T>` en vermijd singletons die tenant-specifieke state bijhouden.

## Tests en referenties

- Race-condition tests: [Wigo4it.MultiTenant.NserviceBus.Tests](../Wigo4it.MultiTenant.NserviceBus.Tests).
- Integratietests: [Wigo4it.MultiTenant.NServiceBus.IntegrationTests](../Wigo4it.MultiTenant.NServiceBus.IntegrationTests).
- Voorbeeldapp: [Wigo4it.MultiTenant.NServiceBus.Sample](../Wigo4it.MultiTenant.NServiceBus.Sample).

# Wigo4it.MultiTenant.NServiceBus

NServiceBus integratie voor Wigo4it.MultiTenant. Biedt multi-tenant message handling met automatische tenant resolution uit message headers.

## Installatie

```bash
dotnet add package Wigo4it.MultiTenant.NServiceBus
```

**Let op**: Deze package heeft `Wigo4it.MultiTenant` als dependency. Zorg dat je de basis multi-tenant concepten begrijpt voordat je deze package gebruikt.

## Belangrijkste Concepten

Deze package breidt de basis `Wigo4it.MultiTenant` functionaliteit uit met NServiceBus-specifieke features:

- **Automatische tenant resolution** uit message headers
- **MultiTenantBehavior** voor de NServiceBus pipeline
- **Tenant context propagatie** tussen message handlers
- **Integratie met IOptions<>** voor tenant-specifieke configuratie

### Tenant Headers

Messages moeten de volgende headers bevatten:
- `Wigo4it.Wegwijzer.TenantCode.Forwardable`
- `Wigo4it.Wegwijzer.EnvironmentName.Forwardable`
- `Wigo4it.Socrates.GemeenteCode.Forwardable`

Deze headers worden gebruikt om de tenant te identificeren voordat de message handler wordt aangeroepen.

## Setup in NServiceBus Endpoint

### Stap 1: Configureer MultiTenant Services

```csharp
using Wigo4it.MultiTenant;
using Wigo4it.MultiTenant.NServiceBus;

// In Program.cs
builder.Services.AddWigo4itMultiTenant<MyTenantInfo>(
    NServiceBusTenantIdResolver.DetermineTenantIdentifier
);

// Configureer tenant-specifieke options
builder.Services.ConfigurePerTenant<MyTenantOptions, MyTenantInfo>((options, tenant) =>
{
    options.CustomSetting = tenant.CustomSetting;
    options.FeatureEnabled = tenant.FeatureEnabled;
});
```

### Stap 2: Registreer MultiTenant Behavior in NServiceBus Pipeline

```csharp
public static EndpointConfiguration CreateEndpointConfiguration(HostBuilderContext context)
{
    var endpointConfiguration = new EndpointConfiguration("MyEndpoint");
    
    // ... andere endpoint configuratie ...
    
    // Registreer multi-tenant behavior
    endpointConfiguration.Pipeline.RegisterWigo4ItMultiTenantBehavior();
    
    return endpointConfiguration;
}

// In Program.cs
builder.Host.UseNServiceBus(context => CreateEndpointConfiguration(context));
```

**Optioneel Callback**: Je kunt een callback meegeven om te worden geïnformeerd wanneer een tenant is resolved:

```csharp
endpointConfiguration.Pipeline.RegisterWigo4ItMultiTenantBehavior(tenantContext =>
{
    // Wordt aangeroepen nadat tenant is resolved
    var tenantInfo = tenantContext.TenantInfo;
    Console.WriteLine($"Processing message for tenant: {tenantInfo.Identifier}");
});
```

## Message Handler met Tenant Context

```csharp
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Options;

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
        
        // Gebruik tenant-specifieke connection string
        var connectionString = tenantInfo?.ConnectionString;
        
        // ... verwerk message ...
        
        return Task.CompletedTask;
    }
}
```

## Berichten Versturen met Tenant Headers

### Vanuit een ASP.NET Core Endpoint

```csharp
app.MapPost("/send/{tenantCode}/{environmentName}/{gemeenteCode}",
    async (string tenantCode, string environmentName, string gemeenteCode, 
           IMessageSession messageSession) =>
    {
        var sendOptions = new SendOptions();
        sendOptions.RouteToThisEndpoint(); // of .SetDestination("DestinationQueue")
        
        // Zet tenant headers
        sendOptions.SetHeader(MultitenancyHeaders.WegwijzerTenantCode, tenantCode);
        sendOptions.SetHeader(MultitenancyHeaders.WegwijzerEnvironmentName, environmentName);
        sendOptions.SetHeader(MultitenancyHeaders.GemeenteCode, gemeenteCode);

        await messageSession.Send(new MyMessage { ... }, sendOptions);
        
        return Results.Accepted();
    });
```

### Vanuit een Message Handler

```csharp
public async Task Handle(MyMessage message, IMessageHandlerContext context)
{
    var tenantInfo = _tenantContextAccessor.MultiTenantContext?.TenantInfo;
    
    var sendOptions = new SendOptions();
    sendOptions.SetDestination("AnotherQueue");
    
    // Headers worden automatisch gekopieerd van het inkomende bericht
    // Als je naar een nieuwe tenant wilt sturen, override de headers:
    sendOptions.SetHeader(MultitenancyHeaders.WegwijzerTenantCode, "9999");
    sendOptions.SetHeader(MultitenancyHeaders.WegwijzerEnvironmentName, "prod");
    sendOptions.SetHeader(MultitenancyHeaders.GemeenteCode, "1234");
    
    await context.Send(new AnotherMessage { ... }, sendOptions);
}
```

**Belangrijk**: NServiceBus kopieert standaard headers door naar uitgaande berichten, inclusief de tenant headers.

## Voorbeeld

Zie de `Wigo4it.MultiTenant.NServiceBus.Sample` folder in de repository voor een werkend voorbeeld met:
- Volledige endpoint configuratie
- Message handler met tenant context
- Tenant-specifieke options configuratie
- Voorbeeld appsettings.json met meerdere tenants
- HTTP endpoints voor het versturen van berichten

### Sample Uitvoeren

```bash
cd Wigo4it.MultiTenant.NServiceBus.Sample
dotnet run

# In een andere terminal, stuur een bericht:
curl -X POST http://localhost:5000/send/9446/dev/0599
```

De handler zal het bericht verwerken met de tenant context voor "9446-dev-0599".

## Best Practices

### 1. Valideer altijd tenant resolution

```csharp
public class MyMessageHandler : IHandleMessages<MyMessage>
{
    public Task Handle(MyMessage message, IMessageHandlerContext context)
    {
        var tenantInfo = _tenantContextAccessor.MultiTenantContext?.TenantInfo;
        
        if (tenantInfo == null)
        {
            throw new InvalidOperationException("Tenant context is not available");
        }
        
        // ... verwerk message ...
    }
}
```

De `MultiTenantBehavior` gooit al een exception als de tenant niet kan worden geresolved, maar extra validatie in je handler kan helpen bij debugging.

### 2. Test met meerdere tenants

Schrijf tests die concurrent berichten verwerken voor verschillende tenants om race conditions te detecteren:

```csharp
[Test]
public async Task Should_handle_concurrent_messages_for_different_tenants()
{
    var tasks = new[]
    {
        SendMessageForTenant("9446-dev-0599"),
        SendMessageForTenant("9446-dev-0518"),
        SendMessageForTenant("9446-dev-0599"),
        SendMessageForTenant("9446-dev-0518"),
    };
    
    await Task.WhenAll(tasks);
    
    // Valideer dat elke message correct is verwerkt met de juiste tenant context
}
```

Zie `RaceConditionTests.cs` in de test projecten voor complete voorbeelden.

### 3. Gebruik IOptions<> voor tenant-specifieke configuratie

In plaats van directe toegang tot `TenantInfo`, gebruik `IOptions<>` patterns zoals beschreven in de `Wigo4it.MultiTenant` documentatie. Dit zorgt voor betere separation of concerns en makkelijker testen.

### 4. Zorg voor correcte service lifetimes

Services die `IOptions<T>` gebruiken met tenant-specifieke waarden moeten **scoped** of **transient** zijn, niet singleton:

```csharp
// ✅ Correct
builder.Services.AddScoped<MyService>();

// ❌ Vermijd dit voor services met tenant-specifieke dependencies
builder.Services.AddSingleton<MyService>();
```

## Troubleshooting

### Tenant kan niet worden geresolved

**Probleem:** `InvalidOperationException: Tenant could not be resolved.`

**Oplossingen:**
1. Controleer dat alle drie de headers aanwezig zijn in het bericht
2. Valideer dat de tenant identifier bestaat in de configuratie
3. Controleer dat `RegisterWigo4ItMultiTenantBehavior()` is aangeroepen
4. Valideer dat `NServiceBusTenantIdResolver.DetermineTenantIdentifier` is gebruikt bij setup

### Race conditions in concurrent scenarios

**Probleem:** Options bevatten waarden van verkeerde tenant bij hoge concurrency

**Oplossingen:**
1. Gebruik `IOptions<T>` of `IOptionsSnapshot<T>` in plaats van `IOptionsMonitor<T>` voor scoped services
2. Zorg dat services scoped of transient zijn (niet singleton) als ze `IOptions<T>` gebruiken
3. Valideer met de RaceConditionTests uit de test projecten

### Headers worden niet doorgegeven

**Probleem:** Uitgaande berichten bevatten geen tenant headers

**Oplossing:**
NServiceBus kopieert headers standaard door. Als headers niet worden doorgegeven, controleer dan:
1. Of de headers correct zijn gezet op het originele bericht
2. Of je niet per ongeluk SendOptions gebruikt die headers overschrijven
3. Gebruik `context.Send()` in plaats van `messageSession.Send()` vanuit een handler voor automatische header propagatie

## Veelgestelde Vragen

### Worden tenant headers automatisch doorgestuurd naar andere services?

Ja, NServiceBus kopieert headers standaard door naar uitgaande berichten. Dit betekent dat als je vanuit een handler een nieuw bericht stuurt, de tenant headers automatisch worden meegestuurd.

Als je naar een andere tenant wilt sturen, moet je de headers expliciet overschrijven:

```csharp
var sendOptions = new SendOptions();
sendOptions.SetHeader(MultitenancyHeaders.WegwijzerTenantCode, "9999");
sendOptions.SetHeader(MultitenancyHeaders.WegwijzerEnvironmentName, "prod");
sendOptions.SetHeader(MultitenancyHeaders.GemeenteCode, "1234");
await context.Send(message, sendOptions);
```

### Kan ik de tenant ID formaat aanpassen?

Ja, maar je moet dan ook `NServiceBusTenantIdResolver.DetermineTenantIdentifier` aanpassen of een eigen resolver implementeren:

```csharp
builder.Services.AddWigo4itMultiTenant<MyTenantInfo>(async context =>
{
    var messageContext = (IIncomingPhysicalMessageContext)context;
    // Custom logic om tenant ID te bepalen
    return "custom-tenant-id-format";
});
```

### Hoe test ik message handlers met tenant context?

Gebruik de NServiceBus.Testing library en mock de tenant context:

```csharp
[Test]
public async Task Should_process_message_with_tenant_context()
{
    // Arrange
    var tenantContext = new Mock<IMultiTenantContextAccessor<MyTenantInfo>>();
    tenantContext.Setup(x => x.MultiTenantContext.TenantInfo)
        .Returns(new MyTenantInfo { Identifier = "9446-dev-0599" });
    
    var handler = new MyMessageHandler(tenantContext.Object);
    
    // Act
    await handler.Handle(new MyMessage(), new TestableMessageHandlerContext());
    
    // Assert
    // ...
}
```

## Afhankelijkheden

- **Wigo4it.MultiTenant** - Basis multi-tenant functionaliteit
- **NServiceBus** - Message bus framework
- **Finbuckle.MultiTenant** - Basis multi-tenancy framework
- **.NET 8.0** of hoger

## Licentie

Zie de LICENSE file in de repository voor licentie informatie.

## Support

Voor vragen of problemen, neem contact op met het Wigo4it development team.

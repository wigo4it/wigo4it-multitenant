# Wigo4it.MultiTenant

Multi-tenant ondersteuning voor Wigo4it applicaties, gebouwd op [Finbuckle.MultiTenant](https://github.com/Finbuckle/Finbuckle.MultiTenant). Deze libraries bieden een gestandaardiseerde manier om multi-tenancy te implementeren in zowel web applicaties als NServiceBus message handlers.

## Overzicht

De Wigo4it.MultiTenant libraries bestaan uit twee hoofdcomponenten:

- **Wigo4it.MultiTenant** - Basis multi-tenant functionaliteit met configuratie-gebaseerde tenant opslag
- **Wigo4it.MultiTenant.NServiceBus** - NServiceBus integratie voor multi-tenant message handling

## Belangrijkste Concepten

### Tenant Identificatie

Een tenant wordt geïdentificeerd door een combinatie van drie waarden:
- **TenantCode** - 4-cijferige Wegwijzer tenantcode (bijv. "9446")
- **EnvironmentName** - Wegwijzer omgevingsnaam (bijv. "dev", "prod")
- **GemeenteCode** - 4-cijferige gemeentecode (bijv. "0599", "0518")

Deze worden gecombineerd tot een unieke identifier in het formaat: `{TenantCode}-{EnvironmentName}-{GemeenteCode}` (bijv. "9446-dev-0599")

### Headers

Voor communicatie tussen services worden de volgende headers gebruikt:
- `Wigo4it.Wegwijzer.TenantCode.Forwardable`
- `Wigo4it.Wegwijzer.EnvironmentName.Forwardable`
- `Wigo4it.Socrates.GemeenteCode.Forwardable`

Deze zijn beschikbaar via de statische class `MultitenancyHeaders`.

## Wigo4it.MultiTenant

### Installatie

```bash
dotnet add package Wigo4it.MultiTenant
```

### Basis Setup

```csharp
using Wigo4it.MultiTenant;

// In Program.cs of Startup.cs
builder.Services.AddWigo4itMultiTenant<YourTenantInfo>(tenantIdentifierResolver);
```

### Configuratie Structuur

De tenant configuratie volgt een hiërarchische structuur in `appsettings.json`:

```json
{
  "Tenants": {
    "{TenantCode}": {
      "Environments": {
        "{EnvironmentName}": {
          "Defaults": {
            "tenantcode": "9446",
            "environmentname": "dev",
            "connectionstring": "Host=localhost;Database=demo;...",
            "CustomProperty": "Default waarde voor alle gemeenten"
          },
          "Gemeenten": {
            "{GemeenteCode}": {
              "identifier": "9446-dev-0599",
              "name": "Gemeente 0599",
              "gemeentecode": "0599",
              "hoofdgemeente": "H0599",
              "CustomProperty": "Overschreven waarde voor deze gemeente"
            }
          }
        }
      }
    }
  }
}
```

#### Configuratie Hiërarchie

De configuratie werkt met een **defaults-override patroon**:

1. **Defaults sectie** - Bevat waarden die gelden voor alle gemeenten binnen een omgeving
2. **Gemeenten sectie** - Bevat gemeente-specifieke overschrijvingen

Properties die in de `Gemeenten` sectie worden gedefinieerd overschrijven automatisch de waarden uit `Defaults`.

### Custom TenantInfo Class

Maak een eigen TenantInfo class die erft van `Wigo4itTenantInfo`:

```csharp
public record MyTenantInfo : Wigo4itTenantInfo
{
    public string? CustomSetting { get; set; }
    public bool FeatureEnabled { get; set; }
    public MyComplexObject ComplexProperty { get; set; } = null!;
}

public class MyComplexObject
{
    public string SomeProperty { get; set; }
    public int AnotherProperty { get; set; }
}
```

**Belangrijk**: De properties in je TenantInfo class worden automatisch gebonden aan de configuratie. Gebruik lowercase property namen in de JSON configuratie (convention van .NET Configuration binding).

### Tenant-specifieke IOptions<> Configuratie

Dit is een van de belangrijkste features: je kunt `IOptions<T>` classes maken die automatisch tenant-specifieke waarden krijgen.

#### Stap 1: Definieer je Options Class

```csharp
public class MyTenantOptions
{
    public string? CustomSetting { get; set; }
    public bool FeatureEnabled { get; set; }
    public string? ApiEndpoint { get; set; }
}
```

#### Stap 2: Configureer Options Mapping

```csharp
using Finbuckle.MultiTenant;

builder.Services.AddWigo4itMultiTenant<MyTenantInfo>(tenantIdentifierResolver);

// Map TenantInfo properties naar Options
builder.Services.ConfigurePerTenant<MyTenantOptions, MyTenantInfo>((options, tenantInfo) =>
{
    options.CustomSetting = tenantInfo.CustomSetting;
    options.FeatureEnabled = tenantInfo.FeatureEnabled;
    options.ApiEndpoint = tenantInfo.ComplexProperty?.ApiUrl;
});
```

#### Stap 3: Gebruik Options in je Code

```csharp
using Microsoft.Extensions.Options;

public class MyService
{
    private readonly MyTenantOptions _options;

    public MyService(IOptions<MyTenantOptions> options)
    {
        _options = options.Value;
    }

    public void DoSomething()
    {
        // _options.CustomSetting bevat nu de waarde voor de huidige tenant
        Console.WriteLine($"Custom setting: {_options.CustomSetting}");
    }
}
```

**Opties voor Options Injection**:
- `IOptions<T>` - Voor scoped/transient services (aanbevolen)
- `IOptionsSnapshot<T>` - Voor scoped services met snapshot semantiek
- `IOptionsMonitor<T>` - Voor singletons (gebruikt `.CurrentValue`)

### Toegang tot TenantInfo

```csharp
using Finbuckle.MultiTenant.Abstractions;

public class MyHandler
{
    private readonly IMultiTenantContextAccessor<MyTenantInfo> _tenantContextAccessor;

    public MyHandler(IMultiTenantContextAccessor<MyTenantInfo> tenantContextAccessor)
    {
        _tenantContextAccessor = tenantContextAccessor;
    }

    public void DoWork()
    {
        var tenantInfo = _tenantContextAccessor.MultiTenantContext?.TenantInfo;
        if (tenantInfo != null)
        {
            Console.WriteLine($"Tenant: {tenantInfo.Identifier}");
            Console.WriteLine($"Connection: {tenantInfo.ConnectionString}");
        }
    }
}
```

**Best Practice**: Gebruik altijd `IOptions<T>` in plaats van directe toegang tot `TenantInfo` waar mogelijk. Map alleen de properties die je nodig hebt naar je Options class.

### Voorbeeld Configuratie

```json
{
  "Tenants": {
    "9446": {
      "Environments": {
        "dev": {
          "Defaults": {
            "tenantcode": "9446",
            "environmentname": "dev",
            "connectionstring": "Host=localhost;Database=demo;Username=demo;Password=demo",
            "CustomSetting": "Default waarde",
            "FeatureEnabled": "true",
            "ComplexProperty": {
              "ApiUrl": "https://api-dev.example.com",
              "Timeout": "30"
            }
          },
          "Gemeenten": {
            "0599": {
              "identifier": "9446-dev-0599",
              "name": "Gemeente Den Haag",
              "gemeentecode": "0599",
              "hoofdgemeente": "H0599"
            },
            "0518": {
              "identifier": "9446-dev-0518",
              "name": "Gemeente Amsterdam",
              "gemeentecode": "0518",
              "hoofdgemeente": "H0518",
              "CustomSetting": "Overschreven voor Amsterdam",
              "ComplexProperty": {
                "ApiUrl": "https://api-amsterdam.example.com"
              }
            }
          }
        }
      }
    }
  }
}
```

In dit voorbeeld:
- Gemeente 0599 krijgt alle default waarden
- Gemeente 0518 overschrijft `CustomSetting` en de `ApiUrl` in `ComplexProperty`
- Gemeente 0518 erft wel de `Timeout` waarde van `ComplexProperty` uit Defaults

## Wigo4it.MultiTenant.NServiceBus

### Installatie

```bash
dotnet add package Wigo4it.MultiTenant.NServiceBus
```

### Setup in NServiceBus Endpoint

#### Stap 1: Configureer MultiTenant Services

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

#### Stap 2: Registreer MultiTenant Behavior in NServiceBus Pipeline

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

### Message Handler met Tenant Context

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

### Berichten Versturen met Tenant Headers

#### Vanuit een ASP.NET Core Endpoint

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

#### Vanuit een Message Handler

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

## Complete Voorbeeld

Zie de `Wigo4it.MultiTenant.NServiceBus.Sample` folder voor een werkend voorbeeld met:
- Volledige endpoint configuratie
- Message handler met tenant context
- Tenant-specifieke options configuratie
- Voorbeeld appsettings.json met meerdere tenants

### Sample Structuur

```
Wigo4it.MultiTenant.NServiceBus.Sample/
├── Program.cs                      # Web host en endpoint setup
├── SampleEndpointConfiguration.cs  # NServiceBus configuratie
├── SampleServices.cs               # Service registratie
├── SampleTenantInfo.cs            # Custom TenantInfo class
├── SampleTenantOptions.cs         # Options class voor tenant settings
├── SampleMessageHandler.cs        # Message handler met tenant context
├── appsettings.json               # Tenant configuratie
└── requests.http                  # Voorbeeld HTTP requests
```

### Sample Uitvoeren

```bash
cd Wigo4it.MultiTenant.NServiceBus.Sample
dotnet run

# In een andere terminal, stuur een bericht:
curl -X POST http://localhost:5000/send/9446/dev/0599
```

De handler zal het bericht verwerken met de tenant context voor "9446-dev-0599".

## Testing

### Unit Tests

Zie `Wigo4it.MultiTenant.Tests` voor voorbeelden van:
- DictionaryConfigurationStore tests
- Configuratie hierarchie tests (defaults en overrides)
- Tenant resolution tests

### Race Condition Tests

Zie `Wigo4it.MultiTenant.NserviceBus.Tests` voor:
- Concurrency tests met meerdere tenants
- Validatie dat IOptions correct tenant-specifiek blijft onder hoge load
- Tests voor IOptions, IOptionsSnapshot en IOptionsMonitor

### Integration Tests

Zie `Wigo4it.MultiTenant.NServiceBus.IntegrationTests` voor:
- End-to-end tests met echte NServiceBus berichten
- Tests voor tenant resolution in message pipeline
- Configuratie override tests

## Best Practices

### 1. Gebruik IOptions<T> in plaats van directe TenantInfo toegang

❌ **Vermijd dit:**
```csharp
public class MyService
{
    public MyService(IMultiTenantContextAccessor<MyTenantInfo> accessor)
    {
        var setting = accessor.MultiTenantContext?.TenantInfo?.CustomSetting;
    }
}
```

✅ **Doe dit:**
```csharp
public class MyService
{
    public MyService(IOptions<MyTenantOptions> options)
    {
        var setting = options.Value.CustomSetting;
    }
}
```

**Voordelen:**
- Expliciete afhankelijkheden
- Gemakkelijker te testen
- Type-safe toegang tot settings
- Betere separation of concerns

### 2. Map alleen benodigde properties naar Options

Maak specifieke Options classes voor verschillende doeleinden:

```csharp
// Voor database toegang
public class DatabaseOptions
{
    public string ConnectionString { get; set; }
}

// Voor feature flags
public class FeatureOptions
{
    public bool NewFeatureEnabled { get; set; }
}

// Voor externe service configuratie
public class ExternalApiOptions
{
    public string ApiUrl { get; set; }
    public int TimeoutSeconds { get; set; }
}
```

### 3. Gebruik Defaults om duplicatie te vermijden

Plaats gemeenschappelijke waarden in de `Defaults` sectie en override alleen waar nodig:

```json
{
  "Tenants": {
    "9446": {
      "Environments": {
        "dev": {
          "Defaults": {
            "connectionstring": "Host=dev-db.example.com;...",
            "ApiUrl": "https://api-dev.example.com",
            "Timeout": "30"
          },
          "Gemeenten": {
            "0599": {
              "identifier": "9446-dev-0599",
              "gemeentecode": "0599"
            },
            "0518": {
              "identifier": "9446-dev-0518",
              "gemeentecode": "0518",
              "connectionstring": "Host=special-db.example.com;..."
            }
          }
        }
      }
    }
  }
}
```

### 4. Valideer altijd tenant resolution

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

### 5. Test met meerdere tenants

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

Zie `RaceConditionTests.cs` voor complete voorbeelden.

## Troubleshooting

### Tenant kan niet worden geresolved

**Probleem:** `InvalidOperationException: Tenant could not be resolved.`

**Oplossingen:**
1. Controleer dat alle drie de headers aanwezig zijn in het bericht
2. Valideer dat de tenant identifier bestaat in de configuratie
3. Controleer de case-sensitivity van property namen in de configuratie

### Options bevatten niet de verwachte waarden

**Probleem:** `IOptions<T>.Value` bevat null of default waarden

**Oplossingen:**
1. Controleer dat `ConfigurePerTenant` correct is aangeroepen
2. Valideer de property mapping in `ConfigurePerTenant`
3. Controleer dat de property namen in de configuratie lowercase zijn
4. Gebruik de debugger om `TenantInfo` te inspecteren en te zien welke waarden daar wel/niet zijn

### Configuratie wordt niet geladen

**Probleem:** `TenantInfo` properties zijn null of default

**Oplossingen:**
1. Controleer de configuratie structuur (Tenants > {Code} > Environments > {Name} > Defaults/Gemeenten)
2. Valideer dat property namen lowercase zijn in JSON
3. Controleer dat nested objecten correct zijn gestructureerd
4. Gebruik `BindNonPublicProperties = true` indien je private setters gebruikt (wordt automatisch gedaan)

### Race conditions in concurrent scenarios

**Probleem:** Options bevatten waarden van verkeerde tenant bij hoge concurrency

**Oplossingen:**
1. Gebruik `IOptions<T>` of `IOptionsSnapshot<T>` in plaats van `IOptionsMonitor<T>` voor scoped services
2. Zorg dat services scoped of transient zijn (niet singleton) als ze `IOptions<T>` gebruiken
3. Valideer met de RaceConditionTests uit de test projecten

## Veelgestelde Vragen

### Kan ik Azure App Configuration gebruiken in plaats van appsettings.json?

Ja! De `DictionaryConfigurationStore` werkt met elke .NET `IConfiguration` provider. Je kunt Azure App Configuration toevoegen:

```csharp
builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(connectionString)
           .Select(KeyFilter.Any)
           .ConfigureRefresh(refresh => { ... });
});
```

De tenant configuratie zal automatisch worden geladen vanuit Azure App Configuration.

### Kan ik meerdere TenantInfo types gebruiken?

Nee, per applicatie moet je één TenantInfo type kiezen. Gebruik nested objecten of extra properties om verschillende soorten configuratie op te slaan.

### Hoe werk ik met connection strings per gemeente?

Definieer de connection string in de `Defaults` en override waar nodig:

```json
{
  "Tenants": {
    "9446": {
      "Environments": {
        "prod": {
          "Defaults": {
            "connectionstring": "Host=prod-db.example.com;..."
          },
          "Gemeenten": {
            "0599": {
              "connectionstring": "Host=gemeente-0599-db.example.com;..."
            }
          }
        }
      }
    }
  }
}
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

## Afhankelijkheden

- **Finbuckle.MultiTenant** - Basis multi-tenancy framework
- **NServiceBus** (alleen voor NServiceBus package) - Message bus framework
- **.NET 8.0** of hoger

## Licentie

Zie de LICENSE file in de repository voor licentie informatie.

## Support

Voor vragen of problemen, neem contact op met het Wigo4it development team.

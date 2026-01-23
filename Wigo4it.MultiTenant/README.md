# Wigo4it.MultiTenant

Multi-tenant ondersteuning voor Wigo4it applicaties, gebouwd op [Finbuckle.MultiTenant](https://github.com/Finbuckle/Finbuckle.MultiTenant). Deze library biedt een gestandaardiseerde manier om multi-tenancy te implementeren met configuratie-gebaseerde tenant opslag.

## Installatie

```bash
dotnet add package Wigo4it.MultiTenant
```

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

## Basis Setup

```csharp
using Wigo4it.MultiTenant;

// In Program.cs of Startup.cs
builder.Services.AddWigo4itMultiTenant<YourTenantInfo>(tenantIdentifierResolver);
```

## Configuratie Structuur

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

### Configuratie Hiërarchie

De configuratie werkt met een **defaults-override patroon**:

1. **Defaults sectie** - Bevat waarden die gelden voor alle gemeenten binnen een omgeving
2. **Gemeenten sectie** - Bevat gemeente-specifieke overschrijvingen

Properties die in de `Gemeenten` sectie worden gedefinieerd overschrijven automatisch de waarden uit `Defaults`.

## Custom TenantInfo Class

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

## Tenant-specifieke IOptions<> Configuratie

Dit is een van de belangrijkste features: je kunt `IOptions<T>` classes maken die automatisch tenant-specifieke waarden krijgen.

### Stap 1: Definieer je Options Class

```csharp
public class MyTenantOptions
{
    public string? CustomSetting { get; set; }
    public bool FeatureEnabled { get; set; }
    public string? ApiEndpoint { get; set; }
}
```

### Stap 2: Configureer Options Mapping

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

### Stap 3: Gebruik Options in je Code

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

## Toegang tot TenantInfo

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

## Voorbeeld Configuratie

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

## Troubleshooting

### Tenant kan niet worden geresolved

**Probleem:** `InvalidOperationException: Tenant could not be resolved.`

**Oplossingen:**
1. Controleer dat de tenant identifier bestaat in de configuratie
2. Valideer de case-sensitivity van property namen in de configuratie
3. Controleer de configuratie structuur (Tenants > {Code} > Environments > {Name})

### Options bevatten niet de verwachte waarden

**Probleem:** `IOptions<T>.Value` bevat null of default waarden

**Oplossingen:**
1. Controleer dat `ConfigurePerTenant` correct is aangeroepen
2. Valideer de property mapping in `ConfigurePerTenant`
3. Controleer dat de property namen in de configuratie lowercase zijn
4. Gebruik de debugger om `TenantInfo` te inspecteren

### Configuratie wordt niet geladen

**Probleem:** `TenantInfo` properties zijn null of default

**Oplossingen:**
1. Controleer de configuratie structuur (Tenants > {Code} > Environments > {Name} > Defaults/Gemeenten)
2. Valideer dat property namen lowercase zijn in JSON
3. Controleer dat nested objecten correct zijn gestructureerd

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

## Afhankelijkheden

- **Finbuckle.MultiTenant** - Basis multi-tenancy framework
- **.NET 8.0** of hoger

## Licentie

Zie de LICENSE file in de repository voor licentie informatie.

## Support

Voor vragen of problemen, neem contact op met het Wigo4it development team.

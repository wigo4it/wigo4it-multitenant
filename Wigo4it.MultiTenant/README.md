# Wigo4it.MultiTenant

Basisbibliotheek voor multi-tenant Wigo4it applicaties, gebouwd op Finbuckle.MultiTenant. Biedt tenant-resolutie, configuratiebinding naar `Wigo4itTenantInfo` (of je eigen subtype) en per-tenant `IOptions<T>`.

## Installatie

```bash
dotnet add package Wigo4it.MultiTenant
```

## Basis setup

```csharp
using Wigo4it.MultiTenant;

// In Program.cs of Startup.cs
builder.Services.AddWigo4itMultiTenant<MyTenantInfo>(tenantIdentifierResolver);
```

`tenantIdentifierResolver` is een `Func<object, Task<string?>> tenantIdentifierResolver` die de Tenant Identifier bepaalt op basis van de request context. Het type van de context is afhankelijk van het type applicatie. Voor een ASP.Net app zal dit een `HttpContext` zijn, voor NServiceBus een `IIncomingPhysicalMessageContext`. 
Voorgedefinieerde `tenantIdentifierResolver`s per type applicatie zijn beschikbaar in de specifieke packages (bijvoorbeeld `NServiceBusTenantIdResolver.DetermineTenantIdentifier` in `Wigo4it.MultiTenant.NServiceBus`).

## Configuratiestructuur

De configuratie volgt een hiërarchische structuur in `appsettings.json`:

```json
{
  "Tenants": {
    "{TenantCode}": {
      "Environments": {
        "{EnvironmentName}": {
          "Defaults": {
            "CustomProperty": "Default waarde voor alle gemeenten"
          },
          "Gemeenten": {
            "{GemeenteCode}": {
              "identifier": "9446-0599so1-0599",
              "name": "Gemeente 0599",
              "gemeentecode": "0599",
              "hoofdgemeente": "0599",
              "CustomProperty": "Overschreven waarde voor deze gemeente"
            }
          }
        }
      }
    }
  }
}
```

Elke configuration provider ondersteunt door .Net kan gebruikt worden om deze structuur op te bouwen. Zie de [documentatie](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration).

**Configuratie-hiërarchie**

1. `Defaults` bevat waarden die gelden voor alle gemeenten binnen een omgeving.
2. `Gemeenten` kan specifieke waarden overschrijven. Waarden die hier ontbreken, erven uit `Defaults`.

## Eigen TenantInfo type

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

De properties in je `TenantInfo` worden automatisch gebonden vanuit configuratie. Gebruik lowercase propertynamen in JSON (conventie van .NET configuration binding).

## Tenant-specifieke IOptions

1. Definieer je options type:

```csharp
public class MyTenantOptions
{
    public string? CustomSetting { get; set; }
    public bool FeatureEnabled { get; set; }
    public string? ApiEndpoint { get; set; }
}
```

2. Map `TenantInfo` naar je options:

```csharp
builder.Services.AddWigo4itMultiTenant<MyTenantInfo>(tenantIdentifierResolver);

builder.Services.ConfigurePerTenant<MyTenantOptions, MyTenantInfo>((options, tenantInfo) =>
{
    options.CustomSetting = tenantInfo.CustomSetting;
    options.FeatureEnabled = tenantInfo.FeatureEnabled;
    options.ApiEndpoint = tenantInfo.ComplexProperty?.ApiUrl;
});
```

3. Gebruik `IOptions<MyTenantOptions>` in services:

```csharp
public class MyService
{
    private readonly MyTenantOptions _options;

    public MyService(IOptions<MyTenantOptions> options)
    {
        _options = options.Value;
    }
}
```
## Toegang tot TenantInfo

```csharp
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
        // Gebruik bij voorkeur options, maar TenantInfo is beschikbaar indien nodig
    }
}
```

Gebruik `IOptions<T>` als primaire toegang tot tenantgegevens om afhankelijkheden expliciet en testbaar te houden.

## Voorbeeldconfiguratie

```json
{
  "Tenants": {
    "9446": {
      "Environments": {
        "0599so1": {
          "Defaults": {
            "CustomSetting": "Default waarde",
            "ComplexProperty": {
              "ApiUrl": "https://api-rotterdam.example.com",
              "Timeout": "30"
            }
          },
          "Gemeenten": {
            "0599": {
              "identifier": "9446-0599so1-0599",
              "name": "Gemeente Rotterdam",
              "gemeentecode": "0599",
              "hoofdgemeente": "0599"
            },
            "0502": {
              "identifier": "9446-0599so1-0502",
              "name": "Gemeente Alblasserdam",
              "gemeentecode": "0502",
              "hoofdgemeente": "0518",
              "CustomSetting": "Overschreven voor Alblasserdam",
              "ComplexProperty": {
                "ApiUrl": "https://api-alblasserdam.example.com"
              }
            }
          }
        }
      }
    }
  }
}
```

- Gemeente 0599 krijgt de defaults.
- Gemeente 0502 overschrijft `CustomSetting` en `ComplexProperty.ApiUrl`, maar erft `ComplexProperty.Timeout` uit defaults.

## Best practices

- Gebruik `IOptions<T>` in plaats van directe `TenantInfo` toegang waar mogelijk.
- Maak opties per domein (database, feature flags, externe API) en map alleen de benodigde properties.
- Zet gedeelde waarden in `Defaults` en overschrijf alleen waar nodig per gemeente.
- Valideer tenant-resolutie vroeg en gooi een duidelijke fout als de tenant ontbreekt.

## Troubleshooting

- **Tenant kan niet worden geresolved**: controleer headers/input van je resolver en of de tenant in configuratie bestaat.
- **Options leeg of null**: verifieer de mapping in `ConfigurePerTenant`.
- **Configuratie ontbreekt**: check de `Tenants > {code} > Environments > {name} > Defaults/Gemeenten` structuur en of nested objecten correct zijn.

## Veelgestelde vragen

- **Azure App Configuration gebruiken?** Ja, `DictionaryConfigurationStore` werkt met elke `IConfiguration` provider.
- **Meerdere TenantInfo types?** Niet binnen één applicatie; gebruik één subtype met eventuele nested objecten.
- **Connection strings per gemeente?** Definieer in `Defaults` en override per gemeente waar nodig.

## Tests

Zie [Wigo4it.MultiTenant.Tests](../Wigo4it.MultiTenant.Tests) voor unit tests van configuratie en resolvers.

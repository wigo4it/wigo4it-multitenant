# Wigo4it.MultiTenant.AspNetCore

ASP.NET Core integration voor Wigo4it.MultiTenant. De library resolved tenants uit HTTP-headers, zet de tenantcontext voor requests en stuurt headers door naar uitgaande NServiceBus-berichten wanneer gecombineerd gebruikt.

## Installatie

```bash
dotnet add package Wigo4it.MultiTenant.AspNetCore
```

## Setup in een ASP.NET Core applicatie

1. **Registreren van multi-tenant services**

```csharp
using Wigo4it.MultiTenant.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWigo4itMultiTenantAspNetCore<MyTenantInfo>();

builder.Services.ConfigurePerTenant<MyTenantOptions, MyTenantInfo>((options, tenant) =>
{
    options.CustomSetting = tenant.CustomSetting;
    options.FeatureEnabled = tenant.FeatureEnabled;
});
```

2. **Middleware activeren**

```csharp
var app = builder.Build();

app.UseWigo4itMultiTenant();

app.MapControllers();
app.Run();
```

## Controller met tenant context

```csharp
[ApiController]
[Route("[controller]")]
public class MyController : ControllerBase
{
    private readonly ILogger<MyController> _logger;
    private readonly IMultiTenantContextAccessor<MyTenantInfo> _tenantContextAccessor;
    private readonly MyTenantOptions _options;

    public MyController(
        ILogger<MyController> logger,
        IMultiTenantContextAccessor<MyTenantInfo> tenantContextAccessor,
        IOptions<MyTenantOptions> options)
    {
        _logger = logger;
        _tenantContextAccessor = tenantContextAccessor;
        _options = options.Value;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var tenantInfo = _tenantContextAccessor.MultiTenantContext?.TenantInfo;

        _logger.LogInformation(
            "Processing request for tenant {TenantId} with setting {Setting}",
            tenantInfo?.Identifier,
            _options.CustomSetting);

        return Ok(new { Tenant = tenantInfo?.Identifier, Setting = _options.CustomSetting });
    }
}
```

Let op: gebruik bij voorkeur `IOptions<>` om tenant-specifieke configuratie in je controller te benaderen in plaats van `IMultiTenantContextAccessor<>` zodat controllers niet afhankelijk worden van multi-tenant configuratiecode.

## HTTP requests met headers

Verstuur requests met de volgende HTTP-headers:

```http
GET /api/myendpoint HTTP/1.1
Host: localhost:5000
Wigo4it.Wegwijzer.TenantCode.Forwardable: 9446
Wigo4it.Wegwijzer.EnvironmentName.Forwardable: 0518pr1
Wigo4it.Socrates.GemeenteCode.Forwardable: 0001
```

## Integratie met NServiceBus

Wanneer je zowel `Wigo4it.MultiTenant.AspNetCore` als `Wigo4it.MultiTenant.NServiceBus` gebruikt, worden de headers automatisch doorgestuurd naar uitgaande NServiceBus-berichten:

```csharp
// In je controller of service
[HttpPost("send-message")]
public async Task<IActionResult> SendMessage(
    [FromServices] IMessageSession messageSession)
{
    // Headers worden automatisch gekopieerd naar het bericht
    await messageSession.SendLocal(new MyMessage { /* payload */ });
    
    return Accepted();
}
```

## Best practices

- Gebruik `ConfigurePerTenant` om alleen de benodigde properties naar options te mappen; injecteer `IOptions<T>` in controllers/services.
- Log de resolved tenant voor diagnostiek.
- Zorg ervoor dat alle drie de headers aanwezig zijn in elke request.

## Troubleshooting

- **Tenant could not be resolved**: controleer dat alle drie de headers aanwezig zijn en dat de identifier in configuratie bestaat.
- **Headers worden niet doorgestuurd naar NServiceBus**: zorg ervoor dat de middleware vóór `UseMultiTenant()` wordt aangeroepen.
- **Options bevatten unexpected waarden**: check de mapping en lowercase propertynamen in JSON.

## Tests en referenties

- Unit tests: [Wigo4it.MultiTenant.AspNetCore.Tests](../Wigo4it.MultiTenant.AspNetCore.Tests).
- Integratietests: [Wigo4it.MultiTenant.AspNetCore.IntegrationTests](../Wigo4it.MultiTenant.AspNetCore.IntegrationTests).

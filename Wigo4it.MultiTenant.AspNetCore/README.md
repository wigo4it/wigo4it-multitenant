# Wigo4it.MultiTenant.AspNetCore

ASP.NET Core integratie voor `Wigo4it.MultiTenant`. Deze library leest tenant-identificerende claims of HTTP-headers en zet de tenant context. 
Deze context wordt in `Wigo4it.MultiTenant` om tenant-specifieke configuratie te bepalen

## Installatie

```bash
dotnet add package Wigo4it.MultiTenant.AspNetCore
```

## Setup in Program.cs

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Wigo4it.MultiTenant;
using Wigo4it.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// AddAuthentication en AddAuthorization is alleen nodig bij gebruik van TenantIdResolutionStrategy.Claims
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddAuthorization();

// Resolve tenant op basis van claims in het token
builder.Services.AddWigo4itMultiTenantAspNetCore(o =>
            o.TenantIdResolutionStrategy = TenantIdResolutionStrategy.Claims);
// óf resolve tenant op basis van headers:
builder.Services.AddWigo4itMultiTenantAspNetCore(o =>
            o.TenantIdResolutionStrategy = TenantIdResolutionStrategy.Headers);


var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseMultiTenant();

app.MapGet("/", () => "ok");
app.Run();
```

# Tenant resolution strategie
Tenant resolution kan op twee manieren worden gedaan: via claims in een JWT token of via HTTP headers. Deze keuze wordt op host niveau gemaakt via de `TenantIdResolutionStrategy` optie.

## Claims based tenant resolution
Configureer de library om tenant identifiers uit claims te lezen:
```csharp
builder.Services.AddWigo4itMultiTenantAspNetCore(options =>
    options.TenantIdResolutionStrategy = TenantIdResolutionStrategy.Claims);
```

De Tenant identifier wordt vervolgens opgebouwd uit de volgende claims:
- `w4-ww-tenant` (`MultitenancyIdentifiers.Claims.WegwijzerTenantCode`)
- `w4-ww-env` (`MultitenancyIdentifiers.Claims.WegwijzerEnvironmentName`)
- `w4-ww-gemeente` (`MultitenancyIdentifiers.Claims.GemeenteCode`)

De tenant identifier wordt opgebouwd als `{tenantCode}-{environmentName}-{gemeenteCode}`.

## Header based tenant resolution
Configureer de library om tenant identifiers uit HTTP headers te lezen:
```csharp
builder.Services.AddWigo4itMultiTenantAspNetCore(options =>
    options.TenantIdResolutionStrategy = TenantIdResolutionStrategy.Headers);
```

De Tenant identifier wordt vervolgens opgebouwd uit de volgende headers:
- `X-Wigo4it-Wegwijzer-TenantCode` (`MultitenancyIdentifiers.HttpHeaders.WegwijzerTenantCode`)
- `X-Wigo4it-Wegwijzer-EnvironmentName` (`MultitenancyIdentifiers.HttpHeaders.WegwijzerEnvironmentName`)
- `X-Wigo4it-Socrates-GemeenteCode` (`MultitenancyIdentifiers.HttpHeaders.GemeenteCode`)

De tenant identifier wordt opgebouwd als `{tenantCode}-{environmentName}-{gemeenteCode}`.

## Tenant resolution op basis van configuratie
In plaats van een hardcoded strategie te gebruiken, kan de tenant resolution ook worden geconfigureerd via ASP.Net Core configuratie (bijvoorbeeld `Wigo4it:MultiTenant`):

```csharp
builder.Services.AddWigo4itMultiTenantAspNetCore(options =>
	builder.Configuration.GetSection("Wigo4it:MultiTenant").Bind(options));
```

Let hierbij op dat de `Wigo4it:MultiTenant` sectie beschikbaar is in de configuratie, bijvoorbeeld via `appsettings.json`:

```json
{
  "Wigo4it": {
    "MultiTenant": {
      "TenantIdResolutionStrategy": "Claims"
    }
  }
}
```
of environment variables:
```bash
export Wigo4it__MultiTenant__TenantIdResolutionStrategy=Claims
```

## Middleware volgorde

Plaats `app.UseMultiTenant()` **na** authenticatie (`UseAuthentication`) zodat claims beschikbaar zijn, en vóór endpoints die tenant-specifieke opties gebruiken.


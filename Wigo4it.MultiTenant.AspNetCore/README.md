# Wigo4it.MultiTenant.AspNetCore

ASP.NET Core integratie voor `Wigo4it.MultiTenant`. Deze library leest tenant-identificerende claims of HTTP-headers en zet de tenant context via middleware + Finbuckle strategy.

## Installatie

```bash
dotnet add package Wigo4it.MultiTenant.AspNetCore
```

## Setup in Program.cs

```csharp
using Wigo4it.MultiTenant;
using Wigo4it.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();

builder.Services.AddWigo4itMultiTenantAspNetCore();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseMultiTenant();

app.MapGet("/", () => "ok");
app.Run();
```

## Benodigde claims in token

- `w4-ww-tenant` (`MultitenancyIdentifiers.Claims.WegwijzerTenantCode`)
- `w4-ww-env` (`MultitenancyIdentifiers.Claims.WegwijzerEnvironmentName`)
- `w4-ww-gemeente` (`MultitenancyIdentifiers.Claims.GemeenteCode`)

De tenant identifier wordt opgebouwd als `{tenantCode}-{environmentName}-{gemeenteCode}`.

## Alternatief: headers gebruiken

Geef een options configuratie mee:

```csharp
builder.Services.AddWigo4itMultiTenantAspNetCore(options =>
	options.TenantIdResolutionStrategy = TenantIdResolutionStrategy.Headers);
```

Of bind vanuit configuratie (bijvoorbeeld `Wigo4it:MultiTenant`):

```csharp
builder.Services.AddWigo4itMultiTenantAspNetCore(options =>
	builder.Configuration.GetSection("Wigo4it:MultiTenant").Bind(options));
```

Benodigde headers:

- `X-Wigo4it-Wegwijzer-TenantCode` (`MultitenancyIdentifiers.HttpHeaders.WegwijzerTenantCode`)
- `X-Wigo4it-Wegwijzer-EnvironmentName` (`MultitenancyIdentifiers.HttpHeaders.WegwijzerEnvironmentName`)
- `X-Wigo4it-Socrates-GemeenteCode` (`MultitenancyIdentifiers.HttpHeaders.GemeenteCode`)

## Middleware volgorde

Plaats `app.UseMultiTenant()` **na** authenticatie (`UseAuthentication`) zodat claims beschikbaar zijn, en vóór endpoints die tenant-specifieke opties gebruiken.


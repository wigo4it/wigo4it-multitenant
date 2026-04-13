# Wigo4it.MultiTenant.AspNetCore

ASP.NET Core integratie voor `Wigo4it.MultiTenant`. Deze library leest tenant-identificerende claims uit het inkomende token en zet de tenant context via middleware + Finbuckle strategy.

## Installatie

```bash
dotnet add package Wigo4it.MultiTenant.AspNetCore
```

## Setup in Program.cs

```csharp
using Wigo4it.MultiTenant;
using Wigo4it.MultiTenant.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();

builder.Services.AddWigo4itMultiTenantAspNetCore();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseWigo4itMultiTenantAspNetCore();

app.MapGet("/", () => "ok");
app.Run();
```

## Benodigde claims in token

- `w4-ww-tenant` (`MultitenancyClaims.WegwijzerTenantCode`)
- `w4-ww-env` (`MultitenancyClaims.WegwijzerEnvironmentName`)
- `w4-ww-gemeente` (`MultitenancyClaims.GemeenteCode`)

De tenant identifier wordt opgebouwd als `{tenantCode}-{environmentName}-{gemeenteCode}`.

## Middleware volgorde

Plaats `app.UseWigo4itMultiTenantAspNetCore()` **na** authenticatie (`UseAuthentication`) zodat claims beschikbaar zijn, en vóór endpoints die tenant-specifieke opties gebruiken.


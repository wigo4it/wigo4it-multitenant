using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Wigo4it.MultiTenant;
using Wigo4it.MultiTenant.AspNetCore.Sample;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureSampleServices(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseMultiTenant();

// Health check endpoint
app.MapGet("/", () => "AspNetCore multi-tenant sample running.");

// Demonstreert dat SampleTenantOptions correct worden resolved op basis van de tenant
app.MapGet(
    "/tenant-info",
    [Authorize]
    (
        HttpContext context,
        IOptions<SampleTenantOptions> sampleOptions,
        IOptions<Wigo4itTenantOptions> tenantOptions,
        Finbuckle.MultiTenant.Abstractions.IMultiTenantContextAccessor mtContextAccessor
    ) =>
    {
        // Haal de tenant identifier op uit de multitenancy context, niet rechtstreeks uit claims of headers
        var tenantIdentifier = mtContextAccessor.MultiTenantContext?.TenantInfo?.Id;

        return tenantIdentifier == null
            ? Results.BadRequest("Could not resolve tenant identifier")
            : Results.Ok(new
            {
                Message = "Tenant information successfully resolved",
                TenantIdentifier = tenantIdentifier,
                TenantCode = tenantOptions.Value.TenantCode,
                EnvironmentName = tenantOptions.Value.EnvironmentName,
                GemeenteCode = tenantOptions.Value.GemeenteCode,
                CustomSetting = sampleOptions.Value.CustomSetting ?? "Not configured"
            });
    }
).WithName("GetTenantInfo");

await app.RunAsync();

// Maak de Program klasse toegankelijk voor integratietesten.
public abstract partial class Program { }




using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Wigo4it.MultiTenant;
using Wigo4it.MultiTenant.AspNetCore.Sample;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureSampleServices();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseMultiTenant();

// Health check eindpunt
app.MapGet("/", () => "AspNetCore multi-tenant sample running.");

// Demonstreert dat SampleTenantOptions correct worden opgelost op basis van de opgeloste tenant
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
        // Haal de tenant identifier op uit de multitenancy context, niet uit de claims
        var tenantIdentifier = mtContextAccessor.MultiTenantContext?.TenantInfo?.Id;

        return Results.Ok(new
        {
            Message = "Tenant information successfully resolved from JWT claims",
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




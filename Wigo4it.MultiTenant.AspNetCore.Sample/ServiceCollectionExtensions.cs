using Finbuckle.MultiTenant.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Wigo4it.MultiTenant.AspNetCore.Sample;

/// <summary>
/// Service collection extensies voor het configureren van de voorbeeld AspNetCore multitenancy setup.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configureert de voorbeeldservices met multitenancy ondersteuning.
    /// </summary>
    public static IServiceCollection ConfigureSampleServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configureer JWT Bearer authenticatie zonder tokenvalidatie voor voorbeelddoeleinden.
        // In productie moet de juiste tokenvalidatie worden geconfigureerd.
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Schakel alle validatie uit voor voorbeelddoeleinden
                options.TokenValidationParameters.ValidateIssuer = false;
                options.TokenValidationParameters.ValidateAudience = false;
                options.TokenValidationParameters.ValidateLifetime = false;
                options.TokenValidationParameters.ValidateIssuerSigningKey = false;

                options.TokenHandlers.Clear();
                options.TokenHandlers.Add(new UnsafeJwtTokenHandler());
            });

        services.AddAuthorization();

        // Configure multitenancy en bind ASP.NET Core opties rechtstreeks vanuit configuratie.
        services.AddWigo4itMultiTenantAspNetCore<SampleTenantInfo>(options =>
            configuration.GetSection("Wigo4it:MultiTenant").Bind(options)
        );

        // Configureer SampleTenantOptions om per tenant te resolven vanuit configuratie
        services.ConfigurePerTenant<SampleTenantOptions, SampleTenantInfo>(
            (options, tenantInfo) =>
            {
                options.CustomSetting = tenantInfo.CustomSetting;
            }
        );

        return services;
    }
}
using Finbuckle.MultiTenant.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Wigo4it.MultiTenant.AspNetCore.Sample;

/// <summary>
/// Service collection extensies voor het configureren van de voorbeeld AspNetCore multitenancy setup.
/// </summary>
public static class SampleServices
{
    /// <summary>
    /// Configureert de voorbeeldservices met multitenancy ondersteuning.
    /// </summary>
    public static IServiceCollection ConfigureSampleServices(this IServiceCollection services)
    {
        // Configureer JWT Bearer authenticatie zonder tokenvalidatie voor voorbeelddoeleinden.
        // In productie moet de juiste tokenvalidatie worden geconfigureerd.
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

        // Configureer multitenancy met de AspNetCore strategie via de tenant identifier uit de claims
        services.AddWigo4itMultiTenant<SampleTenantInfo>(builder =>
            builder.WithDelegateStrategy(AspNetCoreTenantIdFromClaimsResolver.DetermineTenantIdentifier)
        );

        // Configureer SampleTenantOptions om per tenant op te lossen vanuit configuratie
        services.ConfigurePerTenant<SampleTenantOptions, SampleTenantInfo>(
            (options, tenantInfo) =>
            {
                options.CustomSetting = tenantInfo.CustomSetting;
            }
        );

        return services;
    }
}

/// <summary>
/// Onveilige JWT validator uitsluitend voor voorbeelddoeleinden - valideert JWT zonder de handtekening te controleren.
/// NOOIT gebruiken in productie!
/// </summary>
internal class UnsafeJwtTokenHandler : TokenHandler
{
    private static readonly JwtSecurityTokenHandler JwtHandler = new();

    public override Task<TokenValidationResult> ValidateTokenAsync(string token,
        TokenValidationParameters validationParameters)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult(new TokenValidationResult
            {
                IsValid = false,
                Exception = new SecurityTokenMalformedException("Bearer token cannot be empty.")
            });
        }

        if (!JwtHandler.CanReadToken(token))
        {
            return Task.FromResult(new TokenValidationResult
            {
                IsValid = false,
                Exception = new SecurityTokenMalformedException("Bearer token must be valid.")
            });
        }

        try
        {
            var jwtToken = JwtHandler.ReadJwtToken(token);

            var result = new TokenValidationResult
            {
                ClaimsIdentity = new ClaimsIdentity(jwtToken.Claims, JwtBearerDefaults.AuthenticationScheme),
                SecurityToken = jwtToken,
                IsValid = true
            };
            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            return Task.FromResult(new TokenValidationResult
            {
                IsValid = false,
                Exception = ex
            });
        }
    }
}


using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Wigo4it.MultiTenant.AspNetCore;

/// <summary>
/// Resolveert de tenant identifier voor ASP.NET Core requests.
/// Eerst wordt een eerder bepaalde waarde uit <see cref="HttpContext.Items"/> gebruikt,
/// daarna worden de claims uit het inkomende token gelezen.
/// </summary>
public static class AspNetCoreTenantIdFromClaimsResolver
{
    /// <summary>
    /// Bepaalt de tenant identifier voor de gegeven request context.
    /// Geeft <see langword="null"/> terug als geen geldige tenant claims aanwezig zijn.
    /// </summary>
    public static Task<string?> DetermineTenantIdentifier(object context)
    {
        var httpContext = (HttpContext)context;
        return Task.FromResult(httpContext.User.CaptureTenantIdentifier());
    }

    /// <summary>
    /// Leest tenant-gerelateerde claims uit de principal en bouwt de identifier op als
    /// {TenantCode}-{EnvironmentName}-{GemeenteCode}.
    /// Geeft <see langword="null"/> terug bij ontbrekende claims of ongeauthenticeerde principal.
    /// </summary>
    public static string? CaptureTenantIdentifier(this ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var tenantCode = principal.FindFirst(MultitenancyClaims.WegwijzerTenantCode)?.Value;
        var environmentName = principal.FindFirst(MultitenancyClaims.WegwijzerEnvironmentName)?.Value;
        var gemeenteCode = principal.FindFirst(MultitenancyClaims.GemeenteCode)?.Value;

        if (
            string.IsNullOrWhiteSpace(tenantCode)
            || string.IsNullOrWhiteSpace(environmentName)
            || string.IsNullOrWhiteSpace(gemeenteCode)
        )
        {
            return null;
        }

        return $"{tenantCode}-{environmentName}-{gemeenteCode}";
    }
}

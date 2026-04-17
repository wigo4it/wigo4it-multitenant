using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Wigo4it.MultiTenant.AspNetCore;

/// <summary>
/// Resolvet de tenant identifier voor ASP.NET Core requests op basis van claims in de authenticated principal.
/// </summary>
public static class AspNetCoreTenantIdFromClaimsResolver
{
    /// <summary>
    /// Bepaalt de tenant identifier voor de gegeven request context.
    /// Geeft <see langword="null"/> terug als geen geldige tenant claims aanwezig zijn.
    /// </summary>
    public static Task<string?> DetermineTenantIdentifier(object context)
    {
        if (context is not HttpContext httpContext)
        {
            return Task.FromResult<string?>(null);
        }
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

        var tenantCode = principal.FindFirst(MultitenancyIdentifiers.Claims.WegwijzerTenantCode)?.Value?.Trim(' ', '"');
        var environmentName = principal.FindFirst(MultitenancyIdentifiers.Claims.WegwijzerEnvironmentName)?.Value?.Trim(' ', '"');
        var gemeenteCode = principal.FindFirst(MultitenancyIdentifiers.Claims.GemeenteCode)?.Value?.Trim(' ', '"');

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

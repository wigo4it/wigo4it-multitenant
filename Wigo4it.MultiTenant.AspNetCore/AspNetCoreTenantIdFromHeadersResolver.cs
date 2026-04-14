using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Wigo4it.MultiTenant.AspNetCore;

/// <summary>
/// Resolveert de tenant identifier voor ASP.NET Core requests op basis van HTTP headers.
/// </summary>
public static class AspNetCoreTenantIdFromHeadersResolver
{
    /// <summary>
    /// Bepaalt de tenant identifier voor de gegeven request context.
    /// Geeft <see langword="null"/> terug als vereiste headers ontbreken.
    /// </summary>
    public static Task<string?> DetermineTenantIdentifier(object context)
    {
        return Task.FromResult(context is HttpContext httpContext ? httpContext.Request.Headers.CaptureTenantIdentifier() : null);
    }

    /// <summary>
    /// Leest tenant-gerelateerde headers en bouwt de identifier op als
    /// {TenantCode}-{EnvironmentName}-{GemeenteCode}.
    /// Geeft <see langword="null"/> terug bij ontbrekende of lege headerwaarden.
    /// </summary>
    public static string? CaptureTenantIdentifier(this IHeaderDictionary headers)
    {
        var tenantCode = GetHeaderValue(headers, MultitenancyIdentifiers.HttpHeaders.WegwijzerTenantCode);
        var environmentName = GetHeaderValue(headers, MultitenancyIdentifiers.HttpHeaders.WegwijzerEnvironmentName);
        var gemeenteCode = GetHeaderValue(headers, MultitenancyIdentifiers.HttpHeaders.GemeenteCode);

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

    private static string? GetHeaderValue(IHeaderDictionary headers, string key)
    {
        return headers.TryGetValue(key, out StringValues value) && !StringValues.IsNullOrEmpty(value) ? value.ToString() : null;
    }
}

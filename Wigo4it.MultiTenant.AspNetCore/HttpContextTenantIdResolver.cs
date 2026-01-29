using Microsoft.AspNetCore.Http;

namespace Wigo4it.MultiTenant.AspNetCore;

public static class HttpContextTenantIdResolver
{
    public static Task<string?> DetermineTenantIdentifier(object context)
    {
        var httpContext = (HttpContext)context;

        return Task.FromResult<string?>(httpContext.CaptureTenantIdentifier());
    }

    public static string CaptureTenantIdentifier(this HttpContext httpContext)
    {
        var headers = httpContext.Request.Headers;
        
        return $"{headers[MultitenancyHeaders.WegwijzerTenantCode]}"
            + $"-{headers[MultitenancyHeaders.WegwijzerEnvironmentName]}"
            + $"-{headers[MultitenancyHeaders.GemeenteCode]}";
    }
}

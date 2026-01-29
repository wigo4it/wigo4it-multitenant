using Microsoft.AspNetCore.Http;

namespace Wigo4it.MultiTenant.AspNetCore;

/// <summary>
/// Middleware that captures multi-tenancy headers from incoming HTTP requests
/// and makes them available to downstream components (like HeaderForwarder for NServiceBus).
/// </summary>
public class MultitenancyHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public MultitenancyHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, MultitenancyHeadersAccessor headersAccessor)
    {
        // Extract headers from HTTP request
        var headers = context.Request.Headers;
        
        if (headers.TryGetValue(MultitenancyHeaders.WegwijzerTenantCode, out var tenantCode))
        {
            headersAccessor.SetHeader(MultitenancyHeaders.WegwijzerTenantCode, tenantCode.ToString());
        }
        
        if (headers.TryGetValue(MultitenancyHeaders.WegwijzerEnvironmentName, out var environmentName))
        {
            headersAccessor.SetHeader(MultitenancyHeaders.WegwijzerEnvironmentName, environmentName.ToString());
        }
        
        if (headers.TryGetValue(MultitenancyHeaders.GemeenteCode, out var gemeenteCode))
        {
            headersAccessor.SetHeader(MultitenancyHeaders.GemeenteCode, gemeenteCode.ToString());
        }

        await _next(context);
    }
}

using Microsoft.AspNetCore.Http;

namespace Wigo4it.MultiTenant.AspNetCore;

/// <summary>
/// Middleware om multi-tenancy headers uit inkomende incoming HTTP requests te halen
/// en beschikbara te maken voor downstream componenten (zoals HeaderForwarder voor NServiceBus).
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
        try
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
        finally
        {
            // Clear headers after request completes to prevent leaking to subsequent requests
            headersAccessor.Clear();
        }
    }
}

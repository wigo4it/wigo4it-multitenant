using Microsoft.Extensions.Options;

namespace Wigo4it.MultiTenant.MultiTenantHttpClient;

internal class MultiTenantHeadersDelegatingHandler(IOptions<Wigo4itTenantOptions> tenantOptions) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.TryAddWithoutValidation(
            MultitenancyIdentifiers.HttpHeaders.WegwijzerEnvironmentName,
            tenantOptions.Value.EnvironmentName
        );
        request.Headers.TryAddWithoutValidation(
            MultitenancyIdentifiers.HttpHeaders.WegwijzerTenantCode,
            tenantOptions.Value.TenantCode
        );
        request.Headers.TryAddWithoutValidation(
            MultitenancyIdentifiers.HttpHeaders.GemeenteCode,
            tenantOptions.Value.GemeenteCode
        );
        return base.SendAsync(request, cancellationToken);
    }
}

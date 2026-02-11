using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.Extensions;

namespace Wigo4it.MultiTenant.NServiceBus.Sample;

public static class SampleServices
{
    public static IServiceCollection ConfigureSampleServices(this IServiceCollection services)
    {
        services.AddWigo4itMultiTenant<SampleTenantInfo>(b =>
            b.WithRouteStrategy("tenantIdentifier", false) // Voor de ASP.NET core pipeline
                .WithDelegateStrategy(NServiceBusTenantIdResolver.DetermineTenantIdentifier) // Voor de NServiceBus message pipeline
        );
        services.AddWigo4itMultiTenantNServiceBus();

        services.ConfigurePerTenant<SampleTenantOptions, SampleTenantInfo>(
            (o, t) =>
            {
                o.CustomSetting = t.CustomSetting;
            }
        );

        return services;
    }
}

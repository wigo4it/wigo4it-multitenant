using Finbuckle.MultiTenant.Extensions;

namespace Wigo4it.MultiTenant.NServiceBus.Sample;

public static class SampleServices
{
    public static IServiceCollection ConfigureSampleServices(this IServiceCollection services)
    {
        services.AddWigo4itMultiTenant<SampleTenantInfo>(NServiceBusTenantIdResolver.DetermineTenantIdentifier);

        services.ConfigurePerTenant<SampleTenantOptions, SampleTenantInfo>((o, t) =>
        {
            o.CustomSetting = t.CustomSetting;
        });

        return services;
    }
}
using Finbuckle.MultiTenant.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Wigo4it.MultiTenant;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddWigo4itMultiTenant<TTenantInfo>(Func<object, Task<string?>> tenantIdentifierResolver)
            where TTenantInfo : Wigo4itTenantInfo
        {
            services
                .AddMultiTenant<TTenantInfo>()
                .WithDelegateStrategy(tenantIdentifierResolver)
                .WithStore<DictionaryConfigurationStore<TTenantInfo>>(ServiceLifetime.Singleton);

            services.ConfigurePerTenant<Wigo4itTenantOptions, TTenantInfo>(
                (o, t) =>
                {
                    o.TenantCode = t.Options.TenantCode;
                    o.EnvironmentName = t.Options.EnvironmentName;
                    o.GemeenteCode = t.Options.GemeenteCode;
                }
            );

            return services;
        }

        public IServiceCollection AddWigo4itMultiTenant(Func<object, Task<string?>> tenantIdentifierResolver)
        {
            return services.AddWigo4itMultiTenant<Wigo4itTenantInfo>(tenantIdentifierResolver);
        }
    }
}

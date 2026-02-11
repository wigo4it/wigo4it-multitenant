using Finbuckle.MultiTenant;
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
            return services.AddWigo4itMultiTenant<TTenantInfo>(builder =>
            {
                builder.WithDelegateStrategy(tenantIdentifierResolver);
            });
        }

        public IServiceCollection AddWigo4itMultiTenant<TTenantInfo>(
            Action<MultiTenantBuilder<TTenantInfo>> configureMultitenantBuilder
        )
            where TTenantInfo : Wigo4itTenantInfo
        {
            var builder = services
                .AddMultiTenant<TTenantInfo>()
                .WithStore<DictionaryConfigurationStore<TTenantInfo>>(ServiceLifetime.Singleton);

            configureMultitenantBuilder(builder);

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

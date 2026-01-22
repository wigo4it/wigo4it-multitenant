using Finbuckle.MultiTenant;
using Microsoft.Extensions.DependencyInjection;

namespace Wigo4it.MultiTenant;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWigo4itMultiTenant<TTenantInfo>(
        this IServiceCollection services,
        Func<object, Task<string?>> tenantIdentifierResolver) where TTenantInfo: Wigo4itTenantInfo, new()
    {
        services.AddMultiTenant<TTenantInfo>()
            .WithDelegateStrategy(tenantIdentifierResolver)
            .WithStore<DictionaryConfigurationStore<TTenantInfo>>(ServiceLifetime.Singleton)
            .WithStore<DictionaryConfigurationStore<TTenantInfo>>(ServiceLifetime.Scoped);

        return services;
    }

    public static IServiceCollection AddWigo4itMultiTenant(
        this IServiceCollection services,
        Func<object, Task<string?>> tenantIdentifierResolver)
    {
        return services.AddWigo4itMultiTenant<Wigo4itTenantInfo>(tenantIdentifierResolver);
    }
}

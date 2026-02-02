using Finbuckle.MultiTenant.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Wigo4it.MultiTenant;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWigo4itMultiTenant<TTenantInfo>(
        this IServiceCollection services,
        Func<object, Task<string?>> tenantIdentifierResolver) where TTenantInfo: Wigo4itTenantInfo
    {
        services.AddMultiTenant<TTenantInfo>()
            .WithDelegateStrategy(tenantIdentifierResolver)
            .WithStore<DictionaryConfigurationStore<TTenantInfo>>(ServiceLifetime.Singleton);

        return services;
    }

    public static IServiceCollection AddWigo4itMultiTenant(
        this IServiceCollection services,
        Func<object, Task<string?>> tenantIdentifierResolver)
    {
        return services.AddWigo4itMultiTenant<Wigo4itTenantInfo>(tenantIdentifierResolver);
    }
}

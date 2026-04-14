using Microsoft.Extensions.DependencyInjection;

namespace Wigo4it.MultiTenant.AspNetCore;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddWigo4itMultiTenantAspNetCore<TTenantInfo>(
            Action<Wigo4ItMultiTenantAspNetCoreOptions>? configure = null
        )
            where TTenantInfo : Wigo4itTenantInfo
        {
            var options = new Wigo4ItMultiTenantAspNetCoreOptions();
            configure?.Invoke(options);

            Func<object, Task<string?>> resolver = options.TenantIdResolutionStrategy switch
            {
                TenantIdResolutionStrategy.Headers => AspNetCoreTenantIdFromHeadersResolver.DetermineTenantIdentifier,
                _ => AspNetCoreTenantIdFromClaimsResolver.DetermineTenantIdentifier,
            };

            return services.AddWigo4itMultiTenant<TTenantInfo>(resolver);
        }

        public IServiceCollection AddWigo4itMultiTenantAspNetCore(Action<Wigo4ItMultiTenantAspNetCoreOptions>? configure = null)
        {
            return services.AddWigo4itMultiTenantAspNetCore<Wigo4itTenantInfo>(configure);
        }
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace Wigo4it.MultiTenant.AspNetCore;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddWigo4itMultiTenantAspNetCore<TTenantInfo>(
            TenantIdResolutionStrategy strategy
        )
            where TTenantInfo : Wigo4itTenantInfo
        {
            Func<object, Task<string?>> resolver = strategy switch
            {
                TenantIdResolutionStrategy.Headers => AspNetCoreTenantIdFromHeadersResolver.DetermineTenantIdentifier,
                _ => AspNetCoreTenantIdFromClaimsResolver.DetermineTenantIdentifier,
            };

            return services.AddWigo4itMultiTenant<TTenantInfo>(resolver);
        }

        public IServiceCollection AddWigo4itMultiTenantAspNetCore(
            TenantIdResolutionStrategy strategy
        )
        {
            return services.AddWigo4itMultiTenantAspNetCore<Wigo4itTenantInfo>(strategy);
        }
    }
}


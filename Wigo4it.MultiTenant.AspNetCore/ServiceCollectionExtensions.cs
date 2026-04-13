using Microsoft.Extensions.DependencyInjection;

namespace Wigo4it.MultiTenant.AspNetCore;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddWigo4itMultiTenantAspNetCore<TTenantInfo>()
            where TTenantInfo : Wigo4itTenantInfo
        {
            return services.AddWigo4itMultiTenant<TTenantInfo>(AspNetCoreTenantIdFromClaimsResolver.DetermineTenantIdentifier);
        }

        public IServiceCollection AddWigo4itMultiTenantAspNetCore()
        {
            return services.AddWigo4itMultiTenantAspNetCore<Wigo4itTenantInfo>();
        }
    }
}


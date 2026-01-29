using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Wigo4it.MultiTenant.AspNetCore;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWigo4itMultiTenantAspNetCore<TTenantInfo>(
        this IServiceCollection services) where TTenantInfo : Wigo4itTenantInfo, new()
    {
        services.AddScoped<MultitenancyHeadersAccessor>();
        
        services.AddMultiTenant<TTenantInfo>()
            .WithDelegateStrategy(HttpContextTenantIdResolver.DetermineTenantIdentifier)
            .WithStore<DictionaryConfigurationStore<TTenantInfo>>(ServiceLifetime.Singleton);

        return services;
    }

    public static IServiceCollection AddWigo4itMultiTenantAspNetCore(
        this IServiceCollection services)
    {
        return services.AddWigo4itMultiTenantAspNetCore<Wigo4itTenantInfo>();
    }

    public static IApplicationBuilder UseWigo4itMultiTenant(this IApplicationBuilder app)
    {
        app.UseMiddleware<MultitenancyHeadersMiddleware>();
        app.UseMultiTenant();
        
        return app;
    }
}

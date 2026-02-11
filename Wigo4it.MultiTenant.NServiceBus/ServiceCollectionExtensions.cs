using Microsoft.Extensions.DependencyInjection;
using NServiceBus.MessageMutator;

namespace Wigo4it.MultiTenant.NServiceBus;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddWigo4itMultiTenantNServiceBus()
        {
            services.AddScoped<IMutateOutgoingMessages, HeaderForwarder>();

            return services;
        }
    }
}

using Finbuckle.MultiTenant.Abstractions;
using NServiceBus.MessageMutator;

namespace Wigo4it.MultiTenant.NServiceBus;

public static class PipelineSettingsExtensions
{
    public static void UseWigo4itMultiTenant(this EndpointConfiguration endpointConfiguration,
        Action<IMultiTenantContext>? onMultiTenantContextResolved = null)
    {
        endpointConfiguration.RegisterMessageMutator(new HeaderForwarder());
        
        endpointConfiguration.Pipeline.Register(_ => new MultiTenantBehavior(onMultiTenantContextResolved), 
            "Enables Finbuckle Multi-Tenancy support in NServiceBus message handling.");
    }
}
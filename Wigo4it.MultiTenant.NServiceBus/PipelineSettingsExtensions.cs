using Finbuckle.MultiTenant.Abstractions;
using NServiceBus.Pipeline;

namespace Wigo4it.MultiTenant.NServiceBus;

public static class PipelineSettingsExtensions
{
    public static void RegisterWigo4ItMultiTenantBehavior(this PipelineSettings pipelineSettings, Action<IMultiTenantContext>? onMultiTenantContextResolved = null)
    {
        pipelineSettings.Register(_ => new MultiTenantBehavior(onMultiTenantContextResolved), 
            "Enables Finbuckle Multi-Tenancy support in NServiceBus message handling.");
    }
}
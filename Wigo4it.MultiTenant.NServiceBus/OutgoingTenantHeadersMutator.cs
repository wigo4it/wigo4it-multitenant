using Microsoft.Extensions.Options;
using NServiceBus.MessageMutator;

namespace Wigo4it.MultiTenant.NServiceBus;

public class OutgoingTenantHeadersMutator(IOptions<Wigo4itTenantOptions> tenantOptions) : IMutateOutgoingMessages
{
    public Task MutateOutgoing(MutateOutgoingMessageContext context)
    {
        context.OutgoingHeaders[MultitenancyIdentifiers.MessageHeaders.WegwijzerTenantCode] = tenantOptions.Value.TenantCode;
        context.OutgoingHeaders[MultitenancyIdentifiers.MessageHeaders.WegwijzerEnvironmentName] = tenantOptions.Value.EnvironmentName;
        context.OutgoingHeaders[MultitenancyIdentifiers.MessageHeaders.GemeenteCode] = tenantOptions.Value.GemeenteCode;

        return Task.CompletedTask;
    }
}

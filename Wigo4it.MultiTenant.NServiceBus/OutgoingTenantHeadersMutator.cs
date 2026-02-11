using Microsoft.Extensions.Options;
using NServiceBus.MessageMutator;

namespace Wigo4it.MultiTenant.NServiceBus;

public class OutgoingTenantHeadersMutator(IOptions<Wigo4itTenantOptions> tenantOptions) : IMutateOutgoingMessages
{
    public Task MutateOutgoing(MutateOutgoingMessageContext context)
    {
        context.OutgoingHeaders[MultitenancyHeaders.WegwijzerTenantCode] = tenantOptions.Value.TenantCode;
        context.OutgoingHeaders[MultitenancyHeaders.WegwijzerEnvironmentName] = tenantOptions.Value.EnvironmentName;
        context.OutgoingHeaders[MultitenancyHeaders.GemeenteCode] = tenantOptions.Value.GemeenteCode;

        return Task.CompletedTask;
    }
}

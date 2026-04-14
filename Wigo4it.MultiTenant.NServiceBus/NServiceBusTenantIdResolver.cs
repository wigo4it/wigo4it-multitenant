using NServiceBus.Pipeline;
using NServiceBus.Transport;

namespace Wigo4it.MultiTenant.NServiceBus;

public static class NServiceBusTenantIdResolver
{
    public static Task<string?> DetermineTenantIdentifier(object context)
    {
        var messageContext = (IIncomingPhysicalMessageContext)context;

        return Task.FromResult<string?>(messageContext.Message.CaptureTenantIdentifier());
    }

    public static string CaptureTenantIdentifier(this IncomingMessage message)
    {
        return $"{message.Headers[MultitenancyIdentifiers.MessageHeaders.WegwijzerTenantCode]}"
            + $"-{message.Headers[MultitenancyIdentifiers.MessageHeaders.WegwijzerEnvironmentName]}"
            + $"-{message.Headers[MultitenancyIdentifiers.MessageHeaders.GemeenteCode]}";
    }
}

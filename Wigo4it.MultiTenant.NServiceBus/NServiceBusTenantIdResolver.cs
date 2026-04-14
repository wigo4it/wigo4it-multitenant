using NServiceBus.Pipeline;
using NServiceBus.Transport;

namespace Wigo4it.MultiTenant.NServiceBus;

public static class NServiceBusTenantIdResolver
{
    public static Task<string?> DetermineTenantIdentifier(object context)
    {
        return context is not IIncomingPhysicalMessageContext messageContext
            ? Task.FromResult<string?>(null)
            : Task.FromResult<string?>(messageContext.Message.CaptureTenantIdentifier());
    }

    public static string CaptureTenantIdentifier(this IncomingMessage message)
    {
        return $"{message.Headers[MultitenancyIdentifiers.MessageHeaders.WegwijzerTenantCode]}"
            + $"-{message.Headers[MultitenancyIdentifiers.MessageHeaders.WegwijzerEnvironmentName]}"
            + $"-{message.Headers[MultitenancyIdentifiers.MessageHeaders.GemeenteCode]}";
    }
}

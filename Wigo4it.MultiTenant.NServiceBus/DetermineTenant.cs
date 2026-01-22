using NServiceBus.Pipeline;
using NServiceBus.Transport;

namespace Wigo4it.MultiTenant.NServiceBus;

public static class DetermineTenant
{
    public static Task<string?> DetermineTenantIdentifier(object context)
    {
        var messageContext = (IIncomingPhysicalMessageContext)context;

        return Task.FromResult<string?>(messageContext.Message.CaptureTenantId());
    }

    public static string CaptureTenantId(this IncomingMessage message)
    {
        return $"{message.Headers[MultitenancyHeaders.WegwijzerTenantCode]}"
            + $"-{message.Headers[MultitenancyHeaders.WegwijzerEnvironmentName]}"
            + $"-{message.Headers[MultitenancyHeaders.GemeenteCode]}";
    }
}

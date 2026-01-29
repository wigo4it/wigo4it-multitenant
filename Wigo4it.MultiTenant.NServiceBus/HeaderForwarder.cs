using NServiceBus.MessageMutator;

namespace Wigo4it.MultiTenant.NServiceBus;

/// <summary>
/// Deze class kopieert alle 'forwardable' headers op binnenkomende messages naar de outgoing message.
/// Een message header is 'forwardable' als deze begint met "Wigo4it" en eindigt met "Forwardable"
/// Headers kunnen afkomstig zijn van binnenkomende NServiceBus messages of van HTTP requests (via MultitenancyHeadersAccessor)
/// </summary>
public class HeaderForwarder : IMutateOutgoingMessages
{
    public Task MutateOutgoing(MutateOutgoingMessageContext context)
    {
        // Try to get headers from incoming NServiceBus message
        if (context.TryGetIncomingHeaders(out var incomingHeaders))
        {
            foreach (
                var headerKey in incomingHeaders.Keys.Where(k =>
                    k.StartsWith("Wigo4it", StringComparison.OrdinalIgnoreCase)
                    && k.EndsWith("Forwardable", StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                CopyHeader(incomingHeaders, context.OutgoingHeaders, headerKey);
            }
        }
        
        // Also try to get headers from MultitenancyHeadersAccessor (for HTTP request originated headers)
        var headersAccessor = new MultitenancyHeadersAccessor();
        foreach (var header in headersAccessor.Headers.Where(h =>
            h.Key.StartsWith("Wigo4it", StringComparison.OrdinalIgnoreCase)
            && h.Key.EndsWith("Forwardable", StringComparison.OrdinalIgnoreCase)))
        {
            CopyHeader(headersAccessor.Headers, context.OutgoingHeaders, header.Key);
        }

        return Task.CompletedTask;
    }

    private static void CopyHeader(
        IReadOnlyDictionary<string, string> incomingHeaders,
        Dictionary<string, string> outgoingHeaders,
        string headerKey
    )
    {
        if (!outgoingHeaders.ContainsKey(headerKey))
        {
            outgoingHeaders[headerKey] = incomingHeaders[headerKey];
        }
    }
}

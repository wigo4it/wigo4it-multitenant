using NServiceBus.MessageMutator;

namespace Wigo4it.MultiTenant.NServiceBus;

/// <summary>
/// Deze class kopieert alle 'forwardable' headers op binnenkomende messages naar de outgoing message.
/// Een message header is 'forwardable' als deze begint met "Wigo4it" en eindigt met "Forwardable"
/// </summary>
public class HeaderForwarder : IMutateOutgoingMessages
{
    public Task MutateOutgoing(MutateOutgoingMessageContext context)
    {
        if (!context.TryGetIncomingHeaders(out var incomingHeaders))
        {
            return Task.CompletedTask;
        }

        foreach (
            var headerKey in incomingHeaders.Keys.Where(k =>
                k.StartsWith("Wigo4it", StringComparison.OrdinalIgnoreCase)
                && k.EndsWith("Forwardable", StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            CopyHeader(incomingHeaders, context.OutgoingHeaders, headerKey);
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

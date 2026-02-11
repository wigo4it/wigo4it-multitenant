using NServiceBus.Extensibility;
using NServiceBus.Pipeline;
using NServiceBus.Transport;

namespace Wigo4it.MultiTenant.NserviceBus.Tests;

/// <summary>
/// Test helper class to mock IIncomingPhysicalMessageContext for testing purposes.
/// </summary>
internal class TestIncomingPhysicalMessageContext(IncomingMessage message) : IIncomingPhysicalMessageContext
{
    public IncomingMessage Message { get; } = message;
    public IReadOnlyDictionary<string, string> MessageHeaders { get; } = message.Headers;
    public string MessageId { get; } = message.MessageId;
    public IServiceProvider Builder => throw new NotImplementedException();
    public ContextBag Extensions { get; } = new();
    public CancellationToken CancellationToken => CancellationToken.None;
    public string ReplyToAddress => throw new NotImplementedException();

    public void UpdateMessage(ReadOnlyMemory<byte> body)
    {
        throw new NotImplementedException();
    }

    public Task Send(object message, SendOptions options)
    {
        throw new NotImplementedException();
    }

    public Task Send<T>(Action<T> messageConstructor, SendOptions options)
    {
        throw new NotImplementedException();
    }

    public Task Publish(object message, PublishOptions options)
    {
        throw new NotImplementedException();
    }

    public Task Publish<T>(Action<T> messageConstructor, PublishOptions publishOptions)
    {
        throw new NotImplementedException();
    }

    public Task Reply(object message, ReplyOptions options)
    {
        throw new NotImplementedException();
    }

    public Task Reply<T>(Action<T> messageConstructor, ReplyOptions options)
    {
        throw new NotImplementedException();
    }

    public Task ForwardCurrentMessageTo(string destination)
    {
        throw new NotImplementedException();
    }
}

using NServiceBus;

namespace Wigo4it.MultiTenant.NServiceBus.Sample;

public class SampleMessage : ICommand
{
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}

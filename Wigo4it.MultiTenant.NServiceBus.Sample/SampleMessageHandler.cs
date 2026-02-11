using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Options;

namespace Wigo4it.MultiTenant.NServiceBus.Sample;

public class SampleMessageHandler(
    ILogger<SampleMessageHandler> logger,
    IMultiTenantContextAccessor<SampleTenantInfo> tenantContextAccessor,
    IOptions<SampleTenantOptions> tenantOptions
) : IHandleMessages<SampleMessage>
{
    public Task Handle(SampleMessage message, IMessageHandlerContext context)
    {
        var tenantInfo = tenantContextAccessor.MultiTenantContext?.TenantInfo;
        var options = tenantOptions.Value;

        logger.LogInformation(
            "Handled message '{Content}' for tenant {TenantIdentifier} at {HandledAt} | " + "Custom setting: {CustomSetting}",
            message.Content,
            tenantInfo?.Identifier ?? "<unknown>",
            DateTime.UtcNow,
            options.CustomSetting ?? "<not set>"
        );

        return Task.CompletedTask;
    }
}

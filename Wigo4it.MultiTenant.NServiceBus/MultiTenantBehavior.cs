using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus.Pipeline;

namespace Wigo4it.MultiTenant.NServiceBus;

public class MultiTenantBehavior(Action<IMultiTenantContext>? onMultiTenantContextResolved) : Behavior<IIncomingPhysicalMessageContext>
{
    public override async Task Invoke(IIncomingPhysicalMessageContext context, Func<Task> next)
    {
        var tenantResolver = context.Builder.GetRequiredService<ITenantResolver>();
        var tenantContext = await tenantResolver.ResolveAsync(context);

        if (!tenantContext.IsResolved)
        {
            throw new InvalidOperationException("Tenant could not be resolved.");
        }

        var mtcSetter = context.Builder.GetRequiredService<IMultiTenantContextSetter>();
        mtcSetter.MultiTenantContext = tenantContext;

        onMultiTenantContextResolved?.Invoke(tenantContext);

        await next();
    }
}

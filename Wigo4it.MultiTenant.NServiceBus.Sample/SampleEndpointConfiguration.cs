namespace Wigo4it.MultiTenant.NServiceBus.Sample;

public static class SampleEndpointConfiguration
{
    public static EndpointConfiguration Create(HostBuilderContext context, Action<EndpointConfiguration>? configure = null)
    {
        var endpointConfiguration = new EndpointConfiguration("Wigo4it.MultiTenant.NServiceBus.Sample");

        var transport = endpointConfiguration.UseTransport<LearningTransport>();
        transport.StorageDirectory(Path.Combine(context.HostingEnvironment.ContentRootPath, ".nsbtransport"));

        endpointConfiguration.UsePersistence<LearningPersistence>();
        endpointConfiguration.UseSerialization<SystemJsonSerializer>();
        endpointConfiguration.EnableInstallers();

        endpointConfiguration.Pipeline.RegisterWigo4ItMultiTenantBehavior(_ => { });

        configure?.Invoke(endpointConfiguration);
        
        return endpointConfiguration;
    }
}
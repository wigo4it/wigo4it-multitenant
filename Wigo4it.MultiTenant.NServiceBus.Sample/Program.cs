using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Microsoft.Extensions.Options;
using Wigo4it.MultiTenant;
using Wigo4it.MultiTenant.NServiceBus.Sample;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureSampleServices();

builder.Host.UseNServiceBus(EndpointConfigurationBuilder);

var app = builder.Build();

app.MapGet("/", () => "NServiceBus multi-tenant sample running.");

app.MapPost(
    "/send/{tenantIdentifier}",
    async (
        string tenantIdentifier,
        IMessageSession messageSession,
        IOptions<Wigo4itTenantOptions> x,
        IOptionsMonitor<Wigo4itTenantOptions> y
    ) =>
    {
        await messageSession.SendLocal(
            new SampleMessage { Content = $"Sample message for {tenantIdentifier}", CreatedAtUtc = DateTime.UtcNow }
        );

        return Results.Accepted();
    }
);

app.UseMultiTenant();

app.Run();

// Make Program class accessible for integration testing
public partial class Program
{
    public static Func<HostBuilderContext, EndpointConfiguration> EndpointConfigurationBuilder = context =>
        SampleEndpointConfiguration.Create(context);
}

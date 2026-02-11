using Wigo4it.MultiTenant;
using Wigo4it.MultiTenant.NServiceBus.Sample;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureSampleServices();

builder.Host.UseNServiceBus(context => SampleEndpointConfiguration.Create(context));

var app = builder.Build();

app.MapGet("/", () => "NServiceBus multi-tenant sample running.");

app.MapPost(
    "/send/{tenantCode}/{environmentName}/{gemeenteCode}",
    async (string tenantCode, string environmentName, string gemeenteCode, IMessageSession messageSession) =>
    {
        var sendOptions = new SendOptions();
        sendOptions.RouteToThisEndpoint();
        sendOptions.SetHeader(MultitenancyHeaders.WegwijzerTenantCode, tenantCode);
        sendOptions.SetHeader(MultitenancyHeaders.WegwijzerEnvironmentName, environmentName);
        sendOptions.SetHeader(MultitenancyHeaders.GemeenteCode, gemeenteCode);

        await messageSession.Send(
            new SampleMessage
            {
                Content = $"Sample message for {tenantCode}-{environmentName}-{gemeenteCode}",
                CreatedAtUtc = DateTime.UtcNow,
            },
            sendOptions
        );

        return Results.Accepted();
    }
);

app.Run();

// Make Program class accessible for integration testing
public partial class Program { }

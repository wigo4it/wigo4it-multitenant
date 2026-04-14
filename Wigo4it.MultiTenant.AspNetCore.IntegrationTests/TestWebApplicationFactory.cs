using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Wigo4it.MultiTenant.AspNetCore.IntegrationTests;

/// <summary>
/// Host de sample ASP.NET Core applicatie in-memory voor integratietesten.
/// </summary>
public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly TenantIdResolutionStrategy _strategy;

    /// <summary>
    /// Creates a new factory with the specified tenant ID resolution strategy.
    /// </summary>
    /// <param name="strategy">The tenant ID resolution strategy to use. Defaults to Claims.</param>
    public TestWebApplicationFactory(TenantIdResolutionStrategy strategy)
    {
        _strategy = strategy;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Override the TenantIdResolutionStrategy for testing
        var strategyValue = _strategy switch
        {
            TenantIdResolutionStrategy.Headers => "Headers",
            _ => "Claims"
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Wigo4it:MultiTenant:TenantIdResolutionStrategy", strategyValue }
            })
            .Build();


        builder.UseConfiguration(config);
        
    }
}


using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Wigo4it.MultiTenant.AspNetCore.IntegrationTests;

/// <summary>
/// Integration tests for multi-tenant ASP.NET Core middleware.
/// Tests that HTTP headers are properly captured and made available via MultitenancyHeadersAccessor.
/// </summary>
public class AspNetCoreMiddlewareIntegrationTests
{
    [Test]
    public async Task Middleware_ShouldCaptureHeadersFromHttpRequest()
    {
        // Arrange
        const string tenantCode = "9446";
        const string environmentName = "dev";
        const string gemeenteCode = "0599";

        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddWigo4itMultiTenantAspNetCore<Wigo4itTenantInfo>();
                    })
                    .Configure(app =>
                    {
                        app.UseWigo4itMultiTenant();
                        
                        app.Run(async context =>
                        {
                            var accessor = context.RequestServices.GetRequiredService<MultitenancyHeadersAccessor>();
                            var headers = accessor.Headers;
                            
                            await context.Response.WriteAsync($"TenantCode:{headers.GetValueOrDefault(MultitenancyHeaders.WegwijzerTenantCode)}");
                            await context.Response.WriteAsync($",EnvironmentName:{headers.GetValueOrDefault(MultitenancyHeaders.WegwijzerEnvironmentName)}");
                            await context.Response.WriteAsync($",GemeenteCode:{headers.GetValueOrDefault(MultitenancyHeaders.GemeenteCode)}");
                        });
                    });
            })
            .StartAsync();

        var client = host.GetTestClient();
        client.DefaultRequestHeaders.Add(MultitenancyHeaders.WegwijzerTenantCode, tenantCode);
        client.DefaultRequestHeaders.Add(MultitenancyHeaders.WegwijzerEnvironmentName, environmentName);
        client.DefaultRequestHeaders.Add(MultitenancyHeaders.GemeenteCode, gemeenteCode);

        // Act
        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(content, Does.Contain($"TenantCode:{tenantCode}"));
        Assert.That(content, Does.Contain($"EnvironmentName:{environmentName}"));
        Assert.That(content, Does.Contain($"GemeenteCode:{gemeenteCode}"));
    }

    [Test]
    public async Task Middleware_ShouldClearHeadersAfterRequest()
    {
        // Arrange
        const string tenantCode1 = "9446";
        const string tenantCode2 = "0518";

        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddWigo4itMultiTenantAspNetCore<Wigo4itTenantInfo>();
                    })
                    .Configure(app =>
                    {
                        app.UseWigo4itMultiTenant();
                        
                        app.Run(async context =>
                        {
                            var accessor = context.RequestServices.GetRequiredService<MultitenancyHeadersAccessor>();
                            var headers = accessor.Headers;
                            
                            await context.Response.WriteAsync($"TenantCode:{headers.GetValueOrDefault(MultitenancyHeaders.WegwijzerTenantCode)}");
                        });
                    });
            })
            .StartAsync();

        var client = host.GetTestClient();

        // Act - First request
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add(MultitenancyHeaders.WegwijzerTenantCode, tenantCode1);
        var response1 = await client.GetAsync("/");
        var content1 = await response1.Content.ReadAsStringAsync();

        // Act - Second request with different tenant
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add(MultitenancyHeaders.WegwijzerTenantCode, tenantCode2);
        var response2 = await client.GetAsync("/");
        var content2 = await response2.Content.ReadAsStringAsync();

        // Assert - Each request should have its own tenant
        Assert.That(content1, Does.Contain($"TenantCode:{tenantCode1}"));
        Assert.That(content2, Does.Contain($"TenantCode:{tenantCode2}"));
    }

    [Test]
    [TestCase("9446", "dev", "0599")]
    [TestCase("0518", "prod", "0001")]
    public async Task Middleware_ShouldResolveTenantFromHeaders(
        string tenantCode, 
        string environmentName, 
        string gemeenteCode)
    {
        // Arrange
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddWigo4itMultiTenantAspNetCore<Wigo4itTenantInfo>();
                    })
                    .Configure(app =>
                    {
                        app.UseWigo4itMultiTenant();
                        
                        app.Run(async context =>
                        {
                            await context.Response.WriteAsync("OK");
                        });
                    });
            })
            .StartAsync();

        var client = host.GetTestClient();
        client.DefaultRequestHeaders.Add(MultitenancyHeaders.WegwijzerTenantCode, tenantCode);
        client.DefaultRequestHeaders.Add(MultitenancyHeaders.WegwijzerEnvironmentName, environmentName);
        client.DefaultRequestHeaders.Add(MultitenancyHeaders.GemeenteCode, gemeenteCode);

        // Act
        var response = await client.GetAsync("/");

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
    }
}

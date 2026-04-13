using System.Net;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;

namespace Wigo4it.MultiTenant.AspNetCore.IntegrationTests;

public class TenantInfoEndpointsFromClaimsTests
{
    private const string DevToken0518 =
        "eyJhbGciOiAiSFMyNTYiLCAidHlwIjogIkpXVCJ9.eyJ3NC13dy10ZW5hbnQiOiAiOTQ0NiIsICJ3NC13dy1lbnYiOiAiZGV2IiwgInc0LXd3LWdlbWVlbnRlIjogIjA1MTgifQ.dummysignature";

    private const string TestToken0599 =
        "eyJhbGciOiAiSFMyNTYiLCAidHlwIjogIkpXVCJ9.eyJ3NC13dy10ZW5hbnQiOiAiOTQ0NiIsICJ3NC13dy1lbnYiOiAidGVzdCIsICJ3NC13dy1nZW1lZW50ZSI6ICIwNTk5In0.dummysignature";

    private TestWebApplicationFactory? _factory;

    [TearDown]
    public void TearDown()
    {
        _factory?.Dispose();
    }

    [TestCase(
        DevToken0518,
        "9446-dev-0518",
        "dev",
        "0518",
        "Sample setting for tenant 9446 in dev environment for gemeente 0518"
    )]
    [TestCase(
        TestToken0599,
        "9446-test-0599",
        "test",
        "0599",
        "Sample setting for tenant 9446 in test environment for gemeente 0599"
    )]
    public async Task GetTenantInfo_WithToken_ShouldReturnTenantData(
        string token,
        string expectedTenantIdentifier,
        string expectedEnvironmentName,
        string expectedGemeenteCode,
        string expectedCustomSetting)
    {
        _factory = new TestWebApplicationFactory();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/tenant-info");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var payload = await ParseAsObjectAsync(response);
        Assert.That(payload["message"]?.GetValue<string>(), Is.EqualTo("Tenant information successfully resolved from JWT claims"));
        Assert.That(payload["tenantIdentifier"]?.GetValue<string>(), Is.EqualTo(expectedTenantIdentifier));
        Assert.That(payload["tenantCode"]?.GetValue<string>(), Is.EqualTo("9446"));
        Assert.That(payload["environmentName"]?.GetValue<string>(), Is.EqualTo(expectedEnvironmentName));
        Assert.That(payload["gemeenteCode"]?.GetValue<string>(), Is.EqualTo(expectedGemeenteCode));
        Assert.That(payload["customSetting"]?.GetValue<string>(), Is.EqualTo(expectedCustomSetting));
    }

    private static async Task<JsonObject> ParseAsObjectAsync(HttpResponseMessage response)
    {
        var payloadText = await response.Content.ReadAsStringAsync();
        var payload = JsonNode.Parse(payloadText) as JsonObject;

        Assert.That(payload, Is.Not.Null, "Response body is not a valid JSON object.");
        return payload!;
    }
}


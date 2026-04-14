using System.Net;
using System.Text.Json.Nodes;

namespace Wigo4it.MultiTenant.AspNetCore.IntegrationTests;

public class TenantInfoEndpointsFromHeadersTests
{
    private const string DummyToken =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ3NC13dy10ZW5hbnQiOiI5NDQ2IiwidzQtd3ctZW52IjoicHJkIiwidzQtd3ctZ2VtZWVudGUiOiIwMTIzIn0.VtgcRY4Le4JCeNDkx1TgRzCdph7l85FuF9pLAoZoXeQ";

    private TestWebApplicationFactory? _factory;

    [TearDown]
    public void TearDown()
    {
        _factory?.Dispose();
    }

    [TestCase(
        "9446",
        "dev",
        "0518",
        "9446-dev-0518",
        "dev",
        "0518",
        "Sample setting for tenant 9446 in dev environment for gemeente 0518"
    )]
    [TestCase(
        "9446",
        "test",
        "0599",
        "9446-test-0599",
        "test",
        "0599",
        "Sample setting for tenant 9446 in test environment for gemeente 0599"
    )]
    public async Task GetTenantInfo_WithHeaders_ShouldReturnTenantData(
        string tenantCode,
        string environmentName,
        string gemeenteCode,
        string expectedTenantIdentifier,
        string expectedEnvironmentName,
        string expectedGemeenteCode,
        string expectedCustomSetting
    )
    {
        _factory = new TestWebApplicationFactory(TenantIdResolutionStrategy.Headers);
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {DummyToken}");

        // Set the tenant identifier headers
        client.DefaultRequestHeaders.Add(MultitenancyIdentifiers.HttpHeaders.WegwijzerTenantCode, tenantCode);
        client.DefaultRequestHeaders.Add(MultitenancyIdentifiers.HttpHeaders.WegwijzerEnvironmentName, environmentName);
        client.DefaultRequestHeaders.Add(MultitenancyIdentifiers.HttpHeaders.GemeenteCode, gemeenteCode);

        var response = await client.GetAsync("/tenant-info");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var payload = await ParseAsObjectAsync(response);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(payload["tenantIdentifier"]?.GetValue<string>(), Is.EqualTo(expectedTenantIdentifier));
            Assert.That(payload["tenantCode"]?.GetValue<string>(), Is.EqualTo("9446"));
            Assert.That(payload["environmentName"]?.GetValue<string>(), Is.EqualTo(expectedEnvironmentName));
            Assert.That(payload["gemeenteCode"]?.GetValue<string>(), Is.EqualTo(expectedGemeenteCode));
            Assert.That(payload["customSetting"]?.GetValue<string>(), Is.EqualTo(expectedCustomSetting));
        }
    }

    private static async Task<JsonObject> ParseAsObjectAsync(HttpResponseMessage response)
    {
        var payloadText = await response.Content.ReadAsStringAsync();
        var payload = JsonNode.Parse(payloadText) as JsonObject;

        Assert.That(payload, Is.Not.Null, "Response body is not a valid JSON object.");
        return payload!;
    }
}

using Microsoft.AspNetCore.Http;

namespace Wigo4it.MultiTenant.AspNetCore.Tests;

[TestFixture]
public class AspNetCoreTenantIdFromHeadersResolverTests
{
    [Test]
    public async Task DetermineTenantIdentifier_WithExpectedHeaders_ReturnsIdentifier()
    {
        var context = CreateContextWithHeaders(
            (MultitenancyIdentifiers.HttpHeaders.WegwijzerTenantCode, "9446"),
            (MultitenancyIdentifiers.HttpHeaders.WegwijzerEnvironmentName, "0518pr1"),
            (MultitenancyIdentifiers.HttpHeaders.GemeenteCode, "0001")
        );

        var identifier = await AspNetCoreTenantIdFromHeadersResolver.DetermineTenantIdentifier(context);

        Assert.That(identifier, Is.EqualTo("9446-0518pr1-0001"));
    }

    [Test]
    public async Task DetermineTenantIdentifier_WithMissingHeader_ReturnsNull()
    {
        var context = CreateContextWithHeaders(
            (MultitenancyIdentifiers.HttpHeaders.WegwijzerTenantCode, "9446"),
            (MultitenancyIdentifiers.HttpHeaders.WegwijzerEnvironmentName, "0518pr1")
        );

        var identifier = await AspNetCoreTenantIdFromHeadersResolver.DetermineTenantIdentifier(context);

        Assert.That(identifier, Is.Null);
    }

    [Test]
    public async Task DetermineTenantIdentifier_WithEmptyHeaderValue_ReturnsNull()
    {
        var context = CreateContextWithHeaders(
            (MultitenancyIdentifiers.HttpHeaders.WegwijzerTenantCode, "9446"),
            (MultitenancyIdentifiers.HttpHeaders.WegwijzerEnvironmentName, "   "),
            (MultitenancyIdentifiers.HttpHeaders.GemeenteCode, "0001")
        );

        var identifier = await AspNetCoreTenantIdFromHeadersResolver.DetermineTenantIdentifier(context);

        Assert.That(identifier, Is.Null);
    }

    private static DefaultHttpContext CreateContextWithHeaders(params (string Key, string Value)[] headers)
    {
        var context = new DefaultHttpContext();
        foreach (var header in headers)
        {
            context.Request.Headers[header.Key] = header.Value;
        }

        return context;
    }
}

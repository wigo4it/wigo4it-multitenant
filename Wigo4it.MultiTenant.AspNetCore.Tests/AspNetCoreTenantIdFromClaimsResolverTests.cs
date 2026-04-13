using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Wigo4it.MultiTenant.AspNetCore;

namespace Wigo4it.MultiTenant.AspNetCore.Tests;

[TestFixture]
public class AspNetCoreTenantIdFromClaimsResolverTests
{
    [Test]
    public async Task DetermineTenantIdentifier_WithAuthenticatedUserAndClaims_ReturnsIdentifier()
    {
        var context = CreateContextWithClaims(
            (MultitenancyClaims.WegwijzerTenantCode, "9446"),
            (MultitenancyClaims.WegwijzerEnvironmentName, "0518pr1"),
            (MultitenancyClaims.GemeenteCode, "0001")
        );

        var identifier = await AspNetCoreTenantIdFromClaimsResolver.DetermineTenantIdentifier(context);

        Assert.That(identifier, Is.EqualTo("9446-0518pr1-0001"));
    }
    
    [Test]
    public async Task DetermineTenantIdentifier_WithMissingClaim_ReturnsNull()
    {
        var context = CreateContextWithClaims(
            (MultitenancyClaims.WegwijzerTenantCode, "9446"),
            (MultitenancyClaims.WegwijzerEnvironmentName, "0518pr1")
        );

        var identifier = await AspNetCoreTenantIdFromClaimsResolver.DetermineTenantIdentifier(context);

        Assert.That(identifier, Is.Null);
    }

    [Test]
    public async Task DetermineTenantIdentifier_WithUnauthenticatedUser_ReturnsNull()
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity()),
        };

        var identifier = await AspNetCoreTenantIdFromClaimsResolver.DetermineTenantIdentifier(context);

        Assert.That(identifier, Is.Null);
    }

    private static DefaultHttpContext CreateContextWithClaims(params (string Type, string Value)[] claims)
    {
        var identity = new ClaimsIdentity("test-auth");
        foreach (var claim in claims)
        {
            identity.AddClaim(new Claim(claim.Type, claim.Value));
        }

        return new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity),
        };
    }
}


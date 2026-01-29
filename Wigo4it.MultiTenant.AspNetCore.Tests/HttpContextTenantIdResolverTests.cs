using Microsoft.AspNetCore.Http;
using Wigo4it.MultiTenant.AspNetCore;

namespace Wigo4it.MultiTenant.AspNetCore.Tests;

[TestFixture]
public class HttpContextTenantIdResolverTests
{
    [Test]
    public async Task DetermineTenantIdentifier_WithValidHeaders_ReturnsTenantIdentifier()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[MultitenancyHeaders.WegwijzerTenantCode] = "9446";
        httpContext.Request.Headers[MultitenancyHeaders.WegwijzerEnvironmentName] = "0518pr1";
        httpContext.Request.Headers[MultitenancyHeaders.GemeenteCode] = "0001";

        // Act
        var result = await HttpContextTenantIdResolver.DetermineTenantIdentifier(httpContext);

        // Assert
        Assert.That(result, Is.EqualTo("9446-0518pr1-0001"));
    }

    [Test]
    [TestCaseSource(nameof(ValidTenantIdentifierCases))]
    public void CaptureTenantIdentifier_WithValidHeaders_ReturnsCorrectFormat(
        string tenantCode,
        string environmentName,
        string gemeenteCode,
        string expectedResult)
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[MultitenancyHeaders.WegwijzerTenantCode] = tenantCode;
        httpContext.Request.Headers[MultitenancyHeaders.WegwijzerEnvironmentName] = environmentName;
        httpContext.Request.Headers[MultitenancyHeaders.GemeenteCode] = gemeenteCode;

        // Act
        var result = httpContext.CaptureTenantIdentifier();

        // Assert
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [Test]
    [TestCaseSource(nameof(MissingHeaderCases))]
    public void CaptureTenantIdentifier_WithMissingHeaders_ReturnsStringWithEmptyParts(
        Dictionary<string, string> headers,
        string expectedResult)
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        foreach (var header in headers)
        {
            httpContext.Request.Headers[header.Key] = header.Value;
        }

        // Act
        var result = httpContext.CaptureTenantIdentifier();

        // Assert
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    private static IEnumerable<TestCaseData> ValidTenantIdentifierCases()
    {
        yield return new TestCaseData("9446", "0518pr1", "0001", "9446-0518pr1-0001")
            .SetName("Tenant 9446 with 0518pr1 environment");
        
        yield return new TestCaseData("0518", "0363ac2", "0002", "0518-0363ac2-0002")
            .SetName("Tenant 0518 with 0363ac2 environment");
        
        yield return new TestCaseData("0599", "0344so1", "1234", "0599-0344so1-1234")
            .SetName("Tenant 0599 with 0344so1 environment");
        
        yield return new TestCaseData("0344", "0599pr3", "0344", "0344-0599pr3-0344")
            .SetName("Tenant 0344 with 0599pr3 environment matching gemeente code");
        
        yield return new TestCaseData("0363", "0518si2", "0363", "0363-0518si2-0363")
            .SetName("Tenant 0363 with 0518si2 environment matching gemeente code");
        
        yield return new TestCaseData("9446", "0363pr1", "0599", "9446-0363pr1-0599")
            .SetName("Tenant 9446 with 0363pr1 environment");
        
        yield return new TestCaseData("0518", "0344ac1", "0518", "0518-0344ac1-0518")
            .SetName("Tenant 0518 with 0344ac1 environment");
        
        yield return new TestCaseData("0599", "0599so2", "123456", "0599-0599so2-123456")
            .SetName("Tenant 0599 with 0599so2 environment and long gemeente code");
        
        yield return new TestCaseData("0344", "0518si3", "9999", "0344-0518si3-9999")
            .SetName("Tenant 0344 with 0518si3 environment");
    }

    private static IEnumerable<TestCaseData> MissingHeaderCases()
    {
        yield return new TestCaseData(
            new Dictionary<string, string>
            {
                [MultitenancyHeaders.WegwijzerEnvironmentName] = "0518pr1",
                [MultitenancyHeaders.GemeenteCode] = "0001"
            },
            "-0518pr1-0001")
            .SetName("Missing tenant code header");
        
        yield return new TestCaseData(
            new Dictionary<string, string>
            {
                [MultitenancyHeaders.WegwijzerTenantCode] = "9446",
                [MultitenancyHeaders.GemeenteCode] = "0001"
            },
            "9446--0001")
            .SetName("Missing environment name header");
        
        yield return new TestCaseData(
            new Dictionary<string, string>
            {
                [MultitenancyHeaders.WegwijzerTenantCode] = "0518",
                [MultitenancyHeaders.WegwijzerEnvironmentName] = "0363ac2"
            },
            "0518-0363ac2-")
            .SetName("Missing gemeente code header");
        
        yield return new TestCaseData(
            new Dictionary<string, string>(),
            "--")
            .SetName("Empty headers dictionary");
        
        yield return new TestCaseData(
            new Dictionary<string, string>
            {
                ["SomeOtherHeader"] = "value"
            },
            "--")
            .SetName("Irrelevant headers only");
    }
}

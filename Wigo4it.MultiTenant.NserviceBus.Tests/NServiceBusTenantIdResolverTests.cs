using NServiceBus.Transport;
using Wigo4it.MultiTenant.NServiceBus;

namespace Wigo4it.MultiTenant.NserviceBus.Tests;

[TestFixture]
public class NServiceBusTenantIdResolverTests
{
    [Test]
    public async Task DetermineTenantIdentifier_WithValidHeaders_ReturnsTenantIdentifier()
    {
        // Arrange
        var headers = new Dictionary<string, string>
        {
            [MultitenancyHeaders.WegwijzerTenantCode] = "9446",
            [MultitenancyHeaders.WegwijzerEnvironmentName] = "0518pr1",
            [MultitenancyHeaders.GemeenteCode] = "0001",
        };
        var message = new IncomingMessage("messageId", headers, Array.Empty<byte>());
        var context = new TestIncomingPhysicalMessageContext(message);

        // Act
        var result = await NServiceBusTenantIdResolver.DetermineTenantIdentifier(context);

        // Assert
        Assert.That(result, Is.EqualTo("9446-0518pr1-0001"));
    }

    [Test]
    [TestCaseSource(nameof(ValidTenantIdentifierCases))]
    public void CaptureTenantIdentifier_WithValidHeaders_ReturnsCorrectFormat(
        string tenantCode,
        string environmentName,
        string gemeenteCode,
        string expectedResult
    )
    {
        // Arrange
        var headers = new Dictionary<string, string>
        {
            [MultitenancyHeaders.WegwijzerTenantCode] = tenantCode,
            [MultitenancyHeaders.WegwijzerEnvironmentName] = environmentName,
            [MultitenancyHeaders.GemeenteCode] = gemeenteCode,
        };
        var message = new IncomingMessage("messageId", headers, Array.Empty<byte>());

        // Act
        var result = message.CaptureTenantIdentifier();

        // Assert
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [Test]
    [TestCaseSource(nameof(MissingHeaderCases))]
    public void CaptureTenantIdentifier_WithMissingHeaders_ThrowsKeyNotFoundException(
        Dictionary<string, string> headers,
        string testDescription
    )
    {
        // Arrange
        var message = new IncomingMessage("messageId", headers, Array.Empty<byte>());

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => message.CaptureTenantIdentifier());
    }

    [Test]
    [TestCaseSource(nameof(NullHeaderValueCases))]
    public void CaptureTenantIdentifier_WithNullHeaderValues_ReturnsStringWithNull(
        Dictionary<string, string?> headers,
        string expectedResult
    )
    {
        // Arrange
        var message = new IncomingMessage("messageId", headers, Array.Empty<byte>());

        // Act
        var result = message.CaptureTenantIdentifier();

        // Assert
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    private static IEnumerable<TestCaseData> ValidTenantIdentifierCases()
    {
        yield return new TestCaseData("9446", "0518pr1", "0001", "9446-0518pr1-0001").SetName(
            "Tenant 9446 with 0518pr1 environment"
        );

        yield return new TestCaseData("0518", "0363ac2", "0002", "0518-0363ac2-0002").SetName(
            "Tenant 0518 with 0363ac2 environment"
        );

        yield return new TestCaseData("0599", "0344so1", "1234", "0599-0344so1-1234").SetName(
            "Tenant 0599 with 0344so1 environment"
        );

        yield return new TestCaseData("0344", "0599pr3", "0344", "0344-0599pr3-0344").SetName(
            "Tenant 0344 with 0599pr3 environment matching gemeente code"
        );

        yield return new TestCaseData("0363", "0518si2", "0363", "0363-0518si2-0363").SetName(
            "Tenant 0363 with 0518si2 environment matching gemeente code"
        );

        yield return new TestCaseData("9446", "0363pr1", "0599", "9446-0363pr1-0599").SetName(
            "Tenant 9446 with 0363pr1 environment"
        );

        yield return new TestCaseData("0518", "0344ac1", "0518", "0518-0344ac1-0518").SetName(
            "Tenant 0518 with 0344ac1 environment"
        );

        yield return new TestCaseData("0599", "0599so2", "123456", "0599-0599so2-123456").SetName(
            "Tenant 0599 with 0599so2 environment and long gemeente code"
        );

        yield return new TestCaseData("0344", "0518si3", "9999", "0344-0518si3-9999").SetName(
            "Tenant 0344 with 0518si3 environment"
        );
    }

    private static IEnumerable<TestCaseData> MissingHeaderCases()
    {
        yield return new TestCaseData(
            new Dictionary<string, string>
            {
                [MultitenancyHeaders.WegwijzerEnvironmentName] = "0518pr1",
                [MultitenancyHeaders.GemeenteCode] = "0001",
            },
            "Missing WegwijzerTenantCode header"
        ).SetName("Missing tenant code header");

        yield return new TestCaseData(
            new Dictionary<string, string>
            {
                [MultitenancyHeaders.WegwijzerTenantCode] = "9446",
                [MultitenancyHeaders.GemeenteCode] = "0001",
            },
            "Missing WegwijzerEnvironmentName header"
        ).SetName("Missing environment name header");

        yield return new TestCaseData(
            new Dictionary<string, string>
            {
                [MultitenancyHeaders.WegwijzerTenantCode] = "0518",
                [MultitenancyHeaders.WegwijzerEnvironmentName] = "0363ac2",
            },
            "Missing GemeenteCode header"
        ).SetName("Missing gemeente code header");

        yield return new TestCaseData(new Dictionary<string, string>(), "No headers present").SetName("Empty headers dictionary");

        yield return new TestCaseData(
            new Dictionary<string, string> { ["SomeOtherHeader"] = "value" },
            "Only irrelevant headers present"
        ).SetName("Irrelevant headers only");
    }

    private static IEnumerable<TestCaseData> NullHeaderValueCases()
    {
        yield return new TestCaseData(
            new Dictionary<string, string?>
            {
                [MultitenancyHeaders.WegwijzerTenantCode] = null,
                [MultitenancyHeaders.WegwijzerEnvironmentName] = "0344so1",
                [MultitenancyHeaders.GemeenteCode] = "0001",
            },
            "-0344so1-0001"
        ).SetName("Null tenant code value");

        yield return new TestCaseData(
            new Dictionary<string, string?>
            {
                [MultitenancyHeaders.WegwijzerTenantCode] = "0599",
                [MultitenancyHeaders.WegwijzerEnvironmentName] = null,
                [MultitenancyHeaders.GemeenteCode] = "0001",
            },
            "0599--0001"
        ).SetName("Null environment name value");

        yield return new TestCaseData(
            new Dictionary<string, string?>
            {
                [MultitenancyHeaders.WegwijzerTenantCode] = "0363",
                [MultitenancyHeaders.WegwijzerEnvironmentName] = "0599pr2",
                [MultitenancyHeaders.GemeenteCode] = null,
            },
            "0363-0599pr2-"
        ).SetName("Null gemeente code value");
    }
}

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Configuration;

namespace Wigo4it.MultiTenant.Tests;

[TestFixture]
public class DictionaryConfigurationStoreTests
{
    private IConfiguration _configuration;
    private DictionaryConfigurationStore<TestTenantInfo> _sut;

    [SetUp]
    public void SetUp()
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["tenants:9446:environments:xyz:defaults:tenantcode"] = "9446",
                    ["tenants:9446:environments:xyz:defaults:environmentname"] = "xyz",
                    ["tenants:9446:environments:xyz:defaults:info:someproperty"] = "true",
                    ["tenants:9446:environments:xyz:gemeenten:0599:identifier"] = "9446-xyz-0599",
                    ["tenants:9446:environments:xyz:gemeenten:0606:identifier"] = "9446-xyz-0606",
                    ["tenants:9446:environments:xyz:gemeenten:0606:info:someproperty"] = "false",
                }
            )
            .Build();

        _sut = new DictionaryConfigurationStore<TestTenantInfo>(_configuration);
    }

    [Test]
    public async Task GetById_should_be_same_as_GetByIdentifier()
    {
        var byIdentifier = await _sut.GetByIdentifierAsync("9446-xyz-0599");
        var byId = await _sut.GetAsync("9446-xyz-0599");

        Assert.That(byIdentifier, Is.SameAs(byId));
    }

    [Test]
    public async Task GetByIdentifier_should_return_tenant()
    {
        var tenant0599 = await _sut.GetByIdentifierAsync("9446-xyz-0599");
        Assert.That(tenant0599, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(tenant0599.GemeenteCode, Is.EqualTo("0599"));
            Assert.That(tenant0599.EnvironmentName, Is.EqualTo("xyz"));
            Assert.That(tenant0599.TenantCode, Is.EqualTo("9446"));
        });

        var tenant0606 = await _sut.GetByIdentifierAsync("9446-xyz-0606");
        Assert.That(tenant0606, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(tenant0606.GemeenteCode, Is.EqualTo("0606"));
            Assert.That(tenant0606.EnvironmentName, Is.EqualTo("xyz"));
            Assert.That(tenant0606.TenantCode, Is.EqualTo("9446"));
        });
    }

    [Test]
    public async Task GetByIdentifier_should_return_with_defaults_if_not_overriden()
    {
        var tenant = await _sut.GetByIdentifierAsync("9446-xyz-0599");
        Assert.That(tenant, Is.Not.Null);
        Assert.That(tenant.Info, Is.Not.Null);
        Assert.That(tenant.Info.SomeProperty, Is.True);
    }

    [Test]
    public async Task GetByIdentifier_should_return_with_overriden_values()
    {
        var tenant = await _sut.GetByIdentifierAsync("9446-xyz-0606");
        Assert.That(tenant, Is.Not.Null);
        Assert.That(tenant.Info, Is.Not.Null);
        Assert.That(tenant.Info.SomeProperty, Is.False);
    }

    [Test]
    public async Task GetByIdentifier_should_return_null_for_unknown_identifier()
    {
        var tenant = await _sut.GetByIdentifierAsync("9446-xyz-9999");
        Assert.That(tenant, Is.Null);
    }

    [Test]
    public async Task GetAll_should_return_all_tenants()
    {
        var tenants = await _sut.GetAllAsync();
        Assert.That(tenants.ToList(), Has.Count.EqualTo(2));
    }

    [Test]
    public void Constructor_should_not_throw_when_no_tenants()
    {
        _configuration = new ConfigurationBuilder().Build();
        Assert.DoesNotThrow(() => _sut = new DictionaryConfigurationStore<TestTenantInfo>(_configuration));
    }

    [Test]
    public async Task GetById_should_return_null_when_no_tenants()
    {
        _configuration = new ConfigurationBuilder().Build();
        var noTenantsSut = new DictionaryConfigurationStore(_configuration);
        Assert.That(await noTenantsSut.GetAsync("abc"), Is.Null);
    }

    [Test]
    public async Task GetByIdentifier_should_return_null_when_no_tenants()
    {
        _configuration = new ConfigurationBuilder().Build();
        var noTenantsSut = new DictionaryConfigurationStore(_configuration);
        Assert.That(await noTenantsSut.GetByIdentifierAsync("abc"), Is.Null);
    }

    [Test]
    public async Task GetAll_should_return_empty_when_no_tenants()
    {
        _configuration = new ConfigurationBuilder().Build();
        var noTenantsSut = new DictionaryConfigurationStore(_configuration);
        Assert.That(await noTenantsSut.GetAllAsync(), Is.Empty);
    }

    [Test]
    public async Task Id_should_not_be_null()
    {
        // This is important for Finbuckle IOptions caching to work correctly
        ITenantInfo? tenant0599 = await _sut.GetByIdentifierAsync("9446-xyz-0599");
        Assert.That(tenant0599, Is.Not.Null);

        Assert.That(tenant0599.Id, Is.EqualTo("9446-xyz-0599"));
    }
}

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

        using (Assert.EnterMultipleScope())
        {
            Assert.That(tenant0599.Options.GemeenteCode, Is.EqualTo("0599"));
            Assert.That(tenant0599.Options.EnvironmentName, Is.EqualTo("xyz"));
            Assert.That(tenant0599.Options.TenantCode, Is.EqualTo("9446"));
        }

        var tenant0606 = await _sut.GetByIdentifierAsync("9446-xyz-0606");
        Assert.That(tenant0606, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(tenant0606.Options.GemeenteCode, Is.EqualTo("0606"));
            Assert.That(tenant0606.Options.EnvironmentName, Is.EqualTo("xyz"));
            Assert.That(tenant0606.Options.TenantCode, Is.EqualTo("9446"));
        }
    }

    [Test]
    public async Task GetByIdentifier_should_return_with_defaults_if_not_overriden()
    {
        var tenant = await _sut.GetByIdentifierAsync("9446-xyz-0599");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(tenant, Is.Not.Null);
            Assert.That(tenant?.Info, Is.Not.Null);
            Assert.That(tenant?.Info.SomeProperty, Is.True);
        }
    }

    [Test]
    public async Task GetByIdentifier_should_return_with_overriden_values()
    {
        var tenant = await _sut.GetByIdentifierAsync("9446-xyz-0606");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(tenant, Is.Not.Null);
            Assert.That(tenant?.Info, Is.Not.Null);
            Assert.That(tenant?.Info.SomeProperty, Is.False);
        }
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

    [Test]
    public async Task GetAllAsync_with_take_and_skip_should_return_correct_page()
    {
        var tenants = await _sut.GetAllAsync(take: 1, skip: 0);
        var tenantList = tenants.ToList();

        Assert.That(tenantList, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task GetAllAsync_with_take_and_skip_should_skip_correct_number()
    {
        var firstPage = await _sut.GetAllAsync(take: 1, skip: 0);
        var secondPage = await _sut.GetAllAsync(take: 1, skip: 1);

        var firstTenant = firstPage.FirstOrDefault();
        var secondTenant = secondPage.FirstOrDefault();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(firstTenant, Is.Not.Null);
            Assert.That(secondTenant, Is.Not.Null);
            Assert.That(firstTenant?.Identifier, Is.Not.EqualTo(secondTenant?.Identifier));
        }
    }

    [Test]
    public async Task GetAllAsync_with_take_and_skip_should_return_all_when_take_exceeds_count()
    {
        var tenants = await _sut.GetAllAsync(take: 10, skip: 0);
        var tenantList = tenants.ToList();

        Assert.That(tenantList, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetAllAsync_with_take_and_skip_should_return_empty_when_skip_exceeds_count()
    {
        var tenants = await _sut.GetAllAsync(take: 10, skip: 10);
        var tenantList = tenants.ToList();

        Assert.That(tenantList, Is.Empty);
    }

    [Test]
    public async Task GetAllAsync_with_take_and_skip_should_return_empty_when_take_is_zero()
    {
        var tenants = await _sut.GetAllAsync(take: 0, skip: 0);
        var tenantList = tenants.ToList();

        Assert.That(tenantList, Is.Empty);
    }

    [Test]
    public async Task GetAllAsync_with_take_and_skip_should_return_all_from_skip_when_take_exceeds_remaining()
    {
        var tenants = await _sut.GetAllAsync(take: 10, skip: 1);
        var tenantList = tenants.ToList();

        Assert.That(tenantList, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task GetAllAsync_with_take_and_skip_should_return_empty_when_no_tenants()
    {
        _configuration = new ConfigurationBuilder().Build();
        var noTenantsSut = new DictionaryConfigurationStore(_configuration);

        var tenants = await noTenantsSut.GetAllAsync(take: 10, skip: 0);

        Assert.That(tenants, Is.Empty);
    }

    [Test]
    public async Task GetAllAsync_with_take_and_skip_should_return_all_tenants_when_skip_is_zero_and_take_covers_all()
    {
        var tenants = await _sut.GetAllAsync(take: 2, skip: 0);
        var tenantList = tenants.ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(tenantList, Has.Count.EqualTo(2));
            Assert.That(tenantList.Select(t => t.Options.GemeenteCode), Is.EquivalentTo(["0599", "0606"]));
        }
    }
}

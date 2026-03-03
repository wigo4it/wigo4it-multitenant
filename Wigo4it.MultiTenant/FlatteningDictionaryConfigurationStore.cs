using System.Globalization;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Primitives;

namespace Wigo4it.MultiTenant;

/// <summary>
/// We willen afwijken van de verplichte structuur uit de Finbuckle.MultiTenant library. Deze verwacht namelijk een lijst/array in IConfiguration.
/// Om een array in een keyvault secret of env variable in te vullen is een volgnummer nodig, echter hebben we vanuit een Wegwijzer deployment
/// maar zicht hebben op 1 omgeving, en kunnen we dus niet het volgnummer bepalen.
///
/// Daarnaast willen we getrapt waarden in kunnen stellen. Dit betekent dat we instellingen op omgevingniveau of op gemeenteniveau willen
/// kunnen bepalen. Zo houden we het aantal instellingen beperkt. Het beperken van dit aantal is belangrijk omdat AddAzureConfiguration
/// bij het opstarten alle keys wilt inlezen. Dit zou dan waarschijnlijk leiden tot throttling / failed starts vanuit Azure.
///
/// In de wegwijzer en deze library heeft het woord "Tenant" verschillende betekenissen. In de Wegwijzer is een tenant
/// een G4 gemeente / Wigo4it. In deze library is `Wigo4itTenantInfo` een (rand)gemeente, in deze class wordt de vertaling gedaan.
///
/// Deze code is grotendeels geinspireerd door
/// https://github.com/Finbuckle/Finbuckle.MultiTenant/blob/eafec795fe93cf6e77a855e5cae7ea124d1a5557/src/Finbuckle.MultiTenant/Stores/ConfigurationStore.cs
/// </summary>
public class FlatteningDictionaryConfigurationStore : IMultiTenantStore<FlattendConfigTennantInfo>
{
    private readonly IConfiguration _sectie;
    private Dictionary<string, FlattendConfigTennantInfo>? _tenantMap;

    private const string ConfiguratieSectie = "Tenants";
    private const string EnvironmentsSectie = "Environments";
    private const string GemeentenSectie = "Gemeenten";

    private static readonly string[] StructuralSections =
        [ConfiguratieSectie, EnvironmentsSectie, GemeentenSectie];
    

    public FlatteningDictionaryConfigurationStore(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        _sectie = configuration;

        UpdateTenantMap();
        ChangeToken.OnChange(configuration.GetReloadToken, UpdateTenantMap);
    }

    private void UpdateTenantMap()
    {
        var tenants =
            from wegwijzerTenant in _sectie.GetSection(ConfiguratieSectie).GetChildren()
            from environment in wegwijzerTenant.GetSection(EnvironmentsSectie).GetChildren()
            from gemeente in environment.GetSection(GemeentenSectie).GetChildren()
            select new FlattendConfigTennantInfo(
                identifier : gemeente.GetValue<string>("identifier")!,
                lazyConfiguration: new Lazy<IConfiguration>(()=> MergeSections([_sectie, environment, gemeente])));

        _tenantMap = tenants.ToDictionary(x => x.Identifier, StringComparer.InvariantCultureIgnoreCase);
    }


    private static IConfiguration MergeSections(IConfiguration[] sections)
    {
        var all = sections.Reverse().SelectMany(GetLeafValues).DistinctBy(kv=>kv.Key);

        var provider = new MemoryConfigurationProvider(
            new MemoryConfigurationSource { InitialData = all! });
        
        return new ConfigurationRoot([provider]);
    }

    private static IEnumerable<KeyValuePair<string, string>> GetLeafValues(IConfiguration section)
    {
        var basePath = (section as IConfigurationSection)?.Path;

        return section.AsEnumerable()
            .Where(kvp => kvp.Value is not null)
            .Select(kvp => KeyValuePair.Create(StripPrefix(kvp.Key, basePath), kvp.Value!))
            .Where(kvp => !string.IsNullOrEmpty(kvp.Key))
            .Where(kvp => !IsStructuralSection(kvp.Key));
    }

    private static string StripPrefix(string key, string? prefix)
        => string.IsNullOrEmpty(prefix) ? key : key[prefix.Length..].TrimStart(':');

    private static bool IsStructuralSection(string relativePath)
        => StructuralSections.Any(s => relativePath.StartsWith(s + ":", StringComparison.OrdinalIgnoreCase));


    public Task<FlattendConfigTennantInfo?> GetAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return Task.FromResult(_tenantMap?.GetValueOrDefault(id));
    }

    public async Task<IEnumerable<FlattendConfigTennantInfo>> GetAllAsync() => await Task.FromResult(GetAll());

    public async Task<IEnumerable<FlattendConfigTennantInfo>> GetAllAsync(int take, int skip) => await Task.FromResult(GetAll().Skip(skip).Take(take));

    private List<FlattendConfigTennantInfo> GetAll() => _tenantMap?.Select(x => x.Value).ToList() ?? [];

    public Task<FlattendConfigTennantInfo?> GetByIdentifierAsync(string identifier) => Task.FromResult(GetByIdentifier(identifier));

    private FlattendConfigTennantInfo? GetByIdentifier(string identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        return _tenantMap?.GetValueOrDefault(identifier);
    }

    public Task<bool> AddAsync(FlattendConfigTennantInfo tenantInfo) => throw new NotSupportedException();

    public Task<bool> RemoveAsync(string identifier) => throw new NotSupportedException();

    public Task<bool> UpdateAsync(FlattendConfigTennantInfo tenantInfo) => throw new NotSupportedException();
}


public class FlattendConfigTennantInfo(string identifier, Lazy<IConfiguration> lazyConfiguration) : ITenantInfo
{
    /// <summary>
    /// Deze gebruiken we zelf niet, is slechts verplicht vanuit ITenantInfo
    /// </summary>
    string ITenantInfo.Id => identifier;

    /// <summary>
    /// {Wegwijzer TenantCode}-{Wegwijzer EnvironmentName}-{GemeenteCode}
    /// </summary>
    public string Identifier => identifier;
    
    public IConfiguration Configuration => lazyConfiguration.Value;
    
}




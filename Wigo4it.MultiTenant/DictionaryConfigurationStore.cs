using System.Globalization;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Configuration;
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
public class DictionaryConfigurationStore<TTenantInfo> : IMultiTenantStore<TTenantInfo> where TTenantInfo : Wigo4itTenantInfo
{
    private readonly IConfigurationSection _sectie;
    private Dictionary<string, TTenantInfo>? _tenantMap;

    private const string ConfiguratieSectie = "Tenants";
    private const string EnvironmentsSectie = "Environments";
    private const string DefaultsSectie = "Defaults";
    private const string GemeentenSectie = "Gemeenten";

    public DictionaryConfigurationStore(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        _sectie = configuration.GetSection(ConfiguratieSectie);

        UpdateTenantMap();
        ChangeToken.OnChange(_sectie.GetReloadToken, UpdateTenantMap);
    }

    private void UpdateTenantMap()
    {
        var tenants =
            from wegwijzerTenant in _sectie.GetChildren()
            from environment in wegwijzerTenant.GetSection(EnvironmentsSectie).GetChildren()
            from gemeenteTenantSectie in environment.GetSection(GemeentenSectie).GetChildren()
            let defaultTenant = environment
                .GetSection(DefaultsSectie)
                .Get<TTenantInfo>(options => options.BindNonPublicProperties = true)
            let specificTenant = OverrideDefaults(defaultTenant, gemeenteTenantSectie)
            select specificTenant with
            {
                EnvironmentName = environment.Key,
                TenantCode = wegwijzerTenant.Key,
                GemeenteCode = gemeenteTenantSectie.Key,
            };

        _tenantMap = tenants.ToDictionary(
            x => x.Identifier?.ToLower() ?? throw new ArgumentException("Tenant without Identifier found in config."),
            x => x
        );
    }

    private static TTenantInfo OverrideDefaults(
        TTenantInfo gemeenteTenantWithDefaults,
        IConfigurationSection gemeenteTenantSectie
    )
    {
        gemeenteTenantSectie.Bind(gemeenteTenantWithDefaults, options => options.BindNonPublicProperties = true);
        return gemeenteTenantWithDefaults;
    }

    public Task<TTenantInfo?> GetAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return Task.FromResult(_tenantMap?.GetValueOrDefault(id.ToLower(CultureInfo.InvariantCulture)));
    }

    public async Task<IEnumerable<TTenantInfo>> GetAllAsync()
    {
        return await Task.FromResult(GetAll());
    }
    
    public async Task<IEnumerable<TTenantInfo>> GetAllAsync(int take, int skip)
    {
        return await Task.FromResult(GetAll().Skip(skip).Take(take));
    }

    private List<TTenantInfo> GetAll()
    {
        return _tenantMap?.Select(x => x.Value).ToList() ?? [];
    }

    public Task<TTenantInfo?> GetByIdentifierAsync(string identifier)
    {
        return Task.FromResult(GetByIdentifier(identifier));
    }

    private TTenantInfo? GetByIdentifier(string identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        return _tenantMap?.GetValueOrDefault(identifier.ToLower(CultureInfo.InvariantCulture));
    }
    
    public Task<bool> AddAsync(TTenantInfo tenantInfo)
    {
        throw new NotSupportedException();
    }

    public Task<bool> RemoveAsync(string identifier)
    {
        throw new NotSupportedException();
    }

    public Task<bool> UpdateAsync(TTenantInfo tenantInfo)
    {
        throw new NotSupportedException();
    }
}

public class DictionaryConfigurationStore : DictionaryConfigurationStore<Wigo4itTenantInfo>
{
    public DictionaryConfigurationStore(IConfiguration configuration)
        : base(configuration)
    {
    }
}

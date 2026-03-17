using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Wigo4it.MultiTenant;

/// <summary>
/// Implementatie van IConfiguration die meerdere IConfiguration's wrapt.
/// De eerste IConfiguration die een waarde heeft voor een key, heeft prioriteit.
/// </summary>
class MultiLevelConfiguration(params IReadOnlyCollection<IConfiguration> inner) : IConfiguration
{
    public IEnumerable<IConfigurationSection> GetChildren() =>
        // We moeten de child secties van alle inner IConfiguration's teruggeven,
        // Omdat dezelfde child-sectie op meerdere niveaus kan voorkomen groeperen we die dan tot 1 MultiLevelConfigurationSection.
        inner
            .SelectMany(l => l.GetChildren()
                .Where(FilterSubSections))
            .GroupBy(s => s.Key)
            .Select(g=> new MultiLevelConfigurationSection(g.ToArray()));
    
    private bool FilterSubSections(IConfigurationSection section) => !SectionNames.All.Contains(section.Key);

    public IChangeToken GetReloadToken() => inner.First().GetReloadToken();

    public IConfigurationSection GetSection(string key) =>
        new MultiLevelConfigurationSection(inner.Select(l => l.GetSection(key)).ToArray());

    public string? this[string key]
    {
        get => inner.Select(l=>l[key]).FirstOrDefault(v => v is not null);
        set => inner.FirstOrDefault()?[key] = value;
    }
    
    class MultiLevelConfigurationSection(IReadOnlyCollection<IConfigurationSection> inner) : MultiLevelConfiguration(inner) , IConfigurationSection
    {
        public string Key => inner.First().Key;
        public string Path => inner.First().Path;
        public string? Value
        {
            get => inner.Select(l => l.Value).FirstOrDefault(v => v is not null);
            set => inner.FirstOrDefault()?.Value = value;
        }
    }
}
namespace Wigo4it.MultiTenant.NserviceBus.Tests;

/// <summary>
/// Bedoeld voor algemene informatie over de omgeving zoals bijvoorbeeld hoofdgemeente en tenant. Houdt deze class
/// vrij van connection strings, urls e.d.
/// </summary>
public record TestOptions
{
    /// <summary>
    /// {Wegwijzer TenantCode}-{Wegwijzer EnvironmentName}-{GemeenteCode}
    /// </summary>
    public string? Identifier { get; set; }

    /// <summary>
    /// Gebruikersvriendelijke naam van de omgeving, bedoeld voor weergave in de UI en telemetrie
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Viercijferige gemeentecode
    /// </summary>
    public string? Hoofdgemeente { get; set; }

    /// <summary>
    /// Viercijferige gemeentecode
    /// </summary>
    public string? GemeenteCode { get; set; }
}

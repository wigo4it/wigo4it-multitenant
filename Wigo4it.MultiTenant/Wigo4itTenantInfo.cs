using Finbuckle.MultiTenant.Abstractions;

namespace Wigo4it.MultiTenant;

/// <summary>
/// Bevat alle properties die tenant-specifiek zijn. Het is niet de bedoeling om deze direct te gebruiken in handler code.
/// Map altijd een subset van de properties naar een <see cref="Microsoft.Extensions.Options.IOptions{TOptions}"/>-gebaseerde class.
/// </summary>
public record Wigo4itTenantInfo : ITenantInfo
{
    /// <summary>
    /// Deze gebruiken we zelf niet, is slechts verplicht vanuit ITenantInfo
    /// </summary>
    string? ITenantInfo.Id
    {
        get => Identifier;
    }

    /// <summary>
    /// {Wegwijzer TenantCode}-{Wegwijzer EnvironmentName}-{GemeenteCode}
    /// </summary>
    public string? Identifier { get; set; }
    public string? Name { get; set; }

    // Onderstaande properties kunnen niet op required gezet worden, omdat Finbuckle een parameterloze
    // constructor vereist. We garanderen echter voor onze consumers dat alle properties altijd gevuld zijn.
    public string ConnectionString { get; set; } = null!;

    /// <summary>
    /// De 4-cijferige tenantcode
    /// </summary>
    public string TenantCode { get; set; } = null!;

    /// <summary>
    /// De naam van de omgeving in de wegwijzer
    /// </summary>
    public string EnvironmentName { get; set; } = null!;

    public string GemeenteCode { get; set; } = null!;

    /// <summary>
    /// Viercijferige gemeentecode
    /// </summary>
    public string Hoofdgemeente { get; set; } = null!;
}

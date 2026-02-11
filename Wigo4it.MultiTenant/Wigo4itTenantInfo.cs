using Finbuckle.MultiTenant.Abstractions;

namespace Wigo4it.MultiTenant;

/// <summary>
/// Bevat alle properties die tenant-specifiek zijn. Het is niet de bedoeling om deze direct te gebruiken in applicatiecode.
/// Map altijd een subset van de properties naar een <see cref="Microsoft.Extensions.Options.IOptions{TOptions}"/>-gebaseerde class.
/// </summary>
public record Wigo4itTenantInfo : ITenantInfo
{
    /// <summary>
    /// Deze gebruiken we zelf niet, is slechts verplicht vanuit ITenantInfo
    /// </summary>
    string ITenantInfo.Id => Identifier;

    /// <summary>
    /// {Wegwijzer TenantCode}-{Wegwijzer EnvironmentName}-{GemeenteCode}
    /// </summary>
    public required string Identifier { get; init; }

    /// <summary>
    /// De connectionstring naar de database voor deze tenant
    /// </summary>
    public required string ConnectionString { get; set; }

    /// <summary>
    /// De 4-cijferige tenantcode in de wegwijzer
    /// </summary>
    public required string TenantCode { get; set; }

    /// <summary>
    /// De naam van de omgeving in de wegwijzer
    /// </summary>
    public required string EnvironmentName { get; set; }

    /// <summary>
    /// Viercijferige gemeentecode van de (rand)gemeente
    /// </summary>
    public required string GemeenteCode { get; set; }
}

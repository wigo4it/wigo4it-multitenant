namespace Wigo4it.MultiTenant;

public record Wigo4itTenantOptions
{
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

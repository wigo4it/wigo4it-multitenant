namespace Wigo4it.MultiTenant.AspNetCore.Sample;

/// <summary>
/// Voorbeeld tenant info die Wigo4itTenantInfo uitbreidt met aangepaste eigenschappen.
/// </summary>
public record SampleTenantInfo : Wigo4itTenantInfo
{
    /// <summary>
    /// Aangepaste instelling specifiek voor deze tenant.
    /// </summary>
    public string? CustomSetting { get; set; }
}


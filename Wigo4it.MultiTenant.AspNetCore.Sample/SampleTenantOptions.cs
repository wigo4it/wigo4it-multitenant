namespace Wigo4it.MultiTenant.AspNetCore.Sample;

/// <summary>
/// Voorbeeld opties klasse die tenant-specifieke configuratie demonstreert.
/// Deze klasse wordt gebonden aan de appsettings.json onder de configuratie van elke tenant.
/// </summary>
public class SampleTenantOptions
{
    /// <summary>
    /// Haalt een aangepaste tenant-specifieke instelling op of stelt deze in.
    /// </summary>
    public string? CustomSetting { get; set; }
}

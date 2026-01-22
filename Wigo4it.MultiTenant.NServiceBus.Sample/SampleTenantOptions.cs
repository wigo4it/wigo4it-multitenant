namespace Wigo4it.MultiTenant.NServiceBus.Sample;

/// <summary>
/// Sample options class that demonstrates tenant-specific configuration.
/// This class is bound to the appsettings.json under each tenant's configuration.
/// </summary>
public class SampleTenantOptions
{
    /// <summary>
    /// Gets or sets a custom tenant-specific setting.
    /// </summary>
    public string? CustomSetting { get; set; }
}

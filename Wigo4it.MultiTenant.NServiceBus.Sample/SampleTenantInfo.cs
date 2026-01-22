namespace Wigo4it.MultiTenant.NServiceBus.Sample;

public record SampleTenantInfo : Wigo4itTenantInfo
{
    public string? CustomSetting { get; set; }
}
namespace Wigo4it.MultiTenant.NserviceBus.Tests;

public record TestTenantInfo : Wigo4itTenantInfo
{
    public required string Hoofdgemeente { get; set; }
    public required string Name { get; set; }
}

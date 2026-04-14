namespace Wigo4it.MultiTenant;

public class MultitenancyIdentifiers
{
    public required string WegwijzerTenantCode { get; init; }
    public required string WegwijzerEnvironmentName { get; init; }
    public required string GemeenteCode { get; init; }

    public static readonly MultitenancyIdentifiers MessageHeaders = new()
    {
        WegwijzerTenantCode = "Wigo4it.Wegwijzer.TenantCode",
        WegwijzerEnvironmentName = "Wigo4it.Wegwijzer.EnvironmentName",
        GemeenteCode = "Wigo4it.Socrates.GemeenteCode"
    };

    public static readonly MultitenancyIdentifiers HttpHeaders = new()
    {
        WegwijzerTenantCode = "X-Wigo4it-Wegwijzer-TenantCode",
        WegwijzerEnvironmentName = "X-Wigo4it-Wegwijzer-EnvironmentName",
        GemeenteCode = "X-Wigo4it-Socrates-GemeenteCode"
    };
    
    public static readonly MultitenancyIdentifiers Claims = new()
    {
        WegwijzerTenantCode = "w4-ww-tenant",
        WegwijzerEnvironmentName = "w4-ww-env",
        GemeenteCode = "w4-ww-gemeente"
    };
}
namespace Wigo4it.MultiTenant;

class SectionNames
{
    public const string TenantsSectie = "Tenants";
    public const string EnvironmentsSectie = "Environments";
    public const string GemeentenSectie = "Gemeenten";
    public static string[] All => [TenantsSectie, EnvironmentsSectie, GemeentenSectie];
}
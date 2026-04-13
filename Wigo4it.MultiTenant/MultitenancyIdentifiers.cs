namespace Wigo4it.MultiTenant;

public abstract class MultitenancyIdentifiers
{
    public static string WegwijzerTenantCode => throw new NotImplementedException();
    public static string WegwijzerEnvironmentName => throw new NotImplementedException();
    public static string GemeenteCode => throw new NotImplementedException();
}

public class MultitenancyHeaders : MultitenancyIdentifiers
{
    public new static string WegwijzerTenantCode => "Wigo4it.Wegwijzer.TenantCode";
    public new static string WegwijzerEnvironmentName => "Wigo4it.Wegwijzer.EnvironmentName";
    public new static string GemeenteCode => "Wigo4it.Socrates.GemeenteCode";
}

public class MultitenancyClaims : MultitenancyIdentifiers
{
    public new static string WegwijzerTenantCode => "w4-ww-tenant";
    public new static string WegwijzerEnvironmentName => "w4-ww-env";
    public new static string GemeenteCode => "w4-ww-gemeente";
}
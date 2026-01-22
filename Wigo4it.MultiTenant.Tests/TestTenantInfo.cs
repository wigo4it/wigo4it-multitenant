namespace Wigo4it.MultiTenant.Tests;

public record TestTenantInfo : Wigo4itTenantInfo
{
    public DummyTestTenantInfo Info { get; set; } = null!;
}

public class DummyTestTenantInfo
{
    public bool SomeProperty { get; set; }
}

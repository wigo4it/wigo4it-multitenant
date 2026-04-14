namespace Wigo4it.MultiTenant.AspNetCore;

/// <summary>
/// Configureeropties voor ASP.NET Core tenant-resolutie.
/// </summary>
public sealed class Wigo4ItMultiTenantAspNetCoreOptions
{
    /// <summary>
    /// Bepaalt uit welke request-bron de tenant identifier wordt afgeleid.
    /// </summary>
    public TenantIdResolutionStrategy TenantIdResolutionStrategy { get; set; } = TenantIdResolutionStrategy.Claims;
}

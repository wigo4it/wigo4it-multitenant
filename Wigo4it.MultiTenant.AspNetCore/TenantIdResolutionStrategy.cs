namespace Wigo4it.MultiTenant.AspNetCore;

/// <summary>
/// Bepaalt hoe de tenant identifier wordt afgeleid uit een inkomende ASP.NET Core request.
/// </summary>
public enum TenantIdResolutionStrategy
{
    /// <summary>
    /// Leest de tenant identifier uit de claims van het inkomende JWT-token.
    /// </summary>
    Claims,

    /// <summary>
    /// Leest de tenant identifier uit de HTTP-headers van het inkomende request.
    /// </summary>
    Headers,
}


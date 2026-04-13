using Microsoft.AspNetCore.Mvc.Testing;

namespace Wigo4it.MultiTenant.AspNetCore.IntegrationTests;

/// <summary>
/// Host de sample ASP.NET Core applicatie in-memory voor integratietesten.
/// </summary>
public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>;


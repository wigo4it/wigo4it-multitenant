# Wigo4it.MultiTenant.AspNetCore.IntegrationTests

Integratietests voor `Wigo4it.MultiTenant.AspNetCore.Sample` met `WebApplicationFactory`.

Gedekte scenario's (pariteit met `requests.http`):
- `GET /` health check
- `GET /tenant-info` met token `9446-dev-0518`
- `GET /tenant-info` met token `9446-test-0599`

## Run

```bash
dotnet test Wigo4it.MultiTenant.AspNetCore.IntegrationTests/Wigo4it.MultiTenant.AspNetCore.IntegrationTests.csproj
```


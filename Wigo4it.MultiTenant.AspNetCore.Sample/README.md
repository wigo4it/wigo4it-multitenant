# Wigo4it.MultiTenant.AspNetCore Sample

Een ASP.NET Core applicatie die demonstreert hoe `Wigo4it.MultiTenant.AspNetCore` tenant-specifieke opties (`SampleTenantOptions`) correct resolved uit JWT tokens met multitenancy claims of HTTP-headers.

## Overzicht

Deze sample toont hoe:
- JWT tokens met multitenancy claims worden geparseerd (zonder token validation)
- Tenant-context wordt bepaald op basis van claims in het token of op basis van HTTP-headers
- Tenant-specifieke opties worden injecteerd via dependency injection
- Tenant-resolutie kan schakelen tussen claims en headers via `Wigo4it:MultiTenant:TenantIdResolutionStrategy`

## Setup

### Vereisten
- .NET 10.0+
- ASP.NET Core

### Installatie
```bash
cd Wigo4it.MultiTenant.AspNetCore.Sample
dotnet restore
dotnet build
```

### Starten
```bash
dotnet run
```

De applicatie start standaard op `http://localhost:5000`.

## Multitenancy Claims in JWT Token

De volgende claims moeten aanwezig zijn in het JWT token:
- `w4-ww-tenant` - 4-cijferige tenant code (bijv. "9446")
- `w4-ww-env` - Environment name (bijv. "dev", "test", "prod")
- `w4-ww-gemeente` - 4-cijferige gemeente code (bijv. "0518", "0599")

De tenant identifier wordt opgebouwd als: `{tenantCode}-{environmentName}-{gemeenteCode}`

Bijvoorbeeld: `9446-dev-0518`

## Endpoints

### GET /
Health check endpoint dat bevestigt dat de applicatie draait.

### GET /tenant-info
Demonstreert dat `SampleTenantOptions` correct worden resolved op basis van de geconfigureerde tenant-resolutie (claims of headers).

**Headers:**
```
Authorization: Bearer <jwt_token>
```

**Response voorbeeld:**
```json
{
  "message": "Tenant information successfully resolved",
  "tenantIdentifier": "9446-dev-0518",
  "tenantCode": "9446",
  "environmentName": "dev",
  "gemeenteCode": "0518",
  "customSetting": "Sample setting for tenant 9446 in dev environment for gemeente 0518"
}
```

## Testing

Gebruik het meegeleverde `requests.http` bestand voor het testen van de endpoints met voorgedefinieerde JWT tokens en, in header-modus, met expliciete tenant headers.

### JWT Tokens

Er zijn drie voorgedefinieerde tokens in `requests.http`:

1. **Dev token (9446-dev-0518)**
   ```
   eyJhbGciOiAiSFMyNTYiLCAidHlwIjogIkpXVCJ9.eyJ3NC13dy10ZW5hbnQiOiAiOTQ0NiIsICJ3NC13dy1lbnYiOiAiZGV2IiwgInc0LXd3LWdlbWVlbnRlIjogIjA1MTgifQ.dummysignature
   ```

2. **Test token (9446-test-0599)**
   ```
   eyJhbGciOiAiSFMyNTYiLCAidHlwIjogIkpXVCJ9.eyJ3NC13dy10ZW5hbnQiOiAiOTQ0NiIsICJ3NC13dy1lbnYiOiAidGVzdCIsICJ3NC13dy1nZW1lZW50ZSI6ICIwNTk5In0.dummysignature
   ```

3. **Token zonder tenant claims**
   ```
   eyJhbGciOiAiSFMyNTYiLCAidHlwIjogIkpXVCJ9.eyJzdWIiOiAiZHVtbXktdXNlciJ9.dummysignature
   ```

Deze tokens hebben geen geldige handtekening, maar dat is prima voor deze sample omdat token validation is uitgeschakeld.

## Configuratie

### appsettings.json

De tenant-configuratie wordt gedefinieerd onder `Tenants` met de volgende structuur:

```json
{
  "Tenants": {
    "{tenantCode}": {
      "Environments": {
        "{environmentName}": {
          "Gemeenten": {
            "{gemeenteCode}": {
              "CustomSetting": "Custom value for this tenant"
            }
          }
        }
      }
    }
  }
}
```

Elke tenant-specifieke instelling onder het juiste pad wordt automatisch gekoppeld aan `SampleTenantOptions` via `ConfigurePerTenant<SampleTenantOptions, SampleTenantInfo>`.

## Hoe het werkt

1. **JWT Token Parser**: De `JwtBearer` authentication handler leest het JWT token en extraheert claims.
2. **Tenant Identifier Resolver**: afhankelijk van `TenantIdResolutionStrategy` gebruikt de library claims (`AspNetCoreTenantIdFromClaimsResolver`) of HTTP-headers (`AspNetCoreTenantIdFromHeadersResolver`) om `{tenantCode}-{environmentName}-{gemeenteCode}` te bepalen.
3. **Tenant Context**: `app.UseMultiTenant()` zet de Finbuckle tenant-context op basis van de resolved identifier.
4. **Tenant Store**: `DictionaryConfigurationStore` leest de tenant configuratie uit `appsettings.json`.
5. **Dependency Injection**: `SampleTenantOptions` worden per tenant ingesteld via `ConfigurePerTenant`.
6. **Options Resolution**: bij aanvraag van `IOptions<SampleTenantOptions>` geeft DI de juiste tenant-specifieke opties.

## Specifieke implementatiedetails

### JWT Token Validation
De sample gebruikt een custom `UnsafeJwtTokenHandler` die JWT tokens accepteert zonder de handtekening te valideren. Dit is **ALLEEN** geschikt voor sample/test doeleinden.

In productie moet een proper JWT token validation worden ingesteld met:
- Geldige ondertekende tokens
- Issuer validation
- Audience validation
- Expiration validation
- Signature key validation

Zie `SampleServices.cs` voor de configuratie details.

## Debugging

Voor het debuggen kunt u volgende breakpoints instellen:
- `SampleServices.ConfigureSampleServices()` - Ziet configuratie
- `AspNetCoreTenantIdFromClaimsResolver.DetermineTenantIdentifier()` of `AspNetCoreTenantIdFromHeadersResolver.DetermineTenantIdentifier()` - Ziet tenant resolution
- De endpoint handlers - Ziet injecteerde opties

## Productie Use

Voor gebruik in productie:
- Zorg voor gepaste JWT token validation
- Implementeer environment-specifieke appsettings bestanden (appsettings.Production.json)
- Voeg proper error handling toe
- Zorg voor adequate logging van tenant context
- Valideer dat alle verwachte tenants in de configuratie aanwezig zijn





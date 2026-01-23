# Wigo4it MultiTenant suite

Multi-tenant ondersteuning voor Wigo4it applicaties, gebouwd op Finbuckle.MultiTenant en uitbreidbaar naar NServiceBus. Deze repository bevat de basisbibliotheek én de NServiceBus-integratie, plus voorbeelden en tests.

## Wat vind je hier

- [Wigo4it.MultiTenant](Wigo4it.MultiTenant/README.md): basis multi-tenant functionaliteit met configuratie-gebaseerde tenant opslag.
- [Wigo4it.MultiTenant.NServiceBus](Wigo4it.MultiTenant.NServiceBus/README.md): NServiceBus-integratie die tenant headers resolveert en de pipeline multi-tenant maakt.
- [Wigo4it.MultiTenant.NServiceBus.Sample](Wigo4it.MultiTenant.NServiceBus.Sample): end-to-end voorbeeldapplicatie.
- Testsuites voor unit-, integratie- en race-condition tests.

## Kernconcepten

- **Tenant identifier**: `{TenantCode}-{EnvironmentName}-{GemeenteCode}`, bijvoorbeeld `9446-dev-0599`.
- **Headers** voor service-naar-service communicatie:
  - `Wigo4it.Wegwijzer.TenantCode.Forwardable`
  - `Wigo4it.Wegwijzer.EnvironmentName.Forwardable`
  - `Wigo4it.Socrates.GemeenteCode.Forwardable`
  Beschikbaar via de statische class `MultitenancyHeaders` in de basisbibliotheek.
- **Configuratie-hiërarchie**: defaults per omgeving met overrides per gemeente. Dit wordt door `DictionaryConfigurationStore` samengevoegd tot een `Wigo4itTenantInfo` (of eigen subtype) per tenant.

## Hoe de packages samenwerken

1. **Wigo4it.MultiTenant** resolveert de tenant (bijvoorbeeld uit HTTP headers) en bindt configuratie naar jouw `TenantInfo` subtype. `ConfigurePerTenant` projecteert deze waarden naar `IOptions<T>` per request.
2. **Wigo4it.MultiTenant.NServiceBus** haalt dezelfde headers uit inkomende berichten, zet de tenantcontext en zorgt dat uitgaande berichten standaard dezelfde tenant headers meekrijgen.
3. De sample app laat zien hoe beide samen worden gebruikt: HTTP endpoint stuurt NServiceBus-berichten met tenant headers die de handler weer oppakt.

## Snel starten

1. Kies het pakket:
   - Web/worker zonder NServiceBus? Gebruik `dotnet add package Wigo4it.MultiTenant`.
   - NServiceBus endpoint? Gebruik `dotnet add package Wigo4it.MultiTenant.NServiceBus`.
2. Volg de stappen in de betreffende README voor configuratie en options mapping.
3. Bekijk de sample voor een volledige referentie-configuratie en message flow.

## Documentatie per pakket

- [Basisbibliotheek](Wigo4it.MultiTenant/README.md) – configuratiestructuur, options mapping, best practices en troubleshooting.
- [NServiceBus integratie](Wigo4it.MultiTenant.NServiceBus/README.md) – pipeline-setup, message handlers, headers doorsturen en performance tips.

## Tests en voorbeelden

- Unit tests: zie [Wigo4it.MultiTenant.Tests](Wigo4it.MultiTenant.Tests) voor configuratie- en resolver-tests.
- Race-condition tests: zie [Wigo4it.MultiTenant.NserviceBus.Tests](Wigo4it.MultiTenant.NserviceBus.Tests) voor concurrency scenarios met `IOptions`.
- Integratietests: zie [Wigo4it.MultiTenant.NServiceBus.IntegrationTests](Wigo4it.MultiTenant.NServiceBus.IntegrationTests).
- Voorbeeldapp: zie [Wigo4it.MultiTenant.NServiceBus.Sample](Wigo4it.MultiTenant.NServiceBus.Sample).

## Licentie en support

- Licentie: MIT (zie LICENSE).
- Ondersteuning: neem contact op met het Wigo4it development team.

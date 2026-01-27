# Wigo4it MultiTenant suite

Multi-tenant ondersteuning voor Wigo4it applicaties, gebouwd op Finbuckle.MultiTenant. Deze repository bevat de basisbibliotheek én de NServiceBus-integratie, plus voorbeelden en tests.

## Wat vind je hier

- [Wigo4it.MultiTenant](Wigo4it.MultiTenant/README.md): basis multi-tenant functionaliteit met configuratie per tenant.
- [Wigo4it.MultiTenant.NServiceBus](Wigo4it.MultiTenant.NServiceBus/README.md): NServiceBus-integratie die tenant headers resolved en op basis daarvan de tenant-specifieke configuratie laadt.
- [Wigo4it.MultiTenant.NServiceBus.Sample](Wigo4it.MultiTenant.NServiceBus.Sample): end-to-end voorbeeldapplicatie.
- Testsuites voor unit- en integratietests.

## Kernconcepten
Een Tenant in Wigo4it context is gedefinieerd als een individuele (rand)gemeente binnen een omgeving zoals die in de Wegwijzer aangemaakt wordt.

Om een Tenant uniek te identificeren zijn drie gegevens nodig:
- `TenantCode`: De viercijferige TenantCode waaronder de omgeving in de Wegwijzer aangemaakt is. **Let op**: de definitie van Tenant in de Wegwijzer is een andere dan we hier hanteren. Voorbeeld: `0518` of `9446`.
- `EnvironmentName`: De naam van de omgeving zoals die in de Wegwijzer aangemaakt is. Voorbeeld: `0344so1` of `0518pr1`.
- `GemeenteCode`: De viercijferige gemeentecode van de individuele (rand)gemeente waarvoor het request bedoeld is. Voorbeeld: `0363` of `0321`.

- **Tenant identifier**: Combineert bovenstaande drie gegevens tot een unieke identifier, volgens het formaat `{TenantCode}-{EnvironmentName}-{GemeenteCode}`, bijvoorbeeld `9446-0344so1-0321`.
- **Headers** Om de tenant-identificerende gegevens door te geven maken we gebruik van drie headers:
  - `Wigo4it.Wegwijzer.TenantCode.Forwardable`
  - `Wigo4it.Wegwijzer.EnvironmentName.Forwardable`
  - `Wigo4it.Socrates.GemeenteCode.Forwardable`
  Beschikbaar via de statische class `MultitenancyHeaders` in `Wigo4it.MultiTenant`.
- **Configuratie-hiërarchie**: defaults per omgeving met overrides per gemeente. Dit wordt door `DictionaryConfigurationStore` samengevoegd tot een `Wigo4itTenantInfo` (of eigen subtype) per tenant.

## Hoe de packages samenwerken

1. **Wigo4it.MultiTenant** resolved de tenant (bijvoorbeeld uit HTTP headers) en bindt configuratie naar jouw `TenantInfo` subtype. `ConfigurePerTenant` projecteert deze waarden naar `IOptions<T>` per request.
2. **Wigo4it.MultiTenant.NServiceBus** haalt dezelfde headers uit inkomende berichten, zet de tenantcontext en zorgt dat uitgaande berichten standaard dezelfde tenant headers meekrijgen.
3. De sample app laat zien hoe beide samen worden gebruikt.

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


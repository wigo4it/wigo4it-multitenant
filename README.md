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
- `Wigo4it.Wegwijzer.TenantCode`
- `Wigo4it.Wegwijzer.EnvironmentName`
- `Wigo4it.Socrates.GemeenteCode`
Beschikbaar via de statische class `MultitenancyHeaders` in `Wigo4it.MultiTenant`.
- **Configuratie-hiërarchie**: defaults per omgeving met overrides per gemeente. Dit wordt door `DictionaryConfigurationStore` samengevoegd tot een `Wigo4itTenantInfo` (of eigen subtype) per tenant.

## Hoe de packages samenwerken

1. **Wigo4it.MultiTenant** resolved de tenant (bijvoorbeeld uit HTTP headers) en bindt configuratie naar jouw `TenantInfo` subtype. `ConfigurePerTenant` projecteert deze waarden naar `IOptions<Wigo4itTenantOptions>` per request.
2. **Wigo4it.MultiTenant.NServiceBus** haalt dezelfde headers uit inkomende berichten, zet de tenantcontext en zet op uitgaande berichten dezelfde headers op basis van `IOptions<Wigo4itTenantOptions>`
3. De sample app laat zien hoe beide samen worden gebruikt.

## Snel starten

1. Kies het pakket:
   - Web/worker zonder NServiceBus? Gebruik `dotnet add package Wigo4it.MultiTenant`.
   - NServiceBus endpoint? Gebruik `dotnet add package Wigo4it.MultiTenant.NServiceBus`.
2. Volg de stappen in de betreffende README voor configuratie en options mapping.
3. Bekijk de sample voor een volledige referentie-configuratie en message flow.

## Documentatie per pakket

- [Basisbibliotheek](Wigo4it.MultiTenant/README.md) – configuratiestructuur, options mapping, best practices en troubleshooting.
- [NServiceBus integratie](Wigo4it.MultiTenant.NServiceBus/README.md) – pipeline-setup, message handlers, tenant headers op uitgaande berichten en performance tips.

## Tests en voorbeelden

- Unit tests: zie [Wigo4it.MultiTenant.Tests](Wigo4it.MultiTenant.Tests) voor configuratie- en resolver-tests.
- Race-condition tests: zie [Wigo4it.MultiTenant.NserviceBus.Tests](Wigo4it.MultiTenant.NserviceBus.Tests) voor concurrency scenarios met `IOptions`.
- Integratietests: zie [Wigo4it.MultiTenant.NServiceBus.IntegrationTests](Wigo4it.MultiTenant.NServiceBus.IntegrationTests).
- Voorbeeldapp: zie [Wigo4it.MultiTenant.NServiceBus.Sample](Wigo4it.MultiTenant.NServiceBus.Sample).

## Licentie-informatie en Afhankelijkheden
Dit project is beschikbaar gesteld onder de [European Union Public Licence (EUPL-1.2)](https://joinup.ec.europa.eu/collection/eupl/eupl-text-eupl-12). Hoewel de broncode van dit project open-source is, maakt de applicatie gebruik van externe bibliotheken van derden waarvoor aanvullende voorwaarden van toepassing kunnen zijn.

### Belangrijke opmerking over NServiceBus
Deze software maakt gebruik van NServiceBus (ontwikkeld door Particular Software). Gebruikers die deze repository klonen of de applicatie zelf willen draaien, moeten rekening houden met het volgende:

- **Eigen licentie benodigd**: Onze organisatie gebruikt NServiceBus onder een specifieke commerciële overeenkomst. Deze licentie is niet overdraagbaar aan derden.
- **RPL-licentie**: Zonder commerciële licentie valt het gebruik van NServiceBus doorgaans onder de [Reciprocal Public License (RPL-1.5)](https://opensource.org/license/rpl-1-5). Dit kan verplichtingen met zich meebrengen voor uw eigen projecten.

**Let op**: Gebruikers zijn zelf verantwoordelijk voor het verkrijgen van de juiste licenties. Wij adviseren om de licentievoorwaarden van [Particular Software](https://particular.net/) te raadplegen om te bepalen of u een eigen (gratis of commerciële) licentie nodig heeft voor uw specifieke gebruikssituatie.

Voor een volledig overzicht van alle gebruikte pakketten en hun respectievelijke licenties, zie het bestand `THIRD-PARTY-NOTICES.txt` in deze repository.
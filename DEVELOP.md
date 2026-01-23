# DEVELOP

Dit document beschrijft hoe we branches beheren en hoe we de major versie verhogen. De afspraken zijn afgestemd op de GitVersion configuratie in [GitVersion.yml](GitVersion.yml).

## Branching strategie
- **main**: stabiele releases. GitVersion geeft hier een productieready versie zonder label.
- **develop**: integratie van nieuwe features. Geeft automatisch beta-versies.
- **feature/***: korte-lopende branches voor nieuwe functionaliteit vanaf develop. Geeft alpha-versies.
- **Pull requests**: feature → develop; develop → main voor productie releases.

## Versiebeheer met GitVersion
- **Mode**: ContinuousDeployment; elke commit resulteert in een nieuwe buildversie.
- **SemVer bump tokens** in commit messages:
  - `+semver: major` of `+semver: breaking`
  - `+semver: minor` of `+semver: feature`
  - `+semver: patch` of `+semver: fix`
  - `+semver: none` om geen bump af te dwingen
- **Labels per branch**: main (geen), develop (beta), feature (alpha).
- **Tag prefix**: `v` (bijv. `v2.3.0`).

## Major versie verhogen
1. **Plan breaking changes**: verzamel breaking wijzigingen op develop; rond tests en documentatie af.
2. **Forceer major bump**: voeg een commit toe op develop met `git commit --allow-empty -m "chore: bump to v<nieuw_major>.0.0 +semver: major"` zodat GitVersion het major nummer ophoogt.
3. **Merge naar main**: open een PR develop → main. Na merge genereert de pipeline een versie `v<nieuw_major>.0.0` (zonder label).
4. **Taggen (indien handmatig nodig)**: als tagging niet door de pipeline gebeurt, tag de main-merge commit met `git tag v<nieuw_major>.0.0` en `git push --tags`.

## Praktische tips
- Gebruik consistente branchnamen: `feature/<beschrijving>`.
- Voeg altijd het juiste `+semver:` token toe wanneer het standaard increment niet klopt.
- Houd feature branches kortlevend; merge regelmatig terug naar develop.
- Controleer pipelines na merges; een mislukte build betekent geen betrouwbare versie.

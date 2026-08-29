# World Events Standalone

World Events Standalone extracts the campaign chronicle, character story,
realm affairs, war statistics, marriage, and strategic-map UI from Ages of
Calradia into an independently downloadable Bannerlord module.

## Install and use

Extract `WorldEventsStandalone` into Bannerlord's `Modules` directory, enable
it after the official single-player modules, load a campaign, and press **F10**
to open or close World Events. Escape and the on-screen close control also
close the overlay.

The module targets Bannerlord v1.4.8 and .NET Framework 4.7.2. It does not
require Harmony, MCM, or Ages of Calradia.

The campaign calendar follows Bannerlord's native time values: four seasons
per year, 21 days per season, and 84 days per year.

## Compatibility

Do not enable this module together with Ages of Calradia. Both products own the
same World Events ledger save keys so players can move an existing chronicle
between them, but simultaneous activation would register duplicate campaign
behaviors and UI resources. The module manifest declares this incompatibility.

Optional culture, religion, and population strategic-map modes remain visible
but show their safe built-in fallback when no compatible demographic provider
is installed. World Events Standalone does not change campaign pacing, economy,
pregnancy, party speed, weather, diplomacy cadence, or any native game model.

## Build

Run `New-StandaloneRelease.ps1`. It compiles a fresh standalone DLL, stages
only the required UI/runtime assets, verifies the package contract, and creates
`artifacts/WorldEventsStandalone-v1.0.9.zip` at the repository root.

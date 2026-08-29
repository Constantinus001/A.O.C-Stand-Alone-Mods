# World Events Standalone

This directory contains the complete World Events Standalone v1.0.9 module and its C# source project.

## Layout

- `Module/WorldEventsStandalone/` is the verified runtime module, including the known-good GUI, sprite data, strategic-map artwork, TPAC, DLL, and PDB.
- `Source/` is a buildable .NET Framework 4.7.2 source project for the published v1.0.9 assembly.
- `Build.ps1` compiles the source without altering the verified module.
- `Verify.ps1` checks the module contract, critical GUI files, source tree, and recorded runtime hashes.
- `New-StandaloneRelease.ps1` verifies, builds, stages the complete module, replaces its DLL/PDB with the fresh build, verifies the staged UI, and creates a zip in `artifacts/`.

## Source provenance

The original v1.0.9 Git tag was attached to the full Ages of Calradia source tree rather than a dedicated standalone project. To avoid publishing unrelated or incorrect source, this standalone project was recovered from the exact released v1.0.9 DLL and matching PDB. It was normalized only for compatibility with the available .NET Framework build chain and then compiled successfully with zero warnings and zero errors.

The untouched published DLL and PDB remain under `Module/WorldEventsStandalone/bin/Win64_Shipping_Client/` so the tested release is preserved.

## Build

Bannerlord v1.4.8 must be installed. The scripts automatically check the normal Steam locations. For another location:

```powershell
.\Build.ps1 -BannerlordDir 'D:\Games\Mount & Blade II Bannerlord'
```

Create a verified distributable:

```powershell
.\New-StandaloneRelease.ps1
```

Do not enable this standalone module together with the full Ages of Calradia module; both register the World Events systems and save keys.

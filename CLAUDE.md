# AutoTrader Redux

## Versioning
The mod version is `<Version value = "..."/>` in `AutoTrader/src/SubModule.xml`. Bump it for every release uploaded to Nexus. For a small fix release (bug fixes only), add or increase a fourth number on the current version, e.g. `v1.4.8` → `v1.4.8.1` → `v1.4.8.2`.

Only the mod's own `Version` changes. `DependentVersion` on the Native dependency in `SubModule.xml` and `AutoTraderGameVersion` in `AutoTraderConfig.cs` hold the **game** version and stay unchanged unless the target game version changes.

Each release gets its own `## vX.Y.Z[.N] (date)` section at the top of `CHANGELOG.md`.

## Release package
Name the Nexus archive `AutoTrader Redux vX.Y.Z[.N].7z`. It contains one top-level `AutoTrader` module folder (SubModule.xml, `bin\Win64_Shipping_Client\AutoTrader.dll`, GUI, ModuleData), without .pdb files.

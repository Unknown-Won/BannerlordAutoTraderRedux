# AutoTrader Redux

## Versioning
The mod version is `<Version value = "..."/>` in `AutoTrader/src/SubModule.xml`. Bump it for every release uploaded to Nexus. For a small fix release (bug fixes only), add or increase a fourth number on the current version, e.g. `v1.4.8` → `v1.4.8.1` → `v1.4.8.2`.

Only the mod's own `Version` changes. `DependentVersion` on the Native dependency in `SubModule.xml` and `AutoTraderGameVersion` in `AutoTraderConfig.cs` hold the **game** version and stay unchanged unless the target game version changes.

Each release gets its own `## vX.Y.Z[.N] (date)` section at the top of `CHANGELOG.md`.

## Release package
Name the Nexus archive `AutoTrader Redux vX.Y.Z[.N].7z`. It contains one top-level `AutoTrader` module folder (SubModule.xml, `bin\Win64_Shipping_Client\AutoTrader.dll`, GUI, ModuleData), without .pdb files.

## Nexus release notes
Every Nexus release needs two texts. Write both for the user, along with the CHANGELOG.md section:
1. **Changelog:** goes on the mod's Description page, in the collapsible "Changelogs" tab. It can be as long as needed. List every user-facing change since the last release, as one bullet per change, worded for players, not code.
2. **File description:** shown under the file on the Files tab, where most players see it. Keep it short: a high-level overview of the release (or of the mod), the supported game version, requirements and optional mods, load order notes, and anything players must do when updating.

Save them in the project files as `AutoTraderRedux/nexus-release-vX.Y.Z[.N].md`.

## Other mods' AutoTrader support
Castle Market Towns adds its own castle "Automatically trade wares" button. It finds `AutoTrader.TradeBehavior.AutoTradeGoodsCondition` / `AutoTradeGoodsConsequence` by reflection and transpiles `AutoTraderLogicConnector.GetMerchantType`, expecting exactly one `Settlement.IsTown` call there. Keep those names, signatures and that single `IsTown` call, or its support switches itself off. AutoTrader does not detect other mods' trade menus by their Trade icon, because mods also use that icon for options that don't trade.

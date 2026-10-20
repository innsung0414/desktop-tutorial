# Deep Blue animated fish model sources

This note records the active fish assets and their runtime locations. Deep Blue ships 37 collectible model entries: 35 Quaternius fish and two Fish Alive samples. The Quaternius clownfish and Fish Alive clownfish are kept as separate visual variants.

## Quaternius Animated Cute Fish Pack

- Creator: Quaternius
- Bundle: Animated Cute Fish Pack
- Source: https://poly.pizza/bundle/Animated-Fish-Bundle-44zhHN1UbT
- Creator and license: https://quaternius.com/packs/cutefish.html
- License: CC0 1.0 / public domain
- Imported models: 35 fish FBX files
- Source models: `Assets/Archive/QuaterniusFish/Models/`
- Animator controllers: `Assets/Archive/QuaterniusFish/Controllers/`
- Runtime prefabs: `Assets/Resources/DeepBlueFish/Prefabs/`
- Journal icons: `Assets/Art/UI/Quaternius/`

The source bundle contains 52 FBX assets: 35 fish, 16 fishing/environment props, and one worm bait model. The runtime fish prefabs use a looping swim state and, when available, a short caught reaction. Unity generates mesh normals during import.

## Fish Alive - FREE Samples

- Creator: DA Denys Almaral
- Unity Asset Store product: https://assetstore.unity.com/packages/3d/characters/animals/fish/fish-alive-free-samples-362806
- Imported samples: `freshWater_guppy` and `marine_clownfish`
- Runtime prefabs: `Assets/Resources/DeepBlueFish/FishAlive/Guppy.prefab` and `Clownfish.prefab`
- Journal icons: `Assets/Art/UI/FishAlive/`

The user registered the free sample and approved the Asset Store terms before import. The project package is version 1.0; the online product page showed version 1.2 during the 2026-10-17 account check. Keep those version references distinct.

## Active roster and behavior

- All 37 IDs are listed in `DeepBlueFishDetails.CurrentSpeciesIds` and saved in `Assets/Scenes/DeepBlueFishing.unity`.
- `Fish_Asset_<ModelName>` loads the matching Quaternius runtime prefab. The two Fish Alive IDs load the sample prefabs from their separate Resources folder.
- The journal shows six entries per page, for seven pages total. Rarity and catch weights use the game balance ranges in `DeepBlueFishDetails`; undiscovered entries currently receive a 4× catch-weight bonus.
- Twelve older icon-only fish IDs remain as archived profile keys. Their catch and best-weight history stays separate from the current 37 entries, and fish remaining in the old inventory can still be sold.

## Unity verification — 2026-10-17

The active scene, journal, and fishing reward table were checked in Unity Play Mode. All 37 showcase paths loaded a prefab with an Animator, at least one animation clip, and a SkinnedMeshRenderer; no missing entries were found. Windows x64 r16 built successfully at 137.0 MB with 0 build errors. BuildReport lists all 35 Resources prefabs and both Fish Alive runtime assets. Independent Windows keyboard, trading, save/relaunch, and beginner-play acceptance remain tracked in `TASKS.md`.

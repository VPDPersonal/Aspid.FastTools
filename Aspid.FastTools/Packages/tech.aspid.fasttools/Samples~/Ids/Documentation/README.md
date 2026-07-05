# Ids Sample

Demonstrates the `IId` / `IdRegistry` / `[UniqueId]` trio: fields show a human-readable string in the Inspector while serializing as a stable integer, and the Inspector catches collisions at edit-time.

> **New here? Start with [TUTORIAL.md](TUTORIAL.md)** ([RU](TUTORIAL_RU.md)) — a guided, step-by-step tour (Lessons 1–4) built around `Scripts/Tutorial/IdsTutorial.cs` and `Scenes/IdsTutorial.unity`. This page is the demo-scene walkthrough; the tutorial teaches the workflow.

## How it works

- `IId` — a marker interface declaring the `int Id { get; }` property.
- `IdRegistry` — a `ScriptableObject` that binds a struct type to a list of `(Id, Name)` entries and keeps the name ↔ int map available at runtime. The property drawer renders a dropdown sourced from this registry.
- `[UniqueId]` — validates at edit-time that no two `ScriptableObject` assets share the same resolved integer ID.

## Scenario

An enemy catalog. Each `EnemyDefinition` asset holds a unique `EnemyId` plus display data (`_displayName`, `_maxHealth`, `_moveSpeed`). An `EnemySpawner` picks a target `EnemyId` via dropdown and looks the matching asset up in its catalog on `Start()`.

Look at:

- `Scripts/EnemyId.cs` — `partial struct : IId`. `IdStructGenerator` emits `__stringId`, `_id`, and the `Id` property.
- `Scripts/EnemyDefinition.cs:10` — `[UniqueId]` on a serialized `EnemyId` field prevents duplicate IDs across assets.
- `Data/IdRegistry_EnemyId.asset` — the registry binding names (`fly_enemy_dragon`, `walk_enemy_goblin`, `walk_enemy_orc`, `walk_enemy_skeleton`) to stable ints.
- `Scripts/EnemySpawner.cs:9` — dropdown-selected `EnemyId` resolved to `int` at runtime via `.Id`.

## How to run

Open `Scenes/Ids.unity` and enter Play Mode — the scene hosts a pre-wired `EnemySpawner` (from `Prefabs/Ids.prefab`) with all four `Data/*_enemy_*.asset` files in its `Catalog` and `walk_enemy_orc` as the `Spawn Target`. The Console logs the resolved `EnemyDefinition` (display name, HP, move speed). Switch the `Spawn Target` dropdown — the picker is sourced from `IdRegistry_EnemyId` — to see different lookups.

To create more entries, open `Data/IdRegistry_EnemyId.asset` to add registry rows, then `Assets > Create > Aspid > FastTools > Samples > Enemy Definition` for the asset side.

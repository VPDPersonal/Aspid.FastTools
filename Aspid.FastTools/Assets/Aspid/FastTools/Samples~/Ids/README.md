# Ids Sample

Demonstrates the `IId` / `IdRegistry` / `[UniqueId]` trio: fields show a human-readable string in the Inspector while serializing as a stable integer, and the Inspector catches collisions at edit-time.

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

Open `Scenes/Ids.unity` — it contains an `EnemySpawner` GameObject (also available as `Prefabs/Ids.prefab`). Wire it up once:

1. Drag the four `Data/*_enemy_*.asset` files into the spawner's `Catalog` array.
2. Pick a target enemy from the `Spawn Target` dropdown — the picker is sourced from `IdRegistry_EnemyId`.
3. Enter Play Mode — the Console logs the resolved `EnemyDefinition` (display name, HP, move speed). Switch the dropdown to see different lookups.

To create more entries, open `Data/IdRegistry_EnemyId.asset` to add registry rows, then `Assets > Create > Aspid > FastTools > Samples > Enemy Definition` for the asset side.

# Ids — Step-by-Step Tutorial

The Ids trio gives you human-readable names in the Inspector backed by stable integer IDs on disk:

- `IId` — a marker interface with a single `int Id { get; }` property; declaring a `partial struct : IId`
  triggers `IdStructGenerator`, which emits all the serialization boilerplate.
- `IdRegistry` — a `ScriptableObject` binding one id struct type to a list of `(Id, Name)` rows; it powers the
  Inspector dropdown and the runtime `int ↔ string` lookups.
- `[UniqueId]` — edit-time validation that no two assets share the same resolved ID.

Each lesson maps to one `STEP` section of the `IdsTutorial` component.

## Open the tutorial

1. Open the Welcome window (**Tools → Aspid 🐍 → FastTools → Welcome**) and import the **Ids** sample.
2. Open **`Scenes/IdsTutorial.unity`** and select the **Ids Tutorial** GameObject.

---

## Lesson 1 — Declare an id struct & pick a value

**Field:** `EnemyId _step1EnemyId` — pre-set to `walk_enemy_goblin`.

The whole user-side declaration is one line — see `Scripts/EnemyId.cs`:

```csharp
[Serializable]
public partial struct EnemyId : IId { }
```

`IdStructGenerator` emits `__stringId`, `_id` and the `Id` property into the other half of the partial.

In the Inspector the field renders as a **name dropdown**, not an int field:

1. Open the dropdown on `_step1EnemyId` — the four enemy names come from `Data/IdRegistry_EnemyId.asset`.
2. Pick another entry, e.g. `fly_enemy_dragon`.

- What's serialized is the **stable int** (plus the name string for migration) — renaming an entry in the
  registry later does not break assets that reference it.
- Each `IId` struct binds to exactly **one** registry asset; `IdRegistryResolver` finds it for the drawer.

---

## Lesson 2 — The registry behind the dropdown

**Field:** `IdRegistry _step2Registry` — references `Data/IdRegistry_EnemyId.asset`.

1. Click the field to ping the registry asset and open it.
2. The Inspector lists the `(Id, Name)` rows: `1 = fly_enemy_dragon` … `4 = walk_enemy_skeleton`.
3. Add a row, e.g. `swim_enemy_shark` — a fresh ID is assigned automatically.
4. Go back to the tutorial GameObject: the STEP 1 dropdown now offers the new name.

- A registry is created via **Assets → Create → Aspid → Id Registry** and bound to a struct type once.
- IDs are handed out from an internal counter and never reused — deleting a row does not recycle its int.

---

## Lesson 3 — `[UniqueId]` collision guard

**Field:** `EnemyDefinition _step3Definition` — references `Data/walk_enemy_goblin.asset`.

`EnemyDefinition._id` carries `[UniqueId]` (see `Scripts/EnemyDefinition.cs`), so no two `EnemyDefinition`
assets may resolve to the same ID:

1. Select `Data/walk_enemy_goblin.asset` and duplicate it (**Cmd/Ctrl+D**).
2. Select the duplicate — the Inspector flags the `Id` field: the ID is already taken by the original.
3. Fix it by picking a free name in the dropdown (e.g. the `swim_enemy_shark` you added in Lesson 2) — the
   warning disappears.
4. Delete the duplicate when done.

- The guard is edit-time only — it costs nothing at runtime.

---

## Lesson 4 — Runtime lookups

**Fields:** `string _step4NameToResolve`, `EnemyDefinition[] _step4Catalog`.

Right-click the component header → **Log Tutorial Lookups** (works in Edit Mode). The Console shows every
runtime API in action:

```csharp
_registry.TryGetName(_step1EnemyId.Id, out var name); // int → name
_registry.TryGetId("walk_enemy_orc", out var id);     // name → int
_registry.Contains(999);                              // membership check → false

foreach (var entry in _registry)                      // iterate (Id, Name) rows in asset order
    Debug.Log($"{entry.Key} = {entry.Value}");
```

The last log line resolves the STEP 1 id against `_step4Catalog` — the exact pattern `Scripts/EnemySpawner.cs`
uses in the demo scene: compare `enemy.Id.Id` to the target and act on the match.

- Change `_step4NameToResolve` to a name that does not exist to see `TryGetId` fail gracefully.
- Lookups are dictionary-backed; the cache rebuilds lazily after the asset changes (`InvalidateCache` /
  `EnsureCache`).

---

## When names or IDs change

- **Renaming a registry entry** keeps its int — every field that references the ID stays valid and simply
  shows the new name.
- **Deleting a registry entry** leaves referencing fields with an int that no longer resolves; `TryGetName`
  returns `false` for it. Re-add a row or re-pick the field.
- The serialized name string on each field is a migration aid — the int is the source of truth.

---

## Where to look in code

| File | Shows |
|---|---|
| `Scripts/Tutorial/IdsTutorial.cs` | All four lessons as numbered fields + the `Log Tutorial Lookups` context menu |
| `Scripts/EnemyId.cs` | The one-line `partial struct : IId` declaration |
| `Scripts/EnemyDefinition.cs` | `[UniqueId]` on a serialized `EnemyId` field |
| `Scripts/EnemySpawner.cs` | Catalog lookup by id in a real component (the `Ids.unity` demo scene) |
| [README.md](README.md) | The compact demo-scene walkthrough |

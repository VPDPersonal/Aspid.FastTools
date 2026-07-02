# SerializeReferences Sample

A tiny loadout system that demonstrates `[TypeSelector]` — a searchable, hierarchical type dropdown for `[SerializeReference]` fields. Put both attributes on one field and the Inspector lets you pick which concrete implementation is instantiated; `<None>` clears the reference, and the instance's serialized fields appear inline under the foldout.

```csharp
[SerializeReference] [TypeSelector]
private IWeapon _weapon;
```

> **New here? Start with [TUTORIAL.md](TUTORIAL.md)** ([RU](TUTORIAL_RU.md)) — a guided, step-by-step tour (Lessons 1–8) built around `Scripts/Tutorial/TypeSelectorTutorial.cs` and `Scenes/TypeSelectorTutorial.unity`. This page is the feature reference; the tutorial is the walkthrough.

![The type picker window](../../Documentation/Images/aspid_fasttools_type_selector_window.png)

*The same searchable picker window, shown here on another candidate list — your fields open it filtered to their own type hierarchy.*

## How to run

Ready-made demo prefabs live in `Prefabs/` — double-click one to open it in Prefab Mode, or drag it into any scene. Start with **`Loadout.prefab`**, pre-filled with `Primary Weapon = Railgun` (carrying a nested `BurnEffect` charge effect), `Sidearms = [Pistol, Shotgun]`, `On Hit Effect = FreezeEffect`. Then experiment:

1. Click any type dropdown and pick another implementation — the instance is created and its serialized fields appear inline under the foldout.
2. Expand `Railgun` and change its nested `Charge Effect` to see recursive polymorphic editing.
3. Press **+** on `Sidearms` and give each element its own weapon type.
4. Open `On Hit Effect` — note only `BurnEffect` / `FreezeEffect` are offered (the abstract `StatusEffect` is hidden).
5. Open `Modifier` — the picker offers the concrete subclasses **and** the open generic `Modifier<T>`; picking `Modifier<T>` opens a second page inside the same picker to choose `T`. Open `Float Modifier` — only candidates assignable to `Modifier<float>` are offered, with `T` inferred (no extra page). The full walkthrough is [TUTORIAL.md, Lesson 6](TUTORIAL.md#lesson-6--generic-hierarchies).
6. Right-click the component header → **Log Loadout** to print the configured loadout to the Console.

Prefer building from scratch? Add an empty GameObject and attach the **Loadout** component.

Switching a field back to `<None>` clears the reference. If a stored type is later renamed or deleted, the dropdown shows a `<Missing …>` caption and a warning instead of silently clearing.

### The demo prefabs

| Prefab | Shows |
|---|---|
| `Loadout.prefab` | Every field flavour: single, list, abstract base, generics, nesting |
| `SlottedLoadout.prefab` | References inside plain `[Serializable]` containers (Lesson 7) |
| `LoadoutMissingType.prefab` | The missing-type warning and its inline **Fix** |
| `NestedLoadout.prefab` | A three-level hierarchy for the **Asset References** graph |
| `LoadoutSharedRef.prefab` | Shared-reference pairs, colour coding, **Make Unique Reference** |
| `IMGUILoadout.prefab` | The same data as `Loadout.prefab`, forced through the IMGUI renderer (see *The IMGUI path* below) |

## What's in the code

- `Scripts/Loadout.cs` — single (`IWeapon`), `List<IWeapon>`, abstract-base (`StatusEffect`) and generic (`IModifier` / `Modifier<float>`) `[SerializeReference]` fields, each annotated with `[TypeSelector]`.
- `Scripts/Weapons/` — `IWeapon` interface with its `IMelee` / `IRanged` branches and implementations (`Sword`, `Pistol`, `Shotgun`, `Railgun`, `Crossbow`). `Railgun` nests another `[TypeSelector]` field; `Crossbow` carries a `[MovedFrom]` used by the migration demo.
- `Scripts/Effects/` — abstract `StatusEffect` base with `BurnEffect` / `FreezeEffect`. The dropdown offers only the concrete subclasses.
- `Scripts/Modifiers/` — the generic hierarchy: open generic `Modifier<T>` plus closed subclasses over `float` / `int` / `string` ([TUTORIAL.md, Lesson 6](TUTORIAL.md#lesson-6--generic-hierarchies)).
- `Scripts/WeaponSlot.cs` — the plain `[Serializable]` container used by `SlottedLoadout` and the tutorial's Lesson 7.
- `Scripts/WeaponPreset.cs` + `Presets/` — `ScriptableObject`s whose stored type identities are deliberately broken or stale, used by the repair flows below.

## Maintenance features

The drawer also helps recover from the ways a managed reference goes wrong in practice.

### Copy / Paste & keep-data

- **Right-click** any selector header → **Copy Serialize Reference** / **Paste Serialize Reference**. Paste rebuilds an *independent* instance in the target field and is greyed out when the copied type does not fit the field.
- **Switching the type** keeps the fields the old and new implementation share. Set `Sidearms[0]` to `Pistol`, give it a damage value, then switch it to `Shotgun` and back — the `Pistol` value is still there.

### Repair a missing type — `BrokenWeaponPreset.asset` & `LoadoutMissingType.prefab`

Five assets ship storing type identities that no longer resolve directly:

- `Presets/BrokenWeaponPreset.asset` — a `ScriptableObject` whose `Weapon` references a missing `GhostWeapon`.
- `Presets/BrokenArsenalPreset.asset` — a second `ScriptableObject` that also references the missing `GhostWeapon`, three times over (`Weapon` plus two of its `Alternates`), so it shares a broken type with `BrokenWeaponPreset.asset`.
- `Prefabs/LoadoutMissingType.prefab` — a prefab whose `Sidearms → Element 0` references a missing `GhostPistol`.
- `Presets/MovedWeaponPreset.asset` — a `ScriptableObject` whose `Weapon` stores `Pistol` under an old `…Samples.SerializeReferences.Legacy` namespace, as if the class had been moved without `[MovedFrom]` — this one demonstrates the one-click **Smart Fix** below.
- `Presets/RenamedWeaponPreset.asset` — a `ScriptableObject` whose `Weapon` stores the old `CrossbowLauncher` class name; the class now ships as `Crossbow` carrying a declared `[MovedFrom]`, so the Inspector shows a healthy weapon and only the file is stale — this one demonstrates the **Migrate all** flow in Project References.

Select any of the first four **in the Project window**. The missing field shows a `<Missing …>` caption, a **Missing type** warning, and a **Fix** button:

1. Click **Fix** — the usual searchable type picker opens. Choose `Pistol`.
2. The reference is restored to a `Pistol` with its preserved data (the prefab keeps `_damage = 15`, `_magazineSize = 12`; the asset keeps `_damage = 25`, `_magazineSize = 8`). Picking the type rewrites the stored type in the asset file rather than recreating the instance, so the values survive.

When the broken identity has a plausible successor, the warning also carries a one-click **Smart Fix** suggestion —
open `MovedWeaponPreset.asset`: its notice ends with **`→ Pistol?`** (hover for the full identity and the ranking
reason). Click it to re-point the reference without opening the picker, keeping `_damage = 21`, `_magazineSize = 6`.
Suggestions rank a declared `[MovedFrom]` highest, then a same-named type in another namespace/assembly, a casing-only
rename, and a near-miss name backed by the orphaned data's field shape — and are never applied automatically. (A move
that ships `[MovedFrom]` from the start never breaks at all: Unity migrates it on load; Smart Fix catches the moves
that forgot it. The `GhostWeapon`/`GhostPistol` assets above have no plausible successor, so they show no suggestion —
that contrast is intentional.)

> The repair reads and rewrites the asset file directly — Unity does not expose a missing type through its serialization API (and on GameObjects/prefabs even drops it from the live object, UUM-129100), so the orphaned type and data are recovered straight from the YAML. It works for ScriptableObjects and prefab assets selected in the Project (rewritten in their YAML), for objects open in **Prefab Mode** (repaired on the live instance), and for objects in a **clean saved scene** (located via `GlobalObjectId`) — but not for an **unsaved/dirty scene** or a **prefab-instance override**, which have no committed asset document to map the reference to.
>
> When a missing reference is nested inside another value or sits on a child object the Inspector can't reach, use **`Tools → Aspid 🐍 → FastTools → Asset References`** instead: it scans the whole asset file and lists every missing reference (any depth, any child) with its own **Fix** picker.
>
> Its **Project References** tab sweeps every asset under `Assets/` and groups the broken references by their stored type — so `BrokenWeaponPreset.asset` and `BrokenArsenalPreset.asset` collapse into a single **GhostWeapon** group (`4 entries · 2 files`). One **Fix all** picks a single replacement and re-points every entry across both files at once. And `RenamedWeaponPreset.asset` surfaces there as a calm, info-tinted **pending migration** instead of a warning: its stored `CrossbowLauncher` matches the `[MovedFrom]` declared on `Crossbow`, so the card offers an authoritative **Migrate all (1) → Crossbow** that bakes the rename into the file — after which the attribute could be deleted from code.

### Map a nested graph — `NestedLoadout.prefab`

`Prefabs/NestedLoadout.prefab` is a three-level hierarchy — `NestedLoadout → WeaponSlot → BackupSlot` — with a `Loadout` on **every** object, so each child carries a broken reference the Inspector can't reach from outside Prefab Mode:

- **NestedLoadout** (root) — `Primary Weapon = Railgun` (with a nested `BurnEffect` charge effect), `Sidearms = [GhostPistol (missing), <None> (empty slot)]`, `On Hit Effect = FreezeEffect`.
- **WeaponSlot** (child) — `Primary Weapon = GhostBlade` (missing), `Sidearms[0] = Pistol`.
- **BackupSlot** (grandchild) — `On Hit Effect = GhostAura` (missing), `Primary Weapon = Shotgun`.

Select it **in the Project window** and open the **Asset References** tab — **`Tools → Aspid 🐍 → FastTools → Asset References`**. The graph maps all three components at once (one document per object). Every reference is an inline dropdown: pick a type to assign / re-point it, or `<None>` to clear it; the missing `GhostPistol` / `GhostBlade` / `GhostAura` cards carry the amber **Fix Missing** action. Nesting is read from the field path (`_primaryWeapon._chargeEffect`), not from indentation, so the flat card stack stays scannable.

### Un-share aliased references & tell groups apart by colour — `LoadoutSharedRef.prefab`

`Prefabs/LoadoutSharedRef.prefab` carries **two independent** shared-reference pairs on one object (each pair is a state you can also reach by duplicating an array element), so the rid-colour stripe/notice actually earns its keep:

- `Sidearms[0]` and `Sidearms[1]` both back the same `Pistol` — one colour.
- `Primary Weapon → Charge Effect` and `On Hit Effect` both back the same `BurnEffect` — a different colour, even though one is nested three levels deep and the other is a top-level field.

1. Open it — each pair shows a **shared reference** notice and editing one member changes its partner. Matching stripe/notice colour means matching instance regardless of where the field sits in the hierarchy, so the two pairs read as two distinct colours.
2. **Right-click** a member → **Make Unique Reference**. It gets its own copy of the data and the two fields become independent — its notice clears, and so does its former partner's, since nothing is shared any more.

## The IMGUI path

The drawer ships both a UIToolkit and an IMGUI rendering path, at full feature parity. **`Prefabs/IMGUILoadout.prefab`** carries the same data as `Loadout.prefab` but forces the IMGUI path, so you can compare the two renderers side by side — or copy the pattern into an IMGUI-only project.

The trick is the companion editor: `IMGUILoadoutEditor` overrides `OnInspectorGUI` **without** `CreateInspectorGUI` — that alone routes every nested `[TypeSelector]` field through `SerializeReferenceIMGUIPropertyDrawer` instead of the UIToolkit `CreatePropertyGUI`. The one IMGUI-specific caveat: Unity applies the drawer per list *element*, so a `[SerializeReference]` list's **+** button is drawn with `SerializeReferenceIMGUIList.Draw(listProperty, label, elementType)` to keep the picker-backed, de-aliased add. See `Scripts/IMGUILoadout.cs` + `Scripts/Editor/IMGUILoadoutEditor.cs`.

All maintenance windows (**Asset References**, **Project References**) are renderer-agnostic and work identically for both paths.

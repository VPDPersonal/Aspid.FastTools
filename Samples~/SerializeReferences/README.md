# SerializeReferences Sample

A tiny loadout system that demonstrates `[TypeSelector]` — a searchable, hierarchical type dropdown for `[SerializeReference]` fields. You pick which concrete implementation of a polymorphic field is instantiated, directly in the Inspector.

> **New here? Start with [TUTORIAL.md](TUTORIAL.md)** ([RU](TUTORIAL_RU.md)) — a guided, step-by-step tour (Lessons 1–8) built around `Scripts/Tutorial/TypeSelectorTutorial.cs` and `Scenes/TypeSelectorTutorial.unity`. This page is the feature reference; the tutorial is the walkthrough.

Look at:

- `Scripts/Loadout.cs` — single (`IWeapon`), `List<IWeapon>`, and abstract-base (`StatusEffect`) `[SerializeReference]` fields, each annotated with `[TypeSelector]`.
- `Scripts/Weapons/` — `IWeapon` interface and its implementations (`Pistol`, `Shotgun`, `Railgun`). `Railgun` nests another `[TypeSelector]` field, showing recursive polymorphic editing.
- `Scripts/Effects/` — abstract `StatusEffect` base with `BurnEffect` / `FreezeEffect`. The dropdown offers only the concrete subclasses; the abstract base is never listed.
- `Scripts/Modifiers/` — generic hierarchy: a non-abstract `Modifier<T>` generic class (`IModifier`) with closed-generic subclasses `DamageModifier : Modifier<float>`, `AmmoModifier : Modifier<int>`, `NameModifier : Modifier<string>`. An `IModifier` field offers all three subclasses **and** the open generic `Modifier<T>` — picking `Modifier<T>` opens a second window to choose the argument `T`. A `Modifier<float>` field offers only the candidates assignable to it (`DamageModifier`, and `Modifier<T>` with `T` inferred to `float`).
- `Scripts/WeaponPreset.cs` + `Presets/BrokenWeaponPreset.asset` — a `ScriptableObject` whose `_weapon` points at a type that no longer exists, used to demonstrate the missing-type repair flow (see *Maintenance features* below).

The drawer ships both a UIToolkit and an IMGUI rendering path. The `IMGUILoadout` variant forces the IMGUI path so you can compare them or migrate IMGUI-only projects:

- `Scripts/IMGUILoadout.cs` + `Scripts/Editor/IMGUILoadoutEditor.cs` — the same fields rendered via `OnInspectorGUI` (`SerializeReferenceIMGUIPropertyDrawer`).

## How to run

Two ready-made prefabs live in `Prefabs/` — double-click to open in Prefab Mode, or drag either into any scene:

- **Loadout** (`Prefabs/Loadout.prefab`) — UIToolkit path. Pre-filled: `Primary Weapon = Railgun` (with a nested `BurnEffect` charge effect), `Sidearms = [Pistol, Shotgun]`, `On Hit Effect = FreezeEffect`.
- **IMGUILoadout** (`Prefabs/IMGUILoadout.prefab`) — IMGUI path. Pre-filled: `Primary Weapon = Pistol`, `On Hit Effect = BurnEffect`.

Then experiment with the dropdowns:

1. Click any type dropdown and pick another implementation — the instance is created and its serialized fields appear inline under the foldout.
2. Expand `Railgun` and change its nested `Charge Effect` to see recursive polymorphic editing.
3. Press **+** on `Sidearms` and give each element its own weapon type.
4. Open `On Hit Effect` — note only `BurnEffect` / `FreezeEffect` are offered (the abstract `StatusEffect` is hidden).
5. Open `Modifier` — the three concrete subclasses (`DamageModifier`, `AmmoModifier`, `NameModifier`) are offered alongside the open generic `Modifier<T>`. Pick `Modifier<T>` and a second window opens to choose the argument `T` (try `string`, then `float`) before the instance is created. Open `Float Modifier` — only candidates assignable to `Modifier<float>` are offered (`DamageModifier`, and `Modifier<T>` whose `T` is inferred to `float` without the extra window).
6. Right-click the component header → **Log Loadout** to print the configured loadout to the Console.

Prefer building from scratch? Add an empty GameObject and attach the **Loadout** (UIToolkit) or **IMGUILoadout** (IMGUI) component.

Switching a field back to `<None>` clears the reference. If a stored type is later renamed or deleted, the dropdown shows a `<Missing …>` caption and a warning instead of silently clearing.

## Maintenance features

The drawer also helps recover from the two ways a managed reference goes wrong in practice.

### Copy / Paste & keep-data

- **Right-click** any selector header → **Copy Serialize Reference** / **Paste Serialize Reference**. Paste rebuilds an *independent* instance in the target field and is greyed out when the copied type does not fit the field.
- **Switching the type** keeps the fields the old and new implementation share. Set `Sidearms[0]` to `Pistol`, give it a damage value, then switch it to `Shotgun` and back — the `Pistol` value is still there.

### Repair a missing type — `BrokenWeaponPreset.asset` & `LoadoutMissingType.prefab`

Three assets ship pre-broken, pointing at classes that do not exist:

- `Presets/BrokenWeaponPreset.asset` — a `ScriptableObject` whose `Weapon` references a missing `GhostWeapon`.
- `Presets/BrokenArsenalPreset.asset` — a second `ScriptableObject` that also references the missing `GhostWeapon`, three times over (`Weapon` plus two of its `Alternates`), so it shares a broken type with `BrokenWeaponPreset.asset`.
- `Prefabs/LoadoutMissingType.prefab` — a prefab whose `Sidearms → Element 0` references a missing `GhostPistol`.

Select either **in the Project window**. The missing field shows a `<Missing …>` caption, a **Missing type** warning, and a **Fix** button:

1. Click **Fix** — the usual searchable type picker opens. Choose `Pistol`.
2. The reference is restored to a `Pistol` with its preserved data (the prefab keeps `_damage = 15`, `_magazineSize = 12`; the asset keeps `_damage = 25`, `_magazineSize = 8`). Picking the type rewrites the stored type in the asset file rather than recreating the instance, so the values survive.

> The repair reads and rewrites the asset file directly — Unity does not expose a missing type through its serialization API (and on GameObjects/prefabs even drops it from the live object, UUM-129100), so the orphaned type and data are recovered straight from the YAML. It therefore needs a **saved asset file**: it works for ScriptableObjects and prefab assets selected in the Project, but not for objects edited in Prefab Mode or instances living in a scene (no backing asset to rewrite).
>
> When a missing reference is nested inside another value or sits on a child object the Inspector can't reach, use **`Tools → Aspid 🐍 → Repair Missing References FastTools`** instead: it scans the whole asset file and lists every missing reference (any depth, any child) with its own **Fix** picker.
>
> Its **Project References** tab sweeps every asset under `Assets/` and groups the broken references by their stored type — so `BrokenWeaponPreset.asset` and `BrokenArsenalPreset.asset` collapse into a single **GhostWeapon** group (`4 entries · 2 files`). One **Fix all** picks a single replacement and re-points every entry across both files at once.

### Un-share an aliased reference — `LoadoutSharedRef.prefab`

`Prefabs/LoadoutSharedRef.prefab` has both `Sidearms` elements backed by the **same** instance (a state you can also reach by duplicating an array element).

1. Open it — both elements show a **shared reference** notice; editing one changes the other.
2. **Right-click** one element → **Make Unique Reference**. It gets its own copy of the data and the two fields become independent.

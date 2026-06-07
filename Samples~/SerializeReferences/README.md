# SerializeReferences Sample

A tiny loadout system that demonstrates `[SerializeReferenceSelector]` — a searchable, hierarchical type dropdown for `[SerializeReference]` fields. You pick which concrete implementation of a polymorphic field is instantiated, directly in the Inspector.

Look at:

- `Scripts/Loadout.cs` — single (`IWeapon`), `List<IWeapon>`, and abstract-base (`StatusEffect`) `[SerializeReference]` fields, each annotated with `[SerializeReferenceSelector]`.
- `Scripts/Weapons/` — `IWeapon` interface and its implementations (`Pistol`, `Shotgun`, `Railgun`). `Railgun` nests another `[SerializeReferenceSelector]` field, showing recursive polymorphic editing.
- `Scripts/Effects/` — abstract `StatusEffect` base with `BurnEffect` / `FreezeEffect`. The dropdown offers only the concrete subclasses; the abstract base is never listed.
- `Scripts/Modifiers/` — generic hierarchy: a non-abstract `Modifier<T>` generic class (`IModifier`) with closed-generic subclasses `DamageModifier : Modifier<float>`, `AmmoModifier : Modifier<int>`, `NameModifier : Modifier<string>`. An `IModifier` field offers all three subclasses **and** the open generic `Modifier<T>` — picking `Modifier<T>` opens a second window to choose the argument `T`. A `Modifier<float>` field offers only the candidates assignable to it (`DamageModifier`, and `Modifier<T>` with `T` inferred to `float`).

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

# SerializeReferences Sample

A tiny loadout system that demonstrates `[SerializeReferenceSelector]` — a searchable, hierarchical type dropdown for `[SerializeReference]` fields. You pick which concrete implementation of a polymorphic field is instantiated, directly in the Inspector.

Look at:

- `Scripts/Loadout.cs` — single (`IWeapon`), `List<IWeapon>`, and abstract-base (`StatusEffect`) `[SerializeReference]` fields, each annotated with `[SerializeReferenceSelector]`.
- `Scripts/Weapons/` — `IWeapon` interface and its implementations (`Pistol`, `Shotgun`, `Railgun`). `Railgun` nests another `[SerializeReferenceSelector]` field, showing recursive polymorphic editing.
- `Scripts/Effects/` — abstract `StatusEffect` base with `BurnEffect` / `FreezeEffect`. The dropdown offers only the concrete subclasses; the abstract base is never listed.

The drawer ships both a UIToolkit and an IMGUI rendering path. The `IMGUILoadout` variant forces the IMGUI path so you can compare them or migrate IMGUI-only projects:

- `Scripts/IMGUILoadout.cs` + `Scripts/Editor/IMGUILoadoutEditor.cs` — the same fields rendered via `OnInspectorGUI` (`SerializeReferenceIMGUIPropertyDrawer`).

## How to run

1. Create an empty GameObject in any scene and add the **Loadout** component (UIToolkit path) or **IMGUILoadout** component (IMGUI path).
2. In the Inspector, click a `<None>` dropdown and pick an implementation — e.g. `Primary Weapon → Railgun`. The instance is created and its serialized fields appear inline under the foldout.
3. Pick `Railgun`'s nested `Charge Effect → BurnEffect` to see recursive polymorphic editing.
4. Press **+** on `Sidearms` and give each element its own weapon type.
5. Set `On Hit Effect` — note only `BurnEffect` / `FreezeEffect` are offered (the abstract `StatusEffect` is hidden).
6. Right-click the component header → **Log Loadout** to print the configured loadout to the Console.

Switching a field back to `<None>` clears the reference. If a stored type is later renamed or deleted, the dropdown shows a `<Missing …>` caption and a warning instead of silently clearing.

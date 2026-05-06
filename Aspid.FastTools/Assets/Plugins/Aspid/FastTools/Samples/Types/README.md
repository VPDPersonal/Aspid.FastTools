# Types Sample

A tiny ability system that demonstrates polymorphic type selection in the Unity Inspector using `SerializableType<T>`, `TypeSelectorAttribute`, and `ComponentTypeSelector`. The player picks an `Ability` subclass and a list of `AbilityModifier` subclasses; enemies use `ComponentTypeSelector` so the concrete enemy script can be hot-swapped from the Inspector.

Look at:

- `Scripts/Abilities/AbilitySelector.cs:20` — `SerializableType<Ability>` field, constrained picker for a single subtype.
- `Scripts/Abilities/AbilitySelector.cs:26` — `[TypeSelector(typeof(AbilityModifier))]` on a `string[]` field.
- `Scripts/Enemies/EnemyBase.cs:18` — `ComponentTypeSelector` declaration that swaps the attached script in place.

Both Type drawers ship a UIToolkit and an IMGUI rendering path. Parallel `IMGUI*` variants force the IMGUI path so you can compare them side by side or migrate IMGUI-only projects:

- `Scripts/Abilities/IMGUIAbilitySelector.cs` + `Scripts/Editor/IMGUIAbilityHolderEditor.cs` — same `SerializableType<T>` / `[TypeSelector]` fields rendered via `OnInspectorGUI`.
- `Scripts/Enemies/IMGUI/IMGUIEnemyBase.cs` (+ `IMGUIFastEnemy`, `IMGUITankEnemy`) + `Scripts/Editor/IMGUIEnemyBaseEditor.cs` — IMGUI counterpart of the `ComponentTypeSelector` swap flow.

## How to run

Open `Scenes/Types.unity` — it contains two prefab instances:

- **AbilitySelector** (`Prefabs/AbilitySelector.prefab`) — an `AbilitySelector` with `Heal` pre-picked and all three modifiers filled in. Enter Play Mode to see the Console log the activated ability and each applied modifier.
- **Enemy** (`Prefabs/Enemy.prefab`) — a `FastEnemy` wired up through `ComponentTypeSelector`. Select it in the Hierarchy and use the type dropdown at the top of the Inspector to swap between `FastEnemy` and `TankEnemy` in place; the `Health` field persists across the swap.

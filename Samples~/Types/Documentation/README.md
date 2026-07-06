# Types Sample

A tiny ability system that demonstrates polymorphic type selection in the Unity Inspector using `SerializableType<T>`, `TypeSelectorAttribute`, and `ComponentTypeSelector`. The player picks an `Ability` subclass and a list of `AbilityModifier` subclasses; enemies use `ComponentTypeSelector` so the concrete enemy script can be hot-swapped from the Inspector.

> **New here? Start with [TUTORIAL.md](TUTORIAL.md)** ([RU](TUTORIAL_RU.md)) — a guided, step-by-step tour (Lessons 1–6) built around `Scripts/Tutorial/TypesTutorial.cs` and `Scenes/TypesTutorial.unity`. This page is the demo-scene walkthrough; the tutorial teaches the workflow.

Look at:

- `Scripts/Abilities/AbilitySelector.cs:20` — `SerializableType<Ability>` field, constrained picker for a single subtype.
- `Scripts/Abilities/AbilitySelector.cs:25` — `[TypeSelector(typeof(AbilityModifier))]` on a `string[]` field.
- `Scripts/Enemies/EnemyBase.cs:18` — `ComponentTypeSelector` declaration that swaps the attached script in place.

## How to run

Open `Scenes/Types.unity` — it contains two prefab instances:

- **AbilitySelector** (`Prefabs/AbilitySelector.prefab`) — an `AbilitySelector` with `Dash` pre-picked and all three modifiers filled in. Enter Play Mode to see the Console log the activated ability and each applied modifier.
- **Enemy** (`Prefabs/Enemy.prefab`) — a `FastEnemy` wired up through `ComponentTypeSelector`. Select it in the Hierarchy and use the type dropdown at the top of the Inspector to swap between `FastEnemy` and `TankEnemy` in place; the `Health` field persists across the swap.

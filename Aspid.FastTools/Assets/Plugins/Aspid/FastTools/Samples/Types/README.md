# Types Sample

A tiny ability system that demonstrates polymorphic type selection in the Unity Inspector using `SerializableType<T>`, `TypeSelectorAttribute`, and `ComponentTypeSelector`. The player picks an `Ability` subclass and a list of `AbilityModifier` subclasses; enemies use `ComponentTypeSelector` so the concrete enemy script can be hot-swapped from the Inspector.

Look at:

- `Scripts/AbilitySelector.cs:21` — `SerializableType<Ability>` field, constrained picker for a single subtype.
- `Scripts/AbilitySelector.cs:26` — `[TypeSelector(typeof(AbilityModifier), AllowAbstractTypes = false)]` on a `string[]` field.
- `Scripts/Enemies/EnemyBase.cs:18` — `ComponentTypeSelector` declaration that swaps the attached script in place.

To run: open `Scenes/Types.unity` (will be created in Unity) and press Play.

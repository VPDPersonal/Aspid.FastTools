# Types Sample

An enemy spawner that stores three `System.Type`s in one component and does something visible with each of them in Play Mode: which `Enemy` subclass to spawn, which elite variant to mix in, and which spawn pattern to lay the wave out with. The API reference lives in [Serializable Type System](../../Documentation/02-serializable-types.md); this page is the hands-on tour.

## Open it

1. Import the sample (**Package Manager → Aspid.FastTools → Samples**, or **Tools → Aspid 🐍 → FastTools → Welcome**).
2. Open `Scenes/Types.unity` and select **Enemy Spawner**.
3. Enter Play Mode: a wave of eight capsules spawns in a circle every six seconds, every fourth one an `ArmoredGrunt`, and walks to the center.

## Try

1. **Rename-safe component type.** `Enemy Type` is a `SerializableMonoScript<Enemy>`: the field references the script asset, not the class name. Rename the class in `Scripts/Enemies/Grunt.cs` to `Footman` (and the file), let Unity recompile, and the field still reads `Footman`. A `SerializableType` would have gone `<Missing>`.
2. **Dependent picker.** `Elite Type` is a plain `string` with `[TypeSelector(nameof(_enemyType))]`, so its picker offers only subtypes of whatever `Enemy Type` currently holds. Switch `Enemy Type` to `Archer` and open `Elite Type`: `Sniper` is offered, `ArmoredGrunt` is gone.
3. **Picker presentation.** Open `Pattern`. The candidates sit under one **Spawn Patterns** group with friendly names, tooltips and icons, all from `[TypeSelectorDisplay]` on the pattern classes. `OriginPattern` is not listed because it is `Hidden`; `Allow = TypeAllow.None` on the field keeps the `ISpawnPattern` interface itself out too. Pick **Grid** and spawn a wave.
4. **Required.** Set `Enemy Type` to `<None>`: an inline notice appears, and the field counts as a violation for the build/CI gate described in [SerializeReference Tooling](../../Documentation/04-serialize-reference-tooling.md).
5. **Swap a component in place.** Select **Placed Enemy (swap its type)**. The dropdown at the top of its Inspector comes from the `ComponentTypeSelector` field on `Enemy`. Switch `Archer` to `Brute`: `Health` and `Speed`, declared on the shared base, keep their values, while `Keep Distance` (Archer-only) is gone.

## Where to look

| File | Shows |
|---|---|
| `Scripts/EnemySpawner.cs` | `SerializableMonoScript<T>` with `Required`, a member-referenced `[TypeSelector]`, `SerializableType<T>` with `Allow = TypeAllow.None`, resolving each with `.Type` / `Type.GetType` |
| `Scripts/Enemies/Enemy.cs` | The `ComponentTypeSelector` field on the base class; subclasses in the same folder |
| `Scripts/Spawning/*.cs` | Plain C# strategies decorated with `[TypeSelectorDisplay]` (`Name`, `Group`, `Tooltip`, `Icon`, `Hidden`) |

Related: [SerializeReferences sample](../SerializeReferences/README.md) for `[TypeSelector]` on `[SerializeReference]` fields, [EditorTools sample](../EditorTools/README.md) for opening the same picker from your own editor code.

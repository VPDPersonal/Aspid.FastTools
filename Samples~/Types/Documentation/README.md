# Types Sample

A spawner whose enemy type and wave pattern are picked in the Inspector, without touching code.

![A wave of regular and elite enemies moves toward the center.](Images/demo.gif)

A wave of regular and elite enemies moves toward the center.

## Open it

1. Import the sample: **Tools → Aspid 🐍 → FastTools → Welcome** → **Samples** → **Import** on **Types**.
2. Open `Scenes/Types.unity` and enter Play Mode: every six seconds eight enemies appear in a circle, every fourth one an <code lang="class-name">ArmoredGrunt</code>, and walk to the center.

## Try

Exit Play Mode and select **Enemy Spawner**.

![Enemy types and spawn pattern in the Inspector.](Images/type-fields.png)

Enemy types and spawn pattern in the Inspector.

1. **A component type that survives a rename.** **Enemy Type** is a <code lang="class-name">SerializableMonoScript&lt;Enemy&gt;</code>: along with the class name, the field keeps a reference to the script asset. Rename the class in `Scripts/Enemies/Grunt.cs` to <code lang="class-name">Footman</code>, together with the file and its `.meta`, so that the derived <code lang="class-name">ArmoredGrunt</code> is updated too. After the recompile the field reads <code lang="class-name">Footman</code>; a <code lang="class-name">SerializableType</code> in its place would show `<Missing …>`.
2. **A dependent picker.** **Elite Type** is a string with <code lang="csharp">[TypeSelector(nameof(_enemyType))]</code>: its picker offers only the class in **Enemy Type** and its subclasses. Switch **Enemy Type** to <code lang="class-name">Archer</code> and open **Elite Type**: <code lang="class-name">Archer</code> and <code lang="class-name">Sniper</code> are offered, <code lang="class-name">ArmoredGrunt</code> is not. The field keeps <code lang="class-name">ArmoredGrunt</code> until you pick another type, so pick <code lang="class-name">Sniper</code>.
3. **Names, groups and icons.** Open **Pattern**: the patterns sit in one **Spawn Patterns** group, each with its own name, tooltip and icon from <code lang="csharp">[TypeSelectorDisplay]</code>. <code lang="class-name">OriginPattern</code> is hidden with `Hidden`, and <code lang="csharp">Allow = TypeAllow.None</code> on the field leaves out the <code lang="class-name">ISpawnPattern</code> interface itself. Pick **Grid** and enter Play Mode: the waves line up in a grid.
4. **A required field.** Set **Enemy Type** to `<None>`: a notice appears under the field. Save the scene, and **Project References → Scan Project** and CI runs with `-srGateRequired` report the field as a violation; a player build does not check it ([what each run checks](../../../Documentation/07-serialize-reference-validation.md#what-each-run-checks)).
5. **Swap a component in place.** Select **Placed Enemy (swap its type)** and set **Health** to `250`. The dropdown at the top of its Inspector is the <code lang="class-name">ComponentTypeSelector</code> field of <code lang="class-name">Enemy</code>. Switch <code lang="class-name">Archer</code> to <code lang="class-name">Brute</code>: **Health** and **Speed**, declared on the shared base, keep their values, and **Keep Distance**, which only <code lang="class-name">Archer</code> has, is gone.

## Where to look

| File | Shows |
|---|---|
| `Scripts/EnemySpawner.cs` | <code lang="class-name">SerializableMonoScript&lt;T&gt;</code> with `Required`, <code lang="csharp">[TypeSelector]</code> referencing another field, <code lang="class-name">SerializableType&lt;T&gt;</code> with <code lang="csharp">Allow = TypeAllow.None</code>, reading each type through <code lang="csharp">.Type</code> and <code lang="csharp">Type.GetType()</code> |
| `Scripts/Enemies/Enemy.cs` | The <code lang="class-name">ComponentTypeSelector</code> field on the base class; subclasses in the same folder |
| `Scripts/Spawning/` | The <code lang="class-name">ISpawnPattern</code> interface and plain C# patterns with <code lang="csharp">[TypeSelectorDisplay]</code> |

Reference: [Serializable Types](../../../Documentation/02-serializable-types.md), [TypeSelector](../../../Documentation/03-type-selector.md) and [ComponentTypeSelector](../../../Documentation/05-component-type-selector.md). See also the [SerializeReferences sample](../../SerializeReferences/Documentation/README.md) for <code lang="csharp">[TypeSelector]</code> on <code lang="csharp">[SerializeReference]</code> fields and the [EditorTools sample](../../EditorTools/Documentation/README.md) for the same picker opened from editor code.

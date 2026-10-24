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

1. **A type that survives a rename.** **Enemy Type** is a <code lang="class-name">SerializableMonoScript&lt;Enemy&gt;</code>. Rename <code lang="class-name">Grunt</code> to <code lang="class-name">Footman</code> in the code, the file and its `.meta`, so that <code lang="class-name">ArmoredGrunt</code> follows: after the recompile the field shows <code lang="class-name">Footman</code>.
2. **A dependent picker.** **Elite Type** offers only the class in **Enemy Type** and its subclasses. Switch **Enemy Type** to <code lang="class-name">Archer</code> and open **Elite Type**: only <code lang="class-name">Archer</code> is left.
3. **Names, groups and icons.** Open **Pattern**: **Circle** and **Line** sit in the **Spawn Patterns** group with icons from <code lang="csharp">[TypeSelectorDisplay]</code>, and the `Hidden` <code lang="class-name">OriginPattern</code> is left out. Pick **Line** and enter Play Mode: the enemies start in a line.
4. **Swap a component in place.** Select **Placed Enemy (swap its type)** and set **Health** to `250`. In the <code lang="class-name">ComponentTypeSelector</code> dropdown at the top of its Inspector, switch <code lang="class-name">Archer</code> to <code lang="class-name">Grunt</code>: **Health** stays `250`, and **Keep Distance**, which only <code lang="class-name">Archer</code> has, is gone.

## Where to look

| File | Shows |
|---|---|
| `Scripts/EnemySpawner.cs` | <code lang="class-name">SerializableMonoScript&lt;T&gt;</code>, <code lang="class-name">SerializableType&lt;T&gt;</code> and <code lang="csharp">[TypeSelector]</code> that follows another field |
| `Scripts/Enemies/Enemy.cs` | The <code lang="class-name">ComponentTypeSelector</code> field |
| `Scripts/Spawning/` | <code lang="csharp">[TypeSelectorDisplay]</code> names, groups and icons, and <code lang="csharp">[Preserve]</code>: a pattern is stored only by its name, so without it managed code stripping removes the class from a player |

Reference: [Serializable Types](../../docs/02-serializable-types.md), [TypeSelector](../../docs/03-type-selector.md) and [ComponentTypeSelector](../../docs/05-component-type-selector.md).

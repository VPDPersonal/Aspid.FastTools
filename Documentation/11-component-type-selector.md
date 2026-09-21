# ComponentTypeSelector

`ComponentTypeSelector` changes the type of an existing component or ScriptableObject in the Inspector. Switching between subclasses preserves shared field values.

## Quick start

Add the field to a base class. The picker offers compatible concrete types and has no `<None>` entry.

```csharp
using UnityEngine;
using Aspid.FastTools.Types;

public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField] private ComponentTypeSelector _enemyType;
    [SerializeField, Min(0)] private float _health = 100f;
}
```

Save the base class as `EnemyBase.cs`. Create subclasses in **separate files** matching their class names:

| FastEnemy.cs | ArmoredEnemy.cs |
|---|---|
| <pre lang="csharp"><code>using UnityEngine;&#10;&#10;public sealed class FastEnemy : EnemyBase&#10;&#123;&#10;    [SerializeField] private float _speed = 25f;&#10;&#125;</code></pre> | <pre lang="csharp"><code>using UnityEngine;&#10;&#10;public sealed class ArmoredEnemy : EnemyBase&#10;&#123;&#10;    [SerializeField] private int _armor = 10;&#10;&#125;</code></pre> |

Add **FastEnemy** to a GameObject, set **Health = 75**, and select **ArmoredEnemy** in the picker. The shared `Health` remains, `Speed` disappears, and `Armor` appears. Do not assume fields unique to the previous class will survive a later switch back.

![Switching a component type with ComponentTypeSelector](Images/component-type-selector.gif)

Switching a component type with ComponentTypeSelector

The selected class must have its own script file that Unity recognizes. If no suitable script is found, the type stays unchanged and the Console shows a warning.

## Package sample

Try component type switching in the [Types sample](../Samples~/Types/Documentation/README.md).

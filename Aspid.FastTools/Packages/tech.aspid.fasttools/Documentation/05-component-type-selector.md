# ComponentTypeSelector

Change a component's class right in the Inspector while shared fields keep their values.

## Quick start

```csharp
public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField] private ComponentTypeSelector _enemyType;
    [SerializeField] private float _health = 100f;
}
```

| FastEnemy.cs | ArmoredEnemy.cs |
|---|---|
| <pre lang="csharp"><code>public sealed class FastEnemy : EnemyBase&#10;&#123;&#10;    [SerializeField]&#10;    private float _speed = 25f;&#10;&#125;</code></pre> | <pre lang="csharp"><code>public sealed class ArmoredEnemy : EnemyBase&#10;&#123;&#10;    [SerializeField]&#10;    private int _armor = 10;&#10;&#125;</code></pre> |

The list offers the concrete subclasses of the class that declares the field, with no `<None>` entry.

![Picking ArmoredEnemy instead of FastEnemy keeps Health = 75](Images/component-type-selector.gif)

| Field | FastEnemy | ArmoredEnemy picked | Back to FastEnemy |
|---|---|---|---|
| **Health** | 75 | 75 | 75 |
| **Speed** | 40 | — | 25 |
| **Armor** | — | 10 | — |

## When the type does not change

The switch follows the rules of **Add Component**: it first adds the components the new class lists in <code lang="csharp">[RequireComponent]</code>, and one Undo reverts it together with them. The class stays as it was and the Console shows a warning when:

- the class has no script file of its own with the same name, such as a nested class or a second class in a file;
- the new class is marked <code lang="csharp">[DisallowMultipleComponent]</code> and the GameObject already has one;
- the switch would remove a class another component requires;
- the new class requires a component that cannot be added, such as the abstract <code lang="class-name">Collider</code>.

## Related features

For class display settings, search and favorites, see [TypeSelector](03-type-selector.md#typeselectordisplay). Here the class declaring the field determines the candidates; the <code lang="csharp">[TypeSelector]</code> attribute does not configure them.

To select a type in a separate field, see [Serializable Types](02-serializable-types.md); to create an object in a field, see [SerializeReference Selector](04-serialize-reference-selector.md).

## Package sample

The [Types](../Samples~/Types/Documentation/README.md) sample shows component type switching.

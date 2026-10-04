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

The list offers the concrete subclasses of the class that declares the field, with no `<None>` entry. Display settings from [<code lang="csharp">[TypeSelectorDisplay]</code>](03-type-selector.md#typeselectordisplay) apply here, while <code lang="csharp">[TypeSelector]</code> does not configure the list.

![Picking ArmoredEnemy instead of FastEnemy keeps Health = 75](Images/component-type-selector.gif)

| Field | <code lang="class-name">FastEnemy</code> | <code lang="class-name">ArmoredEnemy</code> picked | Back to <code lang="class-name">FastEnemy</code> |
|---|---|---|---|
| **Health** | 75 | 75 | 75 |
| **Speed** | 40 | — | 25 |
| **Armor** | — | 10 | — |

## When the type does not change

The switch follows the rules of **Add Component**: the components the new class lists in <code lang="csharp">[RequireComponent]</code> are added with it. If Unity would refuse to add the new class or remove the old one, the class stays as it was and the Console shows why. The same happens when the class has no script file of its own with the same name, such as a nested class.

## Package sample

The [Types](../Samples~/Types/Documentation/README.md) sample shows component type switching.

![A wave of regular and elite enemies in the Types scene](../Samples~/Types/Documentation/Images/demo.gif)

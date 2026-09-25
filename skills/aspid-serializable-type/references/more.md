# TypeSelectorWindow and ComponentTypeSelector

## TypeSelectorWindow (editor code)

```csharp
using Aspid.FastTools.Types;
using Aspid.FastTools.Types.Editors;

TypeSelectorWindow.Show(
    screenRect,                                   // button rect in SCREEN coordinates
    new TypeSelectorFilter { Types = new[] { typeof(Weapon) }, Allow = TypeAllow.None },
    currentAqn: selectedTypeName,
    onSelected: aqn => selectedTypeName = aqn);   // null for <None>; not called when dismissed
```

- `TypeSelectorFilter` is a struct whose default `Allow` is `None` (the attribute's default is `All`). Other members:
  `Predicate`, `AdditionalTypes`, `ArgumentFilter`, `InferredArgumentFilter`, `IncludeHidden`, `HideNoneOption`.
- `currentAqn: ""` (default) marks `<None>` as current; `null` marks nothing.
- Picking an open generic type walks the user through its arguments and returns the closed type's name.

## ComponentTypeSelector

```csharp
using Aspid.FastTools.Types;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField] private ComponentTypeSelector _enemyType;   // marker struct, holds no data
    [SerializeField, Min(0)] private float _health = 100f;
}
```

The dropdown lists concrete subclasses of the declaring class (no `<None>`) and rewrites the object's script,
keeping the fields both classes share. Each subclass must live in its own file named after the class, otherwise the
switch is skipped with a Console warning.

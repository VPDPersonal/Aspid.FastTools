# TypeField, TypeSelectorWindow and ComponentTypeSelector

## TypeField (editor code)

```csharp
using Aspid.FastTools.Types;
using Aspid.FastTools.Types.Editors;

// Bound: writes the assembly-qualified name with Undo and follows outside edits.
root.Add(new TypeField(serializedObject.FindProperty("_weaponName"))
{
    Types = new[] { typeof(Weapon) },             // assignable to ALL of them; null entries are dropped
    Allow = TypeAllow.None,                       // the default: concrete types only
});

// Unbound: store the name yourself.
var field = new TypeField("Weapon") { Types = new[] { typeof(Weapon) } };
field.SetValueFromAssemblyQualifiedNameWithoutNotify(_weaponName);
field.RegisterValueChangedCallback(_ => _weaponName = field.AssemblyQualifiedName);
```

- `InspectorTypeField` is the same field with its label aligned to the Inspector rows.
- The bound constructors accept only a string property; any other property throws `ArgumentException`.
- A pick writes the property first, then sends `ChangeEvent<Type>`. Read `evt.newValue`: the caller's
  `SerializedObject` still holds the old name until `Update()`.
- A stored name that no longer resolves shows `<Missing …>`. `value` is then `null`, but `AssemblyQualifiedName`
  keeps the stored name: save `AssemblyQualifiedName`, not `value?.AssemblyQualifiedName`.
- Other members: `Predicate`, `HideNoneOption`, `IsReadOnly`, `ExcludeEditorOnlyTypes` (a bound field sets it for
  a property of a runtime object).
- A closed generic base type (`IHandler<int>`) also offers the generic classes that close to it (`Handler<int>`).
- On a prefab instance an overridden value gets a bold label and **Apply** / **Revert** in its context menu.
- UXML creates an unbound field: `binding-path` does not bind it, and `Types` and `Predicate` have no UXML
  attributes. Create a bound or constrained field in C#.

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
switch is skipped with a Console warning. The switch adds the new class's `[RequireComponent]` components and is
refused (with a warning) when it would duplicate a `[DisallowMultipleComponent]` class, drop a class another
component requires, or need a component that cannot be added (an abstract class such as `Collider`).

---
name: aspid-serializable-type
description: "Aspid.FastTools type picking in the Unity Inspector: SerializableType<T>, SerializableMonoScript<T>, [TypeSelector] on string, wrapper and [SerializeReference] fields, [TypeSelectorDisplay], ComponentTypeSelector, analyzers AFT0001-AFT0009. Use when storing a System.Type in a serialized field, letting designers pick a class or an interface implementation in the Inspector, or fixing these analyzer warnings."
---

# Serializable types and [TypeSelector]

Namespace `Aspid.FastTools.Types`. If the user's scripts have an `.asmdef`, it must reference `Aspid.FastTools`
(editor code using `TypeSelectorWindow` also `Aspid.FastTools.Editor`).

| Need | Use |
|---|---|
| Store a type, create the instance in code | `SerializableType<T>` |
| Same, but survive renaming/moving the class with its file | `SerializableMonoScript<T>` |
| Picker on a `string` (stores the assembly-qualified name) or extra constraints | `[TypeSelector]` |
| Store an instance with data of a chosen implementation | `[TypeSelector]` + `[SerializeReference]` |
| Switch a MonoBehaviour/ScriptableObject to a sibling subclass | `ComponentTypeSelector`, see [references/more.md](references/more.md) |
| Open the picker from custom editor code | `TypeSelectorWindow.Show`, see [references/more.md](references/more.md) |

```csharp
using System;
using System.Collections.Generic;
using Aspid.FastTools.Types;
using UnityEngine;

public sealed class Armory : MonoBehaviour
{
    [TypeSelector(Allow = TypeAllow.None)]                  // concrete classes only
    [SerializeField] private SerializableType<Weapon> _weaponClass;

    [TypeSelector(typeof(ITwoHanded), Allow = TypeAllow.None)]   // MeleeWeapon AND ITwoHanded
    [SerializeField] private SerializableType<MeleeWeapon> _heavy;

    [TypeSelector(typeof(Weapon), Required = true)]
    [SerializeField] private string _backupName;           // Type.GetType(_backupName)

    [TypeSelector(nameof(_weaponClass))]                    // constraint = current value of _weaponClass
    [SerializeField] private string _variantName;

    [TypeSelector(typeof(IMelee))]                          // IWeapon AND IMelee; implementations must be [Serializable]
    [SerializeReference] private IWeapon _primary;

    [TypeSelector]
    [SerializeReference] private List<IWeapon> _sidearms = new();

    private void Start()
    {
        Type type = _weaponClass;                           // implicit Type?, null when empty or unresolved
        if (type != null) Activator.CreateInstance(type);
    }
}
```

## SerializableType / SerializableMonoScript

- Both have a picker without any attribute; `[TypeSelector]` only adds constraints or `Required`.
- Read: `.Type` (lazy; the result, `null` included, is cached until the stored name changes), implicit `Type?`,
  `AssemblyQualifiedName` (stored name, kept when it no longer resolves), `BaseType` (`typeof(T)`).
- `T` limits the picker and the constructor only: a name stored before `T` or the class's base changed still resolves,
  so check `BaseType.IsAssignableFrom(type)` before casting when that matters.
- A player resolves only the stored name, which managed stripping does not see: with Managed Stripping Level Low or
  higher, mark types picked only in the Inspector `[Preserve]` (`UnityEngine.Scripting`) or list them in `link.xml`,
  else `.Type` is `null` in the build. Same for `[TypeSelector]` strings.
- Create in code: `new SerializableType<Weapon>(typeof(Sword))` (throws `ArgumentException` if not assignable;
  `null` = empty). `SerializableMonoScript` has no public constructor.
- `SerializableMonoScript` accepts only top-level non-generic classes in a file of the same name; the user can drag
  the `.cs` file onto the field. Only it follows renames (in the editor it falls back to the script's class until the
  asset is re-saved): for `SerializableType` and strings a renamed class or namespace makes `.Type` return `null`
  silently.
- Declare the generic variant on the field: Unity serializes by the declared type, so a `SerializableType` field
  loses `T`.

## [TypeSelector]

- Valid on `string`, the wrappers, `[SerializeReference]` fields and arrays/`List<T>` of these (else `AFT0001`).
- Constructors: `()`, `(params Type[])`, `(params string[])`; no mixing of `Type` and `string` in one attribute.
- Several types mean assignable to **all** of them. Alternative classes (`typeof(Sword), typeof(Axe)`) leave the picker
  empty (`AFT0009`): pass their common base or interface.
- `Allow` defaults to `TypeAllow.All` (abstract classes and interfaces included). Set `Allow = TypeAllow.None`
  whenever the type will be instantiated. Ignored on `[SerializeReference]` (`AFT0002`).
- A string argument is first looked up as an instance field/property of the declaring class (`Type`, `string`, a
  wrapper, or an array of these; inside a `[Serializable]` class or list element, on that instance), else parsed as
  `"Namespace.Type, Assembly"`. Always use `nameof(...)`. An empty source adds no constraint.
- `Required = true` shows an inline warning when empty and feeds the package's Project References scan and CI gate;
  it is not a runtime null check.
- `[Conditional("UNITY_EDITOR")]`: never read it at runtime.
- `[SerializeReference]`: the field type is the first constraint, only instantiable classes are offered, nested
  managed references get the selector automatically; `UnityEngine.Object` field types are invalid (`AFT0004`).

## [TypeSelectorDisplay]

On a class/struct/interface: `Name`, `Group` (`"Combat/Modifiers"`, replaces namespace grouping), `Tooltip`, `Icon`
(`EditorGUIUtility.IconContent` name, `Assets/...` path, or `Resources` path), `Hidden = true` (not inherited). Search
still matches the real type name.

## Analyzers

Fix the code, do not suppress: `AFT0001` unsupported field; `AFT0002` `Allow` on `[SerializeReference]`; `AFT0003`
base type shares no concrete type with the field; `AFT0004` managed reference to a `UnityEngine.Object` type; `AFT0005`
no concrete implementation; `AFT0006` string is neither a member nor a type name; `AFT0007` member cannot supply base
types; `AFT0008` invalid type-name syntax; `AFT0009` base types have no type in common.

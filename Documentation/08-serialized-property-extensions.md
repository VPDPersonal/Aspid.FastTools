# SerializedProperty Extensions

`SerializedProperty` extensions for writing values, resizing collections, and finding a field’s type and owner.

## Quick start

The examples on this page work with the `AbilityBook.cs` component:

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Ability
{
    public string Name = "Fireball";
}

public class AbilityBook : MonoBehaviour
{
    [SerializeField] private int _manaCost = 10;
    [SerializeField] private float _cooldown = 1f;
    [SerializeField] private List<Ability> _abilities = new() { new Ability() };
}
```

In its custom `Editor`, add `using Aspid.FastTools.Editors;` and write a field value:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>var manaCost = serializedObject&#10;    .FindProperty("_manaCost");&#10;&#10;serializedObject.Update();&#10;manaCost.intValue = 42;&#10;serializedObject&#10;    .ApplyModifiedProperties();</code></pre> | <pre lang="csharp"><code>var manaCost = serializedObject&#10;    .FindProperty("_manaCost");&#10;&#10;manaCost&#10;    .Update()&#10;    .SetIntAndApply(42);</code></pre> |

Setters, `Update()` and both `Apply…()` methods return the original property, so calls chain. The `AndApply` suffix applies the write with Undo.

**Multiple fields:** update once, write the values, then apply them together:

```csharp
serializedObject.Update();
serializedObject.FindProperty("_cooldown").SetFloat(0.5f);
serializedObject.FindProperty("_manaCost").SetIntAndApply(10);
```

The [EditorTools sample](../Samples~/EditorTools/Documentation/README.md) includes a working button example.

> [!IMPORTANT]
> Applying affects **all pending changes** on the associated `SerializedObject`.
> `Update()` discards unapplied writes — call it before changing fields.

## Update / Apply

The same Unity operations, called on a property:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>property.serializedObject&#10;    .Update();</code></pre> | <pre lang="csharp"><code>property.Update();</code></pre> |
| <pre lang="csharp"><code>property.serializedObject&#10;    .UpdateIfRequiredOrScript();</code></pre> | <pre lang="csharp"><code>property&#10;    .UpdateIfRequiredOrScript();</code></pre> |
| <pre lang="csharp"><code>property.serializedObject&#10;    .ApplyModifiedProperties();</code></pre> | <pre lang="csharp"><code>property&#10;    .ApplyModifiedProperties();</code></pre> |
| <pre lang="csharp"><code>property.serializedObject&#10;    .ApplyModifiedPropertiesWithoutUndo();</code></pre> | <pre lang="csharp"><code>property&#10;    .ApplyModifiedPropertiesWithoutUndo();</code></pre> |

## Writing values

Choose when to apply the write:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>// Apply later&#10;manaCost.intValue = 42;</code></pre> | <pre lang="csharp"><code>// Apply later&#10;manaCost.SetInt(42);</code></pre> |
| <pre lang="csharp"><code>// With Undo&#10;manaCost.intValue = 42;&#10;manaCost.serializedObject&#10;    .ApplyModifiedProperties();</code></pre> | <pre lang="csharp"><code>// With Undo&#10;manaCost.SetIntAndApply(42);</code></pre> |
| <pre lang="csharp"><code>// Without Undo&#10;manaCost.intValue = 42;&#10;manaCost.serializedObject&#10;    .ApplyModifiedPropertiesWithoutUndo();</code></pre> | <pre lang="csharp"><code>// Without Undo&#10;manaCost&#10;    .SetIntAndApplyWithoutUndo(42);</code></pre> |

Use `SetValue` as an alternative to an explicit setter: `SetValue(42)` is equivalent to `SetInt(42)`, and `SetValue(0.5f)` to `SetFloat(0.5f)`. The overload is selected **by the argument’s type**, which must match the field.

The `AndApply` and `AndApplyWithoutUndo` suffixes are available for all setters below, including `SetValue`, enums, collections, and references.

### Supported types

Every `SerializedProperty` value type has an explicit setter and a `SetValue` overload: `SetInt`, `SetUint`, `SetLong`, `SetUlong`, `SetFloat`, `SetDouble`, `SetBool`, `SetString`, `SetColor`, `SetGradient`, `SetHash128`, `SetRect`, `SetRectInt`, `SetBounds`, `SetBoundsInt`, `SetVector2`, `SetVector2Int`, `SetVector3`, `SetVector3Int`, `SetVector4`, `SetQuaternion` and `SetAnimationCurve`. `SetEntityId` for `UnityEngine.EntityId` is available in Unity 6.2 and newer only.

### Enums

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>// Second enum value&#10;mode.enumValueIndex = 1;</code></pre> | <pre lang="csharp"><code>// Second enum value&#10;mode.SetEnumIndex(1);</code></pre> |
| <pre lang="csharp"><code>// [Flags] bit mask&#10;flags.enumValueFlag = mask;</code></pre> | <pre lang="csharp"><code>// [Flags] bit mask&#10;flags.SetEnumFlag(mask);</code></pre> |

### Arrays and lists

For the `_abilities` collection property:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>abilities.arraySize = 5;&#10;abilities.arraySize += 1;&#10;abilities.arraySize += 2;&#10;abilities.arraySize -= 2;&#10;abilities.arraySize -= 1;</code></pre> | <pre lang="csharp"><code>abilities.SetArraySize(5);&#10;abilities.AddArraySize();     // +1&#10;abilities.AddArraySize(2);    // +2&#10;abilities.RemoveArraySize(2); // -2&#10;abilities.RemoveArraySize();  // -1</code></pre> |

These methods only change the collection size. `RemoveArraySize` removes elements from the end; initialize new elements separately via `GetArrayElementAtIndex()`.

### References and boxed values

Choose the setter by how the field is serialized:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>// [SerializeReference]&#10;property.managedReferenceValue = instance;</code></pre> | <pre lang="csharp"><code>// [SerializeReference]&#10;property.SetManagedReference(instance);</code></pre> |
| <pre lang="csharp"><code>// UnityEngine.Object&#10;property.objectReferenceValue = asset;</code></pre> | <pre lang="csharp"><code>// UnityEngine.Object&#10;property.SetObjectReference(asset);</code></pre> |
| <pre lang="csharp"><code>// ExposedReference&lt;T&gt;&#10;property.exposedReferenceValue = target;</code></pre> | <pre lang="csharp"><code>// ExposedReference&lt;T&gt;&#10;property.SetExposedReference(target);</code></pre> |
| <pre lang="csharp"><code>// boxedValue&#10;property.boxedValue = value;</code></pre> | <pre lang="csharp"><code>// boxedValue&#10;property.SetBoxed(value);</code></pre> |

## Field type and owner

For the `AbilityBook` from the quick start, get the collection, its first element and the element’s field. The list must contain at least one element:

```csharp
var abilities = serializedObject.FindProperty("_abilities");
var ability = abilities.GetArrayElementAtIndex(0);
var abilityName = ability.FindPropertyRelative("Name");
```

### GetPropertyType()

Returns the declared field type, including `[SerializeReference]` fields. For a collection element, it returns the element type:

```csharp
abilities.GetPropertyType();   // typeof(List<Ability>)
ability.GetPropertyType();     // typeof(Ability)
abilityName.GetPropertyType(); // typeof(string)
```

### GetFieldInfo()

Returns the `FieldInfo`, resolving base-class fields and fields inside `[SerializeReference]` instances:

```csharp
abilityName.GetFieldInfo(); // Ability.Name field
ability.GetFieldInfo();     // AbilityBook._abilities field
```

### GetDeclaringInstance()

Returns the object that owns the field. For a collection element, it is the collection’s owner:

```csharp
ability.GetDeclaringInstance();     // AbilityBook instance
abilityName.GetDeclaringInstance(); // Ability instance
```

All three methods return `null` when resolution fails. They read the first target object (`targetObject`), so apply pending writes first. A struct owner is returned as a boxed copy: modifying it does not update the original.

## Member name and property checks

For the properties from the previous section:

| Call | Result |
|---|---|
| `ability.GetMemberName()` | `"_abilities"` — the collection name without the index |
| `abilityName.GetMemberName()` | `"Name"` |
| `ability.IsArrayElement()` | `true` |
| `abilityName.IsArrayElement()` | `false` — a field inside the element |
| `ability.HasFoldout()` | `true` |
| `abilityName.HasFoldout()` | `false` |

`HasFoldout()` checks for the `Generic` type and visible child properties. Managed references and custom `PropertyDrawer` layouts are not considered.

## Independent property

An inspector’s `SerializedObject` lives only while the inspector is open, so its properties cannot be kept for a deferred call. `Persistent()` returns the same property on a new `SerializedObject` for the same target objects:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>var independentObject =&#10;    new SerializedObject(property&#10;        .serializedObject.targetObjects);&#10;var independent = independentObject&#10;    .FindProperty(property.propertyPath);</code></pre> | <pre lang="csharp"><code>var independent = property.Persistent();</code></pre> |

The caller owns the new object: dispose it and the property after the write.

```csharp
var independent = manaCost.Persistent();
if (independent == null) return;

EditorApplication.delayCall += () =>
{
    using (independent.serializedObject)
    using (independent)
        independent.Update().SetIntAndApply(42);
};
```

`Persistent()` returns `null` when the property path no longer exists. Pending writes on the source object are not copied; the source view sees the changes after `Update()`. The target objects and property path must stay valid until the call.

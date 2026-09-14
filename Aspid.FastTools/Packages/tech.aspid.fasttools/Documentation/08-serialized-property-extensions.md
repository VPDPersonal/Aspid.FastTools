# SerializedProperty Extensions

`SerializedProperty` extensions for writing values, resizing collections, and finding a field’s type and owner.

## Quick start

Add `using Aspid.FastTools.Editors;` to your Editor script. Here, `serializedObject` comes from a custom inspector and `_manaCost` is an `int` field on its target.

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>var manaCost = serializedObject<br />    .FindProperty("_manaCost");<br /><br />serializedObject.Update();<br />manaCost.intValue = 42;<br />serializedObject<br />    .ApplyModifiedProperties();</code></pre> | <pre lang="csharp"><code>var manaCost = serializedObject<br />    .FindProperty("_manaCost");<br /><br />manaCost<br />    .Update()<br />    .SetIntAndApply(42);</code></pre> |

Setters, `Update`, and `Apply` return the original property for chaining. This example applies the write with Undo.

**Multiple fields:** update once, write the values, then apply them together:

```csharp
serializedObject.Update();
serializedObject.FindProperty("_cooldown").SetFloat(0.5f);
serializedObject.FindProperty("_manaCost").SetIntAndApply(10);
```

Here, `_cooldown` is a `float` field on the same object. The [EditorTools sample](../Samples~/EditorTools/Documentation/README.md) includes a working button example.

> [!IMPORTANT]
> Applying affects **all pending changes** on the associated `SerializedObject`.
> `Update()` discards unapplied writes — call it before changing fields.

## Update / Apply

The same Unity operations, called on a property. Each line below shows a separate option:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>property.serializedObject<br />    .Update();<br />property.serializedObject<br />    .UpdateIfRequiredOrScript();</code></pre> | <pre lang="csharp"><code>property.Update();<br /><br />property<br />    .UpdateIfRequiredOrScript();</code></pre> |
| <pre lang="csharp"><code>property.serializedObject<br />    .ApplyModifiedProperties();<br />property.serializedObject<br />    .ApplyModifiedPropertiesWithoutUndo();</code></pre> | <pre lang="csharp"><code>property<br />    .ApplyModifiedProperties();<br />property<br />    .ApplyModifiedPropertiesWithoutUndo();</code></pre> |

## Writing values

Choose when to apply the write:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>// Apply later<br />manaCost.intValue = 42;</code></pre> | <pre lang="csharp"><code>// Apply later<br />manaCost.SetInt(42);</code></pre> |
| <pre lang="csharp"><code>// With Undo<br />manaCost.intValue = 42;<br />manaCost.serializedObject<br />    .ApplyModifiedProperties();</code></pre> | <pre lang="csharp"><code>// With Undo<br />manaCost.SetIntAndApply(42);</code></pre> |
| <pre lang="csharp"><code>// Without Undo<br />manaCost.intValue = 42;<br />manaCost.serializedObject<br />    .ApplyModifiedPropertiesWithoutUndo();</code></pre> | <pre lang="csharp"><code>// Without Undo<br />manaCost<br />    .SetIntAndApplyWithoutUndo(42);</code></pre> |

Use `SetValue` as an alternative to an explicit setter: `SetValue(42)` is equivalent to `SetInt(42)`, and `SetValue(0.5f)` to `SetFloat(0.5f)`. The overload is selected **by the argument’s type**, which must match the field.

The `AndApply` and `AndApplyWithoutUndo` suffixes are available for all setters below, including `SetValue`, enums, collections, and references.

### Supported types

Each type has an explicit setter and `SetValue` overloads:

| Method | Value type |
|---|---|
| `SetInt` | `int` |
| `SetUint` | `uint` |
| `SetLong` | `long` |
| `SetUlong` | `ulong` |
| `SetFloat` | `float` |
| `SetDouble` | `double` |
| `SetBool` | `bool` |
| `SetString` | `string` |
| `SetColor` | `Color` |
| `SetGradient` | `Gradient` |
| `SetHash128` | `Hash128` |
| `SetRect` / `SetRectInt` | `Rect` / `RectInt` |
| `SetBounds` / `SetBoundsInt` | `Bounds` / `BoundsInt` |
| `SetVector2` / `SetVector2Int` | `Vector2` / `Vector2Int` |
| `SetVector3` / `SetVector3Int` | `Vector3` / `Vector3Int` |
| `SetVector4` | `Vector4` |
| `SetQuaternion` | `Quaternion` |
| `SetAnimationCurve` | `AnimationCurve` |
| `SetEntityId` | `EntityId` (`UnityEngine`) |

`SetEntityId` and its overloads are available only in Unity 6.2 and later.

### Enum setters

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>// Second enum entry<br />mode.enumValueIndex = 1;<br /><br />// [Flags] bitmask<br />flags.enumValueFlag = mask;</code></pre> | <pre lang="csharp"><code>// Second enum entry<br />mode.SetEnumIndex(1);<br /><br />// [Flags] bitmask<br />flags.SetEnumFlag(mask);</code></pre> |

## Arrays and lists

For an array or list property named `items`:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>items.arraySize = 5;<br />items.arraySize += 1;<br />items.arraySize += 2;<br />items.arraySize -= 2;<br />items.arraySize -= 1;</code></pre> | <pre lang="csharp"><code>items.SetArraySize(5);<br />items.AddArraySize();     // +1<br />items.AddArraySize(2);    // +2<br />items.RemoveArraySize(2); // -2<br />items.RemoveArraySize();  // -1</code></pre> |

These methods only resize the collection. `RemoveArraySize` removes elements from the end; initialize new elements separately through `GetArrayElementAtIndex()`.

## References and boxed values

Choose a setter according to how the field is serialized; each entry below is a separate option:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>// [SerializeReference]<br />property.managedReferenceValue = instance;<br /><br />// UnityEngine.Object<br />property.objectReferenceValue = asset;<br /><br />// ExposedReference&lt;T&gt;<br />property.exposedReferenceValue = target;<br /><br />// boxedValue<br />property.boxedValue = value;</code></pre> | <pre lang="csharp"><code>// [SerializeReference]<br />property.SetManagedReference(instance);<br /><br />// UnityEngine.Object<br />property.SetObjectReference(asset);<br /><br />// ExposedReference&lt;T&gt;<br />property.SetExposedReference(target);<br /><br />// boxedValue<br />property.SetBoxed(value);</code></pre> |

## Field type and owner

### Example: collection, element, and nested field

The `AbilityBook.cs` component:

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Ability
{
    public int ManaCost = 10;
}

public class AbilityBook : MonoBehaviour
{
    [SerializeField] private List<Ability> _abilities = new() { new Ability() };
}
```

In the inspector, retrieve the collection, its first element, and that element’s field. The list must contain at least one element:

```csharp
var abilities = serializedObject.FindProperty("_abilities");
var ability = abilities.GetArrayElementAtIndex(0);
var manaCost = ability.FindPropertyRelative("ManaCost");
```

### GetPropertyType()

Returns the declared field type, including for `[SerializeReference]`. For a collection element, returns the element type:

```csharp
abilities.GetPropertyType(); // typeof(List<Ability>)
ability.GetPropertyType();   // typeof(Ability)
manaCost.GetPropertyType();  // typeof(int)
```

### GetFieldInfo()

Returns the `FieldInfo`, including fields in base classes and inside `[SerializeReference]`:

```csharp
manaCost.GetFieldInfo(); // The Ability.ManaCost field
ability.GetFieldInfo();  // The AbilityBook._abilities field
```

### GetDeclaringInstance()

Returns the object that owns the field. For a collection element, this is the collection’s owner:

```csharp
ability.GetDeclaringInstance();  // AbilityBook instance
manaCost.GetDeclaringInstance(); // Ability instance
```

All three methods return `null` if the lookup fails. They read the first target object (`targetObject`), so apply pending writes first. A struct owner is returned as a boxed copy: changing it does not update the original.

## Field name and property checks

Using the properties from the previous example:

| Call | Result |
|---|---|
| `ability.GetMemberName()` | `"_abilities"` — the collection name without an index |
| `manaCost.GetMemberName()` | `"ManaCost"` |
| `ability.IsArrayElement()` | `true` |
| `manaCost.IsArrayElement()` | `false` — a field inside an element |
| `ability.HasFoldout()` | `true` |
| `manaCost.HasFoldout()` | `false` |

`HasFoldout()` checks for `Generic` with visible child properties. Managed references and custom `PropertyDrawer` layouts are not considered.

## Independent property

`Persistent()` finds the same path on a new `SerializedObject` for the same targets. This is useful for deferred calls:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>var independentObject =<br />    new SerializedObject(property<br />        .serializedObject.targetObjects);<br />var independent = independentObject<br />    .FindProperty(property.propertyPath);</code></pre> | <pre lang="csharp"><code>var independent = property.Persistent();</code></pre> |

For an `int` property, capture `independent` in a one-shot delegate and dispose the resources after writing:

```csharp
if (independent == null) return;

Action<int> applyValue = value =>
{
    using (independent.serializedObject)
    using (independent)
    {
        independent.Update().SetIntAndApply(value);
    }
};

EditorApplication.delayCall += () => applyValue(42);
```

The new `SerializedObject` has an independent lifetime but edits the same targets. Unapplied writes are not copied; the source sees changes after `Update()`. The targets and property path must remain valid until the call.

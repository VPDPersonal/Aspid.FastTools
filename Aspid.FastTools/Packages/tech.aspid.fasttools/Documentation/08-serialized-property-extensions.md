# SerializedProperty Extensions

Chainable extension methods that let a `SerializedProperty` write and apply its own value in one call, without going through its `SerializedObject`. A second group of methods tells which C# field and which object stand behind a property.

## Quick start

The examples on this page work with the `AbilityBook` component:

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

public interface IAbilityEffect { }

public enum Targeting { Single, Area }

[Flags]
public enum DamageTypes { Fire = 1, Ice = 2, Poison = 4 }

[Serializable]
public class BurnEffect : IAbilityEffect
{
    public float Damage = 5f;
}

[Serializable]
public class Ability
{
    public string Name = "Fireball";
}

public class AbilityBook : MonoBehaviour
{
    [SerializeField] private int _manaCost = 10;
    [SerializeField] private float _cooldown = 1f;
    [SerializeField] private Sprite _icon;
    [SerializeField] private Targeting _targeting;
    [SerializeField] private DamageTypes _damageTypes;
    [SerializeField] private List<Ability> _abilities = new() { new Ability() };
    [SerializeReference] private IAbilityEffect _effect = new BurnEffect();
}
```

In its custom `Editor`, add `using Aspid.FastTools.Editors;` and write a field value:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>var manaCost = serializedObject&#10;    .FindProperty("_manaCost");&#10;&#10;serializedObject.Update();&#10;manaCost.intValue = 42;&#10;serializedObject&#10;    .ApplyModifiedProperties();</code></pre> | <pre lang="csharp"><code>var manaCost = serializedObject&#10;    .FindProperty("_manaCost");&#10;&#10;manaCost&#10;    .Update()&#10;    .SetIntAndApply(42);</code></pre> |

Setters, `Update()` and both `Apply…()` methods return the original property, so calls chain. Every setter has `AndApply` (with Undo) and `AndApplyWithoutUndo` variants.

**Multiple fields:** update once, write the values, then apply them together:

```csharp
serializedObject.Update();
serializedObject.FindProperty("_cooldown").SetFloat(0.5f);
serializedObject.FindProperty("_manaCost").SetIntAndApply(10);
```

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

`SetValue` is an alternative to the explicit setter: `SetValue(42)` is equivalent to `SetInt(42)`, and `SetValue(0.5f)` to `SetFloat(0.5f)`. The overload is selected **by the argument’s type**, which must match the field.

### Supported types

Every `SerializedProperty` value type has an explicit setter and a `SetValue` overload:

| Values | Setters |
|---|---|
| Numbers | `SetInt`, `SetUint`, `SetLong`, `SetUlong`, `SetFloat`, `SetDouble` |
| Text, bool and hash | `SetString`, `SetBool`, `SetHash128` |
| Vectors | `SetVector2`, `SetVector2Int`, `SetVector3`, `SetVector3Int`, `SetVector4`, `SetQuaternion` |
| Areas | `SetRect`, `SetRectInt`, `SetBounds`, `SetBoundsInt` |
| Unity types | `SetColor`, `SetGradient`, `SetAnimationCurve` |
| Unity 6.2 and newer | `SetEntityId` for `UnityEngine.EntityId` |

### Enums

For the `_targeting` and `_damageTypes` properties:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>// Targeting.Area&#10;targeting.enumValueIndex = 1;</code></pre> | <pre lang="csharp"><code>// Targeting.Area&#10;targeting.SetEnumIndex(1);</code></pre> |
| <pre lang="csharp"><code>// Fire &#124; Ice&#10;damageTypes.enumValueFlag = 3;</code></pre> | <pre lang="csharp"><code>// Fire &#124; Ice&#10;damageTypes.SetEnumFlag(3);</code></pre> |

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
| <pre lang="csharp"><code>// [SerializeReference]&#10;effect.managedReferenceValue = instance;</code></pre> | <pre lang="csharp"><code>// [SerializeReference]&#10;effect.SetManagedReference(instance);</code></pre> |
| <pre lang="csharp"><code>// UnityEngine.Object&#10;icon.objectReferenceValue = sprite;</code></pre> | <pre lang="csharp"><code>// UnityEngine.Object&#10;icon.SetObjectReference(sprite);</code></pre> |
| <pre lang="csharp"><code>// ExposedReference&lt;T&gt;&#10;property.exposedReferenceValue = target;</code></pre> | <pre lang="csharp"><code>// ExposedReference&lt;T&gt;&#10;property.SetExposedReference(target);</code></pre> |
| <pre lang="csharp"><code>// boxedValue&#10;property.boxedValue = value;</code></pre> | <pre lang="csharp"><code>// boxedValue&#10;property.SetBoxed(value);</code></pre> |

## Field type and owner

Three methods find the C# field behind a property through reflection. For the `AbilityBook` from the quick start:

```csharp
var abilities    = serializedObject.FindProperty("_abilities");
var ability      = abilities.GetArrayElementAtIndex(0);
var abilityName  = ability.FindPropertyRelative("Name");
var effect       = serializedObject.FindProperty("_effect");
var effectDamage = effect.FindPropertyRelative("Damage");
```

| Property | `GetPropertyType()` | `GetFieldInfo()` | `GetDeclaringInstance()` |
|---|---|---|---|
| `abilities` | `List<Ability>` | `AbilityBook._abilities` | the `AbilityBook` |
| `ability` | `Ability` | `AbilityBook._abilities` | the `AbilityBook` |
| `abilityName` | `string` | `Ability.Name` | the `Ability` at index 0 |
| `effect` | `IAbilityEffect` | `AbilityBook._effect` | the `AbilityBook` |
| `effectDamage` | `float` | `BurnEffect.Damage` | the `BurnEffect` instance |

- `GetPropertyType()` returns the **declared** field type: for `[SerializeReference]` the interface or base class, not the instance type; for a collection element, the element type.
- `GetFieldInfo()` looks the field up on the owner’s actual type, including private fields of base classes.
- `GetDeclaringInstance()` returns the object that owns the field; for a collection element, the collection’s owner.

All three methods return `null` when resolution fails: a missing field, a `null` reference on the path or an index outside the list. They read the **first** target object (`targetObject`) and see applied values only, so apply pending writes first.

> [!WARNING]
> When the field’s owner is a struct, `GetDeclaringInstance()` returns a boxed copy. Changes to that copy never reach the original; write values through the `SerializedProperty` instead.

## Member name and property checks

For the same properties:

| Call | Result |
|---|---|
| `ability.GetMemberName()` | `"_abilities"` — the collection name without the index |
| `abilityName.GetMemberName()` | `"Name"` |
| `ability.IsArrayElement()` | `true` |
| `abilityName.IsArrayElement()` | `false` — a field inside the element |
| `ability.HasFoldout()` | `true` |
| `abilityName.HasFoldout()` | `false` |
| `effect.HasFoldout()` | `false` — `[SerializeReference]` |

`HasFoldout()` is `true` only for a `Generic` property with visible child properties, as in the default Inspector. `[SerializeReference]` and custom `PropertyDrawer` layouts are not considered.

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

`Persistent()` returns `null` when the property path no longer exists. Pending writes on the source object are not copied; the source view sees the changes after `Update()`. The target objects must stay alive until the deferred call.

## Package sample

In [EditorTools](../Samples~/EditorTools/Documentation/README.md), the **Halve cooldown, +5 MP** button writes two properties with `SetFloat` and `SetIntAndApply` as one Undo step.

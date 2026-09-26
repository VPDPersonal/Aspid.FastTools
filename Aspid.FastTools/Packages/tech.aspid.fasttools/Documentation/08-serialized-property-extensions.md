# SerializedProperty Extensions

Unity writes a field from an editor through the `SerializedObject`: `Update()`, the assignment and `ApplyModifiedProperties()` are three separate statements. With FastTools the property does it in one chain — `manaCost.Update().SetIntAndApply(42)` — and records Undo. Three more methods return what `SerializedProperty` itself does not expose: the C# field type, its `FieldInfo` and the object that owns it.

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

`Update()`, `UpdateIfRequiredOrScript()`, `ApplyModifiedProperties()` and `ApplyModifiedPropertiesWithoutUndo()` call the same methods of `serializedObject`. They and every setter return the property, so calls chain.

**Multiple fields:** update once and write the values — `…AndApply` on the last one applies every pending write of the `SerializedObject`:

```csharp
serializedObject.Update();
serializedObject.FindProperty("_cooldown").SetFloat(0.5f);
serializedObject.FindProperty("_manaCost").SetIntAndApply(10);
```

## Writing values

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>manaCost.intValue = 42;</code></pre> | <pre lang="csharp"><code>manaCost.SetInt(42);</code></pre> |
| <pre lang="csharp"><code>manaCost.intValue = 42;&#10;manaCost.serializedObject&#10;    .ApplyModifiedProperties();</code></pre> | <pre lang="csharp"><code>manaCost.SetIntAndApply(42);</code></pre> |
| <pre lang="csharp"><code>manaCost.intValue = 42;&#10;manaCost.serializedObject&#10;    .ApplyModifiedPropertiesWithoutUndo();</code></pre> | <pre lang="csharp"><code>manaCost&#10;    .SetIntAndApplyWithoutUndo(42);</code></pre> |

Every setter on this page comes in these three forms.

`SetValue(42)` is the same as `SetInt(42)`: the overload is chosen **by the argument's type**, which must match the field.

### Supported types

Each of these setters also has a `SetValue` overload:

| Values | Setters |
|---|---|
| Numbers | `SetInt`, `SetUint`, `SetLong`, `SetUlong`, `SetFloat`, `SetDouble` |
| Text, bool and hash | `SetString`, `SetBool`, `SetHash128` |
| Vectors | `SetVector2`, `SetVector2Int`, `SetVector3`, `SetVector3Int`, `SetVector4`, `SetQuaternion` |
| Areas | `SetRect`, `SetRectInt`, `SetBounds`, `SetBoundsInt` |
| Unity types | `SetColor`, `SetGradient`, `SetAnimationCurve` |
| Unity 6.2 and newer | `SetEntityId` |

### Enums

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>targeting.enumValueIndex = 1;</code></pre> | <pre lang="csharp"><code>targeting.SetEnumIndex(1);</code></pre> |
| <pre lang="csharp"><code>damageTypes.enumValueFlag = (int)&#10;    (DamageTypes.Fire &#124; DamageTypes.Ice);</code></pre> | <pre lang="csharp"><code>damageTypes.SetEnumFlag((int)&#10;    (DamageTypes.Fire &#124; DamageTypes.Ice));</code></pre> |

### Arrays and lists

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>abilities.arraySize = 5;&#10;abilities.arraySize += 1;&#10;abilities.arraySize += 2;&#10;abilities.arraySize -= 2;&#10;abilities.arraySize -= 1;</code></pre> | <pre lang="csharp"><code>abilities.SetArraySize(5);&#10;abilities.AddArraySize();&#10;abilities.AddArraySize(2);&#10;abilities.RemoveArraySize(2);&#10;abilities.RemoveArraySize();</code></pre> |

### References and boxed values

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>effect.managedReferenceValue = instance;</code></pre> | <pre lang="csharp"><code>effect.SetManagedReference(instance);</code></pre> |
| <pre lang="csharp"><code>icon.objectReferenceValue = sprite;</code></pre> | <pre lang="csharp"><code>icon.SetObjectReference(sprite);</code></pre> |
| <pre lang="csharp"><code>ability.boxedValue =&#10;    new Ability &#123; Name = "Frostbolt" &#125;;</code></pre> | <pre lang="csharp"><code>ability.SetBoxed(&#10;    new Ability &#123; Name = "Frostbolt" &#125;);</code></pre> |

`SetExposedReference` sets `exposedReferenceValue` of an `ExposedReference<T>` field.

> [!NOTE]
> Without an `IExposedPropertyTable` context, Unity's setter applies the write itself, with Undo: `SetExposedReference` does not wait for an apply, and `SetExposedReferenceAndApplyWithoutUndo` still records Undo.

## Field type and owner

Three methods find the C# field behind a property through reflection:

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

`GetFieldInfo()` also finds private fields declared in base classes.

All three methods return `null` when resolution fails: a missing field, a `null` reference on the path or an index outside the list. They read the **first** target object (`targetObject`) and see applied values only, so apply pending writes first.

> [!WARNING]
> When the field's owner is a struct, `GetDeclaringInstance()` returns a boxed copy. Changes to that copy never reach the original; write values through the `SerializedProperty` instead.

## Member name and property checks

| Call | Result |
|---|---|
| `ability.GetMemberName()` | `"_abilities"` |
| `abilityName.GetMemberName()` | `"Name"` |
| `ability.IsArrayElement()` | `true` |
| `abilityName.IsArrayElement()` | `false` |
| `ability.HasFoldout()` | `true` |
| `effect.HasFoldout()` | `false` |

`HasFoldout()` is `true` for a `Generic` property with visible child properties.

> [!NOTE]
> A `[SerializeReference]` field returns `false`, although the Inspector draws it with a foldout. Custom `PropertyDrawer` layouts are not considered.

## Persistent()

An inspector's `SerializedObject` lives only while the inspector is open, so its properties cannot be kept for a deferred call. `Persistent()` returns the same property on a new `SerializedObject` for the same target objects and context:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>var source = manaCost.serializedObject;&#10;var independentObject = new SerializedObject(&#10;    source.targetObjects, source.context);&#10;var independent = independentObject&#10;    .FindProperty(manaCost.propertyPath);</code></pre> | <pre lang="csharp"><code>var independent = manaCost.Persistent();</code></pre> |

The caller owns the new object:

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

`Persistent()` returns `null` when the property path no longer exists. Pending writes on the source object are not copied.

## Package sample

In [EditorTools](../Samples~/EditorTools/Documentation/README.md), the **Halve cooldown, +5 MP** button writes two properties with `SetFloat` and `SetIntAndApply` as one Undo step.

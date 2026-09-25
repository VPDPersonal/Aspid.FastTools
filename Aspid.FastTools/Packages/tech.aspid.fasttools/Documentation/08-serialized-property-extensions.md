# SerializedProperty Extensions

Write fields from the editor in one line with Undo — and get the C# type, field and object behind a property.

## Quick start

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>var manaCost = serializedObject&#10;    .FindProperty("_manaCost");&#10;&#10;serializedObject.Update();&#10;manaCost.intValue = 42;&#10;serializedObject&#10;    .ApplyModifiedProperties();</code></pre> | <pre lang="csharp"><code>var manaCost = serializedObject&#10;    .FindProperty("_manaCost");&#10;&#10;manaCost&#10;    .Update()&#10;    .SetIntAndApply(42);</code></pre> |

## Writing values

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>manaCost.intValue = 42;</code></pre> | <pre lang="csharp"><code>manaCost.SetInt(42);</code></pre> |
| <pre lang="csharp"><code>manaCost.intValue = 42;&#10;manaCost.serializedObject&#10;    .ApplyModifiedProperties();</code></pre> | <pre lang="csharp"><code>manaCost.SetIntAndApply(42);</code></pre> |
| <pre lang="csharp"><code>manaCost.intValue = 42;&#10;manaCost.serializedObject&#10;    .ApplyModifiedPropertiesWithoutUndo();</code></pre> | <pre lang="csharp"><code>manaCost&#10;    .SetIntAndApplyWithoutUndo(42);</code></pre> |

Every setter on this page except <code lang="function">SetExposedReference</code> comes in these three forms.

<code lang="csharp">SetValue(42)</code> is the same as <code lang="csharp">SetInt(42)</code>: the overload is chosen **by the argument's type**, which must match the field.

### Supported types

Each of these setters also has a <code lang="function">SetValue</code> overload:

| Values | Setters |
|---|---|
| Numbers | <code lang="function">SetInt</code>, <code lang="function">SetUint</code>, <code lang="function">SetLong</code>, <code lang="function">SetUlong</code>, <code lang="function">SetFloat</code>, <code lang="function">SetDouble</code> |
| Text, bool and hash | <code lang="function">SetString</code>, <code lang="function">SetBool</code>, <code lang="function">SetHash128</code> |
| Vectors | <code lang="function">SetVector2</code>, <code lang="function">SetVector2Int</code>, <code lang="function">SetVector3</code>, <code lang="function">SetVector3Int</code>, <code lang="function">SetVector4</code>, <code lang="function">SetQuaternion</code> |
| Areas | <code lang="function">SetRect</code>, <code lang="function">SetRectInt</code>, <code lang="function">SetBounds</code>, <code lang="function">SetBoundsInt</code> |
| Unity types | <code lang="function">SetColor</code>, <code lang="function">SetGradient</code>, <code lang="function">SetAnimationCurve</code> |
| Object IDs | <code lang="function">SetEntityId</code> |

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
| <pre lang="csharp"><code>target.exposedReferenceValue = light;</code></pre> | <pre lang="csharp"><code>target.SetExposedReference(light);</code></pre> |

### Update() and Apply…()

<code lang="csharp">Update()</code>, <code lang="csharp">UpdateIfRequiredOrScript()</code>, <code lang="csharp">ApplyModifiedProperties()</code> and <code lang="csharp">ApplyModifiedPropertiesWithoutUndo()</code> call the <code lang="csharp">serializedObject</code> methods of the same name and, like every setter, return the property.

## The C# field behind a property

Three methods find the C# field behind a property through reflection:

```csharp
public class AbilityBook : MonoBehaviour
{
    [SerializeField]
    private List<Ability> _abilities = new() { new Ability() };

    [SerializeReference]
    private IAbilityEffect _effect = new BurnEffect();
}

[Serializable]
public class Ability { public string Name = "Fireball"; }

[Serializable]
public class BurnEffect : IAbilityEffect { public float Damage = 5f; }
```

```csharp
var abilities    = serializedObject.FindProperty("_abilities");
var ability      = abilities.GetArrayElementAtIndex(0);
var abilityName  = ability.FindPropertyRelative("Name");
var effect       = serializedObject.FindProperty("_effect");
var effectDamage = effect.FindPropertyRelative("Damage");
```

| Property | <code lang="csharp">GetPropertyType()</code> | <code lang="csharp">GetFieldInfo()</code> | <code lang="csharp">GetDeclaringInstance()</code> |
|---|---|---|---|
| <code lang="csharp">abilities</code> | <code lang="class-name">List&lt;Ability&gt;</code> | <code lang="csharp">AbilityBook._abilities</code> | the <code lang="class-name">AbilityBook</code> |
| <code lang="csharp">ability</code> | <code lang="class-name">Ability</code> | <code lang="csharp">AbilityBook._abilities</code> | the <code lang="class-name">AbilityBook</code> |
| <code lang="csharp">abilityName</code> | <code lang="csharp">string</code> | <code lang="csharp">Ability.Name</code> | the <code lang="class-name">Ability</code> at index 0 |
| <code lang="csharp">effect</code> | <code lang="class-name">IAbilityEffect</code> | <code lang="csharp">AbilityBook._effect</code> | the <code lang="class-name">AbilityBook</code> |
| <code lang="csharp">effectDamage</code> | <code lang="csharp">float</code> | <code lang="csharp">BurnEffect.Damage</code> | the <code lang="class-name">BurnEffect</code> instance |

When the field cannot be resolved, all three methods return <code lang="csharp">null</code>. They read only the first target object and only applied values.

> [!WARNING]
> When the field's owner is a struct, <code lang="csharp">GetDeclaringInstance()</code> returns a boxed copy. Changes to that copy never reach the original; write values through the <code lang="class-name">SerializedProperty</code> instead.

## Member name and drawing checks

| Call | Result |
|---|---|
| <code lang="csharp">ability.GetMemberName()</code> | <code lang="csharp">"_abilities"</code> |
| <code lang="csharp">abilityName.GetMemberName()</code> | <code lang="csharp">"Name"</code> |
| <code lang="csharp">ability.IsArrayElement()</code> | <code lang="csharp">true</code> |
| <code lang="csharp">abilityName.IsArrayElement()</code> | <code lang="csharp">false</code> |
| <code lang="csharp">ability.HasFoldout()</code> | <code lang="csharp">true</code> |
| <code lang="csharp">effect.HasFoldout()</code> | <code lang="csharp">false</code> |

> [!NOTE]
> <code lang="csharp">HasFoldout()</code> does not mirror the Inspector: it returns <code lang="csharp">false</code> for <code lang="csharp">[SerializeReference]</code>, which the Inspector draws with a foldout, and ignores custom <code lang="class-name">PropertyDrawer</code>s.

## Persistent()

An inspector's <code lang="class-name">SerializedObject</code> lives only while the inspector is open, so its properties cannot be kept for a deferred call. <code lang="csharp">Persistent()</code> returns the same property on a new <code lang="class-name">SerializedObject</code> for the same target objects:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>var independentObject =&#10;    new SerializedObject(manaCost&#10;        .serializedObject.targetObjects);&#10;var independent = independentObject&#10;    .FindProperty(manaCost.propertyPath);</code></pre> | <pre lang="csharp"><code>var independent = manaCost.Persistent();</code></pre> |

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

<code lang="csharp">Persistent()</code> returns <code lang="csharp">null</code> when the property path no longer exists. Pending writes on the source object are not copied.

## Package sample

In [EditorTools](../Samples~/EditorTools/Documentation/README.md), the **Halve cooldown, +5 MP** button writes two properties with <code lang="function">SetFloat</code> and <code lang="function">SetIntAndApply</code>.

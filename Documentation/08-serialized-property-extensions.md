# SerializedProperty Extensions

Write editor properties in one line — and reach the actual C# field the property shows in the Inspector.

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

### Supported types

| Values | Setters |
|---|---|
| Numbers | <code lang="function">SetInt</code>, <code lang="function">SetUint</code>, <code lang="function">SetLong</code>, <code lang="function">SetUlong</code>, <code lang="function">SetFloat</code>, <code lang="function">SetDouble</code> |
| Text, bool and hash | <code lang="function">SetString</code>, <code lang="function">SetBool</code>, <code lang="function">SetHash128</code> |
| Vectors | <code lang="function">SetVector2</code>, <code lang="function">SetVector2Int</code>, <code lang="function">SetVector3</code>, <code lang="function">SetVector3Int</code>, <code lang="function">SetVector4</code>, <code lang="function">SetQuaternion</code> |
| Areas | <code lang="function">SetRect</code>, <code lang="function">SetRectInt</code>, <code lang="function">SetBounds</code>, <code lang="function">SetBoundsInt</code> |
| Unity types | <code lang="function">SetColor</code>, <code lang="function">SetGradient</code>, <code lang="function">SetAnimationCurve</code> |
| Object IDs | <code lang="function">SetEntityId</code> |

Each has a <code lang="function">SetValue</code> overload: <code lang="csharp">SetValue(42)</code> calls <code lang="function">SetInt</code>, so the argument type must match the field type.

### Enums, arrays and references

| Unity | FastTools |
|---|---|
| <code lang="csharp">enumValueIndex = 1</code> | <code lang="csharp">SetEnumIndex(1)</code> |
| <code lang="csharp">enumValueFlag = flags</code> | <code lang="csharp">SetEnumFlag(flags)</code> |
| <code lang="csharp">arraySize = 5</code> | <code lang="csharp">SetArraySize(5)</code> |
| <code lang="csharp">arraySize += n</code> | <code lang="csharp">AddArraySize(n = 1)</code> |
| <code lang="csharp">arraySize -= n</code> | <code lang="csharp">RemoveArraySize(n = 1)</code> |
| <code lang="csharp">managedReferenceValue = value</code> | <code lang="csharp">SetManagedReference(value)</code> |
| <code lang="csharp">objectReferenceValue = value</code> | <code lang="csharp">SetObjectReference(value)</code> |
| <code lang="csharp">boxedValue = value</code> | <code lang="csharp">SetBoxed(value)</code> |
| <code lang="csharp">exposedReferenceValue = value</code> | <code lang="csharp">SetExposedReference(value)</code> |

### Update() and Apply…()

| Unity | FastTools |
|---|---|
| <code lang="csharp">serializedObject.Update()</code> | <code lang="csharp">Update()</code> |
| <code lang="csharp">serializedObject.UpdateIfRequiredOrScript()</code> | <code lang="csharp">UpdateIfRequiredOrScript()</code> |
| <code lang="csharp">serializedObject.ApplyModifiedProperties()</code> | <code lang="csharp">ApplyModifiedProperties()</code> |
| <code lang="csharp">serializedObject.ApplyModifiedPropertiesWithoutUndo()</code> | <code lang="csharp">ApplyModifiedPropertiesWithoutUndo()</code> |

They are called on the property and, like every setter, return it.

## The C# field from the Inspector

Three methods find the C# field the property shows in the Inspector: its type, the field itself and the object that holds it.

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

| Property path | <code lang="csharp">GetPropertyType()</code> | <code lang="csharp">GetFieldInfo()</code> | <code lang="csharp">GetDeclaringInstance()</code> |
|---|---|---|---|
| <code lang="csharp">_abilities</code> | <code lang="class-name">List&lt;Ability&gt;</code> | <code lang="csharp">AbilityBook._abilities</code> | the <code lang="class-name">AbilityBook</code> |
| <code lang="csharp">_abilities[0]</code> | <code lang="class-name">Ability</code> | <code lang="csharp">AbilityBook._abilities</code> | the <code lang="class-name">AbilityBook</code> |
| <code lang="csharp">_abilities[0].Name</code> | <code lang="csharp">string</code> | <code lang="csharp">Ability.Name</code> | the <code lang="class-name">Ability</code> at index 0 |
| <code lang="csharp">_effect</code> | <code lang="class-name">IAbilityEffect</code> | <code lang="csharp">AbilityBook._effect</code> | the <code lang="class-name">AbilityBook</code> |
| <code lang="csharp">_effect.Damage</code> | <code lang="csharp">float</code> | <code lang="csharp">BurnEffect.Damage</code> | the <code lang="class-name">BurnEffect</code> instance |

- if the field is not found, <code lang="csharp">GetPropertyType()</code> and <code lang="csharp">GetFieldInfo()</code> return <code lang="csharp">null</code>, and <code lang="csharp">GetDeclaringInstance()</code> does only when the owning object is not found;
- the methods read only the first target object and only applied values.

> [!WARNING]
> When the field's owner is a struct, <code lang="csharp">GetDeclaringInstance()</code> returns a boxed copy. Changes to that copy never reach the original; write values through the <code lang="class-name">SerializedProperty</code> instead.

## Member name and drawing checks

| Property path | <code lang="csharp">GetMemberName()</code> | <code lang="csharp">IsArrayElement()</code> | <code lang="csharp">HasFoldout()</code> |
|---|---|---|---|
| <code lang="csharp">_abilities[0]</code> | <code lang="csharp">"_abilities"</code> | <code lang="csharp">true</code> | <code lang="csharp">true</code> |
| <code lang="csharp">_abilities[0].Name</code> | <code lang="csharp">"Name"</code> | <code lang="csharp">false</code> | <code lang="csharp">false</code> |
| <code lang="csharp">_effect</code> | <code lang="csharp">"_effect"</code> | <code lang="csharp">false</code> | <code lang="csharp">false</code> |

> [!NOTE]
> <code lang="csharp">HasFoldout()</code> returns <code lang="csharp">true</code> for a plain compound field with visible children, so it can differ from the Inspector: a <code lang="csharp">[SerializeReference]</code> field gets a foldout in the Inspector but <code lang="csharp">false</code> here, and custom <code lang="class-name">PropertyDrawer</code>s are not taken into account.

## Persistent()

<code lang="csharp">Persistent()</code> returns a copy of the property on its own <code lang="class-name">SerializedObject</code>, so it can be used later, for example in <code lang="csharp">EditorApplication.delayCall</code>.

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>var independentObject =&#10;    new SerializedObject(manaCost&#10;        .serializedObject.targetObjects);&#10;var independent = independentObject&#10;    .FindProperty(manaCost.propertyPath);</code></pre> | <pre lang="csharp"><code>var independent = manaCost.Persistent();</code></pre> |

- the copy belongs to the caller: dispose it together with its <code lang="csharp">serializedObject</code>;
- unapplied writes of the source property do not reach the copy;
- if the property is missing on the targets, the method returns <code lang="csharp">null</code>.

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

## Package sample

In [EditorTools](../Samples~/EditorTools/Documentation/README.md), the **Halve cooldown, +5 MP** button writes two properties with <code lang="function">SetFloat</code> and <code lang="function">SetIntAndApply</code>.

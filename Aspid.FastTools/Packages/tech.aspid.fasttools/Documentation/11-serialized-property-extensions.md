# SerializedProperty Extensions

Write values and access C# fields through SerializedProperty.

## Quick start

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>serializedObject.Update();&#10;manaCost.intValue = 42;&#10;serializedObject&#10;    .ApplyModifiedProperties();</code></pre> | <pre lang="csharp"><code>manaCost&#10;    .Update()&#10;    .SetIntAndApply(42);</code></pre> |

## Writing values

| Call | What it does |
|---|---|
| <code lang="csharp">SetInt(42)</code> | Writes the value |
| <code lang="csharp">SetIntAndApply(42)</code> | Writes the value and applies changes |
| <code lang="csharp">SetIntAndApplyWithoutUndo(42)</code> | Writes the value and applies changes without Undo |

The <code lang="function">SetValue</code> overloads select a setter by argument type: <code lang="csharp">SetValue(42)</code> calls <code lang="function">SetInt</code>. The argument type must match the field type. See the [API reference](https://vpdpersonal.github.io/Aspid.FastTools/api/Aspid.FastTools.Editors.SerializePropertyExtensions) for the full list of setters.

### Enums, arrays and references

| Unity | FastTools |
|---|---|
| <code lang="csharp">enumValueIndex = 1</code> | <code lang="csharp">SetEnumIndex(1)</code> |
| <code lang="csharp">enumValueFlag = flags</code> | <code lang="csharp">SetEnumFlag(flags)</code> |
| <code lang="csharp">arraySize = 5</code> | <code lang="csharp">SetArraySize(5)</code> |
| <code lang="csharp">arraySize++</code> | <code lang="csharp">AddArraySize()</code> |
| <code lang="csharp">arraySize--</code> | <code lang="csharp">RemoveArraySize()</code> |
| <code lang="csharp">managedReferenceValue = value</code> | <code lang="csharp">SetManagedReference(value)</code> |
| <code lang="csharp">objectReferenceValue = value</code> | <code lang="csharp">SetObjectReference(value)</code> |
| <code lang="csharp">boxedValue = value</code> | <code lang="csharp">SetBoxed(value)</code> |
| <code lang="csharp">exposedReferenceValue = value</code> | <code lang="csharp">SetExposedReference(value)</code> |

### Update() and Apply…()

These methods are called on the property and, like setters, return it for chaining.

| Unity | FastTools |
|---|---|
| <code lang="csharp">serializedObject.Update()</code> | <code lang="csharp">Update()</code> |
| <code lang="csharp">serializedObject.UpdateIfRequiredOrScript()</code> | <code lang="csharp">UpdateIfRequiredOrScript()</code> |
| <code lang="csharp">serializedObject.ApplyModifiedProperties()</code> | <code lang="csharp">ApplyModifiedProperties()</code> |
| <code lang="csharp">serializedObject.ApplyModifiedPropertiesWithoutUndo()</code> | <code lang="csharp">ApplyModifiedPropertiesWithoutUndo()</code> |

## Accessing the C# field

The methods return the field type, its <code lang="class-name">FieldInfo</code> and the owning object.

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

| Field / element | <code lang="csharp">GetPropertyType()</code> | <code lang="csharp">GetFieldInfo()</code> | <code lang="csharp">GetDeclaringInstance()</code> |
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

| Field / element | <code lang="csharp">GetMemberName()</code> | <code lang="csharp">IsArrayElement()</code> | <code lang="csharp">HasFoldout()</code> |
|---|---|---|---|
| <code lang="csharp">_abilities[0]</code> | <code lang="csharp">"_abilities"</code> | <code lang="csharp">true</code> | <code lang="csharp">true</code> |
| <code lang="csharp">_abilities[0].Name</code> | <code lang="csharp">"Name"</code> | <code lang="csharp">false</code> | <code lang="csharp">false</code> |
| <code lang="csharp">_effect</code> | <code lang="csharp">"_effect"</code> | <code lang="csharp">false</code> | <code lang="csharp">false</code> |

> [!NOTE]
> <code lang="csharp">HasFoldout()</code> checks ordinary compound fields with visible children. It does not account for <code lang="csharp">[SerializeReference]</code> or custom <code lang="class-name">PropertyDrawer</code>s.

## Persistent()

<code lang="csharp">Persistent()</code> returns a copy of the property on its own <code lang="class-name">SerializedObject</code>, so it can be used later, for example in <code lang="csharp">EditorApplication.delayCall</code>.

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

![The Halve cooldown, +5 MP button changes the cost and the cooldown, and Undo restores them.](../Samples~/EditorTools/Documentation/Images/demo.gif)

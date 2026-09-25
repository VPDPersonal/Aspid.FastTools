# SerializedProperty Extensions

Запись полей из редактора одной строкой с Undo — и тип, поле и объект C#, которые стоят за свойством.

## Быстрый старт

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>var manaCost = serializedObject&#10;    .FindProperty("_manaCost");&#10;&#10;serializedObject.Update();&#10;manaCost.intValue = 42;&#10;serializedObject&#10;    .ApplyModifiedProperties();</code></pre> | <pre lang="csharp"><code>var manaCost = serializedObject&#10;    .FindProperty("_manaCost");&#10;&#10;manaCost&#10;    .Update()&#10;    .SetIntAndApply(42);</code></pre> |

## Запись значений

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>manaCost.intValue = 42;</code></pre> | <pre lang="csharp"><code>manaCost.SetInt(42);</code></pre> |
| <pre lang="csharp"><code>manaCost.intValue = 42;&#10;manaCost.serializedObject&#10;    .ApplyModifiedProperties();</code></pre> | <pre lang="csharp"><code>manaCost.SetIntAndApply(42);</code></pre> |
| <pre lang="csharp"><code>manaCost.intValue = 42;&#10;manaCost.serializedObject&#10;    .ApplyModifiedPropertiesWithoutUndo();</code></pre> | <pre lang="csharp"><code>manaCost&#10;    .SetIntAndApplyWithoutUndo(42);</code></pre> |

Каждый сеттер на этой странице, кроме <code lang="function">SetExposedReference</code>, есть в этих трёх формах.

<code lang="csharp">SetValue(42)</code> — то же, что <code lang="csharp">SetInt(42)</code>: перегрузка выбирается **по типу аргумента**, который должен совпадать с типом поля.

### Поддерживаемые типы

У каждого из этих сеттеров есть перегрузка <code lang="function">SetValue</code>:

| Значения | Сеттеры |
|---|---|
| Числа | <code lang="function">SetInt</code>, <code lang="function">SetUint</code>, <code lang="function">SetLong</code>, <code lang="function">SetUlong</code>, <code lang="function">SetFloat</code>, <code lang="function">SetDouble</code> |
| Текст, bool и хэш | <code lang="function">SetString</code>, <code lang="function">SetBool</code>, <code lang="function">SetHash128</code> |
| Векторы | <code lang="function">SetVector2</code>, <code lang="function">SetVector2Int</code>, <code lang="function">SetVector3</code>, <code lang="function">SetVector3Int</code>, <code lang="function">SetVector4</code>, <code lang="function">SetQuaternion</code> |
| Области | <code lang="function">SetRect</code>, <code lang="function">SetRectInt</code>, <code lang="function">SetBounds</code>, <code lang="function">SetBoundsInt</code> |
| Типы Unity | <code lang="function">SetColor</code>, <code lang="function">SetGradient</code>, <code lang="function">SetAnimationCurve</code> |
| Идентификаторы объектов | <code lang="function">SetEntityId</code> |

### Перечисления

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>targeting.enumValueIndex = 1;</code></pre> | <pre lang="csharp"><code>targeting.SetEnumIndex(1);</code></pre> |
| <pre lang="csharp"><code>damageTypes.enumValueFlag = (int)&#10;    (DamageTypes.Fire &#124; DamageTypes.Ice);</code></pre> | <pre lang="csharp"><code>damageTypes.SetEnumFlag((int)&#10;    (DamageTypes.Fire &#124; DamageTypes.Ice));</code></pre> |

### Массивы и списки

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>abilities.arraySize = 5;&#10;abilities.arraySize += 1;&#10;abilities.arraySize += 2;&#10;abilities.arraySize -= 2;&#10;abilities.arraySize -= 1;</code></pre> | <pre lang="csharp"><code>abilities.SetArraySize(5);&#10;abilities.AddArraySize();&#10;abilities.AddArraySize(2);&#10;abilities.RemoveArraySize(2);&#10;abilities.RemoveArraySize();</code></pre> |

### Ссылки и boxed-значения

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>effect.managedReferenceValue = instance;</code></pre> | <pre lang="csharp"><code>effect.SetManagedReference(instance);</code></pre> |
| <pre lang="csharp"><code>icon.objectReferenceValue = sprite;</code></pre> | <pre lang="csharp"><code>icon.SetObjectReference(sprite);</code></pre> |
| <pre lang="csharp"><code>ability.boxedValue =&#10;    new Ability &#123; Name = "Frostbolt" &#125;;</code></pre> | <pre lang="csharp"><code>ability.SetBoxed(&#10;    new Ability &#123; Name = "Frostbolt" &#125;);</code></pre> |
| <pre lang="csharp"><code>target.exposedReferenceValue = light;</code></pre> | <pre lang="csharp"><code>target.SetExposedReference(light);</code></pre> |

### Update() и Apply…()

<code lang="csharp">Update()</code>, <code lang="csharp">UpdateIfRequiredOrScript()</code>, <code lang="csharp">ApplyModifiedProperties()</code> и <code lang="csharp">ApplyModifiedPropertiesWithoutUndo()</code> вызывают одноимённые методы <code lang="csharp">serializedObject</code> и, как все сеттеры, возвращают свойство.

## Поле C# за свойством

Три метода через рефлексию находят поле C#, стоящее за свойством:

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

| Свойство | <code lang="csharp">GetPropertyType()</code> | <code lang="csharp">GetFieldInfo()</code> | <code lang="csharp">GetDeclaringInstance()</code> |
|---|---|---|---|
| <code lang="csharp">abilities</code> | <code lang="class-name">List&lt;Ability&gt;</code> | <code lang="csharp">AbilityBook._abilities</code> | экземпляр <code lang="class-name">AbilityBook</code> |
| <code lang="csharp">ability</code> | <code lang="class-name">Ability</code> | <code lang="csharp">AbilityBook._abilities</code> | экземпляр <code lang="class-name">AbilityBook</code> |
| <code lang="csharp">abilityName</code> | <code lang="csharp">string</code> | <code lang="csharp">Ability.Name</code> | <code lang="class-name">Ability</code> с индексом 0 |
| <code lang="csharp">effect</code> | <code lang="class-name">IAbilityEffect</code> | <code lang="csharp">AbilityBook._effect</code> | экземпляр <code lang="class-name">AbilityBook</code> |
| <code lang="csharp">effectDamage</code> | <code lang="csharp">float</code> | <code lang="csharp">BurnEffect.Damage</code> | экземпляр <code lang="class-name">BurnEffect</code> |

Если поле не найдено, все три метода возвращают <code lang="csharp">null</code>. Они читают только первый целевой объект и только применённые значения.

> [!WARNING]
> Если владелец поля — структура, <code lang="csharp">GetDeclaringInstance()</code> возвращает её boxed-копию. Изменения такой копии не попадают в оригинал; записывайте значения через <code lang="class-name">SerializedProperty</code>.

## Имя поля и проверки для отрисовки

| Вызов | Результат |
|---|---|
| <code lang="csharp">ability.GetMemberName()</code> | <code lang="csharp">"_abilities"</code> |
| <code lang="csharp">abilityName.GetMemberName()</code> | <code lang="csharp">"Name"</code> |
| <code lang="csharp">ability.IsArrayElement()</code> | <code lang="csharp">true</code> |
| <code lang="csharp">abilityName.IsArrayElement()</code> | <code lang="csharp">false</code> |
| <code lang="csharp">ability.HasFoldout()</code> | <code lang="csharp">true</code> |
| <code lang="csharp">effect.HasFoldout()</code> | <code lang="csharp">false</code> |

> [!NOTE]
> <code lang="csharp">HasFoldout()</code> не повторяет инспектор: для <code lang="csharp">[SerializeReference]</code> возвращает <code lang="csharp">false</code>, хотя инспектор рисует foldout, и не учитывает кастомные <code lang="class-name">PropertyDrawer</code>.

## Persistent()

<code lang="csharp">Persistent()</code> возвращает то же свойство на новом <code lang="class-name">SerializedObject</code>, которое можно сохранить для отложенного вызова, даже когда инспектор уже закрыт:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>var independentObject =&#10;    new SerializedObject(manaCost&#10;        .serializedObject.targetObjects);&#10;var independent = independentObject&#10;    .FindProperty(manaCost.propertyPath);</code></pre> | <pre lang="csharp"><code>var independent = manaCost.Persistent();</code></pre> |

Новый объект принадлежит вызывающему коду, а неприменённые записи исходного в него не попадают:

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

## Пример в пакете

В [EditorTools](../../Samples~/EditorTools/Documentation/README.ru.md) кнопка **Halve cooldown, +5 MP** записывает два свойства через <code lang="function">SetFloat</code> и <code lang="function">SetIntAndApply</code>.

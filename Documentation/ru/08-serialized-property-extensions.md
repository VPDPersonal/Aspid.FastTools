# SerializedProperty Extensions

Запись свойств в редакторе одной строкой — и доступ к настоящему полю C#, которое свойство показывает в инспекторе.

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

### Поддерживаемые типы

| Значения | Сеттеры |
|---|---|
| Числа | <code lang="function">SetInt</code>, <code lang="function">SetUint</code>, <code lang="function">SetLong</code>, <code lang="function">SetUlong</code>, <code lang="function">SetFloat</code>, <code lang="function">SetDouble</code> |
| Текст, bool и хэш | <code lang="function">SetString</code>, <code lang="function">SetBool</code>, <code lang="function">SetHash128</code> |
| Векторы | <code lang="function">SetVector2</code>, <code lang="function">SetVector2Int</code>, <code lang="function">SetVector3</code>, <code lang="function">SetVector3Int</code>, <code lang="function">SetVector4</code>, <code lang="function">SetQuaternion</code> |
| Области | <code lang="function">SetRect</code>, <code lang="function">SetRectInt</code>, <code lang="function">SetBounds</code>, <code lang="function">SetBoundsInt</code> |
| Типы Unity | <code lang="function">SetColor</code>, <code lang="function">SetGradient</code>, <code lang="function">SetAnimationCurve</code> |
| Идентификаторы объектов | <code lang="function">SetEntityId</code> |

У каждого есть перегрузка <code lang="function">SetValue</code>: <code lang="csharp">SetValue(42)</code> вызывает <code lang="function">SetInt</code>, поэтому тип аргумента должен совпадать с типом поля.

### Перечисления, массивы и ссылки

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

### Update() и Apply…()

| Unity | FastTools |
|---|---|
| <code lang="csharp">serializedObject.Update()</code> | <code lang="csharp">Update()</code> |
| <code lang="csharp">serializedObject.UpdateIfRequiredOrScript()</code> | <code lang="csharp">UpdateIfRequiredOrScript()</code> |
| <code lang="csharp">serializedObject.ApplyModifiedProperties()</code> | <code lang="csharp">ApplyModifiedProperties()</code> |
| <code lang="csharp">serializedObject.ApplyModifiedPropertiesWithoutUndo()</code> | <code lang="csharp">ApplyModifiedPropertiesWithoutUndo()</code> |

Методы вызываются прямо на свойстве и, как все сеттеры, возвращают его.

## Поле C# из инспектора

Три метода находят поле C#, которое свойство показывает в инспекторе: его тип, само поле и объект, в котором оно лежит.

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

| Путь свойства | <code lang="csharp">GetPropertyType()</code> | <code lang="csharp">GetFieldInfo()</code> | <code lang="csharp">GetDeclaringInstance()</code> |
|---|---|---|---|
| <code lang="csharp">_abilities</code> | <code lang="class-name">List&lt;Ability&gt;</code> | <code lang="csharp">AbilityBook._abilities</code> | экземпляр <code lang="class-name">AbilityBook</code> |
| <code lang="csharp">_abilities[0]</code> | <code lang="class-name">Ability</code> | <code lang="csharp">AbilityBook._abilities</code> | экземпляр <code lang="class-name">AbilityBook</code> |
| <code lang="csharp">_abilities[0].Name</code> | <code lang="csharp">string</code> | <code lang="csharp">Ability.Name</code> | <code lang="class-name">Ability</code> с индексом 0 |
| <code lang="csharp">_effect</code> | <code lang="class-name">IAbilityEffect</code> | <code lang="csharp">AbilityBook._effect</code> | экземпляр <code lang="class-name">AbilityBook</code> |
| <code lang="csharp">_effect.Damage</code> | <code lang="csharp">float</code> | <code lang="csharp">BurnEffect.Damage</code> | экземпляр <code lang="class-name">BurnEffect</code> |

- если поле не найдено, <code lang="csharp">GetPropertyType()</code> и <code lang="csharp">GetFieldInfo()</code> возвращают <code lang="csharp">null</code>, а <code lang="csharp">GetDeclaringInstance()</code> — только когда не найден объект-владелец;
- методы читают только первый целевой объект и только применённые значения.

> [!WARNING]
> Если владелец поля — структура, <code lang="csharp">GetDeclaringInstance()</code> возвращает её boxed-копию. Изменения такой копии не попадают в оригинал; записывайте значения через <code lang="class-name">SerializedProperty</code>.

## Имя поля и проверки для отрисовки

| Путь свойства | <code lang="csharp">GetMemberName()</code> | <code lang="csharp">IsArrayElement()</code> | <code lang="csharp">HasFoldout()</code> |
|---|---|---|---|
| <code lang="csharp">_abilities[0]</code> | <code lang="csharp">"_abilities"</code> | <code lang="csharp">true</code> | <code lang="csharp">true</code> |
| <code lang="csharp">_abilities[0].Name</code> | <code lang="csharp">"Name"</code> | <code lang="csharp">false</code> | <code lang="csharp">false</code> |
| <code lang="csharp">_effect</code> | <code lang="csharp">"_effect"</code> | <code lang="csharp">false</code> | <code lang="csharp">false</code> |

> [!NOTE]
> <code lang="csharp">HasFoldout()</code> возвращает <code lang="csharp">true</code> для обычного составного поля с видимыми вложенными полями, поэтому может не совпадать с инспектором: у поля <code lang="csharp">[SerializeReference]</code> инспектор рисует foldout, а метод возвращает <code lang="csharp">false</code>; свои <code lang="class-name">PropertyDrawer</code> метод не учитывает.

## Persistent()

<code lang="csharp">Persistent()</code> возвращает копию свойства на собственном <code lang="class-name">SerializedObject</code>: её можно использовать позже, например в <code lang="csharp">EditorApplication.delayCall</code>.

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>var source = manaCost.serializedObject;&#10;var independentObject = new SerializedObject(&#10;    source.targetObjects, source.context);&#10;var independent = independentObject&#10;    .FindProperty(manaCost.propertyPath);</code></pre> | <pre lang="csharp"><code>var independent = manaCost.Persistent();</code></pre> |

- копия принадлежит вызывающему коду: освободите её вместе с её <code lang="csharp">serializedObject</code>;
- неприменённые записи исходного свойства в копию не попадают;
- если свойства на объектах нет, метод возвращает <code lang="csharp">null</code>.

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

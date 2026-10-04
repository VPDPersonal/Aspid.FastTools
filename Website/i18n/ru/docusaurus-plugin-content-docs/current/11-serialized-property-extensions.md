# SerializedProperty Extensions

Запись значений и доступ к полям C# через SerializedProperty.

## Быстрый старт

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>serializedObject.Update();&#10;manaCost.intValue = 42;&#10;serializedObject&#10;    .ApplyModifiedProperties();</code></pre> | <pre lang="csharp"><code>manaCost&#10;    .Update()&#10;    .SetIntAndApply(42);</code></pre> |

## Запись значений

| Вызов | Что делает |
|---|---|
| <code lang="csharp">SetInt(42)</code> | Записывает значение |
| <code lang="csharp">SetIntAndApply(42)</code> | Записывает и применяет изменения |
| <code lang="csharp">SetIntAndApplyWithoutUndo(42)</code> | Записывает и применяет изменения без Undo |

Перегрузки <code lang="function">SetValue</code> выбирают сеттер по типу аргумента: <code lang="csharp">SetValue(42)</code> вызывает <code lang="function">SetInt</code>. Тип аргумента должен совпадать с типом поля. Полный список сеттеров — в [справочнике API](https://vpdpersonal.github.io/Aspid.FastTools/ru/api/Aspid.FastTools.Editors.SerializePropertyExtensions).

### Перечисления, массивы и ссылки

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

### Update() и Apply…()

Методы вызываются на свойстве и, как сеттеры, возвращают его для цепочек вызовов.

| Unity | FastTools |
|---|---|
| <code lang="csharp">serializedObject.Update()</code> | <code lang="csharp">Update()</code> |
| <code lang="csharp">serializedObject.UpdateIfRequiredOrScript()</code> | <code lang="csharp">UpdateIfRequiredOrScript()</code> |
| <code lang="csharp">serializedObject.ApplyModifiedProperties()</code> | <code lang="csharp">ApplyModifiedProperties()</code> |
| <code lang="csharp">serializedObject.ApplyModifiedPropertiesWithoutUndo()</code> | <code lang="csharp">ApplyModifiedPropertiesWithoutUndo()</code> |

## Доступ к полю C#

Методы возвращают тип поля, его <code lang="class-name">FieldInfo</code> и объект-владелец.

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

| Поле / элемент | <code lang="csharp">GetPropertyType()</code> | <code lang="csharp">GetFieldInfo()</code> | <code lang="csharp">GetDeclaringInstance()</code> |
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

| Поле / элемент | <code lang="csharp">GetMemberName()</code> | <code lang="csharp">IsArrayElement()</code> | <code lang="csharp">HasFoldout()</code> |
|---|---|---|---|
| <code lang="csharp">_abilities[0]</code> | <code lang="csharp">"_abilities"</code> | <code lang="csharp">true</code> | <code lang="csharp">true</code> |
| <code lang="csharp">_abilities[0].Name</code> | <code lang="csharp">"Name"</code> | <code lang="csharp">false</code> | <code lang="csharp">false</code> |
| <code lang="csharp">_effect</code> | <code lang="csharp">"_effect"</code> | <code lang="csharp">false</code> | <code lang="csharp">false</code> |

> [!NOTE]
> <code lang="csharp">HasFoldout()</code> проверяет обычные составные поля с видимыми вложенными полями. Не учитывает <code lang="csharp">[SerializeReference]</code> и пользовательские <code lang="class-name">PropertyDrawer</code>.

## Persistent()

<code lang="csharp">Persistent()</code> возвращает копию свойства на собственном <code lang="class-name">SerializedObject</code>: её можно использовать позже, например в <code lang="csharp">EditorApplication.delayCall</code>.

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

![Кнопка Halve cooldown, +5 MP меняет стоимость и перезарядку, а Undo возвращает прежние значения.](../../Samples~/EditorTools/Documentation/Images/demo.gif)

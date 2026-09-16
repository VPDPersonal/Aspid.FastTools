# SerializedProperty Extensions

Расширения `SerializedProperty` для записи значений, изменения размера коллекций и поиска типа поля и его владельца.

## Быстрый старт

Примеры на этой странице работают с компонентом `AbilityBook.cs`:

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

В его пользовательском `Editor` добавьте `using Aspid.FastTools.Editors;` и запишите значение поля:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>var manaCost = serializedObject&#10;    .FindProperty("_manaCost");&#10;&#10;serializedObject.Update();&#10;manaCost.intValue = 42;&#10;serializedObject&#10;    .ApplyModifiedProperties();</code></pre> | <pre lang="csharp"><code>var manaCost = serializedObject&#10;    .FindProperty("_manaCost");&#10;&#10;manaCost&#10;    .Update()&#10;    .SetIntAndApply(42);</code></pre> |

Сеттеры, `Update()` и оба `Apply…()` возвращают исходное свойство, поэтому вызовы выстраиваются в цепочку. Суффикс `AndApply` применяет запись с Undo.

**Несколько полей:** обновите объект один раз, запишите значения и примените их вместе:

```csharp
serializedObject.Update();
serializedObject.FindProperty("_cooldown").SetFloat(0.5f);
serializedObject.FindProperty("_manaCost").SetIntAndApply(10);
```

Рабочий пример кнопки есть в [EditorTools](../../Samples~/EditorTools/Documentation/README.ru.md).

> [!IMPORTANT]
> Применение затрагивает **все накопленные изменения** связанного `SerializedObject`.
> `Update()` сбрасывает неприменённые записи — вызывайте его до изменения полей.

## Обновление и применение

Те же операции Unity, но с вызовом на свойстве:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>property.serializedObject&#10;    .Update();</code></pre> | <pre lang="csharp"><code>property.Update();</code></pre> |
| <pre lang="csharp"><code>property.serializedObject&#10;    .UpdateIfRequiredOrScript();</code></pre> | <pre lang="csharp"><code>property&#10;    .UpdateIfRequiredOrScript();</code></pre> |
| <pre lang="csharp"><code>property.serializedObject&#10;    .ApplyModifiedProperties();</code></pre> | <pre lang="csharp"><code>property&#10;    .ApplyModifiedProperties();</code></pre> |
| <pre lang="csharp"><code>property.serializedObject&#10;    .ApplyModifiedPropertiesWithoutUndo();</code></pre> | <pre lang="csharp"><code>property&#10;    .ApplyModifiedPropertiesWithoutUndo();</code></pre> |

## Запись значений

Выберите, когда применять запись:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>// Применить позже&#10;manaCost.intValue = 42;</code></pre> | <pre lang="csharp"><code>// Применить позже&#10;manaCost.SetInt(42);</code></pre> |
| <pre lang="csharp"><code>// С Undo&#10;manaCost.intValue = 42;&#10;manaCost.serializedObject&#10;    .ApplyModifiedProperties();</code></pre> | <pre lang="csharp"><code>// С Undo&#10;manaCost.SetIntAndApply(42);</code></pre> |
| <pre lang="csharp"><code>// Без Undo&#10;manaCost.intValue = 42;&#10;manaCost.serializedObject&#10;    .ApplyModifiedPropertiesWithoutUndo();</code></pre> | <pre lang="csharp"><code>// Без Undo&#10;manaCost&#10;    .SetIntAndApplyWithoutUndo(42);</code></pre> |

Вместо явного сеттера можно использовать `SetValue`: `SetValue(42)` эквивалентен `SetInt(42)`, а `SetValue(0.5f)` — `SetFloat(0.5f)`. Перегрузка выбирается **по типу аргумента**, который должен соответствовать типу поля.

Суффиксы `AndApply` и `AndApplyWithoutUndo` доступны для всех сеттеров ниже, включая `SetValue`, enum, коллекции и ссылки.

### Поддерживаемые типы

Явный сеттер и перегрузка `SetValue` есть для каждого типа значения `SerializedProperty`: `SetInt`, `SetUint`, `SetLong`, `SetUlong`, `SetFloat`, `SetDouble`, `SetBool`, `SetString`, `SetColor`, `SetGradient`, `SetHash128`, `SetRect`, `SetRectInt`, `SetBounds`, `SetBoundsInt`, `SetVector2`, `SetVector2Int`, `SetVector3`, `SetVector3Int`, `SetVector4`, `SetQuaternion` и `SetAnimationCurve`. `SetEntityId` для `UnityEngine.EntityId` доступен только в Unity 6.2 и новее.

### Перечисления

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>// Второе значение enum&#10;mode.enumValueIndex = 1;</code></pre> | <pre lang="csharp"><code>// Второе значение enum&#10;mode.SetEnumIndex(1);</code></pre> |
| <pre lang="csharp"><code>// Битовая маска [Flags]&#10;flags.enumValueFlag = mask;</code></pre> | <pre lang="csharp"><code>// Битовая маска [Flags]&#10;flags.SetEnumFlag(mask);</code></pre> |

### Массивы и списки

Для свойства коллекции `_abilities`:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>abilities.arraySize = 5;&#10;abilities.arraySize += 1;&#10;abilities.arraySize += 2;&#10;abilities.arraySize -= 2;&#10;abilities.arraySize -= 1;</code></pre> | <pre lang="csharp"><code>abilities.SetArraySize(5);&#10;abilities.AddArraySize();     // +1&#10;abilities.AddArraySize(2);    // +2&#10;abilities.RemoveArraySize(2); // -2&#10;abilities.RemoveArraySize();  // -1</code></pre> |

Методы меняют только размер коллекции. `RemoveArraySize` убирает элементы с конца; новые элементы инициализируйте отдельно через `GetArrayElementAtIndex()`.

### Ссылки и boxed-значения

Выберите сеттер по способу сериализации поля:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>// [SerializeReference]&#10;property.managedReferenceValue = instance;</code></pre> | <pre lang="csharp"><code>// [SerializeReference]&#10;property.SetManagedReference(instance);</code></pre> |
| <pre lang="csharp"><code>// UnityEngine.Object&#10;property.objectReferenceValue = asset;</code></pre> | <pre lang="csharp"><code>// UnityEngine.Object&#10;property.SetObjectReference(asset);</code></pre> |
| <pre lang="csharp"><code>// ExposedReference&lt;T&gt;&#10;property.exposedReferenceValue = target;</code></pre> | <pre lang="csharp"><code>// ExposedReference&lt;T&gt;&#10;property.SetExposedReference(target);</code></pre> |
| <pre lang="csharp"><code>// boxedValue&#10;property.boxedValue = value;</code></pre> | <pre lang="csharp"><code>// boxedValue&#10;property.SetBoxed(value);</code></pre> |

## Тип поля и объект-владелец

Для `AbilityBook` из быстрого старта получим свойства коллекции, первого элемента и его поля. Список должен содержать хотя бы один элемент:

```csharp
var abilities = serializedObject.FindProperty("_abilities");
var ability = abilities.GetArrayElementAtIndex(0);
var abilityName = ability.FindPropertyRelative("Name");
```

### GetPropertyType()

Возвращает объявленный тип поля, в том числе для `[SerializeReference]`. Для элемента коллекции — тип элемента:

```csharp
abilities.GetPropertyType();   // typeof(List<Ability>)
ability.GetPropertyType();     // typeof(Ability)
abilityName.GetPropertyType(); // typeof(string)
```

### GetFieldInfo()

Возвращает `FieldInfo`, учитывая поля базовых классов и поля внутри `[SerializeReference]`:

```csharp
abilityName.GetFieldInfo(); // Поле Ability.Name
ability.GetFieldInfo();     // Поле AbilityBook._abilities
```

### GetDeclaringInstance()

Возвращает объект, которому принадлежит поле. Для элемента коллекции это владелец коллекции:

```csharp
ability.GetDeclaringInstance();     // Экземпляр AbilityBook
abilityName.GetDeclaringInstance(); // Экземпляр Ability
```

Все три метода возвращают `null`, если поиск не удался. Они читают первый целевой объект (`targetObject`), поэтому сначала примените накопленные записи. Владелец-структура возвращается как boxed-копия: её изменение не обновляет оригинал.

## Имя поля и проверка свойства

Для свойств из предыдущего раздела:

| Вызов | Результат |
|---|---|
| `ability.GetMemberName()` | `"_abilities"` — имя коллекции без индекса |
| `abilityName.GetMemberName()` | `"Name"` |
| `ability.IsArrayElement()` | `true` |
| `abilityName.IsArrayElement()` | `false` — поле внутри элемента |
| `ability.HasFoldout()` | `true` |
| `abilityName.HasFoldout()` | `false` |

`HasFoldout()` проверяет тип `Generic` и наличие видимых дочерних свойств. Managed-ссылки и разметка пользовательских `PropertyDrawer` не учитываются.

## Независимое свойство

`SerializedObject` инспектора живёт, пока открыт инспектор, поэтому свойство нельзя сохранить для отложенного вызова. `Persistent()` возвращает то же свойство на новом `SerializedObject` для тех же целевых объектов:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>var independentObject =&#10;    new SerializedObject(property&#10;        .serializedObject.targetObjects);&#10;var independent = independentObject&#10;    .FindProperty(property.propertyPath);</code></pre> | <pre lang="csharp"><code>var independent = property.Persistent();</code></pre> |

Новый объект принадлежит вызывающему коду: освободите его и свойство после записи.

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

`Persistent()` возвращает `null`, если путь свойства больше не существует. Неприменённые записи исходного объекта не копируются; исходное представление увидит изменения после `Update()`. Целевые объекты и путь свойства должны оставаться действительными до вызова.

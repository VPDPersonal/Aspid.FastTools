# SerializedProperty Extensions

Расширения `SerializedProperty` для записи значений, изменения размера коллекций и поиска типа поля и его владельца.

## Быстрый старт

В Editor-скрипте добавьте `using Aspid.FastTools.Editors;`. Здесь `serializedObject` — объект из пользовательского инспектора, а `_manaCost` — его поле типа `int`.

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>var manaCost = serializedObject&#10;    .FindProperty("_manaCost");&#10;&#10;serializedObject.Update();&#10;manaCost.intValue = 42;&#10;serializedObject&#10;    .ApplyModifiedProperties();</code></pre> | <pre lang="csharp"><code>var manaCost = serializedObject&#10;    .FindProperty("_manaCost");&#10;&#10;manaCost&#10;    .Update()&#10;    .SetIntAndApply(42);</code></pre> |

Сеттеры, `Update` и `Apply` возвращают исходное свойство для цепочек вызовов. В примере запись применяется с Undo.

**Несколько полей:** обновите объект один раз, запишите значения и примените их вместе:

```csharp
serializedObject.Update();
serializedObject.FindProperty("_cooldown").SetFloat(0.5f);
serializedObject.FindProperty("_manaCost").SetIntAndApply(10);
```

Здесь `_cooldown` — поле `float` того же объекта. Рабочий пример кнопки есть в [EditorTools](../../Samples~/EditorTools/Documentation/README.ru.md).

> [!IMPORTANT]
> Применение затрагивает **все накопленные изменения** связанного `SerializedObject`.
> `Update()` сбрасывает неприменённые записи — вызывайте его до изменения полей.

## Обновление и применение

Те же операции Unity, но с вызовом на свойстве. Строки ниже показывают отдельные варианты вызова:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>property.serializedObject&#10;    .Update();&#10;property.serializedObject&#10;    .UpdateIfRequiredOrScript();</code></pre> | <pre lang="csharp"><code>property.Update();&#10;&#10;property&#10;    .UpdateIfRequiredOrScript();</code></pre> |
| <pre lang="csharp"><code>property.serializedObject&#10;    .ApplyModifiedProperties();&#10;property.serializedObject&#10;    .ApplyModifiedPropertiesWithoutUndo();</code></pre> | <pre lang="csharp"><code>property&#10;    .ApplyModifiedProperties();&#10;property&#10;    .ApplyModifiedPropertiesWithoutUndo();</code></pre> |

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

Для каждого типа есть явный сеттер и перегрузки `SetValue`:

| Метод | Тип значения |
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

`SetEntityId` и его перегрузки доступны только в Unity 6.2 и новее.

### Перечисления

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>// Второй пункт enum&#10;mode.enumValueIndex = 1;&#10;&#10;// Битовая маска [Flags]&#10;flags.enumValueFlag = mask;</code></pre> | <pre lang="csharp"><code>// Второй пункт enum&#10;mode.SetEnumIndex(1);&#10;&#10;// Битовая маска [Flags]&#10;flags.SetEnumFlag(mask);</code></pre> |

## Массивы и списки

Для свойства массива или списка `items`:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>items.arraySize = 5;&#10;items.arraySize += 1;&#10;items.arraySize += 2;&#10;items.arraySize -= 2;&#10;items.arraySize -= 1;</code></pre> | <pre lang="csharp"><code>items.SetArraySize(5);&#10;items.AddArraySize();     // +1&#10;items.AddArraySize(2);    // +2&#10;items.RemoveArraySize(2); // -2&#10;items.RemoveArraySize();  // -1</code></pre> |

Методы меняют только размер коллекции. `RemoveArraySize` убирает элементы с конца; новые элементы инициализируйте отдельно через `GetArrayElementAtIndex()`.

## Ссылки и boxed-значения

Выберите сеттер по способу сериализации поля; строки ниже — отдельные варианты:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>// [SerializeReference]&#10;property.managedReferenceValue = instance;&#10;&#10;// UnityEngine.Object&#10;property.objectReferenceValue = asset;&#10;&#10;// ExposedReference&lt;T&gt;&#10;property.exposedReferenceValue = target;&#10;&#10;// boxedValue&#10;property.boxedValue = value;</code></pre> | <pre lang="csharp"><code>// [SerializeReference]&#10;property.SetManagedReference(instance);&#10;&#10;// UnityEngine.Object&#10;property.SetObjectReference(asset);&#10;&#10;// ExposedReference&lt;T&gt;&#10;property.SetExposedReference(target);&#10;&#10;// boxedValue&#10;property.SetBoxed(value);</code></pre> |

## Тип поля и объект-владелец

### Пример: коллекция, элемент и вложенное поле

Компонент `AbilityBook.cs`:

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

В инспекторе получим свойства коллекции, первого элемента и его поля. Список должен содержать хотя бы один элемент:

```csharp
var abilities = serializedObject.FindProperty("_abilities");
var ability = abilities.GetArrayElementAtIndex(0);
var manaCost = ability.FindPropertyRelative("ManaCost");
```

### GetPropertyType()

Возвращает объявленный тип поля, в том числе для `[SerializeReference]`. Для элемента коллекции — тип элемента:

```csharp
abilities.GetPropertyType(); // typeof(List<Ability>)
ability.GetPropertyType();   // typeof(Ability)
manaCost.GetPropertyType();  // typeof(int)
```

### GetFieldInfo()

Возвращает `FieldInfo`, учитывая поля базовых классов и поля внутри `[SerializeReference]`:

```csharp
manaCost.GetFieldInfo(); // Поле Ability.ManaCost
ability.GetFieldInfo();  // Поле AbilityBook._abilities
```

### GetDeclaringInstance()

Возвращает объект, которому принадлежит поле. Для элемента коллекции это владелец коллекции:

```csharp
ability.GetDeclaringInstance();  // Экземпляр AbilityBook
manaCost.GetDeclaringInstance(); // Экземпляр Ability
```

Все три метода возвращают `null`, если поиск не удался. Они читают первый целевой объект (`targetObject`), поэтому сначала примените накопленные записи. Владелец-структура возвращается как boxed-копия: её изменение не обновляет оригинал.

## Имя поля и проверка свойства

Для свойств из предыдущего примера:

| Вызов | Результат |
|---|---|
| `ability.GetMemberName()` | `"_abilities"` — имя коллекции без индекса |
| `manaCost.GetMemberName()` | `"ManaCost"` |
| `ability.IsArrayElement()` | `true` |
| `manaCost.IsArrayElement()` | `false` — поле внутри элемента |
| `ability.HasFoldout()` | `true` |
| `manaCost.HasFoldout()` | `false` |

`HasFoldout()` проверяет тип `Generic` и наличие видимых дочерних свойств. Managed-ссылки и разметка пользовательских `PropertyDrawer` не учитываются.

## Независимое свойство

`Persistent()` находит тот же путь в новом `SerializedObject` для тех же целевых объектов. Это удобно для отложенного вызова:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>var independentObject =&#10;    new SerializedObject(property&#10;        .serializedObject.targetObjects);&#10;var independent = independentObject&#10;    .FindProperty(property.propertyPath);</code></pre> | <pre lang="csharp"><code>var independent = property.Persistent();</code></pre> |

Для свойства типа `int` захватите `independent` в одноразовом делегате и освободите ресурсы после записи:

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

Новый `SerializedObject` не зависит от времени жизни исходного, но редактирует те же объекты. Неприменённые записи не копируются; исходное представление увидит изменения после `Update()`. Целевые объекты и путь свойства должны оставаться действительными до вызова.

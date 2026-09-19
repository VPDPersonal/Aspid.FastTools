# SerializedProperty Extensions

Цепочечные методы расширения, с которыми `SerializedProperty` записывает и применяет своё значение одним вызовом, не обращаясь к своему `SerializedObject`. Вторая группа методов отвечает, какое поле C# и какой объект стоят за свойством.

## Быстрый старт

Примеры на этой странице работают с компонентом `AbilityBook`:

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

В его пользовательском `Editor` добавьте `using Aspid.FastTools.Editors;` и запишите значение поля:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>var manaCost = serializedObject&#10;    .FindProperty("_manaCost");&#10;&#10;serializedObject.Update();&#10;manaCost.intValue = 42;&#10;serializedObject&#10;    .ApplyModifiedProperties();</code></pre> | <pre lang="csharp"><code>var manaCost = serializedObject&#10;    .FindProperty("_manaCost");&#10;&#10;manaCost&#10;    .Update()&#10;    .SetIntAndApply(42);</code></pre> |

Сеттеры, `Update()` и оба `Apply…()` возвращают исходное свойство, поэтому вызовы выстраиваются в цепочку. У каждого сеттера есть варианты `AndApply` (с Undo) и `AndApplyWithoutUndo`.

**Несколько полей:** обновите объект один раз, запишите значения и примените их вместе:

```csharp
serializedObject.Update();
serializedObject.FindProperty("_cooldown").SetFloat(0.5f);
serializedObject.FindProperty("_manaCost").SetIntAndApply(10);
```

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

`SetValue` — альтернатива явному сеттеру: `SetValue(42)` эквивалентен `SetInt(42)`, а `SetValue(0.5f)` — `SetFloat(0.5f)`. Перегрузка выбирается **по типу аргумента**, который должен соответствовать типу поля.

### Поддерживаемые типы

Явный сеттер и перегрузка `SetValue` есть для каждого типа значения `SerializedProperty`:

| Значения | Сеттеры |
|---|---|
| Числа | `SetInt`, `SetUint`, `SetLong`, `SetUlong`, `SetFloat`, `SetDouble` |
| Текст, bool и хэш | `SetString`, `SetBool`, `SetHash128` |
| Векторы | `SetVector2`, `SetVector2Int`, `SetVector3`, `SetVector3Int`, `SetVector4`, `SetQuaternion` |
| Области | `SetRect`, `SetRectInt`, `SetBounds`, `SetBoundsInt` |
| Типы Unity | `SetColor`, `SetGradient`, `SetAnimationCurve` |
| Unity 6.2 и новее | `SetEntityId` для `UnityEngine.EntityId` |

### Перечисления

Для свойств `_targeting` и `_damageTypes`:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>// Targeting.Area&#10;targeting.enumValueIndex = 1;</code></pre> | <pre lang="csharp"><code>// Targeting.Area&#10;targeting.SetEnumIndex(1);</code></pre> |
| <pre lang="csharp"><code>// Fire &#124; Ice&#10;damageTypes.enumValueFlag = 3;</code></pre> | <pre lang="csharp"><code>// Fire &#124; Ice&#10;damageTypes.SetEnumFlag(3);</code></pre> |

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
| <pre lang="csharp"><code>// [SerializeReference]&#10;effect.managedReferenceValue = instance;</code></pre> | <pre lang="csharp"><code>// [SerializeReference]&#10;effect.SetManagedReference(instance);</code></pre> |
| <pre lang="csharp"><code>// UnityEngine.Object&#10;icon.objectReferenceValue = sprite;</code></pre> | <pre lang="csharp"><code>// UnityEngine.Object&#10;icon.SetObjectReference(sprite);</code></pre> |
| <pre lang="csharp"><code>// ExposedReference&lt;T&gt;&#10;property.exposedReferenceValue = target;</code></pre> | <pre lang="csharp"><code>// ExposedReference&lt;T&gt;&#10;property.SetExposedReference(target);</code></pre> |
| <pre lang="csharp"><code>// boxedValue&#10;property.boxedValue = value;</code></pre> | <pre lang="csharp"><code>// boxedValue&#10;property.SetBoxed(value);</code></pre> |

## Тип поля и объект-владелец

Три метода через рефлексию находят поле C#, стоящее за свойством. Для `AbilityBook` из быстрого старта:

```csharp
var abilities    = serializedObject.FindProperty("_abilities");
var ability      = abilities.GetArrayElementAtIndex(0);
var abilityName  = ability.FindPropertyRelative("Name");
var effect       = serializedObject.FindProperty("_effect");
var effectDamage = effect.FindPropertyRelative("Damage");
```

| Свойство | `GetPropertyType()` | `GetFieldInfo()` | `GetDeclaringInstance()` |
|---|---|---|---|
| `abilities` | `List<Ability>` | `AbilityBook._abilities` | экземпляр `AbilityBook` |
| `ability` | `Ability` | `AbilityBook._abilities` | экземпляр `AbilityBook` |
| `abilityName` | `string` | `Ability.Name` | `Ability` с индексом 0 |
| `effect` | `IAbilityEffect` | `AbilityBook._effect` | экземпляр `AbilityBook` |
| `effectDamage` | `float` | `BurnEffect.Damage` | экземпляр `BurnEffect` |

- `GetPropertyType()` возвращает **объявленный** тип поля: для `[SerializeReference]` — интерфейс или базовый класс, а не тип экземпляра; для элемента коллекции — тип элемента.
- `GetFieldInfo()` ищет поле по фактическому типу владельца, включая приватные поля базовых классов.
- `GetDeclaringInstance()` возвращает объект, которому принадлежит поле; для элемента коллекции — владельца коллекции.

Все три метода возвращают `null`, если поиск не удался: поле не найдено, на пути встретилась `null`-ссылка или индекс вышел за границы списка. Они читают **первый** целевой объект (`targetObject`) и видят только применённые значения, поэтому сначала примените накопленные записи.

> [!WARNING]
> Если владелец поля — структура, `GetDeclaringInstance()` возвращает её boxed-копию. Изменения такой копии не попадают в оригинал; записывайте значения через `SerializedProperty`.

## Имя поля и проверка свойства

Для тех же свойств:

| Вызов | Результат |
|---|---|
| `ability.GetMemberName()` | `"_abilities"` — имя коллекции без индекса |
| `abilityName.GetMemberName()` | `"Name"` |
| `ability.IsArrayElement()` | `true` |
| `abilityName.IsArrayElement()` | `false` — поле внутри элемента |
| `ability.HasFoldout()` | `true` |
| `abilityName.HasFoldout()` | `false` |
| `effect.HasFoldout()` | `false` — `[SerializeReference]` |

`HasFoldout()` — это `true` только для свойства `Generic` с видимыми дочерними свойствами, как у стандартного инспектора. `[SerializeReference]` и пользовательские `PropertyDrawer` не учитываются.

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

`Persistent()` возвращает `null`, если путь свойства больше не существует. Неприменённые записи исходного объекта не копируются; исходное представление увидит изменения после `Update()`. Целевые объекты должны существовать до момента отложенного вызова.

## Пример в пакете

В [EditorTools](../../Samples~/EditorTools/Documentation/README.ru.md) кнопка **Halve cooldown, +5 MP** записывает два свойства через `SetFloat` и `SetIntAndApply` одним шагом Undo.

# SerializedProperty Extensions

Изменяйте сериализованные поля из инспекторов и окон редактора короткими цепочками: обновите данные, задайте значение и примените изменения. Дополнительные методы работают с размером массивов и помогают найти тип поля и его владельца.

```csharp
using Aspid.FastTools.Editors;
```

Сеттеры и методы синхронизации возвращают то же свойство; методы рефлексии возвращают найденный тип, поле или объект. Код доступен только в редакторе — размещайте его в папке `Editor` или сборке для Editor.

## Быстрый старт

В обработчике кнопки вашего `Editor` или `EditorWindow` можно изменить поле `_manaCost` типа `int`:

```csharp
var manaCost = serializedObject.FindProperty("_manaCost");
manaCost.Update().SetIntAndApply(42);
```

`Update` считывает актуальное состояние объекта, `SetInt` меняет сериализованное значение, а `AndApply` применяет изменения через Unity. Обычный инспектор увидит новое значение; изменение поддерживает Undo.

Для нескольких полей обновляйте `SerializedObject` один раз до записи и применяйте изменения после последнего сеттера:

```csharp
serializedObject.Update();
serializedObject.FindProperty("_cooldown").SetFloat(0.5f);
serializedObject.FindProperty("_manaCost").SetInt(10);
serializedObject.ApplyModifiedProperties();
```

Здесь `_cooldown` — поле `float`, `_manaCost` — поле `int` того же объекта. Готовый пример такой кнопки есть в [EditorTools](../../Samples~/EditorTools/Documentation/README.ru.md).

> [!IMPORTANT]
> `Update()` обновляет весь `SerializedObject` и сбрасывает ещё не применённые изменения. Вызывайте его до серии записей. `ApplyModifiedProperties()` и все варианты `AndApply` применяют все накопленные изменения этого `SerializedObject`.

## Обновление и применение

Тонкие обёртки над одноимёнными методами `SerializedObject` у `property.serializedObject`.

```csharp
property
    .Update()
    .SetInt(42)
    .ApplyModifiedProperties();
```

| Метод | Описание |
|-------|----------|
| `Update()` | Вызывает `serializedObject.Update()` |
| `UpdateIfRequiredOrScript()` | Вызывает `serializedObject.UpdateIfRequiredOrScript()` |
| `ApplyModifiedProperties()` | Применяет изменения с поддержкой Undo |
| `ApplyModifiedPropertiesWithoutUndo()` | Применяет изменения без записи шага Undo |

## Запись значений

Для каждого поддерживаемого типа существуют четыре варианта:

| Вариант | Поведение |
|---------|-----------|
| `SetValue(value)` | Перегруженный метод: тип аргумента при компиляции определяет нужный сеттер |
| `SetValueAndApply(value)` | `SetValue(value)` плюс `ApplyModifiedProperties()` |
| `SetXxx(value)` | Типизированный сеттер (например, `SetInt`), пишущий в соответствующее поле `SerializedProperty.xxxValue` |
| `SetXxxAndApply(value)` | `SetXxx(value)` плюс `ApplyModifiedProperties()` |

### Поддерживаемые типы

| Семейство методов | Unity-тип | Примечания |
|-------------------|-----------|------------|
| `SetInt` | `int` | |
| `SetUint` | `uint` | |
| `SetLong` | `long` | |
| `SetUlong` | `ulong` | |
| `SetFloat` | `float` | |
| `SetDouble` | `double` | |
| `SetBool` | `bool` | |
| `SetString` | `string` | |
| `SetColor` | `Color` | |
| `SetGradient` | `Gradient` | |
| `SetHash128` | `Hash128` | |
| `SetRect` / `SetRectInt` | `Rect` / `RectInt` | |
| `SetBounds` / `SetBoundsInt` | `Bounds` / `BoundsInt` | |
| `SetVector2` / `SetVector2Int` | `Vector2` / `Vector2Int` | |
| `SetVector3` / `SetVector3Int` | `Vector3` / `Vector3Int` | |
| `SetVector4` | `Vector4` | |
| `SetQuaternion` | `Quaternion` | |
| `SetAnimationCurve` | `AnimationCurve` | |
| `SetEntityId` | `EntityId` (`UnityEngine`) | Unity 6.2+ |

### Перечисления

Значения enum не идут через `SetValue` — используйте явную пару ниже в зависимости от того, является ли поле `[Flags]`-перечислением:

| Метод | Описание |
|-------|----------|
| `SetEnumFlag(int)` / `SetEnumFlagAndApply(int)` | Пишет в `enumValueFlag` |
| `SetEnumIndex(int)` / `SetEnumIndexAndApply(int)` | Пишет в `enumValueIndex` |

Тип сеттера должен соответствовать типу поля. Например, `SetValue(10)` выбирает запись `int`, а `SetValue(10f)` — `float`; метод не определяет нужный тип по содержимому `SerializedProperty`. Для `object` используйте явный сеттер ссылки или `SetBoxed` по назначению.

`SetEnumIndex` принимает индекс в списке значений enum, а не числовое значение перечисления. Например, для `enum Tier { Basic = 10, Advanced = 20 }` индекс `Advanced` равен `1`.

## Массивы и списки

| Метод | Описание |
|-------|----------|
| `SetArraySize(int)` / `SetArraySizeAndApply(int)` | Устанавливает `property.arraySize` |
| `AddArraySize(int = 1)` / `AddArraySizeAndApply(int = 1)` | Увеличивает `arraySize` на указанное количество (по умолчанию `1`) |
| `RemoveArraySize(int = 1)` / `RemoveArraySizeAndApply(int = 1)` | Уменьшает `arraySize` на указанное количество (по умолчанию `1`) |

Методы меняют размер массива или списка, а не задают содержимое новых элементов. Получите добавленный элемент через `GetArrayElementAtIndex` и явно заполните его:

```csharp
var weights = serializedObject.FindProperty("_weights"); // float[]
weights.Update().AddArraySize();
weights.GetArrayElementAtIndex(weights.arraySize - 1).SetFloat(1f);
weights.ApplyModifiedProperties();
```

## Ссылки и boxed-значения

| Метод | Описание | Примечания |
|-------|----------|------------|
| `SetManagedReference(object)` / `SetManagedReferenceAndApply(object)` | Пишет в `managedReferenceValue` (поле должно быть помечено `[SerializeReference]`) | |
| `SetObjectReference(Object)` / `SetObjectReferenceAndApply(Object)` | Пишет в `objectReferenceValue` | |
| `SetExposedReference(Object)` / `SetExposedReferenceAndApply(Object)` | Пишет в `exposedReferenceValue` | |
| `SetBoxed(object)` / `SetBoxedAndApply(object)` | Пишет в `boxedValue` | Unity 6+ |

## Тип поля и объект-владелец

Эти методы помогают разобрать поле в пользовательском drawer или инспекторе:

| Метод | Возвращает | Описание |
|-------|------------|----------|
| `GetPropertyType()` | `Type` или `null` | Возвращает `FieldType` поля, стоящего за property (для элемента массива/списка — тип элемента). `null`, если поле не удаётся разрешить. |
| `GetFieldInfo()` | `FieldInfo` или `null` | Находит backing-поле, разрешая экземпляр-владелец property (`GetDeclaringInstance`) и ища поле на его runtime-типе, включая базовые классы — поэтому цепочка с `[SerializeReference]` разрешается естественно. Для элемента массива/списка возвращается поле коллекции (как `PropertyDrawer.fieldInfo`). |
| `GetDeclaringInstance()` | `object` или `null` | Идёт по `propertyPath` от корневого `targetObject` и возвращает runtime-экземпляр, на котором объявлено backing-поле property (для элемента массива/списка — владелец поля коллекции). `null`, если путь не удаётся разрешить. Владелец-структура возвращается как boxed-копия — изменения в ней не попадут в сериализуемый объект. |

```csharp
public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
{
    var fieldType = property.GetPropertyType();
    var owner = property.GetDeclaringInstance();
    // …
}
```

`GetPropertyType()` возвращает объявленный тип поля. Для конкретной реализации managed-ссылки используйте `property.managedReferenceValue?.GetType()`. При мультивыделении `GetDeclaringInstance()` идёт от `targetObject`, то есть первого целевого объекта.

## Независимое свойство

`Persistent()` возвращает свойство с тем же путём на новом `SerializedObject`. Оно полезно, если свойство нужно сохранить отдельно от исходного потока инспектора. Ещё не применённые изменения исходного объекта не копируются.

Новый `SerializedObject` принадлежит вызывающему коду: освободите его через `Dispose`, когда он больше не нужен. Метод может вернуть `null`, если путь на целевых объектах больше не существует.

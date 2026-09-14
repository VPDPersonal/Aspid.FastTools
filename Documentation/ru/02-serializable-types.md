# Serializable Type System

Выбирайте `System.Type` в инспекторе и сохраняйте его вместе с компонентом или ассетом. Поле открывает окно с поиском, а в коде возвращает выбранный тип — например, для создания компонента или выбора реализации фабрики.

![Выбор сериализуемого типа в инспекторе](../Images/aspid_fasttools_serializable_type.gif)

Выбор сериализуемого типа в инспекторе

## Быстрый старт

Добавьте поле `SerializableType<T>`: аргумент `T` ограничит выбор совместимыми типами. В примере можно выбрать конкретного наследника `MonoBehaviour`, а при запуске он добавится на тот же GameObject.

```csharp
using UnityEngine;
using Aspid.FastTools.Types;

public sealed class ComponentSpawner : MonoBehaviour
{
    [TypeSelector(Allow = TypeAllow.None)]
    [SerializeField] private SerializableType<MonoBehaviour> _componentType;

    private void Start()
    {
        var type = _componentType?.Type;
        if (type == null || type.IsAbstract || type.ContainsGenericParameters)
            return;

        gameObject.AddComponent(type);
    }
}
```

В инспекторе откройте поле **Component Type**, найдите нужный класс и выберите его. `TypeAllow.None` исключает абстрактные классы и интерфейсы из списка. Пункт `<None>` очищает выбор; для пустого или неразрешимого значения свойство `Type` возвращает `null`.

Готовые поля и сценарии смены типов собраны в [примере Types](../../Samples~/Types/Documentation/README.ru.md).

## Какой инструмент выбрать

| Задача | Инструмент |
|---|---|
| Хранить тип, включая вложенные и generic-типы | [`SerializableType`](#serializabletype) |
| Сохранить связь с файлом скрипта при переименовании класса | [`SerializableMonoScript`](#serializablemonoscript) |
| Добавить выбор типа к строке или ограничить существующее поле | [`TypeSelector`](#typeselectorattribute) |
| Создать экземпляр в поле `[SerializeReference]` | [SerializeReference Selector](03-serialize-reference-selector.md) |
| Настроить имя, группу, иконку или видимость кандидата | [`TypeSelectorDisplay`](#typeselectordisplay) |
| Открыть окно выбора из редакторского кода | [`TypeSelectorWindow`](#typeselectorwindow) |
| Сменить тип самого компонента или ScriptableObject | [`ComponentTypeSelector`](#componenttypeselector) |

## SerializableType

`SerializableType` хранит assembly-qualified name — имя типа вместе с его сборкой. Свойство `Type` лениво разрешает эту строку в `System.Type`. Исходная строка доступна через `AssemblyQualifiedName`.

- `SerializableType` хранит любой тип.
- `SerializableType<T>` ограничивает выбор типами, совместимыми с `T`, включая реализации интерфейса.
- Оба варианта неявно преобразуются в `System.Type` и поддерживают создание из кода.

```csharp
var selected = new SerializableType<MonoBehaviour>(typeof(ComponentSpawner));
System.Type type = selected;
```

Конструктор generic-варианта выбрасывает `ArgumentException`, если переданный тип несовместим с `T`.

> [!NOTE]
> Unity сериализует обёртку по объявленному типу поля. Если присвоить `SerializableType<T>` в поле `SerializableType`, после загрузки выбранный тип сохранится, а ограничение `T` — нет. Объявляйте generic-тип непосредственно у поля.

## SerializableMonoScript

Используйте `SerializableMonoScript` или `SerializableMonoScript<T>`, когда типу соответствует отдельный файл скрипта. В редакторе обёртка хранит ссылку на `MonoScript` и при сериализации обновляет имя типа из этого ассета. Переименование или перенос класса сохраняет выбор, пока Unity распознаёт класс в том же ассете скрипта.

```csharp
[TypeSelector(Allow = TypeAllow.None)]
[SerializeField] private SerializableMonoScript<MonoBehaviour> _componentType;
```

Тип можно выбрать в окне поиска или перетащить его скрипт из **Project**. Подходят классы, которые возвращает `MonoScript.GetClass()`: не вложенные, не generic, объявленные в файле с соответствующим именем. Для вложенных и generic-типов используйте `SerializableType`.

В сборке плеера остаётся только имя типа; `Type` разрешает его так же, как у `SerializableType`. Ссылка на ассет нужна только редактору.

Оба семейства наследуют `SerializableTypeBase`, но `SerializableMonoScript` не наследует `SerializableType`. Публичного конструктора с `Type` у `SerializableMonoScript` нет — настраивайте эту обёртку через инспектор.

## TypeSelectorAttribute

`[TypeSelector]` управляет выбором типа у поля. Результат зависит от того, какое значение поле хранит:

| Поле | Результат выбора |
|---|---|
| `string` | Записывается assembly-qualified name |
| `SerializableType` / `SerializableMonoScript` | Настраивается встроенный селектор обёртки |
| `[SerializeReference]` | Создаётся экземпляр выбранной реализации |

```csharp
// Строка с именем конкретного компонента.
[TypeSelector(typeof(MonoBehaviour), Allow = TypeAllow.None)]
[SerializeField] private string _componentTypeName;

// Только компоненты, реализующие ваш интерфейс IDamageable.
[TypeSelector(typeof(IDamageable))]
[SerializeField] private SerializableType<MonoBehaviour> _damageableType;
```

При нескольких базовых типах кандидат должен быть совместим **со всеми**. У generic-обёртки ограничения атрибута пересекаются с аргументом `T`. Массивы и списки получают выбор для каждого элемента.

Атрибут помечен `[Conditional("UNITY_EDITOR")]`: в сборку плеера его применение не попадает.

### Параметры выбора

| Параметр | По умолчанию | Поведение |
|---|---|---|
| `Allow` | `TypeAllow.All` | Включает абстрактные классы и интерфейсы в дополнение к конкретным типам. `None` исключает обе категории; `Abstract` и `Interface` включают их по отдельности. На `[SerializeReference]` игнорируется |
| `Required` | `false` | Показывает предупреждение, если строка или имя типа пусты либо managed-ссылка равна `null` |

`Required` не заполняет поле автоматически и не заменяет проверку значения в коде. Для проектной проверки незаполненных обязательных полей запускайте [CI-проверку с `-srGateRequired`](04-serialize-reference-tooling.md#запуск-в-ci).

![Пустое обязательное поле показывает предупреждение рядом с селектором](../Images/aspid_fasttools_type_selector_required.png)

Пустое обязательное поле показывает предупреждение рядом с селектором

<details>
<summary>Формы аргументов TypeSelector</summary>

```csharp
[TypeSelector]
[TypeSelector(typeof(MonoBehaviour))]
[TypeSelector(typeof(MonoBehaviour), typeof(IDamageable))]
[TypeSelector("Namespace.TypeName, AssemblyName")]
[TypeSelector(nameof(_category))]
```

Конструкторы принимают один `Type`, массив `Type`, одну строку или массив строк. Строка задаёт имя члена объекта либо assembly-qualified name типа.

</details>

<a id="dynamic-base-types-via-member-references"></a>

## Ограничение из другого поля

Передайте `nameof(...)`, чтобы текущее значение поля или свойства управляло списком кандидатов:

```csharp
[SerializeField] private SerializableType<MonoBehaviour> _category;

[TypeSelector(nameof(_category))]
[SerializeField] private string _componentTypeName;
```

Измените **Category** в инспекторе — список у **Component Type Name** перестроится. Член должен принадлежать тому же объекту и быть нестатическим полем или свойством типа `Type`, `string`, `SerializableType` / `SerializableType<T>` либо массивом таких значений.

Строка сначала проверяется как имя члена. Если она не обозначает подходящий идентификатор, используется разрешение через `Type.GetType`. Для прямой ссылки на доступный тип предпочитайте `typeof`, для ссылки на член — `nameof`.

Анализатор проверяет ссылки на члены правилами `AFT0006`–`AFT0008`. Если проблему нельзя обнаружить при компиляции, инспектор показывает предупреждение у поля.

## TypeSelectorDisplay

Настройте представление класса в селекторе с помощью `[TypeSelectorDisplay]`:

```csharp
using Aspid.FastTools.Types;

[TypeSelectorDisplay(
    Name = "Damage ×",
    Group = "Combat/Modifiers",
    Tooltip = "Scales incoming damage",
    Icon = "d_ScriptableObject Icon")]
public sealed class DamageModifier { }
```

![Имя Damage ×, иконка и группа Combat/Modifiers в окне выбора](../Images/aspid_fasttools_type_selector_display.png)

Имя Damage ×, иконка и группа Combat/Modifiers в окне выбора

| Свойство | Результат |
|---|---|
| `Name` | Заменяет короткое имя в списке и закрытом поле. Поиск продолжает находить настоящее имя типа |
| `Group` | Заменяет группировку по namespace; `/` разделяет уровни, например `Combat/Melee` |
| `Tooltip` | Задаёт текст подсказки при наведении |
| `Icon` | Принимает имя `EditorGUIUtility.IconContent`, путь к ассету с расширением или путь в `Resources` без расширения |
| `Hidden` | При `true` скрывает тип из обычного выбора. Не наследуется и не мешает присваиванию из кода или отображению уже сохранённого значения |

Для `Name` и `Group` пустое значение оставляет стандартное поведение; `null` у `Tooltip` и `Icon` не задаёт переопределения.

Ручная починка через **Fix** и групповое восстановление могут предлагать скрытые типы. **Smart Fix** их не предлагает.

> [!NOTE]
> `TypeSelectorDisplay` также зависит от `UNITY_EDITOR` в сборке, где атрибут применён. Если класс собран во внешнюю DLL без этого символа, настройки атрибута, включая `Hidden`, в неё не попадут.

## TypeSelectorWindow

Общее окно выбора доступно как публичный API для инспекторов и `EditorWindow`. Типы сгруппированы по namespace или заданному `Group`; поиск находит кандидатов без ручного перехода по группам.

![Избранные и недавние типы на корневой странице селектора](../Images/aspid_fasttools_type_selector_window.png)

Избранные и недавние типы на корневой странице селектора

| Действие | Управление |
|---|---|
| Перейти по списку / выбрать / закрыть | Стрелки / Enter / Escape |
| Вернуться к родительской группе | Стрелка влево или хлебные крошки |
| Добавить тип в избранное | Space или звёздочка при наведении |
| Очистить поле | `<None>` |

**Favorites** и **Recent** хранятся локально для проекта в `EditorPrefs` и скрываются во время поиска. Их отображение и ёмкость истории настраиваются в **Settings** окна FastTools. Текущий тип отмечен галочкой, у групп показаны счётчики кандидатов.

### Generic-типы

При выборе открытого generic-типа окно предлагает выбрать его аргументы и возвращает сконструированный тип:

![Выбор аргумента generic-типа в селекторе](../Images/aspid_fasttools_type_selector_generic.gif)

Выбор аргумента generic-типа в селекторе

В стандартном выборе аргументов предлагаются сериализуемые значения: примитивы, enum, строки, ссылки на `UnityEngine.Object`, классы и структуры с `[Serializable]`. Аргумент также должен удовлетворять ограничениям generic-параметра. Правила вывода аргументов для managed-ссылок описаны в [SerializeReference Selector](03-serialize-reference-selector.md#generic-типы).

### Открытие из кода

В редакторском коде вызовите `TypeSelectorWindow.Show` из пространства имён `Aspid.FastTools.Types.Editors`:

```csharp
TypeSelectorWindow.Show(
    screenRect,
    new TypeSelectorFilter
    {
        Types = new[] { typeof(MonoBehaviour) },
        Allow = TypeAllow.None
    },
    currentAqn: selectedTypeName,
    onSelected: aqn => selectedTypeName = aqn);
```

Здесь `screenRect` — прямоугольник кнопки в **экранных координатах**, `selectedTypeName` — строка текущего выбора. Обработчик получает assembly-qualified name или `null` при выборе `<None>`. Если значение хранится в ассете, запишите его через `SerializedProperty` и примените изменения.

<details>
<summary>Дополнительные фильтры TypeSelectorFilter</summary>

| Свойство | Назначение |
|---|---|
| `Types` | Все базовые типы, которым должен соответствовать кандидат |
| `Allow` | Разрешённые категории: абстрактные классы и интерфейсы |
| `Predicate` | Дополнительное условие для кандидата |
| `AdditionalTypes` | Кандидаты, обходящие `Types`, `Allow` и `Predicate`; фильтр скрытых типов сохраняется |
| `ArgumentFilter` | Фильтр аргументов generic-типа, выбираемых вручную |
| `InferredArgumentFilter` | Фильтр аргументов, выведенных из типа поля |
| `IncludeHidden` | Показывать типы с `Hidden = true` |
| `HideNoneOption` | Убрать пустой выбор `<None>` |

</details>

Полный пример привязки окна к кнопке есть в [EditorTools](../../Samples~/EditorTools/Documentation/README.ru.md).

## ComponentTypeSelector

Этот селектор меняет тип **самого компонента или ScriptableObject**. Добавьте поле в базовый класс: список автоматически ограничится его подтипами.

```csharp
using UnityEngine;
using Aspid.FastTools.Types;

public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField] private ComponentTypeSelector _enemyType;
    [SerializeField, Min(0)] private float _health = 100f;

    public abstract void Attack();
}
```

Создайте конкретных наследников `EnemyBase` в отдельных файлах скриптов и добавьте одного из них на GameObject. Поле **Enemy Type** позволит переключить реализацию.

![Смена типа компонента через ComponentTypeSelector](../Images/aspid_fasttools_component_type_selector.gif)

Смена типа компонента через ComponentTypeSelector

При выборе редактор заменяет `m_Script` у сериализованного объекта. В UI Toolkit-инспекторе стандартная строка **Script** скрывается, пока присутствует селектор; IMGUI-инспектор продолжает рисовать её самостоятельно.

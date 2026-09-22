# Serializable Type System

Выбор типа в инспекторе. Тип сохраняется вместе с компонентом или ассетом и читается в коде как `System.Type`. Обёртки хранят только сам тип — экземпляр создаёт ваш код. Для экземпляра с редактируемыми данными есть [SerializeReference Selector](03-serialize-reference-selector.md).

## Быстрый старт

Unity не сериализует поле `System.Type` напрямую. Вместо строки, которую нужно заполнять и разрешать вручную, объявите `SerializableType<T>`. Аргумент `T` ограничивает выбор совместимыми типами. Для примера используем `Collider`:

| До — строка с именем типа | После — SerializableType |
|---|---|
| <pre lang="csharp"><code>[SerializeField]&#10;private string _colliderTypeName;&#10;&#10;public System.Type ColliderType =&gt;&#10;    string.IsNullOrEmpty(_colliderTypeName)&#10;        ? null&#10;        : System.Type.GetType(&#10;            _colliderTypeName, false);</code></pre> | <pre lang="csharp"><code>[TypeSelector(Allow = TypeAllow.None)]&#10;[SerializeField]&#10;private SerializableType&lt;Collider&gt;&#10;    _colliderType;&#10;&#10;public System.Type ColliderType =&gt;&#10;    _colliderType?.Type;</code></pre> |

Сама обёртка уже имеет селектор; атрибут здесь исключает абстрактные классы и интерфейсы.

![Выбор сериализуемого типа в инспекторе](../Images/serializable-type-quick-start.gif)

Выбор сериализуемого типа в инспекторе

## Какой инструмент выбрать

| Задача | Инструмент |
|---|---|
| Хранить тип, включая generic-типы и типы, объявленные внутри другого класса | [`SerializableType`](#serializabletype) |
| Сохранять выбор при переименовании класса и его файла | [`SerializableMonoScript`](#serializablemonoscript) |
| Добавить выбор типа к строке или ограничить поле | [`TypeSelector`](#typeselectorattribute) |
| Хранить экземпляр в `[SerializeReference]` | [SerializeReference Selector](03-serialize-reference-selector.md) |
| Настроить имя, группу, иконку или видимость кандидата | [`TypeSelectorDisplay`](#typeselectordisplay) |
| Открыть окно из редакторского кода | [`TypeSelectorWindow`](#typeselectorwindow) |

## SerializableType

`SerializableType` хранит assembly-qualified name — имя типа вместе со сборкой.

| Вариант | Ограничение выбора |
|---|---|
| `SerializableType` | Без базового ограничения |
| `SerializableType<T>` | Типы, совместимые с `T`, включая реализации интерфейса |

Оба варианта неявно преобразуются в `System.Type` и имеют публичный конструктор с аргументом `Type`:

```csharp
var selected = new SerializableType<Collider>(typeof(BoxCollider));
System.Type type = selected;

var empty = new SerializableType<Collider>(null);
```

Тип должен быть совместим с `T`, иначе конструктор выбросит `ArgumentException`. Для пустой обёртки передайте `null`; публичного конструктора без аргументов нет.

| Свойство или вызов | Результат |
|---|---|
| `Type` | Разрешённый `System.Type`; `null`, если выбор пуст или имя больше не разрешается |
| `AssemblyQualifiedName` | Сохранённое имя, даже если тип потерян; пустая строка для пустого выбора |
| `BaseType` | `typeof(object)` либо `typeof(T)` у generic-варианта |
| `ToString()` | Короткое имя найденного типа; иначе сохранённое имя |

### Пустое значение и переименование

После переименования класса, namespace или сборки сохранённое имя может перестать разрешаться. Тогда инспектор покажет `<Missing>`; перед использованием проверяйте `.Type` на `null`.

> [!NOTE]
> Unity сериализует обёртку по объявленному типу поля. Если присвоить `SerializableType<T>` в поле `SerializableType`, выбранный тип переживёт загрузку, а ограничение `T` — нет. Объявляйте generic-вариант непосредственно у поля. Это же правило относится к `SerializableMonoScript<T>`.

## SerializableMonoScript

`SerializableMonoScript` связывает выбранный тип с ассетом скрипта и сохраняет выбор при согласованном переименовании или переносе класса и файла. Выберите тип в инспекторе или перетащите `.cs` из **Project**.

| Хранение имени | Связь с ассетом скрипта |
|---|---|
| <pre lang="csharp"><code>[TypeSelector(Allow = TypeAllow.None)]&#10;[SerializeField]&#10;private SerializableType&lt;MonoBehaviour&gt;&#10;    _componentType;</code></pre> | <pre lang="csharp"><code>[TypeSelector(Allow = TypeAllow.None)]&#10;[SerializeField]&#10;private SerializableMonoScript&lt;MonoBehaviour&gt;&#10;    _componentType;</code></pre> |

| Возможность | SerializableType | SerializableMonoScript |
|---|---|---|
| Выбор через окно поиска | Да | Да, только типы с подходящим MonoScript |
| Generic-типы и типы, объявленные внутри другого класса | Да | Нет |
| Встроенные типы Unity без ассета `MonoScript`, например `BoxCollider` | Да | Нет |
| Обновление имени после переименования скрипта | Вручную | Из сохранённого MonoScript при сериализации |
| Создание из кода с `Type` | Публичный конструктор | Публичного конструктора нет |
| В плеере | Имя типа | Имя типа; ссылка на MonoScript только в редакторе |

`BoxCollider` поставляется в сборке `UnityEngine.PhysicsModule`, поэтому в проекте нет связанного с ним ассета скрипта.

Скрипт должен содержать класс верхнего уровня, не generic, в файле с соответствующим именем; `MonoScript.GetClass()` должен возвращать этот класс. При переименовании сохраняйте ассет и его `.meta`. Если Unity перестаёт распознавать класс, обёртка оставляет последнее известное имя.

Читайте выбранный тип через `.Type` или неявное преобразование в `System.Type`, как у `SerializableType`.

## TypeSelectorAttribute

Атрибут настраивает выбор у поля. Обёртки имеют встроенный селектор и без атрибута; для обычной строки атрибут добавляет его.

| Поле | Результат выбора |
|---|---|
| `string` | Записывается assembly-qualified name |
| `SerializableType` / `SerializableMonoScript` | Настраивается выбор обёртки |
| `[SerializeReference]` | Создаётся экземпляр выбранной реализации |

### Ограничения и коллекции

```csharp
[TypeSelector(typeof(MonoBehaviour), Allow = TypeAllow.None)]
[SerializeField] private string _componentTypeName;

[TypeSelector(typeof(IDamageable), Allow = TypeAllow.None)]
[SerializeField] private SerializableType<MonoBehaviour> _damageableType;

[TypeSelector(Allow = TypeAllow.None)]
[SerializeField] private SerializableType<Collider>[] _colliderTypes;
```

`IDamageable` здесь — ваш интерфейс. В поле `_damageableType` предлагаются компоненты, которые одновременно наследуют `MonoBehaviour` и реализуют `IDamageable`. Ограничения действуют одновременно (**И**) на полях любого вида. Массивы и списки получают выбор для каждого элемента.

У `[SerializeReference]` первым ограничением служит тип поля. Допустим, `Sword` реализует `IWeapon` и `IMelee`, а `Glaive` — `IWeapon`, `IMelee` и `IRanged`:

```csharp
[TypeSelector(typeof(IMelee), typeof(IRanged))]
[SerializeReference] private IWeapon _weapon;
```

В поле можно выбрать только `Glaive`: `Sword` не реализует `IRanged`. Чтобы разрешить определённый набор классов, дайте им общий интерфейс и укажите его: перечисление самих классов (`typeof(Pistol), typeof(Rifle)`) оставит список пустым, и анализатор `AFT0009` об этом предупредит. Подробнее — [настройка селектора экземпляров](03-serialize-reference-selector.md#настройка-выбора).

### Конструкторы и свойства

| Свойство | По умолчанию | Поведение |
|---|---|---|
| `Allow` | `TypeAllow.All` | `Abstract` добавляет абстрактные классы, `Interface` — интерфейсы; `All` включает обе категории, `None` исключает их. На `[SerializeReference]` игнорируется |
| `Required` | `false` | Предупреждает о пустом имени типа или `null` в managed-ссылке |

Статические классы в списке не отображаются. Для строки или обёртки `Allow` фильтрует категории типов, но не проверяет наличие конструктора без параметров.

<details>
<summary>Формы аргументов TypeSelector</summary>

```csharp
[TypeSelector]
[TypeSelector(typeof(MonoBehaviour))]
[TypeSelector(typeof(MonoBehaviour), typeof(IDamageable))]
[TypeSelector("Namespace.TypeName, AssemblyName")]
[TypeSelector(nameof(_category))]
```

На поле можно поставить один `[TypeSelector]`. Он принимает аргументы `Type` или `string`: один, несколько через запятую (`params`) либо массив. Без аргументов атрибут не добавляет ограничений. Строка сначала ищется как имя поля или свойства; если такой член не найден — как имя типа.

</details>

### Обязательное поле

```csharp
[TypeSelector(Required = true, Allow = TypeAllow.None)]
[SerializeField] private SerializableType<Collider> _requiredType;
```

![Пустое обязательное поле показывает предупреждение рядом с селектором](../Images/type-selector-required.png)

Пустое обязательное поле показывает предупреждение рядом с селектором

С `Required = true` пункт `<None>` остаётся доступным: можно очистить поле, и рядом появится предупреждение. У строки или обёртки проверяется пустое сохранённое имя; потерянный тип с непустым именем эту проверку проходит.

Настройка проверки по всему проекту и в CI описана в разделе [проверки обязательных полей](04-serialize-reference-tooling.md#где-проверяются-обязательные-поля).

<a id="dynamic-base-types-via-member-references"></a>

## Ограничение из другого поля

Передайте `nameof(...)`, чтобы текущее значение поля или свойства управляло списком кандидатов. Например, базовая категория и зависящий от неё выбор:

```csharp
[SerializeField] private SerializableType<MonoBehaviour> _category;

[TypeSelector(nameof(_category), Allow = TypeAllow.None)]
[SerializeField] private string _componentTypeName;
```

Измените **Category**, затем откройте **Component Type Name**: список будет ограничен выбранным типом и его наследниками. Смена ограничения сама по себе не очищает ранее выбранное имя — проверьте зависимое поле и при необходимости выберите тип заново.

| Источник ограничения | Поддержка |
|---|---|
| `System.Type` | Один тип |
| `string` | Имя типа, разрешаемое через `Type.GetType` |
| `SerializableType`, `SerializableMonoScript` и их generic-варианты | Разрешённое значение `.Type` |
| Массив этих значений | Несколько ограничений одновременно |

Источник — нестатическое поле или читаемое свойство объекта, который редактирует инспектор. Подходят и унаследованные члены; индексаторы не поддерживаются. Пустой источник не добавляет ограничения. У generic-обёртки её собственный `T` продолжает ограничивать выбор.

Для типа используйте `typeof`, для поля или свойства — `nameof`. Если строка не указывает ни на член объекта, ни на доступный тип, инспектор показывает предупреждение.

![Опечатка _categroy вместо _category вызывает предупреждение. nameof(_category) помогает избежать такой ошибки.](../Images/type-selector-constraint-warning.png)

Опечатка _categroy вместо _category вызывает предупреждение. nameof(_category) помогает избежать такой ошибки.

## TypeSelectorDisplay

`TypeSelectorDisplay` задаёт подпись, группу, иконку и подсказку типа в окне выбора:

```csharp
using Aspid.FastTools.Types;

[TypeSelectorDisplay(
    Name = "Damage ×",
    Group = "Combat/Modifiers",
    Tooltip = "Scales incoming damage",
    Icon = "d_ScriptableObject Icon")]
public sealed class DamageModifier { }
```

![Имя Damage ×, иконка и группа Combat/Modifiers в окне выбора](../Images/type-selector-display.png)

Имя Damage ×, иконка и группа Combat/Modifiers в окне выбора

| Свойство | Результат |
|---|---|
| `Name` | Подпись в списке и закрытом поле. Поиск продолжает находить настоящее имя типа |
| `Group` | Группировка вместо namespace; `/` разделяет уровни, например `Combat/Melee` |
| `Tooltip` | Текст подсказки при наведении |
| `Icon` | Имя `EditorGUIUtility.IconContent`, путь к ассету с расширением или путь в `Resources` без расширения |
| `Hidden` | При `true` скрывает тип из обычного выбора. Не наследуется и не мешает присваиванию из кода или отображению сохранённого значения |

> [!NOTE]
> `TypeSelectorDisplay` зависит от `UNITY_EDITOR` в сборке, где атрибут применён. Если класс собран во внешнюю DLL без этого символа, настройки атрибута, включая `Hidden`, в неё не попадут.

## TypeSelectorWindow

Окно группирует типы по namespace или `Group` и различает одинаковые имена по сборкам. Через `TypeSelectorWindow` его можно открыть из своего инспектора или окна редактора.

![Избранные и недавние типы на корневой странице селектора](../Images/type-selector-window.png)

Избранные и недавние типы на корневой странице селектора

| Действие | Управление |
|---|---|
| Перемещение / выбор / закрытие | Стрелки / Enter / Escape |
| Возврат в родительскую группу | Стрелка влево или хлебные крошки |
| Переключение избранного | Space или звёздочка при наведении |
| Очистка значения | `<None>` |

Отображение **Favorites**, **Recent** и ёмкость истории настраиваются во вкладке **Settings** окна FastTools.

### Generic-типы

При выборе открытого generic-типа окно предлагает выбрать аргументы, а затем возвращает сконструированный закрытый тип. Например, для `Container<T>` после выбора `int` результатом будет `Container<int>`. Generic-аргумент тоже может быть generic-типом: сначала окно попросит задать его собственные аргументы.

![Выбор аргумента generic-типа в селекторе](../Images/type-selector-generic.gif)

Выбор аргумента generic-типа в селекторе

Аргумент должен удовлетворять ограничениям generic-параметра; `[Serializable]` не требуется. Для `[SerializeReference]` действуют дополнительные [правила сериализуемости и вывода аргументов](03-serialize-reference-selector.md#generic-типы).

### Открытие из кода

В editor-скрипте используйте `Aspid.FastTools.Types.Editors`. `screenRect` — прямоугольник кнопки в **экранных координатах**, `selectedTypeName` — строка текущего выбора:

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

Обработчик получает assembly-qualified name или `null` при выборе `<None>`. Закрытие окна без выбора не присваивает значение. Если результат хранится в ассете, записывайте его через `SerializedProperty` и применяйте изменения.

`currentAqn` задаёт текущую отметку; пустая строка отмечает `<None>`, а `null` оставляет выбор без отметки.

### Фильтры окна

`TypeSelectorFilter` — структура. У `default` значение `Allow` равно `None`, в отличие от атрибута `[TypeSelector]`, где по умолчанию `All`. Задавайте режим явно, когда нужны абстрактные классы или интерфейсы.

<details>
<summary>Свойства фильтра окна</summary>

| Свойство | Назначение |
|---|---|
| `Types` | Все базовые типы, которым должен соответствовать кандидат |
| `Allow` | Разрешённые категории: абстрактные классы и интерфейсы |
| `Predicate` | Дополнительное условие после проверки типа и категории |
| `AdditionalTypes` | Кандидаты, обходящие `Types`, `Allow` и `Predicate`; фильтр `Hidden` сохраняется |
| `ArgumentFilter` | Дополнительный фильтр аргументов, выбираемых вручную |
| `InferredArgumentFilter` | Фильтр аргументов, выведенных из типа поля |
| `IncludeHidden` | Показывать типы с `Hidden = true` |
| `HideNoneOption` | Скрыть `<None>` на корневой странице |

Для сужения списка используйте `Predicate`; `AdditionalTypes` добавляет кандидатов в обход ограничений.

</details>

Пример окна, работающего с ассетами, есть в [EditorTools](../../Samples~/EditorTools/Documentation/README.ru.md).

## Если выбор не работает

| Симптом | Что проверить |
|---|---|
| Нужный класс отсутствует | Совместимость с базой и всеми ограничениями, `Allow`, `Hidden` и ошибки компиляции |
| В SerializableMonoScript нет типа, который есть в SerializableType | Есть ли отдельный файл скрипта и возвращает ли `MonoScript.GetClass()` нужный класс |
| После смены Category осталось старое значение | Ограничение меняет список кандидатов, а не переписывает зависимое поле |
| `<Missing>` при непустом имени | Не изменились ли класс, namespace или сборка; выберите существующий тип заново |
| Required не предупреждает о потерянном типе | Для строк и обёрток проверяется пустое имя, а не успешность его разрешения |
| Type выбран, но объект не появился | Хранение типа не создаёт экземпляр; используйте свой код создания или [SerializeReference Selector](03-serialize-reference-selector.md) |

## Пример в пакете

Выбор типов врагов и паттерна расстановки в инспекторе показан в примере [Types](../../Samples~/Types/Documentation/README.ru.md).

![Волна обычных и элитных врагов движется к центру.](../../Samples~/Types/Documentation/Images/demo.gif)

Волна обычных и элитных врагов движется к центру.

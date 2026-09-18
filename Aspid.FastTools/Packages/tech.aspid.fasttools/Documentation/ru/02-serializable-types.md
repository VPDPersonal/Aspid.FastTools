# Serializable Type System

Выбирайте тип в инспекторе, сохраняйте его вместе с компонентом или ассетом и получайте `System.Type` в коде. Обёртки хранят выбранный тип; создание экземпляра остаётся за вашим кодом. Для хранения экземпляра с редактируемыми данными используйте [SerializeReference Selector](03-serialize-reference-selector.md).

## Быстрый старт

Unity не сериализует поле `System.Type` напрямую. Вместо строки, которую нужно заполнять и разрешать вручную, объявите `SerializableType<T>`. Аргумент `T` ограничивает выбор совместимыми типами. Для примера используем `Collider`:

| До — строка с именем типа | После — SerializableType |
|---|---|
| <pre lang="csharp"><code>[SerializeField]&#10;private string _colliderTypeName;&#10;&#10;public System.Type ColliderType =&gt;&#10;    string.IsNullOrEmpty(_colliderTypeName)&#10;        ? null&#10;        : System.Type.GetType(&#10;            _colliderTypeName, false);</code></pre> | <pre lang="csharp"><code>[TypeSelector(Allow = TypeAllow.None)]&#10;[SerializeField]&#10;private SerializableType&lt;Collider&gt;&#10;    _colliderType;&#10;&#10;public System.Type ColliderType =&gt;&#10;    _colliderType?.Type;</code></pre> |

Для варианта справа нужны `using UnityEngine;` и `using Aspid.FastTools.Types;`. Сама обёртка уже имеет селектор; атрибут здесь исключает абстрактные классы и интерфейсы.

### Пример: добавить выбранный Collider

Сохраните код в `ColliderSpawner.cs`:

```csharp
using UnityEngine;
using Aspid.FastTools.Types;

public sealed class ColliderSpawner : MonoBehaviour
{
    [TypeSelector(Allow = TypeAllow.None)]
    [SerializeField] private SerializableType<Collider> _colliderType;

    private void Start()
    {
        var type = _colliderType?.Type;
        if (type == null || type.IsAbstract || type.ContainsGenericParameters)
            return;
        if (!typeof(Collider).IsAssignableFrom(type)) return;

        gameObject.AddComponent(type);
    }
}
```

1. Добавьте `ColliderSpawner` на GameObject.
2. Откройте поле **Collider Type**, найдите и выберите **BoxCollider**.
3. Войдите в Play Mode: на объекте появится **Box Collider**.
4. Выйдите из Play Mode и выберите `<None>`: при следующем запуске компонент ничего не добавит.

![Выбор сериализуемого типа в инспекторе](../Images/aspid_fasttools_serializable_type.gif)

Выбор сериализуемого типа в инспекторе

Готовые сценарии с типами врагов и фабрикой расстановки собраны в [примере Types](../../Samples~/Types/Documentation/README.ru.md).

## Какой инструмент выбрать

| Задача | Инструмент |
|---|---|
| Хранить тип, включая вложенные и generic-типы | [`SerializableType`](#serializabletype) |
| Сохранять выбор при переименовании класса и его файла | [`SerializableMonoScript`](#serializablemonoscript) |
| Добавить выбор типа к строке или ограничить поле | [`TypeSelector`](#typeselectorattribute) |
| Хранить экземпляр в `[SerializeReference]` | [SerializeReference Selector](03-serialize-reference-selector.md) |
| Настроить имя, группу, иконку или видимость кандидата | [`TypeSelectorDisplay`](#typeselectordisplay) |
| Открыть окно из редакторского кода | [`TypeSelectorWindow`](#typeselectorwindow) |
| Сменить тип самого компонента или ScriptableObject | [`ComponentTypeSelector`](#componenttypeselector) |

## SerializableType

`SerializableType` хранит assembly-qualified name — имя типа вместе со сборкой. `Type` лениво разрешает строку и кэширует найденный `System.Type`; после десериализации кэш сбрасывается. Обёртка не хранит поля выбранного класса.

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

`new SerializableType<Collider>(typeof(string))` выбросит `ArgumentException`. Публичного конструктора без аргументов нет: для пустой обёртки из кода передайте `null`, а в сериализованном поле можно оставить инициализацию Unity.

| Свойство или вызов | Результат |
|---|---|
| `Type` | Разрешённый `System.Type`; `null`, если выбор пуст или имя больше не разрешается |
| `AssemblyQualifiedName` | Сохранённое имя, даже если тип потерян; пустая строка для пустого выбора |
| `BaseType` | `typeof(object)` либо `typeof(T)` у generic-варианта |
| `ToString()` | Короткое имя найденного типа; иначе сохранённое имя |

### Пустое значение и переименование

Проверяйте `Type` перед использованием. Пустой выбор и потерянный тип дают `null`, но у потерянного типа остаётся `AssemblyQualifiedName`, а инспектор показывает `<Missing>`. Переименование класса, namespace или сборки может сделать старую строку неразрешимой; само хранение имени не обеспечивает миграцию.

> [!NOTE]
> Unity сериализует обёртку по объявленному типу поля. Если присвоить `SerializableType<T>` в поле `SerializableType`, выбранный тип переживёт загрузку, а ограничение `T` — нет. Объявляйте generic-вариант непосредственно у поля. Это же правило относится к `SerializableMonoScript<T>`.

## SerializableMonoScript

Используйте это семейство, когда выбранный тип связан с файлом скрипта. В редакторе обёртка хранит ссылку на `MonoScript` и при сериализации обновляет имя типа из этого ассета. Выбор переживает переименование или перенос, пока Unity распознаёт нужный класс в том же ассете скрипта.

| Хранение имени | Связь с ассетом скрипта |
|---|---|
| <pre lang="csharp"><code>[TypeSelector(Allow = TypeAllow.None)]&#10;[SerializeField]&#10;private SerializableType&lt;MonoBehaviour&gt;&#10;    _componentType;</code></pre> | <pre lang="csharp"><code>[TypeSelector(Allow = TypeAllow.None)]&#10;[SerializeField]&#10;private SerializableMonoScript&lt;MonoBehaviour&gt;&#10;    _componentType;</code></pre> |

| Возможность | SerializableType | SerializableMonoScript |
|---|---|---|
| Выбор через окно поиска | Да | Да, только типы с подходящим MonoScript |
| Вложенные и generic-типы | Да | Нет |
| Типы без отдельного скрипта, например BoxCollider | Да | Нет |
| Обновление имени после переименования скрипта | Вручную | Из сохранённого MonoScript при сериализации |
| Создание из кода с `Type` | Публичный конструктор | Публичного конструктора нет |
| В плеере | Имя типа | Имя типа; ссылка на MonoScript только в редакторе |

Выберите тип через поиск или перетащите `.cs` из **Project**. Нужен класс, который возвращает `MonoScript.GetClass()`: верхнего уровня, не generic, в файле с соответствующим именем. Переименовывайте класс и файл согласованно, сохраняя ассет и его `.meta`. Если скрипт удалён или перестал разрешать класс, обёртка сохраняет последнее известное имя, но не восстанавливает класс автоматически.

Оба семейства наследуют `SerializableTypeBase`, но `SerializableMonoScript` не наследует `SerializableType`. Чтение `.Type` и неявное преобразование в `System.Type` доступны у обоих; настраивайте `SerializableMonoScript` через инспектор.

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

`IDamageable` здесь — ваш интерфейс. В поле `_damageableType` предлагаются компоненты, которые одновременно наследуют `MonoBehaviour` и реализуют `IDamageable`. Для строк и обёрток несколько ограничений пересекаются: кандидат должен удовлетворять **каждому**. Массивы и списки получают выбор для каждого элемента.

На `[SerializeReference]` действуют правила [селектора экземпляров](03-serialize-reference-selector.md#настройка-выбора). Атрибут помечен `[Conditional("UNITY_EDITOR")]`: его применение не попадает в сборку плеера.

### Параметры выбора

| Параметр | По умолчанию | Поведение |
|---|---|---|
| `Allow` | `TypeAllow.All` | `Abstract` добавляет абстрактные классы, `Interface` — интерфейсы; `All` включает обе категории, `None` исключает их. На `[SerializeReference]` игнорируется |
| `Required` | `false` | Предупреждает о пустом имени типа или `null` в managed-ссылке |

`TypeAllow.None` означает конкретные типы, а не пустой список. Статические классы не предлагаются. Этот фильтр не проверяет наличие нужного вашему коду конструктора: перед созданием обычного C#-объекта учитывайте требования своей фабрики.

<details>
<summary>Формы аргументов TypeSelector</summary>

```csharp
[TypeSelector]
[TypeSelector(typeof(MonoBehaviour))]
[TypeSelector(typeof(MonoBehaviour), typeof(IDamageable))]
[TypeSelector("Namespace.TypeName, AssemblyName")]
[TypeSelector(nameof(_category))]
```

Это альтернативные варианты, а не набор атрибутов для одного поля. Конструкторы принимают один `Type`, массив `Type`, одну строку или массив строк. Строка обозначает член объекта либо имя типа вместе со сборкой.

</details>

### Обязательное поле

```csharp
[TypeSelector(Required = true, Allow = TypeAllow.None)]
[SerializeField] private SerializableType<Collider> _requiredType;
```

![Пустое обязательное поле показывает предупреждение рядом с селектором](../Images/aspid_fasttools_type_selector_required.png)

Пустое обязательное поле показывает предупреждение рядом с селектором

`Required` не выбирает тип автоматически, не блокирует `<None>` и не заменяет проверку в коде. У строк и обёрток проверяется **пустое сохранённое имя**: непустое имя потерянного типа — другая проблема, оно не становится нарушением `Required`.

Проектный поиск обязательных полей работает в [Project References](04-serialize-reference-tooling.md#где-проверяются-обязательные-поля) при включённой проверке. Для CI нужен [`-srGateRequired`](04-serialize-reference-tooling.md#запуск-в-ci); обычная проверка перед сборкой плеера такие поля не проверяет.

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

Строка сначала ищется как имя члена объекта, затем — как имя типа. Для доступного типа предпочитайте `typeof`, для члена — `nameof`. Анализатор проверяет ссылки на члены правилами `AFT0006`–`AFT0008`; если ограничение не удалось разрешить во время работы инспектора, поле показывает предупреждение.

## TypeSelectorDisplay

Атрибут меняет представление кандидата, сохраняя его реальное имя и идентичность в данных. Для обычного класса:

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
| `Name` | Подпись в списке и закрытом поле. Поиск продолжает находить настоящее имя типа |
| `Group` | Группировка вместо namespace; `/` разделяет уровни, например `Combat/Melee` |
| `Tooltip` | Текст подсказки при наведении |
| `Icon` | Имя `EditorGUIUtility.IconContent`, путь к ассету с расширением или путь в `Resources` без расширения |
| `Hidden` | При `true` скрывает тип из обычного выбора. Не наследуется и не мешает присваиванию из кода или отображению сохранённого значения |

Пустые `Name` и `Group` оставляют стандартное представление; `null` у `Tooltip` и `Icon` не задаёт переопределения. Для generic-типа подпись сохраняет список аргументов. Классу, используемому только как сохранённое имя, не нужен `[Serializable]`; если он хранится как экземпляр `[SerializeReference]`, требования сериализации действуют отдельно.

Ручные **Fix** и массовое восстановление могут предлагать скрытые типы. **Smart Fix** их не предлагает: скрытие из обычного выбора не должно мешать восстановлению старых данных.

> [!NOTE]
> `TypeSelectorDisplay` зависит от `UNITY_EDITOR` в сборке, где атрибут применён. Если класс собран во внешнюю DLL без этого символа, настройки атрибута, включая `Hidden`, в неё не попадут.

## TypeSelectorWindow

Общее окно выбора используется полями и доступно как публичный API для своих инспекторов и `EditorWindow`. Типы сгруппированы по namespace или `Group`; поиск находит кандидатов без ручного перехода по группам. Для одинаковых имён окно показывает различия сборок.

![Избранные и недавние типы на корневой странице селектора](../Images/aspid_fasttools_type_selector_window.png)

Избранные и недавние типы на корневой странице селектора

| Действие | Управление |
|---|---|
| Перемещение / выбор / закрытие | Стрелки / Enter / Escape |
| Возврат в родительскую группу | Стрелка влево или хлебные крошки |
| Переключение избранного | Space или звёздочка при наведении |
| Очистка значения | `<None>` |

**Favorites** и **Recent** хранятся локально для проекта в `EditorPrefs` и скрываются во время поиска. Их отображение и ёмкость истории настраиваются в **Settings** окна FastTools. Текущий тип отмечен галочкой, у групп показаны счётчики кандидатов.

### Generic-типы

При выборе открытого generic-типа окно предлагает выбрать аргументы, а затем возвращает сконструированный закрытый тип. Например, для `Container<T>` после выбора `int` результатом будет `Container<int>`. Generic-аргумент тоже может быть generic-типом: сначала окно закроет его собственные параметры.

![Выбор аргумента generic-типа в селекторе](../Images/aspid_fasttools_type_selector_generic.gif)

Выбор аргумента generic-типа в селекторе

Аргумент должен удовлетворять ограничениям параметра, включая базовые типы, интерфейсы и `where`-ограничения. Обычный селектор имени типа не требует `[Serializable]` от аргумента: он сохраняет имя, а не значение этого типа. Для `[SerializeReference]` дополнительно применяются [правила сериализуемости и вывода аргументов](03-serialize-reference-selector.md#generic-типы).

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

<details>
<summary>Полное IMGUI-окно с кнопкой выбора</summary>

Создайте `TypePickerWindow.cs` в папке `Editor` и откройте **Tools → Type Picker**. Окно показывает выбранное имя, не создавая компонент:

```csharp
using System;
using UnityEditor;
using UnityEngine;
using Aspid.FastTools.Types;
using Aspid.FastTools.Types.Editors;

public sealed class TypePickerWindow : EditorWindow
{
    [SerializeField] private string _selectedTypeName;

    [MenuItem("Tools/Type Picker")]
    private static void Open() => GetWindow<TypePickerWindow>("Type Picker");

    private void OnGUI()
    {
        var caption = string.IsNullOrEmpty(_selectedTypeName)
            ? "<None>"
            : Type.GetType(_selectedTypeName, false)?.Name ?? "<Missing>";
        var rect = GUILayoutUtility.GetRect(new GUIContent(caption), GUI.skin.button);

        if (GUI.Button(rect, caption))
        {
            TypeSelectorWindow.Show(
                GUIUtility.GUIToScreenRect(rect),
                new TypeSelectorFilter
                {
                    Types = new[] { typeof(MonoBehaviour) },
                    Allow = TypeAllow.None
                },
                _selectedTypeName,
                aqn =>
                {
                    _selectedTypeName = aqn;
                    Repaint();
                });
        }
    }
}
```

</details>

### Фильтры окна

`TypeSelectorFilter` — структура. У `default` значение `Allow` равно `None`, в отличие от атрибута `[TypeSelector]`, где по умолчанию `All`. Задавайте режим явно, когда нужны абстрактные классы или интерфейсы.

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

`AdditionalTypes` — не дополнительное ограничение: используйте `Predicate`, если хотите сузить список. `currentAqn` задаёт текущую отметку; пустая строка выбирает пустую строку списка, а `null` оставляет выбор без отметки.

Пример окна, работающего с ассетами, есть в [EditorTools](../../Samples~/EditorTools/Documentation/README.ru.md).

## ComponentTypeSelector

Этот селектор меняет тип **самого компонента или ScriptableObject**, а не хранит тип для будущего создания. Добавьте поле в базовый класс: список ограничится конкретными типами, совместимыми с классом, где объявлено поле. Пункт `<None>` скрыт.

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

Сохраните базу в `EnemyBase.cs`. Создайте наследников в **отдельных файлах**, совпадающих с именами классов:

| FastEnemy.cs | ArmoredEnemy.cs |
|---|---|
| <pre lang="csharp"><code>using UnityEngine;&#10;&#10;public sealed class FastEnemy : EnemyBase&#10;&#123;&#10;    [SerializeField] private float _speed = 25f;&#10;&#10;    public override void Attack() =&gt;&#10;        Debug.Log($"Speed: &#123;_speed&#125;");&#10;&#125;</code></pre> | <pre lang="csharp"><code>using UnityEngine;&#10;&#10;public sealed class ArmoredEnemy : EnemyBase&#10;&#123;&#10;    [SerializeField] private int _armor = 10;&#10;&#10;    public override void Attack() =&gt;&#10;        Debug.Log($"Armor: &#123;_armor&#125;");&#10;&#125;</code></pre> |

Добавьте **FastEnemy** на GameObject, задайте **Health = 75** и через селектор выберите **ArmoredEnemy**. Общий `Health` сохранится, поле `Speed` исчезнет, появится `Armor`. Уникальные поля прежнего класса не следует считать сохранёнными для обратного переключения.

![Смена типа компонента через ComponentTypeSelector](../Images/aspid_fasttools_component_type_selector.gif)

Смена типа компонента через ComponentTypeSelector

Редактор заменяет `m_Script` через сериализацию Unity. Это не универсальная миграция данных между несовместимыми классами: проверьте общие и новые поля после переключения. Выбранный класс должен иметь собственный `MonoScript`, у которого `GetClass()` возвращает именно этот класс. Если подходящий скрипт не найден, тип не меняется, а Console получает предупреждение.

В UI Toolkit-инспекторе стандартная строка **Script** скрывается, пока присутствует селектор; IMGUI-инспектор продолжает рисовать её самостоятельно. Переключение и сценарий сохранения общих полей можно пройти в [примере Types](../../Samples~/Types/Documentation/README.ru.md).

## Если выбор не работает

| Симптом | Что проверить |
|---|---|
| Нужный класс отсутствует | Совместимость с базой и всеми ограничениями, `Allow`, `Hidden` и ошибки компиляции |
| В SerializableMonoScript нет типа, который есть в SerializableType | Есть ли отдельный файл скрипта и возвращает ли `MonoScript.GetClass()` нужный класс |
| После смены Category осталось старое значение | Ограничение меняет список кандидатов, а не переписывает зависимое поле |
| `<Missing>` при непустом имени | Не изменились ли класс, namespace или сборка; выберите существующий тип заново |
| Required не предупреждает о потерянном типе | Для строк и обёрток проверяется пустое имя, а не успешность его разрешения |
| Type выбран, но объект не появился | Хранение типа не создаёт экземпляр; используйте свой код создания или SerializeReference Selector |

## Продолжить

- [Types](../../Samples~/Types/Documentation/README.ru.md) — рабочая сцена с врагами, зависимым выбором и заменой компонента.
- [SerializeReference Selector](03-serialize-reference-selector.md) — экземпляры с данными, вложенные ссылки и восстановление.
- [SerializeReference Tooling](04-serialize-reference-tooling.md) — проектная проверка и CI.

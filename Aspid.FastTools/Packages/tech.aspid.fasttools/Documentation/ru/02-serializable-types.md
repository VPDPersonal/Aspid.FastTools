# Serializable Type System

Тип класса как обычное поле: Unity его сохраняет, а в инспекторе он выбирается из списка.

## Быстрый старт

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>[SerializeField]&#10;private string _primaryWeaponName;&#10;&#10;public System.Type PrimaryWeapon =&gt;&#10;    string.IsNullOrEmpty(&#10;        _primaryWeaponName)&#10;        ? null&#10;        : System.Type.GetType(&#10;            _primaryWeaponName, false);</code></pre> | <pre lang="csharp"><code>[TypeSelector(Allow = TypeAllow.None)]&#10;[SerializeField]&#10;private SerializableType&lt;Weapon&gt;&#10;    _primaryWeapon;&#10;&#10;public System.Type PrimaryWeapon =&gt;&#10;    _primaryWeapon;</code></pre> |

![Выбор сериализуемого типа в инспекторе](../Images/serializable-type-quick-start.gif)

Выбор сериализуемого типа в инспекторе

## SerializableType

<code lang="class-name">SerializableType</code> хранит assembly-qualified name — имя типа вместе со сборкой.

| Вариант | Ограничение выбора |
|---|---|
| <code lang="class-name">SerializableType</code> | Без базового ограничения |
| <code lang="csharp">SerializableType&lt;T&gt;</code> | Типы, совместимые с <code lang="class-name">T</code>, включая реализации интерфейса |

Оба варианта неявно преобразуются в <code lang="csharp">System.Type</code> и создаются из кода конструктором с аргументом <code lang="class-name">Type</code>; тип, несовместимый с <code lang="class-name">T</code>, вызывает <code lang="class-name">ArgumentException</code>:

```csharp
var primary = new SerializableType<Weapon>(typeof(Sword));
System.Type type = primary;

var empty = new SerializableType<Weapon>(null);
```

Потерянный тип — сохранённое имя, которое перестало находиться после переименования класса, namespace или сборки; инспектор показывает его как `<Missing …>` с этим именем.

| Свойство или вызов | <code lang="csharp">primary</code> | <code lang="csharp">empty</code> | Потерянный тип |
|---|---|---|---|
| <code lang="csharp">Type</code> | <code lang="csharp">typeof(Sword)</code> | <code lang="csharp">null</code> | <code lang="csharp">null</code> |
| <code lang="csharp">AssemblyQualifiedName</code> | Имя <code lang="class-name">Sword</code> со сборкой | <code lang="csharp">""</code> | Сохранённое имя |
| <code lang="csharp">BaseType</code> | <code lang="csharp">typeof(Weapon)</code> | <code lang="csharp">typeof(Weapon)</code> | <code lang="csharp">typeof(Weapon)</code> |
| <code lang="csharp">ToString()</code> | <code lang="csharp">"Sword"</code> | <code lang="csharp">""</code> | Сохранённое имя |

- У <code lang="class-name">SerializableType</code> без <code lang="class-name">T</code> свойство <code lang="csharp">BaseType</code> равно <code lang="csharp">typeof(object)</code>.
- <code lang="csharp">ToString()</code> найденного типа возвращает <code lang="csharp">Type.Name</code>: у generic-типа это <code lang="string">Amplify`1</code>, а не подпись из окна выбора.

> [!NOTE]
> Unity сериализует обёртку по объявленному типу поля. Если присвоить <code lang="csharp">SerializableType&lt;T&gt;</code> в поле <code lang="class-name">SerializableType</code>, выбранный тип переживёт загрузку, а ограничение <code lang="class-name">T</code> — нет. Объявляйте generic-вариант непосредственно у поля. Это же правило относится к <code lang="csharp">SerializableMonoScript&lt;T&gt;</code>.

## SerializableMonoScript

<code lang="class-name">SerializableMonoScript</code> связывает выбранный тип с ассетом скрипта и сохраняет выбор при согласованном переименовании или переносе класса и файла. Поле объявляется так же: <code lang="csharp">SerializableMonoScript&lt;Weapon&gt; _primaryWeapon</code>. Выберите тип в инспекторе или перетащите `.cs` из **Project**.

| Возможность | <code lang="class-name">SerializableType</code> | <code lang="class-name">SerializableMonoScript</code> |
|---|---|---|
| Выбор через окно поиска | Да | Да, только типы с подходящим скриптом |
| Generic-типы и типы, объявленные внутри другого класса | Да | Нет |
| Типы без своего `.cs` в проекте: из DLL и модулей Unity | Да | Нет |
| Обновление имени после переименования скрипта | Вручную | Из сохранённого MonoScript при сериализации |
| Создание из кода с <code lang="class-name">Type</code> | Публичный конструктор | Публичного конструктора нет |

Подходящий скрипт — файл runtime-сборки с классом верхнего уровня, не generic, чьё имя совпадает с именем файла: для <code lang="class-name">Sword</code> это `Sword.cs`.

- При переименовании сохраняйте ассет и его `.meta`.
- Если сохранённое имя уже не находится, редактор берёт тип из скрипта — в Play Mode тоже.
- Если Unity перестаёт распознавать класс, обёртка оставляет последнее известное имя.

> [!WARNING]
> В плеере <code lang="class-name">SerializableType</code> и <code lang="class-name">SerializableMonoScript</code> ищут тип по сохранённому имени, а managed code stripping не видит имён, записанных в сценах и ассетах. Начиная с **Managed Stripping Level** Low, класс, на который ссылается только инспектор, может не попасть в билд, и <code lang="csharp">.Type</code> вернёт <code lang="csharp">null</code>, хотя в редакторе тип находится. Пометьте такие классы <code lang="csharp">[Preserve]</code> (`UnityEngine.Scripting`) или перечислите их в `link.xml`. То же относится к <code lang="csharp">[TypeSelector]</code> на <code lang="csharp">string</code>.

## TypeSelector

Атрибут настраивает выбор у поля и добавляет селектор обычной строке.

| Поле | Результат выбора |
|---|---|
| <code lang="csharp">string</code> | Записывается assembly-qualified name |
| <code lang="class-name">SerializableType</code> / <code lang="class-name">SerializableMonoScript</code> | Настраивается выбор обёртки |
| <code lang="csharp">[SerializeReference]</code> | Создаётся экземпляр выбранной реализации — см. [SerializeReference Selector](03-serialize-reference-selector.md) |

### Ограничения и коллекции

```csharp
public interface ITwoHanded { }

public abstract class Weapon { }
public abstract class MeleeWeapon : Weapon { }
public abstract class RangedWeapon : Weapon { }

public sealed class Sword : MeleeWeapon { }
public sealed class Axe : MeleeWeapon, ITwoHanded { }
public sealed class Bow : RangedWeapon, ITwoHanded { }
```

```csharp
[TypeSelector(typeof(Weapon), Allow = TypeAllow.None)]
[SerializeField] private string _backupWeaponName;

[TypeSelector(typeof(ITwoHanded), Allow = TypeAllow.None)]
[SerializeField] private SerializableType<MeleeWeapon> _heavyWeapon;

[TypeSelector(Allow = TypeAllow.None)]
[SerializeField] private SerializableType<Weapon>[] _loadout;
```

<code lang="csharp">_heavyWeapon</code> предлагает только <code lang="class-name">Axe</code>: <code lang="class-name">Sword</code> не реализует <code lang="class-name">ITwoHanded</code>, а <code lang="class-name">Bow</code> не наследует <code lang="class-name">MeleeWeapon</code>. Ограничения действуют одновременно (**И**) на полях любого вида. Массивы и списки получают выбор для каждого элемента.

У <code lang="csharp">[SerializeReference]</code> первым ограничением служит тип поля, поэтому и здесь доступен только <code lang="class-name">Axe</code>:

```csharp
[TypeSelector(typeof(ITwoHanded))]
[SerializeReference] private MeleeWeapon _heldWeapon;
```

Чтобы разрешить определённый набор классов, дайте им общий интерфейс или базовый класс и укажите его: перечисление самих классов (<code lang="csharp">typeof(Sword), typeof(Axe)</code>) оставит список пустым, и анализатор `AFT0009` об этом предупредит. Подробнее — [настройка селектора экземпляров](03-serialize-reference-selector.md#какие-классы-в-списке).

<details>
<summary>Формы аргументов TypeSelector</summary>

```csharp
[TypeSelector]
[TypeSelector(typeof(Weapon))]
[TypeSelector(typeof(MeleeWeapon), typeof(ITwoHanded))]
[TypeSelector("Namespace.TypeName, AssemblyName")]
[TypeSelector(nameof(_weaponClass))]
```

- На поле ставится один <code lang="csharp">[TypeSelector]</code>.
- Аргументы — <code lang="class-name">Type</code> или <code lang="csharp">string</code>: один, несколько через запятую или массив; в одном атрибуте они не смешиваются.
- Без аргументов атрибут не добавляет ограничений.
- Строка сначала ищется как член класса, где объявлено поле, затем — как имя типа.

</details>

### Свойства

| Свойство | По умолчанию | Поведение |
|---|---|---|
| <code lang="csharp">Allow</code> | <code lang="csharp">TypeAllow.All</code> | <code lang="csharp">Abstract</code> добавляет абстрактные классы, <code lang="csharp">Interface</code> — интерфейсы; <code lang="csharp">All</code> включает обе категории, <code lang="csharp">None</code> исключает их. На <code lang="csharp">[SerializeReference]</code> игнорируется |
| <code lang="csharp">Required</code> | <code lang="csharp">false</code> | Предупреждает о пустом имени типа или <code lang="csharp">null</code> в managed-ссылке |

В инспекторе runtime-объекта селектор не предлагает типы из editor-only сборок (`UnityEditor`, asmdef только для Editor и папки `Editor`): в билде плеера они не найдутся.

- Поля editor-only объектов, например окон и настроек редактора, предлагают любые типы.
- Правило определяется классом объекта, поэтому поле runtime-объекта под <code lang="csharp">#if UNITY_EDITOR</code> их тоже не предлагает.

### Обязательное поле

```csharp
[TypeSelector(typeof(Weapon), Required = true)]
[SerializeField] private string _secondaryWeapon;
```

![Пустое обязательное поле показывает предупреждение под селектором](../Images/type-selector-required.png)

Пустое обязательное поле показывает предупреждение под селектором

С <code lang="csharp">Required = true</code> пункт `<None>` остаётся доступным. У строки или обёртки проверяется пустое сохранённое имя; потерянный тип с непустым именем эту проверку проходит.

Настройка проверки по всему проекту и в CI описана в разделе [проверки обязательных полей](04-serialize-reference-tooling.md#где-проверяются-обязательные-поля).

### Ограничение из другого поля

Передайте <code lang="csharp">nameof(...)</code>, чтобы текущее значение поля или свойства управляло списком кандидатов:

```csharp
[SerializeField] private SerializableType<Weapon> _weaponClass;

[TypeSelector(nameof(_weaponClass), Allow = TypeAllow.None)]
[SerializeField] private string _weaponName;
```

Выберите <code lang="class-name">MeleeWeapon</code> в **Weapon Class** — **Weapon Name** предложит <code lang="class-name">Sword</code> и <code lang="class-name">Axe</code>. Смена ограничения не очищает ранее выбранное имя.

| Источник ограничения | Поддержка |
|---|---|
| <code lang="csharp">System.Type</code> | Один тип |
| <code lang="csharp">string</code> | Имя типа, разрешаемое через <code lang="csharp">Type.GetType</code> |
| <code lang="class-name">SerializableType</code>, <code lang="class-name">SerializableMonoScript</code> и их generic-варианты | Разрешённое значение <code lang="csharp">.Type</code> |
| Массив этих значений | Несколько ограничений одновременно; <code lang="csharp">List&lt;T&gt;</code> не поддерживается |

- Источник — нестатическое поле или читаемое свойство класса, где объявлено поле с атрибутом, включая унаследованные; индексаторы не поддерживаются.
- У поля внутри <code lang="csharp">[Serializable]</code>-класса или элемента списка источник читается из того же экземпляра.
- Пустой или неразрешённый источник не добавляет ограничения.
- У generic-обёртки её собственный <code lang="class-name">T</code> продолжает ограничивать выбор.

Ошибки в строковых аргументах находят анализаторы:

- `AFT0006` — строка из одного слова, но такого члена у класса нет;
- `AFT0007` — член не может задать базовые типы;
- `AFT0008` — строка не похожа на имя типа.

Если имя типа записано верно, но такой тип не загружен, предупреждение показывает инспектор.

![Ограничение не разрешилось — инспектор показывает предупреждение под полем](../Images/type-selector-constraint-warning.png)

Ограничение не разрешилось — инспектор показывает предупреждение под полем

## TypeSelectorDisplay

<code lang="csharp">[TypeSelectorDisplay]</code> на классе меняет только его строку в окне выбора:

| Параметр на <code lang="class-name">DamageModifier</code> | В окне выбора |
|---|---|
| <code lang="csharp">Name = "Damage ×"</code> | Damage × в списке и в закрытом поле; поиск находит и по DamageModifier |
| <code lang="csharp">Group = "Combat/Modifiers"</code> | Combat → Modifiers → Damage × вместо namespace |
| <code lang="csharp">Tooltip = "Scales incoming damage"</code> | Подсказка при наведении |
| <code lang="csharp">Icon = "d_ScriptableObject Icon"</code> | Иконка по имени `EditorGUIUtility.IconContent`, по пути ассета от `Assets/` или `Packages/` с расширением либо по пути в `Resources` без расширения |
| <code lang="csharp">Hidden = true</code> | Нет в списке; присваивание из кода и уже сохранённое значение работают |

Подклассы настроек не наследуют.

![Имя Damage ×, иконка и группа Combat/Modifiers в окне выбора](../Images/type-selector-display.png)

Имя Damage ×, иконка и группа Combat/Modifiers в окне выбора

> [!NOTE]
> <code lang="csharp">[TypeSelector]</code> и <code lang="csharp">[TypeSelectorDisplay]</code> помечены <code lang="csharp">[Conditional("UNITY_EDITOR")]</code>. В классах из внешней DLL, собранной без этого символа, их настроек нет, включая <code lang="csharp">Hidden</code>.

## TypeSelectorWindow

Окно группирует типы по namespace или <code lang="csharp">Group</code> и различает одинаковые имена по сборкам. Через <code lang="class-name">TypeSelectorWindow</code> его можно открыть из своего инспектора или окна редактора.

![Избранные и недавние типы на корневой странице селектора](../Images/type-selector-window.png)

Избранные и недавние типы на корневой странице селектора

| Действие | Управление |
|---|---|
| Перемещение / выбор | Стрелки вверх и вниз / Enter |
| Вход в группу / возврат | Стрелка вправо / стрелка влево или хлебные крошки |
| Поиск | Начните печатать |
| Переключение избранного | Space или звёздочка при наведении |
| Очистка значения | `<None>` |
| Закрытие | Escape; при открытом поиске первые нажатия очищают и сворачивают его |

Раздел **Favorites** и длина истории **Recent** (0 скрывает её) настраиваются во вкладке **Settings** окна FastTools, там же оба списка очищаются. Шестерёнка в окне выбора открывает эту вкладку.

### Generic-типы

При выборе открытого generic-типа окно предлагает выбрать аргументы и возвращает сконструированный закрытый тип:

```csharp
public abstract class CombatModifier { }
public abstract class StatusEffect { }
public sealed class Burning : StatusEffect { }

public sealed class Amplify<T> : CombatModifier
    where T : StatusEffect { }

[SerializeField] private SerializableType<CombatModifier> _modifier;
```

Выберите <code lang="csharp">Amplify&lt;T&gt;</code> в <code lang="csharp">_modifier</code> — окно предложит наследников <code lang="class-name">StatusEffect</code>, а после выбора <code lang="class-name">Burning</code> запишет <code lang="csharp">Amplify&lt;Burning&gt;</code>.

![Выбор аргумента generic-типа в селекторе](../Images/type-selector-generic.gif)

Выбор аргумента generic-типа в селекторе

- Generic-аргумент может сам быть generic-типом: окно сначала спросит его аргументы.
- Если все аргументы выводятся из типа поля, закрытый тип возвращается сразу.
- Аргумент должен удовлетворять ограничениям generic-параметра; интерфейсы, абстрактные классы и скрытые типы в аргументах не предлагаются.
- Для <code lang="csharp">[SerializeReference]</code> аргументы выводятся из типа поля — см. [SerializeReference Selector](03-serialize-reference-selector.md#какие-классы-в-списке).

### Открытие из кода

<code lang="csharp">screenRect</code> — прямоугольник кнопки в **экранных координатах**, <code lang="csharp">selectedTypeName</code> — строка текущего выбора:

```csharp
using Aspid.FastTools.Types;
using Aspid.FastTools.Types.Editors;

TypeSelectorWindow.Show(
    screenRect,
    new TypeSelectorFilter
    {
        Types = new[] { typeof(Weapon) },
        Allow = TypeAllow.None
    },
    currentAqn: selectedTypeName,
    onSelected: aqn => selectedTypeName = aqn);
```

Обработчик получает assembly-qualified name или <code lang="csharp">null</code> при выборе `<None>`; закрытие окна без выбора его не вызывает.

<code lang="csharp">currentAqn</code> задаёт текущую отметку:

- пустая строка (значение по умолчанию) отмечает `<None>`;
- <code lang="csharp">null</code> оставляет выбор без отметки;
- имя, которого нет в списке, тоже остаётся без отметки, поэтому Enter сразу после открытия не сотрёт сохранённое имя потерянного типа.

### Фильтры окна

<code lang="class-name">TypeSelectorFilter</code> — структура. У её <code lang="csharp">default</code>:

- пустой <code lang="csharp">Types</code> пропускает любые типы;
- <code lang="csharp">Allow</code> равен <code lang="csharp">None</code>, а не <code lang="csharp">All</code>, как у <code lang="csharp">[TypeSelector]</code>: задавайте <code lang="csharp">Allow</code> явно, когда нужны абстрактные классы или интерфейсы.

<details>
<summary>Свойства фильтра окна</summary>

| Свойство | Назначение |
|---|---|
| <code lang="csharp">Types</code> | Все базовые типы, которым должен соответствовать кандидат |
| <code lang="csharp">Allow</code> | Разрешённые категории: абстрактные классы и интерфейсы |
| <code lang="csharp">Predicate</code> | Дополнительное условие после проверки типа и категории |
| <code lang="csharp">AdditionalTypes</code> | Кандидаты, обходящие <code lang="csharp">Types</code>, <code lang="csharp">Allow</code> и <code lang="csharp">Predicate</code>; фильтр <code lang="csharp">Hidden</code> сохраняется |
| <code lang="csharp">ArgumentFilter</code> | Дополнительный фильтр аргументов, выбираемых вручную |
| <code lang="csharp">InferredArgumentFilter</code> | Фильтр аргументов, выведенных из типа поля |
| <code lang="csharp">IncludeHidden</code> | Показывать типы с <code lang="csharp">Hidden = true</code>, в том числе в аргументах |
| <code lang="csharp">HideNoneOption</code> | Скрыть `<None>` на корневой странице |

</details>

## Пример в пакете

Выбор типов врагов и паттерна расстановки в инспекторе показан в примере [Types](../../Samples~/Types/Documentation/README.ru.md), а окно выбора, открытое из редакторского кода, — в [EditorTools](../../Samples~/EditorTools/Documentation/README.ru.md).

![Волна обычных и элитных врагов движется к центру.](../../Samples~/Types/Documentation/Images/demo.gif)

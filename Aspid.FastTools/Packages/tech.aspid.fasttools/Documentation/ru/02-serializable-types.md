# Serializable Type System

Тип класса как обычное поле: Unity его сохраняет, а в инспекторе он выбирается из списка.

## Быстрый старт

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>[SerializeField]&#10;private string _primaryWeaponName;&#10;&#10;public System.Type PrimaryWeapon =&gt;&#10;    string.IsNullOrEmpty(&#10;        _primaryWeaponName)&#10;        ? null&#10;        : System.Type.GetType(&#10;            _primaryWeaponName, false);</code></pre> | <pre lang="csharp"><code>[TypeSelector(Allow = TypeAllow.None)]&#10;[SerializeField]&#10;private SerializableType&lt;Weapon&gt;&#10;    _primaryWeapon;&#10;&#10;public System.Type PrimaryWeapon =&gt;&#10;    _primaryWeapon;</code></pre> |

![Выбор сериализуемого типа в инспекторе](../Images/serializable-type-quick-start.gif)

## SerializableType

<code lang="class-name">SerializableType</code> хранит assembly-qualified name — имя типа вместе со сборкой.

| Вариант | Ограничение выбора |
|---|---|
| <code lang="class-name">SerializableType</code> | Без базового ограничения |
| <code lang="class-name">SerializableType&lt;T&gt;</code> | Типы, совместимые с <code lang="class-name">T</code> |

Из кода обёртку создаёт конструктор; тип, несовместимый с <code lang="class-name">T</code>, вызывает <code lang="class-name">ArgumentException</code>:

```csharp
var primary = new SerializableType<Weapon>(typeof(Sword));
System.Type type = primary;

var empty = new SerializableType<Weapon>(null);
```

<code lang="csharp">ToString()</code> найденного типа возвращает <code lang="csharp">Type.Name</code>: у generic-типа это <code lang="string">Amplify`1</code>, а не подпись из окна выбора.

### Потерянный тип

Сохранённое имя, которое перестало находиться после переименования класса, namespace или сборки. Инспектор показывает его как `<Missing …>`.

![Потерянный тип Game.Combat.Spear в поле инспектора](../Images/serializable-type-missing.png)

- <code lang="csharp">Type</code> возвращает <code lang="csharp">null</code>.
- <code lang="csharp">AssemblyQualifiedName</code> и <code lang="csharp">ToString()</code> возвращают сохранённое имя, по которому тип можно восстановить.

> [!WARNING]
> В плеере <code lang="class-name">SerializableType</code> и <code lang="class-name">SerializableMonoScript</code> ищут тип по имени — это строка в данных сцены, префаба или ассета. Managed code stripping такие строки не разбирает, поэтому начиная с **Managed Stripping Level** Low класс, выбранный только в инспекторе, может не попасть в билд, и <code lang="csharp">.Type</code> вернёт <code lang="csharp">null</code>, хотя в редакторе тип находится. Пометьте такие классы <code lang="csharp">[Preserve]</code> (<code lang="csharp">UnityEngine.Scripting</code>) или перечислите их в `link.xml`. То же относится к <code lang="csharp">[TypeSelector]</code> на <code lang="csharp">string</code>.

## SerializableMonoScript

То же поле, но выбор переживает переименование класса: поле помнит сам ассет скрипта. Тип выбирают в инспекторе или перетаскивают на поле `.cs` из **Project**.

| После переименования `Sword.cs` → `Blade.cs` | <code lang="class-name">SerializableType</code> | <code lang="class-name">SerializableMonoScript</code> |
|---|---|---|
| <code lang="csharp">.Type</code> | <code lang="csharp">null</code> — [потерянный тип](#потерянный-тип) | <code lang="class-name">Blade</code>, в Play Mode тоже |
| Сохранённое имя | <code lang="class-name">Sword</code> | <code lang="class-name">Blade</code>, как только ассет пересохранят |

Ограничения:

- в списке только классы со своим `.cs`: верхнего уровня, не generic, с именем как у файла;
- generic-типы, вложенные классы и типы из DLL выбрать нельзя;
- публичного конструктора нет, из кода поле не создать;
- если переименовать класс без файла или файл вне Unity без `.meta`, связь теряется и поле показывает потерянный тип.

## TypeSelector

| Поле | Результат выбора |
|---|---|
| <code lang="csharp">string</code> | Записывается assembly-qualified name |
| <code lang="class-name">SerializableType</code> / <code lang="class-name">SerializableMonoScript</code> | Настраивается выбор обёртки |
| <code lang="csharp">[SerializeReference]</code> | Создаётся экземпляр выбранной реализации — см. [SerializeReference Selector](03-serialize-reference-selector.md) |

### Какие типы в списке

```csharp
public interface ITwoHanded { }

public abstract class Weapon { }
public abstract class MeleeWeapon : Weapon { }
public abstract class RangedWeapon : Weapon { }

public sealed class Sword : MeleeWeapon { }
public sealed class Axe : MeleeWeapon, ITwoHanded { }
public sealed class Bow : RangedWeapon, ITwoHanded { }
```

| Поле | <code lang="csharp">[TypeSelector(…, Allow = TypeAllow.None)]</code> | Типы в списке |
|---|---|---|
| <code lang="csharp">string</code> | <code lang="csharp">typeof(Weapon)</code> | Axe, Bow, Sword |
| <code lang="class-name">SerializableType&lt;MeleeWeapon&gt;</code> | <code lang="csharp">typeof(ITwoHanded)</code> | Axe — единственный <code lang="class-name">MeleeWeapon</code> с <code lang="class-name">ITwoHanded</code> |
| <code lang="class-name">SerializableType&lt;Weapon&gt;</code> | <code lang="csharp">"MeleeWeapon, Assembly-CSharp"</code> | Axe, Sword |
| <code lang="class-name">SerializableType&lt;Weapon&gt;</code> | <code lang="csharp">typeof(Sword), typeof(Axe)</code> | Пусто, `AFT0009` предупредит: ни один класс не наследует оба |
| <code lang="class-name">SerializableType&lt;Weapon&gt;[]</code> | без аргумента | Axe, Bow, Sword у каждого элемента |

Чтобы разрешить набор классов, дайте им общий интерфейс или базовый класс и укажите его.

### Свойства

| Свойство | По умолчанию | Поведение |
|---|---|---|
| <code lang="csharp">Allow</code> | <code lang="csharp">TypeAllow.All</code> | Пускает в список абстрактные классы (<code lang="csharp">Abstract</code>), интерфейсы (<code lang="csharp">Interface</code>), оба вида или ни один. На <code lang="csharp">[SerializeReference]</code> игнорируется |
| <code lang="csharp">Required</code> | <code lang="csharp">false</code> | Предупреждает о пустом имени типа или <code lang="csharp">null</code> в managed-ссылке |

> [!NOTE]
> В инспекторе runtime-объекта селектор не предлагает типы из editor-only сборок (`UnityEditor`, asmdef только для Editor и папки `Editor`): в билде плеера они не найдутся. Правило определяется классом объекта, поэтому поле runtime-объекта под <code lang="csharp">#if UNITY_EDITOR</code> их тоже не предлагает.

### Обязательное поле

```csharp
[TypeSelector(typeof(Weapon), Required = true)]
[SerializeField] private string _secondaryWeapon;
```

![Пустое обязательное поле показывает предупреждение под селектором](../Images/type-selector-required.png)

С <code lang="csharp">Required = true</code> пункт `<None>` остаётся доступным. У строки или обёртки проверяется пустое сохранённое имя; потерянный тип с непустым именем эту проверку проходит.

Настройка проверки по всему проекту и в CI описана в разделе [проверки обязательных полей](04-serialize-reference-tooling.md#что-проверяет-каждый-запуск).

### Ограничение из другого поля

Передайте <code lang="csharp">nameof(...)</code>, чтобы текущее значение поля или свойства управляло списком кандидатов:

```csharp
[SerializeField] private SerializableType<Weapon> _weaponClass;

[TypeSelector(nameof(_weaponClass), Allow = TypeAllow.None)]
[SerializeField] private string _weaponName;
```

Выберите <code lang="class-name">MeleeWeapon</code> в **Weapon Class** — **Weapon Name** предложит <code lang="class-name">Sword</code> и <code lang="class-name">Axe</code>. Смена ограничения не очищает ранее выбранное имя.

![Выбор MeleeWeapon в Weapon Class оставляет в Weapon Name только Axe и Sword](../Images/type-selector-member-constraint.gif)

| Источник ограничения | Что ограничивает |
|---|---|
| <code lang="class-name">System.Type</code> | Один тип |
| <code lang="csharp">string</code> | Имя типа, разрешаемое через <code lang="csharp">Type.GetType()</code> |
| <code lang="class-name">SerializableType</code> / <code lang="class-name">SerializableMonoScript</code> | Разрешённое значение <code lang="csharp">.Type</code> |
| Массив этих значений | Несколько ограничений одновременно; <code lang="class-name">List&lt;T&gt;</code> не поддерживается |

- Строка сначала ищется среди нестатических полей и читаемых свойств класса, где объявлено поле, включая унаследованные, затем — как имя типа.
- У поля внутри <code lang="csharp">[Serializable]</code>-класса или элемента списка источник читается из того же экземпляра.
- Пока источник пуст или не разрешился, ограничения от него нет: строковый **Weapon Name** предложит все неабстрактные классы проекта. У обёртки остаётся её собственный <code lang="class-name">T</code>.

### Ошибки в строковых аргументах

```csharp
[TypeSelector("Spear, Assembly-CSharp")]
[SerializeField] private string _weaponName;
```

Ошибки в строках находят анализаторы:

- `AFT0006` — строка из одного слова, но такого члена у класса нет;
- `AFT0007` — член не может задать базовые типы;
- `AFT0008` — строка не похожа на имя типа.

Если имя типа записано верно, но такой тип не загружен, как <code lang="class-name">Spear</code> выше, предупреждение показывает инспектор:

![Ограничение не разрешилось — инспектор показывает предупреждение под полем](../Images/type-selector-constraint-warning.png)

## TypeSelectorDisplay

<code lang="csharp">[TypeSelectorDisplay]</code> на классе меняет только его строку в окне выбора:

| Параметр на <code lang="class-name">Sword</code> | В окне выбора |
|---|---|
| <code lang="csharp">Name = "Longsword"</code> | Longsword в списке и в закрытом поле; поиск находит и по Sword |
| <code lang="csharp">Group = "Weapons/Melee"</code> | Weapons → Melee → Longsword вместо namespace |
| <code lang="csharp">Tooltip = "A balanced blade"</code> | Подсказка при наведении |
| <code lang="csharp">Icon = "d_ScriptableObject Icon"</code> | Иконка: встроенная по имени, ассет по пути с расширением или из `Resources` без расширения |
| <code lang="csharp">Hidden = true</code> | Нет в списке; присваивание из кода и уже сохранённое значение работают |

Подклассы настроек не наследуют.

![Имя Longsword, иконка и группа Weapons/Melee в окне выбора](../Images/type-selector-display.png)

> [!NOTE]
> <code lang="csharp">[TypeSelector]</code> и <code lang="csharp">[TypeSelectorDisplay]</code> помечены <code lang="csharp">[Conditional("UNITY_EDITOR")]</code>. В классах из внешней DLL, собранной без этого символа, их настроек нет, включая <code lang="csharp">Hidden</code>.

## Окно выбора

Окно группирует типы по namespace или <code lang="csharp">Group</code> и различает одинаковые имена по сборкам.

![Избранные и недавние типы на корневой странице окна выбора](../Images/type-selector-window.png)

На корневой странице окно держит типы, которые нужны чаще других:

- **Favorites** — избранное. Чтобы добавить тип, нажмите звёздочку справа от его строки или Space, когда строка выделена.
- **Recent** — последние выбранные типы.

Показ Favorites и длина Recent (0 скрывает раздел) настраиваются во вкладке **Settings** окна FastTools. Её открывает шестерёнка в правом нижнем углу окна выбора, там же оба списка очищаются.

### Generic-типы

При выборе открытого generic-типа окно предлагает выбрать аргументы и возвращает сконструированный закрытый тип:

```csharp
public abstract class Enchantment { }
public sealed class Fire : Enchantment { }
public sealed class Frost : Enchantment { }

public sealed class Enchanted<T> : MeleeWeapon
    where T : Enchantment { }
```

Выберите <code lang="class-name">Enchanted&lt;T&gt;</code> в поле <code lang="csharp">_primaryWeapon</code> — окно предложит наследников <code lang="class-name">Enchantment</code>, а после выбора <code lang="class-name">Fire</code> запишет <code lang="class-name">Enchanted&lt;Fire&gt;</code>.

![Выбор аргумента generic-типа в окне выбора](../Images/type-selector-generic.gif)

- Generic-аргумент может сам быть generic-типом: окно сначала спросит его аргументы.
- Если все аргументы выводятся из типа поля, закрытый тип возвращается сразу.
- Интерфейсы, абстрактные классы и скрытые типы в аргументах не предлагаются.

## TypeSelectorWindow

<code lang="class-name">TypeSelectorWindow</code> открывает то же окно из кастомного инспектора или окна редактора, например по кнопке UI Toolkit:

```csharp
using Aspid.FastTools.Types.Editors;

var button = new Button { text = "Select weapon" };
button.clicked += () => TypeSelectorWindow.Show(
    GUIUtility.GUIToScreenRect(button.worldBound),
    new TypeSelectorFilter { Types = new[] { typeof(Weapon) } },
    currentAqn: selectedTypeName,
    onSelected: aqn => selectedTypeName = aqn);
```

Обработчик получает assembly-qualified name или <code lang="csharp">null</code> при выборе `<None>`; закрытие окна без выбора его не вызывает.

| <code lang="csharp">currentAqn</code> | Отмечено при открытии |
|---|---|
| Имя типа из списка | Этот тип; окно открывается в его группе |
| <code lang="csharp">""</code> (по умолчанию) | `<None>` |
| <code lang="csharp">null</code> | Ничего |
| Имя, которого нет в списке | Ничего: Enter сразу после открытия не сотрёт сохранённое имя |

### TypeSelectorFilter

| Свойство | По умолчанию | Назначение |
|---|---|---|
| <code lang="csharp">Types</code> | пусто — любые типы | Базовые типы; кандидат совместим с каждым |
| <code lang="csharp">Allow</code> | <code lang="csharp">None</code>; у <code lang="csharp">[TypeSelector]</code> — <code lang="csharp">All</code> | Разрешённые категории: абстрактные классы и интерфейсы |
| <code lang="csharp">Predicate</code> | <code lang="csharp">null</code> | Условие поверх <code lang="csharp">Types</code> и <code lang="csharp">Allow</code> |
| <code lang="csharp">AdditionalTypes</code> | <code lang="csharp">null</code> | Кандидаты, обходящие <code lang="csharp">Types</code>, <code lang="csharp">Allow</code> и <code lang="csharp">Predicate</code>; фильтр <code lang="csharp">Hidden</code> сохраняется |
| <code lang="csharp">ArgumentFilter</code> | <code lang="csharp">null</code> | Условие для аргументов, выбираемых вручную, сверх ограничений <code lang="csharp">where</code> |
| <code lang="csharp">InferredArgumentFilter</code> | <code lang="csharp">null</code> | Фильтр аргументов, выведенных из <code lang="csharp">Types</code>; получает generic-определение, параметр и аргумент |
| <code lang="csharp">IncludeHidden</code> | <code lang="csharp">false</code> | Показывать типы с <code lang="csharp">Hidden = true</code>, в том числе в аргументах |
| <code lang="csharp">HideNoneOption</code> | <code lang="csharp">false</code> | Скрыть `<None>` на корневой странице |

## Пример в пакете

Выбор типов врагов и паттерна расстановки в инспекторе показан в примере [Types](../../Samples~/Types/Documentation/README.ru.md), а окно выбора, открытое из редакторского кода, — в [EditorTools](../../Samples~/EditorTools/Documentation/README.ru.md).

![Волна обычных и элитных врагов движется к центру.](../../Samples~/Types/Documentation/Images/demo.gif)

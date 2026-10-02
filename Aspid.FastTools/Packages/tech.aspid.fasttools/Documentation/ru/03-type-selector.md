# TypeSelector

Настраивает доступные типы и их отображение в окне выбора.

## Быстрый старт

```csharp
[TypeSelector(typeof(Weapon), Allow = TypeAllow.None)]
[SerializeField] private string _weaponName;
```

Поле предлагает конкретные классы оружия и сохраняет имя выбранного типа.

## Где применяется

| Поле | Результат выбора |
|---|---|
| <code lang="csharp">string</code> | Записывается assembly-qualified name |
| [Serializable Types](02-serializable-types.md) | Настраивается выбор обёртки |
| <code lang="csharp">[SerializeReference]</code> | Создаётся экземпляр выбранной реализации — см. [SerializeReference Selector](04-serialize-reference-selector.md) |

## Какие типы в списке

Все ограничения атрибута действуют **одновременно**. У обёртки дополнительно учитывается <code lang="class-name">T</code>, у <code lang="csharp">[SerializeReference]</code> — тип поля.

```csharp
public interface ITwoHanded { }

public abstract class Weapon { }
public abstract class MeleeWeapon : Weapon { }
public abstract class RangedWeapon : Weapon { }

public sealed class Sword : MeleeWeapon { }
public sealed class Axe : MeleeWeapon, ITwoHanded { }
public sealed class Bow : RangedWeapon, ITwoHanded { }
```

| Ограничение | Результат |
|---|---|
| <code lang="csharp">typeof(Weapon)</code> | Типы, совместимые с <code lang="class-name">Weapon</code> |
| <code lang="csharp">typeof(Weapon), typeof(ITwoHanded)</code> | Оружие, реализующее <code lang="class-name">ITwoHanded</code> |
| <code lang="csharp">typeof(Sword), typeof(Axe)</code> | Пустой список: класс не может наследовать оба; анализатор `AFT0009` сообщает об этом |

Чтобы разрешить несколько классов, укажите их общий базовый класс или интерфейс. На массиве или списке ограничение применяется к каждому элементу.

## Свойства

| Свойство | По умолчанию | Поведение |
|---|---|---|
| <code lang="csharp">Allow</code> | <code lang="csharp">TypeAllow.All</code> | Пускает в список абстрактные классы (<code lang="csharp">Abstract</code>), интерфейсы (<code lang="csharp">Interface</code>), оба вида или ни один. На <code lang="csharp">[SerializeReference]</code> игнорируется |
| <code lang="csharp">Required</code> | <code lang="csharp">false</code> | Предупреждает о пустом имени типа или <code lang="csharp">null</code> в managed-ссылке |

> [!NOTE]
> В инспекторе runtime-объекта селектор не предлагает типы из editor-only сборок (`UnityEditor`, asmdef только для Editor и папки `Editor`): в билде плеера они не найдутся. Правило определяется классом объекта, поэтому поле runtime-объекта под <code lang="csharp">#if UNITY_EDITOR</code> их тоже не предлагает.

## Обязательное поле

```csharp
[TypeSelector(typeof(Weapon), Required = true)]
[SerializeField] private string _secondaryWeapon;
```

![Пустое обязательное поле показывает предупреждение под селектором](../Images/type-selector-required.png)

С <code lang="csharp">Required = true</code> пункт `<None>` остаётся доступным. У строки или обёртки проверяется пустое сохранённое имя; потерянный тип с непустым именем эту проверку проходит.

Настройка проверки по всему проекту и в CI описана в разделе [проверки обязательных полей](07-serialize-reference-validation.md#что-проверяет-каждый-запуск).

## Ограничение из другого поля

Передайте <code lang="csharp">nameof(...)</code>, чтобы текущее значение поля или свойства управляло списком кандидатов:

```csharp
[SerializeField] private SerializableType<Weapon> _weaponClass;

[TypeSelector(nameof(_weaponClass), Allow = TypeAllow.None)]
[SerializeField] private string _weaponName;
```

Выберите <code lang="class-name">MeleeWeapon</code> в **Weapon Class** — **Weapon Name** предложит его конкретных наследников. Смена ограничения не очищает ранее выбранное имя.

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

## Ошибки в строковых аргументах

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

public sealed class Enchanted<T> : Weapon
    where T : Enchantment { }
```

Выберите <code lang="class-name">Enchanted&lt;T&gt;</code> в поле <code lang="class-name">SerializableType&lt;Weapon&gt;</code> — окно предложит наследников <code lang="class-name">Enchantment</code>, а после выбора <code lang="class-name">Fire</code> запишет <code lang="class-name">Enchanted&lt;Fire&gt;</code>.

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
    screenRect: GUIUtility.GUIToScreenRect(button.worldBound),
    filter: new TypeSelectorFilter
    {
        Types = new[] { typeof(Weapon) }
    },
    currentAqn: selectedTypeName,
    onSelected: aqn => selectedTypeName = aqn);
```

Обработчик получает assembly-qualified name или <code lang="csharp">null</code> при выборе `<None>`; закрытие окна без выбора его не вызывает.

- <code lang="csharp">currentAqn</code> отмечает свой тип при открытии, а <code lang="csharp">""</code> — `<None>`; <code lang="csharp">null</code> или имя, которого нет в списке, не отмечают ничего, поэтому Enter сразу после открытия не сотрёт сохранённое имя.
- <code lang="csharp">TypeSelectorFilter.Allow</code> по умолчанию <code lang="csharp">TypeAllow.None</code>, в отличие от <code lang="csharp">[TypeSelector]</code>: окно выше предлагает только конкретное оружие.

Свойства фильтра и параметры окна — в справочнике API: [TypeSelectorFilter](https://vpdpersonal.github.io/Aspid.FastTools/ru/api/Aspid.FastTools.Types.Editors.TypeSelectorFilter), [TypeSelectorWindow](https://vpdpersonal.github.io/Aspid.FastTools/ru/api/Aspid.FastTools.Types.Editors.TypeSelectorWindow).

## Пример в пакете

Окно выбора из редакторского кода показано в примере [EditorTools](../../Samples~/EditorTools/Documentation/README.ru.md).

# VisualElement Extensions

В UI Toolkit каждое свойство элемента — отдельная инструкция: четыре строки на отступы, переменная на каждый дочерний элемент и `Add` в конце. FastTools превращает свойства, стили и события в методы, которые возвращают сам элемент, поэтому заголовок инспектора собирается одним выражением, а `SetPaddingX(12)` заменяет пару `style.paddingLeft` и `style.paddingRight`.

## Быстрый старт

Примеры на этой странице собирают инспектор ассета `AbilityConfig` из [примера EditorTools](../../Samples~/EditorTools/Documentation/README.ru.md). Добавьте `using Aspid.FastTools.UIElements;`:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>var title = new Label("Ability Config");&#10;title.style.fontSize = 14;&#10;&#10;var header = new VisualElement();&#10;header.style.paddingLeft = 12;&#10;header.style.paddingRight = 12;&#10;header.style.paddingTop = 10;&#10;header.style.paddingBottom = 10;&#10;header.Add(title);</code></pre> | <pre lang="csharp"><code>var header = new VisualElement()&#10;    .SetPaddingX(12)&#10;    .SetPaddingY(10)&#10;    .AddChild(new Label("Ability Config")&#10;        .SetFontSize(14));</code></pre> |

Сеттер возвращает тот же тип: `new Button().SetText("Create")` — это `Button`. `AddChild` и другие операции с дочерними элементами возвращают родителя. Методы на `style`, `textEdition` и `textSelection` возвращают этот объект, а не элемент.

## Имена методов

Имена методов строятся по одному правилу:

| Unity | FastTools | Примеры |
|---|---|---|
| свойство `x` | `SetX(value)` | `tooltip` → `SetTooltip` |
| свойство `isX` | `SetX(value)`, без `is` | `isDelayed` → `SetDelayed` |
| свойство стиля `style.x` | `SetX(value)` на элементе и на `style` | `style.fontSize` → `SetFontSize` |
| событие `x` | `AddX` / `RemoveX` | `clicked` → `AddClicked` |
| свойство-делегат `x` | `SetX`; у `Action` ещё `AddX` / `RemoveX` | `bindItem` → `SetBindItem`, `AddBindItem` |
| метод `M()` | `MSelf()` | `Focus()` → `FocusSelf()` |

Правило покрывает свойства и события всех элементов — от `VisualElement` до `MultiColumnTreeView`; полный список — в [справочнике API](https://vpdpersonal.github.io/Aspid.FastTools/ru/api/Aspid.FastTools.UIElements). Исключения:

- `EnumField` и `EnumFlagsField` (в редакторе) получают `Initialize` вместо `Init`;
- `Button.SetClickable` принимает и `Clickable`, и `Action`;
- `IMGUIContainer.MarkDirtyLayout` сохраняет своё имя.

`IsFocused()` проверяет фокус:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>bool focused = search.focusController?&#10;    .focusedElement == search;</code></pre> | <pre lang="csharp"><code>bool focused = search.IsFocused();</code></pre> |

## Дочерние элементы

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>header.Add(title);</code></pre> | <pre lang="csharp"><code>header.AddChild(title);</code></pre> |
| <pre lang="csharp"><code>header.Add(title);&#10;header.Add(badge);</code></pre> | <pre lang="csharp"><code>header.AddChildren(title, badge);</code></pre> |
| <pre lang="csharp"><code>header.Insert(0, badge);</code></pre> | <pre lang="csharp"><code>header.InsertChild(0, badge);</code></pre> |
| <pre lang="csharp"><code>header.Insert(0, title);&#10;header.Insert(1, badge);</code></pre> | <pre lang="csharp"><code>header.InsertChildren(0, title, badge);</code></pre> |
| <pre lang="csharp"><code>header.Remove(badge);</code></pre> | <pre lang="csharp"><code>header.RemoveChild(badge);</code></pre> |
| <pre lang="csharp"><code>header.RemoveAt(0);</code></pre> | <pre lang="csharp"><code>header.RemoveChildAt(0);</code></pre> |
| <pre lang="csharp"><code>header.Clear();</code></pre> | <pre lang="csharp"><code>header.ClearChildren();</code></pre> |
| <pre lang="csharp"><code>if (isFree)&#10;    body.Add(helpBox);</code></pre> | <pre lang="csharp"><code>body.AddChildIf(isFree, helpBox);</code></pre> |

У каждого метода есть вариант `…If(condition, …)`. `AddChildren` и `InsertChildren` принимают `params`, `IEnumerable`, `List`, `Span` или `ReadOnlySpan`, сохраняют порядок элементов и пропускают `null`-коллекцию.

## Стили

Одно значение задаёт все стороны, `X` — левую и правую, `Y` — верхнюю и нижнюю. Пропущенные именованные параметры оставляют прежнее значение:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>header.style.paddingLeft = 12;&#10;header.style.paddingRight = 12;&#10;header.style.borderBottomWidth = 1;</code></pre> | <pre lang="csharp"><code>header&#10;    .SetPaddingX(12)&#10;    .SetBorderWidth(bottom: 1);</code></pre> |

| Метод | Задаёт | Варианты |
|---|---|---|
| `SetMargin`, `SetPadding` | `margin…`, `padding…` | `X`, `Y`, `Top`, `Right`, `Bottom`, `Left` |
| `SetBorderWidth`, `SetBorderColor` | `border…Width`, `border…Color` | `X`, `Y`, `Top`, `Right`, `Bottom`, `Left` |
| `SetUnitySlice` | `unitySlice…` | `X`, `Y`, `Top`, `Right`, `Bottom`, `Left` |
| `SetDistance` | `top`, `right`, `bottom`, `left` | `X`, `Y`; одна сторона: `SetTop`, `SetRight`, `SetBottom`, `SetLeft` |
| `SetBorderRadius` | `border…Radius` | `Top`, `Bottom`, `Left`, `Right`, `TopLeft`, `TopRight`, `BottomRight`, `BottomLeft` |
| `SetSize`, `SetMinSize`, `SetMaxSize` | `width` и `height`, `min…`, `max…` | одно значение, два или одно именованное |
| `SetBackgroundPosition` | `backgroundPositionX`, `backgroundPositionY` | `X`, `Y` |

Сеттеры цвета принимают и строку hex, а сеттеры ассетов — путь в `Resources`:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>if (ColorUtility.TryParseHtmlString(&#10;        "#FFC24D", out var color))&#10;    badge.style.color = color;</code></pre> | <pre lang="csharp"><code>badge.SetColor("#FFC24D");</code></pre> |
| <pre lang="csharp"><code>root.style.backgroundImage = Resources&#10;    .Load&lt;Texture2D&gt;("UI/Card");</code></pre> | <pre lang="csharp"><code>root.SetBackgroundImageFromResources(&#10;    "UI/Card");</code></pre> |

Если строка не разбирается как цвет или по пути нет ассета, метод пишет предупреждение в консоль и оставляет элемент без изменений. Путь в `Resources` принимают также `SetImageFromResources`, `SetSpriteFromResources`, `SetVectorImageFromResources` и `AddStyleSheetFromResources`.

### Bold и italic

`SetNormalUnityFontStyleAndWeight()` сбрасывает оба флага; остальные пресеты меняют один флаг и сохраняют второй:

| Метод | Меняет |
|---|---|
| `AddBold…` | `Normal` → `Bold`, `Italic` → `BoldAndItalic` |
| `RemoveBold…` | `Bold` → `Normal`, `BoldAndItalic` → `Italic` |
| `AddItalic…` | `Normal` → `Italic`, `Bold` → `BoldAndItalic` |
| `RemoveItalic…` | `Italic` → `Normal`, `BoldAndItalic` → `Bold` |

Остальные значения не меняются.

> [!NOTE]
> Пресеты читают текущее значение из `style` элемента, а не итоговый стиль: начертание, заданное в USS, считается `Normal`.

## USS-классы и таблицы стилей

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>badge.AddToClassList("free");</code></pre> | <pre lang="csharp"><code>badge.AddClass("free");</code></pre> |
| <pre lang="csharp"><code>badge.RemoveFromClassList("free");</code></pre> | <pre lang="csharp"><code>badge.RemoveClass("free");</code></pre> |
| <pre lang="csharp"><code>badge.ToggleInClassList("free");</code></pre> | <pre lang="csharp"><code>badge.ToggleClass("free");</code></pre> |
| <pre lang="csharp"><code>badge.EnableInClassList(&#10;    "free", isFree);</code></pre> | <pre lang="csharp"><code>badge.EnableClass("free", isFree);</code></pre> |
| <pre lang="csharp"><code>badge.ClearClassList();</code></pre> | <pre lang="csharp"><code>badge.ClearClasses();</code></pre> |
| <pre lang="csharp"><code>root.styleSheets.Add(styleSheet);</code></pre> | <pre lang="csharp"><code>root.AddStyleSheet(styleSheet);</code></pre> |
| <pre lang="csharp"><code>root.styleSheets.Remove(styleSheet);</code></pre> | <pre lang="csharp"><code>root.RemoveStyleSheet(styleSheet);</code></pre> |
| <pre lang="csharp"><code>root.styleSheets.Remove(Resources&#10;    .Load&lt;StyleSheet&gt;("UI/Ability"));</code></pre> | <pre lang="csharp"><code>root.RemoveStyleSheetFromResources(&#10;    "UI/Ability");</code></pre> |

## Значения и события

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>manaCost.value = 10;</code></pre> | <pre lang="csharp"><code>manaCost.SetValue(10);</code></pre> |
| <pre lang="csharp"><code>manaCost.SetValueWithoutNotify(10);</code></pre> | <pre lang="csharp"><code>manaCost.SetValue(10, notify: false);</code></pre> |
| <pre lang="csharp"><code>manaCost.RegisterValueChangedCallback(&#10;    OnManaCostChanged);</code></pre> | <pre lang="csharp"><code>manaCost.AddValueChanged(&#10;    OnManaCostChanged);</code></pre> |
| <pre lang="csharp"><code>manaCost.UnregisterValueChangedCallback(&#10;    OnManaCostChanged);</code></pre> | <pre lang="csharp"><code>manaCost.RemoveValueChanged(&#10;    OnManaCostChanged);</code></pre> |

Типизированные перегрузки покрывают числа, `string`, `bool`, векторы, типы Unity вроде `Color` и `Gradient`, а при установленном `com.unity.mathematics` — его векторы, матрицы и `quaternion`, поэтому `AddValueChanged(evt => …)` не требует аргументов типа. Для остальных типов — `SetValue<TField, TValue>` и `AddValueChanged<TField, TValue>`.

## Манипуляторы

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>title.AddManipulator(&#10;    new Clickable(Refresh));</code></pre> | <pre lang="csharp"><code>title.AddClickable(Refresh);</code></pre> |
| <pre lang="csharp"><code>var clickable = new Clickable(Refresh);&#10;title.AddManipulator(clickable);</code></pre> | <pre lang="csharp"><code>title.AddClickable(&#10;    Refresh, out var clickable);</code></pre> |
| <pre lang="csharp"><code>title.AddManipulator(&#10;    new KeyboardNavigationManipulator(&#10;        OnNavigate));</code></pre> | <pre lang="csharp"><code>title.AddKeyboardNavigationManipulator(&#10;    OnNavigate);</code></pre> |
| <pre lang="csharp"><code>title.AddManipulator(&#10;    new ContextualMenuManipulator(&#10;        BuildMenu));</code></pre> | <pre lang="csharp"><code>title.AddContextualMenuManipulator(&#10;    BuildMenu);</code></pre> |

У `AddClickable` и методов `Add…Manipulator` есть перегрузка с `out`, которая сохраняет манипулятор для `RemoveManipulatorSelf`. `AddClickable` принимает и `Action<EventBase>`, и `Action` с `delay` и `interval`.

## Расширения редактора

Эти методы лежат в сборке `Aspid.FastTools.Editor` и работают только в редакторе. Добавьте `using Aspid.FastTools.UIElements.Editors;`:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>title.bindingPath = "_abilityName";&#10;title.Bind(serializedObject);</code></pre> | <pre lang="csharp"><code>title.BindTo(&#10;    serializedObject, "_abilityName");</code></pre> |
| <pre lang="csharp"><code>title.bindingPath = "_abilityName";</code></pre> | <pre lang="csharp"><code>title.SetBindingPath("_abilityName");</code></pre> |
| <pre lang="csharp"><code>title.BindProperty(abilityName);</code></pre> | <pre lang="csharp"><code>title.BindPropertyTo(abilityName);</code></pre> |
| <pre lang="csharp"><code>root.Bind(serializedObject);</code></pre> | <pre lang="csharp"><code>root.BindTo(serializedObject);</code></pre> |
| <pre lang="csharp"><code>root.Unbind();</code></pre> | <pre lang="csharp"><code>root.UnbindFrom();</code></pre> |

У `PropertyField` есть `SetLabel` и `AddValueChanged` / `RemoveValueChanged` с `SerializedPropertyChangeEvent`:

```csharp
var manaCost = new PropertyField(
        serializedObject.FindProperty("_manaCost"))
    .SetLabel("Mana cost")
    .AddValueChanged(_ => Refresh());
```

Для записи в свойство из собственного кода используйте [SerializedProperty Extensions](08-serialized-property-extensions.md).

### Скрипт и окно-владелец

`AddOpenScriptCommand` открывает в IDE скрипт `MonoBehaviour` или `ScriptableObject` по двойному клику левой кнопкой; для других объектов ничего не добавляет:

```csharp
title.AddOpenScriptCommand(target);
```

`GetOwnerWindow()` возвращает окно, на панели которого лежит элемент; иначе — окно в фокусе, затем окно под курсором или `null`.

## Собственные свойства USS

`TryGetByEnum` читает строковое свойство USS и разбирает его как enum без учёта регистра:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>if (evt.customStyle.TryGetValue(&#10;        ThemeProperty, out var raw)&#10;    &amp;&amp; Enum.TryParse(raw,&#10;        ignoreCase: true,&#10;        out PreviewTheme theme))&#10;    ApplyTheme(theme);</code></pre> | <pre lang="csharp"><code>if (evt.customStyle.TryGetByEnum(&#10;        ThemeProperty,&#10;        out PreviewTheme theme))&#10;    ApplyTheme(theme);</code></pre> |

## Пример в пакете

В [EditorTools](../../Samples~/EditorTools/Documentation/README.ru.md) окно каталога и инспектор `AbilityConfig` собраны на этих расширениях: `ListView` одной цепочкой, `BindTo` для полей, `AddOpenScriptCommand` на заголовке.

![Halve cooldown, +5 MP обновляет поля и описание эффекта; Undo возвращает прежние значения.](../../Samples~/EditorTools/Documentation/Images/demo.gif)

Halve cooldown, +5 MP обновляет поля и описание эффекта; Undo возвращает прежние значения.

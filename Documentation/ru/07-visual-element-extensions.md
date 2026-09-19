# VisualElement Extensions
Расширения UI Toolkit для построения деревьев элементов, настройки стилей, подписки на события и привязки полей в редакторе. Методы возвращают настраиваемый элемент, чтобы объединять вызовы в цепочки.

## Быстрый старт

Добавьте `using Aspid.FastTools.UIElements;` к скрипту с `using UnityEngine.UIElements;`. Так выглядит одна и та же панель с заголовком:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>var title = new Label("Stats");&#10;title.style.fontSize = 18;&#10;&#10;var panel = new VisualElement();&#10;panel.style.paddingLeft = 12;&#10;panel.style.paddingRight = 12;&#10;panel.style.paddingTop = 8;&#10;panel.style.paddingBottom = 8;&#10;panel.Add(title);</code></pre> | <pre lang="csharp"><code>var panel = new VisualElement()&#10;    .SetPaddingX(12)&#10;    .SetPaddingY(8)&#10;    .AddChild(new Label("Stats")&#10;        .SetFontSize(18));</code></pre> |

Сеттеры сохраняют тип: `new Button().SetText("Refresh")` возвращает `Button`. Операции с дочерними узлами возвращают **родителя** — следующий вызов продолжает настраивать его.

## Найти нужное расширение

| Задача | Раздел |
|---|---|
| Построить дерево, задать имя или доступность | [Элементы и дочерние узлы](#элементы-и-дочерние-узлы) |
| Управлять фокусом и клавиатурной навигацией | [Фокус](#фокус) |
| Подключить USS и переключить классы | [USS и классы](#uss-и-классы) |
| Задать размеры, отступы, цвет и рамку | [Стили](#стили) |
| Установить значение поля и подписаться на изменение | [Значения и события](#значения-и-события) |
| Настроить кнопку, поле или изображение | [Конкретные элементы](#конкретные-элементы) |
| Создать список с переиспользованием строк | [Списки и деревья](#списки-и-деревья) |
| Привязать SerializedObject или открыть скрипт | [Расширения редактора](#расширения-редактора) |
| Прочитать собственное свойство USS как enum | [Собственные свойства USS](#собственные-свойства-uss) |

## Элементы и дочерние узлы

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>element.name = name;</code></pre> | <pre lang="csharp"><code>element.SetName(name);</code></pre> |
| <pre lang="csharp"><code>element.visible = visible;</code></pre> | <pre lang="csharp"><code>element.SetVisible(visible);</code></pre> |
| <pre lang="csharp"><code>element.tooltip = tooltip;</code></pre> | <pre lang="csharp"><code>element.SetTooltip(tooltip);</code></pre> |
| <pre lang="csharp"><code>element.userData = data;</code></pre> | <pre lang="csharp"><code>element.SetUserData(data);</code></pre> |
| <pre lang="csharp"><code>element.SetEnabled(enabled);</code></pre> | <pre lang="csharp"><code>element.SetEnabledSelf(enabled);</code></pre> |
| <pre lang="csharp"><code>element.pickingMode = mode;</code></pre> | <pre lang="csharp"><code>element.SetPickingMode(mode);</code></pre> |
| <pre lang="csharp"><code>element.usageHints = hints;</code></pre> | <pre lang="csharp"><code>element.SetUsageHints(hints);</code></pre> |
| <pre lang="csharp"><code>element.viewDataKey = key;</code></pre> | <pre lang="csharp"><code>element.SetViewDataKey(key);</code></pre> |
| <pre lang="csharp"><code>element.languageDirection = direction;</code></pre> | <pre lang="csharp"><code>element.SetLanguageDirection(direction);</code></pre> |
| <pre lang="csharp"><code>element.disablePlayModeTint = disable;</code></pre> | <pre lang="csharp"><code>element.SetDisablePlayModeTint(disable);</code></pre> |
| <pre lang="csharp"><code>element.dataSource = source;</code></pre> | <pre lang="csharp"><code>element.SetDataSource(source);</code></pre> |
| <pre lang="csharp"><code>element.dataSourceType = type;</code></pre> | <pre lang="csharp"><code>element.SetDataSourceType(type);</code></pre> |
| <pre lang="csharp"><code>element.dataSourcePath = path;</code></pre> | <pre lang="csharp"><code>element.SetDataSourcePath(path);</code></pre> |

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>panel.Add(child);</code></pre> | <pre lang="csharp"><code>panel.AddChild(child);&#10;panel.AddChildIf(condition, child);</code></pre> |
| <pre lang="csharp"><code>foreach (var child in children)&#10;    panel.Add(child);</code></pre> | <pre lang="csharp"><code>panel.AddChildren(a, b, c);&#10;panel.AddChildren(enumerable);&#10;panel.AddChildren(list);&#10;panel.AddChildren(span);&#10;panel.AddChildren(readOnlySpan);&#10;panel.AddChildrenIf(condition, …);</code></pre> |
| <pre lang="csharp"><code>panel.Insert(index, child);</code></pre> | <pre lang="csharp"><code>panel.InsertChild(index, child);&#10;panel.InsertChildIf(condition, index, child);</code></pre> |
| <pre lang="csharp"><code>foreach (var child in children)&#10;    panel.Insert(index++, child);</code></pre> | <pre lang="csharp"><code>panel.InsertChildren(index, a, b, c);&#10;panel.InsertChildren(index, enumerable);&#10;panel.InsertChildren(index, list);&#10;panel.InsertChildren(index, span);&#10;panel.InsertChildren(index, readOnlySpan);&#10;panel.InsertChildrenIf(condition, index, …);</code></pre> |
| <pre lang="csharp"><code>panel.Remove(child);</code></pre> | <pre lang="csharp"><code>panel.RemoveChild(child);&#10;panel.RemoveChildIf(condition, child);</code></pre> |
| <pre lang="csharp"><code>panel.RemoveAt(index);</code></pre> | <pre lang="csharp"><code>panel.RemoveChildAt(index);&#10;panel.RemoveChildAtIf(condition, index);</code></pre> |
| <pre lang="csharp"><code>panel.Clear();</code></pre> | <pre lang="csharp"><code>panel.ClearChildren();&#10;panel.ClearChildrenIf(condition);</code></pre> |

Эти методы возвращают родительский элемент, поэтому их можно объединять в цепочку. `AddChildren` и `InsertChildren` сохраняют порядок переданных элементов.

> [!NOTE]
> `*If` проверяет условие только в момент вызова. Аргументы вычисляются заранее: `AddChildIf(false, new Label("Warning"))` создаст `Label`, но не добавит его в дерево. Для дорогого создания используйте обычный `if`.

### Видимость и доступность

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>element.visible = false;</code></pre> | <pre lang="csharp"><code>element.SetVisible(false);</code></pre> |
| <pre lang="csharp"><code>element.style.display =&#10;    DisplayStyle.None;</code></pre> | <pre lang="csharp"><code>element.SetDisplay(DisplayStyle.None);</code></pre> |
| <pre lang="csharp"><code>element.SetEnabled(false);</code></pre> | <pre lang="csharp"><code>element.SetEnabledSelf(false);</code></pre> |

## Фокус

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>search.Focus();</code></pre> | <pre lang="csharp"><code>search.FocusSelf();</code></pre> |
| <pre lang="csharp"><code>search.Blur();</code></pre> | <pre lang="csharp"><code>search.BlurSelf();</code></pre> |
| <pre lang="csharp"><code>bool focused =&#10;    search.focusController?.focusedElement&#10;        == search;</code></pre> | <pre lang="csharp"><code>bool focused = search.IsFocused();</code></pre> |
| <pre lang="csharp"><code>search.tabIndex = 0;</code></pre> | <pre lang="csharp"><code>search.SetTabIndex(0);</code></pre> |
| <pre lang="csharp"><code>search.focusable = true;</code></pre> | <pre lang="csharp"><code>search.SetFocusable(true);</code></pre> |
| <pre lang="csharp"><code>search.delegatesFocus = true;</code></pre> | <pre lang="csharp"><code>search.SetDelegatesFocus(true);</code></pre> |

## USS и классы

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>panel.AddToClassList("ability-card");</code></pre> | <pre lang="csharp"><code>panel.AddClass("ability-card");</code></pre> |
| <pre lang="csharp"><code>panel.RemoveFromClassList("ability-card");</code></pre> | <pre lang="csharp"><code>panel.RemoveClass("ability-card");</code></pre> |
| <pre lang="csharp"><code>panel.ClearClassList();</code></pre> | <pre lang="csharp"><code>panel.ClearClasses();</code></pre> |
| <pre lang="csharp"><code>panel.ToggleInClassList("playing");</code></pre> | <pre lang="csharp"><code>panel.ToggleClass("playing");</code></pre> |
| <pre lang="csharp"><code>panel.EnableInClassList(&#10;    "playing", Application.isPlaying);</code></pre> | <pre lang="csharp"><code>panel.EnableClass(&#10;    "playing", Application.isPlaying);</code></pre> |
| <pre lang="csharp"><code>panel.styleSheets.Add(styleSheet);</code></pre> | <pre lang="csharp"><code>panel.AddStyleSheet(styleSheet);</code></pre> |
| <pre lang="csharp"><code>panel.styleSheets.Remove(styleSheet);</code></pre> | <pre lang="csharp"><code>panel.RemoveStyleSheet(styleSheet);</code></pre> |
| <pre lang="csharp"><code>panel.styleSheets.Add(&#10;    Resources.Load&lt;StyleSheet&gt;("UI/AbilityCard"));</code></pre> | <pre lang="csharp"><code>panel.AddStyleSheetFromResources("UI/AbilityCard");</code></pre> |
| <pre lang="csharp"><code>panel.styleSheets.Remove(&#10;    Resources.Load&lt;StyleSheet&gt;("UI/AbilityCard"));</code></pre> | <pre lang="csharp"><code>panel.RemoveStyleSheetFromResources("UI/AbilityCard");</code></pre> |

## Стили

### Стороны, оси и единицы измерения

Общее значение задаёт все стороны; `X` — левую и правую, `Y` — верхнюю и нижнюю. В перегрузках с необязательными параметрами пропущенные стороны сохраняют прежнее значение:

```csharp
panel
    .SetPadding(8)               // все стороны
    .SetPaddingX(12)             // слева и справа
    .SetMargin(top: 4, bottom: 8)
    .SetSize(width: Length.Percent(100));
```

### Настройка через IStyle

Те же методы доступны на `element.style`. Такая цепочка возвращает `IStyle`, поэтому продолжать её методами элемента нужно отдельным вызовом:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>panel.style.paddingLeft = 12;&#10;panel.style.paddingRight = 12;&#10;panel.style.height = 48;</code></pre> | <pre lang="csharp"><code>panel.style&#10;    .SetPaddingX(12)&#10;    .SetHeight(48);</code></pre> |

### Справочник стилей

Основные примеры рассчитаны на Unity 6.0. В раскрывающихся блоках отмечены методы для более новых версий Unity.

<details>
<summary>Раскладка</summary>

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>panel.style.flexBasis = 120;</code></pre> | <pre lang="csharp"><code>panel.SetFlexBasis(120);</code></pre> |
| <pre lang="csharp"><code>panel.style.flexGrow = 1;</code></pre> | <pre lang="csharp"><code>panel.SetFlexGrow(1);</code></pre> |
| <pre lang="csharp"><code>panel.style.flexShrink = 0;</code></pre> | <pre lang="csharp"><code>panel.SetFlexShrink(0);</code></pre> |
| <pre lang="csharp"><code>panel.style.flexWrap = Wrap.Wrap;</code></pre> | <pre lang="csharp"><code>panel.SetFlexWrap(Wrap.Wrap);</code></pre> |
| <pre lang="csharp"><code>panel.style.flexDirection = FlexDirection.Row;</code></pre> | <pre lang="csharp"><code>panel.SetFlexDirection(FlexDirection.Row);</code></pre> |
| <pre lang="csharp"><code>panel.style.alignSelf = Align.Center;</code></pre> | <pre lang="csharp"><code>panel.SetAlignSelf(Align.Center);</code></pre> |
| <pre lang="csharp"><code>panel.style.alignItems = Align.Center;</code></pre> | <pre lang="csharp"><code>panel.SetAlignItems(Align.Center);</code></pre> |
| <pre lang="csharp"><code>panel.style.alignContent = Align.Stretch;</code></pre> | <pre lang="csharp"><code>panel.SetAlignContent(Align.Stretch);</code></pre> |
| <pre lang="csharp"><code>panel.style.justifyContent = Justify.SpaceBetween;</code></pre> | <pre lang="csharp"><code>panel.SetJustifyContent(Justify.SpaceBetween);</code></pre> |
| <pre lang="csharp"><code>panel.style.position = Position.Absolute;</code></pre> | <pre lang="csharp"><code>panel.SetPosition(Position.Absolute);</code></pre> |

</details>

<details>
<summary>Размеры</summary>

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>panel.style.width = 48;&#10;panel.style.height = 48;</code></pre> | <pre lang="csharp"><code>panel.SetSize(48);</code></pre> |
| <pre lang="csharp"><code>panel.style.width = 240;&#10;panel.style.height = 48;</code></pre> | <pre lang="csharp"><code>panel.SetSize(240, 48);</code></pre> |
| <pre lang="csharp"><code>panel.style.width = Length.Percent(100);</code></pre> | <pre lang="csharp"><code>panel.SetSize(width: Length.Percent(100));</code></pre> |
| <pre lang="csharp"><code>panel.style.minWidth = 120;&#10;panel.style.minHeight = 120;</code></pre> | <pre lang="csharp"><code>panel.SetMinSize(120);</code></pre> |
| <pre lang="csharp"><code>panel.style.minWidth = 120;&#10;panel.style.minHeight = 32;</code></pre> | <pre lang="csharp"><code>panel.SetMinSize(120, 32);</code></pre> |
| <pre lang="csharp"><code>panel.style.minHeight = 32;</code></pre> | <pre lang="csharp"><code>panel.SetMinSize(minHeight: 32);</code></pre> |
| <pre lang="csharp"><code>panel.style.maxWidth = 480;&#10;panel.style.maxHeight = 480;</code></pre> | <pre lang="csharp"><code>panel.SetMaxSize(480);</code></pre> |
| <pre lang="csharp"><code>panel.style.maxWidth = 480;&#10;panel.style.maxHeight = 320;</code></pre> | <pre lang="csharp"><code>panel.SetMaxSize(480, 320);</code></pre> |
| <pre lang="csharp"><code>panel.style.maxWidth = 480;</code></pre> | <pre lang="csharp"><code>panel.SetMaxSize(maxWidth: 480);</code></pre> |
| <pre lang="csharp"><code>panel.style.width = 240;</code></pre> | <pre lang="csharp"><code>panel.SetWidth(240);</code></pre> |
| <pre lang="csharp"><code>panel.style.minWidth = 120;</code></pre> | <pre lang="csharp"><code>panel.SetMinWidth(120);</code></pre> |
| <pre lang="csharp"><code>panel.style.maxWidth = 480;</code></pre> | <pre lang="csharp"><code>panel.SetMaxWidth(480);</code></pre> |
| <pre lang="csharp"><code>panel.style.height = 48;</code></pre> | <pre lang="csharp"><code>panel.SetHeight(48);</code></pre> |
| <pre lang="csharp"><code>panel.style.minHeight = 32;</code></pre> | <pre lang="csharp"><code>panel.SetMinHeight(32);</code></pre> |
| <pre lang="csharp"><code>panel.style.maxHeight = 320;</code></pre> | <pre lang="csharp"><code>panel.SetMaxHeight(320);</code></pre> |

</details>

<details>
<summary>Отступы и позиционирование</summary>

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>panel.style.marginTop = 8;&#10;panel.style.marginRight = 8;&#10;panel.style.marginBottom = 8;&#10;panel.style.marginLeft = 8;</code></pre> | <pre lang="csharp"><code>panel.SetMargin(8);</code></pre> |
| <pre lang="csharp"><code>panel.style.marginTop = 8;&#10;panel.style.marginBottom = 8;</code></pre> | <pre lang="csharp"><code>panel.SetMargin(top: 8, bottom: 8);</code></pre> |
| <pre lang="csharp"><code>panel.style.marginLeft = 8;&#10;panel.style.marginRight = 8;</code></pre> | <pre lang="csharp"><code>panel.SetMarginX(8);</code></pre> |
| <pre lang="csharp"><code>panel.style.marginTop = 8;&#10;panel.style.marginBottom = 8;</code></pre> | <pre lang="csharp"><code>panel.SetMarginY(8);</code></pre> |
| <pre lang="csharp"><code>panel.style.marginTop = 8;</code></pre> | <pre lang="csharp"><code>panel.SetMarginTop(8);</code></pre> |
| <pre lang="csharp"><code>panel.style.marginRight = 8;</code></pre> | <pre lang="csharp"><code>panel.SetMarginRight(8);</code></pre> |
| <pre lang="csharp"><code>panel.style.marginBottom = 8;</code></pre> | <pre lang="csharp"><code>panel.SetMarginBottom(8);</code></pre> |
| <pre lang="csharp"><code>panel.style.marginLeft = 8;</code></pre> | <pre lang="csharp"><code>panel.SetMarginLeft(8);</code></pre> |
| <pre lang="csharp"><code>panel.style.paddingTop = 12;&#10;panel.style.paddingRight = 12;&#10;panel.style.paddingBottom = 12;&#10;panel.style.paddingLeft = 12;</code></pre> | <pre lang="csharp"><code>panel.SetPadding(12);</code></pre> |
| <pre lang="csharp"><code>panel.style.paddingTop = 12;&#10;panel.style.paddingBottom = 12;</code></pre> | <pre lang="csharp"><code>panel.SetPadding(top: 12, bottom: 12);</code></pre> |
| <pre lang="csharp"><code>panel.style.paddingLeft = 12;&#10;panel.style.paddingRight = 12;</code></pre> | <pre lang="csharp"><code>panel.SetPaddingX(12);</code></pre> |
| <pre lang="csharp"><code>panel.style.paddingTop = 12;&#10;panel.style.paddingBottom = 12;</code></pre> | <pre lang="csharp"><code>panel.SetPaddingY(12);</code></pre> |
| <pre lang="csharp"><code>panel.style.paddingTop = 12;</code></pre> | <pre lang="csharp"><code>panel.SetPaddingTop(12);</code></pre> |
| <pre lang="csharp"><code>panel.style.paddingRight = 12;</code></pre> | <pre lang="csharp"><code>panel.SetPaddingRight(12);</code></pre> |
| <pre lang="csharp"><code>panel.style.paddingBottom = 12;</code></pre> | <pre lang="csharp"><code>panel.SetPaddingBottom(12);</code></pre> |
| <pre lang="csharp"><code>panel.style.paddingLeft = 12;</code></pre> | <pre lang="csharp"><code>panel.SetPaddingLeft(12);</code></pre> |
| <pre lang="csharp"><code>panel.style.top = 0;&#10;panel.style.right = 0;&#10;panel.style.bottom = 0;&#10;panel.style.left = 0;</code></pre> | <pre lang="csharp"><code>panel.SetDistance(0);</code></pre> |
| <pre lang="csharp"><code>panel.style.top = 0;&#10;panel.style.bottom = 0;</code></pre> | <pre lang="csharp"><code>panel.SetDistance(top: 0, bottom: 0);</code></pre> |
| <pre lang="csharp"><code>panel.style.left = 0;&#10;panel.style.right = 0;</code></pre> | <pre lang="csharp"><code>panel.SetDistanceX(0);</code></pre> |
| <pre lang="csharp"><code>panel.style.top = 0;&#10;panel.style.bottom = 0;</code></pre> | <pre lang="csharp"><code>panel.SetDistanceY(0);</code></pre> |
| <pre lang="csharp"><code>panel.style.top = 0;</code></pre> | <pre lang="csharp"><code>panel.SetTop(0);</code></pre> |
| <pre lang="csharp"><code>panel.style.right = 0;</code></pre> | <pre lang="csharp"><code>panel.SetRight(0);</code></pre> |
| <pre lang="csharp"><code>panel.style.bottom = 0;</code></pre> | <pre lang="csharp"><code>panel.SetBottom(0);</code></pre> |
| <pre lang="csharp"><code>panel.style.left = 0;</code></pre> | <pre lang="csharp"><code>panel.SetLeft(0);</code></pre> |

> `SetDistance` — обёртка для четырёх свойств `top`/`right`/`bottom`/`left`, используемых при абсолютном позиционировании. `SetTop`, `SetRight`, `SetBottom`, `SetLeft` — это прямые алиасы для одного свойства.

</details>

<details>
<summary>Шрифт</summary>

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>panel.style.unityFont = font;</code></pre> | <pre lang="csharp"><code>panel.SetUnityFont(font);</code></pre> |
| <pre lang="csharp"><code>panel.style.fontSize = 14;</code></pre> | <pre lang="csharp"><code>panel.SetFontSize(14);</code></pre> |
| <pre lang="csharp"><code>panel.style.unityFontDefinition = fontDefinition;</code></pre> | <pre lang="csharp"><code>panel.SetUnityFontDefinition(fontDefinition);</code></pre> |
| <pre lang="csharp"><code>panel.style.unityFontStyleAndWeight = FontStyle.Bold;</code></pre> | <pre lang="csharp"><code>panel.SetUnityFontStyleAndWeight(FontStyle.Bold);</code></pre> |

</details>

<details>
<summary>Начертание шрифта</summary>

Удобные методы для переключения bold / italic без перезаписи другого флага:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>panel.style.unityFontStyleAndWeight =&#10;    FontStyle.Normal;</code></pre> | <pre lang="csharp"><code>panel.SetNormalUnityFontStyleAndWeight();</code></pre> |
| <pre lang="csharp"><code>var current =&#10;    panel.style.unityFontStyleAndWeight.value;&#10;panel.style.unityFontStyleAndWeight =&#10;    current == FontStyle.Italic&#10;        ? FontStyle.BoldAndItalic&#10;        : FontStyle.Bold;</code></pre> | <pre lang="csharp"><code>panel.AddBoldUnityFontStyleAndWeight();</code></pre> |
| <pre lang="csharp"><code>var current =&#10;    panel.style.unityFontStyleAndWeight.value;&#10;panel.style.unityFontStyleAndWeight =&#10;    current == FontStyle.BoldAndItalic&#10;        ? FontStyle.Italic&#10;        : FontStyle.Normal;</code></pre> | <pre lang="csharp"><code>panel.RemoveBoldUnityFontStyleAndWeight();</code></pre> |
| <pre lang="csharp"><code>var current =&#10;    panel.style.unityFontStyleAndWeight.value;&#10;panel.style.unityFontStyleAndWeight =&#10;    current == FontStyle.Bold&#10;        ? FontStyle.BoldAndItalic&#10;        : FontStyle.Italic;</code></pre> | <pre lang="csharp"><code>panel.AddItalicUnityFontStyleAndWeight();</code></pre> |
| <pre lang="csharp"><code>var current =&#10;    panel.style.unityFontStyleAndWeight.value;&#10;panel.style.unityFontStyleAndWeight =&#10;    current == FontStyle.BoldAndItalic&#10;        ? FontStyle.Bold&#10;        : FontStyle.Normal;</code></pre> | <pre lang="csharp"><code>panel.RemoveItalicUnityFontStyleAndWeight();</code></pre> |

</details>

<details>
<summary>Текст</summary>

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>panel.style.wordSpacing = 2;</code></pre> | <pre lang="csharp"><code>panel.SetWordSpacing(2);</code></pre> |
| <pre lang="csharp"><code>panel.style.letterSpacing = 1;</code></pre> | <pre lang="csharp"><code>panel.SetLetterSpacing(1);</code></pre> |
| <pre lang="csharp"><code>panel.style.unityTextAlign = TextAnchor.MiddleCenter;</code></pre> | <pre lang="csharp"><code>panel.SetUnityTextAlign(TextAnchor.MiddleCenter);</code></pre> |
| <pre lang="csharp"><code>panel.style.textShadow = shadow;</code></pre> | <pre lang="csharp"><code>panel.SetTextShadow(shadow);</code></pre> |
| <pre lang="csharp"><code>panel.style.unityTextOutlineColor = Color.black;</code></pre> | <pre lang="csharp"><code>panel.SetUnityTextOutlineColor(Color.black);</code></pre> |
| <pre lang="csharp"><code>panel.style.unityTextOutlineWidth = 1;</code></pre> | <pre lang="csharp"><code>panel.SetUnityTextOutlineWidth(1);</code></pre> |
| <pre lang="csharp"><code>panel.style.unityParagraphSpacing = 8;</code></pre> | <pre lang="csharp"><code>panel.SetUnityParagraphSpacing(8);</code></pre> |
| <pre lang="csharp"><code>panel.style.textOverflow = TextOverflow.Ellipsis;</code></pre> | <pre lang="csharp"><code>panel.SetTextOverflow(TextOverflow.Ellipsis);</code></pre> |
| <pre lang="csharp"><code>panel.style.unityTextOverflowPosition = &#10;    TextOverflowPosition.End;</code></pre> | <pre lang="csharp"><code>panel.SetUnityTextOverflowPosition(&#10;    TextOverflowPosition.End);</code></pre> |
| <pre lang="csharp"><code>panel.style.unityTextGenerator = &#10;    TextGeneratorType.Advanced;</code></pre> | <pre lang="csharp"><code>panel.SetUnityTextGenerator(&#10;    TextGeneratorType.Advanced);</code></pre> |
| <pre lang="csharp"><code>panel.style.unityEditorTextRenderingMode = &#10;    EditorTextRenderingMode.SDF;</code></pre> | <pre lang="csharp"><code>panel.SetUnityEditorTextRenderingMode(&#10;    EditorTextRenderingMode.SDF);</code></pre> |
| <pre lang="csharp"><code>panel.style.whiteSpace = WhiteSpace.NoWrap;</code></pre> | <pre lang="csharp"><code>panel.SetWhiteSpace(WhiteSpace.NoWrap);</code></pre> |
| <pre lang="csharp"><code>// Unity 6.2+&#10;panel.style.unityTextAutoSize = autoSize;</code></pre> | <pre lang="csharp"><code>// Unity 6.2+&#10;panel.SetUnityTextAutoSize(autoSize);</code></pre> |

</details>

<details>
<summary>Цвет и прозрачность</summary>

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>panel.style.color = Color.white;</code></pre> | <pre lang="csharp"><code>panel.SetColor(Color.white);</code></pre> |
| <pre lang="csharp"><code>if (ColorUtility.TryParseHtmlString(&#10;        "#FF8800", out var color))&#10;    panel.style.color = color;</code></pre> | <pre lang="csharp"><code>panel.SetColor("#FF8800");</code></pre> |
| <pre lang="csharp"><code>panel.style.opacity = 0.5f;</code></pre> | <pre lang="csharp"><code>panel.SetOpacity(0.5f);</code></pre> |

</details>

<details>
<summary>Рамка</summary>

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>panel.style.borderTopColor = Color.gray;&#10;panel.style.borderRightColor = Color.gray;&#10;panel.style.borderBottomColor = Color.gray;&#10;panel.style.borderLeftColor = Color.gray;</code></pre> | <pre lang="csharp"><code>panel.SetBorderColor(Color.gray);</code></pre> |
| <pre lang="csharp"><code>if (ColorUtility.TryParseHtmlString(&#10;        "#333333", out var color))&#10;&#123;&#10;    panel.style.borderTopColor = color;&#10;    panel.style.borderRightColor = color;&#10;    panel.style.borderBottomColor = color;&#10;    panel.style.borderLeftColor = color;&#10;&#125;</code></pre> | <pre lang="csharp"><code>panel.SetBorderColor("#333333");</code></pre> |
| <pre lang="csharp"><code>panel.style.borderTopColor = Color.gray;&#10;panel.style.borderBottomColor = Color.gray;</code></pre> | <pre lang="csharp"><code>panel.SetBorderColor(&#10;    top: Color.gray, bottom: Color.gray);</code></pre> |
| <pre lang="csharp"><code>panel.style.borderLeftColor = Color.gray;&#10;panel.style.borderRightColor = Color.gray;</code></pre> | <pre lang="csharp"><code>panel.SetBorderColorX(Color.gray);</code></pre> |
| <pre lang="csharp"><code>panel.style.borderTopColor = Color.gray;&#10;panel.style.borderBottomColor = Color.gray;</code></pre> | <pre lang="csharp"><code>panel.SetBorderColorY(Color.gray);</code></pre> |
| <pre lang="csharp"><code>panel.style.borderTopColor = Color.gray;</code></pre> | <pre lang="csharp"><code>panel.SetBorderColorTop(Color.gray);</code></pre> |
| <pre lang="csharp"><code>panel.style.borderRightColor = Color.gray;</code></pre> | <pre lang="csharp"><code>panel.SetBorderColorRight(Color.gray);</code></pre> |
| <pre lang="csharp"><code>panel.style.borderBottomColor = Color.gray;</code></pre> | <pre lang="csharp"><code>panel.SetBorderColorBottom(Color.gray);</code></pre> |
| <pre lang="csharp"><code>panel.style.borderLeftColor = Color.gray;</code></pre> | <pre lang="csharp"><code>panel.SetBorderColorLeft(Color.gray);</code></pre> |
| <pre lang="csharp"><code>panel.style.borderTopLeftRadius = 6;&#10;panel.style.borderTopRightRadius = 6;&#10;panel.style.borderBottomRightRadius = 6;&#10;panel.style.borderBottomLeftRadius = 6;</code></pre> | <pre lang="csharp"><code>panel.SetBorderRadius(6);</code></pre> |
| <pre lang="csharp"><code>panel.style.borderTopLeftRadius = 6;&#10;panel.style.borderTopRightRadius = 6;</code></pre> | <pre lang="csharp"><code>panel.SetBorderRadius(&#10;    topLeft: 6, topRight: 6);</code></pre> |
| <pre lang="csharp"><code>panel.style.borderTopLeftRadius = 6;&#10;panel.style.borderTopRightRadius = 6;</code></pre> | <pre lang="csharp"><code>panel.SetBorderRadiusTop(6);</code></pre> |
| <pre lang="csharp"><code>panel.style.borderBottomLeftRadius = 6;&#10;panel.style.borderBottomRightRadius = 6;</code></pre> | <pre lang="csharp"><code>panel.SetBorderRadiusBottom(6);</code></pre> |
| <pre lang="csharp"><code>panel.style.borderTopLeftRadius = 6;&#10;panel.style.borderBottomLeftRadius = 6;</code></pre> | <pre lang="csharp"><code>panel.SetBorderRadiusLeft(6);</code></pre> |
| <pre lang="csharp"><code>panel.style.borderTopRightRadius = 6;&#10;panel.style.borderBottomRightRadius = 6;</code></pre> | <pre lang="csharp"><code>panel.SetBorderRadiusRight(6);</code></pre> |
| <pre lang="csharp"><code>panel.style.borderTopLeftRadius = 6;</code></pre> | <pre lang="csharp"><code>panel.SetBorderRadiusTopLeft(6);</code></pre> |
| <pre lang="csharp"><code>panel.style.borderTopRightRadius = 6;</code></pre> | <pre lang="csharp"><code>panel.SetBorderRadiusTopRight(6);</code></pre> |
| <pre lang="csharp"><code>panel.style.borderBottomRightRadius = 6;</code></pre> | <pre lang="csharp"><code>panel.SetBorderRadiusBottomRight(6);</code></pre> |
| <pre lang="csharp"><code>panel.style.borderBottomLeftRadius = 6;</code></pre> | <pre lang="csharp"><code>panel.SetBorderRadiusBottomLeft(6);</code></pre> |
| <pre lang="csharp"><code>panel.style.borderTopWidth = 1;&#10;panel.style.borderRightWidth = 1;&#10;panel.style.borderBottomWidth = 1;&#10;panel.style.borderLeftWidth = 1;</code></pre> | <pre lang="csharp"><code>panel.SetBorderWidth(1);</code></pre> |
| <pre lang="csharp"><code>panel.style.borderTopWidth = 1;&#10;panel.style.borderBottomWidth = 1;</code></pre> | <pre lang="csharp"><code>panel.SetBorderWidth(top: 1, bottom: 1);</code></pre> |
| <pre lang="csharp"><code>panel.style.borderLeftWidth = 1;&#10;panel.style.borderRightWidth = 1;</code></pre> | <pre lang="csharp"><code>panel.SetBorderWidthX(1);</code></pre> |
| <pre lang="csharp"><code>panel.style.borderTopWidth = 1;&#10;panel.style.borderBottomWidth = 1;</code></pre> | <pre lang="csharp"><code>panel.SetBorderWidthY(1);</code></pre> |
| <pre lang="csharp"><code>panel.style.borderTopWidth = 1;</code></pre> | <pre lang="csharp"><code>panel.SetBorderWidthTop(1);</code></pre> |
| <pre lang="csharp"><code>panel.style.borderRightWidth = 1;</code></pre> | <pre lang="csharp"><code>panel.SetBorderWidthRight(1);</code></pre> |
| <pre lang="csharp"><code>panel.style.borderBottomWidth = 1;</code></pre> | <pre lang="csharp"><code>panel.SetBorderWidthBottom(1);</code></pre> |
| <pre lang="csharp"><code>panel.style.borderLeftWidth = 1;</code></pre> | <pre lang="csharp"><code>panel.SetBorderWidthLeft(1);</code></pre> |

</details>

<details>
<summary>Фон</summary>

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>panel.style.backgroundColor = Color.black;</code></pre> | <pre lang="csharp"><code>panel.SetBackgroundColor(Color.black);</code></pre> |
| <pre lang="csharp"><code>if (ColorUtility.TryParseHtmlString(&#10;        "#1B1B1B", out var color))&#10;    panel.style.backgroundColor = color;</code></pre> | <pre lang="csharp"><code>panel.SetBackgroundColor("#1B1B1B");</code></pre> |
| <pre lang="csharp"><code>panel.style.backgroundImage = texture;</code></pre> | <pre lang="csharp"><code>panel.SetBackgroundImage(texture);</code></pre> |
| <pre lang="csharp"><code>panel.style.backgroundImage =&#10;    Resources.Load&lt;Texture2D&gt;("UI/CardBackground");</code></pre> | <pre lang="csharp"><code>panel.SetBackgroundImageFromResources(&#10;    "UI/CardBackground");</code></pre> |
| <pre lang="csharp"><code>panel.style.backgroundSize = backgroundSize;</code></pre> | <pre lang="csharp"><code>panel.SetBackgroundSize(backgroundSize);</code></pre> |
| <pre lang="csharp"><code>panel.style.backgroundRepeat = backgroundRepeat;</code></pre> | <pre lang="csharp"><code>panel.SetBackgroundRepeat(backgroundRepeat);</code></pre> |
| <pre lang="csharp"><code>panel.style.backgroundPositionX = position;&#10;panel.style.backgroundPositionY = position;</code></pre> | <pre lang="csharp"><code>panel.SetBackgroundPosition(position);</code></pre> |
| <pre lang="csharp"><code>panel.style.backgroundPositionY = position;</code></pre> | <pre lang="csharp"><code>panel.SetBackgroundPosition(y: position);</code></pre> |
| <pre lang="csharp"><code>panel.style.backgroundPositionX = position;</code></pre> | <pre lang="csharp"><code>panel.SetBackgroundPositionX(position);</code></pre> |
| <pre lang="csharp"><code>panel.style.backgroundPositionY = position;</code></pre> | <pre lang="csharp"><code>panel.SetBackgroundPositionY(position);</code></pre> |
| <pre lang="csharp"><code>panel.style.unityBackgroundImageTintColor =&#10;    Color.white;</code></pre> | <pre lang="csharp"><code>panel.SetUnityBackgroundImageTintColor(&#10;    Color.white);</code></pre> |

</details>

<details>
<summary>Трансформации</summary>

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>panel.style.scale = new Scale(Vector2.one * 1.2f);</code></pre> | <pre lang="csharp"><code>panel.SetScale(new Scale(Vector2.one * 1.2f));</code></pre> |
| <pre lang="csharp"><code>panel.style.rotate = new Rotate(45);</code></pre> | <pre lang="csharp"><code>panel.SetRotate(new Rotate(45));</code></pre> |
| <pre lang="csharp"><code>panel.style.translate = new Translate(8, 0);</code></pre> | <pre lang="csharp"><code>panel.SetTranslate(new Translate(8, 0));</code></pre> |
| <pre lang="csharp"><code>panel.style.transformOrigin = transformOrigin;</code></pre> | <pre lang="csharp"><code>panel.SetTransformOrigin(transformOrigin);</code></pre> |

</details>

<details>
<summary>Пропорции, фильтры и материал</summary>

Доступно начиная с Unity 6000.3+.

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>panel.style.aspectRatio = aspectRatio;</code></pre> | <pre lang="csharp"><code>panel.SetAspectRatio(aspectRatio);</code></pre> |
| <pre lang="csharp"><code>panel.style.filter = filter;</code></pre> | <pre lang="csharp"><code>panel.SetFilter(filter);</code></pre> |
| <pre lang="csharp"><code>panel.style.unityMaterial = material;</code></pre> | <pre lang="csharp"><code>panel.SetUnityMaterial(material);</code></pre> |

</details>

<details>
<summary>Переходы</summary>

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>panel.style.transitionDelay =&#10;    new List&lt;TimeValue&gt; &#123; 0.1f &#125;;</code></pre> | <pre lang="csharp"><code>panel.SetTransitionDelay(&#10;    new List&lt;TimeValue&gt; &#123; 0.1f &#125;);</code></pre> |
| <pre lang="csharp"><code>panel.style.transitionDuration =&#10;    new List&lt;TimeValue&gt; &#123; 0.3f &#125;;</code></pre> | <pre lang="csharp"><code>panel.SetTransitionDuration(&#10;    new List&lt;TimeValue&gt; &#123; 0.3f &#125;);</code></pre> |
| <pre lang="csharp"><code>panel.style.transitionProperty =&#10;    new List&lt;StylePropertyName&gt; &#123; "opacity" &#125;;</code></pre> | <pre lang="csharp"><code>panel.SetTransitionProperty(&#10;    new List&lt;StylePropertyName&gt; &#123; "opacity" &#125;);</code></pre> |
| <pre lang="csharp"><code>panel.style.transitionTimingFunction =&#10;    new List&lt;EasingFunction&gt; &#123; EasingMode.EaseInOut &#125;;</code></pre> | <pre lang="csharp"><code>panel.SetTransitionTimingFunction(&#10;    new List&lt;EasingFunction&gt; &#123; EasingMode.EaseInOut &#125;);</code></pre> |

</details>

<details>
<summary>Переполнение и видимость</summary>

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>panel.style.overflow = Overflow.Hidden;</code></pre> | <pre lang="csharp"><code>panel.SetOverflow(Overflow.Hidden);</code></pre> |
| <pre lang="csharp"><code>panel.style.unityOverflowClipBox = &#10;    OverflowClipBox.ContentBox;</code></pre> | <pre lang="csharp"><code>panel.SetUnityOverflowClipBox(&#10;    OverflowClipBox.ContentBox);</code></pre> |
| <pre lang="csharp"><code>panel.style.visibility = Visibility.Hidden;</code></pre> | <pre lang="csharp"><code>panel.SetVisibility(Visibility.Hidden);</code></pre> |
| <pre lang="csharp"><code>panel.style.display = DisplayStyle.None;</code></pre> | <pre lang="csharp"><code>panel.SetDisplay(DisplayStyle.None);</code></pre> |

</details>

<details>
<summary>Нарезка изображения</summary>

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>panel.style.unitySliceTop = 4;&#10;panel.style.unitySliceRight = 4;&#10;panel.style.unitySliceBottom = 4;&#10;panel.style.unitySliceLeft = 4;</code></pre> | <pre lang="csharp"><code>panel.SetUnitySlice(4);</code></pre> |
| <pre lang="csharp"><code>panel.style.unitySliceTop = 4;&#10;panel.style.unitySliceBottom = 4;</code></pre> | <pre lang="csharp"><code>panel.SetUnitySlice(top: 4, bottom: 4);</code></pre> |
| <pre lang="csharp"><code>panel.style.unitySliceLeft = 4;&#10;panel.style.unitySliceRight = 4;</code></pre> | <pre lang="csharp"><code>panel.SetUnitySliceX(4);</code></pre> |
| <pre lang="csharp"><code>panel.style.unitySliceTop = 4;&#10;panel.style.unitySliceBottom = 4;</code></pre> | <pre lang="csharp"><code>panel.SetUnitySliceY(4);</code></pre> |
| <pre lang="csharp"><code>panel.style.unitySliceTop = 4;</code></pre> | <pre lang="csharp"><code>panel.SetUnitySliceTop(4);</code></pre> |
| <pre lang="csharp"><code>panel.style.unitySliceRight = 4;</code></pre> | <pre lang="csharp"><code>panel.SetUnitySliceRight(4);</code></pre> |
| <pre lang="csharp"><code>panel.style.unitySliceBottom = 4;</code></pre> | <pre lang="csharp"><code>panel.SetUnitySliceBottom(4);</code></pre> |
| <pre lang="csharp"><code>panel.style.unitySliceLeft = 4;</code></pre> | <pre lang="csharp"><code>panel.SetUnitySliceLeft(4);</code></pre> |
| <pre lang="csharp"><code>panel.style.unitySliceScale = 1;</code></pre> | <pre lang="csharp"><code>panel.SetUnitySliceScale(1);</code></pre> |
| <pre lang="csharp"><code>panel.style.unitySliceType = SliceType.Sliced;</code></pre> | <pre lang="csharp"><code>panel.SetUnitySliceType(SliceType.Sliced);</code></pre> |

</details>

<details>
<summary>Курсор</summary>

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>panel.style.cursor = cursor;</code></pre> | <pre lang="csharp"><code>panel.SetCursor(cursor);</code></pre> |

</details>

## Значения и события

### Значение поля

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>var field = new IntegerField("Mana cost");&#10;field.value = 42;&#10;field.SetValueWithoutNotify(10);</code></pre> | <pre lang="csharp"><code>var field = new IntegerField("Mana cost")&#10;    .SetValue(42)&#10;    .SetValue(10, notify: false);</code></pre> |

### Подписка и отписка

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>field.RegisterValueChangedCallback(&#10;    onChanged);&#10;&#10;field.UnregisterValueChangedCallback(&#10;    onChanged);</code></pre> | <pre lang="csharp"><code>field.AddValueChanged(onChanged);&#10;&#10;field.RemoveValueChanged(onChanged);</code></pre> |

<details>
<summary>Типы значений и интеграция с Unity.Mathematics</summary>

Типизированные перегрузки доступны для `int`, `uint`, `nint`, `nuint`, `long`, `ulong`, `short`, `ushort`, `byte`, `sbyte`, `float`, `double`, `decimal`, `char`, `string`, `bool`, `Color`, `Vector2/3/4`, `Vector2Int/3Int`, `Rect/RectInt`, `Bounds/BoundsInt`, `Hash128`, `GUID` (Unity 6.4+), `Quaternion`, `Matrix4x4`, `Gradient`, `AnimationCurve`, `Delegate`, `Enum`, `Object`, `object`, а также обобщённый вариант `SetValue<T, TValue>` для остальных типов.

> При установленном пакете `com.unity.mathematics` автоматически выставляется define `ASPID_FASTTOOLS_UNITY_MATHEMATICS_INTEGRATION` и добавляются перегрузки `SetValue` / `AddValueChanged` / `RemoveValueChanged` для `int2/3/4` (и `intMxN`), `float2/3/4` (и `floatMxN`), `half`/`half2/3/4`, `bool2/3/4` (и `boolMxN`), а также `quaternion`.

</details>

### Кнопки и манипуляторы

Обычный `VisualElement` тоже можно сделать кликабельным. Перегрузка с `out` позволяет сохранить манипулятор для удаления:

```csharp
panel.AddClickable(Refresh, out var clickable);

// Когда клик больше не нужен
panel.RemoveManipulatorSelf(clickable);
```

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>panel.AddManipulator(manipulator);</code></pre> | <pre lang="csharp"><code>panel.AddManipulatorSelf(manipulator);</code></pre> |
| <pre lang="csharp"><code>panel.RemoveManipulator(manipulator);</code></pre> | <pre lang="csharp"><code>panel.RemoveManipulatorSelf(manipulator);</code></pre> |
| <pre lang="csharp"><code>panel.AddManipulator(new Clickable(Refresh));</code></pre> | <pre lang="csharp"><code>panel.AddClickable(Refresh);</code></pre> |
| <pre lang="csharp"><code>var clickable = new Clickable(Refresh);&#10;panel.AddManipulator(clickable);</code></pre> | <pre lang="csharp"><code>panel.AddClickable(Refresh, out var clickable);</code></pre> |
| <pre lang="csharp"><code>panel.AddManipulator(&#10;    new Clickable(evt =&gt; Refresh()));</code></pre> | <pre lang="csharp"><code>panel.AddClickable(evt =&gt; Refresh());</code></pre> |
| <pre lang="csharp"><code>panel.AddManipulator(&#10;    new Clickable(Refresh, delay: 500, interval: 100));</code></pre> | <pre lang="csharp"><code>panel.AddClickable(&#10;    Refresh, delay: 500, interval: 100);</code></pre> |
| <pre lang="csharp"><code>panel.AddManipulator(&#10;    new KeyboardNavigationManipulator(OnNavigate));</code></pre> | <pre lang="csharp"><code>panel.AddKeyboardNavigationManipulator(OnNavigate);</code></pre> |
| <pre lang="csharp"><code>panel.AddManipulator(&#10;    new ContextualMenuManipulator(BuildMenu));</code></pre> | <pre lang="csharp"><code>panel.AddContextualMenuManipulator(BuildMenu);</code></pre> |

## Конкретные элементы

<details>
<summary>TextElement</summary>

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>label.text = "Hello World";</code></pre> | <pre lang="csharp"><code>label.SetText("Hello World");</code></pre> |
| <pre lang="csharp"><code>label.enableRichText = true;</code></pre> | <pre lang="csharp"><code>label.SetEnableRichText(true);</code></pre> |
| <pre lang="csharp"><code>label.emojiFallbackSupport = true;</code></pre> | <pre lang="csharp"><code>label.SetEmojiFallbackSupport(true);</code></pre> |
| <pre lang="csharp"><code>label.parseEscapeSequences = true;</code></pre> | <pre lang="csharp"><code>label.SetParseEscapeSequences(true);</code></pre> |
| <pre lang="csharp"><code>label.displayTooltipWhenElided = true;</code></pre> | <pre lang="csharp"><code>label.SetDisplayTooltipWhenElided(true);</code></pre> |

</details>

<details>
<summary>ITextEdition (TextField, IntegerField, …)</summary>

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>textField.textEdition.maxLength = 64;</code></pre> | <pre lang="csharp"><code>textField.textEdition.SetMaxLength(64);</code></pre> |
| <pre lang="csharp"><code>textField.textEdition.maskChar = '*';</code></pre> | <pre lang="csharp"><code>textField.textEdition.SetMaskChar('*');</code></pre> |
| <pre lang="csharp"><code>textField.textEdition.isDelayed = true;</code></pre> | <pre lang="csharp"><code>textField.textEdition.SetDelayed(true);</code></pre> |
| <pre lang="csharp"><code>textField.textEdition.isReadOnly = true;</code></pre> | <pre lang="csharp"><code>textField.textEdition.SetReadOnly(true);</code></pre> |
| <pre lang="csharp"><code>textField.textEdition.isPassword = true;</code></pre> | <pre lang="csharp"><code>textField.textEdition.SetPassword(true);</code></pre> |
| <pre lang="csharp"><code>textField.textEdition.placeholder = "Поиск…";</code></pre> | <pre lang="csharp"><code>textField.textEdition.SetPlaceholder("Поиск…");</code></pre> |
| <pre lang="csharp"><code>textField.textEdition.autoCorrection = true;</code></pre> | <pre lang="csharp"><code>textField.textEdition.SetAutoCorrection(true);</code></pre> |
| <pre lang="csharp"><code>textField.textEdition.hideMobileInput = true;</code></pre> | <pre lang="csharp"><code>textField.textEdition.SetHideMobileInput(true);</code></pre> |
| <pre lang="csharp"><code>// Unity 6.4+&#10;textField.textEdition.hideSoftKeyboard = true;</code></pre> | <pre lang="csharp"><code>// Unity 6.4+&#10;textField.textEdition.SetHideSoftKeyboard(true);</code></pre> |
| <pre lang="csharp"><code>textField.textEdition.hidePlaceholderOnFocus = true;</code></pre> | <pre lang="csharp"><code>textField.textEdition.SetHidePlaceholderOnFocus(true);</code></pre> |
| <pre lang="csharp"><code>textField.textEdition.keyboardType =&#10;    TouchScreenKeyboardType.NumberPad;</code></pre> | <pre lang="csharp"><code>textField.textEdition.SetKeyboardType(&#10;    TouchScreenKeyboardType.NumberPad);</code></pre> |

</details>

<details>
<summary>ITextSelection</summary>

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>// Unity 6.3+&#10;textField.textSelection.OnCursorIndexChange += OnCursor;&#10;textField.textSelection.OnCursorIndexChange -= OnCursor;</code></pre> | <pre lang="csharp"><code>// Unity 6.3+&#10;textField.textSelection.AddOnCursorIndexChange(OnCursor);&#10;textField.textSelection.RemoveOnCursorIndexChange(OnCursor);</code></pre> |
| <pre lang="csharp"><code>// Unity 6.3+&#10;textField.textSelection.OnSelectIndexChange += OnSelect;&#10;textField.textSelection.OnSelectIndexChange -= OnSelect;</code></pre> | <pre lang="csharp"><code>// Unity 6.3+&#10;textField.textSelection.AddOnSelectIndexChange(OnSelect);&#10;textField.textSelection.RemoveOnSelectIndexChange(OnSelect);</code></pre> |
| <pre lang="csharp"><code>textField.textSelection.cursorIndex = 0;</code></pre> | <pre lang="csharp"><code>textField.textSelection.SetCursorIndex(0);</code></pre> |
| <pre lang="csharp"><code>textField.textSelection.selectIndex = 0;</code></pre> | <pre lang="csharp"><code>textField.textSelection.SetSelectIndex(0);</code></pre> |
| <pre lang="csharp"><code>textField.textSelection.isSelectable = true;</code></pre> | <pre lang="csharp"><code>textField.textSelection.SetSelectable(true);</code></pre> |
| <pre lang="csharp"><code>textField.textSelection.selectAllOnFocus = true;</code></pre> | <pre lang="csharp"><code>textField.textSelection.SetSelectAllOnFocus(true);</code></pre> |
| <pre lang="csharp"><code>textField.textSelection.selectAllOnMouseUp = true;</code></pre> | <pre lang="csharp"><code>textField.textSelection.SetSelectAllOnMouseUp(true);</code></pre> |
| <pre lang="csharp"><code>textField.textSelection.doubleClickSelectsWord = true;</code></pre> | <pre lang="csharp"><code>textField.textSelection.SetDoubleClickSelectsWord(true);</code></pre> |
| <pre lang="csharp"><code>textField.textSelection.tripleClickSelectsLine = true;</code></pre> | <pre lang="csharp"><code>textField.textSelection.SetTripleClickSelectsLine(true);</code></pre> |

</details>

<details>
<summary>BaseField&lt;TValueType&gt;</summary>

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>field.label = "Mana cost";</code></pre> | <pre lang="csharp"><code>field.SetLabel("Mana cost");</code></pre> |

</details>

<details>
<summary>BaseBoolField (Toggle)</summary>

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>toggle.text = "Показать расширенные настройки";</code></pre> | <pre lang="csharp"><code>toggle.SetText("Показать расширенные настройки");</code></pre> |
| <pre lang="csharp"><code>toggle.label = "Включено";</code></pre> | <pre lang="csharp"><code>toggle.SetLabel("Включено");</code></pre> |
| <pre lang="csharp"><code>toggle.toggleOnLabelClick = true;</code></pre> | <pre lang="csharp"><code>toggle.SetToggleOnLabelClick(true);</code></pre> |

</details>

<details>
<summary>IMixedValueSupport</summary>

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>field.showMixedValue = true;</code></pre> | <pre lang="csharp"><code>field.SetShowMixedValue(true);</code></pre> |

</details>

<details>
<summary>Button</summary>

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>button.clicked += Refresh;</code></pre> | <pre lang="csharp"><code>button.AddClicked(Refresh);</code></pre> |
| <pre lang="csharp"><code>button.clicked -= Refresh;</code></pre> | <pre lang="csharp"><code>button.RemoveClicked(Refresh);</code></pre> |
| <pre lang="csharp"><code>button.clickable = clickable;</code></pre> | <pre lang="csharp"><code>button.SetClickable(clickable);</code></pre> |
| <pre lang="csharp"><code>button.clickable = new Clickable(Refresh);</code></pre> | <pre lang="csharp"><code>button.SetClickable(Refresh);</code></pre> |
| <pre lang="csharp"><code>button.iconImage = iconImage;</code></pre> | <pre lang="csharp"><code>button.SetIconImage(iconImage);</code></pre> |

</details>

<details>
<summary>Slider / BaseSlider&lt;TValue&gt;</summary>

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>slider.lowValue = 0f;</code></pre> | <pre lang="csharp"><code>slider.SetLowValue(0f);</code></pre> |
| <pre lang="csharp"><code>slider.highValue = 100f;</code></pre> | <pre lang="csharp"><code>slider.SetHighValue(100f);</code></pre> |
| <pre lang="csharp"><code>slider.fill = true;</code></pre> | <pre lang="csharp"><code>slider.SetFill(true);</code></pre> |
| <pre lang="csharp"><code>slider.inverted = true;</code></pre> | <pre lang="csharp"><code>slider.SetInverted(true);</code></pre> |
| <pre lang="csharp"><code>slider.pageSize = 10f;</code></pre> | <pre lang="csharp"><code>slider.SetPageSize(10f);</code></pre> |
| <pre lang="csharp"><code>slider.showInputField = true;</code></pre> | <pre lang="csharp"><code>slider.SetShowInputField(true);</code></pre> |
| <pre lang="csharp"><code>slider.direction = SliderDirection.Vertical;</code></pre> | <pre lang="csharp"><code>slider.SetDirection(SliderDirection.Vertical);</code></pre> |

</details>

<details>
<summary>ProgressBar</summary>

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>progressBar.title = "Загрузка…";</code></pre> | <pre lang="csharp"><code>progressBar.SetTitle("Загрузка…");</code></pre> |
| <pre lang="csharp"><code>progressBar.lowValue = 0f;</code></pre> | <pre lang="csharp"><code>progressBar.SetLowValue(0f);</code></pre> |
| <pre lang="csharp"><code>progressBar.highValue = 100f;</code></pre> | <pre lang="csharp"><code>progressBar.SetHighValue(100f);</code></pre> |
| <pre lang="csharp"><code>progressBar.value = 42f;</code></pre> | <pre lang="csharp"><code>progressBar.SetValue(42f);</code></pre> |

</details>

<details>
<summary>HelpBox</summary>

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>helpBox.text = "Что-то пошло не так";</code></pre> | <pre lang="csharp"><code>helpBox.SetText("Что-то пошло не так");</code></pre> |
| <pre lang="csharp"><code>helpBox.messageType =&#10;    HelpBoxMessageType.Warning;</code></pre> | <pre lang="csharp"><code>helpBox.SetMessageType(&#10;    HelpBoxMessageType.Warning);</code></pre> |

</details>

<details>
<summary>EnumField / EnumFlagsField</summary>

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>enumField.Init(&#10;    Mode.Default, includeObsoleteValues: false);</code></pre> | <pre lang="csharp"><code>enumField.Initialize(&#10;    Mode.Default, includeObsoleteValues: false);</code></pre> |

</details>

<details>
<summary>Foldout</summary>

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>foldout.text = "Заголовок раздела";</code></pre> | <pre lang="csharp"><code>foldout.SetText("Заголовок раздела");</code></pre> |
| <pre lang="csharp"><code>foldout.toggleOnLabelClick = true;</code></pre> | <pre lang="csharp"><code>foldout.SetToggleOnLabelClick(true);</code></pre> |

</details>

<details>
<summary>Image</summary>

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>image.image = texture;</code></pre> | <pre lang="csharp"><code>image.SetImage(texture);</code></pre> |
| <pre lang="csharp"><code>image.image =&#10;    Resources.Load&lt;Texture&gt;("UI/Icon");</code></pre> | <pre lang="csharp"><code>image.SetImageFromResources("UI/Icon");</code></pre> |
| <pre lang="csharp"><code>image.sprite = sprite;</code></pre> | <pre lang="csharp"><code>image.SetSprite(sprite);</code></pre> |
| <pre lang="csharp"><code>image.sprite =&#10;    Resources.Load&lt;Sprite&gt;("UI/Icon");</code></pre> | <pre lang="csharp"><code>image.SetSpriteFromResources("UI/Icon");</code></pre> |
| <pre lang="csharp"><code>image.vectorImage = vectorImage;</code></pre> | <pre lang="csharp"><code>image.SetVectorImage(vectorImage);</code></pre> |
| <pre lang="csharp"><code>image.vectorImage =&#10;    Resources.Load&lt;VectorImage&gt;("UI/Icon");</code></pre> | <pre lang="csharp"><code>image.SetVectorImageFromResources("UI/Icon");</code></pre> |
| <pre lang="csharp"><code>image.uv = new Rect(0, 0, 1, 1);</code></pre> | <pre lang="csharp"><code>image.SetUv(new Rect(0, 0, 1, 1));</code></pre> |
| <pre lang="csharp"><code>image.sourceRect = sourceRect;</code></pre> | <pre lang="csharp"><code>image.SetSourceRect(sourceRect);</code></pre> |
| <pre lang="csharp"><code>image.tintColor = Color.white;</code></pre> | <pre lang="csharp"><code>image.SetTintColor(Color.white);</code></pre> |
| <pre lang="csharp"><code>image.scaleMode = ScaleMode.ScaleToFit;</code></pre> | <pre lang="csharp"><code>image.SetScaleMode(ScaleMode.ScaleToFit);</code></pre> |

</details>

<details>
<summary>IMGUIContainer</summary>

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>container.onGUIHandler = OnGUI;</code></pre> | <pre lang="csharp"><code>container.SetOnGUIHandler(OnGUI);</code></pre> |
| <pre lang="csharp"><code>container.onGUIHandler += OnGUI;</code></pre> | <pre lang="csharp"><code>container.AddOnGUIHandler(OnGUI);</code></pre> |
| <pre lang="csharp"><code>container.onGUIHandler -= OnGUI;</code></pre> | <pre lang="csharp"><code>container.RemoveOnGUIHandler(OnGUI);</code></pre> |
| <pre lang="csharp"><code>container.cullingEnabled = true;</code></pre> | <pre lang="csharp"><code>container.SetCullingEnabled(true);</code></pre> |
| <pre lang="csharp"><code>container.contextType = ContextType.Editor;</code></pre> | <pre lang="csharp"><code>container.SetContextType(ContextType.Editor);</code></pre> |
| <pre lang="csharp"><code>container.MarkDirtyLayout();</code></pre> | <pre lang="csharp"><code>container.MarkDirtyLayout();</code></pre> |

</details>

## Списки и деревья

Общие настройки доступны на `ListView`, `TreeView` и их `MultiColumn`-вариантах. `SetMakeItem`, `SetBindItem`, `SetUnbindItem` и `SetDestroyItem` относятся к обычным `ListView` и `TreeView`.

#### Данные и поведение BaseVerticalCollectionView

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>listView.itemsSource = items;</code></pre> | <pre lang="csharp"><code>listView.SetItemsSource(items);</code></pre> |
| <pre lang="csharp"><code>listView.reorderable = true;</code></pre> | <pre lang="csharp"><code>listView.SetReorderable(true);</code></pre> |
| <pre lang="csharp"><code>listView.selectedIndex = 0;</code></pre> | <pre lang="csharp"><code>listView.SetSelectedIndex(0);</code></pre> |
| <pre lang="csharp"><code>listView.selectionType = SelectionType.Single;</code></pre> | <pre lang="csharp"><code>listView.SetSelectionType(SelectionType.Single);</code></pre> |
| <pre lang="csharp"><code>listView.fixedItemHeight = 24;</code></pre> | <pre lang="csharp"><code>listView.SetFixedItemHeight(24);</code></pre> |
| <pre lang="csharp"><code>listView.virtualizationMethod =&#10;    CollectionVirtualizationMethod.DynamicHeight;</code></pre> | <pre lang="csharp"><code>listView.SetVirtualizationMethod(&#10;    CollectionVirtualizationMethod.DynamicHeight);</code></pre> |
| <pre lang="csharp"><code>listView.horizontalScrollingEnabled = true;</code></pre> | <pre lang="csharp"><code>listView.SetHorizontalScrollingEnabled(true);</code></pre> |
| <pre lang="csharp"><code>listView.showAlternatingRowBackgrounds =&#10;    AlternatingRowBackground.All;</code></pre> | <pre lang="csharp"><code>listView.SetShowAlternatingRowBackgrounds(&#10;    AlternatingRowBackground.All);</code></pre> |

#### События BaseVerticalCollectionView

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>listView.itemsChosen += OnItemsChosen;&#10;listView.itemsChosen -= OnItemsChosen;</code></pre> | <pre lang="csharp"><code>listView.AddItemsChosen(OnItemsChosen);&#10;listView.RemoveItemsChosen(OnItemsChosen);</code></pre> |
| <pre lang="csharp"><code>listView.selectionChanged += OnSelectionChanged;&#10;listView.selectionChanged -= OnSelectionChanged;</code></pre> | <pre lang="csharp"><code>listView.AddSelectionChanged(OnSelectionChanged);&#10;listView.RemoveSelectionChanged(OnSelectionChanged);</code></pre> |
| <pre lang="csharp"><code>listView.selectedIndicesChanged += OnIndicesChanged;&#10;listView.selectedIndicesChanged -= OnIndicesChanged;</code></pre> | <pre lang="csharp"><code>listView.AddSelectedIndicesChanged(OnIndicesChanged);&#10;listView.RemoveSelectedIndicesChanged(OnIndicesChanged);</code></pre> |
| <pre lang="csharp"><code>listView.itemIndexChanged += OnItemMoved;&#10;listView.itemIndexChanged -= OnItemMoved;</code></pre> | <pre lang="csharp"><code>listView.AddItemIndexChanged(OnItemMoved);&#10;listView.RemoveItemIndexChanged(OnItemMoved);</code></pre> |
| <pre lang="csharp"><code>listView.itemsSourceChanged += OnSourceChanged;&#10;listView.itemsSourceChanged -= OnSourceChanged;</code></pre> | <pre lang="csharp"><code>listView.AddItemsSourceChanged(OnSourceChanged);&#10;listView.RemoveItemsSourceChanged(OnSourceChanged);</code></pre> |
| <pre lang="csharp"><code>listView.canStartDrag += CanStartDrag;&#10;listView.canStartDrag -= CanStartDrag;</code></pre> | <pre lang="csharp"><code>listView.AddCanStartDrag(CanStartDrag);&#10;listView.RemoveCanStartDrag(CanStartDrag);</code></pre> |
| <pre lang="csharp"><code>listView.setupDragAndDrop += SetupDrag;&#10;listView.setupDragAndDrop -= SetupDrag;</code></pre> | <pre lang="csharp"><code>listView.AddSetupDragAndDrop(SetupDrag);&#10;listView.RemoveSetupDragAndDrop(SetupDrag);</code></pre> |
| <pre lang="csharp"><code>listView.dragAndDropUpdate += UpdateDrag;&#10;listView.dragAndDropUpdate -= UpdateDrag;</code></pre> | <pre lang="csharp"><code>listView.AddDragAndDropUpdate(UpdateDrag);&#10;listView.RemoveDragAndDropUpdate(UpdateDrag);</code></pre> |
| <pre lang="csharp"><code>listView.handleDrop += HandleDrop;&#10;listView.handleDrop -= HandleDrop;</code></pre> | <pre lang="csharp"><code>listView.AddHandleDrop(HandleDrop);&#10;listView.RemoveHandleDrop(HandleDrop);</code></pre> |

#### Настройка BaseListView

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>listView.allowAdd = true;&#10;listView.allowRemove = true;</code></pre> | <pre lang="csharp"><code>listView.SetAllowAdd(true).SetAllowRemove(true);</code></pre> |
| <pre lang="csharp"><code>listView.headerTitle = "Способности";</code></pre> | <pre lang="csharp"><code>listView.SetHeaderTitle("Способности");</code></pre> |
| <pre lang="csharp"><code>listView.showFoldoutHeader = true;</code></pre> | <pre lang="csharp"><code>listView.SetShowFoldoutHeader(true);</code></pre> |
| <pre lang="csharp"><code>listView.showAddRemoveFooter = true;</code></pre> | <pre lang="csharp"><code>listView.SetShowAddRemoveFooter(true);</code></pre> |
| <pre lang="csharp"><code>listView.showBoundCollectionSize = true;</code></pre> | <pre lang="csharp"><code>listView.SetShowBoundCollectionSize(true);</code></pre> |
| <pre lang="csharp"><code>listView.reorderMode = ListViewReorderMode.Animated;</code></pre> | <pre lang="csharp"><code>listView.SetReorderMode(ListViewReorderMode.Animated);</code></pre> |
| <pre lang="csharp"><code>listView.bindingSourceSelectionMode =&#10;    BindingSourceSelectionMode.AutoAssign;</code></pre> | <pre lang="csharp"><code>listView.SetBindingSourceSelectionMode(&#10;    BindingSourceSelectionMode.AutoAssign);</code></pre> |
| <pre lang="csharp"><code>listView.onAdd = OnAdd;&#10;listView.onAdd += OnAdd;&#10;listView.onAdd -= OnAdd;</code></pre> | <pre lang="csharp"><code>listView.SetOnAdd(OnAdd);&#10;listView.AddOnAdd(OnAdd);&#10;listView.RemoveOnAdd(OnAdd);</code></pre> |
| <pre lang="csharp"><code>listView.onRemove = OnRemove;&#10;listView.onRemove += OnRemove;&#10;listView.onRemove -= OnRemove;</code></pre> | <pre lang="csharp"><code>listView.SetOnRemove(OnRemove);&#10;listView.AddOnRemove(OnRemove);&#10;listView.RemoveOnRemove(OnRemove);</code></pre> |
| <pre lang="csharp"><code>listView.overridingAddButtonBehavior = OnAddButton;&#10;listView.overridingAddButtonBehavior += OnAddButton;&#10;listView.overridingAddButtonBehavior -= OnAddButton;</code></pre> | <pre lang="csharp"><code>listView.SetOverridingAddButtonBehavior(OnAddButton);&#10;listView.AddOverridingAddButtonBehavior(OnAddButton);&#10;listView.RemoveOverridingAddButtonBehavior(OnAddButton);</code></pre> |
| <pre lang="csharp"><code>listView.makeFooter = () =&gt; new Label();</code></pre> | <pre lang="csharp"><code>listView.SetMakeFooter(() =&gt; new Label());</code></pre> |
| <pre lang="csharp"><code>listView.makeHeader = () =&gt; new Label();</code></pre> | <pre lang="csharp"><code>listView.SetMakeHeader(() =&gt; new Label());</code></pre> |
| <pre lang="csharp"><code>listView.makeNoneElement =&#10;    () =&gt; new Label("Способностей нет");</code></pre> | <pre lang="csharp"><code>listView.SetMakeNoneElement(&#10;    () =&gt; new Label("Способностей нет"));</code></pre> |
| <pre lang="csharp"><code>listView.itemsAdded += OnItemsAdded;&#10;listView.itemsAdded -= OnItemsAdded;</code></pre> | <pre lang="csharp"><code>listView.AddItemsAdded(OnItemsAdded);&#10;listView.RemoveItemsAdded(OnItemsAdded);</code></pre> |
| <pre lang="csharp"><code>listView.itemsRemoved += OnItemsRemoved;&#10;listView.itemsRemoved -= OnItemsRemoved;</code></pre> | <pre lang="csharp"><code>listView.AddItemsRemoved(OnItemsRemoved);&#10;listView.RemoveItemsRemoved(OnItemsRemoved);</code></pre> |

#### Настройка BaseTreeView

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>treeView.autoExpand = true;</code></pre> | <pre lang="csharp"><code>treeView.SetAutoExpand(true);</code></pre> |
| <pre lang="csharp"><code>treeView.itemExpandedChanged += OnExpanded;&#10;treeView.itemExpandedChanged -= OnExpanded;</code></pre> | <pre lang="csharp"><code>treeView.AddItemExpandedChanged(OnExpanded);&#10;treeView.RemoveItemExpandedChanged(OnExpanded);</code></pre> |

#### Создание элементов ListView и TreeView

Эти методы дублируются в `ListViewExtensions` и `TreeViewExtensions` (каждое работает со своим типом view).

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>listView.makeItem = () =&gt; new Label();</code></pre> | <pre lang="csharp"><code>listView.SetMakeItem(() =&gt; new Label());</code></pre> |
| <pre lang="csharp"><code>listView.bindItem = BindRow;&#10;listView.bindItem += BindRow;&#10;listView.bindItem -= BindRow;</code></pre> | <pre lang="csharp"><code>listView.SetBindItem(BindRow);&#10;listView.AddBindItem(BindRow);&#10;listView.RemoveBindItem(BindRow);</code></pre> |
| <pre lang="csharp"><code>listView.unbindItem = UnbindRow;&#10;listView.unbindItem += UnbindRow;&#10;listView.unbindItem -= UnbindRow;</code></pre> | <pre lang="csharp"><code>listView.SetUnbindItem(UnbindRow);&#10;listView.AddUnbindItem(UnbindRow);&#10;listView.RemoveUnbindItem(UnbindRow);</code></pre> |
| <pre lang="csharp"><code>listView.destroyItem = DestroyRow;&#10;listView.destroyItem += DestroyRow;&#10;listView.destroyItem -= DestroyRow;</code></pre> | <pre lang="csharp"><code>listView.SetDestroyItem(DestroyRow);&#10;listView.AddDestroyItem(DestroyRow);&#10;listView.RemoveDestroyItem(DestroyRow);</code></pre> |
| <pre lang="csharp"><code>listView.itemTemplate = rowTemplate;</code></pre> | <pre lang="csharp"><code>listView.SetItemTemplate(rowTemplate);</code></pre> |

#### `MultiColumnListView` / `MultiColumnTreeView`

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>listView.sortingMode = ColumnSortingMode.Default;</code></pre> | <pre lang="csharp"><code>listView.SetSortingMode(ColumnSortingMode.Default);</code></pre> |
| <pre lang="csharp"><code>listView.columnSortingChanged += OnSortingChanged;&#10;listView.columnSortingChanged -= OnSortingChanged;</code></pre> | <pre lang="csharp"><code>listView.AddColumnSortingChanged(OnSortingChanged);&#10;listView.RemoveColumnSortingChanged(OnSortingChanged);</code></pre> |

## Расширения редактора

Расширения выше работают в редакторе и в игре. Привязка к `SerializedObject` и редакторские команды лежат в сборке `Aspid.FastTools.UIElements.Editors`, поэтому такой код размещайте в editor-сборке, например в папке `Editor`. Добавьте в этот скрипт `using Aspid.FastTools.UIElements.Editors;` и `using UnityEditor.UIElements;`.

### Привязка к SerializedObject

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>field.bindingPath = "_manaCost";&#10;field.Bind(serializedObject);</code></pre> | <pre lang="csharp"><code>field.BindTo(&#10;    serializedObject, "_manaCost");</code></pre> |
| <pre lang="csharp"><code>var property = serializedObject&#10;    .FindProperty("_manaCost");&#10;field.BindProperty(property);</code></pre> | <pre lang="csharp"><code>var property = serializedObject&#10;    .FindProperty("_manaCost");&#10;field.BindPropertyTo(property);</code></pre> |
| <pre lang="csharp"><code>root.Bind(serializedObject);</code></pre> | <pre lang="csharp"><code>root.BindTo(serializedObject);</code></pre> |
| <pre lang="csharp"><code>root.Unbind();</code></pre> | <pre lang="csharp"><code>root.UnbindFrom();</code></pre> |

### PropertyField

`PropertyField.AddValueChanged` получает `SerializedPropertyChangeEvent`. У обычного `IntegerField.AddValueChanged` аргументом будет `ChangeEvent<int>`:

```csharp
var manaCost = serializedObject.FindProperty("_manaCost");
var field = new PropertyField(manaCost)
    .SetLabel("Mana cost")
    .AddValueChanged(evt =>
        Debug.Log(evt.changedProperty.intValue));
```

Для записи в свойство из собственного кода используйте [SerializedProperty Extensions](08-serialized-property-extensions.md).

### Открытие скрипта и окно-владелец

Двойной клик по элементу открывает в IDE скрипт `target` — `MonoBehaviour` или `ScriptableObject`.

```csharp
image.AddOpenScriptCommand(target);
```

`GetOwnerWindow()` ищет окно по панели элемента. Если найти его не удалось, возвращает окно в фокусе, затем окно под курсором; результат может быть `null`. Это полезно при позиционировании попапа, когда клик уже пришёл, а фокус ещё не переключился.

```csharp
var window = image.GetOwnerWindow();
```

## Собственные свойства USS

Чтение строкового свойства USS как enum в `CustomStyleResolvedEvent`:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>if (evt.customStyle.TryGetValue(ThemeProperty, out var raw)&#10;    &amp;&amp; Enum.TryParse(raw, ignoreCase: true, out PanelTheme theme))&#10;    ApplyTheme(theme);</code></pre> | <pre lang="csharp"><code>if (evt.customStyle.TryGetByEnum(ThemeProperty, out PanelTheme theme))&#10;    ApplyTheme(theme);</code></pre> |

## Практический пример

В [EditorTools](../../Samples~/EditorTools/Documentation/README.ru.md) собраны каталог способностей и реактивный инспектор, построенные на этих расширениях:

![Halve cooldown, +5 MP обновляет поля и описание эффекта; Undo возвращает прежние значения.](../../Samples~/EditorTools/Documentation/Images/demo.gif)

Halve cooldown, +5 MP обновляет поля и описание эффекта; Undo возвращает прежние значения.


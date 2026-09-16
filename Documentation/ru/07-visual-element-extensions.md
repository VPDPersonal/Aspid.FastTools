# VisualElement Extensions

Расширения UI Toolkit для построения деревьев элементов, настройки стилей, подписки на события и привязки полей в редакторе. Методы возвращают настраиваемый элемент, чтобы объединять вызовы в цепочки.

## Быстрый старт

Добавьте `using Aspid.FastTools.UIElements;` к скрипту с `using UnityEngine.UIElements;`. Так выглядит одна и та же панель с заголовком:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>var title = new Label("Stats");&#10;title.style.fontSize = 18;&#10;&#10;var panel = new VisualElement();&#10;panel.style.paddingLeft = 12;&#10;panel.style.paddingRight = 12;&#10;panel.style.paddingTop = 8;&#10;panel.style.paddingBottom = 8;&#10;panel.Add(title);</code></pre> | <pre lang="csharp"><code>var panel = new VisualElement()&#10;    .SetPaddingX(12)&#10;    .SetPaddingY(8)&#10;    .AddChild(new Label("Stats")&#10;        .SetFontSize(18));</code></pre> |

Добавьте `panel` в `rootVisualElement` окна редактора или в `UIDocument.rootVisualElement` игрового интерфейса.

<details>
<summary>Полный пример: окно Stats с кнопкой</summary>

Создайте `StatsWindow.cs` в папке `Editor`:

```csharp
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;

public sealed class StatsWindow : EditorWindow
{
    [MenuItem("Tools/Stats")]
    private static void Open() => GetWindow<StatsWindow>("Stats");

    public void CreateGUI()
    {
        rootVisualElement
            .SetPadding(12)
            .AddChild(new Label("Stats").SetFontSize(18))
            .AddChild(new Button(() => Debug.Log("Refresh"))
                .SetText("Refresh")
                .SetMarginTop(8));
    }
}
```

Откройте **Tools → Stats**. Кнопка **Refresh** выведет сообщение в Console.

</details>

### Как читать цепочки

Сеттеры сохраняют тип: `new Button().SetText("Refresh")` возвращает `Button`. Операции с дочерними узлами возвращают **родителя** — следующий вызов продолжает настраивать его:

```csharp
var panel = new VisualElement()
    .AddChild(new Label("Health").SetFontSize(14))
    .SetMarginTop(12); // отступ у panel
```

Цепочки на `element.style`, `textField.textEdition` и `textField.textSelection` возвращают соответствующий интерфейс. Методы-запросы `IsFocused()`, `GetOwnerWindow()` и `TryGetByEnum(...)` цепочку не продолжают: они возвращают `bool` или найденное окно.

Основные расширения работают в редакторе и в игре. Для привязки к `SerializedObject` и редакторских команд дополнительно нужен `Aspid.FastTools.UIElements.Editors`; такой код размещайте в editor-сборке, например в папке `Editor`.

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

Для `panel` из быстрого старта. Дочерние элементы создаются на месте; `*If` добавляет элемент только при истинном условии:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>panel.name = "ability-panel";&#10;panel.Add(new Label("Fireball"));&#10;panel.Add(new Label("Deals 40 damage"));&#10;if (Application.isPlaying)&#10;    panel.Add(new Label("Play mode"));</code></pre> | <pre lang="csharp"><code>panel&#10;    .SetName("ability-panel")&#10;    .AddChildren(&#10;        new Label("Fireball"),&#10;        new Label("Deals 40 damage"))&#10;    .AddChildIf(Application.isPlaying,&#10;        new Label("Play mode"));</code></pre> |
| <pre lang="csharp"><code>var header = new Label("Header");&#10;panel.Insert(0, header);&#10;panel.Remove(header);&#10;panel.RemoveAt(0);&#10;panel.Clear();</code></pre> | <pre lang="csharp"><code>var header = new Label("Header");&#10;panel&#10;    .InsertChild(0, header)&#10;    .RemoveChild(header)&#10;    .RemoveChildAt(0)&#10;    .ClearChildren();</code></pre> |

Эти методы возвращают родительский элемент, поэтому их можно объединять в цепочку. `AddChildren` и `InsertChildren` сохраняют порядок переданных элементов.

> [!NOTE]
> `*If` проверяет условие только в момент вызова. Аргументы вычисляются заранее: `AddChildIf(false, new Label("Warning"))` создаст `Label`, но не добавит его в дерево. Для дорогого создания используйте обычный `if`.

<details>
<summary>Все методы элемента и операции с дочерними узлами</summary>

| Метод | Описание |
|-------|----------|
| `SetName(string)` | Устанавливает `element.name` |
| `SetVisible(bool)` | Устанавливает `element.visible` |
| `SetTooltip(string)` | Устанавливает `element.tooltip` |
| `SetUserData(object)` | Устанавливает `element.userData` |
| `SetEnabledSelf(bool)` | Вызывает `element.SetEnabled`, управляя доступностью элемента |
| `SetPickingMode(PickingMode)` | Устанавливает `element.pickingMode` |
| `SetUsageHints(UsageHints)` | Устанавливает `element.usageHints`; задайте до подключения элемента к панели |
| `SetViewDataKey(string)` | Устанавливает `element.viewDataKey` |
| `SetLanguageDirection(LanguageDirection)` | Устанавливает `element.languageDirection` |
| `SetDisablePlayModeTint(bool)` | Устанавливает `element.disablePlayModeTint` |
| `SetDataSource(object)` | Устанавливает `element.dataSource` |
| `SetDataSourceType(Type)` | Устанавливает `element.dataSourceType` |
| `SetDataSourcePath(PropertyPath)` | Устанавливает `element.dataSourcePath` |
| `AddChild(VisualElement)` | Добавляет дочерний элемент, возвращает родителя |
| `AddChildren(params VisualElement[])` | Добавляет несколько дочерних элементов |
| `InsertChild(int, VisualElement)` | Вставляет дочерний элемент по указанному индексу |
| `InsertChildren(int, params VisualElement[])` | Вставляет несколько дочерних элементов начиная с индекса |
| `RemoveChild(VisualElement)` | Удаляет дочерний элемент, возвращает родителя |
| `RemoveChildAt(int)` | Удаляет дочерний элемент по указанному индексу |
| `ClearChildren()` | Удаляет все дочерние элементы |

`AddChildren` и `InsertChildren` принимают `params VisualElement[]`, `IEnumerable<VisualElement>`, `List<VisualElement>`, `Span<VisualElement>` и `ReadOnlySpan<VisualElement>`.

> У каждой операции с дочерними элементами есть `*If`-вариант (`AddChildIf`, `AddChildrenIf`, `InsertChildIf`, `InsertChildrenIf`, `RemoveChildIf`, `RemoveChildAtIf`, `ClearChildrenIf`) с ведущим параметром `bool condition` — операция выполняется только при `condition == true`.

</details>

### Видимость и доступность

Выберите поведение при скрытии или блокировке:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>// Скрыть, сохранив место в раскладке&#10;element.visible = false;</code></pre> | <pre lang="csharp"><code>// Скрыть, сохранив место в раскладке&#10;element.SetVisible(false);</code></pre> |
| <pre lang="csharp"><code>// Убрать элемент и потомков из раскладки&#10;element.style.display =&#10;    DisplayStyle.None;</code></pre> | <pre lang="csharp"><code>// Убрать элемент и потомков из раскладки&#10;element.SetDisplay(DisplayStyle.None);</code></pre> |
| <pre lang="csharp"><code>// Отключить взаимодействие с элементом и потомками&#10;element.SetEnabled(false);</code></pre> | <pre lang="csharp"><code>// Отключить взаимодействие с элементом и потомками&#10;element.SetEnabledSelf(false);</code></pre> |

Чтобы вернуть элемент, используйте `SetVisible(true)`, `SetDisplay(DisplayStyle.Flex)` или `SetEnabledSelf(true)` соответственно. Доступность дочернего элемента также зависит от доступности его родителей.

## Фокус

Для поля поиска, уже подключённого к `panel`:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>var search = panel.Q&lt;TextField&gt;("search");&#10;search.focusable = true;&#10;search.tabIndex = 0;&#10;search.Focus();</code></pre> | <pre lang="csharp"><code>panel.Q&lt;TextField&gt;("search")&#10;    .SetFocusable(true)&#10;    .SetTabIndex(0)&#10;    .FocusSelf();</code></pre> |

`FocusSelf()` вызывает обычный `Focus()`: элемент должен поддерживать фокус. `IsFocused()` сравнивает элемент с `focusController.focusedElement`; для отсоединённого элемента возвращает `false`.

| Метод | Описание |
|-------|----------|
| `FocusSelf()` | Устанавливает фокус на элемент |
| `BlurSelf()` | Снимает фокус с элемента |
| `IsFocused()` | Возвращает, находится ли элемент в фокусе |
| `SetTabIndex(int)` | Устанавливает `element.tabIndex` |
| `SetFocusable(bool)` | Устанавливает `element.focusable` |
| `SetDelegatesFocus(bool)` | Устанавливает `element.delegatesFocus` |

## USS и классы

Таблица стилей `Assets/Resources/UI/AbilityCard.uss` подключается к `panel`, а класс `playing` включается по состоянию:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>panel.styleSheets.Add(&#10;    Resources.Load&lt;StyleSheet&gt;("UI/AbilityCard"));&#10;panel.AddToClassList("ability-card");&#10;panel.EnableInClassList(&#10;    "playing", Application.isPlaying);</code></pre> | <pre lang="csharp"><code>panel&#10;    .AddStyleSheetFromResources("UI/AbilityCard")&#10;    .AddClass("ability-card")&#10;    .EnableClass("playing", Application.isPlaying);</code></pre> |

`EnableClass` приводит класс к заданному состоянию. `ToggleClass` каждый раз меняет наличие класса на противоположное.

<details>
<summary>Методы классов и таблиц стилей</summary>

| Метод | Описание |
|-------|----------|
| `AddClass(string)` | Добавляет USS-класс |
| `RemoveClass(string)` | Удаляет USS-класс |
| `ClearClasses()` | Удаляет все USS-классы |
| `ToggleClass(string)` | Переключает USS-класс вкл/выкл |
| `EnableClass(string, bool)` | Добавляет или удаляет USS-класс по условию |
| `AddStyleSheet(StyleSheet)` | Добавляет `StyleSheet` |
| `RemoveStyleSheet(StyleSheet)` | Удаляет `StyleSheet` |
| `AddStyleSheetFromResources(string)` | Добавляет таблицу стилей через `Resources.Load` |
| `RemoveStyleSheetFromResources(string)` | Удаляет таблицу стилей, загруженную через `Resources.Load` |

</details>

`AddStyleSheetFromResources` принимает путь внутри папки `Resources` без расширения. Если ресурс не найден, метод выведет предупреждение и вернёт элемент без изменений.

## Стили

Сеттеры записывают inline-стили элемента. Общие значения оформления удобно хранить в USS, а через цепочки задавать размеры и состояния конкретного элемента.

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>panel.style.flexDirection =&#10;    FlexDirection.Row;&#10;panel.style.alignItems = Align.Center;&#10;panel.style.width = 240;&#10;panel.style.height = 48;&#10;panel.style.marginTop = 8;</code></pre> | <pre lang="csharp"><code>panel&#10;    .SetFlexDirection(FlexDirection.Row)&#10;    .SetAlignItems(Align.Center)&#10;    .SetSize(240, 48)&#10;    .SetMarginTop(8);</code></pre> |

### Стороны, оси и единицы измерения

Общее значение задаёт все стороны; `X` — левую и правую, `Y` — верхнюю и нижнюю. В перегрузках с необязательными параметрами пропущенные стороны сохраняют прежнее значение:

```csharp
panel
    .SetPadding(8)               // все стороны
    .SetPaddingX(12)             // слева и справа
    .SetMargin(top: 4, bottom: 8)
    .SetSize(width: Length.Percent(100));
```

Числа для `StyleLength` задаются в пикселях; для процентов используйте `Length.Percent(...)`. `SetDistance` записывает `top`, `right`, `bottom`, `left` — например, `SetPosition(Position.Absolute).SetDistance(0)` растягивает элемент по границам родителя.

### Настройка через IStyle

Те же методы доступны на `element.style`. Такая цепочка возвращает `IStyle`, поэтому продолжать её методами элемента нужно отдельным вызовом:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>panel.style.paddingLeft = 12;&#10;panel.style.paddingRight = 12;&#10;panel.style.height = 48;</code></pre> | <pre lang="csharp"><code>panel.style&#10;    .SetPaddingX(12)&#10;    .SetHeight(48);</code></pre> |

### Справочник стилей

Основные примеры рассчитаны на Unity 6.0. В раскрывающихся блоках отмечены методы для более новых версий Unity.

<details>
<summary>Раскладка</summary>

| Метод | Свойство стиля |
|-------|----------------|
| `SetFlexBasis(StyleLength)` | `flexBasis` |
| `SetFlexGrow(StyleFloat)` | `flexGrow` |
| `SetFlexShrink(StyleFloat)` | `flexShrink` |
| `SetFlexWrap(StyleEnum<Wrap>)` | `flexWrap` |
| `SetFlexDirection(FlexDirection)` | `flexDirection` |
| `SetAlignSelf(StyleEnum<Align>)` | `alignSelf` |
| `SetAlignItems(StyleEnum<Align>)` | `alignItems` |
| `SetAlignContent(StyleEnum<Align>)` | `alignContent` |
| `SetJustifyContent(StyleEnum<Justify>)` | `justifyContent` |
| `SetPosition(StyleEnum<Position>)` | `position` |

</details>

<details>
<summary>Размеры</summary>

| Метод | Описание |
|-------|----------|
| `SetSize(StyleLength)` | Устанавливает ширину и высоту одновременно |
| `SetSize(width?, height?)` | Устанавливает ширину и/или высоту независимо |
| `SetMinSize(StyleLength)` | Устанавливает minWidth и minHeight одновременно |
| `SetMinSize(minWidth?, minHeight?)` | Минимальная ширина и/или высота независимо |
| `SetMaxSize(StyleLength)` | Устанавливает maxWidth и maxHeight одновременно |
| `SetMaxSize(maxWidth?, maxHeight?)` | Максимальная ширина и/или высота независимо |
| `SetWidth(StyleLength)` | `width` |
| `SetMinWidth(StyleLength)` | `minWidth` |
| `SetMaxWidth(StyleLength)` | `maxWidth` |
| `SetHeight(StyleLength)` | `height` |
| `SetMinHeight(StyleLength)` | `minHeight` |
| `SetMaxHeight(StyleLength)` | `maxHeight` |

</details>

<details>
<summary>Отступы и позиционирование</summary>

Для `SetMargin`, `SetPadding` и `SetDistance` доступны общее значение, отдельные стороны (`top`, `right`, `bottom`, `left`) и пары осей X/Y.

| Метод | Свойства стиля |
|-------|----------------|
| `SetMargin(…)` / `SetPadding(…)` / `SetDistance(…)` | `Top/Right/Bottom/Left` (общее значение или по стороне) |
| `SetMarginX/Y` · `SetPaddingX/Y` · `SetDistanceX/Y` | Устанавливает горизонтальную (X = `Left`+`Right`) или вертикальную (Y = `Top`+`Bottom`) пару |
| `SetMarginTop/Right/Bottom/Left` | Margin одной стороны |
| `SetPaddingTop/Right/Bottom/Left` | Padding одной стороны |
| `SetTop` / `SetRight` / `SetBottom` / `SetLeft` | Смещение одной стороны (`top` / `right` / `bottom` / `left`) |

> `SetDistance` — обёртка для четырёх свойств `top`/`right`/`bottom`/`left`, используемых при абсолютном позиционировании. `SetTop`, `SetRight`, `SetBottom`, `SetLeft` — это прямые алиасы для одного свойства.

</details>

<details>
<summary>Шрифт</summary>

| Метод | Свойство стиля |
|-------|----------------|
| `SetUnityFont(StyleFont)` | `unityFont` |
| `SetFontSize(StyleLength)` | `fontSize` |
| `SetUnityFontDefinition(StyleFontDefinition)` | `unityFontDefinition` |
| `SetUnityFontStyleAndWeight(StyleEnum<FontStyle>)` | `unityFontStyleAndWeight` |

</details>

<details>
<summary>Начертание шрифта</summary>

Удобные методы для переключения bold / italic без перезаписи другого флага:

| Метод | Описание |
|-------|----------|
| `SetNormalUnityFontStyleAndWeight()` | Сбрасывает в `FontStyle.Normal` |
| `AddBoldUnityFontStyleAndWeight()` | Добавляет bold, сохраняя italic |
| `RemoveBoldUnityFontStyleAndWeight()` | Убирает bold, сохраняя italic |
| `AddItalicUnityFontStyleAndWeight()` | Добавляет italic, сохраняя bold |
| `RemoveItalicUnityFontStyleAndWeight()` | Убирает italic, сохраняя bold |

</details>

<details>
<summary>Текст</summary>

| Метод | Свойство стиля | Примечания |
|-------|---------------|------------|
| `SetWordSpacing(StyleLength)` | `wordSpacing` | |
| `SetLetterSpacing(StyleLength)` | `letterSpacing` | |
| `SetUnityTextAlign(TextAnchor)` | `unityTextAlign` | |
| `SetTextShadow(StyleTextShadow)` | `textShadow` | |
| `SetUnityTextOutlineColor(StyleColor)` | `unityTextOutlineColor` | |
| `SetUnityTextOutlineWidth(StyleFloat)` | `unityTextOutlineWidth` | |
| `SetUnityParagraphSpacing(StyleLength)` | `unityParagraphSpacing` | |
| `SetTextOverflow(StyleEnum<TextOverflow>)` | `textOverflow` | |
| `SetUnityTextOverflowPosition(TextOverflowPosition)` | `unityTextOverflowPosition` | |
| `SetUnityTextGenerator(TextGeneratorType)` | `unityTextGenerator` | |
| `SetUnityEditorTextRenderingMode(EditorTextRenderingMode)` | `unityEditorTextRenderingMode` | |
| `SetUnityTextAutoSize(StyleTextAutoSize)` | `unityTextAutoSize` | Unity 6.2+ |
| `SetWhiteSpace(StyleEnum<WhiteSpace>)` | `whiteSpace` | |

</details>

<details>
<summary>Цвет и прозрачность</summary>

| Метод | Свойство стиля |
|-------|----------------|
| `SetColor(StyleColor)` | `color` |
| `SetColor(string)` | `color`, разобранный из HTML-строки (`"#RRGGBB"` или именованный цвет) |
| `SetOpacity(StyleFloat)` | `opacity` |

</details>

<details>
<summary>Рамка</summary>

| Метод | Описание |
|-------|----------|
| `SetBorderColor(StyleColor)` | Все стороны |
| `SetBorderColor(top?, right?, bottom?, left?)` | По стороне |
| `SetBorderColorX(StyleColor)` · `SetBorderColorY(StyleColor)` | Горизонтальная (left + right) или вертикальная (top + bottom) пара |
| `SetBorderColorTop/Right/Bottom/Left(StyleColor)` | Одна сторона |
| `SetBorderRadius(StyleLength)` | Все углы |
| `SetBorderRadius(topLeft?, topRight?, bottomLeft?, bottomRight?)` | По углу |
| `SetBorderRadiusTop(StyleLength)` · `SetBorderRadiusBottom(StyleLength)` | Пара верхних или нижних углов |
| `SetBorderRadiusTopLeft/TopRight/BottomLeft/BottomRight(StyleLength)` | Один угол |
| `SetBorderWidth(StyleFloat)` | Все стороны |
| `SetBorderWidth(top?, right?, bottom?, left?)` | По стороне |
| `SetBorderWidthX(StyleFloat)` · `SetBorderWidthY(StyleFloat)` | Горизонтальная или вертикальная пара |
| `SetBorderWidthTop/Right/Bottom/Left(StyleFloat)` | Одна сторона |

</details>

<details>
<summary>Фон</summary>

| Метод | Свойство стиля |
|-------|----------------|
| `SetBackgroundColor(StyleColor)` | `backgroundColor` |
| `SetBackgroundColor(string)` | `backgroundColor`, разобранный из HTML-строки (`"#RRGGBB"` или именованный цвет) |
| `SetBackgroundImage(StyleBackground)` | `backgroundImage` |
| `SetBackgroundImageFromResources(string)` | Загружает `Texture2D` через `Resources.Load` и присваивает его в `backgroundImage` |
| `SetBackgroundSize(StyleBackgroundSize)` | `backgroundSize` |
| `SetBackgroundRepeat(StyleBackgroundRepeat)` | `backgroundRepeat` |
| `SetBackgroundPosition(StyleBackgroundPosition)` | X и Y одновременно |
| `SetBackgroundPosition(x?, y?)` | Независимо |
| `SetBackgroundPositionX(StyleBackgroundPosition)` | `backgroundPositionX` |
| `SetBackgroundPositionY(StyleBackgroundPosition)` | `backgroundPositionY` |
| `SetUnityBackgroundImageTintColor(StyleColor)` | `unityBackgroundImageTintColor` |

</details>

<details>
<summary>Трансформации</summary>

| Метод | Свойство стиля |
|-------|----------------|
| `SetScale(StyleScale)` | `scale` |
| `SetRotate(StyleRotate)` | `rotate` |
| `SetTranslate(StyleTranslate)` | `translate` |
| `SetTransformOrigin(StyleTransformOrigin)` | `transformOrigin` |

</details>

<details>
<summary>Пропорции, фильтры и материал</summary>

Доступно начиная с Unity 6000.3+.

| Метод | Свойство стиля |
|-------|----------------|
| `SetAspectRatio(StyleRatio)` | `aspectRatio` |
| `SetFilter(StyleList<FilterFunction>)` | `filter` |
| `SetUnityMaterial(StyleMaterialDefinition)` | `unityMaterial` |

</details>

<details>
<summary>Переходы</summary>

| Метод | Свойство стиля |
|-------|----------------|
| `SetTransitionDelay(StyleList<TimeValue>)` | `transitionDelay` |
| `SetTransitionDuration(StyleList<TimeValue>)` | `transitionDuration` |
| `SetTransitionProperty(StyleList<StylePropertyName>)` | `transitionProperty` |
| `SetTransitionTimingFunction(StyleList<EasingFunction>)` | `transitionTimingFunction` |

</details>

<details>
<summary>Переполнение и видимость</summary>

| Метод | Свойство стиля |
|-------|----------------|
| `SetOverflow(StyleEnum<Overflow>)` | `overflow` |
| `SetUnityOverflowClipBox(StyleEnum<OverflowClipBox>)` | `unityOverflowClipBox` |
| `SetVisibility(StyleEnum<Visibility>)` | `visibility` |
| `SetDisplay(DisplayStyle)` | `display` |

</details>

<details>
<summary>Нарезка изображения</summary>

| Метод | Описание |
|-------|----------|
| `SetUnitySlice(StyleInt)` | Все стороны |
| `SetUnitySlice(top?, right?, bottom?, left?)` | По стороне |
| `SetUnitySliceX(StyleInt)` · `SetUnitySliceY(StyleInt)` | Горизонтальная (left + right) или вертикальная (top + bottom) пара |
| `SetUnitySliceTop/Right/Bottom/Left(StyleInt)` | Одна сторона |
| `SetUnitySliceScale(StyleFloat)` | `unitySliceScale` |
| `SetUnitySliceType(StyleEnum<SliceType>)` | `unitySliceType` |

</details>

<details>
<summary>Курсор</summary>

| Метод | Свойство стиля |
|-------|----------------|
| `SetCursor(StyleCursor)` | `cursor` |

</details>

## Значения и события

### Значение поля

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>var field = new IntegerField("Mana cost");&#10;field.value = 42;&#10;field.SetValueWithoutNotify(10);</code></pre> | <pre lang="csharp"><code>var field = new IntegerField("Mana cost");&#10;field.SetValue(42);&#10;field.SetValue(10, notify: false);</code></pre> |

По умолчанию `SetValue` присваивает `value` и сохраняет поведение событий Unity. `notify: false` вызывает `SetValueWithoutNotify`: поле обновляется без отправки `ChangeEvent`. Это удобно при синхронизации интерфейса с данными.

### Подписка и отписка

Сохраните обработчик, если понадобится его удалить:

```csharp
var status = new Label();
EventCallback<ChangeEvent<int>> onChanged =
    evt => status.SetText($"Mana: {evt.newValue}");
```

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>field.RegisterValueChangedCallback(&#10;    onChanged);&#10;&#10;// Когда обработчик больше не нужен&#10;field.UnregisterValueChangedCallback(&#10;    onChanged);</code></pre> | <pre lang="csharp"><code>field.AddValueChanged(onChanged);&#10;&#10;&#10;// Когда обработчик больше не нужен&#10;field.RemoveValueChanged(onChanged);</code></pre> |

При отписке передавайте тот же делегат. Новая лямбда с похожим кодом не удалит прежнюю подписку.

<details>
<summary>Типы значений и интеграция с Unity.Mathematics</summary>

Типизированные перегрузки доступны для `int`, `uint`, `nint`, `nuint`, `long`, `ulong`, `short`, `ushort`, `byte`, `sbyte`, `float`, `double`, `decimal`, `char`, `string`, `bool`, `Color`, `Vector2/3/4`, `Vector2Int/3Int`, `Rect/RectInt`, `Bounds/BoundsInt`, `Hash128`, `GUID` (Unity 6.4+), `Quaternion`, `Matrix4x4`, `Gradient`, `AnimationCurve`, `Delegate`, `Enum`, `Object`, `object`, а также обобщённый вариант `SetValue<T, TValue>` для остальных типов.

> При установленном пакете `com.unity.mathematics` автоматически выставляется define `ASPID_FASTTOOLS_UNITY_MATHEMATICS_INTEGRATION` и добавляются перегрузки `SetValue` / `AddValueChanged` / `RemoveValueChanged` для `int2/3/4` (и `intMxN`), `float2/3/4` (и `floatMxN`), `half`/`half2/3/4`, `bool2/3/4` (и `boolMxN`), а также `quaternion`.

</details>

### Кнопки и манипуляторы

```csharp
var button = new Button();
void Refresh() => Debug.Log("Refresh");
```

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>button.text = "Refresh";&#10;button.clicked += Refresh;&#10;&#10;// Отписка&#10;button.clicked -= Refresh;</code></pre> | <pre lang="csharp"><code>button&#10;    .SetText("Refresh")&#10;    .AddClicked(Refresh);&#10;// Отписка&#10;button.RemoveClicked(Refresh);</code></pre> |

Обычный `VisualElement` тоже можно сделать кликабельным. Перегрузка с `out` позволяет сохранить манипулятор для удаления:

```csharp
panel.AddClickable(Refresh, out var clickable);

// Когда клик больше не нужен
panel.RemoveManipulatorSelf(clickable);
```

| Метод | Задача |
|---|---|
| `AddManipulatorSelf` / `RemoveManipulatorSelf` | Добавить или удалить готовый `IManipulator` |
| `AddClickable` | Обработать клик; доступны перегрузки с событием и повторением по `delay` / `interval` |
| `AddKeyboardNavigationManipulator` | Обработать клавиатурную навигацию |
| `AddContextualMenuManipulator` | Заполнить контекстное меню |

У методов создания манипулятора есть перегрузки с `out`. Для остальных событий используйте стандартные `RegisterCallback` и `UnregisterCallback` UI Toolkit.

## Конкретные элементы

Откройте нужный тип: внутри — пример настройки и доступные методы.

<details>
<summary>TextElement</summary>

```csharp
label
    .SetText("Hello World")
    .SetEnableRichText(true)
    .SetParseEscapeSequences(true);
```

| Метод | Описание |
|-------|----------|
| `SetText(string)` | Устанавливает отображаемый текст |
| `SetEnableRichText(bool)` | Включает разбор тегов rich-text |
| `SetEmojiFallbackSupport(bool)` | Включает emoji-fallback при рендеринге |
| `SetParseEscapeSequences(bool)` | Обрабатывать ли escape-последовательности (например, `\n`) |
| `SetDisplayTooltipWhenElided(bool)` | Показывать обрезанный текст в подсказке при наведении |

</details>

<details>
<summary>ITextEdition (TextField, IntegerField, …)</summary>

У текстового поля обращайтесь к `textEdition`. Цепочка возвращает этот интерфейс, а не само поле.

```csharp
textField.textEdition
    .SetPlaceholder("Поиск…")
    .SetMaxLength(64)
    .SetDelayed(true);
```

| Метод | Описание |
|-------|----------|
| `SetMaxLength(int)` | Максимальное число символов |
| `SetMaskChar(char)` | Символ для маскировки пароля |
| `SetDelayed(bool)` | Откладывает изменение значения до потери фокуса / Enter |
| `SetReadOnly(bool)` | Запрещает редактирование |
| `SetPassword(bool)` | Включает password-режим (использует mask char) |
| `SetPlaceholder(string)` | Текст-плейсхолдер для пустого поля |
| `SetAutoCorrection(bool)` | Включает автокоррекцию (mobile) |
| `SetHideMobileInput(bool)` | Скрывает нативное поле ввода на мобильном устройстве |
| `SetHideSoftKeyboard(bool)` | Скрывает экранную клавиатуру (Unity 6.4+) |
| `SetHidePlaceholderOnFocus(bool)` | Убирает плейсхолдер при фокусе |
| `SetKeyboardType(TouchScreenKeyboardType)` | Тип touch-screen клавиатуры |

</details>

<details>
<summary>ITextSelection</summary>

У текстового поля настройка выделения доступна через `textSelection`.

```csharp
textField.textSelection
    .SetSelectable(true)
    .SetSelectAllOnFocus(true);
```

| Метод | Описание |
|-------|----------|
| `AddOnCursorIndexChange(Action)` / `RemoveOnCursorIndexChange(Action)` | Подписка на изменение позиции курсора (Unity 6.3+) |
| `AddOnSelectIndexChange(Action)` / `RemoveOnSelectIndexChange(Action)` | Подписка на изменение якоря выделения (Unity 6.3+) |
| `SetCursorIndex(int)` | Текущая позиция курсора |
| `SetSelectIndex(int)` | Текущий якорь выделения |
| `SetSelectable(bool)` | Можно ли выделять текст |
| `SetSelectAllOnFocus(bool)` | Выделять весь текст при фокусе |
| `SetSelectAllOnMouseUp(bool)` | Выделять весь текст при отпускании мыши |
| `SetDoubleClickSelectsWord(bool)` | Двойной клик выделяет слово |
| `SetTripleClickSelectsLine(bool)` | Тройной клик выделяет строку |

</details>

<details>
<summary>BaseField&lt;TValueType&gt;</summary>

```csharp
var field = new IntegerField()
    .SetLabel("Mana cost")
    .SetValue(42, notify: false);
```

</details>

<details>
<summary>BaseBoolField (Toggle)</summary>

```csharp
toggle
    .SetLabel("Включено")
    .SetText("Показать расширенные настройки")
    .SetToggleOnLabelClick(true);
```

| Метод | Описание |
|-------|----------|
| `SetText(string)` | Устанавливает текст рядом с чекбоксом |
| `SetLabel(string)` | Устанавливает label поля |
| `SetToggleOnLabelClick(bool)` | Переключать ли значение по клику на label |

</details>

<details>
<summary>IMixedValueSupport</summary>

```csharp
field.SetShowMixedValue(true); // показывает индикатор смешанного значения
```

</details>

<details>
<summary>Button</summary>

```csharp
button
    .AddClicked(() => Debug.Log("Clicked"))
    .SetIconImage(myBackground);
```

| Метод | Описание |
|-------|----------|
| `AddClicked(Action)` | Подписка на `Button.clicked` |
| `RemoveClicked(Action)` | Отписка от `Button.clicked` |
| `SetClickable(Clickable)` | Заменяет манипулятор клика. Настраивайте его до подписок `AddClicked` |
| `SetIconImage(Background)` | Устанавливает `Button.iconImage` |

</details>

<details>
<summary>Slider / BaseSlider&lt;TValue&gt;</summary>

```csharp
slider
    .SetLowValue(0f)
    .SetHighValue(100f)
    .SetShowInputField(true);
```

| Метод | Описание |
|-------|----------|
| `SetLowValue(TValue)` | Устанавливает минимальное значение слайдера |
| `SetHighValue(TValue)` | Устанавливает максимальное значение слайдера |
| `SetFill(bool)` | Заполнение трека до текущего значения |
| `SetInverted(bool)` | Инвертирует направление слайдера |
| `SetPageSize(float)` | Шаг изменения значения при постраничной навигации |
| `SetShowInputField(bool)` | Показывает числовое поле ввода рядом со слайдером |
| `SetDirection(SliderDirection)` | Устанавливает ориентацию слайдера |

</details>

<details>
<summary>ProgressBar</summary>

```csharp
progressBar.SetTitle("Загрузка...").SetLowValue(0f).SetHighValue(100f);
```

| Метод | Описание |
|-------|----------|
| `SetTitle(string)` | Устанавливает заголовок, отображаемый в центре |
| `SetLowValue(float)` | Устанавливает минимальное значение |
| `SetHighValue(float)` | Устанавливает максимальное значение |

</details>

<details>
<summary>HelpBox</summary>

```csharp
helpBox
    .SetText("Что-то пошло не так")
    .SetMessageType(HelpBoxMessageType.Warning);
```

| Метод | Описание |
|-------|----------|
| `SetText(string)` | Текст сообщения help-box |
| `SetMessageType(HelpBoxMessageType)` | Иконка / уровень (`None` / `Info` / `Warning` / `Error`) |

</details>

<details>
<summary>EnumField / EnumFlagsField</summary>

```csharp
enumField.Initialize(Mode.Default, includeObsoleteValues: false);
```

| Метод | Описание |
|-------|----------|
| `Initialize(Enum, bool)` | Задаёт значение по умолчанию и набор пунктов; редакторский `EnumFlagsField` поддерживает тот же вызов |

</details>

<details>
<summary>Foldout</summary>

```csharp
foldout
    .SetText("Section Title")
    .SetToggleOnLabelClick(true)
    .SetValue(true);
```

| Метод | Описание |
|-------|----------|
| `SetText(string)` | Заголовок foldout |
| `SetToggleOnLabelClick(bool)` | Переключать ли раскрытие по клику на заголовок |

</details>

<details>
<summary>Image</summary>

```csharp
image
    .SetImage(myTexture)
    .SetTintColor(Color.white)
    .SetScaleMode(ScaleMode.ScaleToFit);
```

| Метод | Описание |
|-------|----------|
| `SetImage(Texture)` | Устанавливает `Image.image` |
| `SetImageFromResources(string)` | Загружает текстуру через `Resources.Load<Texture2D>` |
| `SetSprite(Sprite)` | Устанавливает `Image.sprite` |
| `SetSpriteFromResources(string)` | Загружает sprite через `Resources.Load<Sprite>` |
| `SetVectorImage(VectorImage)` | Устанавливает `Image.vectorImage` |
| `SetVectorImageFromResources(string)` | Загружает vector image через `Resources.Load<VectorImage>` |
| `SetUv(Rect)` | Устанавливает UV-rect |
| `SetSourceRect(Rect)` | Устанавливает source rect |
| `SetTintColor(Color)` | Цветовой tint изображения |
| `SetScaleMode(ScaleMode)` | Режим масштабирования |

</details>

<details>
<summary>IMGUIContainer</summary>

```csharp
container
    .SetOnGUIHandler(() => GUILayout.Label("IMGUI"))
    .SetCullingEnabled(true);
```

| Метод | Описание |
|-------|----------|
| `SetOnGUIHandler(Action)` | Заменяет коллбэк `onGUIHandler` |
| `AddOnGUIHandler(Action)` | Подписка на `onGUIHandler` |
| `RemoveOnGUIHandler(Action)` | Отписка от `onGUIHandler` |
| `SetCullingEnabled(bool)` | Пропускает `onGUIHandler`, когда элемент за пределами экрана |
| `SetContextType(ContextType)` | Устанавливает тип контекста IMGUI |
| `MarkDirtyLayout()` | Помечает IMGUI-layout как «грязный» для пересчёта |

</details>

## Списки и деревья

`ListView` создаёт строки через `makeItem` и переиспользует их через `bindItem`. Список из трёх строк и пустой `ListView`:

```csharp
using System.Collections.Generic;

var items = new List<string> { "Fireball", "Heal", "Shield" };
var listView = new ListView();
```

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>listView.itemsSource = items;&#10;listView.makeItem = () =&gt; new Label();&#10;listView.bindItem = (row, index) =&gt;&#10;    ((Label)row).text = items[index];&#10;listView.selectionType =&#10;    SelectionType.Single;&#10;listView.fixedItemHeight = 24;&#10;listView.style.height = 120;</code></pre> | <pre lang="csharp"><code>listView&#10;    .SetItemsSource(items)&#10;    .SetMakeItem(() =&gt; new Label())&#10;    .SetBindItem((row, index) =&gt;&#10;        ((Label)row).SetText(items[index]))&#10;    .SetSelectionType(SelectionType.Single)&#10;    .SetFixedItemHeight(24)&#10;    .SetHeight(120);</code></pre> |

Добавьте `listView` в дерево интерфейса. После изменения содержимого `items` вызовите `listView.RefreshItems()`. Если строка содержит обработчики, которые зависят от текущего элемента данных, снимайте их при отвязке через `SetUnbindItem`, чтобы не накапливать подписки при переиспользовании.

<details>
<summary>Методы списков и деревьев</summary>

Общие настройки доступны на `ListView`, `TreeView` и их `MultiColumn`-вариантах. `SetMakeItem`, `SetBindItem`, `SetUnbindItem` и `SetDestroyItem` относятся к обычным `ListView` и `TreeView`.

#### Данные и поведение BaseVerticalCollectionView

| Метод | Описание |
|-------|----------|
| `SetItemsSource(IList)` | Источник данных |
| `SetReorderable(bool)` | Включает drag-reorder |
| `SetSelectedIndex(int)` | Выбирает элемент по индексу |
| `SetSelectionType(SelectionType)` | None / Single / Multiple |
| `SetFixedItemHeight(float)` | Фиксированная высота элемента (для виртуализации `FixedHeight`) |
| `SetVirtualizationMethod(CollectionVirtualizationMethod)` | `FixedHeight` или `DynamicHeight` |
| `SetHorizontalScrollingEnabled(bool)` | Включает горизонтальную прокрутку |
| `SetShowAlternatingRowBackgrounds(AlternatingRowBackground)` | Режим зебра-полос |

#### События BaseVerticalCollectionView

| Метод | Описание |
|-------|----------|
| `AddItemsChosen(Action<IEnumerable<object>>)` / `RemoveItemsChosen` | Подтверждение элементов (двойной клик / Enter) |
| `AddSelectionChanged(Action<IEnumerable<object>>)` / `RemoveSelectionChanged` | Изменение выделения (объекты) |
| `AddSelectedIndicesChanged(Action<IEnumerable<int>>)` / `RemoveSelectedIndicesChanged` | Изменение выделения (индексы) |
| `AddItemIndexChanged(Action<int, int>)` / `RemoveItemIndexChanged` | Перемещение элемента (drag-reorder) |
| `AddItemsSourceChanged(Action)` / `RemoveItemsSourceChanged` | Смена ссылки `itemsSource` |
| `AddCanStartDrag(Func<CanStartDragArgs, bool>)` / `RemoveCanStartDrag` | Условие начала перетаскивания |
| `AddSetupDragAndDrop(Func<SetupDragAndDropArgs, StartDragArgs>)` / `RemoveSetupDragAndDrop` | Подготовка drag-and-drop |
| `AddDragAndDropUpdate(Func<HandleDragAndDropArgs, DragVisualMode>)` / `RemoveDragAndDropUpdate` | Визуальный режим drag-and-drop |
| `AddHandleDrop(Func<HandleDragAndDropArgs, DragVisualMode>)` / `RemoveHandleDrop` | Обработка drop |

#### Настройка BaseListView

| Метод | Описание |
|-------|----------|
| `SetAllowAdd(bool)` · `SetAllowRemove(bool)` | Включают встроенные кнопки add/remove |
| `SetHeaderTitle(string)` | Заголовок при включённом foldout-header |
| `SetShowFoldoutHeader(bool)` | Оборачивает список в `Foldout` |
| `SetShowAddRemoveFooter(bool)` | Показывает footer с add/remove |
| `SetShowBoundCollectionSize(bool)` | Поле размера коллекции |
| `SetReorderMode(ListViewReorderMode)` | `Simple` или `Animated` |
| `SetBindingSourceSelectionMode(BindingSourceSelectionMode)` | Auto-assign / manual |
| `SetOnAdd(Action<BaseListView>)` · `AddOnAdd` · `RemoveOnAdd` | Собственный обработчик кнопки добавления |
| `SetOnRemove(Action<BaseListView>)` · `AddOnRemove` · `RemoveOnRemove` | Собственный обработчик кнопки удаления |
| `SetOverridingAddButtonBehavior(Action<BaseListView, Button>)` · `AddOverridingAddButtonBehavior` · `RemoveOverridingAddButtonBehavior` | Заменяет поведение кнопки добавления |
| `SetMakeFooter(Func<VisualElement>)` · `AddMakeFooter` · `RemoveMakeFooter` | Фабрика подвала |
| `SetMakeHeader(Func<VisualElement>)` · `AddMakeHeader` · `RemoveMakeHeader` | Фабрика заголовка |
| `SetMakeNoneElement(Func<VisualElement>)` · `AddMakeNoneElement` · `RemoveMakeNoneElement` | Фабрика элемента пустого списка |
| `AddItemsAdded(Action<IEnumerable<int>>)` / `RemoveItemsAdded` | Добавление элементов по индексам |
| `AddItemsRemoved(Action<IEnumerable<int>>)` / `RemoveItemsRemoved` | Удаление элементов по индексам |

#### Настройка BaseTreeView

| Метод | Описание |
|-------|----------|
| `SetAutoExpand(bool)` | Авто-разворачивание новых узлов |
| `AddItemExpandedChanged(Action<TreeViewExpansionChangedArgs>)` / `RemoveItemExpandedChanged` | Подписка на изменение раскрытия |

#### Создание элементов ListView и TreeView

Эти методы дублируются в `ListViewExtensions` и `TreeViewExtensions` (каждое работает со своим типом view).

| Метод | Описание |
|-------|----------|
| `SetMakeItem(Func<VisualElement>)` · `AddMakeItem` · `RemoveMakeItem` | Фабрика элементов |
| `SetBindItem(Action<VisualElement, int>)` · `AddBindItem` · `RemoveBindItem` | Привязка элемента |
| `SetUnbindItem(Action<VisualElement, int>)` · `AddUnbindItem` · `RemoveUnbindItem` | Отвязка элемента |
| `SetDestroyItem(Action<VisualElement>)` · `AddDestroyItem` · `RemoveDestroyItem` | Уничтожение элемента |
| `SetItemTemplate(VisualTreeAsset)` | UXML-шаблон, по которому строятся элементы |

#### `MultiColumnListView` / `MultiColumnTreeView`

| Метод | Описание |
|-------|----------|
| `SetSortingMode(ColumnSortingMode)` | Встроенный режим сортировки заголовка колонки |

</details>

## Расширения редактора

Добавьте `using Aspid.FastTools.UIElements.Editors;` и `using UnityEditor.UIElements;` в editor-скрипт.

### Привязка к SerializedObject

Поле `_manaCost` — из компонента `AbilityBook` на странице [SerializedProperty Extensions](08-serialized-property-extensions.md#быстрый-старт):

```csharp
var field = new IntegerField("Mana cost");
```

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>field.bindingPath = "_manaCost";&#10;field.Bind(serializedObject);</code></pre> | <pre lang="csharp"><code>field.BindTo(&#10;    serializedObject, "_manaCost");</code></pre> |
| <pre lang="csharp"><code>var property = serializedObject&#10;    .FindProperty("_manaCost");&#10;field.BindProperty(property);</code></pre> | <pre lang="csharp"><code>var property = serializedObject&#10;    .FindProperty("_manaCost");&#10;field.BindPropertyTo(property);</code></pre> |
| <pre lang="csharp"><code>root.Bind(serializedObject);</code></pre> | <pre lang="csharp"><code>root.BindTo(serializedObject);</code></pre> |
| <pre lang="csharp"><code>// Отключить привязку&#10;root.Unbind();</code></pre> | <pre lang="csharp"><code>// Отключить привязку&#10;root.UnbindFrom();</code></pre> |

Для дерева `root` сначала задайте пути полям через `SetBindingPath`, затем вызовите `BindTo` у корня.

В `CreateInspectorGUI()` [Unity автоматически привязывает возвращённое дерево](https://docs.unity3d.com/6000.0/Documentation/Manual/UIE-Binding.html) к `serializedObject`. В таком инспекторе достаточно указать путь:

```csharp
public override VisualElement CreateInspectorGUI()
{
    return new IntegerField("Mana cost")
        .SetBindingPath("_manaCost");
}
```

`SetDataSource`, `SetDataSourceType` и `SetDataSourcePath` настраивают источник runtime data binding UI Toolkit. Они сами по себе не создают привязку к `SerializedObject`.

### PropertyField

`PropertyField.AddValueChanged` получает `SerializedPropertyChangeEvent`. У обычного `IntegerField.AddValueChanged` аргументом будет `ChangeEvent<int>`:

```csharp
var manaCost = serializedObject.FindProperty("_manaCost");
var field = new PropertyField(manaCost)
    .SetLabel("Mana cost")
    .AddValueChanged(evt =>
        Debug.Log(evt.changedProperty.intValue));
```

`RemoveValueChanged` снимает подписку с тем же делегатом. Для записи в свойство из собственного кода используйте [SerializedProperty Extensions](08-serialized-property-extensions.md).

### Открытие скрипта и окно-владелец

```csharp
image.AddOpenScriptCommand(target);
// Двойной клик открывает скрипт target в IDE

var window = image.GetOwnerWindow();
```

`target` — `MonoBehaviour` или `ScriptableObject`, чей скрипт нужно открыть. `GetOwnerWindow()` ищет окно по панели элемента. Если найти его не удалось, возвращает окно в фокусе, затем окно под курсором; результат может быть `null`. Это полезно при позиционировании попапа, когда клик уже пришёл, а фокус ещё не переключился.

## Собственные свойства USS

`TryGetByEnum` читает строковое свойство USS и разбирает его как enum без учёта регистра. Например, для такого правила в подключённом USS:

```css
.ability-panel {
    --ability-theme: dark;
}
```

Создайте элемент, который реагирует на разрешение собственных стилей:

```csharp
using UnityEngine;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;

public sealed class AbilityPanel : VisualElement
{
    private enum PanelTheme { Dark, Light }

    private static readonly CustomStyleProperty<string> ThemeProperty =
        new("--ability-theme");

    public AbilityPanel()
    {
        this.AddClass("ability-panel");
        RegisterCallback<CustomStyleResolvedEvent>(evt =>
        {
            if (evt.customStyle.TryGetByEnum(ThemeProperty, out PanelTheme theme))
                this.SetBackgroundColor(theme == PanelTheme.Dark
                    ? new Color(0.15f, 0.15f, 0.15f)
                    : new Color(0.9f, 0.9f, 0.9f));
        });
    }
}
```

Добавьте `AbilityPanel` в дерево с подключённым USS. Значения `dark` и `Dark` будут разобраны как `PanelTheme.Dark`. Метод возвращает `false`, если свойство отсутствует или строку не удалось разобрать.

## Практический пример

В [EditorTools](../../Samples~/EditorTools/Documentation/README.ru.md) собраны каталог способностей и реактивный инспектор, построенные на этих расширениях:

![Halve cooldown, +5 MP обновляет поля и описание эффекта; Undo возвращает прежние значения.](../../Samples~/EditorTools/Documentation/Images/demo.gif)

Halve cooldown, +5 MP обновляет поля и описание эффекта; Undo возвращает прежние значения.

## Продолжить

- [EditorTools](../../Samples~/EditorTools/Documentation/README.ru.md) — готовое окно с поиском, списком и редактированием ассетов.
- [SerializedProperty Extensions](08-serialized-property-extensions.md) — изменение данных через сериализацию Unity.
- [Editor Helpers](09-editor-helpers.md) — подписи объектов и компонентов.

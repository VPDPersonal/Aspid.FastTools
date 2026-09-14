# VisualElement Extensions

Собирайте деревья UI Toolkit и настраивайте элементы цепочками методов. Дочерние элементы, отступы, стили и обработчики событий остаются рядом в коде — удобно для окон редактора, инспекторов и интерфейсов игры.

## Быстрый старт

Создайте `StatsWindow.cs` в папке `Editor`. Этот пример добавляет окно с заголовком и кнопкой:

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

### Как читать цепочки

Сеттеры возвращают исходный элемент с сохранением его типа. `AddChild` возвращает **родителя**, поэтому следующий вызов продолжает настраивать родителя. Методы-запросы, например `IsFocused()` или `GetOwnerWindow()`, возвращают результат запроса.

```csharp
var panel = new VisualElement()
    .SetPadding(8)
    .AddChild(new Label("Health").SetFontSize(14))
    .SetMarginTop(12); // отступ у panel, не у Label
```

Для основных расширений нужен `Aspid.FastTools.UIElements`. Для редакторского биндинга и команд добавьте `Aspid.FastTools.UIElements.Editors`; этот код должен находиться в editor-сборке.

## Практический пример

В [EditorTools](../../Samples~/EditorTools/Documentation/README.ru.md) собраны каталог способностей и реактивный инспектор. Поле стоимости маны управляет подписью и видимостью предупреждения:

![Изменение стоимости маны обновляет статус и предупреждение в инспекторе](../Images/aspid_fasttools_visual_element.gif)

Изменение стоимости маны обновляет статус и предупреждение в инспекторе

<details>
<summary>Код инспектора с полем стоимости и предупреждением</summary>

Фрагмент для существующего `AbilityConfig` с сериализованным полем `_manaCost` типа `int`. Сохраните редактор в папке `Editor`: заголовок показывает стоимость, а `HelpBox` появляется при нулевом значении. Полный вариант с данными доступен в [EditorTools](../../Samples~/EditorTools/Documentation/README.ru.md).

```csharp
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Aspid.FastTools.Editors;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors;

[CustomEditor(typeof(AbilityConfig))]
internal sealed class AbilityConfigEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var manaCost = serializedObject.FindProperty("_manaCost");

        var badge = new Label()
            .SetFontSize(10).SetUnityFontStyleAndWeight(FontStyle.Bold)
            .SetPaddingX(10).SetPaddingY(3).SetBorderRadius(10).SetBorderWidth(1);

        var helpBox = new HelpBox("This ability costs no mana — is that intentional?", HelpBoxMessageType.Warning)
            .SetMarginTop(8).SetBorderRadius(6);

        Refresh();
        return new VisualElement()
            .SetBorderRadius(10).SetBorderWidth(1).SetPaddingX(14).SetPaddingY(12)
            .AddChild(new VisualElement()
                .SetFlexDirection(FlexDirection.Row).SetAlignItems(Align.Center)
                .AddChild(new Label(target.GetDisplayName()).SetFlexGrow(1).SetFontSize(15))
                .AddChild(badge))
            .AddChild(new PropertyField(manaCost).AddValueChanged(_ => Refresh()))
            .AddChild(helpBox);

        void Refresh()
        {
            var isFree = manaCost.intValue == 0;
            badge.SetText(isFree ? "FREE" : $"{manaCost.intValue} MP");
            helpBox.SetDisplay(isFree ? DisplayStyle.Flex : DisplayStyle.None);
        }
    }
}
```

</details>

## Найти нужное расширение

| Задача | Раздел |
|---|---|
| Построить дерево, задать имя или доступность | [Элементы и дочерние узлы](#элементы-и-дочерние-узлы) |
| Управлять фокусом и клавиатурной навигацией | [Фокус](#фокус) |
| Подключить USS и переключить классы | [USS и классы](#uss-и-классы) |
| Задать размеры, отступы, цвет и рамку | [Стили](#стили) |
| Настроить кнопку, поле, изображение или список | [Конкретные элементы](#конкретные-элементы) |
| Привязать SerializedObject или открыть скрипт | [Расширения редактора](#расширения-редактора) |

## Элементы и дочерние узлы

```csharp
element
    .SetName("MyElement")
    .SetVisible(true)
    .SetTooltip("Текст подсказки")
    .AddChild(new Label("Hello"))
    .AddChildren(child1, child2, child3);
```

| Метод | Описание |
|-------|----------|
| `SetName(string)` | Устанавливает `element.name` |
| `SetVisible(bool)` | Устанавливает `element.visible` |
| `SetTooltip(string)` | Устанавливает `element.tooltip` |
| `SetUserData(object)` | Устанавливает `element.userData` |
| `SetEnabledSelf(bool)` | Вызывает `element.SetEnabled`, управляя доступностью элемента |
| `SetPickingMode(PickingMode)` | Устанавливает `element.pickingMode` |
| `SetUsageHints(UsageHints)` | Устанавливает `element.usageHints` |
| `SetViewDataKey(string)` | Устанавливает `element.viewDataKey` |
| `SetLanguageDirection(LanguageDirection)` | Устанавливает `element.languageDirection` |
| `SetDisablePlayModeTint(bool)` | Устанавливает `element.disablePlayModeTint` |
| `SetDataSource(object)` | Устанавливает `element.dataSource` |
| `SetDataSourceType(Type)` | Устанавливает `element.dataSourceType` |
| `SetDataSourcePath(PropertyPath)` | Устанавливает `element.dataSourcePath` |
| `AddChild(VisualElement)` | Добавляет дочерний элемент, возвращает родителя |
| `AddChildren(params VisualElement[])` | Добавляет несколько дочерних элементов |
| `AddChildren(IEnumerable<VisualElement>)` | Добавляет из последовательности |
| `AddChildren(List<VisualElement>)` | Добавляет из списка |
| `AddChildren(Span<VisualElement>)` | Добавляет из span |
| `AddChildren(ReadOnlySpan<VisualElement>)` | Добавляет из read-only span |
| `InsertChild(int, VisualElement)` | Вставляет дочерний элемент по указанному индексу |
| `InsertChildren(int, params VisualElement[])` | Вставляет несколько дочерних элементов начиная с индекса |
| `InsertChildren(int, IEnumerable<VisualElement>)` | Вставляет из последовательности |
| `InsertChildren(int, List<VisualElement>)` | Вставляет из списка |
| `InsertChildren(int, Span<VisualElement>)` | Вставляет из span |
| `InsertChildren(int, ReadOnlySpan<VisualElement>)` | Вставляет из read-only span |
| `RemoveChild(VisualElement)` | Удаляет дочерний элемент, возвращает родителя |
| `RemoveChildAt(int)` | Удаляет дочерний элемент по указанному индексу |
| `ClearChildren()` | Удаляет все дочерние элементы |

> У каждой операции с дочерними элементами есть `*If`-вариант (`AddChildIf`, `AddChildrenIf`, `InsertChildIf`, `InsertChildrenIf`, `RemoveChildIf`, `RemoveChildAtIf`, `ClearChildrenIf`) с ведущим параметром `bool condition` — операция выполняется только при `condition == true`.

`RegisterCallbackOnce<TEventType>` и `RegisterCallbackOnce<TEventType, TUserArgsType>` доступны в поддерживаемой Unity 6.0.

## Фокус

| Метод | Описание |
|-------|----------|
| `FocusSelf()` | Устанавливает фокус на элемент |
| `BlurSelf()` | Снимает фокус с элемента |
| `IsFocused()` | Возвращает, находится ли элемент в фокусе |
| `SetTabIndex(int)` | Устанавливает `element.tabIndex` |
| `SetFocusable(bool)` | Устанавливает `element.focusable` |
| `SetDelegatesFocus(bool)` | Устанавливает `element.delegatesFocus` |

## USS и классы

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

## Стили

Методы стилей доступны и на элементе, и напрямую на `IStyle`. Выберите категорию, чтобы открыть список методов. Основные примеры рассчитаны на Unity 6.0; расширения для более новых версий отмечены отдельно.

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

Все методы отступов имеют перегрузку с единым значением, перегрузку по сторонам (`top`, `right`, `bottom`, `left`), сеттеры по одной стороне и сеттеры по парам осей X/Y.

| Метод | Свойства стиля |
|-------|----------------|
| `SetMargin(…)` / `SetPadding(…)` / `SetDistance(…)` | `Top/Right/Bottom/Left` (общее значение или per-side) |
| `SetMarginX/Y` · `SetPaddingX/Y` · `SetDistanceX/Y` | Устанавливает горизонтальную (X = `Left`+`Right`) или вертикальную (Y = `Top`+`Bottom`) пару |
| `SetMarginTop/Right/Bottom/Left` | Margin одной стороны |
| `SetPaddingTop/Right/Bottom/Left` | Padding одной стороны |
| `SetTop` / `SetRight` / `SetBottom` / `SetLeft` | Смещение одной стороны для абсолютного позиционирования (свойства `top` / `right` / `bottom` / `left`) |

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
| `SetUnityTextGenerator(TextGeneratorType)` | `unityTextGenerator` | Unity 6+ |
| `SetUnityEditorTextRenderingMode(EditorTextRenderingMode)` | `unityEditorTextRenderingMode` | Unity 6+ |
| `SetUnityTextAutoSize(StyleTextAutoSize)` | `unityTextAutoSize` (Unity 6.2+) |
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
| `SetUnitySliceType(StyleEnum<SliceType>)` | Unity 6+ |

</details>

<details>
<summary>Курсор</summary>

| Метод | Свойство стиля |
|-------|----------------|
| `SetCursor(StyleCursor)` | `cursor` |

</details>

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
| `SetHideMobileInput(bool)` | Скрывает мобильный soft input |
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
field.SetLabel("My Field");
field.SetValue(42);
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
<summary>INotifyValueChanged&lt;T&gt;</summary>

```csharp
field.SetValue(42, notify: false); // устанавливает значение без генерации ChangeEvent
field.AddValueChanged(evt => Debug.Log(evt.newValue));
field.RemoveValueChanged(myCallback);
```

Типизированные перегрузки доступны для `int`, `uint`, `nint`, `nuint`, `long`, `ulong`, `short`, `ushort`, `byte`, `sbyte`, `float`, `double`, `decimal`, `char`, `string`, `bool`, `Color`, `Vector2/3/4`, `Vector2Int/3Int`, `Rect/RectInt`, `Bounds/BoundsInt`, `Hash128`, `GUID` (Unity 6.4+), `Quaternion`, `Matrix4x4`, `Gradient`, `AnimationCurve`, `Delegate`, `Enum`, `Object`, `object`, плюс обобщённый fallback `SetValue<T, TValue>`.

> При установленном пакете `com.unity.mathematics` автоматически выставляется define `ASPID_FASTTOOLS_UNITY_MATHEMATICS_INTEGRATION` и добавляются перегрузки `SetValue` / `AddValueChanged` / `RemoveValueChanged` для `int2/3/4` (и `intMxN`), `float2/3/4` (и `floatMxN`), `half`/`half2/3/4`, `bool2/3/4` (и `boolMxN`), а также `quaternion`.

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

<details>
<summary>Списки и деревья</summary>

Общие методы распределены по нескольким специализированным расширениям:

- `BaseVerticalCollectionViewExtensions` — применяется ко **всем** collection-views (ListView, TreeView, MultiColumn-варианты).
- `BaseListViewExtensions` — применяется к ListView и MultiColumnListView.
- `BaseTreeViewExtensions` — применяется к TreeView и MultiColumnTreeView.
- `ListViewExtensions` / `TreeViewExtensions` — фабрики `MakeItem`/`BindItem`/`UnbindItem`/`DestroyItem` для своего вью.
- `MultiColumnListViewExtensions` / `MultiColumnTreeViewExtensions` — хелперы для multi-column-вариантов.

```csharp
listView
    .SetItemsSource(items)
    .SetMakeItem(() => new Label())
    .SetBindItem((el, i) => ((Label)el).SetText(items[i]))
    .SetSelectionType(SelectionType.Single)
    .AddSelectionChanged(selected => Debug.Log(selected));
```

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
| `AddCanStartDrag(Func<CanStartDragArgs, bool>)` / `RemoveCanStartDrag` | Кастомный gating старта drag |
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
| `SetOnAdd(Action<BaseListView>)` · `AddOnAdd` · `RemoveOnAdd` | Кастомный коллбэк add-кнопки |
| `SetOnRemove(Action<BaseListView>)` · `AddOnRemove` · `RemoveOnRemove` | Кастомный коллбэк remove-кнопки |
| `SetOverridingAddButtonBehavior(Action<BaseListView, Button>)` · `AddOverridingAddButtonBehavior` · `RemoveOverridingAddButtonBehavior` | Подменяет дефолтное поведение add |
| `SetMakeFooter(Func<VisualElement>)` · `AddMakeFooter` · `RemoveMakeFooter` | Фабрика подвала (Unity 6+) |
| `SetMakeHeader(Func<VisualElement>)` · `AddMakeHeader` · `RemoveMakeHeader` | Фабрика заголовка (Unity 6+) |
| `SetMakeNoneElement(Func<VisualElement>)` · `AddMakeNoneElement` · `RemoveMakeNoneElement` | Фабрика empty-state (Unity 6+) |
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

<a id="editor-commands-editor-only"></a>

## Расширения редактора

```csharp
using Aspid.FastTools.UIElements.Editors;

image.AddOpenScriptCommand(target);
// Двойной клик на элемент открывает скрипт 'target' в IDE
```

| Метод | Цель | Описание |
|-------|------|----------|
| `AddOpenScriptCommand(Object)` | `VisualElement` | Регистрирует обработчик двойного клика, открывающий исходный скрипт `MonoBehaviour` / `ScriptableObject` в IDE. |
| `GetOwnerWindow()` | `VisualElement` | Возвращает `EditorWindow`, чья панель содержит элемент (для отсоединённых элементов — откат на окно в фокусе / под курсором). Используйте вместо `EditorWindow.focusedWindow` при привязке попапов к элементу — pointer-события приходят до переключения фокуса на кликнутое окно. |
| `BindTo(SerializedObject)` | `VisualElement` | Вызывает `BindingExtensions.Bind` на элементе. |
| `BindTo(SerializedObject, string propertyPath)` | `IBindable` | Устанавливает `bindingPath` и привязывается к указанному `SerializedObject`. |
| `BindPropertyTo(SerializedProperty)` | `IBindable` | Вызывает `BindingExtensions.BindProperty` для переданного property. |
| `Initialize(Enum defaultValue, bool includeObsoleteValues = false)` | `EnumField` / `EnumFlagsField` | Инициализирует поле указанным значением enum по умолчанию. |
| `AddValueChanged(EventCallback<SerializedPropertyChangeEvent>)` / `RemoveValueChanged(...)` | `PropertyField` | Подписка / отписка от уведомлений об изменении свойства. |

## Собственные свойства USS

```csharp
using Aspid.FastTools.UIElements;

private enum PanelTheme { Dark, Light }

private static readonly CustomStyleProperty<string> ThemeProperty = new("--aspid-fasttools-prop-theme");

void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
{
    if (evt.customStyle.TryGetByEnum(ThemeProperty, out PanelTheme theme))
        ApplyTheme(theme);
}
```

| Метод | Описание |
|-------|----------|
| `ICustomStyle.TryGetByEnum<T>(CustomStyleProperty<string>, out T)` | Резолвит USS custom-property со строковым значением и парсит её регистронезависимо как enum `T`. В примере значения `dark` и `light` из USS превращаются в `PanelTheme`; `ApplyTheme` обозначает ваш обработчик применения темы. |

## Продолжить

- [EditorTools](../../Samples~/EditorTools/Documentation/README.ru.md) — готовое окно с поиском, списком и редактированием ассетов.
- [SerializedProperty Extensions](08-serialized-property-extensions.md) — изменение данных через сериализацию Unity.
- [Editor Helpers](09-editor-helpers.md) — подписи объектов и компонентов.

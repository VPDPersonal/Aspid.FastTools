# VisualElement Extensions

UI Toolkit extensions for building element trees, setting styles, subscribing to events, and binding editor fields. Methods return the configured element so calls can be chained.

<a id="example"></a>

## Quick start

Add `using Aspid.FastTools.UIElements;` to a script that imports `UnityEngine.UIElements`. These examples create the same panel with a heading:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>var title = new Label("Stats");&#10;title.style.fontSize = 18;&#10;&#10;var panel = new VisualElement();&#10;panel.style.paddingLeft = 12;&#10;panel.style.paddingRight = 12;&#10;panel.style.paddingTop = 8;&#10;panel.style.paddingBottom = 8;&#10;panel.Add(title);</code></pre> | <pre lang="csharp"><code>var panel = new VisualElement()&#10;    .SetPaddingX(12)&#10;    .SetPaddingY(8)&#10;    .AddChild(new Label("Stats")&#10;        .SetFontSize(18));</code></pre> |

Add `panel` to an editor window's `rootVisualElement` or a runtime UI's `UIDocument.rootVisualElement`.

<details>
<summary>Complete example: a Stats window with a button</summary>

Create `StatsWindow.cs` in an `Editor` folder:

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

Open **Tools → Stats**. The **Refresh** button prints a message to the Console.

</details>

### Reading a chain

Setters preserve the type: `new Button().SetText("Refresh")` returns a `Button`. Child operations return the **parent**, so the next call continues configuring it:

```csharp
var panel = new VisualElement()
    .AddChild(new Label("Health").SetFontSize(14))
    .SetMarginTop(12); // margin on panel
```

Chains on `element.style`, `textField.textEdition`, and `textField.textSelection` return their respective interfaces. Query methods such as `IsFocused()`, `GetOwnerWindow()`, and `TryGetByEnum(...)` do not continue the chain: they return a `bool` or the resolved window.

Core extensions work in both the editor and the game. `SerializedObject` binding and editor commands also require `Aspid.FastTools.UIElements.Editors`; put that code in an editor assembly, such as an `Editor` folder.

## Find an extension

| Task | Section |
|---|---|
| Build a tree, set a name, or enable an element | [Elements and children](#elements-and-children) |
| Control focus and keyboard navigation | [Focus](#focus) |
| Attach USS and switch classes | [USS and classes](#uss-and-classes) |
| Set dimensions, spacing, colours, and borders | [Styles](#styles) |
| Set a field value and subscribe to changes | [Values and events](#values-and-events) |
| Configure a button, field, or image | [Specific elements](#specific-elements) |
| Create a list that reuses rows | [Lists and trees](#lists-and-trees) |
| Bind a SerializedObject or open a script | [Editor extensions](#editor-extensions) |
| Read a custom USS property as an enum | [Custom USS properties](#custom-uss-properties) |

<a id="core-element-operations"></a>

## Elements and children

For the `panel` from the quick start. Children are created inline; `*If` adds the element only when the condition is true:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>panel.name = "ability-panel";&#10;panel.Add(new Label("Fireball"));&#10;panel.Add(new Label("Deals 40 damage"));&#10;if (Application.isPlaying)&#10;    panel.Add(new Label("Play mode"));</code></pre> | <pre lang="csharp"><code>panel&#10;    .SetName("ability-panel")&#10;    .AddChildren(&#10;        new Label("Fireball"),&#10;        new Label("Deals 40 damage"))&#10;    .AddChildIf(Application.isPlaying,&#10;        new Label("Play mode"));</code></pre> |
| <pre lang="csharp"><code>var header = new Label("Header");&#10;panel.Insert(0, header);&#10;panel.Remove(header);&#10;panel.RemoveAt(0);&#10;panel.Clear();</code></pre> | <pre lang="csharp"><code>var header = new Label("Header");&#10;panel&#10;    .InsertChild(0, header)&#10;    .RemoveChild(header)&#10;    .RemoveChildAt(0)&#10;    .ClearChildren();</code></pre> |

These methods return the parent element, so they can be chained. `AddChildren` and `InsertChildren` preserve the order of the supplied elements.

> [!NOTE]
> `*If` checks the condition only at call time. Arguments are evaluated first: `AddChildIf(false, new Label("Warning"))` creates the `Label` but does not add it to the tree. Use a normal `if` when construction is expensive.

<details>
<summary>All element and child operations</summary>

| Method | Description |
|-------|----------|
| `SetName(string)` | Sets `element.name` |
| `SetVisible(bool)` | Sets `element.visible` |
| `SetTooltip(string)` | Sets `element.tooltip` |
| `SetUserData(object)` | Sets `element.userData` |
| `SetEnabledSelf(bool)` | Calls `element.SetEnabled` to control interaction |
| `SetPickingMode(PickingMode)` | Sets `element.pickingMode` |
| `SetUsageHints(UsageHints)` | Sets `element.usageHints`; configure before attaching the element to a panel |
| `SetViewDataKey(string)` | Sets `element.viewDataKey` |
| `SetLanguageDirection(LanguageDirection)` | Sets `element.languageDirection` |
| `SetDisablePlayModeTint(bool)` | Sets `element.disablePlayModeTint` |
| `SetDataSource(object)` | Sets `element.dataSource` |
| `SetDataSourceType(Type)` | Sets `element.dataSourceType` |
| `SetDataSourcePath(PropertyPath)` | Sets `element.dataSourcePath` |
| `AddChild(VisualElement)` | Appends a child, returns the parent |
| `AddChildren(params VisualElement[])` | Appends multiple children |
| `InsertChild(int, VisualElement)` | Inserts a child at the specified index |
| `InsertChildren(int, params VisualElement[])` | Inserts multiple children starting at an index |
| `RemoveChild(VisualElement)` | Removes a child, returns the parent |
| `RemoveChildAt(int)` | Removes the child at the specified index |
| `ClearChildren()` | Removes all children |

`AddChildren` and `InsertChildren` accept `params VisualElement[]`, `IEnumerable<VisualElement>`, `List<VisualElement>`, `Span<VisualElement>`, and `ReadOnlySpan<VisualElement>`.

> Every child operation has an `*If` variant (`AddChildIf`, `AddChildrenIf`, `InsertChildIf`, `InsertChildrenIf`, `RemoveChildIf`, `RemoveChildAtIf`, `ClearChildrenIf`) with a leading `bool condition`. It runs only when `condition == true`.

</details>

### Visibility and interaction

Choose how hiding or disabling should behave:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>// Hide while retaining layout space&#10;element.visible = false;</code></pre> | <pre lang="csharp"><code>// Hide while retaining layout space&#10;element.SetVisible(false);</code></pre> |
| <pre lang="csharp"><code>// Remove the element and descendants from layout&#10;element.style.display =&#10;    DisplayStyle.None;</code></pre> | <pre lang="csharp"><code>// Remove the element and descendants from layout&#10;element.SetDisplay(DisplayStyle.None);</code></pre> |
| <pre lang="csharp"><code>// Disable interaction with the element and descendants&#10;element.SetEnabled(false);</code></pre> | <pre lang="csharp"><code>// Disable interaction with the element and descendants&#10;element.SetEnabledSelf(false);</code></pre> |

Restore the element with `SetVisible(true)`, `SetDisplay(DisplayStyle.Flex)`, or `SetEnabledSelf(true)`, respectively. A child's enabled state also depends on its parents.

<a id="focusable"></a>

## Focus

For a search field already attached to `panel`:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>var search = panel.Q&lt;TextField&gt;("search");&#10;search.focusable = true;&#10;search.tabIndex = 0;&#10;search.Focus();</code></pre> | <pre lang="csharp"><code>panel.Q&lt;TextField&gt;("search")&#10;    .SetFocusable(true)&#10;    .SetTabIndex(0)&#10;    .FocusSelf();</code></pre> |

`FocusSelf()` calls the normal `Focus()`: the element must be focusable. `IsFocused()` compares the element with `focusController.focusedElement` and returns `false` when detached.

| Method | Description |
|-------|----------|
| `FocusSelf()` | Attempts to give focus to the element |
| `BlurSelf()` | Tells the element to release focus |
| `IsFocused()` | Returns whether the element currently has keyboard focus |
| `SetTabIndex(int)` | Sets `element.tabIndex` |
| `SetFocusable(bool)` | Sets `element.focusable` |
| `SetDelegatesFocus(bool)` | Sets `element.delegatesFocus` |

<a id="uss--class-operations"></a>

## USS and classes

The `Assets/Resources/UI/AbilityCard.uss` style sheet is attached to `panel`, and the `playing` class follows the current state:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>panel.styleSheets.Add(&#10;    Resources.Load&lt;StyleSheet&gt;("UI/AbilityCard"));&#10;panel.AddToClassList("ability-card");&#10;panel.EnableInClassList(&#10;    "playing", Application.isPlaying);</code></pre> | <pre lang="csharp"><code>panel&#10;    .AddStyleSheetFromResources("UI/AbilityCard")&#10;    .AddClass("ability-card")&#10;    .EnableClass("playing", Application.isPlaying);</code></pre> |

`EnableClass` sets class membership to the requested state. `ToggleClass` reverses class membership on each call.

<details>
<summary>Class and style-sheet methods</summary>

| Method | Description |
|-------|----------|
| `AddClass(string)` | Adds a USS class |
| `RemoveClass(string)` | Removes a USS class |
| `ClearClasses()` | Removes all USS classes |
| `ToggleClass(string)` | Toggles a USS class on/off |
| `EnableClass(string, bool)` | Adds or removes a USS class based on a condition |
| `AddStyleSheet(StyleSheet)` | Adds a `StyleSheet` |
| `RemoveStyleSheet(StyleSheet)` | Removes a `StyleSheet` |
| `AddStyleSheetFromResources(string)` | Adds a stylesheet loaded via `Resources.Load` |
| `RemoveStyleSheetFromResources(string)` | Removes a stylesheet loaded via `Resources.Load` |

</details>

For `AddStyleSheetFromResources("UI/AbilityCard")`, place the file in a `Resources` folder, such as `Assets/Resources/UI/AbilityCard.uss`. Omit the extension from the path. If the resource is missing, the method logs a warning and returns the unchanged element.

<a id="style-extensions--by-category"></a>

## Styles

Setters write the element's inline styles. Keep shared appearance rules in USS and use chains for the dimensions and state of individual elements.

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>panel.style.flexDirection =&#10;    FlexDirection.Row;&#10;panel.style.alignItems = Align.Center;&#10;panel.style.width = 240;&#10;panel.style.height = 48;&#10;panel.style.marginTop = 8;</code></pre> | <pre lang="csharp"><code>panel&#10;    .SetFlexDirection(FlexDirection.Row)&#10;    .SetAlignItems(Align.Center)&#10;    .SetSize(240, 48)&#10;    .SetMarginTop(8);</code></pre> |

### Sides, axes, and units

A shared value sets all sides; `X` means left and right, and `Y` means top and bottom. In overloads with optional parameters, omitted sides keep their previous values:

```csharp
panel
    .SetPadding(8)               // all sides
    .SetPaddingX(12)             // left and right
    .SetMargin(top: 4, bottom: 8)
    .SetSize(width: Length.Percent(100));
```

Numeric `StyleLength` values use pixels; use `Length.Percent(...)` for percentages. `SetDistance` writes `top`, `right`, `bottom`, and `left`. For example, `SetPosition(Position.Absolute).SetDistance(0)` stretches an element to its parent's edges.

### Configuring IStyle

The same methods are available on `element.style`. That chain returns `IStyle`, so resume element methods in a separate call:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>panel.style.paddingLeft = 12;&#10;panel.style.paddingRight = 12;&#10;panel.style.height = 48;</code></pre> | <pre lang="csharp"><code>panel.style&#10;    .SetPaddingX(12)&#10;    .SetHeight(48);</code></pre> |

### Style reference

The main examples target Unity 6.0. Disclosure sections mark methods that require newer Unity versions.

<details>
<summary>Layout</summary>

| Method | Style property |
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
<summary>Size</summary>

| Method | Description |
|-------|----------|
| `SetSize(StyleLength)` | Sets both width and height |
| `SetSize(width?, height?)` | Sets width and/or height independently |
| `SetMinSize(StyleLength)` | Sets both minWidth and minHeight |
| `SetMinSize(minWidth?, minHeight?)` | Minimum width and/or height independently |
| `SetMaxSize(StyleLength)` | Sets both maxWidth and maxHeight |
| `SetMaxSize(maxWidth?, maxHeight?)` | Maximum width and/or height independently |
| `SetWidth(StyleLength)` | `width` |
| `SetMinWidth(StyleLength)` | `minWidth` |
| `SetMaxWidth(StyleLength)` | `maxWidth` |
| `SetHeight(StyleLength)` | `height` |
| `SetMinHeight(StyleLength)` | `minHeight` |
| `SetMaxHeight(StyleLength)` | `maxHeight` |

</details>

<details>
<summary>Spacing and positioning</summary>

`SetMargin`, `SetPadding`, and `SetDistance` support one shared value, individual sides (`top`, `right`, `bottom`, `left`), and X/Y axis pairs.

| Method | Style properties |
|-------|----------------|
| `SetMargin(…)` / `SetPadding(…)` / `SetDistance(…)` | `Top/Right/Bottom/Left` (uniform or per-side) |
| `SetMarginX/Y` · `SetPaddingX/Y` · `SetDistanceX/Y` | Sets the horizontal (X = `Left`+`Right`) or vertical (Y = `Top`+`Bottom`) pair |
| `SetMarginTop/Right/Bottom/Left` | Single-side margin |
| `SetPaddingTop/Right/Bottom/Left` | Single-side padding |
| `SetTop` / `SetRight` / `SetBottom` / `SetLeft` | Offset for one side (`top` / `right` / `bottom` / `left`) |

> `SetDistance` wraps the four `top`/`right`/`bottom`/`left` properties used for absolute positioning. `SetTop`, `SetRight`, `SetBottom`, and `SetLeft` directly alias one property each.

</details>

<details>
<summary>Font</summary>

| Method | Style property |
|-------|----------------|
| `SetUnityFont(StyleFont)` | `unityFont` |
| `SetFontSize(StyleLength)` | `fontSize` |
| `SetUnityFontDefinition(StyleFontDefinition)` | `unityFontDefinition` |
| `SetUnityFontStyleAndWeight(StyleEnum<FontStyle>)` | `unityFontStyleAndWeight` |

</details>

<details>
<summary>Font style presets</summary>

Convenience methods toggle bold or italic without overwriting the other flag:

| Method | Description |
|-------|----------|
| `SetNormalUnityFontStyleAndWeight()` | Resets to `FontStyle.Normal` |
| `AddBoldUnityFontStyleAndWeight()` | Adds bold, preserving italic |
| `RemoveBoldUnityFontStyleAndWeight()` | Removes bold, preserving italic |
| `AddItalicUnityFontStyleAndWeight()` | Adds italic, preserving bold |
| `RemoveItalicUnityFontStyleAndWeight()` | Removes italic, preserving bold |

</details>

<details>
<summary>Text</summary>

| Method | Style property | Notes |
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
<summary>Colour and opacity</summary>

| Method | Style property |
|-------|----------------|
| `SetColor(StyleColor)` | `color` |
| `SetColor(string)` | `color` parsed from an HTML string (`"#RRGGBB"` or a named color) |
| `SetOpacity(StyleFloat)` | `opacity` |

</details>

<details>
<summary>Border</summary>

| Method | Description |
|-------|----------|
| `SetBorderColor(StyleColor)` | All sides |
| `SetBorderColor(top?, right?, bottom?, left?)` | Per side |
| `SetBorderColorX(StyleColor)` · `SetBorderColorY(StyleColor)` | Horizontal (left + right) or vertical (top + bottom) pair |
| `SetBorderColorTop/Right/Bottom/Left(StyleColor)` | Single side |
| `SetBorderRadius(StyleLength)` | All corners |
| `SetBorderRadius(topLeft?, topRight?, bottomLeft?, bottomRight?)` | Per corner |
| `SetBorderRadiusTop(StyleLength)` · `SetBorderRadiusBottom(StyleLength)` | Top or bottom corner pair |
| `SetBorderRadiusTopLeft/TopRight/BottomLeft/BottomRight(StyleLength)` | Single corner |
| `SetBorderWidth(StyleFloat)` | All sides |
| `SetBorderWidth(top?, right?, bottom?, left?)` | Per side |
| `SetBorderWidthX(StyleFloat)` · `SetBorderWidthY(StyleFloat)` | Horizontal or vertical pair |
| `SetBorderWidthTop/Right/Bottom/Left(StyleFloat)` | Single side |

</details>

<details>
<summary>Background</summary>

| Method | Style property |
|-------|----------------|
| `SetBackgroundColor(StyleColor)` | `backgroundColor` |
| `SetBackgroundColor(string)` | `backgroundColor` parsed from an HTML string (`"#RRGGBB"` or a named color) |
| `SetBackgroundImage(StyleBackground)` | `backgroundImage` |
| `SetBackgroundImageFromResources(string)` | Loads a `Texture2D` via `Resources.Load` and assigns it to `backgroundImage` |
| `SetBackgroundSize(StyleBackgroundSize)` | `backgroundSize` |
| `SetBackgroundRepeat(StyleBackgroundRepeat)` | `backgroundRepeat` |
| `SetBackgroundPosition(StyleBackgroundPosition)` | Both X and Y |
| `SetBackgroundPosition(x?, y?)` | Independently |
| `SetBackgroundPositionX(StyleBackgroundPosition)` | `backgroundPositionX` |
| `SetBackgroundPositionY(StyleBackgroundPosition)` | `backgroundPositionY` |
| `SetUnityBackgroundImageTintColor(StyleColor)` | `unityBackgroundImageTintColor` |

</details>

<details>
<summary>Transform</summary>

| Method | Style property |
|-------|----------------|
| `SetScale(StyleScale)` | `scale` |
| `SetRotate(StyleRotate)` | `rotate` |
| `SetTranslate(StyleTranslate)` | `translate` |
| `SetTransformOrigin(StyleTransformOrigin)` | `transformOrigin` |

</details>

<details>
<summary>Aspect, filter, and material</summary>

Available starting with Unity 6000.3.

| Method | Style property |
|-------|----------------|
| `SetAspectRatio(StyleRatio)` | `aspectRatio` |
| `SetFilter(StyleList<FilterFunction>)` | `filter` |
| `SetUnityMaterial(StyleMaterialDefinition)` | `unityMaterial` |

</details>

<details>
<summary>Transition</summary>

| Method | Style property |
|-------|----------------|
| `SetTransitionDelay(StyleList<TimeValue>)` | `transitionDelay` |
| `SetTransitionDuration(StyleList<TimeValue>)` | `transitionDuration` |
| `SetTransitionProperty(StyleList<StylePropertyName>)` | `transitionProperty` |
| `SetTransitionTimingFunction(StyleList<EasingFunction>)` | `transitionTimingFunction` |

</details>

<details>
<summary>Overflow and visibility</summary>

| Method | Style property |
|-------|----------------|
| `SetOverflow(StyleEnum<Overflow>)` | `overflow` |
| `SetUnityOverflowClipBox(StyleEnum<OverflowClipBox>)` | `unityOverflowClipBox` |
| `SetVisibility(StyleEnum<Visibility>)` | `visibility` |
| `SetDisplay(DisplayStyle)` | `display` |

</details>

<details>
<summary>Image slicing</summary>

| Method | Description |
|-------|----------|
| `SetUnitySlice(StyleInt)` | All sides |
| `SetUnitySlice(top?, right?, bottom?, left?)` | Per side |
| `SetUnitySliceX(StyleInt)` · `SetUnitySliceY(StyleInt)` | Horizontal (left + right) or vertical (top + bottom) pair |
| `SetUnitySliceTop/Right/Bottom/Left(StyleInt)` | Single side |
| `SetUnitySliceScale(StyleFloat)` | `unitySliceScale` |
| `SetUnitySliceType(StyleEnum<SliceType>)` | `unitySliceType` |

</details>

<details>
<summary>Cursor</summary>

| Method | Style property |
|-------|----------------|
| `SetCursor(StyleCursor)` | `cursor` |

</details>

<a id="inotifyvaluechangedt"></a>

## Values and events

### Field values

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>var field = new IntegerField("Mana cost");&#10;field.value = 42;&#10;field.SetValueWithoutNotify(10);</code></pre> | <pre lang="csharp"><code>var field = new IntegerField("Mana cost");&#10;field.SetValue(42);&#10;field.SetValue(10, notify: false);</code></pre> |

By default, `SetValue` assigns `value` and preserves Unity's event behaviour. `notify: false` calls `SetValueWithoutNotify`: it updates the field without sending a `ChangeEvent`. This is useful for synchronizing UI with data.

### Subscribing and unsubscribing

Keep the handler if you need to remove it later:

```csharp
var status = new Label();
EventCallback<ChangeEvent<int>> onChanged =
    evt => status.SetText($"Mana: {evt.newValue}");
```

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>field.RegisterValueChangedCallback(&#10;    onChanged);&#10;&#10;// When the handler is no longer needed&#10;field.UnregisterValueChangedCallback(&#10;    onChanged);</code></pre> | <pre lang="csharp"><code>field.AddValueChanged(onChanged);&#10;&#10;&#10;// When the handler is no longer needed&#10;field.RemoveValueChanged(onChanged);</code></pre> |

Pass the same delegate when unsubscribing. A new lambda with similar code does not remove the previous subscription.

<details>
<summary>Value types and Unity.Mathematics integration</summary>

Typed overloads are available for `int`, `uint`, `nint`, `nuint`, `long`, `ulong`, `short`, `ushort`, `byte`, `sbyte`, `float`, `double`, `decimal`, `char`, `string`, `bool`, `Color`, `Vector2/3/4`, `Vector2Int/3Int`, `Rect/RectInt`, `Bounds/BoundsInt`, `Hash128`, `GUID` (Unity 6.4+), `Quaternion`, `Matrix4x4`, `Gradient`, `AnimationCurve`, `Delegate`, `Enum`, `Object`, and `object`. A generic `SetValue<T, TValue>` covers other types.

> Installing `com.unity.mathematics` automatically sets `ASPID_FASTTOOLS_UNITY_MATHEMATICS_INTEGRATION` and adds `SetValue` / `AddValueChanged` / `RemoveValueChanged` overloads for `int2/3/4` (and `intMxN`), `float2/3/4` (and `floatMxN`), `half`/`half2/3/4`, `bool2/3/4` (and `boolMxN`), and `quaternion`.

</details>

### Buttons and manipulators

```csharp
var button = new Button();
void Refresh() => Debug.Log("Refresh");
```

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>button.text = "Refresh";&#10;button.clicked += Refresh;&#10;&#10;// Unsubscribe&#10;button.clicked -= Refresh;</code></pre> | <pre lang="csharp"><code>button&#10;    .SetText("Refresh")&#10;    .AddClicked(Refresh);&#10;// Unsubscribe&#10;button.RemoveClicked(Refresh);</code></pre> |

An ordinary `VisualElement` can also be clickable. An `out` overload lets you keep the manipulator for removal:

```csharp
panel.AddClickable(Refresh, out var clickable);

// When the click is no longer needed
panel.RemoveManipulatorSelf(clickable);
```

| Method | Task |
|---|---|
| `AddManipulatorSelf` / `RemoveManipulatorSelf` | Add or remove an existing `IManipulator` |
| `AddClickable` | Handle clicks; overloads support an event and repetition via `delay` / `interval` |
| `AddKeyboardNavigationManipulator` | Handle keyboard navigation |
| `AddContextualMenuManipulator` | Populate a context menu |

Manipulator creation methods have `out` overloads. For other events, use standard UI Toolkit `RegisterCallback` and `UnregisterCallback`.

<a id="specialized-element-extensions"></a>

## Specific elements

Expand a type to see its configuration example and available methods.

<details>
<summary>TextElement</summary>

```csharp
label
    .SetText("Hello World")
    .SetEnableRichText(true)
    .SetParseEscapeSequences(true);
```

| Method | Description |
|-------|----------|
| `SetText(string)` | Sets the displayed text |
| `SetEnableRichText(bool)` | Enables rich-text tag parsing |
| `SetEmojiFallbackSupport(bool)` | Enables emoji fallback rendering |
| `SetParseEscapeSequences(bool)` | Whether escape sequences (e.g. `\n`) are parsed |
| `SetDisplayTooltipWhenElided(bool)` | Shows the elided text in a tooltip on hover |

</details>

<details>
<summary>ITextEdition (TextField, IntegerField, …)</summary>

Use a text field's `textEdition`. The chain returns that interface rather than the field itself.

```csharp
textField.textEdition
    .SetPlaceholder("Search…")
    .SetMaxLength(64)
    .SetDelayed(true);
```

| Method | Description |
|-------|----------|
| `SetMaxLength(int)` | Maximum number of characters |
| `SetMaskChar(char)` | Character used to mask password input |
| `SetDelayed(bool)` | Defers value change until focus loss / Enter |
| `SetReadOnly(bool)` | Disables editing |
| `SetPassword(bool)` | Toggles password mode (uses mask char) |
| `SetPlaceholder(string)` | Placeholder text shown when empty |
| `SetAutoCorrection(bool)` | Enables auto-correction (mobile) |
| `SetHideMobileInput(bool)` | Hides the native input field on a mobile device |
| `SetHideSoftKeyboard(bool)` | Hides the on-screen keyboard (Unity 6.4+) |
| `SetHidePlaceholderOnFocus(bool)` | Removes the placeholder on focus |
| `SetKeyboardType(TouchScreenKeyboardType)` | Sets the touch-screen keyboard type |

</details>

<details>
<summary>ITextSelection</summary>

Configure a text field's selection through `textSelection`.

```csharp
textField.textSelection
    .SetSelectable(true)
    .SetSelectAllOnFocus(true);
```

| Method | Description |
|-------|----------|
| `AddOnCursorIndexChange(Action)` / `RemoveOnCursorIndexChange(Action)` | Subscribe to cursor position changes (Unity 6.3+) |
| `AddOnSelectIndexChange(Action)` / `RemoveOnSelectIndexChange(Action)` | Subscribe to selection anchor changes (Unity 6.3+) |
| `SetCursorIndex(int)` | Current cursor position |
| `SetSelectIndex(int)` | Sets the current selection anchor |
| `SetSelectable(bool)` | Whether text can be selected |
| `SetSelectAllOnFocus(bool)` | Selects all text on focus |
| `SetSelectAllOnMouseUp(bool)` | Selects all text on mouse release |
| `SetDoubleClickSelectsWord(bool)` | Double-click selects the word under cursor |
| `SetTripleClickSelectsLine(bool)` | Triple-click selects the line under cursor |

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
    .SetLabel("Enabled")
    .SetText("Show advanced settings")
    .SetToggleOnLabelClick(true);
```

| Method | Description |
|-------|----------|
| `SetText(string)` | Sets the text beside the checkbox |
| `SetLabel(string)` | Sets the field-level label |
| `SetToggleOnLabelClick(bool)` | Whether clicking the label toggles the value |

</details>

<details>
<summary>IMixedValueSupport</summary>

```csharp
field.SetShowMixedValue(true); // show the mixed-value indicator
```

</details>

<details>
<summary>Button</summary>

```csharp
button
    .AddClicked(() => Debug.Log("Clicked"))
    .SetIconImage(myBackground);
```

| Method | Description |
|-------|----------|
| `AddClicked(Action)` | Subscribe to `Button.clicked` |
| `RemoveClicked(Action)` | Unsubscribe from `Button.clicked` |
| `SetClickable(Clickable)` | Replace the click manipulator. Configure it before `AddClicked` subscriptions |
| `SetIconImage(Background)` | Sets `Button.iconImage` |

</details>

<details>
<summary>Slider / BaseSlider&lt;TValue&gt;</summary>

```csharp
slider
    .SetLowValue(0f)
    .SetHighValue(100f)
    .SetShowInputField(true);
```

| Method | Description |
|-------|----------|
| `SetLowValue(TValue)` | Sets the minimum slider value |
| `SetHighValue(TValue)` | Sets the maximum slider value |
| `SetFill(bool)` | Whether the track is filled up to the current value |
| `SetInverted(bool)` | Reverses the slider direction |
| `SetPageSize(float)` | Controls how much the value changes per page step |
| `SetShowInputField(bool)` | Shows a numeric input field alongside the slider |
| `SetDirection(SliderDirection)` | Sets the slider orientation |

</details>

<details>
<summary>ProgressBar</summary>

```csharp
progressBar.SetTitle("Loading...").SetLowValue(0f).SetHighValue(100f);
```

| Method | Description |
|-------|----------|
| `SetTitle(string)` | Sets the title displayed in the center |
| `SetLowValue(float)` | Sets the minimum value |
| `SetHighValue(float)` | Sets the maximum value |

</details>

<details>
<summary>HelpBox</summary>

```csharp
helpBox
    .SetText("Something went wrong")
    .SetMessageType(HelpBoxMessageType.Warning);
```

| Method | Description |
|-------|----------|
| `SetText(string)` | Sets the help-box message |
| `SetMessageType(HelpBoxMessageType)` | Sets the icon / severity (`None` / `Info` / `Warning` / `Error`) |

</details>

<details>
<summary>EnumField / EnumFlagsField</summary>

```csharp
enumField.Initialize(Mode.Default, includeObsoleteValues: false);
```

| Method | Description |
|-------|----------|
| `Initialize(Enum, bool)` | Sets the default value and the choice set; the editor's `EnumFlagsField` supports the same call |

</details>

<details>
<summary>Foldout</summary>

```csharp
foldout
    .SetText("Section Title")
    .SetToggleOnLabelClick(true)
    .SetValue(true);
```

| Method | Description |
|-------|----------|
| `SetText(string)` | Sets the foldout title |
| `SetToggleOnLabelClick(bool)` | Whether clicking the title toggles expansion |

</details>

<details>
<summary>Image</summary>

```csharp
image
    .SetImage(myTexture)
    .SetTintColor(Color.white)
    .SetScaleMode(ScaleMode.ScaleToFit);
```

| Method | Description |
|-------|----------|
| `SetImage(Texture)` | Sets `Image.image` |
| `SetImageFromResources(string)` | Loads a texture via `Resources.Load<Texture2D>` |
| `SetSprite(Sprite)` | Sets `Image.sprite` |
| `SetSpriteFromResources(string)` | Loads a sprite via `Resources.Load<Sprite>` |
| `SetVectorImage(VectorImage)` | Sets `Image.vectorImage` |
| `SetVectorImageFromResources(string)` | Loads a vector image via `Resources.Load<VectorImage>` |
| `SetUv(Rect)` | Sets the UV rect |
| `SetSourceRect(Rect)` | Sets the source rect |
| `SetTintColor(Color)` | Sets the image tint |
| `SetScaleMode(ScaleMode)` | Sets the scale mode |

</details>

<details>
<summary>IMGUIContainer</summary>

```csharp
container
    .SetOnGUIHandler(() => GUILayout.Label("IMGUI"))
    .SetCullingEnabled(true);
```

| Method | Description |
|-------|----------|
| `SetOnGUIHandler(Action)` | Replace the `onGUIHandler` callback |
| `AddOnGUIHandler(Action)` | Subscribe to `onGUIHandler` |
| `RemoveOnGUIHandler(Action)` | Unsubscribe from `onGUIHandler` |
| `SetCullingEnabled(bool)` | Skip `onGUIHandler` when the element is off-screen |
| `SetContextType(ContextType)` | Sets the IMGUI context type |
| `MarkDirtyLayout()` | Marks the IMGUI layout dirty so it is recomputed |

</details>

<a id="collection-views-listview-treeview-multicolumn-variants"></a>

## Lists and trees

`ListView` creates rows through `makeItem` and reuses them through `bindItem`. A list of three strings and an empty `ListView`:

```csharp
using System.Collections.Generic;

var items = new List<string> { "Fireball", "Heal", "Shield" };
var listView = new ListView();
```

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>listView.itemsSource = items;&#10;listView.makeItem = () =&gt; new Label();&#10;listView.bindItem = (row, index) =&gt;&#10;    ((Label)row).text = items[index];&#10;listView.selectionType =&#10;    SelectionType.Single;&#10;listView.fixedItemHeight = 24;&#10;listView.style.height = 120;</code></pre> | <pre lang="csharp"><code>listView&#10;    .SetItemsSource(items)&#10;    .SetMakeItem(() =&gt; new Label())&#10;    .SetBindItem((row, index) =&gt;&#10;        ((Label)row).SetText(items[index]))&#10;    .SetSelectionType(SelectionType.Single)&#10;    .SetFixedItemHeight(24)&#10;    .SetHeight(120);</code></pre> |

Add `listView` to the UI tree. After changing `items`, call `listView.RefreshItems()`. If a row has handlers tied to its current data item, remove them on unbinding through `SetUnbindItem` so reuse does not accumulate subscriptions.

<details>
<summary>List and tree methods</summary>

Shared settings apply to `ListView`, `TreeView`, and their `MultiColumn` variants. `SetMakeItem`, `SetBindItem`, `SetUnbindItem`, and `SetDestroyItem` apply to ordinary `ListView` and `TreeView`.

#### BaseVerticalCollectionView data and behaviour

| Method | Description |
|-------|----------|
| `SetItemsSource(IList)` | Underlying data source |
| `SetReorderable(bool)` | Enables drag-to-reorder |
| `SetSelectedIndex(int)` | Selects a specific index |
| `SetSelectionType(SelectionType)` | None / Single / Multiple |
| `SetFixedItemHeight(float)` | Fixed item height (for `FixedHeight` virtualization) |
| `SetVirtualizationMethod(CollectionVirtualizationMethod)` | `FixedHeight` or `DynamicHeight` |
| `SetHorizontalScrollingEnabled(bool)` | Enables horizontal scrolling |
| `SetShowAlternatingRowBackgrounds(AlternatingRowBackground)` | Zebra striping mode |

#### BaseVerticalCollectionView events

| Method | Description |
|-------|----------|
| `AddItemsChosen(Action<IEnumerable<object>>)` / `RemoveItemsChosen` | Items confirmed (e.g. double-click / Enter) |
| `AddSelectionChanged(Action<IEnumerable<object>>)` / `RemoveSelectionChanged` | Selection changed (objects) |
| `AddSelectedIndicesChanged(Action<IEnumerable<int>>)` / `RemoveSelectedIndicesChanged` | Selection changed (indices) |
| `AddItemIndexChanged(Action<int, int>)` / `RemoveItemIndexChanged` | Item moved (drag-reorder) |
| `AddItemsSourceChanged(Action)` / `RemoveItemsSourceChanged` | `itemsSource` reference changed |
| `AddCanStartDrag(Func<CanStartDragArgs, bool>)` / `RemoveCanStartDrag` | Custom drag-start gating |
| `AddSetupDragAndDrop(Func<SetupDragAndDropArgs, StartDragArgs>)` / `RemoveSetupDragAndDrop` | Drag-and-drop preparation |
| `AddDragAndDropUpdate(Func<HandleDragAndDropArgs, DragVisualMode>)` / `RemoveDragAndDropUpdate` | Drag-and-drop visual mode |
| `AddHandleDrop(Func<HandleDragAndDropArgs, DragVisualMode>)` / `RemoveHandleDrop` | Drop handling |

#### BaseListView configuration

| Method | Description |
|-------|----------|
| `SetAllowAdd(bool)` · `SetAllowRemove(bool)` | Toggles built-in add/remove buttons |
| `SetHeaderTitle(string)` | Title shown when foldout header is on |
| `SetShowFoldoutHeader(bool)` | Wraps the list in a `Foldout` |
| `SetShowAddRemoveFooter(bool)` | Toggles the add/remove footer |
| `SetShowBoundCollectionSize(bool)` | Shows the collection-size field |
| `SetReorderMode(ListViewReorderMode)` | `Simple` or `Animated` |
| `SetBindingSourceSelectionMode(BindingSourceSelectionMode)` | Auto-assign / manual |
| `SetOnAdd(Action<BaseListView>)` · `AddOnAdd` · `RemoveOnAdd` | Custom add-button handler |
| `SetOnRemove(Action<BaseListView>)` · `AddOnRemove` · `RemoveOnRemove` | Custom remove-button handler |
| `SetOverridingAddButtonBehavior(Action<BaseListView, Button>)` · `AddOverridingAddButtonBehavior` · `RemoveOverridingAddButtonBehavior` | Replace add-button behaviour |
| `SetMakeFooter(Func<VisualElement>)` · `AddMakeFooter` · `RemoveMakeFooter` | Footer factory |
| `SetMakeHeader(Func<VisualElement>)` · `AddMakeHeader` · `RemoveMakeHeader` | Header factory |
| `SetMakeNoneElement(Func<VisualElement>)` · `AddMakeNoneElement` · `RemoveMakeNoneElement` | Empty-state factory |
| `AddItemsAdded(Action<IEnumerable<int>>)` / `RemoveItemsAdded` | Items added by index |
| `AddItemsRemoved(Action<IEnumerable<int>>)` / `RemoveItemsRemoved` | Items removed by index |

#### BaseTreeView configuration

| Method | Description |
|-------|----------|
| `SetAutoExpand(bool)` | Auto-expand new nodes |
| `AddItemExpandedChanged(Action<TreeViewExpansionChangedArgs>)` / `RemoveItemExpandedChanged` | Subscribe to expansion changes |

#### Creating ListView and TreeView items

These methods exist in both `ListViewExtensions` and `TreeViewExtensions`, each targeting its own view type.

| Method | Description |
|-------|----------|
| `SetMakeItem(Func<VisualElement>)` · `AddMakeItem` · `RemoveMakeItem` | Item factory |
| `SetBindItem(Action<VisualElement, int>)` · `AddBindItem` · `RemoveBindItem` | Item binding |
| `SetUnbindItem(Action<VisualElement, int>)` · `AddUnbindItem` · `RemoveUnbindItem` | Item unbinding |
| `SetDestroyItem(Action<VisualElement>)` · `AddDestroyItem` · `RemoveDestroyItem` | Item teardown |
| `SetItemTemplate(VisualTreeAsset)` | UXML template used to build items |

#### `MultiColumnListView` / `MultiColumnTreeView`

| Method | Description |
|-------|----------|
| `SetSortingMode(ColumnSortingMode)` | Built-in sorting mode for the column header |

</details>

<a id="editor-commands-editor-only"></a>

## Editor extensions

Add `using Aspid.FastTools.UIElements.Editors;` and `using UnityEditor.UIElements;` to your editor script.

### SerializedObject binding

The `_manaCost` field belongs to the `AbilityBook` component from [SerializedProperty Extensions](08-serialized-property-extensions.md#quick-start):

```csharp
var field = new IntegerField("Mana cost");
```

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>field.bindingPath = "_manaCost";&#10;field.Bind(serializedObject);</code></pre> | <pre lang="csharp"><code>field.BindTo(&#10;    serializedObject, "_manaCost");</code></pre> |
| <pre lang="csharp"><code>var property = serializedObject&#10;    .FindProperty("_manaCost");&#10;field.BindProperty(property);</code></pre> | <pre lang="csharp"><code>var property = serializedObject&#10;    .FindProperty("_manaCost");&#10;field.BindPropertyTo(property);</code></pre> |
| <pre lang="csharp"><code>root.Bind(serializedObject);</code></pre> | <pre lang="csharp"><code>root.BindTo(serializedObject);</code></pre> |
| <pre lang="csharp"><code>// Unbind&#10;root.Unbind();</code></pre> | <pre lang="csharp"><code>// Unbind&#10;root.UnbindFrom();</code></pre> |

For a `root` tree, first set field paths with `SetBindingPath`, then call `BindTo` on the root.

In `CreateInspectorGUI()`, [Unity automatically binds the returned tree](https://docs.unity3d.com/6000.0/Documentation/Manual/UIE-Binding.html) to `serializedObject`. Setting the path is sufficient in that Inspector:

```csharp
public override VisualElement CreateInspectorGUI()
{
    return new IntegerField("Mana cost")
        .SetBindingPath("_manaCost");
}
```

`SetDataSource`, `SetDataSourceType`, and `SetDataSourcePath` configure UI Toolkit runtime data binding sources. They do not create a `SerializedObject` binding by themselves.

### PropertyField

`PropertyField.AddValueChanged` receives a `SerializedPropertyChangeEvent`. For a regular `IntegerField.AddValueChanged`, the argument is a `ChangeEvent<int>`:

```csharp
var manaCost = serializedObject.FindProperty("_manaCost");
var field = new PropertyField(manaCost)
    .SetLabel("Mana cost")
    .AddValueChanged(evt =>
        Debug.Log(evt.changedProperty.intValue));
```

`RemoveValueChanged` removes the same delegate's subscription. To write properties from your own code, see [SerializedProperty Extensions](08-serialized-property-extensions.md).

### Opening scripts and finding the owner window

```csharp
image.AddOpenScriptCommand(target);
// Double-click opens the target's script in the IDE

var window = image.GetOwnerWindow();
```

`target` is the `MonoBehaviour` or `ScriptableObject` whose script should open. `GetOwnerWindow()` looks up the window through the element's panel. If none is found, it falls back to the focused window, then the window under the cursor; the result can be `null`. This helps position a popup when a click has arrived but focus has not switched yet.

<a id="uss-custom-style-helpers-icustomstyle"></a>

## Custom USS properties

`TryGetByEnum` reads a string USS property and parses it as an enum, ignoring case. For example, with this rule in an attached USS file:

```css
.ability-panel {
    --ability-theme: dark;
}
```

Create an element that responds when custom styles are resolved:

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

Add `AbilityPanel` to a tree with the USS attached. Both `dark` and `Dark` parse as `PanelTheme.Dark`. The method returns `false` if the property is missing or the string cannot be parsed.

## Practical example

[EditorTools](../Samples~/EditorTools/Documentation/README.md) contains an ability catalogue and a reactive Inspector built on these extensions:

![Halve cooldown, +5 MP updates both the fields and effect description; Undo restores them.](../Samples~/EditorTools/Documentation/Images/demo.gif)

Halve cooldown, +5 MP updates both the fields and effect description; Undo restores them.

## Next steps

- [EditorTools](../Samples~/EditorTools/Documentation/README.md) — a complete window with search, a list, and asset editing.
- [SerializedProperty Extensions](08-serialized-property-extensions.md) — changing data through Unity serialization.
- [Editor Helpers](09-editor-helpers.md) — object and component labels.

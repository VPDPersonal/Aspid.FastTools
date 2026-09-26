# VisualElement Extensions

In UI Toolkit every element property is a separate statement: four lines for padding, a variable for each child and an `Add` at the end. FastTools turns properties, styles and events into methods that return the element itself, so an inspector header is one expression and `SetPaddingX(12)` replaces the `style.paddingLeft` and `style.paddingRight` pair.

## Quick start

The examples on this page build the inspector of the `AbilityConfig` asset from the [EditorTools sample](../Samples~/EditorTools/Documentation/README.md). Add `using Aspid.FastTools.UIElements;`:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>var title = new Label("Ability Config");&#10;title.style.fontSize = 14;&#10;&#10;var header = new VisualElement();&#10;header.style.paddingLeft = 12;&#10;header.style.paddingRight = 12;&#10;header.style.paddingTop = 10;&#10;header.style.paddingBottom = 10;&#10;header.Add(title);</code></pre> | <pre lang="csharp"><code>var header = new VisualElement()&#10;    .SetPaddingX(12)&#10;    .SetPaddingY(10)&#10;    .AddChild(new Label("Ability Config")&#10;        .SetFontSize(14));</code></pre> |

A setter returns the same type: `new Button().SetText("Create")` is a `Button`. `AddChild` and the other child operations return the parent. Methods on `style`, `textEdition` and `textSelection` return that object, not the element.

## Method names

Method names follow one rule:

| Unity | FastTools | Examples |
|---|---|---|
| property `x` | `SetX(value)` | `tooltip` → `SetTooltip` |
| property `isX` | `SetX(value)`, without `is` | `isDelayed` → `SetDelayed` |
| style property `style.x` | `SetX(value)` on the element and on `style` | `style.fontSize` → `SetFontSize` |
| event `x` | `AddX` / `RemoveX` | `clicked` → `AddClicked` |
| delegate property `x` | `SetX`; an `Action` also gets `AddX` / `RemoveX` | `bindItem` → `SetBindItem`, `AddBindItem` |
| method `M()` | `MSelf()` | `Focus()` → `FocusSelf()` |

The rule covers `VisualElement`, `Focusable`, text elements, fields, sliders, `Button`, `Foldout`, `HelpBox`, `Image`, `ProgressBar`, `IMGUIContainer`, and the list and tree views, but not every property gets a method: `VisualElement.generateVisualContent`, `TextField.multiline` and `ScrollView.mode` have none. The full list is in the [API reference](https://vpdpersonal.github.io/Aspid.FastTools/api/Aspid.FastTools.UIElements). Exceptions:

- `EnumField` and `EnumFlagsField` (in the editor) get `Initialize` in place of `Init`;
- `Button.SetClickable` takes a `Clickable` or an `Action`;
- `TreeView` and `MultiColumnTreeView` are filled with `SetRootItemsSelf`, not `SetItemsSource`.

`IsFocused()` checks focus:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>bool focused = search.focusController?&#10;    .focusedElement == search;</code></pre> | <pre lang="csharp"><code>bool focused = search.IsFocused();</code></pre> |

Like `focusedElement`, it reports the outermost composite element: a field inside a `Vector3Field` or a `ListView` row returns `false`.

## Children

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>header.Add(title);</code></pre> | <pre lang="csharp"><code>header.AddChild(title);</code></pre> |
| <pre lang="csharp"><code>header.Add(title);&#10;header.Add(badge);</code></pre> | <pre lang="csharp"><code>header.AddChildren(title, badge);</code></pre> |
| <pre lang="csharp"><code>header.Insert(0, badge);</code></pre> | <pre lang="csharp"><code>header.InsertChild(0, badge);</code></pre> |
| <pre lang="csharp"><code>header.Insert(0, title);&#10;header.Insert(1, badge);</code></pre> | <pre lang="csharp"><code>header.InsertChildren(0, title, badge);</code></pre> |
| <pre lang="csharp"><code>header.Remove(badge);</code></pre> | <pre lang="csharp"><code>header.RemoveChild(badge);</code></pre> |
| <pre lang="csharp"><code>header.RemoveAt(0);</code></pre> | <pre lang="csharp"><code>header.RemoveChildAt(0);</code></pre> |
| <pre lang="csharp"><code>header.Clear();</code></pre> | <pre lang="csharp"><code>header.ClearChildren();</code></pre> |
| <pre lang="csharp"><code>if (isFree)&#10;    body.Add(helpBox);</code></pre> | <pre lang="csharp"><code>body.AddChildIf(isFree, helpBox);</code></pre> |

Every method has an `…If(condition, …)` variant. `AddChildren` and `InsertChildren` take `params`, `IEnumerable`, `List`, `Span` or `ReadOnlySpan`, keep the order of the elements and skip a `null` collection. An `IEnumerable` is copied first, so `target.AddChildren(source.Children())` moves every child.

## Styles

One value sets every side, `X` sets left and right, `Y` top and bottom. Omitted named parameters keep the previous value:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>header.style.paddingLeft = 12;&#10;header.style.paddingRight = 12;&#10;header.style.borderBottomWidth = 1;</code></pre> | <pre lang="csharp"><code>header&#10;    .SetPaddingX(12)&#10;    .SetBorderWidth(bottom: 1);</code></pre> |

| Method | Sets | Variants |
|---|---|---|
| `SetMargin`, `SetPadding` | `margin…`, `padding…` | `X`, `Y`, `Top`, `Right`, `Bottom`, `Left` |
| `SetBorderWidth`, `SetBorderColor` | `border…Width`, `border…Color` | `X`, `Y`, `Top`, `Right`, `Bottom`, `Left` |
| `SetUnitySlice` | `unitySlice…` | `X`, `Y`, `Top`, `Right`, `Bottom`, `Left` |
| `SetDistance` | `top`, `right`, `bottom`, `left` | `X`, `Y`; one side: `SetTop`, `SetRight`, `SetBottom`, `SetLeft` |
| `SetBorderRadius` | `border…Radius` | `Top`, `Bottom`, `Left`, `Right`, `TopLeft`, `TopRight`, `BottomRight`, `BottomLeft` |
| `SetSize`, `SetMinSize`, `SetMaxSize` | `width` and `height`, `min…`, `max…` | one value, two values or one named |
| `SetBackgroundPosition` | `backgroundPositionX`, `backgroundPositionY` | `X`, `Y` |

Colour setters also take a hex string, and asset setters take a `Resources` path:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>if (ColorUtility.TryParseHtmlString(&#10;        "#FFC24D", out var color))&#10;    badge.style.color = color;</code></pre> | <pre lang="csharp"><code>badge.SetColor("#FFC24D");</code></pre> |
| <pre lang="csharp"><code>root.style.backgroundImage = Resources&#10;    .Load&lt;Texture2D&gt;("UI/Card");</code></pre> | <pre lang="csharp"><code>root.SetBackgroundImageFromResources(&#10;    "UI/Card");</code></pre> |

If the string does not parse as a colour or no asset exists at the path, the method logs a warning and leaves the element unchanged. `SetImageFromResources`, `SetSpriteFromResources`, `SetVectorImageFromResources` and `AddStyleSheetFromResources` take a `Resources` path too.

### Bold and italic

`SetNormalUnityFontStyleAndWeight()` resets both flags; the other presets change one flag and keep the other:

| Method | Changes |
|---|---|
| `AddBold…` | `Normal` → `Bold`, `Italic` → `BoldAndItalic` |
| `RemoveBold…` | `Bold` → `Normal`, `BoldAndItalic` → `Italic` |
| `AddItalic…` | `Normal` → `Italic`, `Bold` → `BoldAndItalic` |
| `RemoveItalic…` | `Italic` → `Normal`, `BoldAndItalic` → `Bold` |

Other values stay as they are.

> [!NOTE]
> The presets read the current value from the element's `style`, not the resolved style: a weight set in USS counts as `Normal`.

## USS classes and style sheets

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>badge.AddToClassList("free");</code></pre> | <pre lang="csharp"><code>badge.AddClass("free");</code></pre> |
| <pre lang="csharp"><code>badge.RemoveFromClassList("free");</code></pre> | <pre lang="csharp"><code>badge.RemoveClass("free");</code></pre> |
| <pre lang="csharp"><code>badge.ToggleInClassList("free");</code></pre> | <pre lang="csharp"><code>badge.ToggleClass("free");</code></pre> |
| <pre lang="csharp"><code>badge.EnableInClassList(&#10;    "free", isFree);</code></pre> | <pre lang="csharp"><code>badge.EnableClass("free", isFree);</code></pre> |
| <pre lang="csharp"><code>badge.ClearClassList();</code></pre> | <pre lang="csharp"><code>badge.ClearClasses();</code></pre> |
| <pre lang="csharp"><code>root.styleSheets.Add(styleSheet);</code></pre> | <pre lang="csharp"><code>root.AddStyleSheet(styleSheet);</code></pre> |
| <pre lang="csharp"><code>root.styleSheets.Remove(styleSheet);</code></pre> | <pre lang="csharp"><code>root.RemoveStyleSheet(styleSheet);</code></pre> |
| <pre lang="csharp"><code>root.styleSheets.Remove(Resources&#10;    .Load&lt;StyleSheet&gt;("UI/Ability"));</code></pre> | <pre lang="csharp"><code>root.RemoveStyleSheetFromResources(&#10;    "UI/Ability");</code></pre> |

## Values and events

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>manaCost.value = 10;</code></pre> | <pre lang="csharp"><code>manaCost.SetValue(10);</code></pre> |
| <pre lang="csharp"><code>manaCost.SetValueWithoutNotify(10);</code></pre> | <pre lang="csharp"><code>manaCost.SetValue(10, notify: false);</code></pre> |
| <pre lang="csharp"><code>manaCost.RegisterValueChangedCallback(&#10;    OnManaCostChanged);</code></pre> | <pre lang="csharp"><code>manaCost.AddValueChanged(&#10;    OnManaCostChanged);</code></pre> |
| <pre lang="csharp"><code>manaCost.UnregisterValueChangedCallback(&#10;    OnManaCostChanged);</code></pre> | <pre lang="csharp"><code>manaCost.RemoveValueChanged(&#10;    OnManaCostChanged);</code></pre> |

Typed overloads cover numbers, `string`, `bool`, vectors, Unity types such as `Color` and `Gradient` and, with `com.unity.mathematics` installed, its vectors, matrices and `quaternion`, so `AddValueChanged(evt => …)` needs no type arguments. Other types use `SetValue<TField, TValue>` and `AddValueChanged<TField, TValue>`.

## Manipulators

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>title.AddManipulator(&#10;    new Clickable(Refresh));</code></pre> | <pre lang="csharp"><code>title.AddClickable(Refresh);</code></pre> |
| <pre lang="csharp"><code>var clickable = new Clickable(Refresh);&#10;title.AddManipulator(clickable);</code></pre> | <pre lang="csharp"><code>title.AddClickable(&#10;    Refresh, out var clickable);</code></pre> |
| <pre lang="csharp"><code>title.AddManipulator(&#10;    new KeyboardNavigationManipulator(&#10;        OnNavigate));</code></pre> | <pre lang="csharp"><code>title.AddKeyboardNavigationManipulator(&#10;    OnNavigate);</code></pre> |
| <pre lang="csharp"><code>title.AddManipulator(&#10;    new ContextualMenuManipulator(&#10;        BuildMenu));</code></pre> | <pre lang="csharp"><code>title.AddContextualMenuManipulator(&#10;    BuildMenu);</code></pre> |

`AddClickable` and the `Add…Manipulator` methods have an `out` overload that keeps the manipulator for `RemoveManipulatorSelf`. `AddClickable` also takes an `Action<EventBase>`, or an `Action` with `delay` and `interval`.

## Editor extensions

These methods live in the `Aspid.FastTools.Editor` assembly and work only in the editor. Add `using Aspid.FastTools.UIElements.Editors;`:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>title.bindingPath = "_abilityName";&#10;title.Bind(serializedObject);</code></pre> | <pre lang="csharp"><code>title.BindTo(&#10;    serializedObject, "_abilityName");</code></pre> |
| <pre lang="csharp"><code>title.bindingPath = "_abilityName";</code></pre> | <pre lang="csharp"><code>title.SetBindingPath("_abilityName");</code></pre> |
| <pre lang="csharp"><code>title.BindProperty(abilityName);</code></pre> | <pre lang="csharp"><code>title.BindPropertyTo(abilityName);</code></pre> |
| <pre lang="csharp"><code>root.Bind(serializedObject);</code></pre> | <pre lang="csharp"><code>root.BindTo(serializedObject);</code></pre> |
| <pre lang="csharp"><code>root.Unbind();</code></pre> | <pre lang="csharp"><code>root.UnbindFrom();</code></pre> |

`PropertyField` gets `SetLabel` and `AddValueChanged` / `RemoveValueChanged` with a `SerializedPropertyChangeEvent`:

```csharp
var manaCost = new PropertyField(
        serializedObject.FindProperty("_manaCost"))
    .SetLabel("Mana cost")
    .AddValueChanged(_ => Refresh());
```

To write a property from your own code, see [SerializedProperty Extensions](08-serialized-property-extensions.md).

### Script and owner window

`AddOpenScriptCommand` opens the script of a `MonoBehaviour` or `ScriptableObject` in the IDE on a left double-click; for any other object it adds nothing:

```csharp
title.AddOpenScriptCommand(target);
```

`GetOwnerWindow()` returns the window whose panel holds the element; otherwise the focused window, then the window under the cursor, or `null`.

## Custom USS properties

`TryGetByEnum` reads a string USS property and parses it as an enum, ignoring case:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>if (evt.customStyle.TryGetValue(&#10;        ThemeProperty, out var raw)&#10;    &amp;&amp; Enum.TryParse(raw,&#10;        ignoreCase: true,&#10;        out PreviewTheme theme))&#10;    ApplyTheme(theme);</code></pre> | <pre lang="csharp"><code>if (evt.customStyle.TryGetByEnum(&#10;        ThemeProperty,&#10;        out PreviewTheme theme))&#10;    ApplyTheme(theme);</code></pre> |

## Package sample

In [EditorTools](../Samples~/EditorTools/Documentation/README.md) the catalogue window and the `AbilityConfig` inspector are built with these extensions: a `ListView` in one chain, `BindTo` for the fields, `AddOpenScriptCommand` on the title.

![Halve cooldown, +5 MP updates both the fields and effect description; Undo restores them.](../Samples~/EditorTools/Documentation/Images/demo.gif)

Halve cooldown, +5 MP updates both the fields and effect description; Undo restores them.

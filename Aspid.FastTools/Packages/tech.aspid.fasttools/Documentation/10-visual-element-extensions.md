# VisualElement Extensions

UI Toolkit interfaces in one call chain — no separate line per property and style.

## Quick start

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>var title = new Label("Ability Config");&#10;title.style.fontSize = 14;&#10;&#10;var header = new VisualElement();&#10;header.style.paddingLeft = 12;&#10;header.style.paddingRight = 12;&#10;header.style.paddingTop = 10;&#10;header.style.paddingBottom = 10;&#10;header.Add(title);</code></pre> | <pre lang="csharp"><code>var header = new VisualElement()&#10;    .SetPaddingX(12)&#10;    .SetPaddingY(10)&#10;    .AddChild(new Label("Ability Config")&#10;        .SetFontSize(14));</code></pre> |

Configuration methods return the original element, preserving its type.

## Element properties

| Unity | FastTools |
|---|---|
| <code lang="csharp">tooltip = "Mana cost"</code> | <code lang="csharp">SetTooltip("Mana cost")</code> |
| <code lang="csharp">isDelayed = true</code> | <code lang="csharp">SetDelayed(true)</code> |
| <code lang="csharp">bindItem = BindRow</code> | <code lang="csharp">SetBindItem(BindRow)</code> |

The full list of extensions is in the [API reference](https://vpdpersonal.github.io/Aspid.FastTools/api/Aspid.FastTools.UIElements).

## Children

| Unity | FastTools |
|---|---|
| <code lang="csharp">Add(title)</code> | <code lang="csharp">AddChild(title)</code> |
| <code lang="csharp">Insert(0, title)</code> | <code lang="csharp">InsertChild(0, title)</code> |
| <code lang="csharp">Remove(title)</code> | <code lang="csharp">RemoveChild(title)</code> |
| <code lang="csharp">RemoveAt(0)</code> | <code lang="csharp">RemoveChildAt(0)</code> |
| <code lang="csharp">Clear()</code> | <code lang="csharp">ClearChildren()</code> |

These methods return the parent. For multiple elements, use <code lang="function">AddChildren</code>, <code lang="function">InsertChildren</code>, <code lang="function">RemoveChildren</code>.

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>if (isFree)&#10;    body.Add(helpBox);</code></pre> | <pre lang="csharp"><code>body.AddChildIf(isFree, helpBox);</code></pre> |

All of these methods have a variant with the suffix <code lang="csharp">If</code>.

## Styles

| Call | Sets |
|---|---|
| <code lang="csharp">SetPadding(8)</code> | every side |
| <code lang="csharp">SetPaddingX(8)</code> | left and right |
| <code lang="csharp">SetPaddingY(8)</code> | top and bottom |
| <code lang="csharp">SetPadding(top: 8, left: 4)</code> | top and left, keeps the rest |
| <code lang="csharp">SetBorderRadiusTop(6)</code> | both top corners |
| <code lang="csharp">SetSize(24, 16)</code> | <code lang="csharp">width</code> and <code lang="csharp">height</code> |
| <code lang="csharp">SetTop(8)</code> | <code lang="csharp">top</code>, like <code lang="csharp">SetDistance(top: 8)</code> |

Other style methods follow the same pattern. See the [API reference](https://vpdpersonal.github.io/Aspid.FastTools/api/Aspid.FastTools.UIElements.VisualElementExtensions) for the full list.

### Colors from strings and assets from Resources

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>if (ColorUtility.TryParseHtmlString(&#10;        "#FFC24D", out var color))&#10;    badge.style.color = color;</code></pre> | <pre lang="csharp"><code>badge.SetColor("#FFC24D");</code></pre> |
| <pre lang="csharp"><code>var texture = Resources&#10;    .Load&lt;Texture2D&gt;("UI/Card");&#10;if (texture != null)&#10;    root.style.backgroundImage = texture;</code></pre> | <pre lang="csharp"><code>root.SetBackgroundImageFromResources(&#10;    "UI/Card");</code></pre> |

<code lang="function">SetImage</code>, <code lang="function">SetSprite</code>, <code lang="function">SetVectorImage</code>, <code lang="function">AddStyleSheet</code> and <code lang="function">RemoveStyleSheet</code> also have a <code lang="csharp">…FromResources</code> variant.

### Bold and italic

Adding or removing one style preserves the other.

| Call | Effect |
|---|---|
| <code lang="csharp">AddBoldUnityFontStyleAndWeight()</code> | Adds bold |
| <code lang="csharp">RemoveBoldUnityFontStyleAndWeight()</code> | Removes bold |
| <code lang="csharp">AddItalicUnityFontStyleAndWeight()</code> | Adds italic |
| <code lang="csharp">RemoveItalicUnityFontStyleAndWeight()</code> | Removes italic |
| <code lang="csharp">SetNormalUnityFontStyleAndWeight()</code> | Removes both styles |

> [!NOTE]
> The presets read the current value from the element's <code lang="csharp">style</code>, not the resolved style: a weight set in USS counts as <code lang="csharp">Normal</code>.

### Custom USS properties

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>if (evt.customStyle.TryGetValue(&#10;        ThemeProperty, out var raw)&#10;    &amp;&amp; Enum.TryParse(raw,&#10;        ignoreCase: true,&#10;        out PreviewTheme theme))&#10;    ApplyTheme(theme);</code></pre> | <pre lang="csharp"><code>if (evt.customStyle.TryGetByEnum(&#10;        ThemeProperty,&#10;        out PreviewTheme theme))&#10;    ApplyTheme(theme);</code></pre> |

## USS classes and style sheets

| Unity | FastTools |
|---|---|
| <code lang="csharp">AddToClassList("selected")</code> | <code lang="csharp">AddClass("selected")</code> |
| <code lang="csharp">RemoveFromClassList("selected")</code> | <code lang="csharp">RemoveClass("selected")</code> |
| <code lang="csharp">ToggleInClassList("selected")</code> | <code lang="csharp">ToggleClass("selected")</code> |
| <code lang="csharp">EnableInClassList("selected", isFree)</code> | <code lang="csharp">EnableClass("selected", isFree)</code> |
| <code lang="csharp">ClearClassList()</code> | <code lang="csharp">ClearClasses()</code> |
| <code lang="csharp">styleSheets.Add(sheet)</code> | <code lang="csharp">AddStyleSheet(sheet)</code> |
| <code lang="csharp">styleSheets.Remove(sheet)</code> | <code lang="csharp">RemoveStyleSheet(sheet)</code> |

## Values and events

| Unity | FastTools |
|---|---|
| <code lang="csharp">value = 10</code> | <code lang="csharp">SetValue(10)</code> |
| <code lang="csharp">SetValueWithoutNotify(10)</code> | <code lang="csharp">SetValue(10, notify: false)</code> |
| <code lang="csharp">RegisterValueChangedCallback(OnChanged)</code> | <code lang="csharp">AddValueChanged(OnChanged)</code> |
| <code lang="csharp">UnregisterValueChangedCallback(OnChanged)</code> | <code lang="csharp">RemoveValueChanged(OnChanged)</code> |
| <code lang="csharp">clicked += Refresh</code> | <code lang="csharp">AddClicked(Refresh)</code> |

For custom value types, use the generic <code lang="csharp">SetValue&lt;T, TValue&gt;(…)</code> and <code lang="csharp">AddValueChanged&lt;TField, TValue&gt;(…)</code>.

## Focus

| Unity | FastTools |
|---|---|
| <code lang="csharp">Focus()</code> | <code lang="csharp">FocusSelf()</code> |
| <code lang="csharp">Blur()</code> | <code lang="csharp">BlurSelf()</code> |

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>if (manaCost.focusController?&#10;        .focusedElement == manaCost)&#10;    Refresh();</code></pre> | <pre lang="csharp"><code>if (manaCost.IsFocused())&#10;    Refresh();</code></pre> |

## Manipulators

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>title.AddManipulator(&#10;    new Clickable(Refresh));</code></pre> | <pre lang="csharp"><code>title.AddClickable(Refresh);</code></pre> |

For keyboard navigation and context menus:

```csharp
title.AddKeyboardNavigationManipulator(OnNavigate);
title.AddContextualMenuManipulator(BuildMenu);
```

The <code lang="csharp">out</code> overload lets you keep the manipulator for removal:

```csharp
title.AddClickable(Refresh, out var clickable);

// Later
title.RemoveManipulatorSelf(clickable);
```

## Editor extensions

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>manaCost.bindingPath = "_manaCost";&#10;manaCost.Bind(serializedObject);</code></pre> | <pre lang="csharp"><code>manaCost.BindTo(&#10;    serializedObject, "_manaCost");</code></pre> |

| Unity | FastTools |
|---|---|
| <code lang="csharp">bindingPath = "_manaCost"</code> | <code lang="csharp">SetBindingPath("_manaCost")</code> |
| <code lang="csharp">BindProperty(property)</code> | <code lang="csharp">BindPropertyTo(property)</code> |
| <code lang="csharp">Unbind()</code> | <code lang="csharp">UnbindFrom()</code> |

<code lang="class-name">PropertyField</code> gets <code lang="function">SetLabel</code> and <code lang="function">AddValueChanged</code> / <code lang="function">RemoveValueChanged</code> with a <code lang="class-name">SerializedPropertyChangeEvent</code>:

```csharp
var manaCost = new PropertyField(
        serializedObject.FindProperty("_manaCost"))
    .SetLabel("Mana cost")
    .AddValueChanged(_ => Refresh());
```

### Open the script on double-click

<code lang="function">AddOpenScriptCommand</code> opens the script of a <code lang="class-name">MonoBehaviour</code> or <code lang="class-name">ScriptableObject</code> in the IDE on a left double-click; for any other object it adds nothing:

```csharp
title.AddOpenScriptCommand(target);
```

### The element's window

```csharp
var window = title.GetOwnerWindow();
```

Returns the element’s window. If none is found, falls back to the focused window, then the window under the pointer; returns <code lang="csharp">null</code> if none is available.

## Package sample

The catalog window and the <code lang="class-name">AbilityConfig</code> inspector in the [EditorTools](../Samples~/EditorTools/Documentation/README.md) sample are built with these extensions.

![The Ability Catalog window from the EditorTools sample](../Samples~/EditorTools/Documentation/Images/demo.gif)

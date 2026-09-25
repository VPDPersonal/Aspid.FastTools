# VisualElement Extensions

UI Toolkit interfaces in one call chain — no separate line per property and style.

## Quick start

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>var title = new Label("Ability Config");&#10;title.style.fontSize = 14;&#10;&#10;var header = new VisualElement();&#10;header.style.paddingLeft = 12;&#10;header.style.paddingRight = 12;&#10;header.style.paddingTop = 10;&#10;header.style.paddingBottom = 10;&#10;header.Add(title);</code></pre> | <pre lang="csharp"><code>var header = new VisualElement()&#10;    .SetPaddingX(12)&#10;    .SetPaddingY(10)&#10;    .AddChild(new Label("Ability Config")&#10;        .SetFontSize(14));</code></pre> |

A setter returns the same type: <code lang="csharp">new Button().SetText("Create")</code> is a <code lang="class-name">Button</code>. <code lang="function">AddChild</code> and the other child operations return the parent. Methods on <code lang="csharp">style</code>, <code lang="csharp">textEdition</code> and <code lang="csharp">textSelection</code> return that object, not the element.

## Method names

Method names follow one rule:

| Unity | FastTools | Examples |
|---|---|---|
| property <code lang="csharp">x</code> | <code lang="csharp">SetX(value)</code> | <code lang="csharp">tooltip</code> → <code lang="function">SetTooltip</code> |
| property <code lang="csharp">isX</code> | <code lang="csharp">SetX(value)</code>, without <code lang="csharp">is</code> | <code lang="csharp">isDelayed</code> → <code lang="function">SetDelayed</code> |
| style property <code lang="csharp">style.x</code> | <code lang="csharp">SetX(value)</code> on the element and on <code lang="csharp">style</code> | <code lang="csharp">style.fontSize</code> → <code lang="function">SetFontSize</code> |
| event <code lang="csharp">x</code> | <code lang="function">AddX</code> / <code lang="function">RemoveX</code> | <code lang="csharp">clicked</code> → <code lang="function">AddClicked</code> |
| delegate property <code lang="csharp">x</code> | <code lang="function">SetX</code>; an <code lang="class-name">Action&lt;…&gt;</code> also gets <code lang="function">AddX</code> / <code lang="function">RemoveX</code> | <code lang="csharp">bindItem</code> → <code lang="function">SetBindItem</code>, <code lang="function">AddBindItem</code> |
| method <code lang="csharp">M()</code> | <code lang="csharp">MSelf()</code> | <code lang="csharp">Focus()</code> → <code lang="csharp">FocusSelf()</code> |

The rule covers the properties and events of every element, from <code lang="class-name">VisualElement</code> to <code lang="class-name">MultiColumnTreeView</code>; the full list is in the [API reference](https://vpdpersonal.github.io/Aspid.FastTools/api/Aspid.FastTools.UIElements). Children and USS classes are renamed separately — see the sections below. Exceptions:

- <code lang="class-name">EnumField</code> and <code lang="class-name">EnumFlagsField</code> get <code lang="function">Initialize</code> in place of <code lang="function">Init</code>;
- <code lang="csharp">Button.SetClickable</code> takes a <code lang="class-name">Clickable</code> or an <code lang="class-name">Action</code>;
- <code lang="csharp">IsFocused()</code> checks focus: <code lang="csharp">search.IsFocused()</code> instead of <code lang="csharp">search.focusController?.focusedElement == search</code>.

## Children

| Unity | FastTools |
|---|---|
| <code lang="function">Add</code> | <code lang="function">AddChild</code> |
| several <code lang="function">Add</code> calls | <code lang="function">AddChildren</code> |
| <code lang="function">Insert</code> | <code lang="function">InsertChild</code> |
| several <code lang="function">Insert</code> calls | <code lang="function">InsertChildren</code> |
| <code lang="function">Remove</code> | <code lang="function">RemoveChild</code> |
| <code lang="function">RemoveAt</code> | <code lang="function">RemoveChildAt</code> |
| <code lang="function">Clear</code> | <code lang="function">ClearChildren</code> |

Every method has an <code lang="csharp">…If(condition, …)</code> variant; <code lang="function">AddChildren</code> and <code lang="function">InsertChildren</code> take both <code lang="csharp">params</code> and collections:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>if (isFree)&#10;    body.Add(helpBox);</code></pre> | <pre lang="csharp"><code>body.AddChildIf(isFree, helpBox);</code></pre> |

## Styles

One value sets every side, <code lang="csharp">X</code> sets left and right, <code lang="csharp">Y</code> top and bottom. Omitted named parameters keep the previous value:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>header.style.paddingLeft = 12;&#10;header.style.paddingRight = 12;&#10;header.style.borderBottomWidth = 1;</code></pre> | <pre lang="csharp"><code>header&#10;    .SetPaddingX(12)&#10;    .SetBorderWidth(bottom: 1);</code></pre> |

| Method | Sets | Variants |
|---|---|---|
| <code lang="function">SetMargin</code>, <code lang="function">SetPadding</code> | <code lang="csharp">margin…</code>, <code lang="csharp">padding…</code> | <code lang="csharp">X</code>, <code lang="csharp">Y</code>, <code lang="csharp">Top</code>, <code lang="csharp">Right</code>, <code lang="csharp">Bottom</code>, <code lang="csharp">Left</code> |
| <code lang="function">SetBorderWidth</code>, <code lang="function">SetBorderColor</code> | <code lang="csharp">border…Width</code>, <code lang="csharp">border…Color</code> | <code lang="csharp">X</code>, <code lang="csharp">Y</code>, <code lang="csharp">Top</code>, <code lang="csharp">Right</code>, <code lang="csharp">Bottom</code>, <code lang="csharp">Left</code> |
| <code lang="function">SetUnitySlice</code> | <code lang="csharp">unitySlice…</code> | <code lang="csharp">X</code>, <code lang="csharp">Y</code>, <code lang="csharp">Top</code>, <code lang="csharp">Right</code>, <code lang="csharp">Bottom</code>, <code lang="csharp">Left</code> |
| <code lang="function">SetDistance</code> | <code lang="csharp">top</code>, <code lang="csharp">right</code>, <code lang="csharp">bottom</code>, <code lang="csharp">left</code> | <code lang="csharp">X</code>, <code lang="csharp">Y</code>; one side: <code lang="function">SetTop</code>, <code lang="function">SetRight</code>, <code lang="function">SetBottom</code>, <code lang="function">SetLeft</code> |
| <code lang="function">SetBorderRadius</code> | <code lang="csharp">border…Radius</code> | <code lang="csharp">Top</code>, <code lang="csharp">Bottom</code>, <code lang="csharp">Left</code>, <code lang="csharp">Right</code>, <code lang="csharp">TopLeft</code>, <code lang="csharp">TopRight</code>, <code lang="csharp">BottomRight</code>, <code lang="csharp">BottomLeft</code> |
| <code lang="function">SetSize</code>, <code lang="function">SetMinSize</code>, <code lang="function">SetMaxSize</code> | <code lang="csharp">width</code> and <code lang="csharp">height</code>, <code lang="csharp">min…</code>, <code lang="csharp">max…</code> | one value, two values or one named |
| <code lang="function">SetBackgroundPosition</code> | <code lang="csharp">backgroundPositionX</code>, <code lang="csharp">backgroundPositionY</code> | <code lang="csharp">X</code>, <code lang="csharp">Y</code> |

The side always comes after the property: <code lang="csharp">borderTopWidth</code> → <code lang="function">SetBorderWidthTop</code>, <code lang="csharp">borderTopLeftRadius</code> → <code lang="function">SetBorderRadiusTopLeft</code>.

Colour setters also take an HTML string (<code lang="csharp">"#FFC24D"</code>, <code lang="csharp">"red"</code>), and asset setters a <code lang="csharp">Resources</code> path:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>if (ColorUtility.TryParseHtmlString(&#10;        "#FFC24D", out var color))&#10;    badge.style.color = color;</code></pre> | <pre lang="csharp"><code>badge.SetColor("#FFC24D");</code></pre> |
| <pre lang="csharp"><code>root.style.backgroundImage = Resources&#10;    .Load&lt;Texture2D&gt;("UI/Card");</code></pre> | <pre lang="csharp"><code>root.SetBackgroundImageFromResources(&#10;    "UI/Card");</code></pre> |

If the string does not parse as a colour or no asset exists at the path, the method logs a warning and leaves the element unchanged. <code lang="function">SetImageFromResources</code>, <code lang="function">SetSpriteFromResources</code>, <code lang="function">SetVectorImageFromResources</code>, <code lang="function">AddStyleSheetFromResources</code> and <code lang="function">RemoveStyleSheetFromResources</code> take a <code lang="csharp">Resources</code> path too.

### Bold and italic

<code lang="csharp">SetNormalUnityFontStyleAndWeight()</code> resets both flags; the other presets change one flag and keep the other:

| Method | Changes |
|---|---|
| <code lang="function">AddBold…</code> | <code lang="csharp">Normal</code> → <code lang="csharp">Bold</code>, <code lang="csharp">Italic</code> → <code lang="csharp">BoldAndItalic</code> |
| <code lang="function">RemoveBold…</code> | <code lang="csharp">Bold</code> → <code lang="csharp">Normal</code>, <code lang="csharp">BoldAndItalic</code> → <code lang="csharp">Italic</code> |
| <code lang="function">AddItalic…</code> | <code lang="csharp">Normal</code> → <code lang="csharp">Italic</code>, <code lang="csharp">Bold</code> → <code lang="csharp">BoldAndItalic</code> |
| <code lang="function">RemoveItalic…</code> | <code lang="csharp">Italic</code> → <code lang="csharp">Normal</code>, <code lang="csharp">BoldAndItalic</code> → <code lang="csharp">Bold</code> |

> [!NOTE]
> The presets read the current value from the element's <code lang="csharp">style</code>, not the resolved style: a weight set in USS counts as <code lang="csharp">Normal</code>.

## USS classes and style sheets

| Unity | FastTools |
|---|---|
| <code lang="function">AddToClassList</code> | <code lang="function">AddClass</code> |
| <code lang="function">RemoveFromClassList</code> | <code lang="function">RemoveClass</code> |
| <code lang="function">ToggleInClassList</code> | <code lang="function">ToggleClass</code> |
| <code lang="function">EnableInClassList</code> | <code lang="function">EnableClass</code> |
| <code lang="function">ClearClassList</code> | <code lang="function">ClearClasses</code> |
| <code lang="csharp">styleSheets.Add</code> | <code lang="function">AddStyleSheet</code> |
| <code lang="csharp">styleSheets.Remove</code> | <code lang="function">RemoveStyleSheet</code> |

## Values and events

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>manaCost.value = 10;</code></pre> | <pre lang="csharp"><code>manaCost.SetValue(10);</code></pre> |
| <pre lang="csharp"><code>manaCost.SetValueWithoutNotify(10);</code></pre> | <pre lang="csharp"><code>manaCost.SetValue(10, notify: false);</code></pre> |
| <pre lang="csharp"><code>manaCost.RegisterValueChangedCallback(&#10;    OnManaCostChanged);</code></pre> | <pre lang="csharp"><code>manaCost.AddValueChanged(&#10;    OnManaCostChanged);</code></pre> |
| <pre lang="csharp"><code>manaCost.UnregisterValueChangedCallback(&#10;    OnManaCostChanged);</code></pre> | <pre lang="csharp"><code>manaCost.RemoveValueChanged(&#10;    OnManaCostChanged);</code></pre> |

Typed overloads cover every Unity field and, with <code lang="csharp">com.unity.mathematics</code> installed, its types, so <code lang="csharp">AddValueChanged(evt =&gt; …)</code> needs no type arguments. For your own types — <code lang="csharp">SetValue&lt;T, TValue&gt;</code> and <code lang="csharp">AddValueChanged&lt;TField, TValue&gt;</code>.

## Manipulators

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>title.AddManipulator(&#10;    new Clickable(Refresh));</code></pre> | <pre lang="csharp"><code>title.AddClickable(Refresh);</code></pre> |
| <pre lang="csharp"><code>var clickable = new Clickable(Refresh);&#10;title.AddManipulator(clickable);</code></pre> | <pre lang="csharp"><code>title.AddClickable(&#10;    Refresh, out var clickable);</code></pre> |
| <pre lang="csharp"><code>title.AddManipulator(&#10;    new KeyboardNavigationManipulator(&#10;        OnNavigate));</code></pre> | <pre lang="csharp"><code>title.AddKeyboardNavigationManipulator(&#10;    OnNavigate);</code></pre> |
| <pre lang="csharp"><code>title.AddManipulator(&#10;    new ContextualMenuManipulator(&#10;        BuildMenu));</code></pre> | <pre lang="csharp"><code>title.AddContextualMenuManipulator(&#10;    BuildMenu);</code></pre> |

<code lang="function">AddClickable</code> and the <code lang="csharp">Add…Manipulator</code> methods have an <code lang="csharp">out</code> overload that keeps the manipulator for <code lang="function">RemoveManipulatorSelf</code>. <code lang="function">AddClickable</code> also takes an <code lang="class-name">Action&lt;EventBase&gt;</code>, or an <code lang="class-name">Action</code> with <code lang="csharp">delay</code> and <code lang="csharp">interval</code>.

## Editor extensions

| Unity | FastTools |
|---|---|
| <code lang="csharp">bindingPath</code> + <code lang="csharp">Bind(serializedObject)</code> | <code lang="csharp">BindTo(serializedObject, path)</code> |
| <code lang="csharp">bindingPath</code> | <code lang="function">SetBindingPath</code> |
| <code lang="function">BindProperty</code> | <code lang="function">BindPropertyTo</code> |
| <code lang="function">Bind</code> | <code lang="function">BindTo</code> |
| <code lang="function">Unbind</code> | <code lang="function">UnbindFrom</code> |

<code lang="class-name">PropertyField</code> gets <code lang="function">SetLabel</code> and <code lang="function">AddValueChanged</code> / <code lang="function">RemoveValueChanged</code> with a <code lang="class-name">SerializedPropertyChangeEvent</code>:

```csharp
var manaCost = new PropertyField(
        serializedObject.FindProperty("_manaCost"))
    .SetLabel("Mana cost")
    .AddValueChanged(_ => Refresh());
```

To write a property from your own code, see [SerializedProperty Extensions](08-serialized-property-extensions.md).

### Script and owner window

<code lang="function">AddOpenScriptCommand</code> opens the script of a <code lang="class-name">MonoBehaviour</code> or <code lang="class-name">ScriptableObject</code> in the IDE on a left double-click; for any other object it adds nothing:

```csharp
title.AddOpenScriptCommand(target);
```

<code lang="csharp">GetOwnerWindow()</code> returns the window whose panel holds the element; otherwise the focused window, then the window under the cursor, or <code lang="csharp">null</code>.

## Custom USS properties

<code lang="function">TryGetByEnum</code> reads a string USS property and parses it as an enum, ignoring case:

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>if (evt.customStyle.TryGetValue(&#10;        ThemeProperty, out var raw)&#10;    &amp;&amp; Enum.TryParse(raw,&#10;        ignoreCase: true,&#10;        out PreviewTheme theme))&#10;    ApplyTheme(theme);</code></pre> | <pre lang="csharp"><code>if (evt.customStyle.TryGetByEnum(&#10;        ThemeProperty,&#10;        out PreviewTheme theme))&#10;    ApplyTheme(theme);</code></pre> |

## Package sample

In [EditorTools](../Samples~/EditorTools/Documentation/README.md) the catalogue window and the <code lang="class-name">AbilityConfig</code> inspector are built with these extensions: a <code lang="class-name">ListView</code> in one chain, <code lang="function">BindTo</code> for the fields, <code lang="function">AddOpenScriptCommand</code> on the title.

![The Ability Catalog window from the EditorTools sample](../Samples~/EditorTools/Documentation/Images/demo.gif)

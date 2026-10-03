# VisualElement Extensions

UI Toolkit interfaces in one call chain — no separate line per property and style.

## Quick start

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>var title = new Label("Ability Config");&#10;title.style.fontSize = 14;&#10;&#10;var header = new VisualElement();&#10;header.style.paddingLeft = 12;&#10;header.style.paddingRight = 12;&#10;header.style.paddingTop = 10;&#10;header.style.paddingBottom = 10;&#10;header.Add(title);</code></pre> | <pre lang="csharp"><code>var header = new VisualElement()&#10;    .SetPaddingX(12)&#10;    .SetPaddingY(10)&#10;    .AddChild(new Label("Ability Config")&#10;        .SetFontSize(14));</code></pre> |

The chain keeps the type: <code lang="csharp">new Button().SetText("Create")</code> is a <code lang="class-name">Button</code>, and <code lang="function">AddChild</code> returns the parent, not the added child.

## Method names

| Unity | FastTools |
|---|---|
| <code lang="csharp">tooltip = "Mana cost"</code> | <code lang="csharp">SetTooltip("Mana cost")</code> |
| <code lang="csharp">isDelayed = true</code> | <code lang="csharp">SetDelayed(true)</code> |
| <code lang="csharp">style.fontSize = 14</code> | <code lang="csharp">SetFontSize(14)</code> |
| <code lang="csharp">clicked += Refresh</code> | <code lang="csharp">AddClicked(Refresh)</code> |
| <code lang="csharp">bindItem = BindRow</code> | <code lang="csharp">SetBindItem(BindRow)</code> |
| <code lang="csharp">Focus()</code> | <code lang="csharp">FocusSelf()</code> |
| <code lang="csharp">Init(theme)</code> | <code lang="csharp">Initialize(theme)</code> |

Other Unity methods get names by meaning — see the sections below. The full list is in the [API reference](https://vpdpersonal.github.io/Aspid.FastTools/api/Aspid.FastTools.UIElements).

## Children

| Unity | FastTools |
|---|---|
| <code lang="function">Add</code> | <code lang="function">AddChild</code>, <code lang="function">AddChildren</code> |
| <code lang="function">Insert</code> | <code lang="function">InsertChild</code>, <code lang="function">InsertChildren</code> |
| <code lang="function">Remove</code> | <code lang="function">RemoveChild</code>, <code lang="function">RemoveChildren</code> |
| <code lang="function">RemoveAt</code> | <code lang="function">RemoveChildAt</code> |
| <code lang="function">Clear</code> | <code lang="function">ClearChildren</code> |

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>if (isFree)&#10;    body.Add(helpBox);</code></pre> | <pre lang="csharp"><code>body.AddChildIf(isFree, helpBox);</code></pre> |

Every method in the first table has an <code lang="csharp">…If</code> variant.

## Styles

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>header.style.marginTop = 4;&#10;header.style.marginBottom = 4;&#10;header.style.borderBottomWidth = 1;</code></pre> | <pre lang="csharp"><code>header&#10;    .SetMarginY(4)&#10;    .SetBorderWidth(bottom: 1);</code></pre> |

| Call | Sets |
|---|---|
| <code lang="csharp">SetPadding(8)</code> | every side |
| <code lang="csharp">SetPaddingX(8)</code> | left and right |
| <code lang="csharp">SetPaddingY(8)</code> | top and bottom |
| <code lang="csharp">SetPadding(top: 8, left: 4)</code> | top and left, keeps the rest |
| <code lang="csharp">SetBorderRadiusTop(6)</code> | both top corners |
| <code lang="csharp">SetSize(24, 16)</code> | <code lang="csharp">width</code> and <code lang="csharp">height</code> |
| <code lang="csharp">SetTop(8)</code> | <code lang="csharp">top</code>, like <code lang="csharp">SetDistance(top: 8)</code> |

The other style methods work the same way.

### Colors from strings and assets from Resources

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>if (ColorUtility.TryParseHtmlString(&#10;        "#FFC24D", out var color))&#10;    badge.style.color = color;</code></pre> | <pre lang="csharp"><code>badge.SetColor("#FFC24D");</code></pre> |
| <pre lang="csharp"><code>root.style.backgroundImage = Resources&#10;    .Load&lt;Texture2D&gt;("UI/Card");</code></pre> | <pre lang="csharp"><code>root.SetBackgroundImageFromResources(&#10;    "UI/Card");</code></pre> |

<code lang="function">SetImage</code>, <code lang="function">SetSprite</code>, <code lang="function">SetVectorImage</code>, <code lang="function">AddStyleSheet</code> and <code lang="function">RemoveStyleSheet</code> also have a <code lang="csharp">…FromResources</code> variant.

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
| <code lang="function">styleSheets.Add</code> | <code lang="function">AddStyleSheet</code> |
| <code lang="function">styleSheets.Remove</code> | <code lang="function">RemoveStyleSheet</code> |

## Values and events

| Unity | FastTools |
|---|---|
| <code lang="csharp">value = 10</code> | <code lang="csharp">SetValue(10)</code> |
| <code lang="csharp">SetValueWithoutNotify(10)</code> | <code lang="csharp">SetValue(10, notify: false)</code> |
| <code lang="function">RegisterValueChangedCallback</code> | <code lang="function">AddValueChanged</code> |
| <code lang="function">UnregisterValueChangedCallback</code> | <code lang="function">RemoveValueChanged</code> |

- <code lang="csharp">AddValueChanged(evt =&gt; …)</code> needs no type arguments: overloads cover every Unity field and, with <code lang="csharp">com.unity.mathematics</code> installed, its types;
- for your own types — <code lang="csharp">SetValue&lt;T, TValue&gt;(…)</code> and <code lang="csharp">AddValueChanged&lt;TField, TValue&gt;(…)</code>.

## Focus

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>if (manaCost.focusController?&#10;        .focusedElement == manaCost)&#10;    Refresh();</code></pre> | <pre lang="csharp"><code>if (manaCost.IsFocused())&#10;    Refresh();</code></pre> |

## Manipulators

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>title.AddManipulator(&#10;    new Clickable(Refresh));</code></pre> | <pre lang="csharp"><code>title.AddClickable(Refresh);</code></pre> |
| <pre lang="csharp"><code>title.AddManipulator(&#10;    new KeyboardNavigationManipulator(&#10;        OnNavigate));</code></pre> | <pre lang="csharp"><code>title.AddKeyboardNavigationManipulator(&#10;    OnNavigate);</code></pre> |
| <pre lang="csharp"><code>title.AddManipulator(&#10;    new ContextualMenuManipulator(&#10;        BuildMenu));</code></pre> | <pre lang="csharp"><code>title.AddContextualMenuManipulator(&#10;    BuildMenu);</code></pre> |
| <pre lang="csharp"><code>var clickable = new Clickable(Refresh);&#10;title.AddManipulator(clickable);&#10;title.RemoveManipulator(clickable);</code></pre> | <pre lang="csharp"><code>title.AddClickable(&#10;    Refresh, out var clickable);&#10;title.RemoveManipulatorSelf(clickable);</code></pre> |

## Editor extensions

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>manaCost.bindingPath = "_manaCost";&#10;manaCost.Bind(serializedObject);</code></pre> | <pre lang="csharp"><code>manaCost.BindTo(&#10;    serializedObject, "_manaCost");</code></pre> |

| Unity | FastTools |
|---|---|
| <code lang="csharp">bindingPath</code> | <code lang="function">SetBindingPath</code> |
| <code lang="function">BindProperty</code> | <code lang="function">BindPropertyTo</code> |
| <code lang="function">Unbind</code> | <code lang="function">UnbindFrom</code> |

<code lang="class-name">PropertyField</code> gets <code lang="function">SetLabel</code> and <code lang="function">AddValueChanged</code> / <code lang="function">RemoveValueChanged</code> with a <code lang="class-name">SerializedPropertyChangeEvent</code>:

```csharp
var manaCost = new PropertyField(
        serializedObject.FindProperty("_manaCost"))
    .SetLabel("Mana cost")
    .AddValueChanged(_ => Refresh());
```

To write a property from your own code, see [SerializedProperty Extensions](08-serialized-property-extensions.md).

### Open the script on double-click

<code lang="function">AddOpenScriptCommand</code> opens the script of a <code lang="class-name">MonoBehaviour</code> or <code lang="class-name">ScriptableObject</code> in the IDE on a left double-click; for any other object it adds nothing:

```csharp
title.AddOpenScriptCommand(target);
```

### The element's window

<code lang="function">GetOwnerWindow</code> returns <code lang="csharp">null</code> if the element is null, detached from a panel, or no hosting window is found.

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>var panel = title?.panel;&#10;var window = panel is null&#10;    ? null&#10;    : Resources&#10;        .FindObjectsOfTypeAll&lt;EditorWindow&gt;()&#10;        .FirstOrDefault(w =&gt;&#10;            w &amp;&amp; w.rootVisualElement.panel == panel);</code></pre> | <pre lang="csharp"><code>var window = title.GetOwnerWindow();</code></pre> |

## Custom USS properties

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>if (evt.customStyle.TryGetValue(&#10;        ThemeProperty, out var raw)&#10;    &amp;&amp; Enum.TryParse(raw,&#10;        ignoreCase: true,&#10;        out PreviewTheme theme))&#10;    ApplyTheme(theme);</code></pre> | <pre lang="csharp"><code>if (evt.customStyle.TryGetByEnum(&#10;        ThemeProperty,&#10;        out PreviewTheme theme))&#10;    ApplyTheme(theme);</code></pre> |

## Package sample

The catalog window and the <code lang="class-name">AbilityConfig</code> inspector in the [EditorTools](../Samples~/EditorTools/Documentation/README.md) sample are built with these extensions.

![The Ability Catalog window from the EditorTools sample](../Samples~/EditorTools/Documentation/Images/demo.gif)

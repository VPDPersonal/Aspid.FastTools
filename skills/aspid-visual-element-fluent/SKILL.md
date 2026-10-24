---
name: aspid-visual-element-fluent
description: "Aspid.FastTools chainable UI Toolkit extensions (namespace Aspid.FastTools.UIElements): SetPadding, SetFlexDirection, AddChild/AddChildren, AddClass, AddClicked, SetValue, AddValueChanged, BindTo and more. Use when C# code creates or configures VisualElements (editor windows, inspectors, runtime UI) in a project with tech.aspid.fasttools, or to replace style.x assignments, AddToClassList, RegisterValueChangedCallback or clicked += with fluent calls."
---

# UI Toolkit fluent extensions

`using Aspid.FastTools.UIElements;` (editor helpers: also `using Aspid.FastTools.UIElements.Editors;`). If the user's
scripts have an `.asmdef`, add `"Aspid.FastTools"` to its `references` (editor code: also `"Aspid.FastTools.Editor"`;
`Unity.Mathematics` overloads: `Aspid.FastTools.VisualElements.Math`, enabled automatically when
`com.unity.mathematics` is installed or on Unity 6000.5+, where the types ship in the engine).

Before using a method you have not seen in this file, confirm its exact name, parameters and receiver constraint in
the installed package source. Do not guess. The package is `Packages/tech.aspid.fasttools` (embedded or local) or
`Library/PackageCache/tech.aspid.fasttools@*` (git or registry); its extensions live in
`Runtime/Scripts/VisualElements/Extensions` and, for editor helpers, `Editor/Scripts/VisualElements/Extensions`:

```sh
grep -rnE -A2 'public static [^=]*\bSetPlaceholder\b' Library/PackageCache/tech.aspid.fasttools@*/*/Scripts/VisualElements/Extensions
```

No match means the method does not exist in the installed version.

```csharp
rootVisualElement.AddChild(new VisualElement()
    .AddClass("settings")
    .AddStyleSheetFromResources("UI/Settings")
    .SetPaddingX(12)
    .AddChildren(
        new Label("Settings").SetFontSize(14).AddBoldUnityFontStyleAndWeight(),
        new TextField("Name").SetPlaceholder("Player").AddValueChanged(evt => Rename(evt.newValue)),
        new Toggle("Enabled").SetValue(true, notify: false),
        new Button().SetTextSelf("Apply").EnableClass("primary", true).AddClicked(Apply)));
```

## Chain rules

- Setters return the same static type (`T : SomeElement`), so subtype methods stay available mid-chain.
- Child operations (`AddChild`, `AddChildren`, `InsertChild`, `RemoveChild`, `RemoveChildren`, `ClearChildren`, and their `...If`
  variants) return the **parent**.
- `element.style.SetX(...)` overloads return the `IStyle`; call the element overload to keep chaining.
- Start from the concrete type: after `Q<VisualElement>()` subtype methods (`AddClicked`, `SetLabel`) do not resolve.

## Naming rule

| Unity | FastTools |
|---|---|
| property `x` / `isX`, `style.x` | `SetX(value)` - `tooltip` -> `SetTooltip`, `isReadOnly` -> `SetReadOnly`, `style.fontSize` -> `SetFontSize` |
| event `x` | `AddX` / `RemoveX` - `clicked` -> `AddClicked` |
| delegate property `x` | `SetX` (an `Action` also `AddX`/`RemoveX`) - `bindItem` -> `SetBindItem` |
| method Unity defines or may define | `...Self` on the same receiver as the Unity method - `SetEnabledSelf`, `AddManipulatorSelf`, `RemoveManipulatorSelf` (`VisualElement`), `FocusSelf`, `BlurSelf` (`Focusable`), `SetTextSelf` (`TextElement`), `MarkDirtyLayoutSelf` (`IMGUIContainer` only), `SetRootItemsSelf` (`BaseTreeView`), `RegisterCallbackSelf`, `RegisterCallbackOnceSelf`, `UnregisterCallbackSelf` (`CallbackEventHandler`), `SetVerticalScrollerVisibilitySelf` (`TextField`) |

| Unity API | FastTools |
|---|---|
| `el.style.paddingLeft = 12; el.style.paddingRight = 12;` | `el.SetPaddingX(12)` |
| `el.style.borderBottomWidth = 1;` | `el.SetBorderWidth(bottom: 1)` (omitted sides unchanged) |
| `el.style.width = 100; el.style.height = 50;` | `el.SetSize(100, 50)` |
| `el.style.color = ...;` | `el.SetColor("#FFC24D")` or a `StyleColor` |
| `el.style.backgroundImage = Resources.Load<Texture2D>("UI/Card");` | `el.SetBackgroundImageFromResources("UI/Card")` |
| `el.style.backgroundImage = new StyleBackground(sprite);` | `el.SetBackgroundImage(sprite)` (also a `VectorImage`) |
| `el.AddToClassList("a"); el.EnableInClassList("b", on);` | `el.AddClass("a").EnableClass("b", on)` (also `ToggleClass`, `RemoveClass`) |
| `el.AddToClassList("a"); el.AddToClassList("b");` | `el.AddClasses("a", "b")` (also `RemoveClasses`, `ToggleClasses`, `EnableClasses(on, "a", "b")`) |
| `el.styleSheets.Add(sheet);` | `el.AddStyleSheet(sheet)`; several: `el.AddStyleSheets(baseSheet, themeSheet)` |
| `el.styleSheets.Insert(0, sheet);` | `el.InsertStyleSheet(0, sheet)` (also `InsertStyleSheets`) |
| `if (dark) el.styleSheets.Add(s); else el.styleSheets.Remove(s);` | `el.EnableStyleSheet(s, dark)` (also `EnableStyleSheets(dark, a, b)`) |
| `el.styleSheets.Add(Resources.Load<StyleSheet>("UI/Base"));` x2 | `el.AddStyleSheetsFromResources("UI/Base", "UI/Dark")` (every style sheet method has `...FromResources`) |
| `if (show) parent.Add(help);` | `parent.AddChildIf(show, help)` |
| `field.SetValueWithoutNotify(v);` | `field.SetValue(v, notify: false)` |
| `field.RegisterValueChangedCallback(cb);` | `field.AddValueChanged(cb)` |
| `el.RegisterCallback<ClickEvent>(OnClick);` | `el.RegisterCallbackSelf<Button, ClickEvent>(OnClick)` (also `RegisterCallbackOnceSelf`, `UnregisterCallbackSelf`) |
| `enumField.Init(v);` | `enumField.Initialize(v)` |
| `dropdown.choices = list; dropdown.index = 1;` | `dropdown.SetChoices(list).SetIndex(1)` (`SetChoices` also on `PopupField<T>` and `RadioButtonGroup`; `DropdownField` also takes an array) |
| `minMax.lowLimit = 0; minMax.highLimit = 100;` | `minMax.SetLowLimit(0).SetHighLimit(100)` (also `SetMinValue`, `SetMaxValue`; see Pitfalls) |
| `textField.multiline = true;` | `textField.SetMultiline(true)` (also `SetVerticalScrollerVisibilitySelf`, after `SetMultiline`) |
| `floatField.formatString = "F2";` | `floatField.SetFormatString("F2")` |
| `el.AddManipulator(new Clickable(cb));` | `el.AddClickable(cb)` (also `AddContextualMenuManipulator`) |
| `scroll.mode = ScrollViewMode.Horizontal; scroll.verticalScrollerVisibility = ScrollerVisibility.Hidden;` | `scroll.SetMode(ScrollViewMode.Horizontal).SetVerticalScrollerVisibility(ScrollerVisibility.Hidden)` (also `SetScrollOffset`, page sizes, `SetElasticity`) |
| `tabs.selectedTabIndex = 1; tab.label = "Stats"; tab.closeable = true;` | `tabs.SetSelectedTabIndex(1)`, `tab.SetLabel("Stats").SetCloseable(true).AddClosed(cb)` (also `SetIconImage`, `AddActiveTabChanged`, `AddTabClosed`) |
| `split.fixedPaneIndex = 1; split.orientation = TwoPaneSplitViewOrientation.Vertical;` | `split.SetFixedPaneIndex(1).SetOrientation(TwoPaneSplitViewOrientation.Vertical)` (also `SetFixedPaneInitialDimension`) |

Editor (`Aspid.FastTools.UIElements.Editors`): `BindTo(serializedObject[, path])`, `UnbindFrom()`,
`BindPropertyTo(property)`, `SetBindingPath(path)`, `PropertyField.SetLabel(...)` / `.AddValueChanged(...)`,
`AddOpenScriptCommand(target)` (double-click opens the script), `GetOwnerWindow()`, `EnumFlagsField.Initialize(...)`,
`AddAspidWindowTheme()` (the FastTools palette and theme; style sheets added after it read the `--aspid-colors-*` tokens).

## Pitfalls

- Unity's own `SetEnabled`, `Focus`, `AddManipulator`, `SetValueWithoutNotify`, `Init`, `MarkDirtyLayout` return
  `void` and end the chain; use the replacements above.
- On a `Label`, `Button` or other `TextElement`, set the text with `SetTextSelf`. From Unity 6000.6, `SetText(string)`
  calls Unity's `TextElement.SetText`, which returns `void`. `Foldout`, `HelpBox` and `Toggle` keep `SetText`.
- From Unity 6000.6, `SetTextSelf` also takes an `int`, a `long`, a `ulong`, a `float` (with a format), a `ReadOnlySpan<char>`,
  a `StringBuilder` or a `char[]` slice and calls Unity's allocation-free `TextElement.SetText`. Use them for text that
  changes every frame instead of `SetTextSelf(value.ToString())`. An element that is not attached to a panel yet gets the
  string instead, without a warning, so attach it first to avoid the allocation.
- `EnableClasses` and `EnableStyleSheets` take the flag first: `EnableClasses(on, "a", "b")`, but `EnableClass("a", on)`.
  Name both flags of `EnableClassIf(condition: x, "a", enable: y)`: swapped bools compile.
- These do not exist: `ToggleInClass`, `EnableInClass`, `...FromResource` (singular),
  `SetUnityFontStyle` (use `SetUnityFontStyleAndWeight`), `AddCallback` (use `RegisterCallbackSelf`),
  `SetVerticalScrollerVisibility` on a `TextField` (Unity's obsolete instance method of that name hides it; use
  `...Self`). On a `ScrollView` it exists.
- Values are `Style*` types: sizes `StyleLength` (`12`, `Length.Percent(50)`, `StyleKeyword.Auto`),
  `SetBackgroundImage` a `Sprite`, a `VectorImage` or a `StyleBackground` (a `Texture2D` converts to it),
  transitions `StyleList<T>` from a `List<T>` (not an array).
- String colours and `Resources` paths never throw: a bad value logs a warning and changes nothing.
  Style sheet methods skip `null` style sheets; `RemoveChild` / `RemoveChildren` skip `null` and non-children.
- Add the tabs before `SetActiveTab` and `SetSelectedTabIndex`: Unity throws for a tab that is not in the view and
  ignores an index out of range. Call `ScrollView.SetScrollOffset` after the content is laid out: the scrollers limit
  the offset to the scrollable range. `Tab.AddClosing` callbacks all run, but only the last result counts.
- Positional arguments of `SetPadding`, `SetMargin`, `SetBorderWidth`, `SetBorderColor` and `SetDistance` run top, right,
  bottom, left: `SetPadding(8, 4)` sets the top and the right side, not vertical and horizontal. For pairs use
  `SetPaddingY(8).SetPaddingX(4)`, or name the arguments. `SetBorderRadius` runs top-left, top-right, bottom-right,
  bottom-left.
- `SetValue(v)` raises the change event only when the element is in a panel and `v` differs from the current value;
  `notify: true` does not force it.
- A slider clamps its value to its range, `0..10` for a new `Slider` or `SliderInt`. Call `SetLowValue` and `SetHighValue`
  before `SetValue`: `new Slider().SetValue(50f).SetHighValue(100f)` ends at 10.
- `RemoveValueChanged` removes only an equal delegate: pass a method group or a delegate kept in a field. A lambda that
  captures a local variable is a new delegate on every call: it cannot be removed, and a `bindItem` handler that
  registers one adds one more callback on every bind.
- `ListView`, `TreeView` and their multi-column variants have no content container. `AddChild` and `AddChildren` throw
  `InvalidOperationException` for a non-null child; `InsertChild`, `RemoveChildAt` and `ClearChildren` do nothing.
  Fill a `ListView` or `MultiColumnListView` with `SetItemsSource`, and a `TreeView` or `MultiColumnTreeView` with
  `SetRootItemsSelf`.
- `...If` variants evaluate their arguments even when the condition is false.
- Unity calls the callback of `SetFormatSelectedValueCallback` at once on the current value, which is `null` for a
  `DropdownField` without a selection: call `SetChoices` and `SetIndex` first, or null-check.
- Call `SetChoices` before `SetIndex`: Unity resolves the index against the current choices, so `SetIndex(1)` before
  `SetChoices(list)` selects nothing and raises no error.
- `MinMaxSlider` clamps the range on every assignment. On the default range of 0 to 10,
  `SetMinValue(20).SetMaxValue(30)` gives 10 to 30. Set both ends with `SetValue(new Vector2(20, 30))`, or the upper end
  first when the range moves up (the lower end first when it moves down). `SetLowLimit` throws `ArgumentException`
  above the current high limit, and `SetHighLimit` below the current low limit.
- `SetVerticalScrollerVisibilitySelf` does nothing on a `TextField` that is not multiline yet: call `SetMultiline(true)`
  first.
- Custom value types (`BaseField<MyType>`) need explicit type arguments for `AddValueChanged`, `SetLabel` and text
  setters: `field.AddValueChanged<MyField, MyType>(evt => ...)`. Likewise `PopupField<T>`: `SetChoices` infers `T`, but
  `SetIndex` and the two `SetFormat...Callback` setters need `popup.SetIndex<PopupField<int>, int>(1)`; `DropdownField`
  has overloads without type arguments.
- `RegisterCallbackSelf` infers its type arguments only from a callback with a typed parameter, such as
  `(ClickEvent evt) => ...`. For a method group or an untyped lambda name both: `RegisterCallbackSelf<Button, ClickEvent>(OnClick)`.
- The package supports Unity 6000.0.53f1. Wrap these in `#if UNITY_6000_x_OR_NEWER` when the project must support older
  versions: `SetUnityTextAutoSize` (6000.2); `SetAspectRatio`, `SetFilter`, `SetUnityMaterial`,
  `Add/RemoveOnCursorIndexChange`, `Add/RemoveOnSelectIndexChange` (6000.3); `SetHideSoftKeyboard`, `GUID` values and
  `GUIDField` setters (6000.4); the `SetTextSelf` overloads for numbers, spans, builders and arrays (6000.6).

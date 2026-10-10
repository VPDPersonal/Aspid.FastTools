---
name: aspid-visual-element-fluent
description: "Aspid.FastTools chainable UI Toolkit extensions (namespace Aspid.FastTools.UIElements): SetPadding, SetFlexDirection, AddChild/AddChildren, AddClass, AddClicked, SetValue, AddValueChanged, BindTo and more. Use when C# code creates or configures VisualElements (editor windows, inspectors, runtime UI) in a project with tech.aspid.fasttools, or to replace style.x assignments, AddToClassList, RegisterValueChangedCallback or clicked += with fluent calls."
---

# UI Toolkit fluent extensions

`using Aspid.FastTools.UIElements;` (editor helpers: also `using Aspid.FastTools.UIElements.Editors;`). If the user's
scripts have an `.asmdef`, reference `Aspid.FastTools` (editor code: also `Aspid.FastTools.Editor`; `Unity.Mathematics`
overloads: `Aspid.FastTools.VisualElements.Math`, enabled automatically when `com.unity.mathematics` is installed).

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
| method Unity already defines | `...Self` on the same receiver as the Unity method - `SetEnabledSelf`, `AddManipulatorSelf`, `RemoveManipulatorSelf` (`VisualElement`), `FocusSelf`, `BlurSelf` (`Focusable`), `SetTextSelf` (`TextElement`), `MarkDirtyLayoutSelf` (`IMGUIContainer` only), `SetRootItemsSelf` (`BaseTreeView`) |

| Unity API | FastTools |
|---|---|
| `el.style.paddingLeft = 12; el.style.paddingRight = 12;` | `el.SetPaddingX(12)` |
| `el.style.borderBottomWidth = 1;` | `el.SetBorderWidth(bottom: 1)` (omitted sides unchanged) |
| `el.style.width = 100; el.style.height = 50;` | `el.SetSize(100, 50)` |
| `el.style.color = ...;` | `el.SetColor("#FFC24D")` or a `StyleColor` |
| `el.style.backgroundImage = Resources.Load<Texture2D>("UI/Card");` | `el.SetBackgroundImageFromResources("UI/Card")` |
| `el.AddToClassList("a"); el.EnableInClassList("b", on);` | `el.AddClass("a").EnableClass("b", on)` (also `ToggleClass`, `RemoveClass`) |
| `el.AddToClassList("a"); el.AddToClassList("b");` | `el.AddClasses("a", "b")` (also `RemoveClasses`, `ToggleClasses`, `EnableClasses(on, "a", "b")`) |
| `el.styleSheets.Add(sheet);` | `el.AddStyleSheet(sheet)`; several: `el.AddStyleSheets(baseSheet, themeSheet)` |
| `el.styleSheets.Insert(0, sheet);` | `el.InsertStyleSheet(0, sheet)` (also `InsertStyleSheets`) |
| `if (dark) el.styleSheets.Add(s); else el.styleSheets.Remove(s);` | `el.EnableStyleSheet(s, dark)` (also `EnableStyleSheets(dark, a, b)`) |
| `el.styleSheets.Add(Resources.Load<StyleSheet>("UI/Base"));` x2 | `el.AddStyleSheetsFromResources("UI/Base", "UI/Dark")` (every style sheet method has `...FromResources`) |
| `if (show) parent.Add(help);` | `parent.AddChildIf(show, help)` |
| `field.SetValueWithoutNotify(v);` | `field.SetValue(v, notify: false)` |
| `field.RegisterValueChangedCallback(cb);` | `field.AddValueChanged(cb)` |
| `enumField.Init(v);` | `enumField.Initialize(v)` |
| `el.AddManipulator(new Clickable(cb));` | `el.AddClickable(cb)` (also `AddContextualMenuManipulator`) |

Editor (`Aspid.FastTools.UIElements.Editors`): `BindTo(serializedObject[, path])`, `UnbindFrom()`,
`BindPropertyTo(property)`, `SetBindingPath(path)`, `PropertyField.SetLabel(...)` / `.AddValueChanged(...)`,
`AddOpenScriptCommand(target)` (double-click opens the script), `GetOwnerWindow()`, `EnumFlagsField.Initialize(...)`.

## Pitfalls

- Unity's own `SetEnabled`, `Focus`, `AddManipulator`, `SetValueWithoutNotify`, `Init`, `MarkDirtyLayout` return
  `void` and end the chain; use the replacements above.
- On a `Label`, `Button` or other `TextElement`, set the text with `SetTextSelf`. From Unity 6000.6, `SetText(string)`
  calls Unity's `TextElement.SetText`, which returns `void`. `Foldout`, `HelpBox` and `Toggle` keep `SetText`.
- `EnableClasses` and `EnableStyleSheets` take the flag first: `EnableClasses(on, "a", "b")`, but `EnableClass("a", on)`.
  Name both flags of `EnableClassIf(condition: x, "a", enable: y)`: swapped bools compile.
- These do not exist: `ToggleInClass`, `EnableInClass`, `...FromResource` (singular),
  `SetUnityFontStyle` (use `SetUnityFontStyleAndWeight`), `AddCallback` (use Unity's `RegisterCallback`).
- Values are `Style*` types: sizes `StyleLength` (`12`, `Length.Percent(50)`, `StyleKeyword.Auto`),
  `SetBackgroundImage` only `StyleBackground`, transitions `StyleList<T>` from a `List<T>` (not an array).
- String colours and `Resources` paths never throw: a bad value logs a warning and changes nothing.
  Style sheet methods skip `null` style sheets; `RemoveChild` / `RemoveChildren` skip `null` and non-children.
- `...If` variants evaluate their arguments even when the condition is false.
- Custom value types (`BaseField<MyType>`) need explicit type arguments for `AddValueChanged`, `SetLabel` and text
  setters: `field.AddValueChanged<MyField, MyType>(evt => ...)`.
- The package supports Unity 6000.0.53f1. Wrap these in `#if UNITY_6000_x_OR_NEWER` when the project must support older
  versions: `SetUnityTextAutoSize` (6000.2); `SetAspectRatio`, `SetFilter`, `SetUnityMaterial`,
  `Add/RemoveOnCursorIndexChange`, `Add/RemoveOnSelectIndexChange` (6000.3); `SetHideSoftKeyboard`, `GUID` values
  (6000.4).

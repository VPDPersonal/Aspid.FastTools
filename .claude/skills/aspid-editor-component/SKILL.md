---
name: aspid-editor-component
description: "Adding or changing an internal UI Toolkit editor component of the Aspid.FastTools package (Editor/Scripts/VisualElements/Internal: AspidLabel, AspidBox, AspidDividingLine and the rest) - element class, preset struct, fluent extensions, ThemeStyle/StatusStyle, USS-driven style structs, stylesheet. For work on this repository only."
metadata:
  internal: true
---

# Internal editor component

`PKG` = `Aspid.FastTools/Packages/tech.aspid.fasttools`. Components are `internal` to `Aspid.FastTools.Editor` and
visible to the assemblies in `PKG/Editor/Scripts/AssemblyInfo.cs` (Aspid.Ids, Aspid.MVVM editors, tests) - changing an
existing component's members can break those packages.

Read and mirror, in full, before writing:
- `PKG/Editor/Scripts/VisualElements/Internal/Components/AspidContainers/AspidBox*.cs` - theme + status only;
- `.../Components/AspidDividingLines/` - own style structs in `Styles/`, one `Set<Property>` per `[UxmlAttribute]`;
- `.../Components/AspidLabels/` - child components, nested preset. Its extensions lack `SetLabelFontStyle` and
  `SetLineDirection`: do not copy that gap;
- `.../Internal/Styles/` - `ThemeStyle`, `StatusStyle`, `InlineStyle<T>`, `AspidStyles`;
- the component's sheet in `PKG/Editor/Resources/UI/Components/Aspid-FastTools-Aspid<Name>.uss`.

## Files

`Components/<Group>/Aspid<Name>.cs`, `Aspid<Name>Preset.cs`, `Aspid<Name>Extensions.cs`, optional
`Styles/Aspid<Name><Property>Style.cs`, plus the `.uss` and every `.meta`. Namespace
`Aspid.FastTools.UIElements.Editors.Internal` with `// ReSharper disable once CheckNamespace`.

Preset and Extensions are skipped only when there is nothing to configure (`AspidSwitch` is a `BaseField<bool>`,
`AspidWindowFooter` takes one constructor flag) or the element is a child that its owner configures through the
owner's style struct (`AspidHoverGradientOverlay` inside `AspidInspectorHeader` / `AspidGradientButton`).

## Rules

- `[UxmlElement(libraryPath = "Aspid/FastTools")] internal sealed partial class`; a parameterless constructor for
  UXML; all constructors chain to `(…, Aspid<Name>Preset preset)`.
- Load only the component's own sheet: `this.AddStyleSheetFromResources(StyleSheetPath)`. The palette and
  `.aspid-fasttools-background*` come from the host root's `AddAspidThemeStyleSheets()`; only standalone roots
  (`AspidWindowFooter`, `InspectorNotice`) call it themselves. A themed background = `.AddClass(AspidStyles.BackgroundStyle)
  .AddClass(AspidStyles.BackgroundRoundedState)`.
- Theme and status always reuse `ThemeStyle` / `StatusStyle`; create style structs after children and the sheet exist
  (their constructors apply the class immediately). A component-owned property (size, direction…) gets its own style
  struct shaped like `AspidLabelSizeStyle`: `CustomStyleProperty<string>` `--aspid-fasttools-metrics-<name>`,
  `InlineStyle<Type>`, class constants, `GetClass` with an empty-class guard for `None`.
- Preset: mutable struct, public fields, setters that assign and return `this`, `static Default` built by chaining.
  A nested component preset must be a **field**, not an auto-property, or its setters modify a copy.
- Extensions: one `Set<Property>` per `[UxmlAttribute]`, returning the concrete element type.
- USS: style against `aspid-fasttools-theme--*` / `aspid-fasttools-status--*` (added by the style structs), use the
  shared palette `var(--aspid-colors-*)` / `var(--aspid-icons-*)` so theme overrides work, own classes
  `aspid-fasttools-<block>[__<element>][--<modifier>]`, own custom properties
  `--aspid-fasttools-{colors|prop|metrics|icons}-<name>`. No inline `style.*` for anything a theme should control.

## Pitfalls

- Preset/constructor values are defaults that USS custom properties may override; `SetValue` (attribute setter or
  extension) makes a value inline and USS stops changing it.
- `new Aspid<Name>Preset()` starts at the first enum members (`ThemeStyle.Type.Darkness`); start from `.Default`.
- UXML references components by full type name and kebab-case attributes (`PKG/Editor/Resources/UI/Windows/`);
  renaming a class, namespace or attribute breaks those files.
- A component shown without a host that applied the theme sheets has unresolved colours: fix the host.

Tests that depend on resolved USS go in `PKG/Tests/Editor/VisualElements/` (see
`AspidAnimatedDotsBackgroundStatusTests.cs`: host in an `EditorWindow`, `AddAspidThemeStyleSheets()`, `yield return null`).

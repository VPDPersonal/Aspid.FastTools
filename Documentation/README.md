<img src="https://raw.githubusercontent.com/VPDPersonal/Aspid.FastTools/main/docs/images/aspid_fasttools_readme_banner.gif" alt="Aspid.FastTools" />

![Unity 6.0+](Images/status-badge-unity.svg)
[![Preview 1.0.0-rc.8](Images/status-badge-preview.svg)](https://github.com/VPDPersonal/Aspid.FastTools/releases/tag/v1.0.0-rc.8)
[![MIT License](Images/status-badge-license.svg)](https://github.com/VPDPersonal/Aspid.FastTools/blob/main/LICENSE)

Aspid.FastTools is a Unity package that takes the routine out of serialization and editor code. Unity does not serialize `System.Type`, offers no Inspector picker for a `[SerializeReference]` field's class, and breaks that reference when the class is renamed. The package adds Inspector pickers for a type and a class and an editor for “enum → value” tables, and finds broken references across the project to repair them without data loss. A profiler marker takes one line, with its name generated from the code; UI Toolkit trees and `SerializedProperty` writes fit in one fluent chain instead of separate assignments.

[Documentation](https://vpdpersonal.github.io/Aspid.FastTools/docs) · [Source code](https://github.com/VPDPersonal/Aspid.FastTools) · [Releases](https://github.com/VPDPersonal/Aspid.FastTools/releases)

## Installation

In **Window → Package Manager**, choose **+ → Install package from git URL…** and paste:

```text
https://github.com/VPDPersonal/Aspid.FastTools.git#upm-preview
```

The URL points to the latest preview; **Update** in the Package Manager brings in the next one. To pin a version, add its tag from [Releases](https://github.com/VPDPersonal/Aspid.FastTools/releases): `https://github.com/VPDPersonal/Aspid.FastTools.git#upm-preview/<version>`.

## Features

### Serialization

#### [Serializable Type System](02-serializable-types.md)

Stores a `System.Type` in a component/asset and lets you pick it in the Inspector from compatible types.

<img src="Images/serializable-type-quick-start.gif" alt="Select a serializable type in the Inspector" width="640" />

#### [ComponentTypeSelector](11-component-type-selector.md)

Changes an added component/ScriptableObject to a derived type without losing shared field values.

<img src="Images/component-type-selector.gif" alt="Switch a component type in the Inspector" width="640" />

#### [SerializeReference Selector](03-serialize-reference-selector.md)

Lets you pick the class for a `[SerializeReference]` field in the Inspector and carries compatible data over when the class changes.

<img src="Images/aspid_fasttools_serialize_reference_selector.gif" alt="Switch Pistol to Shotgun while keeping Damage at 37" width="640" />

#### [SerializeReference Tooling](04-serialize-reference-tooling.md)

Finds lost `[SerializeReference]` entries across the project (prefabs, scenes, assets) and repairs them in groups — by hand, before a build or in CI.

<img src="Images/aspid_fasttools_serialize_reference_tooling.gif" alt="Repair a missing weapon type without losing its data" width="640" />

#### [EnumValues](06-enum-values.md)

Maps enum keys to values (multipliers, colors, assets), edited in the Inspector, flags included.

<img src="../Samples~/EnumValues/Documentation/Images/surface-tables.png" alt="Edit enum keys and their values in the Inspector" width="640" />

### Editor & tooling

#### [ProfilerMarkers](05-profiler-markers.md)

Marks a section with one line; the generator takes the marker name from the code, unique per call site.

```csharp
using (this.Marker())
{
    Simulate();
}
```

#### [VisualElement Extensions](07-visual-element-extensions.md)

Sets element properties, styles and events in a chain, so a UI Toolkit tree is built in one expression.

```csharp
new VisualElement()
  .SetPaddingX(12)
  .AddChild(
    new Label("Ability Config"));
```

#### [SerializedProperty Extensions](08-serialized-property-extensions.md)

Writes a value together with `Update` and `Apply` in one chain, and finds the field's C# type and the object the field belongs to.

```csharp
manaCost
  .Update()
  .SetIntAndApply(42);
```

#### [Editor Helpers](09-editor-helpers.md)

Handles small editor-tooling tasks, such as labelling objects and components with readable names.

```csharp
config.GetDisplayName();
// "Ability Config"

config
  .GetDisplayNameWithIndex();
// "Ability Config (2)"
```

#### [Claude Code Plugin](10-claude-code-plugin.md)

Teaches Claude Code to place `this.Marker()` and build UI with the fluent `VisualElement` extensions.

```text
Add a marker for the whole Simulate method
and a separate one for the neighbour search.
```

## Resources

- [Samples overview](../Samples~/README.md) — scenes and editor tools for serialization, enum tables, profiling and editor UI.
- [API reference](https://vpdpersonal.github.io/Aspid.FastTools/api/Aspid.FastTools.Editors) — signatures and descriptions of every public type and member.
- [Changelog](https://vpdpersonal.github.io/Aspid.FastTools/changelog) — what was added, changed and fixed in each version.

## Help and support

Report bugs or ask questions in [GitHub Issues](https://github.com/VPDPersonal/Aspid.FastTools/issues). Include your Unity version, package version and steps to reproduce a problem.

If the package helps you, star it on [GitHub](https://github.com/VPDPersonal/Aspid.FastTools).

Distributed under the [MIT License](https://github.com/VPDPersonal/Aspid.FastTools/blob/main/LICENSE).

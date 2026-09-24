<img src="https://raw.githubusercontent.com/VPDPersonal/Aspid.FastTools/main/docs/images/aspid_fasttools_readme_banner.gif" alt="Aspid.FastTools" />

[![Unity 6.0+](Images/status-badge-unity.svg)](https://assetstore.unity.com/packages/slug/365584)
[![Preview 1.0.0-rc.8](Images/status-badge-preview.svg)](https://github.com/VPDPersonal/Aspid.FastTools/releases)
[![MIT License](Images/status-badge-license.svg)](https://github.com/VPDPersonal/Aspid.FastTools/blob/main/LICENSE)

Aspid.FastTools is a Unity package that fills the gaps Unity leaves in serialization and editor tooling. Serialized types and polymorphic references stay valid through renames, or get repaired without data loss when they break. The Inspector shows what a `SerializeReference` field holds and lets you swap it. Editor and profiling helpers take a line where Unity needs a class.

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

Store and pick a `System.Type` in the Inspector.

<img src="Images/serializable-type-quick-start.gif" alt="Select a serializable type in the Inspector" width="640" />

#### [ComponentTypeSelector](11-component-type-selector.md)

Switch an existing component's type while preserving shared fields.

<img src="Images/component-type-selector.gif" alt="Switch a component type in the Inspector" width="640" />

#### [SerializeReference Selector](03-serialize-reference-selector.md)

Pick which class a `SerializeReference` field holds, straight from the Inspector.

<img src="Images/aspid_fasttools_serialize_reference_selector.gif" alt="Switch Pistol to Shotgun while keeping Damage at 37" width="640" />

#### [SerializeReference Tooling](04-serialize-reference-tooling.md)

Audit and repair references across the whole project, before builds and in CI.

<img src="Images/aspid_fasttools_serialize_reference_tooling.gif" alt="Repair a missing weapon type without losing its data" width="640" />

#### [EnumValues](06-enum-values.md)

Edit enum → value tables in the Inspector, including flags.

<img src="../Samples~/EnumValues/Documentation/Images/surface-tables.png" alt="Edit enum keys and their values in the Inspector" width="640" />

### Editor & tooling

#### [ProfilerMarkers](05-profiler-markers.md)

Generate a unique profiler marker per call site with `this.Marker()`.

```csharp
using (this.Marker())
{
    Simulate();
}
```

#### [VisualElement Extensions](07-visual-element-extensions.md)

Build UI Toolkit trees with fluent chains.

```csharp
new VisualElement()
  .SetPaddingX(12)
  .AddChild(
    new Label("Stats"));
```

#### [SerializedProperty Extensions](08-serialized-property-extensions.md)

Set values, resize arrays, and inspect the field type and owning object.

```csharp
property
  .Update()
  .SetIntAndApply(42);
```

#### [Editor Helpers](09-editor-helpers.md)

Get readable object and component display names for custom editors.

```csharp
fireAbility.GetDisplayName();
// "Fire Ability"

abilityConfig
  .GetDisplayNameWithIndex();
// "Ability Config (1)"
```

## Documentation and samples

- [Samples overview](../Samples~/README.md) — scenes and editor tools for serialization, enum tables, profiling and editor UI. Import them from the **Welcome** window (**Tools → Aspid 🐍 → FastTools → Welcome**), which opens after installation.
- [API reference](https://vpdpersonal.github.io/Aspid.FastTools/api/Aspid.FastTools) — public types and members. The feature links above explain how to use them.
- [Claude Code plugin](10-claude-code-plugin.md) — optional skills for working with this package in Claude Code.
- [Changelog](https://github.com/VPDPersonal/Aspid.FastTools/blob/main/CHANGELOG.md) — release history.

## Help and support

Report bugs or ask questions in [GitHub Issues](https://github.com/VPDPersonal/Aspid.FastTools/issues). Include your Unity version, package version and steps to reproduce a problem.

Once the package is available on the [Unity Asset Store](https://assetstore.unity.com/packages/slug/365584), you can support development by purchasing it.

Distributed under the [MIT License](https://github.com/VPDPersonal/Aspid.FastTools/blob/main/LICENSE).

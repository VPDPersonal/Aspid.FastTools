<img src="https://raw.githubusercontent.com/VPDPersonal/Aspid.FastTools/main/docs/images/aspid_fasttools_readme_banner.gif" alt="Aspid.FastTools" />

[![Unity 6.0+](Images/status-badge-unity.svg)](https://assetstore.unity.com/packages/slug/365584)
[![Preview 1.0.0-rc.8](Images/status-badge-preview.svg)](https://github.com/VPDPersonal/Aspid.FastTools/releases)
[![MIT License](Images/status-badge-license.svg)](https://github.com/VPDPersonal/Aspid.FastTools/blob/main/LICENSE)

Tools for Unity that cut repetitive code in serialization and editor tooling.

[Documentation](https://vpdpersonal.github.io/Aspid.FastTools/docs) · [Source code](https://github.com/VPDPersonal/Aspid.FastTools) · [Releases](https://github.com/VPDPersonal/Aspid.FastTools/releases)

## Features

| Feature | What it gives you | Preview |
|---|---|---|
| [Serializable Type System](02-serializable-types.md) | Store and pick a `System.Type` in the Inspector | ![Select a serializable type in the Inspector](Images/aspid_fasttools_serializable_type.gif) |
| [SerializeReference Selector](03-serialize-reference-selector.md) | Choose implementations and repair broken references in place | ![Switch Pistol to Shotgun while keeping Damage at 37](Images/aspid_fasttools_serialize_reference_selector.gif) |
| [SerializeReference Tooling](04-serialize-reference-tooling.md) | Audit and repair references across the whole project, before builds and in CI | ![Repair a missing weapon type without losing its data](Images/aspid_fasttools_serialize_reference_tooling.gif) |
| [EnumValues](06-enum-values.md) | Edit enum → value tables in the Inspector, including flags | ![Edit enum keys and their values in the Inspector](../Samples~/EnumValues/Documentation/Images/surface-tables.png) |
| [ProfilerMarkers](05-profiler-markers.md) | Generate a unique profiler marker per call site with `this.Marker()` | <pre lang="csharp"><code>using (this.Marker())&#10;&#123;&#10;    Simulate();&#10;&#125;</code></pre> |
| [VisualElement Extensions](07-visual-element-extensions.md) | Build UI Toolkit trees with fluent chains | <pre lang="csharp"><code>new VisualElement()&#10;  .SetPadding(8)&#10;  .AddChild(&#10;    new Label("Stats"));</code></pre> |
| [SerializedProperty Extensions](08-serialized-property-extensions.md) | Set values, resize arrays, and inspect the field type and owning object | <pre lang="csharp"><code>property&#10;  .Update()&#10;  .SetIntAndApply(42);</code></pre> |
| [Editor Helpers](09-editor-helpers.md) | Get readable object and component display names for custom editors | <pre lang="csharp"><code>audio.GetDisplayName();&#10;// "Audio Source"&#10;&#10;secondAudio&#10;  .GetDisplayNameWithIndex();&#10;// "Audio Source (2)"</code></pre> |

## Installation

In **Window → Package Manager**, choose **+ → Install package from git URL…** and paste:

```text
https://github.com/VPDPersonal/Aspid.FastTools.git#upm-preview/1.0.0-rc.8
```

This installs the preview version covered by this documentation. Git must be installed for UPM Git URLs.

<details>
<summary>Other installation options</summary>

- **Latest preview:** use `https://github.com/VPDPersonal/Aspid.FastTools.git#upm-preview`. Updating the package can bring in a newer preview.
- **Another version:** copy its UPM tag from [Releases](https://github.com/VPDPersonal/Aspid.FastTools/releases).
- **Unity Asset Store:** the package is not yet available in the store. For now, install it using the Git URL above.

> [!WARNING]
> The `upm` branch currently contains the older `com.aspid.fasttools` package (`1.0.0-rc.2`). Use the URL above for `tech.aspid.fasttools` and the features described here.

</details>

## Quick start

1. Install the package with the Git URL above.
2. In **Window → Package Manager**, select **Aspid.FastTools**, open the **Samples** tab and import a sample.
3. Open its scene and read the sample's README; the [samples overview](../Samples~/README.md) lists what each one shows.

## Documentation and samples

- [Samples overview](../Samples~/README.md) — scenes and editor tools for serialization, enum tables, profiling and editor UI.
- [API reference](https://vpdpersonal.github.io/Aspid.FastTools/api/Aspid.FastTools) — public types and members. The feature links above explain how to use them.
- [Claude Code plugin](10-claude-code-plugin.md) — optional skills for working with this package in Claude Code.
- [Changelog](https://github.com/VPDPersonal/Aspid.FastTools/blob/main/CHANGELOG.md) — release history.

## Help and support

Report bugs or ask questions in [GitHub Issues](https://github.com/VPDPersonal/Aspid.FastTools/issues). Include your Unity version, package version and steps to reproduce a problem.

Once the package is available on the [Unity Asset Store](https://assetstore.unity.com/packages/slug/365584), you can support development by purchasing it.

Distributed under the [MIT License](https://github.com/VPDPersonal/Aspid.FastTools/blob/main/LICENSE).

<img src="https://raw.githubusercontent.com/VPDPersonal/Aspid.FastTools/main/docs/images/aspid_fasttools_readme_banner.gif" alt="Aspid.FastTools" />

[![Unity 6.0+](Images/status-badge-unity.svg)](https://assetstore.unity.com/packages/slug/365584)
[![Preview 1.0.0-rc.8](Images/status-badge-preview.svg)](https://github.com/VPDPersonal/Aspid.FastTools/releases)
[![MIT License](Images/status-badge-license.svg)](https://github.com/VPDPersonal/Aspid.FastTools/blob/main/LICENSE)

Tools for Unity that reduce repetitive code: pick and repair `SerializeReference` implementations in the Inspector, serialize types and enum tables, generate profiler markers, and build editor UI with fluent UI Toolkit extensions.

[Documentation](https://vpdpersonal.github.io/Aspid.FastTools/docs) · [Source code](https://github.com/VPDPersonal/Aspid.FastTools) · [Releases](https://github.com/VPDPersonal/Aspid.FastTools/releases)

## Features

| Feature | What it gives you | Preview |
|---|---|---|
| [Serializable Type System](02-serializable-types.md) | Store and pick a `System.Type` in the Inspector | ![Select a serializable type in the Inspector](Images/aspid_fasttools_serializable_type.gif) |
| [SerializeReference Selector](03-serialize-reference-selector.md) | Choose implementations and repair broken references in place | ![Switch Pistol to Shotgun while keeping Damage at 37](Images/aspid_fasttools_serialize_reference_selector.gif) |
| [SerializeReference Tooling](04-serialize-reference-tooling.md) | Audit and repair references across the project; check them before builds and in CI | ![Repair a missing weapon type without losing its data](Images/aspid_fasttools_serialize_reference_tooling.gif) |
| [EnumValues](06-enum-values.md) | Edit enum → value tables in the Inspector, including flags | ![Edit enum keys and their values in the Inspector](../Samples~/EnumValues/Documentation/Images/surface-tables.png) |
| [ProfilerMarkers](05-profiler-markers.md) | Generate a unique profiler marker per call site with `this.Marker()` | <pre lang="csharp"><code>using (this.Marker())<br />&#123;<br />    Simulate();<br />&#125;</code></pre> |
| [VisualElement Extensions](07-visual-element-extensions.md) | Build UI Toolkit trees with fluent chains | <pre lang="csharp"><code>new VisualElement()<br />  .SetPadding(8)<br />  .AddChild(<br />    new Label("Stats"));</code></pre> |
| [SerializedProperty Extensions](08-serialized-property-extensions.md) | Set values, resize arrays, and inspect the field type and owning object | <pre lang="csharp"><code>property<br />  .Update()<br />  .SetIntAndApply(42);</code></pre> |
| [Editor Helpers](09-editor-helpers.md) | Get readable script and object names for custom editors | <pre lang="csharp"><code>audio.GetScriptName();<br />// "Audio Source"<br /><br />secondAudio<br />  .GetScriptNameWithIndex();<br />// "Audio Source (2)"</code></pre> |

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
- **Unity Asset Store:** the package is also [available there](https://assetstore.unity.com/packages/slug/365584); check the listed version before following these docs.

> [!WARNING]
> The `upm` branch currently contains the older `com.aspid.fasttools` package (`1.0.0-rc.2`). Use the URL above for `tech.aspid.fasttools` and the features described here.

</details>

## Quick start

Start with the [samples](../Samples~/README.md) to see the tools in action and explore their code.

## Documentation and samples

- [Samples overview](../Samples~/README.md) — scenes and editor tools for serialization, enum tables, profiling and editor UI.
- [API reference](https://vpdpersonal.github.io/Aspid.FastTools/api/Aspid.FastTools) — public types and members. The feature links above explain how to use them.
- [Claude Code plugin](10-claude-code-plugin.md) — optional skills for working with this package in Claude Code.
- [Changelog](https://github.com/VPDPersonal/Aspid.FastTools/blob/main/CHANGELOG.md) — release history.

## Help and support

Report bugs or ask questions in [GitHub Issues](https://github.com/VPDPersonal/Aspid.FastTools/issues). Include your Unity version, package version and steps to reproduce a problem.

Support development by purchasing the package on the [Unity Asset Store](https://assetstore.unity.com/packages/slug/365584).

## License

[MIT](https://github.com/VPDPersonal/Aspid.FastTools/blob/main/LICENSE).

<img src="https://raw.githubusercontent.com/VPDPersonal/Aspid.FastTools/main/docs/images/aspid_fasttools_readme_banner.gif" alt="Aspid.FastTools" />

<p>
  <a href="https://assetstore.unity.com/packages/slug/365584"><img src="https://img.shields.io/badge/Unity_6.0%2B-000000?style=flat&logo=unity&logoColor=white&color=4fa35d" alt="Unity 6.0+" /></a>
  <a href="https://github.com/VPDPersonal/Aspid.FastTools/releases"><img src="https://img.shields.io/github/package-json/v/VPDPersonal/Aspid.FastTools/upm?label=Stable&labelColor=254d2c&color=4fa35d" alt="Stable" /></a>
  <a href="https://github.com/VPDPersonal/Aspid.FastTools/releases"><img src="https://img.shields.io/github/package-json/v/VPDPersonal/Aspid.FastTools/upm-preview?label=Preview&labelColor=4d4425&color=a3923d" alt="Preview" /></a>
  <a href="LICENSE"><img src="https://img.shields.io/github/license/VPDPersonal/Aspid.FastTools?label=License&labelColor=254d2c&color=4fa35d" alt="License" /></a>
</p>

**Aspid.FastTools** is a Unity toolset that takes the boilerplate out of everyday work. Pick a `SerializeReference` implementation right in the Inspector and audit every such reference across the project. Let Roslyn source generators and analyzers write the repetitive code for you. Round it out with runtime and editor utilities: a serializable `System.Type`, fluent UI Toolkit extensions, and more.

---

### \[[Documentation](https://vpdpersonal.github.io/Aspid.FastTools/)\] \[[Unity Asset Store](https://assetstore.unity.com/packages/slug/365584)\] \[[Releases](https://github.com/VPDPersonal/Aspid.FastTools/releases)\] \[[Donate](#donate)\]

---

## Installation

Install via UPM: **Package Manager → + → Install package from git URL…**

| Channel | URL |
|---|---|
| Stable | `https://github.com/VPDPersonal/Aspid.FastTools.git#upm` |
| Specific version | `https://github.com/VPDPersonal/Aspid.FastTools.git#upm/<version>` |
| Preview | `https://github.com/VPDPersonal/Aspid.FastTools.git#upm-preview` |

## Features

| Feature | What it gives you |
|---|---|
| [Serializable Type System](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/02-serializable-types.md) | Store a `System.Type` in a serialized field and pick it from a searchable window in the Inspector |
| [SerializeReference Selector](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/03-serialize-reference-selector.md) | Pick a `[SerializeReference]` implementation from a dropdown in the Inspector, generic types included, and repair a broken reference in place |
| [SerializeReference Tooling](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/04-serialize-reference-tooling.md) | Find and repair every broken managed reference across the project, and fail the build or CI when one slips through |
| [ProfilerMarkers](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/05-profiler-markers.md) | A unique generated `ProfilerMarker` for every call site, one `this.Marker()` call away |
| [EnumValues](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/06-enum-values.md) | Serializable enum → value tables that handle `[Flags]` and never box |
| [VisualElement Extensions](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/07-visual-element-extensions.md) | Build UI Toolkit trees in code with fluent chains instead of nested blocks |
| [SerializedProperty Extensions](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/08-serialized-property-extensions.md) | Set `SerializedProperty` values in one typed, chainable call and reach the underlying field through reflection |
| [Editor Helpers](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/09-editor-helpers.md) | Readable display names for scripts and Unity objects in custom editors |
| [Claude Code Plugin](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/10-claude-code-plugin.md) | Skills that teach Claude Code this package |

Each feature ships with a sample that doubles as a tutorial — see the [samples overview](Aspid.FastTools/Packages/tech.aspid.fasttools/Samples~/README.md). Russian documentation: [Documentation/ru](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/ru/README.md).

## Donate

This project is developed on a voluntary basis. If you find it useful, you can support its development by purchasing the package on the [Unity Asset Store](https://assetstore.unity.com/packages/slug/365584) — that helps allocate more time to improving and maintaining **Aspid.FastTools**.

## License

**Aspid.FastTools** is distributed under the [MIT License](LICENSE). Release history lives in the [CHANGELOG](CHANGELOG.md).

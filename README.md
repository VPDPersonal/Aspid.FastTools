<img src="https://raw.githubusercontent.com/VPDPersonal/Aspid.FastTools/main/docs/images/aspid_fasttools_readme_banner.gif" alt="Aspid.FastTools" />

<p>
  <a href="https://assetstore.unity.com/packages/slug/365584"><img src="https://img.shields.io/badge/Unity_6.0%2B-000000?style=flat&logo=unity&logoColor=white&color=4fa35d" alt="Unity 6.0+" /></a>
  <a href="https://github.com/VPDPersonal/Aspid.FastTools/releases"><img src="https://img.shields.io/github/package-json/v/VPDPersonal/Aspid.FastTools/upm?label=Stable&labelColor=254d2c&color=4fa35d" alt="Stable" /></a>
  <a href="https://github.com/VPDPersonal/Aspid.FastTools/releases"><img src="https://img.shields.io/github/package-json/v/VPDPersonal/Aspid.FastTools/upm-preview?label=Preview&labelColor=4d4425&color=a3923d" alt="Preview" /></a>
  <a href="LICENSE"><img src="https://img.shields.io/github/license/VPDPersonal/Aspid.FastTools?label=License&labelColor=254d2c&color=4fa35d" alt="License" /></a>
</p>

**Aspid.FastTools** is a Unity toolset that eliminates routine boilerplate. Inside: a convenient `SerializeReference` workflow (an inspector type picker and a project-wide reference audit window), Roslyn source generators and analyzers, and runtime and editor utilities — from a serializable `System.Type` to fluent UI Toolkit extensions.

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

Prefer a manual install? Download the `.unitypackage` from [Releases](https://github.com/VPDPersonal/Aspid.FastTools/releases) or get the package on the [Unity Asset Store](https://assetstore.unity.com/packages/slug/365584). Migrating from `com.aspid.fasttools`? See [Getting Started](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/01-getting-started.md).

## Features

| Feature | What it gives you |
|---|---|
| [Serializable Type System](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/02-serializable-types.md) | `System.Type` as a serialized field, `[TypeSelector]`, a searchable type-picker window, `ComponentTypeSelector` |
| [SerializeReference Selector](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/03-serialize-reference-selector.md) | A type-picker dropdown for `[SerializeReference]` fields, nested inspectors, generics, per-field repair of broken references |
| [SerializeReference Tooling](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/04-serialize-reference-tooling.md) | Project-wide audit and bulk repair tabs, project settings, the build/CI gate |
| [ProfilerMarkers](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/05-profiler-markers.md) | Source-generated, per-call-site `ProfilerMarker`s via `this.Marker()` |
| [EnumValues](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/06-enum-values.md) | Serializable enum → value maps, `[Flags]`-aware, boxing-free |
| [VisualElement Extensions](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/07-visual-element-extensions.md) | Fluent UI Toolkit tree building in code |
| [SerializedProperty Extensions](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/08-serialized-property-extensions.md) | Chainable typed setters and reflection helpers |
| [Editor Helpers](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/09-editor-helpers.md) | Display names for scripts in custom editors |
| [Claude Code Plugin](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/10-claude-code-plugin.md) | Skills that teach Claude Code this package |

Each feature ships with a sample that doubles as a tutorial — see [Samples](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/01-getting-started.md#samples). Russian documentation: [Documentation/ru](Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/ru/README.md).

## Donate

This project is developed on a voluntary basis. If you find it useful, you can support its development by purchasing the package on the [Unity Asset Store](https://assetstore.unity.com/packages/slug/365584) — that helps allocate more time to improving and maintaining **Aspid.FastTools**.

## License

**Aspid.FastTools** is distributed under the [MIT License](LICENSE). Release history lives in the [CHANGELOG](CHANGELOG.md).

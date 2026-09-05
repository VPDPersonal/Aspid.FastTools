<img src="https://raw.githubusercontent.com/VPDPersonal/Aspid.FastTools/main/docs/images/aspid_fasttools_readme_banner.gif" alt="Aspid.FastTools" />

# Introduction

**Aspid.FastTools** is a Unity toolset that eliminates routine boilerplate. Inside: a convenient `SerializeReference` workflow (an inspector type picker and a project-wide reference audit window), Roslyn source generators and analyzers, and runtime and editor utilities — from a serializable `System.Type` to fluent UI Toolkit extensions.

[Source Code](https://github.com/VPDPersonal/Aspid.FastTools) · [Unity Asset Store](https://assetstore.unity.com/packages/slug/365584) · [Releases](https://github.com/VPDPersonal/Aspid.FastTools/releases)

## Getting started

[Installation](01-getting-started.md) — UPM git URL, `.unitypackage`, Asset Store, and the samples that ship with the package.

## Features

| Feature | What it gives you |
|---|---|
| [Serializable Type System](02-serializable-types.md) | `System.Type` as a serialized field, `[TypeSelector]`, a searchable type-picker window, `ComponentTypeSelector` |
| [SerializeReference Selector](03-serialize-reference-selector.md) | A type-picker dropdown for `[SerializeReference]` fields, nested inspectors, generics, per-field repair of broken references |
| [SerializeReference Tooling](04-serialize-reference-tooling.md) | Project-wide audit and bulk repair tabs, project settings, the build/CI gate |
| [ProfilerMarkers](05-profiler-markers.md) | Source-generated, per-call-site `ProfilerMarker`s via `this.Marker()` |
| [EnumValues](06-enum-values.md) | Serializable enum → value maps, `[Flags]`-aware, boxing-free |
| [ID System (Beta)](07-ids.md) | Asset-assignable names mapped to stable integer IDs |
| [VisualElement Extensions](08-visual-element-extensions.md) | Fluent UI Toolkit tree building in code |
| [SerializedProperty Extensions](09-serialized-property-extensions.md) | Chainable typed setters and reflection helpers |
| [IMGUI Layout Scopes](10-imgui-layout-scopes.md) | Disposable `Begin*`/`End*` wrappers with `Rect` access |
| [Editor Helpers](11-editor-helpers.md) | Display names for scripts in custom editors |
| [Claude Code Plugin](12-claude-code-plugin.md) | Skills that teach Claude Code this package |

## Donate

This project is developed on a voluntary basis. If you find it useful, you can support its development by purchasing the package on the [Unity Asset Store](https://assetstore.unity.com/packages/slug/365584) — that helps allocate more time to improving and maintaining **Aspid.FastTools**.

## License

**Aspid.FastTools** is distributed under the [MIT License](https://github.com/VPDPersonal/Aspid.FastTools/blob/main/LICENSE). Release history lives in the [CHANGELOG](https://github.com/VPDPersonal/Aspid.FastTools/blob/main/CHANGELOG.md).

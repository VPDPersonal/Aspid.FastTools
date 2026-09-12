<img src="https://raw.githubusercontent.com/VPDPersonal/Aspid.FastTools/main/docs/images/aspid_fasttools_readme_banner.gif" alt="Aspid.FastTools" />

# Introduction

**Aspid.FastTools** is a Unity toolset that takes the boilerplate out of everyday work. Pick a `SerializeReference` implementation right in the Inspector and audit every such reference across the project. Let Roslyn source generators and analyzers write the repetitive code for you. Round it out with runtime and editor utilities: a serializable `System.Type`, fluent UI Toolkit extensions, and more.

[Source Code](https://github.com/VPDPersonal/Aspid.FastTools) · [Unity Asset Store](https://assetstore.unity.com/packages/slug/365584) · [Releases](https://github.com/VPDPersonal/Aspid.FastTools/releases)

## Getting started

[Getting Started](01-getting-started.md) — install via UPM or the Asset Store, plus the samples for every feature.

## Features

| Feature | What it gives you |
|---|---|
| [Serializable Type System](02-serializable-types.md) | Store a `System.Type` in a serialized field and pick it from a searchable window in the Inspector |
| [SerializeReference Selector](03-serialize-reference-selector.md) | Pick a `[SerializeReference]` implementation from a dropdown in the Inspector, generic types included, and repair a broken reference in place |
| [SerializeReference Tooling](04-serialize-reference-tooling.md) | Find and repair every broken managed reference across the project, and fail the build or CI when one slips through |
| [ProfilerMarkers](05-profiler-markers.md) | A unique generated `ProfilerMarker` for every call site, one `this.Marker()` call away |
| [EnumValues](06-enum-values.md) | Serializable enum → value tables that handle `[Flags]` and never box |
| [VisualElement Extensions](07-visual-element-extensions.md) | Build UI Toolkit trees in code with fluent chains instead of nested blocks |
| [SerializedProperty Extensions](08-serialized-property-extensions.md) | Set `SerializedProperty` values in one typed, chainable call and reach the underlying field through reflection |
| [Editor Helpers](09-editor-helpers.md) | Readable display names for scripts and Unity objects in custom editors |
| [Claude Code Plugin](10-claude-code-plugin.md) | Skills that teach Claude Code this package |

## Donate

This project is developed on a voluntary basis. If you find it useful, you can support its development by purchasing the package on the [Unity Asset Store](https://assetstore.unity.com/packages/slug/365584) — that helps allocate more time to improving and maintaining **Aspid.FastTools**.

## License

**Aspid.FastTools** is distributed under the [MIT License](https://github.com/VPDPersonal/Aspid.FastTools/blob/main/LICENSE). Release history lives in the [CHANGELOG](https://github.com/VPDPersonal/Aspid.FastTools/blob/main/CHANGELOG.md).

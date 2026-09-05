# Aspid.FastTools Documentation

The complete guide to Aspid.FastTools for Unity. Rendered at https://vpdpersonal.github.io/Aspid.FastTools/. Russian version: [ru/](ru/README.md).

## Contents

1. [Getting Started](01-getting-started.md): installation, samples
2. [Serializable Type System](02-serializable-types.md): `SerializableType`, `[TypeSelector]`, `[TypeSelectorDisplay]`, `TypeSelectorWindow`, `ComponentTypeSelector`
3. [SerializeReference Selector](03-serialize-reference-selector.md): the Inspector dropdown for `[SerializeReference]`, repairing broken references
4. [SerializeReference Tooling](04-serialize-reference-tooling.md): bulk repair tabs, project settings, the build/CI gate
5. [ProfilerMarkers](05-profiler-markers.md): `this.Marker()` and the generated markers
6. [EnumValues](06-enum-values.md): `EnumValues<TValue>`, `EnumValues<TEnum, TValue>`
7. [ID System](07-ids.md): `IId`, `[UniqueId]`, `IdRegistry`
8. [VisualElement Extensions](08-visual-element-extensions.md): the fluent UI Toolkit API
9. [SerializedProperty Extensions](09-serialized-property-extensions.md): typed setters, arrays, references, reflection helpers
10. [IMGUI Layout Scopes](10-imgui-layout-scopes.md): `VerticalScope`, `HorizontalScope`, `ScrollViewScope`
11. [Editor Helpers](11-editor-helpers.md): `GetScriptName`, `GetScriptNameWithIndex`
12. [Claude Code Plugin](12-claude-code-plugin.md): the `aspid-fasttools` plugin

## Tutorials

Each sample's `README.md` is its tutorial: [Types](../Samples~/Types/README.md), [SerializeReferences](../Samples~/SerializeReferences/README.md), [EnumValues](../Samples~/EnumValues/README.md), [Ids](../Samples~/Ids/README.md), [ProfilerMarkers](../Samples~/ProfilerMarkers/README.md), [VisualElements](../Samples~/VisualElements/README.md).

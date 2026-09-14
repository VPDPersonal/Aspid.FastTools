# Aspid.FastTools Documentation

The complete guide to Aspid.FastTools for Unity. Rendered at https://vpdpersonal.github.io/Aspid.FastTools/. Russian version: [ru/](ru/README.md).

## Contents

Installation and samples are covered in the [introduction](README.md).

1. [Serializable Type System](02-serializable-types.md): `SerializableType`, `[TypeSelector]`, `[TypeSelectorDisplay]`, `TypeSelectorWindow`, `ComponentTypeSelector`
2. [SerializeReference Selector](03-serialize-reference-selector.md): the Inspector dropdown for `[SerializeReference]`, repairing broken references
3. [SerializeReference Tooling](04-serialize-reference-tooling.md): bulk repair tabs, project settings, the build/CI gate
4. [ProfilerMarkers](05-profiler-markers.md): `this.Marker()` and the generated markers
5. [EnumValues](06-enum-values.md): `EnumValues<TValue>`, `EnumValues<TEnum, TValue>`
6. [VisualElement Extensions](07-visual-element-extensions.md): the fluent UI Toolkit API
7. [SerializedProperty Extensions](08-serialized-property-extensions.md): typed setters, arrays, references, reflection helpers
8. [Editor Helpers](09-editor-helpers.md): `GetDisplayName`, `GetDisplayNameWithIndex`
9. [Claude Code Plugin](10-claude-code-plugin.md): the `aspid-fasttools` plugin

## Tutorials

Each sample's `Documentation/README.md` is its tutorial: [Types](../Samples~/Types/Documentation/README.md), [SerializeReferences](../Samples~/SerializeReferences/Documentation/README.md), [EnumValues](../Samples~/EnumValues/Documentation/README.md), [ProfilerMarkers](../Samples~/ProfilerMarkers/Documentation/README.md), [EditorTools](../Samples~/EditorTools/Documentation/README.md).

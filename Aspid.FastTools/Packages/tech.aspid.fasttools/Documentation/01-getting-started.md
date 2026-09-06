# Getting Started

## Installation

Install Aspid.FastTools via UPM: in the Package Manager click **+ → Install package from git URL…** and paste one of the URLs below.

> [!NOTE]
> **Migrating from `com.aspid.fasttools`:** the package was renamed to `tech.aspid.fasttools` in May 2026. Unity treats it as a different package, so installs of the old id never receive updates — remove the `com.aspid.fasttools` entry from `Packages/manifest.json` and install `tech.aspid.fasttools` via one of the URLs below.

### Stable

The `upm` branch always points to the latest **stable** release:

```
https://github.com/VPDPersonal/Aspid.FastTools.git#upm
```

To install a specific version, target the immutable per-release tag `upm/<version>` — e.g. `upm/1.0.0` once the 1.0.0 release is out (see [Releases](https://github.com/VPDPersonal/Aspid.FastTools/releases) for the list of available versions):

```
https://github.com/VPDPersonal/Aspid.FastTools.git#upm/<version>
```

Prefer a manual install? Download the `.unitypackage` from the [Releases](https://github.com/VPDPersonal/Aspid.FastTools/releases) page, or get the package from the [Unity Asset Store](https://assetstore.unity.com/packages/slug/365584).

### Preview

The `upm-preview` branch always points to the latest **preview** release (rc, beta, alpha, …):

```
https://github.com/VPDPersonal/Aspid.FastTools.git#upm-preview
```

Specific preview versions use the same per-release tag scheme:

```
https://github.com/VPDPersonal/Aspid.FastTools.git#upm-preview/1.0.0-rc.7
```

## Samples

Each feature ships with a sample: a small scene or editor tool that does something visible with the feature, plus a `README.md` that walks through what to try and where to look in the code. Import them from the Package Manager (**Aspid.FastTools → Samples**) or open the **Welcome** tab (`Tools → Aspid 🐍 → FastTools → Welcome`).

| Sample | What it shows |
|---|---|
| [Types](../Samples~/Types/README.md) | An enemy spawner: `SerializableMonoScript<T>`, `SerializableType<T>`, `[TypeSelectorDisplay]`, a member-referenced `[TypeSelector]`, `ComponentTypeSelector` |
| [SerializeReferences](../Samples~/SerializeReferences/README.md) | A turret with polymorphic weapons: the `[SerializeReference]` picker in every field shape, broken assets for the repair tools, an IMGUI inspector |
| [EnumValues](../Samples~/EnumValues/README.md) | A walker over surface tiles: both `EnumValues` variants, default values, `[Flags]` lookup rules |
| [ProfilerMarkers](../Samples~/ProfilerMarkers/README.md) | A flock simulation: the generated marker tree in the Profiler |
| [EditorTools](../Samples~/EditorTools/README.md) | An editor window and inspector: fluent `VisualElement` extensions, `SerializedProperty` setters, editor helpers, `TypeSelectorWindow` |

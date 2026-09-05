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

Each feature ships with a sample that doubles as a tutorial. Import them from the Package Manager (**Aspid.FastTools → Samples**) or open the **Welcome** tab (`Tools → Aspid 🐍 → FastTools → Welcome`).

| Sample | What it shows |
|---|---|
| [Types](../Samples~/Types/README.md) | `SerializableType<T>`, `[TypeSelector]`, `ComponentTypeSelector` in a tiny ability system |
| [SerializeReferences](../Samples~/SerializeReferences/README.md) | The `[SerializeReference]` picker: single fields, lists, narrowing, nesting, generics, `Required` |
| [EnumValues](../Samples~/EnumValues/README.md) | Enum-keyed maps with `[Flags]` handling in UI Toolkit and IMGUI inspectors |
| [Ids](../Samples~/Ids/README.md) | `IId` structs, `[UniqueId]` and `IdRegistry` assets |
| [ProfilerMarkers](../Samples~/ProfilerMarkers/README.md) | `this.Marker()` and the generated markers in the Profiler |
| [VisualElements](../Samples~/VisualElements/README.md) | A custom inspector built with the fluent `VisualElement` extensions |

# Agent Skills

Skills that make a coding agent write code with Aspid.FastTools instead of the raw Unity API.

## Quick start

[Install Aspid.FastTools](README.md#installation) in your Unity project, then ask your agent:

```text prompt
Install the skills from VPDPersonal/Aspid.FastTools in the current project.
```

Or run in the project root:

```bash
npx skills add VPDPersonal/Aspid.FastTools
```

```text prompt
Profile Simulate and the neighbor search
```

| Without skills | With skills |
|---|---|
| <pre lang="csharp"><code>private static readonly&#10;    ProfilerMarker _simulate =&#10;    new("Flock.Simulate");&#10;private static readonly&#10;    ProfilerMarker _neighbors =&#10;    new("Flock.FindNeighbors");&#10;&#10;public void Simulate()&#10;&#123;&#10;    using var _ = _simulate.Auto();&#10;    using (_neighbors.Auto())&#10;        FindNeighbors();&#10;    Integrate();&#10;&#125;</code></pre> | <pre lang="csharp"><code>public void Simulate()&#10;&#123;&#10;    using var _ = this.Marker();&#10;    using (this.Marker()&#10;        .WithName("Neighbors"))&#10;        FindNeighbors();&#10;    Integrate();&#10;&#125;</code></pre> |

## Versions and updates

Without a tag, `npx skills add` takes the skills from the `main` branch, which can already describe API your package version does not have. To match the installed package, add them from its release tag, and run `add` again with the new tag after a package upgrade:

```bash
npx skills add VPDPersonal/Aspid.FastTools#v<version>
```

Releases up to and including `1.0.0-rc.8` do not ship the skills; with them, install from `main`.

To update the skills, repeat the installation. `npx skills update` updates every skill in the project, not only these.

## Skills

Skills activate automatically for matching requests.

### aspid-profiler-marker

Profile a method or a section with <code lang="csharp">this.Marker()</code>. Guide: [ProfilerMarkers](09-profiler-markers.md).

![The aspid-profiler-marker skill adds markers to Simulate](Images/agent-skills-profiler-marker.svg)

### aspid-visual-element-fluent

Build and style UI Toolkit elements in C#. Guide: [VisualElement Extensions](10-visual-element-extensions.md).

![The aspid-visual-element-fluent skill builds the inspector header in one chain](Images/agent-skills-visual-element-fluent.svg)

### aspid-serializable-type

Store a <code lang="class-name">System.Type</code> and pick types in the Inspector. Guides: [Serializable Types](02-serializable-types.md), [TypeSelector](03-type-selector.md), [SerializeReference Selector](04-serialize-reference-selector.md), [ComponentTypeSelector](05-component-type-selector.md).

![The aspid-serializable-type skill adds a weapon type picker field](Images/agent-skills-serializable-type.svg)

### aspid-enum-values

Map enum members to values. Guide: [EnumValues](08-enum-values.md).

![The aspid-enum-values skill replaces the damage multipliers with an EnumValues table](Images/agent-skills-enum-values.svg)

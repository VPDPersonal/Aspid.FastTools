# Agent Skills

Skills that make a coding agent write code with Aspid.FastTools instead of the raw Unity API.

## Quick start

[Install Aspid.FastTools](README.md#installation) in your Unity project. The skills installer needs Node.js 22.20 or newer. Then ask your agent:

```text prompt
Install the skills from VPDPersonal/Aspid.FastTools in the current project, not globally. Run npx skills add VPDPersonal/Aspid.FastTools#v<version>, where <version> is the version of the installed Aspid.FastTools package.
```

Or run in the project root:

```bash
npx skills add VPDPersonal/Aspid.FastTools#v<version>
```

Replace `<version>` with the version of the installed package: `1.0.0` becomes `#v1.0.0`. In the installer, choose the **Project** scope: a global install applies to every Unity project, whatever its package version.

For `1.0.0-rc.8` and older, see [Versions and updates](#versions-and-updates).

```text prompt
Profile Simulate and the neighbor search
```

| Without skills | With skills |
|---|---|
| <pre lang="csharp"><code>private static readonly&#10;    ProfilerMarker _simulate =&#10;    new("Flock.Simulate");&#10;private static readonly&#10;    ProfilerMarker _neighbors =&#10;    new("Flock.FindNeighbors");&#10;&#10;public void Simulate()&#10;&#123;&#10;    using var _ = _simulate.Auto();&#10;    using (_neighbors.Auto())&#10;        FindNeighbors();&#10;    Integrate();&#10;&#125;</code></pre> | <pre lang="csharp"><code>public void Simulate()&#10;&#123;&#10;    using var _ = this.Marker();&#10;    using (this.Marker()&#10;        .WithName("Neighbors"))&#10;        FindNeighbors();&#10;    Integrate();&#10;&#125;</code></pre> |

## Versions and updates

Skills from the `main` branch can describe API that your package version does not have, so the agent writes code that does not compile. A release tag holds the skills that match that release. After a package upgrade, repeat the installation with the new tag.

`npx skills update` does not move skills installed from a tag: it fetches the same tag again and changes nothing. It refreshes only skills installed without a tag, from `main`, and it updates every skill in the project, not only these.

Releases up to and including `1.0.0-rc.8` do not ship the skills. With one of them, upgrade the package to a release that does, or install the package from `main` together with the skills, so both match. In **Window → Package Manager**, choose **+ → Install package from git URL…** and paste this URL:

```text
https://github.com/VPDPersonal/Aspid.FastTools.git?path=/Aspid.FastTools/Packages/tech.aspid.fasttools#main
```

Then install the skills without a tag:

```bash
npx skills add VPDPersonal/Aspid.FastTools
```

The skills used to ship as the `aspid-fasttools` Claude Code plugin. If you have it, remove it with `/plugin marketplace remove aspid-claude-plugins`: it teaches the agent API that no longer exists.

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

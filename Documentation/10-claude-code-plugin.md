# Claude Code Plugin

`aspid-fasttools` adds skills to [Claude Code](https://docs.claude.com/en/docs/claude-code) for profiling methods and building UI with the package’s fluent `VisualElement` extensions.

## Quick start

[Install Aspid.FastTools](README.md#installation) in your Unity project and open the project in Claude Code. Add the marketplace, then install the plugin in the Claude Code session:

```text
/plugin marketplace add VPDPersonal/Aspid.Claude.Plugins
```

```text
/plugin install aspid-fasttools@aspid-claude-plugins
```

The plugin is installed separately from the Unity package. Open `/plugin` to check that `aspid-fasttools` is installed.

## Skills

Skills activate automatically for matching requests. These two cover features documented in this package:

| Skill | Task | API guide |
|---|---|---|
| `aspid-profiler-marker` | Add method and block scopes with `this.Marker()` | [ProfilerMarkers](05-profiler-markers.md) |
| `aspid-visual-element-fluent` | Build and style UI Toolkit elements in C# | [VisualElement Extensions](07-visual-element-extensions.md) |

For example, select a method and ask:

```text
Add a marker for the entire Simulate method and a separate
named marker for the neighbour search.
Use this.Marker() from Aspid.FastTools.
```

Check compilation and inspect the markers in Unity Profiler after applying the changes.

## Compatibility

> [!IMPORTANT]
> The plugin is in alpha. Its [documentation](https://github.com/VPDPersonal/Aspid.Claude.Plugins/blob/main/plugins/aspid-fasttools/README.md) targets the earlier `com.aspid.fasttools` package; these guides describe `tech.aspid.fasttools`. Check suggested code against the installed package’s API.

The plugin also includes `aspid-id-struct` for the earlier package’s `IId` and `[UniqueId]` APIs, which are outside this documentation. The plugin is released independently; see [releases and updates](https://github.com/VPDPersonal/Aspid.Claude.Plugins/releases).

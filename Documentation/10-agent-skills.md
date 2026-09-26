# Agent Skills

A coding agent does not know the Aspid.FastTools API, so it writes a manual `ProfilerMarker`, a run of `style` assignments or an enum `switch` where the package has a shorter answer. The package skills teach it that API: one command installs them into Claude Code, Codex, Cursor, GitHub Copilot, Gemini CLI and other agents, and the agent uses `this.Marker()`, the fluent `VisualElement` extensions, `SerializableType` and `EnumValues` where they fit.

## Quick start

[Install Aspid.FastTools](README.md#installation) in your Unity project, then run in the project root:

```bash
npx skills add VPDPersonal/Aspid.FastTools
```

The skills land in the agents' folders inside the project, such as `.claude/skills` or `.agents/skills`. Commit those folders and the whole team gets the skills. `-y` installs without prompts, `-a claude-code` limits the install to one agent, `-s <skill>` to one skill, and `--list` shows the skills without installing them.

To pull newer versions of the skills later:

```bash
npx skills update
```

Both commands take the skills from the `main` branch, which can already describe API your package version does not have. To match the skills to the installed package, add them from its release tag; `npx skills update` then stays on that tag, and after a package upgrade you run `add` again with the new tag:

```bash
npx skills add VPDPersonal/Aspid.FastTools#v<version>
```

Releases up to and including `1.0.0-rc.8` do not ship these skills; with them, install from `main`, which may describe API your version does not have.

## Skills

Skills activate automatically for matching requests:

| Skill | Task | API guide |
|---|---|---|
| `aspid-profiler-marker` | Profile a method or a section with `this.Marker()` | [ProfilerMarkers](05-profiler-markers.md) |
| `aspid-visual-element-fluent` | Build and style UI Toolkit elements in C# | [VisualElement Extensions](07-visual-element-extensions.md) |
| `aspid-serializable-type` | Store a `System.Type` and pick types in the Inspector | [Serializable Type System](02-serializable-types.md), [SerializeReference Selector](03-serialize-reference-selector.md), [ComponentTypeSelector](11-component-type-selector.md) |
| `aspid-enum-values` | Map enum members to values | [EnumValues](06-enum-values.md) |

For example, select a method and ask:

```text
Add a marker for the entire Simulate method and a separate
named marker for the neighbour search.
```

Check compilation and inspect the markers in Unity Profiler after applying the changes.

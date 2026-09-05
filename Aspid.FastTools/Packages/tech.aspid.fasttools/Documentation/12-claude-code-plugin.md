# Claude Code Plugin

If you use [Claude Code](https://docs.claude.com/en/docs/claude-code), the companion [Aspid.Claude.Plugins](https://github.com/VPDPersonal/Aspid.Claude.Plugins) marketplace ships the `aspid-fasttools` plugin — a set of skills that teach Claude Code this package's conventions and APIs.

> [!WARNING]
> The plugin is still in beta — its skills and commands may change between releases.

Add the marketplace and install the plugin:

```sh
/plugin marketplace add VPDPersonal/Aspid.Claude.Plugins
```

```sh
/plugin install aspid-fasttools@aspid-claude-plugins
```

Included skills:

- **`aspid-id-struct`** — scaffold a new `IId` struct and `[UniqueId]` fields for the [ID System](07-ids.md).
- **`aspid-profiler-marker`** — insert `this.Marker()` call sites with the right `using`/scope shape.
- **`aspid-visual-element-fluent`** — build editor or runtime UI using the fluent [`VisualElement` extensions](08-visual-element-extensions.md).

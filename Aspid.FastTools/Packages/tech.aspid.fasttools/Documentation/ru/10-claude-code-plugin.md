# Claude Code Plugin

Если вы используете [Claude Code](https://docs.claude.com/en/docs/claude-code), сопутствующий маркетплейс [Aspid.Claude.Plugins](https://github.com/VPDPersonal/Aspid.Claude.Plugins) поставляет плагин `aspid-fasttools` — набор скиллов, которые обучают Claude Code конвенциям и API этого пакета.

> [!WARNING]
> Плагин всё ещё находится в бета-версии — его скиллы и команды могут меняться между релизами.

Добавьте маркетплейс и установите плагин:

```sh
/plugin marketplace add VPDPersonal/Aspid.Claude.Plugins
```

```sh
/plugin install aspid-fasttools@aspid-claude-plugins
```

Включённые скиллы:

- **`aspid-profiler-marker`** — вставляет вызовы `this.Marker()` с правильной формой `using`/scope.
- **`aspid-visual-element-fluent`** — собирает editor- или runtime-UI через fluent-[расширения `VisualElement`](07-visual-element-extensions.md).

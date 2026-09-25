# Agent Skills

Coding-агент не знает API Aspid.FastTools, поэтому пишет ручной `ProfilerMarker`, серию присваиваний `style` или `switch` по enum там, где у пакета есть решение короче. Скиллы пакета учат агента этому API: одна команда ставит их в Claude Code, Codex, Cursor, GitHub Copilot, Gemini CLI и другие агенты, и агент использует `this.Marker()`, fluent-расширения `VisualElement`, `SerializableType` и `EnumValues` там, где они подходят.

## Быстрый старт

[Установите Aspid.FastTools](README.md#установка) в Unity-проект и выполните в корне проекта:

```bash
npx skills add VPDPersonal/Aspid.FastTools
```

Скиллы попадают в папки агентов внутри проекта, например `.claude/skills` или `.agents/skills`. Закоммитьте эти папки, и скиллы получит вся команда. `-y` ставит без вопросов, `-a claude-code` ограничивает установку одним агентом, `-s <скилл>` — одним скиллом, а `--list` показывает скиллы без установки.

Чтобы потом подтянуть новые версии скиллов:

```bash
npx skills update
```

## Скиллы

Скиллы активируются автоматически при подходящих запросах:

| Скилл | Задача | Руководство по API |
|---|---|---|
| `aspid-profiler-marker` | Замер метода или участка через `this.Marker()` | [ProfilerMarkers](05-profiler-markers.md) |
| `aspid-visual-element-fluent` | Построение и оформление элементов UI Toolkit в C# | [VisualElement Extensions](07-visual-element-extensions.md) |
| `aspid-serializable-type` | Хранение `System.Type` и выбор типов в инспекторе | [Serializable Type System](02-serializable-types.md), [SerializeReference Selector](03-serialize-reference-selector.md), [ComponentTypeSelector](11-component-type-selector.md) |
| `aspid-enum-values` | Сопоставление значений членам enum | [EnumValues](06-enum-values.md) |

Например, выделите метод и попросите:

```text
Добавь маркер на весь метод Simulate и отдельный
именованный маркер на поиск соседей.
```

После применения изменений проверьте компиляцию и маркеры в Unity Profiler.

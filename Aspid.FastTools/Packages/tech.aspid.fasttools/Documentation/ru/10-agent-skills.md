# Agent Skills

Скиллы, с которыми coding-агент пишет код на Aspid.FastTools, а не на голом Unity API.

## Быстрый старт

[Установите Aspid.FastTools](README.md#установка) в Unity-проект и выполните в корне проекта:

```bash
npx skills add VPDPersonal/Aspid.FastTools
```

```text prompt
Замерь Simulate и отдельно поиск соседей
```

| Без скиллов | Со скиллами |
|---|---|
| <pre lang="csharp"><code>private static readonly&#10;    ProfilerMarker _simulate =&#10;    new("Flock.Simulate");&#10;private static readonly&#10;    ProfilerMarker _neighbors =&#10;    new("Flock.FindNeighbors");&#10;&#10;public void Simulate()&#10;&#123;&#10;    using var _ = _simulate.Auto();&#10;    using (_neighbors.Auto())&#10;        FindNeighbors();&#10;    Integrate();&#10;&#125;</code></pre> | <pre lang="csharp"><code>public void Simulate()&#10;&#123;&#10;    using var _ = this.Marker();&#10;    using (this.Marker()&#10;        .WithName("Neighbors"))&#10;        FindNeighbors();&#10;    Integrate();&#10;&#125;</code></pre> |

## Установка и обновление

Одна команда ставит скиллы в Claude Code, Codex, Cursor, GitHub Copilot, Gemini CLI и другие агенты — в их папки внутри проекта (`.claude/skills`, `.agents/skills`). Закоммитьте эти папки, и скиллы получит вся команда.

| Флаг | Что делает |
|---|---|
| `-a claude-code` | ставит только в одного агента |
| `-s aspid-profiler-marker` | ставит один скилл |
| `-y` | ставит без вопросов |
| `--list` | показывает скиллы без установки |

Чтобы обновить скиллы пакета, повторите установку:

```bash
npx skills add VPDPersonal/Aspid.FastTools -y
```

`npx skills update` обновит все скиллы проекта, не только эти.

## Скиллы

Скиллы активируются автоматически при подходящих запросах.

### aspid-profiler-marker

Замер метода или участка через <code lang="csharp">this.Marker()</code>. Руководство: [ProfilerMarkers](05-profiler-markers.md).

![Скилл aspid-profiler-marker добавляет маркеры в Simulate](../Images/agent-skills-profiler-marker.svg)

### aspid-visual-element-fluent

Построение и оформление элементов UI Toolkit в C#. Руководство: [VisualElement Extensions](07-visual-element-extensions.md).

![Скилл aspid-visual-element-fluent собирает шапку инспектора одной цепочкой](../Images/agent-skills-visual-element-fluent.svg)

### aspid-serializable-type

Хранение <code lang="class-name">System.Type</code> и выбор типов в инспекторе. Руководства: [Serializable Type System](02-serializable-types.md), [SerializeReference Selector](03-serialize-reference-selector.md), [ComponentTypeSelector](11-component-type-selector.md).

![Скилл aspid-serializable-type добавляет поле выбора типа оружия](../Images/agent-skills-serializable-type.svg)

### aspid-enum-values

Сопоставление значений членам enum. Руководство: [EnumValues](06-enum-values.md).

![Скилл aspid-enum-values заменяет множители урона таблицей EnumValues](../Images/agent-skills-enum-values.svg)

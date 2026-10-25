# Agent Skills

Скиллы, с которыми coding-агент пишет код на Aspid.FastTools, а не на голом Unity API.

## Быстрый старт

[Установите Aspid.FastTools](README.md#установка) в Unity-проект. Установщику скиллов нужен Node.js 22.20 или новее.

> [!NOTE]
> Релизы до `1.0.0-rc.8` включительно не содержат скиллов. С таким релизом сначала см. [Версии и обновление](#версии-и-обновление).

Попросите агента:

```text prompt
Установи скиллы из репозитория VPDPersonal/Aspid.FastTools в текущий проект,
не глобально. Выполни npx skills add VPDPersonal/Aspid.FastTools#skills-preview.
```

Или выполните в корне проекта:

```bash
npx skills add VPDPersonal/Aspid.FastTools#skills-preview
```

В установщике выберите область **Project**: глобальная установка действует во всех Unity-проектах, какая бы версия пакета в них ни стояла.

```text prompt
Замерь Simulate и отдельно поиск соседей
```

| Без скиллов | Со скиллами |
|---|---|
| <pre lang="csharp"><code>private static readonly&#10;    ProfilerMarker _simulate =&#10;    new("Flock.Simulate");&#10;private static readonly&#10;    ProfilerMarker _neighbors =&#10;    new("Flock.FindNeighbors");&#10;&#10;public void Simulate()&#10;&#123;&#10;    using var _ = _simulate.Auto();&#10;    using (_neighbors.Auto())&#10;        FindNeighbors();&#10;    Integrate();&#10;&#125;</code></pre> | <pre lang="csharp"><code>public void Simulate()&#10;&#123;&#10;    using var _ = this.Marker();&#10;    using (this.Marker()&#10;        .WithName("Neighbors"))&#10;        FindNeighbors();&#10;    Integrate();&#10;&#125;</code></pre> |

## Версии и обновление

Скиллы приходят из веток релизов, а не из `main`. У каждого канала пакета своя ветка: в `skills-preview` попадает каждый preview-релиз, как в `upm-preview`, а в `skills` — каждый стабильный, как в `upm`. Ставьте скиллы из канала вашего пакета. `npx skills update` переводит их на последний релиз этого канала и обновляет все скиллы проекта, не только эти.

Обновляйте пакет одновременно со скиллами: скиллы новее пакета могут описывать API, которого в нём нет, и тогда агент пишет код, который не собирается. Чтобы скиллы оставались на версии вашего пакета, поставьте их с её тега — `npx skills update` его не двигает:

```bash
npx skills add VPDPersonal/Aspid.FastTools#skills-preview/<version>
```

Тег лежит на канале вашего пакета: `skills/<version>` для стабильного релиза, `skills-preview/<version>` для preview.

Релизы до `1.0.0-rc.8` включительно не содержат скиллов. С ними сначала обновите пакет до релиза со скиллами.

Раньше скиллы поставлялись плагином `aspid-fasttools` для Claude Code. Если он у вас стоит, удалите его командой `/plugin uninstall aspid-fasttools@aspid-claude-plugins`: он учит агента API, которого больше нет.

## Скиллы

Скиллы активируются автоматически при подходящих запросах.

### aspid-profiler-marker

Замер метода или участка через <code lang="csharp">this.Marker()</code>. Руководство: [ProfilerMarkers](09-profiler-markers.md).

![Скилл aspid-profiler-marker добавляет маркеры в Simulate](../../../../docs/Images/agent-skills-profiler-marker.svg)

### aspid-visual-element-fluent

Построение и оформление элементов UI Toolkit в C#. Руководство: [VisualElement Extensions](10-visual-element-extensions.md).

![Скилл aspid-visual-element-fluent собирает шапку инспектора одной цепочкой](../../../../docs/Images/agent-skills-visual-element-fluent.svg)

### aspid-serializable-type

Хранение <code lang="class-name">System.Type</code>, выбор типов в инспекторе и провал CI-задачи при потерянных типах. Руководства: [Serializable Types](02-serializable-types.md), [TypeSelector](03-type-selector.md), [SerializeReference Selector](04-serialize-reference-selector.md), [ComponentTypeSelector](05-component-type-selector.md), [Проверка перед сборкой и CI](07-serialize-reference-validation.md).

![Скилл aspid-serializable-type добавляет поле выбора типа оружия](../../../../docs/Images/agent-skills-serializable-type.svg)

### aspid-enum-values

Сопоставление значений членам enum. Руководство: [EnumValues](08-enum-values.md).

![Скилл aspid-enum-values заменяет множители урона таблицей EnumValues](../../../../docs/Images/agent-skills-enum-values.svg)

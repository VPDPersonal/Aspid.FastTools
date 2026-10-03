# Agent Skills

Скиллы, с которыми coding-агент пишет код на Aspid.FastTools, а не на голом Unity API.

## Быстрый старт

[Установите Aspid.FastTools](README.md#установка) в Unity-проект и попросите агента:

```text prompt
Установи скиллы из репозитория VPDPersonal/Aspid.FastTools в текущий проект.
```

Или выполните в корне проекта:

```bash
npx skills add VPDPersonal/Aspid.FastTools
```

```text prompt
Замерь Simulate и отдельно поиск соседей
```

| Без скиллов | Со скиллами |
|---|---|
| <pre lang="csharp"><code>private static readonly&#10;    ProfilerMarker _simulate =&#10;    new("Flock.Simulate");&#10;private static readonly&#10;    ProfilerMarker _neighbors =&#10;    new("Flock.FindNeighbors");&#10;&#10;public void Simulate()&#10;&#123;&#10;    using var _ = _simulate.Auto();&#10;    using (_neighbors.Auto())&#10;        FindNeighbors();&#10;    Integrate();&#10;&#125;</code></pre> | <pre lang="csharp"><code>public void Simulate()&#10;&#123;&#10;    using var _ = this.Marker();&#10;    using (this.Marker()&#10;        .WithName("Neighbors"))&#10;        FindNeighbors();&#10;    Integrate();&#10;&#125;</code></pre> |

## Версии и обновление

Без тега `npx skills add` берёт скиллы из ветки `main`, где они могут описывать API, которого ещё нет в вашей версии пакета. Чтобы скиллы совпадали с установленным пакетом, ставьте их с тега его релиза, а после обновления пакета снова выполните `add` с новым тегом:

```bash
npx skills add VPDPersonal/Aspid.FastTools#v<version>
```

Релизы до `1.0.0-rc.8` включительно не содержат скиллов; с ними ставьте скиллы из `main`.

Для обновления повторите установку. `npx skills update` обновит все скиллы проекта, не только эти.

## Скиллы

Скиллы активируются автоматически при подходящих запросах.

### aspid-profiler-marker

Замер метода или участка через <code lang="csharp">this.Marker()</code>. Руководство: [ProfilerMarkers](09-profiler-markers.md).

![Скилл aspid-profiler-marker добавляет маркеры в Simulate](../Images/agent-skills-profiler-marker.svg)

### aspid-visual-element-fluent

Построение и оформление элементов UI Toolkit в C#. Руководство: [VisualElement Extensions](10-visual-element-extensions.md).

![Скилл aspid-visual-element-fluent собирает шапку инспектора одной цепочкой](../Images/agent-skills-visual-element-fluent.svg)

### aspid-serializable-type

Хранение <code lang="class-name">System.Type</code> и выбор типов в инспекторе. Руководства: [Serializable Types](02-serializable-types.md), [TypeSelector](03-type-selector.md), [SerializeReference Selector](04-serialize-reference-selector.md), [ComponentTypeSelector](05-component-type-selector.md).

![Скилл aspid-serializable-type добавляет поле выбора типа оружия](../Images/agent-skills-serializable-type.svg)

### aspid-enum-values

Сопоставление значений членам enum. Руководство: [EnumValues](08-enum-values.md).

![Скилл aspid-enum-values заменяет множители урона таблицей EnumValues](../Images/agent-skills-enum-values.svg)

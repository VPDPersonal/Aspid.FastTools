# Claude Code Plugin

`aspid-fasttools` добавляет в [Claude Code](https://docs.claude.com/en/docs/claude-code) скиллы для профилирования методов и построения UI через fluent-расширения `VisualElement` из пакета.

## Быстрый старт

[Установите Aspid.FastTools](README.md#установка) в Unity-проект и откройте проект в Claude Code. В сессии Claude Code добавьте маркетплейс, затем установите плагин:

```text
/plugin marketplace add VPDPersonal/Aspid.Claude.Plugins
```

```text
/plugin install aspid-fasttools@aspid-claude-plugins
```

Плагин устанавливается отдельно от Unity-пакета. Откройте `/plugin`, чтобы проверить наличие `aspid-fasttools` среди установленных плагинов.

## Скиллы

Скиллы активируются автоматически при подходящих запросах. Эти два покрывают возможности, описанные в документации пакета:

| Скилл | Задача | Руководство по API |
|---|---|---|
| `aspid-profiler-marker` | Добавление областей замера методов и блоков через `this.Marker()` | [ProfilerMarkers](05-profiler-markers.md) |
| `aspid-visual-element-fluent` | Построение и оформление элементов UI Toolkit в C# | [VisualElement Extensions](07-visual-element-extensions.md) |

Например, выделите метод и попросите:

```text
Добавь маркер на весь метод Simulate и отдельный
именованный маркер на поиск соседей.
Используй this.Marker() из Aspid.FastTools.
```

После применения изменений проверьте компиляцию и маркеры в Unity Profiler.

## Совместимость

> [!IMPORTANT]
> Плагин находится в alpha. Его [документация](https://github.com/VPDPersonal/Aspid.Claude.Plugins/blob/main/plugins/aspid-fasttools/README_RU.md) ориентирована на прежний пакет `com.aspid.fasttools`, а эти руководства — на `tech.aspid.fasttools`. Сверяйте предлагаемый код с API установленного пакета.

Плагин также содержит `aspid-id-struct` для API `IId` и `[UniqueId]` прежнего пакета, которые не входят в эту документацию. Плагин выпускается независимо; см. [релизы и обновления](https://github.com/VPDPersonal/Aspid.Claude.Plugins/releases).

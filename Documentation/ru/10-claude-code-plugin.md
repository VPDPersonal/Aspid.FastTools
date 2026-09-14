# Claude Code Plugin

Плагин `aspid-fasttools` добавляет в Claude Code скиллы с инструкциями по API пакета: как расставлять маркеры профилирования и собирать интерфейсы через fluent-расширения `VisualElement`. Он устанавливается в Claude Code отдельно от Unity-пакета.

> [!IMPORTANT]
> В [манифесте плагина](https://github.com/VPDPersonal/Aspid.Claude.Plugins/blob/main/plugins/aspid-fasttools/.claude-plugin/plugin.json) сейчас указана версия **0.2.0-alpha**. Его документация ориентирована на прежний `com.aspid.fasttools`, а эти руководства — на `tech.aspid.fasttools`. Перед использованием сверяйте предлагаемый код с API установленной версии пакета.

## Установка

Сначала [установите Aspid.FastTools в Unity-проект](README.md#установка) и откройте проект в [Claude Code](https://docs.claude.com/en/docs/claude-code).

В интерактивной сессии Claude Code добавьте маркетплейс:

```text
/plugin marketplace add VPDPersonal/Aspid.Claude.Plugins
```

Затем установите плагин:

```text
/plugin install aspid-fasttools@aspid-claude-plugins
```

Откройте `/plugin`, чтобы проверить наличие `aspid-fasttools` среди установленных плагинов. Эти команды вводятся в Claude Code.

## Какие задачи покрывает

В опубликованном плагине три скилла. Для текущего пакета полезны прежде всего два:

| Скилл | Назначение | Руководство по API |
|---|---|---|
| `aspid-profiler-marker` | Добавление `this.Marker()` и областей `using` | [ProfilerMarkers](05-profiler-markers.md) |
| `aspid-visual-element-fluent` | Построение и оформление UI Toolkit в коде | [VisualElement Extensions](07-visual-element-extensions.md) |

Третий, `aspid-id-struct`, посвящён `IId` и `[UniqueId]` из прежнего пакета. Он не относится к возможностям, описанным в этой версии документации. Актуальный состав и требования перечислены в [README плагина](https://github.com/VPDPersonal/Aspid.Claude.Plugins/blob/main/plugins/aspid-fasttools/README_RU.md).

## Примеры запросов

Для профилирования:

```text
Добавь маркер на весь метод Simulate и отдельный именованный
маркер на поиск соседей. Используй this.Marker() из Aspid.FastTools.
```

Для интерфейса редактора:

```text
Собери инспектор AbilityConfig через fluent-расширения VisualElement
из Aspid.FastTools. Добавь заголовок, поле стоимости маны и HelpBox,
который появляется при нулевой стоимости. Целевая версия Unity — 6.0.
```

После изменения кода проверьте компиляцию в Unity и результат в инспекторе или Profiler. Скилл помогает выбрать API и форму кода; результат зависит от контекста проекта и версии плагина.

## Обновления и обратная связь

Плагин выпускается независимо от Unity-пакета. Следите за [релизами Aspid.Claude.Plugins](https://github.com/VPDPersonal/Aspid.Claude.Plugins/releases); об ошибках скиллов сообщайте в [Issues плагина](https://github.com/VPDPersonal/Aspid.Claude.Plugins/issues), указав запрос, полученный код и версии плагина и Unity-пакета.

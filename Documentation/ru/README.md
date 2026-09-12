<img src="https://raw.githubusercontent.com/VPDPersonal/Aspid.FastTools/main/docs/images/aspid_fasttools_readme_banner.gif" alt="Aspid.FastTools" />

# Введение

**Aspid.FastTools** — набор инструментов для Unity, который убирает бойлерплейт из повседневной работы. Выбор реализации `SerializeReference` прямо в инспекторе и окно аудита таких ссылок по всему проекту. Roslyn-генераторы и анализаторы, которые пишут повторяющийся код за вас. Утилиты для рантайма и редактора: сериализуемый `System.Type`, fluent-расширения UI Toolkit и другое.

[Исходный код](https://github.com/VPDPersonal/Aspid.FastTools) · [Unity Asset Store](https://assetstore.unity.com/packages/slug/365584) · [Releases](https://github.com/VPDPersonal/Aspid.FastTools/releases)

## Начало работы

[Начало работы](01-getting-started.md) — установка через UPM или Asset Store, а также примеры к каждой фиче.

## Возможности

| Возможность | Что даёт |
|---|---|
| [Serializable Type System](02-serializable-types.md) | Хранит `System.Type` в сериализуемом поле и даёт выбрать тип из окна с поиском прямо в инспекторе |
| [SerializeReference Selector](03-serialize-reference-selector.md) | Выбор реализации `[SerializeReference]` из выпадающего списка в инспекторе, включая generic-типы, и починка битой ссылки на месте |
| [SerializeReference Tooling](04-serialize-reference-tooling.md) | Поиск и починка всех битых managed-ссылок по проекту, а также проверка перед билдом и в CI |
| [ProfilerMarkers](05-profiler-markers.md) | Уникальный генерируемый `ProfilerMarker` для каждого места вызова одним вызовом `this.Marker()` |
| [EnumValues](06-enum-values.md) | Сериализуемые таблицы enum → значение с поддержкой `[Flags]` и без boxing |
| [VisualElement Extensions](07-visual-element-extensions.md) | Построение деревьев UI Toolkit в коде fluent-цепочками вместо вложенных блоков |
| [SerializedProperty Extensions](08-serialized-property-extensions.md) | Типизированная запись значений в `SerializedProperty` одним вызовом в цепочке и доступ к полю через рефлексию |
| [Editor Helpers](09-editor-helpers.md) | Читаемые отображаемые имена скриптов и объектов Unity в кастомных редакторах |
| [Claude Code Plugin](10-claude-code-plugin.md) | Скиллы, обучающие Claude Code этому пакету |

## Поддержать проект

Этот проект разрабатывается на добровольной основе. Если он оказался для вас полезным, поддержать его развитие можно покупкой пакета в [Unity Asset Store](https://assetstore.unity.com/packages/slug/365584) — это помогает уделять больше времени улучшению и сопровождению **Aspid.FastTools**.

## Лицензия

**Aspid.FastTools** распространяется по [лицензии MIT](https://github.com/VPDPersonal/Aspid.FastTools/blob/main/LICENSE). История релизов — в [CHANGELOG](https://github.com/VPDPersonal/Aspid.FastTools/blob/main/CHANGELOG.ru.md).

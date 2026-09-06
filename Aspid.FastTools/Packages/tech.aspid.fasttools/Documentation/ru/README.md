<img src="https://raw.githubusercontent.com/VPDPersonal/Aspid.FastTools/main/docs/images/aspid_fasttools_readme_banner.gif" alt="Aspid.FastTools" />

# Введение

**Aspid.FastTools** — набор инструментов для Unity, избавляющий от рутинного бойлерплейта. Внутри — удобная работа с `SerializeReference` (выбор типа в инспекторе и окно аудита ссылок по всему проекту), Roslyn-генераторы и анализаторы, а также runtime- и editor-утилиты: от сериализуемого `System.Type` до fluent-расширений UI Toolkit.

[Исходный код](https://github.com/VPDPersonal/Aspid.FastTools) · [Unity Asset Store](https://assetstore.unity.com/packages/slug/365584) · [Releases](https://github.com/VPDPersonal/Aspid.FastTools/releases)

## Начало работы

[Установка](01-getting-started.md) — UPM git URL, `.unitypackage`, Asset Store и примеры, поставляемые с пакетом.

## Возможности

| Возможность | Что даёт |
|---|---|
| [Serializable Type System](02-serializable-types.md) | `System.Type` как сериализуемое поле, `[TypeSelector]`, окно выбора типа с поиском, `ComponentTypeSelector` |
| [SerializeReference Selector](03-serialize-reference-selector.md) | Выпадающий выбор типа для полей `[SerializeReference]`, вложенные инспекторы, generics, точечная починка битых ссылок |
| [SerializeReference Tooling](04-serialize-reference-tooling.md) | Аудит и массовая починка по всему проекту, настройки проекта, build/CI-гейт |
| [ProfilerMarkers](05-profiler-markers.md) | Source-generated `ProfilerMarker`, уникальные для каждого места вызова, через `this.Marker()` |
| [EnumValues](06-enum-values.md) | Сериализуемые отображения enum → значение с поддержкой `[Flags]`, без boxing |
| [VisualElement Extensions](07-visual-element-extensions.md) | Fluent-построение UI Toolkit-деревьев в коде |
| [SerializedProperty Extensions](08-serialized-property-extensions.md) | Типизированные сеттеры с fluent-цепочками и рефлексионные хелперы |
| [Editor Helpers](09-editor-helpers.md) | Отображаемые имена скриптов для кастомных редакторов |
| [Claude Code Plugin](10-claude-code-plugin.md) | Скиллы, обучающие Claude Code этому пакету |

## Поддержать проект

Этот проект разрабатывается на добровольной основе. Если он оказался для вас полезным, поддержать его развитие можно покупкой пакета в [Unity Asset Store](https://assetstore.unity.com/packages/slug/365584) — это помогает уделять больше времени улучшению и сопровождению **Aspid.FastTools**.

## Лицензия

**Aspid.FastTools** распространяется по [лицензии MIT](https://github.com/VPDPersonal/Aspid.FastTools/blob/main/LICENSE). История релизов — в [CHANGELOG](https://github.com/VPDPersonal/Aspid.FastTools/blob/main/CHANGELOG.ru.md).

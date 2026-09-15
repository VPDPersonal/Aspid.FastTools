<img src="https://raw.githubusercontent.com/VPDPersonal/Aspid.FastTools/main/docs/images/aspid_fasttools_readme_banner.gif" alt="Aspid.FastTools" />

[![Unity 6.0+](../Images/status-badge-unity.svg)](https://assetstore.unity.com/packages/slug/365584)
[![Preview 1.0.0-rc.8](../Images/status-badge-preview.svg)](https://github.com/VPDPersonal/Aspid.FastTools/releases)
[![MIT License](../Images/status-badge-license.svg)](https://github.com/VPDPersonal/Aspid.FastTools/blob/main/LICENSE)

Инструменты для Unity, которые сокращают повторяющийся код в сериализации и редакторских утилитах.

[Документация](https://vpdpersonal.github.io/Aspid.FastTools/ru/docs) · [Исходный код](https://github.com/VPDPersonal/Aspid.FastTools) · [Релизы](https://github.com/VPDPersonal/Aspid.FastTools/releases)

## Возможности

| Возможность | Что даёт | Демонстрация |
|---|---|---|
| [Serializable Type System](02-serializable-types.md) | Хранение и выбор `System.Type` в инспекторе | ![Выбор сериализуемого типа в инспекторе](../Images/aspid_fasttools_serializable_type.gif) |
| [SerializeReference Selector](03-serialize-reference-selector.md) | Выбор реализации и восстановление битых ссылок на месте | ![Смена Pistol на Shotgun с сохранением Damage = 37](../Images/aspid_fasttools_serialize_reference_selector.gif) |
| [SerializeReference Tooling](04-serialize-reference-tooling.md) | Аудит и восстановление ссылок по всему проекту, в том числе перед сборкой и в CI | ![Восстановление потерянного типа оружия с сохранением данных](../Images/aspid_fasttools_serialize_reference_tooling.gif) |
| [EnumValues](06-enum-values.md) | Редактирование таблиц enum → значение в инспекторе, включая флаги | ![Редактирование enum-ключей и значений в инспекторе](../../Samples~/EnumValues/Documentation/Images/surface-tables.png) |
| [ProfilerMarkers](05-profiler-markers.md) | Уникальный маркер профилирования для каждого места вызова через `this.Marker()` | <pre lang="csharp"><code>using (this.Marker())&#10;&#123;&#10;    Simulate();&#10;&#125;</code></pre> |
| [VisualElement Extensions](07-visual-element-extensions.md) | Построение деревьев UI Toolkit fluent-цепочками | <pre lang="csharp"><code>new VisualElement()&#10;  .SetPadding(8)&#10;  .AddChild(&#10;    new Label("Stats"));</code></pre> |
| [SerializedProperty Extensions](08-serialized-property-extensions.md) | Запись значений, изменение размера массивов, получение типа поля и объекта-владельца | <pre lang="csharp"><code>property&#10;  .Update()&#10;  .SetIntAndApply(42);</code></pre> |
| [Editor Helpers](09-editor-helpers.md) | Читаемые подписи объектов и компонентов для редакторских инструментов | <pre lang="csharp"><code>audio.GetDisplayName();&#10;// "Audio Source"&#10;&#10;secondAudio&#10;  .GetDisplayNameWithIndex();&#10;// "Audio Source (2)"</code></pre> |

## Установка

В **Window → Package Manager** выберите **+ → Install package from git URL…** и вставьте:

```text
https://github.com/VPDPersonal/Aspid.FastTools.git#upm-preview/1.0.0-rc.8
```

Этот URL устанавливает preview-версию, описанную в документации. Для установки через Git URL в системе должен быть установлен Git.

<details>
<summary>Другие варианты установки</summary>

- **Последний preview:** используйте `https://github.com/VPDPersonal/Aspid.FastTools.git#upm-preview`. При обновлении пакета может установиться более новая preview-версия.
- **Другая версия:** скопируйте её UPM-тег со страницы [релизов](https://github.com/VPDPersonal/Aspid.FastTools/releases).
- **Unity Asset Store:** пакет пока недоступен в магазине. Для установки используйте Git URL выше.

> [!WARNING]
> Ветка `upm` сейчас содержит старый пакет `com.aspid.fasttools` (`1.0.0-rc.2`). Для `tech.aspid.fasttools` и описанных здесь возможностей используйте URL выше.

</details>

## Быстрый старт

1. Установите пакет по Git URL выше.
2. В **Window → Package Manager** выберите **Aspid.FastTools**, откройте вкладку **Samples** и импортируйте пример.
3. Откройте его сцену и README; в [обзоре примеров](../../Samples~/README.ru.md) описано, что показывает каждый из них.

## Документация и примеры

- [Обзор примеров](../../Samples~/README.ru.md) — сцены и инструменты для сериализации, enum-таблиц, профилирования и интерфейсов редактора.
- [Справочник API](https://vpdpersonal.github.io/Aspid.FastTools/ru/api/Aspid.FastTools) — публичные типы и члены. Ссылки в таблице возможностей выше ведут к руководствам по их использованию.
- [Плагин Claude Code](10-claude-code-plugin.md) — дополнительные скиллы для работы с пакетом в Claude Code.
- [Журнал изменений](https://github.com/VPDPersonal/Aspid.FastTools/blob/main/CHANGELOG.ru.md) — история релизов.

## Помощь и поддержка

Сообщайте об ошибках и задавайте вопросы в [GitHub Issues](https://github.com/VPDPersonal/Aspid.FastTools/issues). Для ошибки укажите версию Unity, версию пакета и шаги воспроизведения.

После публикации в [Unity Asset Store](https://assetstore.unity.com/packages/slug/365584) вы сможете поддержать разработку покупкой пакета.

Распространяется по [лицензии MIT](https://github.com/VPDPersonal/Aspid.FastTools/blob/main/LICENSE).

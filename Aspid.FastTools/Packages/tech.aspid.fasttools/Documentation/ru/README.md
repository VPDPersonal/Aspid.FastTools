<img src="https://raw.githubusercontent.com/VPDPersonal/Aspid.FastTools/main/docs/images/aspid_fasttools_readme_banner.gif" alt="Aspid.FastTools" />

![Unity 6.0+](../Images/status-badge-unity.svg)
[![Preview 1.0.0-rc.8](../Images/status-badge-preview.svg)](https://github.com/VPDPersonal/Aspid.FastTools/releases/tag/v1.0.0-rc.8)
[![MIT License](../Images/status-badge-license.svg)](https://github.com/VPDPersonal/Aspid.FastTools/blob/main/LICENSE)

Aspid.FastTools — пакет для Unity, закрывающий пробелы в сериализации и редакторских инструментах. Сериализованные типы и полиморфные ссылки переживают переименования, а если ломаются, восстанавливаются без потери данных. Инспектор показывает, что лежит в поле `SerializeReference`, и позволяет это заменить. Редакторские и профилировочные хелперы заменяют одной строкой то, для чего Unity требует отдельный класс.

[Документация](https://vpdpersonal.github.io/Aspid.FastTools/ru/docs) · [Исходный код](https://github.com/VPDPersonal/Aspid.FastTools) · [Релизы](https://github.com/VPDPersonal/Aspid.FastTools/releases)

## Установка

В **Window → Package Manager** выберите **+ → Install package from git URL…** и вставьте:

```text
https://github.com/VPDPersonal/Aspid.FastTools.git#upm-preview
```

URL указывает на последнюю preview-версию; кнопка **Update** в Package Manager подтянет следующую. Чтобы закрепить версию, добавьте её тег со страницы [релизов](https://github.com/VPDPersonal/Aspid.FastTools/releases): `https://github.com/VPDPersonal/Aspid.FastTools.git#upm-preview/<версия>`.

## Возможности

### Сериализация

#### [Serializable Type System](02-serializable-types.md)

Хранение и выбор `System.Type` в инспекторе.

<img src="../Images/serializable-type-quick-start.gif" alt="Выбор сериализуемого типа в инспекторе" width="640" />

#### [ComponentTypeSelector](11-component-type-selector.md)

Смена типа существующего компонента с сохранением общих полей.

<img src="../Images/component-type-selector.gif" alt="Смена типа компонента в инспекторе" width="640" />

#### [SerializeReference Selector](03-serialize-reference-selector.md)

Выбор класса для поля `SerializeReference` прямо в инспекторе.

<img src="../Images/aspid_fasttools_serialize_reference_selector.gif" alt="Смена Pistol на Shotgun с сохранением Damage = 37" width="640" />

#### [SerializeReference Tooling](04-serialize-reference-tooling.md)

Аудит и восстановление ссылок по всему проекту, в том числе перед сборкой и в CI.

<img src="../Images/aspid_fasttools_serialize_reference_tooling.gif" alt="Восстановление потерянного типа оружия с сохранением данных" width="640" />

#### [EnumValues](06-enum-values.md)

Редактирование таблиц enum → значение в инспекторе, включая флаги.

<img src="../../Samples~/EnumValues/Documentation/Images/surface-tables.png" alt="Редактирование enum-ключей и значений в инспекторе" width="640" />

### Редактор и инструменты

#### [ProfilerMarkers](05-profiler-markers.md)

Уникальный маркер профилирования для каждого места вызова через `this.Marker()`.

```csharp
using (this.Marker())
{
    Simulate();
}
```

#### [VisualElement Extensions](07-visual-element-extensions.md)

Построение деревьев UI Toolkit fluent-цепочками.

```csharp
new VisualElement()
  .SetPaddingX(12)
  .AddChild(
    new Label("Stats"));
```

#### [SerializedProperty Extensions](08-serialized-property-extensions.md)

Запись значений, изменение размера массивов, получение типа поля и объекта-владельца.

```csharp
property
  .Update()
  .SetIntAndApply(42);
```

#### [Editor Helpers](09-editor-helpers.md)

Читаемые подписи объектов и компонентов для редакторских инструментов.

```csharp
fireAbility.GetDisplayName();
// "Fire Ability"

abilityConfig
  .GetDisplayNameWithIndex();
// "Ability Config (1)"
```

#### [Claude Code Plugin](10-claude-code-plugin.md)

Скиллы Claude Code для `this.Marker()` и fluent-расширений `VisualElement`.

```text
Добавь маркер на весь метод Simulate
и отдельный на поиск соседей.
```

## Ресурсы

- [Обзор примеров](../../Samples~/README.ru.md) — сцены и инструменты для сериализации, enum-таблиц, профилирования и интерфейсов редактора.
- [Справочник API](https://vpdpersonal.github.io/Aspid.FastTools/ru/api/Aspid.FastTools) — сигнатуры и описания всех публичных типов и членов.
- [Журнал изменений](https://github.com/VPDPersonal/Aspid.FastTools/blob/main/CHANGELOG.ru.md)

## Помощь и поддержка

Сообщайте об ошибках и задавайте вопросы в [GitHub Issues](https://github.com/VPDPersonal/Aspid.FastTools/issues). Для ошибки укажите версию Unity, версию пакета и шаги воспроизведения.

Если пакет пригодился, поставьте ему звезду на [GitHub](https://github.com/VPDPersonal/Aspid.FastTools).

Распространяется по [лицензии MIT](https://github.com/VPDPersonal/Aspid.FastTools/blob/main/LICENSE).

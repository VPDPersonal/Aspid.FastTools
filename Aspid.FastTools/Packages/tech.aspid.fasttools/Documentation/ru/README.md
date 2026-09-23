<img src="https://raw.githubusercontent.com/VPDPersonal/Aspid.FastTools/main/docs/images/aspid_fasttools_readme_banner.gif" alt="Aspid.FastTools" />

[![Unity 6.0+](../Images/status-badge-unity.svg)](https://assetstore.unity.com/packages/slug/365584)
[![Preview 1.0.0-rc.8](../Images/status-badge-preview.svg)](https://github.com/VPDPersonal/Aspid.FastTools/releases)
[![MIT License](../Images/status-badge-license.svg)](https://github.com/VPDPersonal/Aspid.FastTools/blob/main/LICENSE)

Aspid.FastTools — пакет для Unity, закрывающий пробелы в сериализации и редакторских инструментах. Сериализованные типы и полиморфные ссылки переживают переименования, а если ломаются, восстанавливаются без потери данных. Inspector показывает, что лежит в поле `SerializeReference`, и позволяет это заменить. Редакторские и профилировочные хелперы укладываются в строку там, где Unity требует класс.

[Документация](https://vpdpersonal.github.io/Aspid.FastTools/ru/docs) · [Исходный код](https://github.com/VPDPersonal/Aspid.FastTools) · [Релизы](https://github.com/VPDPersonal/Aspid.FastTools/releases)

## Установка

В **Window → Package Manager** выберите **+ → Install package from git URL…** и вставьте:

```text
https://github.com/VPDPersonal/Aspid.FastTools.git#upm-preview
```

Этот URL устанавливает последнюю preview-версию, а при обновлении пакета подтянется более новая. Для установки через Git URL в системе должен быть установлен Git.

<details>
<summary>Другие варианты установки</summary>

- **Другая версия:** скопируйте её UPM-тег со страницы [релизов](https://github.com/VPDPersonal/Aspid.FastTools/releases), например:

  ```text
  https://github.com/VPDPersonal/Aspid.FastTools.git#upm-preview/1.0.0-rc.7
  ```

- **Unity Asset Store:** пакет пока недоступен в магазине. Для установки используйте Git URL выше.
- **Ветка `upm`:** всё ещё содержит старый пакет `com.aspid.fasttools` (`1.0.0-rc.2`). Для `tech.aspid.fasttools` и описанных здесь возможностей используйте URL выше.

</details>

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

## Быстрый старт

1. После установки окно **Welcome** откроется само. Позже его можно открыть через **Tools → Aspid 🐍 → FastTools → Welcome**.
2. Нажмите **Import** у нужного примера; он появится в `Assets/Samples`.
3. Откройте его сцену и README.

## Документация и примеры

- [Обзор примеров](../../Samples~/README.ru.md) — сцены и инструменты для сериализации, enum-таблиц, профилирования и интерфейсов редактора.
- [Справочник API](https://vpdpersonal.github.io/Aspid.FastTools/ru/api/Aspid.FastTools) — публичные типы и члены. Ссылки в разделе возможностей выше ведут к руководствам по их использованию.
- [Плагин Claude Code](10-claude-code-plugin.md) — дополнительные скиллы для работы с пакетом в Claude Code.
- [Журнал изменений](https://github.com/VPDPersonal/Aspid.FastTools/blob/main/CHANGELOG.ru.md) — история релизов.

## Вопросы и ответы

<details>
<summary>Может ли CI падать на сломанных ссылках?</summary>

Да. Выберите `Fail` для **Build / CI gate** в **Project Settings → Aspid FastTools → SerializeReference** или запустите `SerializeReferenceCiGate.RunCheck` в batch-режиме с `-srGateFail`: команда создаёт отчёт и завершается с кодом `1` при нарушениях. Подробнее — в разделе [Запуск в CI](04-serialize-reference-tooling.md#запуск-в-ci).

</details>

<details>
<summary>Почему сканирование пропускает часть ассетов?</summary>

Сканирование читает текстовый YAML с диска. Выберите **Asset Serialization → Mode → Force Text**, пересохраните двоичные ассеты и сохраните изменённые сцены перед сканированием. Подробнее — в [SerializeReference Tooling](04-serialize-reference-tooling.md#быстрый-старт).

</details>

<details>
<summary>Нужны ли для <code>this.Marker()</code> partial-классы или атрибуты?</summary>

Нет. Генератор входит в пакет, а расширение находится в глобальном пространстве имён; оно работает в `MonoBehaviour` и обычных C#-классах. Вызывайте его всегда с `using` и не давайте области пересекать `await` или `yield`. Подробнее — в разделе [Marker()](05-profiler-markers.md#marker).

</details>

## Помощь и поддержка

Сообщайте об ошибках и задавайте вопросы в [GitHub Issues](https://github.com/VPDPersonal/Aspid.FastTools/issues). Для ошибки укажите версию Unity, версию пакета и шаги воспроизведения.

После публикации в [Unity Asset Store](https://assetstore.unity.com/packages/slug/365584) вы сможете поддержать разработку покупкой пакета.

Распространяется по [лицензии MIT](https://github.com/VPDPersonal/Aspid.FastTools/blob/main/LICENSE).

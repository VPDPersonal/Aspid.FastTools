<picture>
  <source media="(prefers-color-scheme: dark)" srcset="../../../../../docs/images/aspid_fasttools_readme_banner_dark.webp" />
  <source media="(prefers-color-scheme: light)" srcset="../../../../../docs/images/aspid_fasttools_readme_banner_light.webp" />
  <img src="../../../../../docs/images/aspid_fasttools_readme_banner.png" alt="Aspid.FastTools" />
</picture>

[![Unity 6.0+](../../../../docs/Images/status-badge-unity.svg)](https://assetstore.unity.com/packages/slug/365584)
[![Preview 1.0.0-rc.8](../../../../docs/Images/status-badge-preview.svg)](https://github.com/VPDPersonal/Aspid.FastTools/releases/tag/v1.0.0-rc.8)
[![MIT License](../../../../docs/Images/status-badge-license.svg)](https://github.com/VPDPersonal/Aspid.FastTools/blob/main/LICENSE)

Aspid.FastTools — пакет для Unity, который убирает рутину из сериализации, профилирования и редакторского кода.

[Документация](https://vpdpersonal.github.io/Aspid.FastTools/ru/docs)

## Установка

В **Window → Package Manager** выберите **+ → Install package from git URL…**, вставьте этот URL и нажмите **Install**:

```text
https://github.com/VPDPersonal/Aspid.FastTools.git#upm-preview
```

URL указывает на последнюю preview-версию; кнопка **Update** в Package Manager установит следующую.

## Возможности

### Сериализация

#### [Serializable Types](02-serializable-types.md)

Сохраняет <code lang="class-name">System.Type</code> в компоненте или ассете и даёт выбрать его в инспекторе из совместимых типов. [TypeSelector](03-type-selector.md) сужает этот список.

<img src="../../../../docs/Images/serializable-type-card.gif" alt="Выбор сериализуемого типа в инспекторе" width="640" />

#### [SerializeReference Selector](04-serialize-reference-selector.md)

Даёт выбрать класс для поля <code lang="csharp">[SerializeReference]</code> в инспекторе и переносит совместимые данные при смене класса.

<img src="../../../../docs/Images/aspid_fasttools_serialize_reference_selector_card.gif" alt="Смена Pistol на Shotgun с сохранением Damage = 37" width="640" />

#### [ComponentTypeSelector](05-component-type-selector.md)

Меняет класс добавленного компонента на наследника, не теряя значения общих полей.

<img src="../../../../docs/Images/component-type-selector-card.gif" alt="Смена типа компонента в инспекторе" width="640" />

#### [Восстановление SerializeReference](06-serialize-reference-tooling.md)

Находит потерянные ссылки <code lang="csharp">[SerializeReference]</code> и имена <code lang="class-name">SerializableType</code> по всему проекту и восстанавливает их группами. [Проверка перед сборкой и CI](07-serialize-reference-validation.md) ловит новые до выпуска.

<img src="../../../../docs/Images/aspid_fasttools_serialize_reference_repair_card.gif" alt="Fix all восстанавливает три потерянные ссылки Blaster как Pistol" width="640" />

#### [EnumValues](08-enum-values.md)

Сопоставляет ключам enum значения — множители, цвета, ассеты — и даёт редактировать их в инспекторе, включая флаги.

<img src="../../../../docs/Images/enum-values-multipliers-populate.gif" alt="Populate Missing Enum Members в таблице Multipliers" width="640" />

### Редактор и инструменты

#### [ProfilerMarkers](09-profiler-markers.md)

Размечает участок одной строкой, а имя маркера генератор собирает из типа, метода и строки вызова.

```csharp
public void Step()
{
    using var _ = this.Marker();
    Integrate();
}
```

#### [VisualElement Extensions](10-visual-element-extensions.md)

Задаёт свойства, стили и события элемента цепочкой, так что дерево UI Toolkit собирается одним выражением.

```csharp
var header = new VisualElement()
    .SetPaddingX(12)
    .SetPaddingY(10)
    .AddChild(new Label("Ability Config")
        .SetFontSize(14));
```

#### [SerializedProperty Extensions](11-serialized-property-extensions.md)

Обновляет объект, записывает значение и применяет изменения одной цепочкой. Ещё находит C#-поле за свойством и объект, которому оно принадлежит.

```csharp
manaCost
  .Update()
  .SetIntAndApply(42);
```

#### [Editor Helpers](12-editor-helpers.md)

Подписывает объекты и компоненты читаемыми именами, а одинаковые компоненты — с номером.

```csharp
caster.GetDisplayName();
// "Ability Caster"

caster.GetDisplayNameWithIndex();
// "Ability Caster (2)": второй AbilityCaster на объекте
```

#### [Theme Override](14-theme-override.md)

Перекрашивает окна FastTools и окно выбора типа файлом USS из вашего проекта.

```css
:root {
    --aspid-colors-bg-dark: rgb(22, 30, 52);
    --aspid-colors-surface-card: rgba(32, 44, 72, 0.6);
}
```

#### [Agent Skills](13-agent-skills.md)

Учит coding-агента писать код с FastTools: маркеры профилировщика, выбор типов, EnumValues и VisualElement Extensions.

```text
Замерь Simulate и отдельно поиск соседей
```

## Ресурсы

- [Обзор примеров](../../../../../Aspid.FastTools/Packages/tech.aspid.fasttools/Samples~/README.ru.md) — сцены и редакторские инструменты для сериализации, enum-таблиц, профилирования и интерфейсов редактора.
- [Справочник API](https://vpdpersonal.github.io/Aspid.FastTools/ru/api/Aspid.FastTools.Editors) — типы, методы и свойства пакета, на английском.
- [Журнал изменений](https://vpdpersonal.github.io/Aspid.FastTools/ru/changelog) — изменения и исправления по версиям.

## Помощь и поддержка

Сообщайте об ошибках и задавайте вопросы в [GitHub Issues](https://github.com/VPDPersonal/Aspid.FastTools/issues). В отчёте об ошибке укажите версию Unity, версию пакета и шаги воспроизведения.

Если пакет пригодился, поставьте ему звезду на [GitHub](https://github.com/VPDPersonal/Aspid.FastTools).

Распространяется по [лицензии MIT](https://github.com/VPDPersonal/Aspid.FastTools/blob/main/LICENSE).

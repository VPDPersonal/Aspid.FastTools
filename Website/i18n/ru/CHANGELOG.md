# Changelog (RU)

> Английская версия: [CHANGELOG.md](../../../CHANGELOG.md). При расхождениях приоритет у английской версии.

Все значимые изменения **Aspid.FastTools** документируются в этом файле.

Формат основан на [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
проект следует [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] — 2026-10-22

Первый стабильный релиз. Нужен Unity **6000.0.53f1** или новее; все функции Inspector работают и в IMGUI, и в UI Toolkit. Prerelease-версии `1.0.0-rc.*` и их заметки лежат в [GitHub Releases](https://github.com/VPDPersonal/Aspid.FastTools/releases).

### Добавлено

- `SerializableType` и `SerializableMonoScript` хранят `System.Type` в компоненте или ассете; `[TypeSelector]` выбирает его в Inspector из совместимых типов.
- SerializeReference Selector выбирает класс поля `[SerializeReference]` и переносит совместимые данные при смене класса.
- `ComponentTypeSelector` меняет добавленный компонент на производный класс без потери общих значений полей.
- Project References находит потерянные ссылки `[SerializeReference]` и имена `SerializableType` по всему проекту и чинит их группами; проверки при сборке и в CI ловят новые.
- `EnumValues` сопоставляет значениям enum данные, которые редактируются в Inspector, включая флаги.
- `this.Marker()` открывает маркер профайлера с именем из типа, метода и строки вызова; его строит source generator.
- Анализаторы `AFT0001`–`AFT0014` сообщают в IDE о неверном использовании `[TypeSelector]` и `this.Marker()`.
- VisualElement Extensions задают свойства, стили и события элемента цепочкой.
- SerializedProperty Extensions обновляют объект, записывают значение и применяют изменение одной цепочкой.
- Editor Helpers: `GetDisplayName()` и `GetDisplayNameWithIndex()` дают объектам и компонентам читаемые имена.
- Окно FastTools (**Tools → Aspid 🐍 → FastTools**) с вкладками Welcome, Asset References, Project References и Settings и темы для окон FastTools.
- Сэмплы для каждой функции и Agent Skills, которые учат агента писать код с FastTools.

[1.0.0]: https://github.com/VPDPersonal/Aspid.FastTools/releases/tag/v1.0.0

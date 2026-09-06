# Changelog (RU)

> Английская версия: [CHANGELOG.md](CHANGELOG.md). При расхождениях приоритет у английской версии.

Все значимые изменения **Aspid.FastTools** документируются в этом файле.

Формат основан на [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
проект следует [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0-rc.8] — 2026-09-06

Первый релиз. Unity **6000.0**, сборки `Aspid.FastTools` / `Aspid.FastTools.Editor`, предсобранные Roslyn-DLL `Aspid.FastTools.Generators` / `Aspid.FastTools.Analyzers`. Все инспекторные возможности работают и в IMGUI, и в UI Toolkit.

### Добавлено

#### Serializable Type System

- `SerializableType` / `SerializableType<T>` — `[Serializable]`-обёртка над `System.Type`, ленивое разрешение, неявное приведение к `Type`, конструктор с `Type`, `AssemblyQualifiedName`.
- `SerializableMonoScript` / `SerializableMonoScript<T>` — то же, но через ассет скрипта, поэтому переименование и перенос класса ничего не ломают; плеер сериализует одно имя.
- `SerializableTypeBase` и `ISerializableType` для полиморфной работы с любой обёрткой.
- `[TypeSelector]` — иерархический пикер типов на полях `string`, `SerializableType` / `SerializableMonoScript` и `[SerializeReference]`, а также на массивах и списках из них. Ограничения базовыми типами, `Allow` (`TypeAllow`), `Required`, ссылки на члены в строковых аргументах (`Type`, `string`, `SerializableType`, массивы из них, разрешаются вживую).
- `[TypeSelectorDisplay]` — `Name`, `Group`, `Tooltip`, `Icon`, `Hidden` для строки типа в пикере.
- `ComponentTypeSelector` — заменяет соседний `Component` на месте через дропдаун типов.
- `TypeSelectorWindow` с публичным API `Show(...)`: дерево пространств имён, поиск, навигация с клавиатуры, Favorites и Recent, `TypeSelectorFilter` (`Predicate`, `AdditionalTypes`, `HideNoneOption`).
- UI Toolkit-элементы `TypeField` / `InspectorTypeField`, на которых построены drawer'ы.

#### SerializeReference Selector

- Дропдаун типов на полях `[SerializeReference]`, включая вложенные ссылки (до 8 уровней); кастомные drawer'ы и `[Header]` / `[Space]` / `[Tooltip]` учитываются.
- Открытые generic-реализации: аргументы выводятся из аргументов типа поля и его интерфейсов, иначе собираются на второй странице пикера.
- Перенос данных при смене типа, Copy / Paste, мультиредактирование, развязывание дубликатов.
- Уведомления об общих ссылках с **Make unique**, цветными группами и переходом к участникам.
- Назначение перетаскиванием `MonoScript`, именованные шаблоны, **Link to Existing**, `+` списка через пикер, **Create New Script…**, **Find Usages**.

#### Починка потерянных ссылок

- Встроенный **Fix** на потерянном типе с сохранением данных; работает для сохранённых ассетов, Prefab Mode и объектов сцены.
- **Smart Fix** ранжирует вероятную замену (`[MovedFrom]`, то же имя в другом месте, регистр, совпадение формы полей).
- Переименования `[MovedFrom]` показываются как ожидающие миграции с **Migrate all** в один клик и не считаются нарушениями.
- Уведомление о поломке после переименования / удаления скрипта или реимпорта; защита удаления скриптов, используемых как managed reference; предпросмотр YAML-diff перед каждой массовой перезаписью.

#### Рабочее окно (`Tools → Aspid 🐍 → FastTools`)

- **Welcome** — примеры с отметками установки; открывается автоматически один раз на версию пакета.
- **Asset References** — весь граф `[SerializeReference]` ассета из YAML с бейджами `MISSING` / `SHARED`, встроенным Fix, Clear для осиротевших записей, Open Source Prefab.
- **Project References** — `Scan Project` по `Assets/`, **Fix all** на тип с Undo, Smart Fix, Migrate all, Required violations.
- **Settings** — все настройки пакета с полосками области (общая / пользовательская) и сбросом по областям.
- Навигация с клавиатуры, легенды, контекстные меню строк на каждой вкладке.
- Проектный индекс использований, провайдер Quick Search `sr:`.
- Build / CI gate: `IPreprocessBuildWithReport` плюс headless `SerializeReferenceCiGate.RunCheck` с `-srGateReport`, `-srGateRequired`, `-srGateWarnOnly`, `-srGateFail`; строгость `Off` / `Warn` / `Fail` и исключённые папки в коммитимом `ProjectSettings/SerializeReferenceSharedSettings.asset`.

#### Настройки

- **Project Settings → Aspid.FastTools → SerializeReference** — авторазвязывание, обнаружение поломок, строгость gate, исключённые папки.
- **Preferences → Aspid.FastTools** — зеркало вкладки Settings: References, Type Selector (Favorites, ёмкость Recent), Welcome, тема.

#### Диагностики анализатора

- `AFT0001` (ошибка) — `[TypeSelector]` на неподдерживаемом поле.
- `AFT0002` (предупреждение) — `Allow` на managed reference игнорируется.
- `AFT0003` (предупреждение) — базовый тип не имеет общих конкретных типов с полем.
- `AFT0004` (ошибка) — `[SerializeReference]` на типе `UnityEngine.Object`.
- `AFT0005` (предупреждение) — ни один конкретный сериализуемый тип не удовлетворяет ограничениям.
- `AFT0006` (ошибка) — строковый аргумент не является ни членом, ни именем типа.
- `AFT0007` (ошибка) — указанный член не может задавать базовые типы.
- `AFT0008` (предупреждение) — строка-не-идентификатор не является корректным именем типа.

#### ProfilerMarkers

- `this.Marker()` — `ProfilerMarker`, уникальный для места вызова.
- `ProfilerMarkersGenerator` — создаёт по полю маркера на место вызова; поддерживает лямбды, локальные функции, `.WithName(...)` и имена `$"..."`; вырезается без `ENABLE_PROFILER`.

#### EnumValues

- `EnumValues<TValue>` — сериализуемый словарь с ключом-enum, значением по умолчанию и поддержкой `[Flags]`.
- `EnumValues<TEnum, TValue>` — типизированный вариант, поиск без боксинга, структурный перечислитель.
- Drawer'ы с редактированием на месте и **Populate Missing Enum Members**.

#### Fluent-расширения VisualElement

- Fluent API на `VisualElement`: раскладка, стиль, границы, цвета, переходы, колбэки, USS, управление детьми с вариантами `*If`, пресеты стилей.
- Помощники для `Button`, `BaseField<T>` (`SetLabel` для 29 типов), `Focusable`, `Foldout`, `HelpBox`, `Image`, `IMGUIContainer`, `IMixedValueSupport`, `INotifyValueChanged`, `IStyle`, `ICustomStyle`, list view, `Manipulators`, `ProgressBar`, `Slider`, `TextElement`, `CallbackEventHandler`.
- Редактор: `BindTo` / `UnbindFrom`, `BindPropertyTo`, `SetBindingPath`, `SetLabel` для `PropertyField`, `AddOpenScriptCommand`, `GetOwnerWindow`.
- `Aspid.FastTools.VisualElements.Math` — `SetValue` / `ValueChanged` для типов `Unity.Mathematics`, компилируется только с `com.unity.mathematics`.

#### Расширения SerializedProperty

- Типизированные сеттеры `Set*` / `Set*AndApply`, `Update`, `ApplyModifiedProperties`, `Persistent`, помощники путей, `GetPropertyType` / `GetFieldInfo` / `GetDeclaringInstance`.

#### Редакторские помощники

- `GetScriptName()` / `GetScriptNameWithIndex()`.
- Команда открытия скрипта, понимающая интерфейсы в файлах с другим именем и вложенные типы.
- `InspectorNotice` / `InspectorNoticeGUI` и брендированные UI Toolkit-компоненты `Aspid*`.

#### Примеры

- **Types**, **SerializeReferences**, **EnumValues**, **ProfilerMarkers**, **EditorTools** — по одной работающей сцене (или окну) с `README.md`.

#### Документация и инструменты

- Документация на английском и русском в `Documentation/`, публикуется на https://vpdpersonal.github.io/Aspid.FastTools/.
- Плагин `aspid-fasttools` для Claude Code в [Aspid.Claude.Plugins](https://github.com/VPDPersonal/Aspid.Claude.Plugins).
- `upm` / `upm/<version>` для стабильных релизов, `upm-preview` для предрелизов.
- EditMode-тесты для YAML-редактора и сканирования CI gate.

[1.0.0-rc.8]: https://github.com/VPDPersonal/Aspid.FastTools/releases/tag/v1.0.0-rc.8

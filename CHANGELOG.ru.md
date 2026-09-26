# Changelog (RU)

> Английская версия: [CHANGELOG.md](CHANGELOG.md). При расхождениях приоритет у английской версии.

Все значимые изменения **Aspid.FastTools** документируются в этом файле.

Формат основан на [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
проект следует [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Добавлено

- Добавлены `RemoveChildren` и `RemoveChildrenIf`: удаляют несколько дочерних элементов за один вызов и принимают те же перегрузки `params`, `IEnumerable`, `List`, `Span` и `ReadOnlySpan`, что и `AddChildren`.
- Для `ToggleButtonGroup` добавлены типизированные перегрузки `SetValue`, `AddValueChanged`, `RemoveValueChanged` и `SetLabel` для `ToggleButtonGroupState`, поэтому вызовы вроде `AddValueChanged(evt => …)` не требуют аргументов типа.
- Добавлены варианты `AndApplyWithoutUndo` для всех сеттеров `SerializedProperty` с немедленным применением, включая перегрузки `SetValue`, ссылки на объекты, перечисления и методы изменения размера массивов.
- Анализатор `AFT0009` (предупреждение) — два базовых типа `[TypeSelector]` не имеют общего типа, поэтому селектор пуст.
- Анализатор `AFT0010` (предупреждение) — вызов `this.Marker()` не открывает маркер профайлера, потому что генератор не поддерживает его тип: тип `private` или `protected` (или вложен в такой тип) либо повторяет имя параметра типа внешнего типа.
- Анализатор `AFT0011` (предупреждение) — scope из `this.Marker()` выброшен (`this.Marker();` отдельной инструкцией, `_ = this.Marker();`, локальная переменная, которую никто не читает), поэтому начатый сэмпл никогда не заканчивается.

### Изменено

- Agent Skills для пакета переехали из плагина `aspid-fasttools` для Claude Code в [Aspid.Claude.Plugins](https://github.com/VPDPersonal/Aspid.Claude.Plugins) в этот репозиторий (`skills/`). Устанавливайте их в Claude Code, Codex, Cursor, GitHub Copilot, Gemini CLI или другой агент командой `npx skills add VPDPersonal/Aspid.FastTools`; плагин больше не публикуется.
- `GetScriptName()` переименован в `GetDisplayName()`, а `GetScriptNameWithIndex()` — в `GetDisplayNameWithIndex()`; замените старые вызовы новыми именами. Оба метода возвращают `string.Empty` для null и уничтоженных объектов. Поиск индекса компонента использует список из пула вместо временных массивов и LINQ.
- `[TypeSelector]` на поле `[SerializeReference]` теперь предлагает только типы, совместимые со всеми типами атрибута, — по тому же правилу, что уже действовало для полей `string` и `SerializableType`; раньше достаточно было совпасть с одним из них. То же относится к `baseTypes` у `SerializeReferenceEditorGUI.CreateField`, `CreateList` и `DrawFieldLayout`. Список альтернатив вроде `typeof(Pistol), typeof(Rifle)` теперь оставляет селектор пустым и вызывает `AFT0009`: дайте разрешённым классам общий интерфейс или базовый класс и укажите его. `AFT0005` теперь проверяет все типы атрибута вместе.
- Сгенерированный код `this.Marker()` теперь объявляет поля маркеров только под `ENABLE_PROFILER`, как и тело `Marker()`, которое их читает: сборки без профайлера больше не создают все маркеры в статическом конструкторе.
- `ProfilerMarkerExtensionsForGenerator.Marker(this object)` стал `Marker<T>(this T)`: вызов в структуре, которую генератор не может обработать, больше не упаковывается, и Burst-джоба компилируется.
- `AFT0010` теперь сообщает о любом вызове `this.Marker()`, который не открывает маркер, а не только о неподдерживаемых типах: получатель другого типа (`other.Marker()`, вызовы в статических классах), метод интерфейса по умолчанию, явный аргумент (`this.Marker(5)`) или аргумент типа, `this?.Marker()`, статическая форма вызова, группа методов и вызов внутри дерева выражений. Для таких вызовов генератор больше не создаёт мёртвые маркеры.
- Generic-класс называет вложенные аргументы типа так, как их пишет C# (`Foo<List<Int32>>.Run (line)`, `Foo<Outer<Int32>.Inner>.Run (line)` вместо ``Foo<List`1>``, `Foo<Inner>`). Generic-структура получает один маркер на точку вызова для всех закрытых типов с именем `Job<T>.Execute (line)`, потому что Burst не может выполнить метку для каждого типа.
- Сгенерированный `Marker()` выбирает маркер по строке через `switch`, поэтому его стоимость больше не растёт с числом точек вызова в типе.

### Удалено

- Из расширений `SerializedProperty` удалён `SetExposedReferenceAndApply()`; вызывайте `SetExposedReference()`. Без контекста `IExposedPropertyTable` сеттер `exposedReferenceValue` в Unity сам применяет запись и записывает Undo, поэтому дополнительное применение ничего не делало, а вариант без Undo поверх него построить нельзя.

### Исправлено

- `GetDisplayName()` и `GetDisplayNameWithIndex()` больше не добавляют « (Script)», когда `[AddComponentMenu]` унаследован от базового класса или его путь пуст либо заканчивается на `/`; такие типы получают «очеловеченное» имя типа. Заголовок теперь берётся из атрибута, объявленного на самом типе, а тип с `[Obsolete]` больше не получает « (Deprecated)».
- Ссылка на член `[TypeSelector(nameof(...))]` у поля внутри `[Serializable]`-класса или элемента списка теперь разрешается на экземпляре, который объявляет поле, — так её уже проверяют анализаторы `AFT0006`–`AFT0008`. Раньше член искался на инспектируемом компоненте или ассете, и инспектор показывал предупреждение. То же относится к селектору окна Asset References.
- Вложенное поле или список `[SerializeReference]` теперь сохраняет пользовательский drawer, который Unity выбирает для него: для открытого generic-типа (`typeof(Effect<>)`), базового класса или интерфейса, даже без `useForChildren`, по типу хранимого экземпляра или элемента списка. Раньше пакет рисовал поверх него свой заголовок и выбор типа.
- `this.Marker()` внутри вложенного типа `private` или `protected` больше не ломает компиляцию ошибкой CS0122. Сгенерированная перегрузка не видит такой тип, поэтому генератор теперь его пропускает: вызов компилируется, маркер не открывается, а `AFT0010` сообщает об этом — чтобы профилировать тип, сделайте его `internal` или `public`.
- `this.Marker()` в типе, вложенном в generic-тип (`Outer<T>.Inner`), теперь компилируется; раньше сгенерированной перегрузке не хватало параметров внешнего типа (CS0246).
- `this.Marker()` в аксессоре индексатора и в статическом конструкторе теперь компилируется; раньше имена сгенерированных полей были недопустимыми. Маркеры называются `Type.Indexer (line)` и `Type.StaticCtor (line)`.
- `this.Marker()` в аксессоре события теперь называется по имени события, как в аксессоре свойства: `Type.Changed (line)` вместо `Type.add_Changed (line)` / `Type.remove_Changed (line)`, в том числе в явной реализации интерфейса.
- `this.Marker()` больше не ломает компиляцию в инициализаторе авто-свойства, в статическом классе, в методе интерфейса по умолчанию и в дереве выражений, в пространстве имён или параметре типа с именем-ключевым словом (`Game.@event`, `Foo<@event>`), а также когда имена типов различаются только регистром или сводятся к одному имени (`Foo`/`foo`, `Foo<T>`/`Foo_1`, `Outer.Inner`/`Outer_Inner`); в части этих случаев пропадали все маркеры сборки.
- `Foo`, `Foo<T>` и `Foo<T1, T2>` в одном пространстве имён больше не делят один сгенерированный класс, в котором маркеры получал только первый тип; маркеры собственного `EnumValues<TEnum, TValue>` пакета теперь открываются.
- Вызов `this.Marker()`, разбитый на несколько строк или стоящий под директивой `#line`, теперь открывает свой маркер; генератор брал не ту строку, что `[CallerLineNumber]`.
- Generic-структура с `[BurstCompile]` больше не ломает сборку Burst (BC1025/BC1360).
- Тип, унаследованный от типа другой сборки, с которой он делит пространство имён через `InternalsVisibleTo`, теперь получает собственные маркеры; раньше его вызовы попадали в перегрузку базового типа и ничего не открывали.
- `WithName($"Br{{ace}}")` даёт `Br{ace}`, текст `WithName` с U+2028, U+2029 или U+0085 компилируется, а собственное расширение `WithName` пользователя больше не переименовывает маркер.
- `Persistent()` больше не оставляет неосвобождённым созданный `SerializedObject`, если путь свойства больше не существует на целевых объектах: он освобождается перед возвратом `null`.
- `Persistent()` сохраняет `context` исходного `SerializedObject`, поэтому `ExposedReference`, прочитанный или записанный через копию, разрешается в той же таблице (например, `PlayableDirector`), а не в значении по умолчанию в ассете.

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
- Помощники для `Button`, `BaseField<T>` (`SetLabel` для 29 типов), `Focusable`, `Foldout`, `HelpBox`, `Image`, `IMGUIContainer`, `IMixedValueSupport`, `INotifyValueChanged`, `IStyle`, `ICustomStyle`, list view, `Manipulators`, `ProgressBar`, `Slider`, `TextElement`.
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
- `upm` / `upm/<version>` для стабильных релизов, `upm-preview` для предрелизов. До первого стабильного релиза в ветке `upm` остаётся старый пакет `com.aspid.fasttools` (`1.0.0-rc.2`).
- EditMode-тесты для YAML-редактора и сканирования CI gate.

[1.0.0-rc.8]: https://github.com/VPDPersonal/Aspid.FastTools/releases/tag/v1.0.0-rc.8

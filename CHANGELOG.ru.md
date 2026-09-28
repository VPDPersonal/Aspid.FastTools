# Changelog (RU)

> Английская версия: [CHANGELOG.md](CHANGELOG.md). При расхождениях приоритет у английской версии.

Все значимые изменения **Aspid.FastTools** документируются в этом файле.

Формат основан на [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
проект следует [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Добавлено

- Добавлены `RemoveChildren` и `RemoveChildrenIf`: удаляют несколько дочерних элементов за один вызов и принимают те же перегрузки `params`, `IEnumerable`, `List`, `Span` и `ReadOnlySpan`, что и `AddChildren`.
- Для `ToggleButtonGroup` добавлены типизированные перегрузки `SetValue`, `AddValueChanged`, `RemoveValueChanged` и `SetLabel` для `ToggleButtonGroupState`, поэтому вызовы вроде `AddValueChanged(evt => …)` не требуют аргументов типа.
- `TextField`, `IntegerField`, `LongField`, `UnsignedIntegerField`, `UnsignedLongField`, `FloatField`, `DoubleField` и `Hash128Field` получили цепочечные сеттеры: `SetMaxLength`, `SetMaskChar`, `SetDelayed`, `SetReadOnly`, `SetPassword`, `SetPlaceholder`, `SetHidePlaceholderOnFocus`, `SetKeyboardType`, `SetAutoCorrection`, `SetHideMobileInput`, `SetHideSoftKeyboard`, а для выделения текста — `SetSelectable`, `SetSelectAllOnFocus`, `SetSelectAllOnMouseUp`, `SetDoubleClickSelectsWord`, `SetTripleClickSelectsLine`, `SetCursorIndex`, `SetSelectIndex`, `AddOnCursorIndexChange` / `RemoveOnCursorIndexChange`, `AddOnSelectIndexChange` / `RemoveOnSelectIndexChange`. Сеттеры `ITextEdition` и `ITextSelection` к этим полям не применяются. Для других типов значений есть `TextInputBaseFieldExtensions` и `TextInputBaseFieldTextSelectionExtensions`.
- Добавлены варианты `AndApplyWithoutUndo` для всех сеттеров `SerializedProperty` с немедленным применением, включая перегрузки `SetValue`, ссылки на объекты, перечисления и методы изменения размера массивов.
- Анализатор `AFT0009` (предупреждение) — два базовых типа `[TypeSelector]` не имеют общего типа, поэтому селектор пуст.
- Анализатор `AFT0010` (предупреждение) — вызов `this.Marker()` не открывает маркер профайлера, потому что генератор не поддерживает его тип: тип `private` или `protected` (или вложен в такой тип) либо повторяет имя параметра типа внешнего типа.
- Анализатор `AFT0011` (предупреждение) — scope из `this.Marker()` выброшен (`this.Marker();` отдельной инструкцией, `_ = this.Marker();`, локальная переменная, которую никто не читает), поэтому начатый сэмпл никогда не заканчивается.

### Изменено

- Agent Skills для пакета переехали из плагина `aspid-fasttools` для Claude Code в [Aspid.Claude.Plugins](https://github.com/VPDPersonal/Aspid.Claude.Plugins) в этот репозиторий (`skills/`). Устанавливайте их в Claude Code, Codex, Cursor, GitHub Copilot, Gemini CLI или другой агент командой `npx skills add VPDPersonal/Aspid.FastTools`; плагин больше не публикуется.
- `GetScriptName()` переименован в `GetDisplayName()`, а `GetScriptNameWithIndex()` — в `GetDisplayNameWithIndex()`; замените старые вызовы новыми именами. Оба метода возвращают `string.Empty` для null и уничтоженных объектов. Поиск индекса компонента использует список из пула вместо временных массивов и LINQ.
- Переименованы расширения VisualElement; замените старые вызовы:
  - `SetIsDelayed` / `SetIsPassword` / `SetIsReadOnly` / `SetIsSelectable` → `SetDelayed` / `SetPassword` / `SetReadOnly` / `SetSelectable`;
  - `IsFocus` → `IsFocused`, `SetFocus` / `SetBlur` → `FocusSelf` / `BlurSelf`;
  - `EnableInClass` / `ToggleInClass` → `EnableClass` / `ToggleClass`;
  - `AddStyleSheets` / `RemoveStyleSheets` → `AddStyleSheet` / `RemoveStyleSheet`, `AddStyleSheetsFromResource` / `RemoveStyleSheetsFromResource` → `AddStyleSheetFromResources` / `RemoveStyleSheetFromResources`;
  - `SetImageFromResource`, `SetSpriteFromResource`, `SetVectorImageFromResource`, `SetBackgroundImageFromResource` → `…FromResources`;
  - `MarkDirtyLayout` → `MarkDirtyLayoutSelf`: `IMGUIContainer.MarkDirtyLayout()` перекрывал старое расширение, и вызов возвращал `void`; новое имя встраивается в цепочку.
- Классы расширений `BaseFieldExtensionsSetLabel<Type>` переименованы в `BaseField<Type>Extensions` (`BaseFieldExtensionsSetLabelInt` → `BaseFieldIntExtensions`), а `ProgressBarExtensions` — в `AbstractProgressBarExtensions`. Вызовы методов расширения не меняются; меняются только вызовы через имя класса.
- У `SetShowMixedValue` больше нет значения аргумента по умолчанию `true`; передавайте значение явно.
- `SetDirection`, `SetFill`, `SetInverted`, `SetPageSize` и `SetShowInputField` теперь возвращают собственный тип слайдера (`Slider`, `SliderInt`) вместо `BaseSlider<TValue>`, поэтому цепочка сохраняет члены слайдера. Они принимают `BaseSlider<float>` и `BaseSlider<int>`; открытые перегрузки для `BaseSlider<TValue>` удалены. Перегрузки для `BaseSlider<int>` находятся в новом классе `SliderIntExtensions`: вызовы методов расширения не меняются, но вызов через `SliderExtensions.` на `SliderInt` нужно заменить на `SliderIntExtensions.`.
- Конструкторы без параметров у `SerializableType` / `SerializableType<T>` больше не публичные: создавайте обёртку через `new SerializableType(type)` или `new SerializableType<T>(type)` (`null` даёт пустую). У `SerializableMonoScript` / `SerializableMonoScript<T>` нет публичных конструкторов: объявляйте их сериализуемыми полями и выбирайте скрипт в инспекторе.
- `[TypeSelector]` на поле `[SerializeReference]` теперь предлагает только типы, совместимые со всеми типами атрибута, — по тому же правилу, что уже действовало для полей `string` и `SerializableType`; раньше достаточно было совпасть с одним из них. То же относится к `baseTypes` у `SerializeReferenceEditorGUI.CreateField`, `CreateList` и `DrawFieldLayout`. Список альтернатив вроде `typeof(Pistol), typeof(Rifle)` теперь оставляет селектор пустым и вызывает `AFT0009`: дайте разрешённым классам общий интерфейс или базовый класс и укажите его. `AFT0005` теперь проверяет все типы атрибута вместе.
- В инспекторе runtime-объекта селектор типов больше не предлагает типы из editor-only сборок (`UnityEditor`, asmdef только для Editor и папки `Editor`): плеер не может их найти, и в билде значение молча становилось `null` или потерянной ссылкой. Поля окон редактора и других editor-only объектов по-прежнему предлагают любые типы.
- Селектор типов без ограничений (`SerializableType` без `T`, `[TypeSelector]` на строке, `TypeField`) переиспользует список типов между открытиями, а не собирает его заново из всех загруженных типов.
- `SerializeReferenceIMGUIList.Draw` теперь бросает `ArgumentNullException` для свойства `null` и `ArgumentException` для свойства, которое не является списком managed references, как `SerializeReferenceEditorGUI.CreateList`; раньше он ничего не рисовал. Метка `null` теперь показывает отображаемое имя свойства; чтобы скрыть её, передайте `GUIContent.none`.
- Сгенерированный код `this.Marker()` теперь объявляет поля маркеров только под `ENABLE_PROFILER`, как и тело `Marker()`, которое их читает: сборки без профайлера больше не создают все маркеры в статическом конструкторе.
- `ProfilerMarkerExtensionsForGenerator.Marker(this object)` стал `Marker<T>(this T, [CallerLineNumber] int line = -1)`: вызов в структуре, которую генератор не может обработать, больше не упаковывается, и Burst-джоба компилируется. Параметры у него те же, что у сгенерированной перегрузки, поэтому маркеры получает и тип без пространства имён, как в шаблоне скрипта Unity. Вызов `this.Marker()` внутри дерева выражений больше не компилируется (CS0854), а группе методов теперь нужен `Func<int, AutoScope>`.
- `AFT0010` теперь сообщает о любом вызове `this.Marker()`, который не открывает маркер, а не только о неподдерживаемых типах: получатель другого типа (`other.Marker()`, вызовы в статических классах), метод интерфейса по умолчанию, явный аргумент (`this.Marker(5)`) или аргумент типа, `this?.Marker()`, статическая форма вызова, группа методов и вызов внутри дерева выражений. Для таких вызовов генератор больше не создаёт мёртвые маркеры.
- Generic-класс называет вложенные аргументы типа так, как их пишет C# (`Foo<List<Int32>>.Run (line)`, `Foo<Outer<Int32>.Inner>.Run (line)` вместо ``Foo<List`1>``, `Foo<Inner>`). Generic-структура получает один маркер на точку вызова для всех закрытых типов с именем `Job<T>.Execute (line)`, потому что Burst не может выполнить метку для каждого типа.
- Сгенерированный `Marker()` выбирает маркер по строке через `switch`, поэтому его стоимость больше не растёт с числом точек вызова в типе.
- Окно Sample Themes подписывается на цикл обновления редактора, рендер камер и сохранение сцен только при включённом предпросмотре **Light** или **Dark**; в режиме по умолчанию **Authored** оно не добавляет редактору обработчиков.
- `Aspid.FastTools.VisualElements.Math` больше не компилируется пустой сборкой в проектах без `com.unity.mathematics`.
- Анимированный фон из точек в окне FastTools и на странице настроек рисует все точки одним мешем, а не тесселирует отдельный путь на каждую точку в каждом кадре, и замирает, пока Unity в фоне, поэтому открытое окно без действий больше не нагружает редактор.

### Удалено

- Из расширений `SerializedProperty` удалён `SetExposedReferenceAndApply()`; вызывайте `SetExposedReference()`. Без контекста `IExposedPropertyTable` сеттер `exposedReferenceValue` в Unity сам применяет запись и записывает Undo, поэтому дополнительное применение ничего не делало, а вариант без Undo поверх него построить нельзя.
- Удалено editor-only свойство `SerializableMonoScript.Script`, которое возвращало ассет `MonoScript`; публичного доступа к ассету больше нет. Тип по-прежнему доступен через `Type` или неявное преобразование в `Type`.
- Удалены `AddMakeItem` / `RemoveMakeItem` у `ListView` и `TreeView`, а также `AddMakeHeader` / `AddMakeFooter` / `AddMakeNoneElement` у `BaseListView` вместе с парами `Remove*`; у списка одна фабрика, поэтому вызывайте `SetMakeItem`, `SetMakeHeader`, `SetMakeFooter` или `SetMakeNoneElement`.
- Runtime-методы `Aspid.FastTools.StringExtensions.ToKebabCase` и `Aspid.FastTools.TypeExtensions.GetMembersInfosIncludingBaseClasses` убраны из публичного API без замены.

### Исправлено

- `GetDisplayName()` и `GetDisplayNameWithIndex()` больше не добавляют « (Script)», когда `[AddComponentMenu]` унаследован от базового класса или его путь пуст либо заканчивается на `/`; такие типы получают «очеловеченное» имя типа. Заголовок теперь берётся из атрибута, объявленного на самом типе, а тип с `[Obsolete]` больше не получает « (Deprecated)».
- Ссылка на член `[TypeSelector(nameof(...))]` у поля внутри `[Serializable]`-класса или элемента списка теперь разрешается на экземпляре, который объявляет поле, — так её уже проверяют анализаторы `AFT0006`–`AFT0008`. Раньше член искался на инспектируемом компоненте или ассете, и инспектор показывал предупреждение. То же относится к селектору окна Asset References.
- Вложенное поле `[SerializeReference]` теперь сохраняет пользовательский drawer, зарегистрированный для его объявленного типа через открытый generic-тип (`typeof(Effect<>)`), базовый класс или интерфейс, даже без `useForChildren`, а также drawer базового класса атрибута; раньше пакет рисовал поверх них свой заголовок и выбор типа. Во вложенном списке такой drawer рисует каждый элемент и в IMGUI, и в UI Toolkit, а **+** сохраняет выбор типа. Drawer только хранимого типа выбор типа не заменяет.
- Сброс пользовательских настроек больше не перечисляет в подсказке и диалоге подтверждения удалённую опцию «Dropdown without [TypeSelector]».
- `ComponentTypeSelector` теперь добавляет компоненты из `[RequireComponent]` нового класса и отменяет смену типа с предупреждением в Console, если она создала бы дубликат класса с `[DisallowMultipleComponent]`, убрала класс, который нужен другому компоненту, или потребовала компонент, который нельзя добавить. Нужные компоненты добавляются раньше нового класса, поэтому его `OnValidate` их уже находит. Раньше объект оставался без нужных компонентов или с дубликатами.
- **Create New Script…** у поля `[SerializeReference]` предлагает `NewInteractable` для `IInteractable` (было `Newnteractable`) и `NewEffect` для `IEffect<T>` или generic-базового класса (было недопустимое ``NewEffect`1``, которое отклонялось при подтверждении).
- Сохранённый шаблон, тип которого сейчас не находится (другая ветка, удалённый пакет, класс перенесён в другой namespace или сборку), теперь только скрывается из **Paste Template**. Раньше открытие контекстного меню удаляло его навсегда, во всех копиях проекта. **Paste Template → Remove Missing (N)…** удаляет такие шаблоны по запросу, а перезапись такого шаблона по имени сообщает, что его тип не найден.
- **Excluded scan folders** принимает только `Assets` и папки внутри неё — только их обходят проектные сканирования. Папка из `Packages/` или другого места сохранялась, но ничего не исключала.
- Поиск в селекторе типов больше не совпадает с частью имени, где указана сборка: короткие запросы вроде `Key`, `Token`, `ver` или `null` находили все типы. Поиск сравнивает подпись, имя типа и полное имя с пространством имён и внешними типами (`Namespace.Outer.Name`).
- Ввод и Backspace в селекторе типов снова меняют запрос после перехода в результаты стрелкой вниз; раньше они пропадали, пока не щёлкнуть по полю поиска.
- Для потерянного типа селектор больше не выделяет `<None>`, поэтому Enter сразу после открытия не стирает сохранённое имя.
- В инспекторе UI Toolkit выбор `<None>` теперь очищает `SerializableMonoScript` с потерянным типом; раньше поле продолжало показывать `<Missing …>`.
- **+** у списка `[TypeSelector]` `[SerializeReference]`, вложенного в элемент другого массива (список структур, список внутри элемента списка), теперь добавляет элемент в этот список; раньше он бросал `InvalidOperationException` и ничего не добавлял.
- **+** у массива, элементы которого содержат поле `[TypeSelector]` `[SerializeReference]` (например, `WeaponSlot[]` в примере), теперь добавляет обычный элемент; раньше, если в массиве уже был элемент, он открывал селектор типа этого поля и бросал исключение.
- **+** у списка managed references при выборе нескольких объектов теперь добавляет независимый экземпляр в каждый объект одной группой Undo. В IMGUI раньше менялся только первый объект, а в UI Toolkit новый элемент делил экземпляр с предыдущим.
- Инспектор `EnumValues` больше не переписывает ключи при отрисовке: строка, ключ которой enum не может распознать (после смены enum в `EnumValues<TValue>`, переименования или удаления члена), показывается со своим ключом, например `<Missing Frozen>`, и сохраняет его, пока вы не выберете член. Раньше ей подставлялся первый член enum, и возврат прежнего типа строки не восстанавливал. Строка, добавленная в пустую таблицу, теперь показывается как `<None>` и пропускается с ошибкой, пока вы не выберете член; раньше ей автоматически подставлялся первый член.
- **Populate Missing Enum Members** теперь работает, когда значение — массив или `List<T>`; раньше пункт бросал исключение и оставлял недосозданную строку. Для enum с алиасами (`Default = Medium`) каждое значение добавляется один раз, а пункт становится неактивным, когда строки есть у всех значений; раньше каждое нажатие добавляло дубликат.
- Строка `EnumValues` с `[Flags]`-enum на `long` или `ulong`, а также на `uint` с членом в 31-м бите, получает выпадающий список флагов, который сохраняет все биты; для 64-битного enum IMGUI-инспектор раньше бросал исключение при каждой перерисовке, а UI Toolkit показывал неверную маску и записывал другой ключ.
- В UI Toolkit-инспекторе выпадающий список ключа строки `EnumValues` теперь следует за Undo, Revert и Paste, а заголовок таблицы показывает метку, переданную в `PropertyField`, а не всегда имя поля.
- Undo после **Fix** отсутствующего типа в открытой сцене или Prefab Mode возвращает отсутствующую ссылку вместе с сохранёнными данными; раньше поле оставалось пустым, а следующее сохранение записывало `rid: -2`. Заменённая запись теперь удаляется при сохранении сцены или префаба (Auto Save в Prefab Mode сохраняет сразу после Fix), поэтому предупреждение Unity об отсутствующих типах остаётся до сохранения; это сохранение также очищает историю Undo объекта, поэтому восстановление уже не отменить.
- Editor-сборка пакета теперь компилируется в Unity 6000.5, где `Object.GetInstanceID()` стал ошибкой (CS0619); раньше проект оставался в Safe Mode, и ни один editor-инструмент пакета не работал.
- Поля в обычном инспекторе, UI Toolkit или IMGUI, теперь учитывают светлую тему редактора: drawer `EnumValues` берёт фон из темы Unity, уведомления о ссылках и подпись пропавшего типа у полей `[SerializeReference]` — цвет предупреждения Unity, а иконки предупреждения, информации и папки — варианты для светлой темы.
- Theme override теперь перекрашивает карточки, панели, статусные заливки, полосы прокрутки, фон из точек и переключатели окон Aspid: их цвета вынесены в токены `--aspid-colors-*`, а переключатели читают необязательные токены `--aspid-colors-switch-*`.
- На Unity 6000.0–6000.2 поле `[SerializeReference]`, пикер и окна Project References и Asset References снова получают стили: два листа стилей использовали запись цвета, которую эти версии не разбирают, поэтому каждый отбрасывался целиком, а при импорте в Console появлялась ошибка.
- Кнопки и foldout'ы внутри поля `[SerializeReference]`, нарисованного на UI Toolkit, сохраняют свой вид: кнопка пользовательского drawer'а или `+` / `−` вложенного списка больше не сжимается до 18×18 с иконкой папки, а вложенный foldout больше не перенимает раскладку заголовка поля.
- Fix, Fix all и Undo в сводке больше не переписывают тип исправной managed-ссылки, если на сломанную указывает `[SerializeReference]`-список внутри другой ссылки (родитель со списком детей). Inspector читает у таких ссылок правильный сохранённый тип и поля, а граф Project References больше не показывает фантомные узлы, чей Clear удалял исправные entry.
- Защита missing-списков теперь сохраняет снимок missing-элемента, на который указывает ещё и список другой ссылки, и восстанавливает его в собственный список объекта, а не в одноимённый список, вложенный в другое поле.
- Элемент списка, восстановленный защитой missing-списков, сохраняет исходный id ссылки, если он свободен, а иначе получает случайный. Раньше он брал следующий id после максимального в файле, который часто уже занимал override в варианте, вложенном префабе или сцене: такой override терял свой тип и делил данные с восстановленным элементом.
- Объекты с отрицательным fileID (компоненты префабов, под-ассеты) теперь читаются как отдельные документы: их missing-ссылки и незаполненные required-поля попадают в сканы и build gate, а Fix, Clear и удаление orphan больше не правят и не удаляют entry соседнего объекта.
- Исправление пропавшего типа `[SerializeReference]` (смена типа, очистка, удаление осиротевшей записи, восстановление элемента списка) теперь сначала делает checkout ассета в системе контроля версий, как это делают сохранения самого Unity. Ассет только для чтения остаётся нетронутым с понятной ошибкой вместо исключения доступа, правка заменяет файл за один шаг, поэтому сбой записи больше не оставляет его обрезанным, а BOM UTF-8 сохраняется.
- Восстановление потерянного типа больше не отбрасывает молча несохранённые изменения восстанавливаемого ассета. **Fix** и **Smart Fix** в инспекторе, а также восстановление и очистка в Asset References предлагают сначала сохранить такой ассет; **Fix all**, **Clear** и **Undo** в Project References пропускают его или очищают в памяти. **Fix** в инспекторе больше не переписывает префаб, открытый в Prefab Mode.
- `this.Marker()` внутри вложенного типа `private` или `protected` больше не ломает компиляцию ошибкой CS0122. Сгенерированная перегрузка не видит такой тип, поэтому генератор теперь его пропускает: вызов компилируется, маркер не открывается, а `AFT0010` сообщает об этом — чтобы профилировать тип, сделайте его `internal` или `public`.
- `this.Marker()` в типе, вложенном в generic-тип (`Outer<T>.Inner`), теперь компилируется; раньше сгенерированной перегрузке не хватало параметров внешнего типа (CS0246).
- `this.Marker()` в аксессоре индексатора и в статическом конструкторе теперь компилируется; раньше имена сгенерированных полей были недопустимыми. Маркеры называются `Type.Indexer (line)` и `Type.StaticCtor (line)`.
- `this.Marker()` в аксессоре события теперь называется по имени события, как в аксессоре свойства: `Type.Changed (line)` вместо `Type.add_Changed (line)` / `Type.remove_Changed (line)`, в том числе в явной реализации интерфейса.
- `this.Marker()` больше не ломает компиляцию в инициализаторе авто-свойства, в статическом классе и в методе интерфейса по умолчанию, в пространстве имён или параметре типа с именем-ключевым словом (`Game.@event`, `Foo<@event>`), а также когда имена типов различаются только регистром или сводятся к одному имени (`Foo`/`foo`, `Foo<T>`/`Foo_1`, `Outer.Inner`/`Outer_Inner`); в части этих случаев пропадали все маркеры сборки.
- `this.Marker()` в типе с `[Obsolete]`, во вложенном в него типе и в generic-типе с таким ограничением больше не вызывает CS0618 в сгенерированном коде, а в типе с `[Obsolete(..., true)]` больше не ломает компиляцию ошибкой CS0619.
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
- **Asset References** — весь граф `[SerializeReference]` ассета из YAML с предупреждающей полосой на отсутствующих типах и бейджами `SHARED`, встроенным Fix, Clear для осиротевших записей, Open Source Prefab.
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

#### Примеры

- **Types**, **SerializeReferences**, **EnumValues**, **ProfilerMarkers**, **EditorTools** — по одной работающей сцене (или окну) с `README.md`.

#### Документация и инструменты

- Документация на английском и русском в `Documentation/`, публикуется на https://vpdpersonal.github.io/Aspid.FastTools/.
- Плагин `aspid-fasttools` для Claude Code в [Aspid.Claude.Plugins](https://github.com/VPDPersonal/Aspid.Claude.Plugins).
- `upm` / `upm/<version>` для стабильных релизов, `upm-preview` для предрелизов. До первого стабильного релиза в ветке `upm` остаётся старый пакет `com.aspid.fasttools` (`1.0.0-rc.2`).
- EditMode-тесты для YAML-редактора и сканирования CI gate.

[1.0.0-rc.8]: https://github.com/VPDPersonal/Aspid.FastTools/releases/tag/v1.0.0-rc.8

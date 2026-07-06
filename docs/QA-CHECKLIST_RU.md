# QA Checklist — полный функциональный чек-лист Aspid.FastTools

> Постоянный регламент ручной проверки. Проходится **целиком перед каждым релизом** (стабильным или rc) и выборочно — по затронутым разделам — перед мержем крупных веток.
> English version: [QA-CHECKLIST.md](QA-CHECKLIST.md). Держать оба файла синхронными.
>
> Пометка *(2×UI)* — пункт проверяется в **обоих** инспекторах: UIToolkit и IMGUI.

## Как пользоваться

1. Скопировать файл или отметить чекбоксы в рабочей копии (не коммитить отметки — файл в репозитории остаётся чистым шаблоном).
2. Прогонять на той версии Unity, для которой готовится релиз; минимум — на минимальной поддерживаемой (6000.0).
3. Любой провал — issue + повторный прогон раздела после фикса.
4. Новая фича = новый пункт здесь **в обоих языках** до мержа её ветки.

---

## 1. TypeSelector — string / SerializableType поля

- [ ] `[TypeSelector]` на `string`-поле: пикер открывается, выбор пишет assembly-qualified имя.
- [ ] `SerializableType` / `SerializableType<T>`: сериализация, generic-констрейнт сужает список.
- [ ] Сужение кандидатов через `[TypeSelector(typeof(Base))]`, несколько базовых типов.
- [ ] `Allow` по умолчанию `TypeAllow.All`: пикер `[TypeSelector]` `string` / `SerializableType` показывает абстрактные классы и интерфейсы; `Allow = TypeAllow.None` ограничивает только конкретными типами.
- [ ] `Required` на string-поле: пустое значение — inline-warning *(2×UI)* + нарушение для gate.
- [ ] `null`-элемент в `Type[]`-члене не роняет пикер.

## 2. Окно выбора типа (TypeSelectorWindow)

- [ ] Поиск: матчит реальное имя и display-имя (`TypeSelectorDisplay.Name`); генерики ищутся и по открытому имени.
- [ ] Навигация: стрелки, Enter, Esc-лестница, type-to-search, breadcrumbs, Space — favorite-toggle.
- [ ] `<None>`: очищает ссылку; preselect на `<None>` при None/Missing значении; ✓ не рисуется там, где текущего значения нет (list `+`, Fix, bulk picker).
- [ ] Favorites: ★ по hover, персист в EditorPrefs, скрытие секции настройкой (список переживает выключение), prune нерезолвящихся типов.
- [ ] Recent: MRU, ёмкость слайдером 0–20 (0 = выкл без потери истории), закрытый генерик пишется открытым определением.
- [ ] Счётчики типов на заголовках секций и namespace-строках (рекурсивные, видны при свёрнутой секции).
- [ ] Текущее значение: зелёный ✓ + bold; divider между pinned-блоком и иерархией.
- [ ] Футер и шапка окна не сжимаются контентом; шапка без лишнего скругления.
- [ ] `TypeSelectorDisplay`: `Name` (в строках и капшене дропдауна, tooltip показывает реальную идентичность), `Group` (заменяет namespace-путь, сегменты шарятся), `Tooltip`, `Icon` (все 3 источника: IconContent / путь ассета / Resources).
- [ ] Дизамбигуация двух типов с одинаковым display-именем.

## 3. SerializeReference-дропдаун *(2×UI везде)*

- [ ] Одиночное поле, `List<T>`/массив, abstract база, interface, сужение базовыми типами.
- [ ] Выбор типа инстанцирует; nested-свойства под foldout; hover-tooltip с полной идентичностью `Namespace.Class, Assembly`.
- [ ] Keep-data при смене типа (совпадающие по имени и shape поля переносятся); nested `[SerializeReference]`-дети не теряются.
- [ ] Открытые генерики: инференс из закрытого поля; вторая страница выбора аргументов с констрейнтами; валидация против типа поля.
- [ ] Copy/Paste через контекст-меню: paste как независимый инстанс, disable при несовместимом типе.
- [ ] Multi-object editing: mixed-state дропдаун, выбор применяет независимые инстансы одним Undo; notices подавлены.
- [ ] Дублирование элемента списка (Duplicate/Ctrl+D/`+`) не алиасит ссылку; bulk restore (Paste Component Values, Revert) не де-алиасит намеренный шаринг.
- [ ] Shared-ссылки: детерминированный цвет группы, бейдж `Shared reference #N`, tooltip со списком путей, клик — скролл к следующему члену + пульс (включая элементы списков).
- [ ] Make Unique / Link to Existing: deep-copy графа с сохранением топологии, циклы безопасны; Link не предлагает алиасящего предка.
- [ ] Authoring: drag MonoScript на поле; Save as Template… / Paste Template ▸ (персист на проект); list `+` открывает пикер; Create New Script… генерит стаб и назначает после компиляции (переживает domain reload).
- [ ] `Required` на managed reference: warning при null *(2×UI)* + учёт в gate.
- [ ] Facade `SerializeReferenceEditorGUI` (`CreateField`/`CreateList`/`DrawFieldLayout`) работает из `CreateInspectorGUI`/`OnInspectorGUI` кастомного эдитора.
- [ ] Позиции notices совпадают в обоих рендерерах (missing/required/mixed — при пустом значении; shared — в самом низу).

## 4. Ремонт missing-type

- [ ] Inline-notice: янтарный стайл, left-ellipsis капшена, **Fix** открывает констрейнутый пикер, hover — полная идентичность.
- [ ] Smart Fix (`· → Pistol?`): ранжирование ([MovedFrom] > same name > casing > shape), никогда не auto-apply; кэш сбрасывается при внешнем изменении ассетов (git checkout).
- [ ] Ремонт: сохранённый SO, prefab-ассет, Prefab Mode (live-инстанс; bail при dirty stage), объект сохранённой сцены (in-memory через GlobalObjectId).
- [ ] Ремонт на любой глубине: nested managed refs, `[Serializable]`-контейнеры, элементы списков; очистка осиротевших nested-ссылок в Prefab Mode.
- [ ] YAML-editor hardening: отказ писать в не-Unity YAML (нет `%TAG !u!`), bail на tab/mixed-индентации.
- [ ] Graph-вью отказывается от YAML-rewrite, пока ассет открыт в сцене/Prefab Mode.

## 5. Окно SerializeReferenceWindow (4 вкладки)

- [ ] **Welcome**: авто-показ раз на версию пакета (и после апдейта), toggle Auto-show, список сэмплов из package.json, меню работает всегда.
- [ ] **Asset References**: граф по YAML — roots/nested/shared/orphaned, бейджи `MISSING`/`SHARED`, rid-цвета, `<None>`-слоты, полный field-path; inline Fix (констрейнутый), Clear на orphaned, Open Source Prefab; pending-migration карточка (info-pill, `Migrate → Type`), headline считает миграции отдельно; персист вкладки и ассета через tab-switch и domain reload.
- [ ] **Project References**: Scan Project по `.prefab`/`.asset`/`.unity`, группировка по типу, `Fix all (N)` с confirm + diff-preview + Undo (undo-квитанция ревертит только неизменённые записи и репортит фактическое число), Smart Fix quick-apply на группе, skip открытых сцен/Prefab Mode, `Migrate all (N) → Type` для [MovedFrom]; линк из результата в Asset References.
- [ ] **Settings**: см. раздел 6.
- [ ] Ctrl+Tab / Ctrl+Shift+Tab — переключение вкладок.

## 6. Settings — три зеркала

> Проверить на **каждой** из трёх поверхностей: вкладка Settings окна, `Preferences → Aspid FastTools`, `Project Settings → Aspid FastTools → SerializeReference` (последняя — только References, нативный вид).

- [ ] Зеркала синхронны live (изменение на одной поверхности видно на другой), переживают dock-move и клики по свитчу.
- [ ] Scope-полоски (зелёная = ProjectSettings, синяя = EditorPrefs) + легенда; Reset to defaults раздельно per-scope с confirm, называющим дефолты; Favorites/Recent переживают reset.
- [ ] References: auto de-alias, breakage detection (per-user), gate severity Off/Warn/Fail (shared asset), excluded folders (список + селектор).
- [ ] Type Selector: hide Favorites, Recent capacity, Saved lists maintenance (confirm с количеством).
- [ ] Appearance: theme override StyleSheet (live, per-project), Create template…; Welcome: auto-show.
- [ ] `SerializeReferenceSharedSettings.asset` коммитится и работает на «чужой» машине/CI.

## 7. Индекс, поиск, защита

- [ ] Usage-индекс строится инкрементально на импорте; повторный Scan Project почти мгновенный; неудачный warm-up сбрасывает индекс в cold.
- [ ] Find Usages: контекст-меню поля и `sr:`-провайдер Quick Search (только explicit — обычный поиск индекс не греет), пинг ассетов.
- [ ] Delete-guard: удаление скрипта / **папки** скриптов с используемыми SR-типами — предупреждение с count и примерами, отмена работает; свипы — чистый текст-скан (ассеты не загружаются).
- [ ] Breakage detection: rename/delete → один toast с deep-link в Repair; pre-existing миссы не алармят; выключение/включение re-baseline'ит; классификация [MovedFrom]-rename vs настоящий breakage.
- [ ] Циклические графы: alias-walk, Link-to-Existing scan, CI-walk не зависают.

## 8. Batch-миграция [MovedFrom]

- [ ] `SerializeReferenceMovedFromResolver`: авторитетный резолв, отказ при неоднозначности и закрытых генериках.
- [ ] Pending migration ≠ violation для build/CI gate.
- [ ] Migrate all: confirm + diff + undo, гейт по констрейнту поля; после bake атрибут можно удалить (YAML больше не хранит старое имя).
- [ ] Сэмпл-демо: `RenamedWeaponPreset.asset` (Crossbow) — flow Migrate all; `MovedWeaponPreset.asset` — Smart Fix `→ Pistol?`.

## 9. Build / CI gate

- [ ] `IPreprocessBuildWithReport`: Warn — лог без фейла; Fail — билд падает при нарушениях; Off — пропуск.
- [ ] Headless `SerializeReferenceCiGate.RunCheck`: коды выхода 0/1, отчёт по пути `-srGateReport`, `-srGateRequired` покрывает prefabs + SO + **сцены** (pure-YAML по `m_Script` guid), флаги `-srGateWarnOnly` / `-srGateFail` перекрывают committed severity.

## 10. Analyzers и Generators

- [ ] `AFT0004` (error): `[SerializeReference]`+`[TypeSelector]` на UnityEngine.Object-наследнике.
- [ ] `AFT0005` (warning): пустой список кандидатов; производительность candidate-scan не регрессировала.
- [ ] `ProfilerMarkersGenerator`: `this.Marker()` уникален per (class, method, line), маркеры видны в Profiler.
- [ ] `IdStructGenerator`: генерация boilerplate, диагностики `AFID001`/`AFID002`.
- [ ] Инкрементальный кэш не ломается при правках (IncrementalCacheTests зелёные).

## 11. Остальные фичи пакета

- [ ] **EnumValues\<TValue\>** *(2×UI)*: обычные и `[Flags]` enum, ключ не сбрасывается при редактировании, добавление/удаление записей.
- [ ] **EnumValues\<TEnum, TValue\>** *(2×UI)*: в инспекторе нет строки выбора типа, строки сразу рендерят типизированные enum-поля, «Populate Missing Enum Members» работает, смена типа поля с `EnumValues<TValue>` (тот же enum) сохраняет сериализованные данные.
- [ ] **Сэмпл EnumValues**: импорт через Package Manager без ошибок; сцена `EnumValuesTutorial` открывается, TUTORIAL-шаги проходимы (пикер типа, Populate Missing Enum Members, правила `[Flags]`-поиска, `Log Tutorial Lookups`, типизированный шаг `EnumValues<TEnum, TValue>` без строки выбора типа); демо-сцена `EnumValues` печатает ожидаемую строку по Space. *(IMGUI-путь: `Assets/DevTests/Enums/EnumValuesDevTest.prefab` в dev-проекте — принудительно IMGUI-инспектор рядом с UIToolkit-двойником, в пакет не входит.)*
- [ ] **Id Registries**: создание через `Assets → Create → Aspid → Id Registry`, `TryGetId`/`TryGetName`/`Contains`, привязка IId-структа ровно к одному реестру (IdRegistryResolver), валидация и мутации в редакторе.
- [ ] **Сэмпл Ids**: импорт через Package Manager без ошибок; сцена `IdsTutorial` открывается, TUTORIAL-шаги проходимы (дропдаун имён из реестра, добавленная строка реестра появляется в дропдауне, предупреждение `[UniqueId]` на дубликате ассета, `Log Tutorial Lookups` покрывает `TryGetId`/`TryGetName`/`Contains`/итерацию и попадание по каталогу); демо-сцена `Ids` логирует найденный `EnemyDefinition` по Play без ручной настройки. *(IMGUI-путь: `Assets/DevTests/Ids/IdsDevTest.prefab` в dev-проекте — принудительно IMGUI-инспектор рядом с UIToolkit-двойником, в пакет не входит.)*
- [ ] **Сэмпл ProfilerMarkers**: импорт через Package Manager без ошибок; сцена `ProfilerMarkersTutorial` открывается, TUTORIAL-шаги проходимы в Play Mode — маркеры `ProfilerMarkersTutorial.*` видны в Profiler'е: именованные `Physics`/`Render`, автоименованный `SimulateInput`, вложенный `AI → AI.Agent`, и две записи `SimulateAudio`, различаемые по строке; демо-сцена `ProfilerMarkers` даёт те же маркеры по Play.
- [ ] **Сэмпл VisualElements**: импорт через Package Manager без ошибок; сцена `VisualElementsTutorial` открывается, пять карточек STEP рендерятся (без Play Mode) — fluent-образец стиля, пресеты шрифта (обычный/жирный/курсив/разрядка), строка-шапка из Ability Name, реактивный бейдж Mana Cost, переключающийся на **FREE** при `0`, и STEP 5 ProgressBar/HelpBox/Button, реагирующие на Charge (полоса заполняется, HelpBox появляется на 100%, **Log charge** выводит значение); демо-инспектор `AbilityConfig` по-прежнему показывает карточку и бейдж FREE/warning.
- [ ] **Сэмпл Types**: импорт через Package Manager без ошибок; сцена `TypesTutorial` открывается, шаги TUTORIAL проходимы (STEP 1 picker `SerializableType<Ability>` над `Ability` и его наследниками — включая абстрактную базу, `TypeAllow.All`; STEP 2 pickers элементов `[TypeSelector]` `string[]`, ограниченные `AbilityModifier`; STEP 3 picker предлагает открытый generic `StackModifier<T>` — при выборе открывается вторая страница для `T`, хранится закрытый `StackModifier<float>`; STEP 4 пустая строка `[TypeSelector(Required = true)]` показывает инлайн-notice «Required type is not set» и питает гейт обязательных полей; STEP 5 дропдаун `ComponentTypeSelector` у Enemy меняет `FastEnemy`↔`TankEnemy` по месту с сохранением `Health`; `Log Tutorial Lookups` логирует резолвнутые типы ability/generic/required/enemy); демо-сцена `Types` на Play логирует активированную способность + модификаторы и меняет скрипт Enemy из дропдауна Inspector. *(IMGUI-путь: `Assets/DevTests/Types/Prefabs/TypesDevTest.prefab` в dev-проекте — принудительно-IMGUI инспекторы `SerializableType`/`[TypeSelector]` рядом с их UIToolkit-двойником плюс принудительно-IMGUI враг `ComponentTypeSelector`, не поставляется с пакетом.)*
- [ ] **SerializedProperty Extensions**: `.SetValue()`, `.Apply()`, chain-вызовы, reflection-хелперы.
- [ ] **VisualElement Extensions**: выборочная проверка fluent API (layout/style/borders/callbacks/USS/child) + Math-satellite (`float2/3/4` INotifyValueChanged) при установленном Mathematics.
- [ ] **IMGUI Scopes**: Vertical/Horizontal/ScrollView — Rect-свойства, корректный Dispose.
- [ ] **MonoScript extensions**: `GetScriptName()` с `[AddComponentMenu]`, индекс-суффикс у дубликатов.
- [ ] **SerializableType**: code-constructed инстанс не кидает на `Type` (null stored name).

## 12. Сэмпл SerializeReferences

- [ ] Импорт через Package Manager без ошибок; сцена `TypeSelectorTutorial` открывается, TUTORIAL шаги проходимы.
- [ ] Pre-broken ассеты в рабочем состоянии «сломанности»: `Ghost*` (ручной Fix — inline / whole-asset / project sweep), `MovedWeaponPreset` (Smart Fix), `RenamedWeaponPreset` (Migrate all).
- [ ] Оба инспектора представлены (UIToolkit и IMGUI-варианты компонентов); комментарии в коде соответствуют поведению.

## 13. Окружения и совместимость

- [ ] Минимальная поддерживаемая Unity (6000.0) и актуальный Unity 6.x: пакет компилируется без ошибок/ворнингов, окно и сэмпл работают.
- [ ] Проект **без** `com.unity.mathematics`: satellite-ассембли выключается, ошибок нет; проект **с** ним: extensions доступны.
- [ ] Light-тема редактора: свитчи, палитра, нотисы читаемы.
- [ ] Плеер-билд собирается: `Unity/Runtime` не тянет `UnityEditor`.

## 14. Автотесты

- [ ] Unity EditMode: `Aspid.FastTools.Unity.Editor.Tests` + `…SerializeReferences.Tests` — зелёные.
- [ ] `dotnet test` для Generators — зелёный.
- [ ] `dotnet test` для Analyzers (сабмодуль) — зелёный.
- [ ] Компиляция проекта без warnings от пакета.

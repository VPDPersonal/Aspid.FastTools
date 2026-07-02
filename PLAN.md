# План доработок UI/UX

> Живой документ. Пополняется по ходу. Статусы: `[ ]` — не начато · `[~]` — в работе · `[x]` — готово.

## 1. [x] «Make unique» — переместить в самый низ

> **Готово (2026-07-02):** IMGUI синхронизирован — shared-плашка рисуется **после** `DrawChildren` (`SerializeReferenceIMGUIPropertyDrawer.cs:278` → `:310`). Паритет с UIToolkit полный: там все плашки живут в host `_notices`, вставленном **после** `_content` (`SerializeReferenceField.cs:161`), но missing/required/mixed рендерятся только когда значения нет (детей нет) — так что фактически «под дочерними полями» оказывается только shared, как и в IMGUI. Это и зафиксировано как замысел в комментарии у IMGUI-отрисовки.

Сейчас плашка `Shared reference — Make unique` рисуется **над** дочерними полями объекта (Damage, Magazine Size). Нужно перенести ссылку `Make unique` в самый низ — под дочерние поля.

- **UIToolkit:** `Unity/Editor/Scripts/SerializeReferences/VisualElements/SerializeReferenceField.cs`
  - плашку строит `UpdateSharedBox()` (вызывает `_sharedNotice.Set(... "Make unique", onAction: MakeUnique)`).
  - сейчас `_sharedNotice` кладётся в host `_notices`, который вставлен **до** `_content` (`_foldout.hierarchy.Insert(... IndexOf(_content), _notices)`).
  - **Правка:** для *shared*-плашки сделать отдельный host/вставку **после** `_content` (под дочерние поля). Missing/required/mixed-плашки оставить сверху.
- **IMGUI (держать в синхроне):** `Drawers/SerializeReferenceIMGUIPropertyDrawer.cs` — `DrawNotice("Shared reference —", "Make unique")` рисуется **до** `DrawChildren(...)`. Перенести после, поправить высоты в `GetPropertyHeight`.
- USS не трогаем (только позиция в дереве): `Resources/UI/SerializeReferences/Aspid-FastTools-SerializeReference.uss`.

## 2. [x] Auto-fix «Missing type — Fix» — добавить в пример

> **Готово (2026-07-01):** добавлены «битые» ассеты (`LoadoutMissingType.prefab` + IMGUI-двойник, `BrokenWeaponPreset.asset`, `BrokenArsenalPreset.asset`), README/TUTORIAL описывают Fix-флоу и проектный свип.
>
> **Дополнено (2026-07-02):** у `Ghost*`-имён нет кандидата ближе Levenshtein ≤ 2 и нет `[MovedFrom]`, поэтому одно-кликовая **Smart Fix**-подсказка в примере не всплывала вовсе — демонстрировался только ручной Fix. Добавлен `Presets/MovedWeaponPreset.asset`: хранит `Pistol` под старым namespace `…Samples.SerializeReferences.Legacy` (перенос без `[MovedFrom]`) → «same type name» 0.8 + бонус за совпадение полей `_damage`/`_magazineSize` = 1.0 → гарантированная подсказка `→ Pistol?`. Вариант с реальным `[MovedFrom]` не годится: Unity нативно мигрирует managed reference по атрибуту при загрузке, ассет не был бы «битым». README/TUTORIAL (EN+RU) дополнены секцией Smart Fix.

Функция авто-починки (`Missing type — Fix`) есть в дровере, но её нет в демонстрационном примере. Нужно завести в сэмпле сценарий с отсутствующим/переименованным типом и показать кнопку Fix.

- **Код фичи:** `SerializeReferences/VisualElements/SerializeReferenceField.cs` → `UpdateMissingBox()` / `OpenFixSelector()` → `SerializeReferenceHelpers.ShowFixTypeSelector(...)`; подсказки — `Extensions/SerializeReferenceRepairSuggestions.cs`.
- **Пример (куда добавить):** `Packages/tech.aspid.fasttools/Samples~/SerializeReferences/`
  - скрипты: `Scripts/WeaponPreset.cs`, `Loadout.cs`, `Weapons/*` — завести намеренно «битый»/переименованный тип.
  - доки: `TUTORIAL.md` / `TUTORIAL_RU.md` / `README.md` / `README_RU.md` (там уже упоминается Fix — дополнить разделом примера).
  - ⚠️ Правим **источник** в `Packages/.../Samples~`, а не импортированную копию в `Assets/Samples/...`.
- Welcome-панель (`Welcome/WelcomeView.cs`) — это список сэмплов, не список фич; отдельно править не нужно.

## 3. [x] Settings-таб — привести к общему стилю

> **Готово (2026-07-02):** тогглы переведены на `AspidSwitch` (auto de-alias, breakage), folder-list переделан (см. п.6), Rid-colours toggle убран. `EnumField` «Build / CI gate» застилен — правила `.aspid-fasttools-settings .unity-enum-field*` (input/text/arrow, hover/focus) в `UI/Windows/Aspid-FastTools-Settings.uss:260-310`. Рестайл скоуплен корневым классом `AspidSettingsUI.RootClass` (`aspid-fasttools-settings`), так что в чужие окна/Project Settings не протекает.

Контролы на вкладке Settings (тогглы `Rid colours`, `Auto de-alias duplicated list elements`, `Breakage detection`, дропдаун `Build / CI gate`, textarea `Excluded scan folders`) — голые Unity-контролы без Aspid-классов, выбиваются из общего оформления окна.

- **View:** `SerializeReferences/Windows/SettingsView.cs` (только заголовок `AspidLabel("Settings")` + `ScrollView`, тему/классы не добавляет).
- **Контролы:** `SerializeReferences/Settings/SerializeReferenceSettingsUI.cs` → `BuildControls(...)` — `Toggle`/`EnumField`/`TextField` без классов.
- **USS:** выделенного стиля нет; окно стилизует `Resources/UI/SerializeReferences/Aspid-FastTools-SerializeReference-Window.uss` (только табы).
- **Правка:** навесить Aspid-классы/добавить settings-селекторы (`.unity-toggle`/`.unity-enum-field`/`.unity-text-field` под контейнером настроек).
  - ⚠️ `BuildControls` шарится с Project Settings (`SerializeReferenceSettingsProvider`) — рестайл скоупить на in-window таб, чтобы не протекло в Project Settings.

## 4. [x] TypeSelector — заголовок в dropdown не должен быть закруглён

> **Готово (2026-07-01):** `border-radius` полностью удалён из правила `.aspid-fasttools-type-selector__header` — углы прямые.

Шапка `Select Type` (с поиском) в режиме dropdown имеет закруглённые верхние углы — нужно сделать прямые.

- **View:** `Types/Selectors/TypeSelectorView.cs` → `CreateHeader()`, класс `aspid-fasttools-type-selector__header`.
- **USS:** `Resources/UI/Types/Aspid-FastTools-TypeSelector.uss` → `.aspid-fasttools-type-selector__header`, строка `border-radius: 8px 8px 0 0;`.
- **Host:** `Types/Selectors/TypeSelectorWindow.cs` → `Show(...)` (`ShowAsDropDown`). View также встраивается инлайн в других местах.
- **Правка:** добавить модификатор-класс при показе в окне (`TypeSelectorWindow.Show`) и в USS переопределить `border-radius: 0` для шапки под этим модификатором — чтобы инлайн-встраивание сохранило скругление. Если скругление нигде не нужно — просто заменить на `0`.

## 5. [x] Циклическое переключение вкладок: Ctrl+Tab / Ctrl+Shift+Tab (+ macOS)

> **Готово (2026-07-02):** добавлены два `[Shortcut]` — `Next Tab` (Ctrl+Tab) и `Previous Tab` (Ctrl+Shift+Tab) в `SerializeReferenceWindow.cs`, оба на `ShortcutModifiers.Control` (физический Ctrl на обеих платформах). Цикл идёт по порядку объявления enum `Mode` (совпадает с тулбаром) с wrap-around через `CycleFrom(args, ±1)`. ✅ Риск «`ShortcutManager` не отдаёт `KeyCode.Tab`» проверен вживую 2026-07-02 — шорткаты работают, fallback не понадобился.

В окне Aspid.FastTools `Ctrl+Tab` должен листать вкладки слева направо, `Ctrl+Shift+Tab` — в обратную сторону. На macOS — работать так же.

- **Где:** `SerializeReferences/Windows/SerializeReferenceWindow.cs`
  - вкладки уже переключаются через `[Shortcut(..., typeof(SerializeReferenceWindow), KeyCode.AlphaN, ShortcutModifiers.Alt)]` + `SwitchFrom(args, Mode)` → `SwitchMode(Mode)`.
  - порядок слева направо: `Mode.Welcome` (Home) → `Mode.Inspect` (Asset References) → `Mode.Project` (Project References) → `Mode.Settings` (⚙). Цикл с заворотом.
  - **Правка:** добавить два новых `[Shortcut]` — next/prev — которые берут текущий `_mode`, считают следующий/предыдущий по этому порядку (с wrap-around) и зовут `SwitchMode(...)`.
- **macOS — ключевой нюанс:** использовать `ShortcutModifiers.Control` (физический Ctrl) на **обеих** платформах, **НЕ** `ShortcutModifiers.Action`. `Action` маппится на ⌘ в macOS, а `Cmd+Tab` зарезервирован ОС под переключение приложений. `Ctrl+Tab` одинаково работает и на Win, и на Mac.
- **Риск (проверить):** `KeyCode.Tab` — навигационная клавиша; `ShortcutManager` может её не отдать / focus-навигация перехватит. Если так — fallback: `RegisterCallback<KeyDownEvent>(..., TrickleDown.TrickleDown)` на `rootVisualElement` (перехват до focus-nav). Минус fallback — описан в комментарии у `[Shortcut]`: KeyDown молчит, когда фокус ушёл на пустой хром окна.
- Бейджи/тултипы (`BindingLabel`/`ShortcutHint`) для cycle-шортката не обязательны; биндинги и так попадут в Edit > Shortcuts и будут переназначаемыми.

## 6. [x] «Excluded scan folders» — список + селектор папки

> **Готово (2026-07-01):** реализован отдельным классом `SerializeReferenceExcludedFoldersField` — add/remove, folder picker, click-to-edit, zebra-строки, hover-тинты (красный delete / зелёный add). Пошли не через `ListView`, а через кастомный стек строк.

Сейчас «Excluded scan folders» — голое multiline `TextField` (путь на строку). Заменить на список путей с кнопкой-селектором папки (folder picker) на каждой строке + add/remove.

- **Где:** `SerializeReferences/Settings/SerializeReferenceSettingsUI.cs` → `BuildControls(...)`, строки 60–73 (Label + multiline `TextField`).
- **Хранилище:** `SerializeReferenceSettings.ExcludedFolders` — уже `string[]`, формат менять не нужно (раньше склеивался/резался по `\n`).
- **Правка:** заменить на `ListView`, забинденный на список путей:
  - `showAddRemoveFooter = true` для add/remove; `makeItem` — строка `TextField` (ручной ввод) + кнопка «обзор» (иконка папки); `bindItem` пишет в элемент массива.
  - селектор: `EditorUtility.OpenFolderPanel("Exclude folder", "Assets", "")` → привести абсолютный путь к project-relative (`FileUtil.GetProjectRelativePath` / срез по `Application.dataPath`); отбросить выбор вне проекта.
  - на любое изменение списка — переписывать `SerializeReferenceSettings.ExcludedFolders` (как сейчас делает `RegisterValueChangedCallback`).
- **Sync:** текущий `SyncFromSettings(...)` рассчитан на `INotifyValueChanged<T>`-контролы; `ListView` под него не подходит — для live-синхрона между in-window табом и Project Settings подписаться на `SerializeReferenceSettings.Changed` отдельно и пересобирать `itemsSource` (с пропуском, если строку сейчас редактируют — как уже сделано для multiline-поля).
- **Связано с п.3** — это тот же файл/метод; стилизацию (п.3) и переделку в список делать заодно, чтобы новый `ListView` сразу получил Aspid-оформление.

## 8. [x] Вынести YAML-движок в отдельную internal-ассембли (подготовка к будущей фиче)

> **Факт на ветке (2026-07-01):** извлечение выполнено — YAML живёт в своей asmdef `Aspid.FastTools.Unity.Editor.SerializeReferences.Yaml` (`SerializeReferences/Yaml/`), дублей в `Extensions/` не осталось. Реализовано **SR-scoped**, а не как общий движок `…Editor.Yaml`. Генерализация имён + 2-й потребитель (`m_Script`-GUID repair) → отдельный будущий заход (см. «Будущее» ниже).

> Архитектурное, не UI/UX. Цель: изолировать самодостаточный YAML-движок так, чтобы он **мог** позже стать отдельной фичей (`if we want it`), и влить его в `main` отдельной веткой раньше большой SR-ветки. Тут (`feature/serialize-reference-dropdown`) после этого фокусируемся на SerializeReference.

> **Статус:** Извлечение сделано и запушено. Ветка `feature/unity-yaml-editor` от `origin/main` (`84a67e1`), коммит `f828ef2` (32 файла) — модуль в internal-ассембли `Aspid.FastTools.Unity.Editor.Yaml` + тест-ассембли. **PR #87 → main открыт** (`type: refactor`, `area: editor`, `status: needs-review`): https://github.com/VPDPersonal/Aspid.FastTools/pull/87. Собран в изолированном git worktree `C:/Users/user/yaml-wt` (фича-ветка не тронута). **Осталось:** (1) проверить компиляцию в Unity в worktree, (2) ревью/мерж PR #87, (3) после влития — rewire-фаза на фича-ветке (см. ниже). Дубли YAML на фича-ветке пока на месте — удалятся на rewire.

**Почему это движок, а не часть SR:** ~60–70% кода — parser-free правка Unity-YAML, не привязанная к managed references. Критерий превращения в публичную фичу — **появление 2-го потребителя** (самый реальный — repair битых `m_Script` GUID-ов: та же природа, Unity не отдаёт старый GUID через API). Пока потребитель один (SR) → выносим как **internal**-инфраструктуру, без генерализации имён.

**Факты зависимостей (проверено):**
- Входящих зависимостей YAML→SR в коде **нет** (только doc-коммент про `SerializeReferenceHelpers.StoredTypeResolves` и литералы `[TypeSelector]` в `Debug.LogError`). Самодостаточна.
- Исходящих (кто зовёт YAML): 11 файлов SR; `ManagedTypeName` — в 7 из них. Имена при выносе **не меняем** → потребители правятся минимально (namespace сохраняем, добавляем asmdef-ссылку).

**Объём (решение: «перенести как есть»):** 4 файла из `Unity/Editor/Scripts/SerializeReferences/Extensions/`:
`SerializeReferenceYaml.cs`, `SerializeReferenceYamlEditor.cs` (тут же `ManagedTypeName`, `RefIds`, `Missing/RequiredViolationEntry`, `RewriteEdit`, `RequiredFieldDescriptor`), `SerializeReferenceYamlProbeCache.cs`, `SerializeReferenceYamlProbeCacheInvalidator.cs`.

**Ветка и git-стратегия (ВАЖНО):**
- Базироваться от **`origin/main` (`84a67e1`)**, НЕ от локального `main` — локальный устарел на 292 коммита (там до-реструктурный layout `Aspid.UnityFastTools/Assets/Plugins/...`). `origin/main` уже несёт новый UPM-layout + ассембли `Aspid.FastTools.Unity.Editor`, но без YAML и без папки `Tests/`.
- HEAD = `origin/main` + 170 коммитов SR. Новая ветка `feature/unity-yaml-editor` от `84a67e1` → PR чисто аддитивный → быстро в `main`.

**Что создаётся на YAML-ветке:**
- Папка `Unity/Editor/Scripts/Yaml/` + asmdef `Aspid.FastTools.Unity.Editor.Yaml` (Editor-only, `references: []` — зависит только от UnityEngine/UnityEditor).
- `AssemblyInfo.cs` с `[assembly: InternalsVisibleTo("Aspid.FastTools.Unity.Editor")]` (будущий потребитель — на `origin/main` ассембли есть, просто пока не зовёт) и `[assembly: InternalsVisibleTo("Aspid.FastTools.Unity.Editor.Yaml.Tests")]`. Типы остаются `internal`.
- Тест-ассембли `Aspid.FastTools.Unity.Editor.Yaml.Tests` (`Tests/Editor/Yaml/`) + 6 «чистых» YAML-тестов + `YamlFixtures.cs`:
  `…YamlEditorTests`, `…YamlEditorReadEdgeTests`, `…YamlEditorRewriteTests`, `…YamlEditorHardeningTests`, `…YamlEditorNullReferenceTests`, `…YamlProbeCacheTests`.
- Переезжающие файлы тащим **вместе с их `.meta`** (сохранить GUID); для новых (asmdef/AssemblyInfo/папки) — генерим `.meta` с новыми GUID.

**Что остаётся на SR-ветке (интеграционные тесты, цепляют SR-специфику):**
- `SerializeReferenceRequiredYamlTests` (→ `SerializeReferenceRequiredGate`, `RequiredFieldDescriptor`, `Aspid.FastTools.Types`).
- `SerializeReferenceManagedTypeNameTests` (→ `SerializeReferenceHelpers`).
- ⚠️ Проверить cross-use `YamlFixtures` этими двумя — если используют, на rewire-фазе либо отдать им ссылку на тест-ассембли, либо держать локальную копию фикстур.

**Rewire-фаза (ПОТОМ, после влития YAML-ветки в main), на `feature/serialize-reference-dropdown`:**
1. Смержить `origin/main` (подтянуть YAML-ассембли).
2. Удалить дубли YAML-исходников из `…/SerializeReferences/Extensions/` и 6 «чистых» тестов из `Tests/Editor/SerializeReferences/`.
3. Добавить в `Aspid.FastTools.Unity.Editor.asmdef` ссылку `Aspid.FastTools.Unity.Editor.Yaml`; в SR Tests asmdef — тоже.
4. Прогнать сборку/тесты в Unity; убедиться, что 11 потребителей видят `internal`-типы (через `InternalsVisibleTo`).

**Будущее (отдельным заходом, when we want it):** генерализация имён (`SerializeReferenceYaml*` → `UnityYaml*`/`AssetYaml*`), split 1352-строчного `SerializeReferenceYamlEditor` на generic-ядро + SR-надстройку, и 2-й потребитель — `m_Script`-GUID repair — как триггер промоушена в публичную фичу.

## 7. [x] TypeSelector — выделение выбранной строки слишком тусклое

> **Готово (2026-07-01):** `item--selected` поднят до `--aspid-colors-shade-light` — выделение чётко отрывается от ховера (пошли путём «более светлый нейтральный тон», без зелёного акцента).

В выпадающем списке типов выделенная строка едва отличима от ховера — читается как «тусклый» селект.

- **Где:** `Resources/UI/Types/Aspid-FastTools-TypeSelector.uss` → `.unity-collection-view__item--selected` (строки 190–193).
- **Причина:** ховер = `--aspid-colors-bg-light` (`#2E2E2E`), выделение = `--aspid-colors-bg-lightness` (`#383838`) — разница ~10 уровней серого, практически неразличима. Вдобавок комментарий (стр. 173–177) обещает «зелёный акцентный рельс» на выбранной строке, но в самом правиле его нет — меняется только фон.
- **Правка:** усилить контраст выделения — варианты:
  - вернуть обещанный зелёный акцент: левый рельс/бордер в `--aspid-colors-status-success-*` (есть `…-success-darkness/dark/light` и `…-success-text-*`), либо зелёно-подкрашенный фон строки;
  - либо просто поднять фон выделения на более светлый/контрастный тон (например `--aspid-colors-shade-light` `#505050`), чтобы оно явно отрывалось от ховера.
- **Связано с п.4** — тот же USS-файл TypeSelector; правки по селектору логично делать одним заходом.

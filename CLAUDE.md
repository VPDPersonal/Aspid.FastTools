## Обзор репозитория

**Aspid.FastTools** — Unity-пакет (`tech.aspid.fasttools`), минимизирующий рутинный шаблонный код. Три компонента:

1. **`Aspid.FastTools/`** — Unity-проект с исходниками пакета (Runtime + Editor)
2. **`Aspid.FastTools.Generators/`** — отдельное .NET-решение с генераторами исходного кода Roslyn; паттерны конвейера и детали по каждому генератору — в `Aspid.FastTools.Generators/CLAUDE.md`
3. **`Aspid.FastTools.Analyzers/`** — отдельное .NET-решение с анализаторами Roslyn, проверяющими использование атрибутов пакета (диагностики `AFT*`)

Внутренние рабочие документы репозитория (roadmap, чек-лист релиза, `QA-CHECKLIST.md`/`QA-CHECKLIST_RU.md`, `DESIGN.md`) живут в `docs/` — в отличие от пользовательской `Documentation/` пакета. Новая функциональность обязана добавить свой пункт в QA-чек-лист на **обоих** языках до того, как её ветка будет влита.

### Сборка

У самого Unity-пакета нет CLI-сборки — Unity компилирует его, когда проект открыт. Обе Roslyn-DLL поставляются внутри пакета уже собранными; скиллы `build-generator` / `build-analyzer` хранят точные команды сборки/тестов/развёртывания (хуки PostToolUse также пересобирают их автоматически при редактировании — см. *Локальная автоматизация Claude Code*).

- **Генератор** (`Aspid.FastTools.Generators/`): при сборке `ILRepack.targets` сливает зависимости `Aspid.Generators.Helper*` в однофайловую DLL, а `Directory.Build.targets` автоматически копирует её в Unity-пакет. Никогда не ссылайся на `SourceGenerator.Foundations` — его внедряемое логирование через `Console` вводит сервер компиляции Unity во взаимную блокировку.
- **Анализатор** (`Aspid.FastTools.Analyzers/`): `Directory.Build.targets` автоматически копирует DLL в Unity-пакет. Копирование намеренно происходит **только для Release** — проекты Tests и Sample ссылаются на анализатор, поэтому прогон `dotnet test` в Debug иначе затёр бы поставляемую Release-DLL.
- Закоммиченные файлы `*.dll.meta` несут метку `RoslynAnalyzer` со всеми исключёнными платформами. Префиксы идентификаторов диагностик: анализатор — `AFT*`, генератор — `AFID*`.

## Архитектура

### Сборки (корень пакета: `Aspid.FastTools/Packages/tech.aspid.fasttools/`)

| Сборка | Расположение | Назначение |
|---|---|---|
| `Aspid.FastTools` | `Source/` | Чистый C#, без зависимости от Unity |
| `Aspid.FastTools.Unity` | `Unity/Runtime/` | Поставляется со сборками игры |
| `Aspid.FastTools.Unity.VisualElements.Math` | `Unity/Runtime/VisualElements/Extensions/INotifyValueChanged/Math/` | Сателлит: `INotifyValueChanged` для `float2/3/4` и т.д. |
| `Aspid.FastTools.Unity.Editor` | `Unity/Editor/Scripts/` | Только редактор, исключается из сборок |
| `Aspid.FastTools.Unity.Editor.SerializeReferences.Yaml` | `Unity/Editor/Scripts/SerializeReferences/Yaml/` | Разбор YAML ассетов, изолирован намеренно |

Плюс: `Tests/Editor/` (Unity Test Runner), `Samples~/` (соглашение UPM с тильдой — импортируется через Package Manager), `Unity/Editor/Resources/UI|Icons/`.

**Правило границы сборок:** код в `Unity/Runtime/` НЕ должен ссылаться на `UnityEditor` — он поставляется со сборками игры.

**Опциональная интеграция с Mathematics:** новый код, зависящий от Mathematics, размещается в сателлитной сборке `Aspid.FastTools.Unity.VisualElements.Math`, компилируемой только когда установлен `com.unity.mathematics` (через `versionDefines`, объявляющий `ASPID_FASTTOOLS_UNITY_MATHEMATICS_INTEGRATION`). Только asmdef сателлита объявляет этот символ — основной runtime-asmdef этого не делает.

### Карта функциональности

Папки функциональности внутри `Unity/Runtime/` и `Unity/Editor/Scripts/` названы по самой функциональности (`Enums`, `Ids`, `ProfilerMarkers`, `Types`, `VisualElements`, `IMGUI`, `SerializedProperties`, `Settings`, `Windows`, `SerializeReferences`, `Extensions`) — `ls` найдёт нужное быстрее, чем этот файл сможет всё перечислить. Только то, о чём структура папок *не* говорит:

| Функциональность | Неочевидные детали |
|---|---|
| ProfilerMarkers | `this.Marker()` возвращает уникальный для места вызова `ProfilerMarker` — генератор исходников выпускает по одному на (класс, метод, строку) |
| TypeSelector | Один атрибут, две формы поля — `string` (AQN) и управляемая ссылка `[SerializeReference]`. **Путь управляемых ссылок живёт в `SerializeReferences/`, а не в `Types/`.** Детали: `Unity/Editor/Scripts/Types/CLAUDE.md` |
| Инструментарий SerializeReference | Всё, что стоит за поверхностью инспектора — `Drawers/`, `VisualElements/` (поля + промпт имени), `Index/`, `Diagnostics/`, `Settings/`, `Editing/`, `Yaml/` (собственный asmdef). **Окно, которое всё это представляет, живёт в `Unity/Editor/Scripts/Windows/`, а не здесь** |
| Правки SerializeReference | `SerializeReferences/Editing/` владеет каждой мутацией, свободен от UI и вызываем из тестов: `SerializeReferenceGraphEditor` (по одной записи за раз), `SerializeReferenceBatchEditor` (пакеты по файлам), `MissingReferenceGroup` (модель сканирования проекта), `SerializeReferenceOpenCopyGuard` (никогда не переписывать файл, чья открытая копия его затрёт), `SerializeReferenceConstraintCache`. **Представление никогда не редактирует ассет само** — оно обращается сюда и решает лишь, перерисовываться ли |
| Окно редактора и вкладки | `Unity/Editor/Scripts/Windows/` — `SerializeReferenceWindow` (меню `Tools/Aspid 🐍/FastTools/…`) плюс каждая размещаемая в нём вкладка, по одной подпапке на каждую: `Welcome/`, `References/`, `Settings/`. Каждое представление сообщает окну `StatusStyle.Type`, а окно заливает им общий точечный холст — цвета заливки живут в USS компонента холста, никогда в коде представлений. Навигация с клавиатуры — общий `NavRing` |
| Вкладка References | `Windows/References/` делится натрое: `Asset/` и `Project/` содержат по одному представлению аудита — `partial` на каждую зону ответственности (обвязка+сканирование / карточки / пикер или действия) плюс чистые `*Summary` (тексты) и `*Analysis` (подсчёт), — а `Shared/` содержит то, что носят оба: `SerializeReferenceAuditUI` (формулировки счётчиков, выделяемые строки, легенда, янтарно-синий вердикт `ResolveStatus`), `AuditPickerHost` (стыковка встроенного пикера), `ManagedReferenceFilter`, `ViolationFieldLabels`. Добавление формы карточки означает новый `Build*` в partial-файле карточек вкладки, а не новую ветку в представлении |
| Реестры Id | Охватывают `Unity/Runtime/Ids/` + `Unity/Editor/Scripts/Ids/`. `IdRegistry` (ScriptableObject) сопоставляет имена со стабильными целочисленными ID; каждая структура `IId` привязывается ровно к **одному** реестру (обеспечивается `IdRegistryResolver`); `IdStructGenerator` выпускает шаблонный код структур. Внутренности редактора: `Unity/Editor/Scripts/Ids/CLAUDE.md` |
| Settings / Preferences | Настройки каждой функциональности живут рядом с ней; `AspidFastToolsPreferencesProvider` + `AspidSettingsUI` и вкладка **Settings** окна только агрегируют их |
| Внутренние компоненты редактора | Строгая четырёхчастная структура на компонент (элемент + `{Name}Preset` + fluent-расширения + `Styles/`) — следуй ей при добавлении нового. Соглашения: `Unity/Editor/Scripts/VisualElements/Internal/CLAUDE.md` |
| Расширения VisualElement | Runtime fluent API в `Unity/Runtime/VisualElements/Extensions/`; командные расширения на стороне редактора в `Unity/Editor/Scripts/VisualElements/Extensions/` |
| Представление Welcome | Не отдельное окно — вкладка `SerializeReferenceWindow` (`Windows/Welcome/`), плюс `WelcomeWindowStartup` (автопоказ при первом импорте); перечисляет устанавливаемые примеры из `package.json` |

### Соглашения по коду редактора

**Доступность членов:** в `internal` классе члены должны объявляться как `internal` (или уже), никогда как `public` — собственный модификатор члена должен показывать его реальную доступность без проверки содержащего класса.

**PropertyDrawer'ы:** всегда `internal sealed class`. Сложные drawer'ы разделяются на статический помощник `{Feature}Drawer` с методами `DrawIMGUI()` и `DrawUIToolkit()` — см. `SerializableTypeDrawer.cs` как образец.

**XML doc-комментарии:** обязательны на каждом `public` члене, экономны на `internal`. `<summary>` — 1–2 предложения, что/зачем, без деталей реализации. `<remarks>` — только для неочевидного поведения, инвариантов или подводных камней; опускай, если это лишь повторит summary или код. `<example>` — только для нетривиальных сценариев использования, где форма применения неочевидна из сигнатуры. Следуй соглашениям Microsoft Framework Design Guidelines.

**USS:** стилизация идёт в USS, код только применяет `.AddClass()`. Именование (BEM-классы + грамматика переменных) и соглашения по загрузке: `Aspid.FastTools/Packages/tech.aspid.fasttools/Unity/Editor/Resources/UI/CLAUDE.md` — прочитай перед тем, как трогать любой `.uss` файл или имена USS-классов / переменные `--aspid-*` в коде.

**README-файлы:** держи 4 файла синхронными: корневые `README.md`/`README_RU.md` и `Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/EN|RU/README.md`. Пути к изображениям различаются: корневые файлы используют `Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/Images/...`, внутренние — `../Images/...`. Справочники по каждой функциональности живут рядом с соответствующим README внутри `EN/`/`RU/`.

### Локальная автоматизация Claude Code

Хуки PostToolUse (подключены в `.claude/settings.json`):

- `.claude/hooks/rebuild-generators-on-change.sh` — при `Edit`/`Write` файлов `*.cs` внутри `Aspid.FastTools.Generators/Aspid.FastTools.Generators/` пересобирает генератор и переразвёртывает DLL в Unity-пакет. Tests и Sample пропускаются — сохраняй эту область при изменении хука.
- `.claude/hooks/rebuild-analyzers-on-change.sh` — то же самое для анализатора (Tests/Sample пропускаются): пересобирает его, а `Directory.Build.targets` разворачивает DLL.

Скиллы в `.claude/skills/`: `build-generator` / `build-analyzer` (сборка + развёртывание Roslyn-DLL), `sync-readmes`, `unity-pipeline` (управление живым редактором через CLI `unity` + `com.unity.pipeline` — цикл перекомпиляции/тестов, `eval`, `sr_gate`, написание `[CliCommand]`), `editor-media-capture` (скриншоты/GIF окон редактора для документации).

**Управление редактором:** одновременно работают несколько редакторов (основной checkout + `.claude/worktrees/shared-*`), поэтому каждой `unity command` нужен `--project-path`. Для проверки живости используй `unity status`, а не `unity pipeline list`. Детали и подводные камни живут в скилле `unity-pipeline`.

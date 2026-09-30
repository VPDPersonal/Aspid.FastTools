# SerializeReference Tooling

Ссылки на переименованные и удалённые классы находятся по всему проекту и чинятся разом — раньше, чем в сборке они станут null.

<a id="проверить-проект"></a>

## Быстрый старт

1. Откройте **Tools → Aspid 🐍 → FastTools → Project References**.
2. Нажмите **Scan Project**: потерянные ссылки сгруппируются по сохранённому классу.
3. В группе нажмите **Fix all**, выберите класс и подтвердите **Rewrite**.

> [!NOTE]
> Двоичные ассеты и нескачанные файлы Git LFS молча пропускаются: оставьте **Asset Serialization → Mode** в **Force Text** (стоит по умолчанию) и скачайте файлы LFS до проверки.

<a id="bulk-repair-tabs"></a>

## Project References: восстановить группу

Project References и Asset References — вкладки одного окна. **Scan Project** читает файлы `.prefab`, `.asset` и `.unity` под `Assets/`, кроме [**Excluded scan folders**](#настройки).

![Project References с группами Fix all, Smart Fix → Pistol и Migrate all](../Images/aspid_fasttools_serialize_reference_project_references.png)

### Действия с группой

| Группа | Кнопка в заголовке | Строка под заголовком |
|---|---|---|
| Потерянный тип | **Fix all ▼** — выбрать класс для всех записей | **Smart Fix → Pistol** — применить класс с тем же или похожим именем; причина в подсказке |
| Переименование с <code lang="csharp">[MovedFrom]</code> | **Reassign all ▼** — выбрать другой класс вместо нового имени | **Migrate all → Crossbow** — записать новое имя, см. [Миграции](#миграции-с-movedfrom) |

Каждое действие спрашивает подтверждение **Rewrite** и пропускает открытые и заблокированные файлы. `<None>` в выборе класса очищает ссылки группы и удаляет их данные, включая поля с тем же `rid`; он спрашивает **Clear** и Undo не имеет.

### Что сохраняется при восстановлении

Восстановление переписывает только класс, пространство имён и сборку записи; её данные и `rid` остаются. Если поля группы разных типов, подтверждение предупреждает, что класс может подойти не всем записям: несовместимые станут <code lang="csharp">null</code> при переимпорте.

В сводке перезаписи есть **Undo**: он возвращает прежний класс записям, в которых всё ещё стоит новый. Undo пропадает после **Rescan**, закрытия окна и перезагрузки домена; **Edit → Undo** перезапись не отменяет.

### Экземпляры префабов

Потерянный класс, заданный через override экземпляра префаба, — в варианте, вложенном префабе или экземпляре в сцене, — показывается в отдельной карточке **Prefab instance overrides**, и проверки сборки считают его потерянным. Старое имя, указанное в <code lang="csharp">[MovedFrom]</code>, показывается там как ожидающая миграция и проверки проходит.

![Карточка Prefab instance overrides с потерянным GhostRailgun в варианте EliteLoadout](../Images/aspid_fasttools_serialize_reference_prefab_overrides.png)

**Fix all**, **Smart Fix**, **Migrate all** и `<None>` такие записи не переписывают: выберите новый класс на экземпляре в инспекторе или отмените override. В Asset References ссылки, которые есть только в override, не видны.

## Asset References: разобрать один ассет

Укажите сохранённый префаб, ScriptableObject или сцену в поле рядом с **Rescan** либо щёлкните строку записи в Project References. Ссылки сгруппированы по объектам-владельцам, у каждой — путь поля и `rid`:

![Asset References: потерянный GhostCrossbow, общий Pistol с меткой SHARED и запись Railgun в Orphaned](../Images/aspid_fasttools_serialize_reference_asset_references.png)

| Обозначение | Значение |
|---|---|
| Полоса с **Fix Missing ▼** (у миграции — **Fix ▼**) | Сохранённый класс не найден; кнопка открывает выбор класса |
| Строка **Smart Fix → Pistol** | Класс, подобранный как у **Smart Fix** в Project References; щелчок записывает его в файл |
| Строка **Migrate → Crossbow** | Класс переименован с <code lang="csharp">[MovedFrom]</code>; щелчок записывает новое имя в файл |
| Полоса с **Change ▼**, **Assign ▼** или **Assign Required ▼** | Меняет класс исправной ссылки, заполняет пустое или обязательное поле; ассет сохраняется сразу |
| **SHARED** | Несколько полей указывают на один экземпляр; одинаковый цвет отмечает связанные поля |
| **Orphaned** | Запись, на которую не указывает ни одно поле; **Clear** удаляет её из файла, без Undo |

**Fix Missing**, **Smart Fix** и **Migrate** записывают класс в файл сразу, без подтверждения, и **Edit → Undo** его не отменяет.

![GhostWeapon восстанавливается как Pistol в Asset References](../Images/aspid_fasttools_serialize_reference_tooling.gif)

## Миграции с MovedFrom

Если <code lang="class-name">CrossbowLauncher</code> переименован в <code lang="class-name">Crossbow</code> с <code lang="csharp">[MovedFrom]</code>, Unity загружает старые ссылки сам. **Migrate all** записывает новое имя в файлы, чтобы атрибут можно было удалить:

| В файле — до Migrate all | После |
|---|---|
| `type: {class: CrossbowLauncher, …}` | `type: {class: Crossbow, …}` |

Группа становится ожидающей миграцией, если старое имя указано в <code lang="csharp">[MovedFrom]</code> ровно у одного класса в проекте и он подходит полю; проверки сборки такую группу потерянной не считают. Если имя указано у нескольких классов или сохранён закрытый generic, группа остаётся потерянным типом.

Удаляйте <code lang="csharp">[MovedFrom]</code>, только когда старое имя не осталось ни в одном файле. **Migrate all** не переписывает его:

- в override экземпляров префабов;
- в открытых, несохранённых и заблокированных файлах, см. [ограничения](#ограничения);
- в папках из **Excluded scan folders**;
- в двоичных ассетах и нескачанных файлах Git LFS;
- в файлах вне `Assets/`.

<a id="project-settings--the-buildci-gate"></a>

## Проверка перед сборкой

Строгость проверки задаёт настройка [**Build / CI gate**](#настройки):

| Режим | Сборка плеера | Отдельный CI-запуск |
|---|---|---|
| `Off` | Проверка пропускается | Ни поиска, ни отчёта, старый отчёт остаётся; код `0` |
| `Warn` | Предупреждение, сборка продолжается | Отчёт и нарушения в журнале; код `0` |
| `Fail` | Потерянные типы прерывают сборку | Отчёт; код `1` при нарушениях |

Сборка проверяет все ассеты под `Assets/`, а не только попадающие в неё: в режиме `Fail` её остановит и неиспользуемый префаб — исключите такие папки в **Excluded scan folders**.

### Что проверяет каждый запуск

| Запуск | Потерянные типы | Пустые поля с <code lang="csharp">Required = true</code> |
|---|---|---|
| **Project References → Scan Project** | Да, вместе с ожидающими миграциями | Если режим не `Off`, отдельной группой **Required violations** |
| **Asset References** | Да | Да, в любом режиме |
| Сборка плеера | Если режим не `Off` | Нет |
| CI без `-srGateRequired` | Если режим не `Off` | Нет |
| CI с `-srGateRequired` | Если режим не `Off` | Если режим не `Off` |

![Группа Required violations: пустое поле _primary в двух префабах](../Images/aspid_fasttools_serialize_reference_required_violations.png)

Обязательное поле задаёт <code lang="csharp">[TypeSelector(Required = true)]</code>, подробнее — в разделе [Обязательное поле](02-serializable-types.md#обязательное-поле). В сценах у проверки Required есть [ограничения](#ограничения).

<a id="headless-ci"></a>

## Запуск в CI

```bash
Unity -batchmode -projectPath . \
  -executeMethod \
  Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceCiGate.RunCheck \
  -srGateReport SerializeReferenceGateReport.txt \
  -srGateRequired -srGateFail
```

Код выхода `2` означает сбой самой проверки.

### Флаги запуска

| Флаг | Действие |
|---|---|
| `-srGateReport <path>` | Путь отчёта от корня проекта, по умолчанию `SerializeReferenceGateReport.txt`; папка должна существовать, файл перезаписывается |
| `-srGateRequired` | Дополнительно проверить незаполненные поля с <code lang="csharp">Required = true</code> |
| `-srGateFail` | Использовать `Fail` вместо режима проекта, даже `Off` |
| `-srGateWarnOnly` | Использовать `Warn` вместо режима проекта, даже `Off`; важнее `-srGateFail`, если переданы оба |

<a id="отчёт-и-коды-выхода"></a>

### Отчёт

В заголовке указано число нарушений и файлов, которые не просканированы, потому что это не текстовый YAML:

```text
# SerializeReference Gate Report
# Violations: 2
# Not scanned (not text YAML): 2
#   Binary	Assets/Legacy/OldLoadout.prefab
#   LfsPointer	Assets/Levels/Arena.unity
```

Пропущенные файлы не меняют код выхода.

После заголовка каждое нарушение занимает одну строку. Поля разделены табуляцией:

```text
KIND    assetPath    fileId    rid    className    fieldPath    origin
```

| Поле | Содержимое |
|---|---|
| `KIND` | `MissingType` или `RequiredUnset` |
| `assetPath` | Путь файла, например `Assets/Presets/BrokenWeaponPreset.asset` |
| `fileId` | Идентификатор объекта-владельца внутри файла; для override экземпляра префаба — идентификатор экземпляра |
| `rid` | Идентификатор managed-ссылки; в строках `RequiredUnset` — `-2` для пустого <code lang="csharp">[SerializeReference]</code> и `0` для <code lang="csharp">string</code> и <code lang="class-name">SerializableType</code> |
| `className` | Сохранённое имя класса для `MissingType`, без отдельных полей namespace и сборки |
| `fieldPath` | Путь обязательного поля; для `MissingType` из override — переопределённое поле, если экземпляр его переопределяет; иначе пусто |
| `origin` | `override` для типа, заданного override экземпляра префаба; иначе пусто |

В Asset References запись находится по `rid`, строка `RequiredUnset` с `rid` `0` — по `fieldPath`; строка с origin `override` — в карточке **Prefab instance overrides** в Project References.

## Настройки

Все настройки собраны в **Tools → Aspid 🐍 → FastTools → Settings**; общие есть и в **Project Settings → Aspid.FastTools → SerializeReference**, личные — в **Preferences → Aspid.FastTools → SerializeReference**.

![Раздел SerializeReference во вкладке Settings](../Images/aspid_fasttools_serialize_reference_settings.png)

| Настройка | По умолчанию | Что делает |
|---|---|---|
| **Build / CI gate** | `Warn` | Задаёт строгость [проверки перед сборкой](#проверка-перед-сборкой) и в CI |
| **Excluded scan folders** | Нет папок | Папки внутри `Assets/`, которых не касаются Project References, проверки сборки и CI и Breakage detection |
| **Auto de-alias duplicated list elements** | Включена | Даёт продублированному элементу списка собственный экземпляр вместо общего `rid` |
| **Breakage detection** | Включена | После изменения скриптов или ассетов сообщает о новых потерянных ссылках уведомлением и в Console |

Breakage detection — личная настройка, она хранится в `EditorPrefs` этого проекта. Остальные общие: они лежат в `ProjectSettings/SerializeReferenceSharedSettings.asset` и действуют для всей команды и CI.

## Ограничения

- **Открытые и заблокированные файлы.** Перезапись пропускает открытые сцены, Prefab Mode, ассеты с несохранёнными изменениями и файлы только для чтения, которые система контроля версий не смогла извлечь (с ошибкой в Console). Asset References предложит сохранить несохранённый ассет; сцену и Prefab Mode сохраните и закройте либо исправьте поле через [Fix в инспекторе](03-serialize-reference-selector.md#восстановление-потерянного-типа). В Project References `<None>` очищает ссылки открытых копий в памяти, и поиск показывает их, пока копии не сохранят.
- **Сцены и потерянные родители.** В сцене и под потерянной родительской ссылкой Asset References чинит только потерянные типы; остальные поля меняйте в инспекторе.
- **Required в сценах.** Сцены читаются из YAML: проверяются поля компонентов и контейнеров, сериализуемых по значению, включая сами поля <code lang="csharp">[SerializeReference]</code>; поля внутри managed-ссылок, коллекции и override экземпляров префабов — нет. Поле, которого нет в файле сцены, нарушением не считается.

## Пример в пакете

Потерянные типы, переименование через <code lang="csharp">[MovedFrom]</code> и общая ссылка для обеих вкладок есть в ассетах примера [SerializeReferences](../../Samples~/SerializeReferences/Documentation/README.ru.md).

# SerializeReference Tooling

Ссылки на переименованные и удалённые классы находятся по всему проекту и чинятся разом — раньше, чем в билде они станут null.

<a id="проверить-проект"></a>

## Быстрый старт

1. Откройте **Tools → Aspid 🐍 → FastTools → Project References**.
2. Нажмите **Scan Project**: потерянные ссылки сгруппируются по сохранённому классу.
3. В группе нажмите **Fix all**, выберите класс и подтвердите **Rewrite**.

> [!NOTE]
> Поиск читает только текстовый YAML: двоичные файлы и указатели Git LFS Project References пропускает молча, их перечисляют проверка сборки и отчёт CI. Поэтому в **Asset Serialization → Mode** нужен **Force Text**.

<a id="bulk-repair-tabs"></a>

## Project References: восстановить группу

Project References и Asset References — вкладки одного окна. **Scan Project** читает файлы `.prefab`, `.asset` и `.unity` под `Assets/`, кроме **Excluded scan folders**, и не только сцены из сборки. Щелчок по строке группы открывает ассет в **Asset References**.

![Project References с группами Fix all, Smart Fix → Pistol и Migrate all](../Images/aspid_fasttools_serialize_reference_project_references.png)

### Какое действие выбрать

| Действие | Что делает |
|---|---|
| **Fix all** | Открывает выбор класса и применяет его ко всем доступным для записи ссылкам группы |
| **Smart Fix → Pistol** | Применяет класс, найденный по <code lang="csharp">[MovedFrom]</code>, тому же имени, тому же имени в другом регистре или похожему имени; подтверждение то же |
| **Migrate all** | Записывает класс, который <code lang="csharp">[MovedFrom]</code> указывает для старого |
| **Reassign all** | Выбирает другой класс для группы, распознанной как миграция |
| `<None>` в выборе класса | Очищает ссылки группы и удаляет их данные, включая поля с тем же `rid`; без Undo |

### Что сохраняется при восстановлении

Восстановление переписывает только класс, пространство имён и сборку записи; её данные и `rid` остаются. Если поля группы разных типов, подтверждение предупреждает, что класс может подойти не всем записям: несовместимые станут <code lang="csharp">null</code> при переимпорте.

В сводке перезаписи есть **Undo**: он возвращает прежний класс записям, в которых всё ещё стоит новый. Undo пропадает после **Rescan**, закрытия окна и перезагрузки домена; **Edit → Undo** перезапись не отменяет.

### Экземпляры префабов

Класс, заданный через override экземпляра префаба — в варианте, вложенном префабе или экземпляре в сцене, — показывается в отдельной карточке **Prefab instance overrides**, и проверки сборки считают его потерянным. Старое имя, указанное в <code lang="csharp">[MovedFrom]</code>, показывается там как ожидающая миграция и проверки проходит.

## Asset References: разобрать один ассет

Укажите сохранённый префаб, ScriptableObject или сцену в поле рядом с **Rescan** либо щёлкните строку в Project References. Ссылки сгруппированы по объектам-владельцам, у каждой — путь поля и `rid`:

| Обозначение | Значение |
|---|---|
| Полоса с **Fix Missing ▼** | Сохранённый класс не найден; кнопка открывает выбор класса |
| Строка **Migrate → Crossbow** | Класс переименован с <code lang="csharp">[MovedFrom]</code>; щелчок записывает новое имя в файл |
| **SHARED** | Несколько полей указывают на один экземпляр; одинаковый цвет отмечает связанные поля |
| **Orphaned** | Запись, на которую не указывает ни одно поле; **Clear** удаляет её из файла, без Undo |

![GhostWeapon восстанавливается как Pistol в Asset References](../Images/aspid_fasttools_serialize_reference_tooling.gif)

> [!WARNING]
> **Fix Missing**, **Smart Fix** и **Migrate** записывают класс в файл сразу, без подтверждения, и Undo его не отменит.

## Миграции с MovedFrom

<code lang="csharp">[MovedFrom]</code> связывает старое имя с переименованным классом, и Unity загружает такие ссылки сам. **Migrate all** записывает новое имя в файлы, чтобы атрибут можно было удалить:

| До — CrossbowLauncher | После — Crossbow |
|---|---|
| <pre lang="csharp"><code>[Serializable]&#10;public sealed class CrossbowLauncher&#10;&#123;&#10;    public int Damage = 14;&#10;&#125;</code></pre> | <pre lang="csharp"><code>[Serializable]&#10;[MovedFrom(false,&#10;    sourceClassName: "CrossbowLauncher")]&#10;public sealed class Crossbow&#10;&#123;&#10;    public int Damage = 14;&#10;&#125;</code></pre> |

Группа становится ожидающей миграцией, если старое имя указано в <code lang="csharp">[MovedFrom]</code> ровно у одного класса в проекте и он подходит полю; проверки сборки такую группу потерянной не считают. Если имя указано у нескольких классов или сохранён закрытый generic, группа остаётся потерянным типом.

Удаляйте <code lang="csharp">[MovedFrom]</code>, только когда старое имя не осталось ни в одном файле. **Migrate all** не переписывает его:

- в override экземпляров префабов;
- в папках из **Excluded scan folders**;
- в двоичных файлах и указателях Git LFS;
- в файлах вне `Assets/`.

<a id="project-settings--the-buildci-gate"></a>

## Проверка перед сборкой

Откройте **Project Settings → Aspid.FastTools → SerializeReference** и задайте **Build / CI gate**; по умолчанию стоит `Warn`:

| Режим | Сборка плеера | Отдельный CI-запуск |
|---|---|---|
| `Off` | Проверка пропускается | Ни поиска, ни отчёта, старый отчёт остаётся; код `0` |
| `Warn` | Предупреждение, сборка продолжается | Отчёт и нарушения в журнале; код `0` |
| `Fail` | Потерянные типы прерывают сборку | Отчёт; код `1` при нарушениях |

### Что проверяет каждый запуск

| Запуск | Потерянные типы | Пустые поля с <code lang="csharp">Required = true</code> |
|---|---|---|
| **Project References → Scan Project** | Да, вместе с ожидающими миграциями | Если режим не `Off`, отдельной группой **Required violations** |
| **Asset References** | Да | Да, в любом режиме |
| Сборка плеера | Если режим не `Off` | Нет |
| CI без `-srGateRequired` | Если режим не `Off` | Нет |
| CI с `-srGateRequired` | Если режим не `Off` | Если режим не `Off` |

Обязательное поле задаёт <code lang="csharp">[TypeSelector(Required = true)]</code>, подробнее — в разделе [Обязательное поле](02-serializable-types.md#обязательное-поле). В сценах у проверки Required есть [ограничения](#ограничения).

### Общие и личные настройки

| Настройка | Где хранится | Назначение |
|---|---|---|
| **Build / CI gate** | В проекте | Строгость проверки |
| **Excluded scan folders** | В проекте | Папки, которые пропускают поиск и проверки |
| **Auto de-alias duplicated list elements** | В проекте | Независимая копия при дублировании элемента списка |
| **Breakage detection** | Локально, `EditorPrefs` | Уведомление и сообщение в Console о новых потерянных ссылках после изменения скриптов или ассетов |

Общие настройки хранятся в `ProjectSettings/SerializeReferenceSharedSettings.asset`, личные — в **Preferences → Aspid.FastTools → SerializeReference**.

<a id="headless-ci"></a>

## Запуск в CI

```bash
Unity -batchmode -projectPath . \
  -executeMethod \
  Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceCiGate.RunCheck \
  -srGateReport SerializeReferenceGateReport.txt \
  -srGateRequired -srGateFail
```

Код выхода `2` означает сбой самой проверки, например если папки для отчёта нет.

### Флаги запуска

| Флаг | Действие |
|---|---|
| `-srGateReport <path>` | Путь отчёта от корня проекта, по умолчанию `SerializeReferenceGateReport.txt`; папка должна существовать, файл перезаписывается |
| `-srGateRequired` | Дополнительно проверить незаполненные поля с <code lang="csharp">Required = true</code> |
| `-srGateFail` | Использовать `Fail` вместо режима проекта, даже `Off` |
| `-srGateWarnOnly` | Использовать `Warn` вместо режима проекта, даже `Off`; побеждает `-srGateFail`, если переданы оба |

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

Путь ассета, `fileId` и `rid` находят запись в Asset References; строка с origin `override` — вместо этого в карточке **Prefab instance overrides** в Project References.

## Ограничения

- **Открытые копии.** Перезапись пропускает открытые сцены, Prefab Mode и ассеты с несохранёнными изменениями. Несохранённый ассет Asset References предложит сохранить; сцену и Prefab Mode сохраните и закройте либо исправьте поле через [Fix в инспекторе](03-serialize-reference-selector.md#восстановление-потерянного-типа). `<None>` очищает такие ссылки в памяти, и поиск показывает их, пока их не сохранят.
- **Override экземпляров префабов.** **Fix all**, **Smart Fix**, **Migrate all** и `<None>` их не переписывают: выберите новый класс на экземпляре в инспекторе или отмените override. Asset References не показывает ссылки, которые есть только в override.
- **Сцены и потерянные родители.** В сцене и под потерянной родительской ссылкой Asset References чинит только потерянные типы; остальные поля меняйте в инспекторе.
- **Required в сценах.** Сцены читаются из YAML: проверяются поля компонентов и контейнеров, сериализуемых по значению, включая сами поля <code lang="csharp">[SerializeReference]</code>; поля внутри managed-ссылок, коллекции и override экземпляров префабов — нет. Поле, которого нет в файле сцены, нарушением не считается.

## Пример в пакете

Потерянные типы, переименование через <code lang="csharp">[MovedFrom]</code> и общая ссылка для обоих окон есть в ассетах примера [SerializeReferences](../../Samples~/SerializeReferences/Documentation/README.ru.md).

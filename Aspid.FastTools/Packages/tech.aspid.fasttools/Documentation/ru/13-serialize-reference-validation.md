# Проверка перед сборкой и CI

Находит потерянные ссылки и незаполненные обязательные поля до выпуска проекта.

## Быстрый старт

Откройте **Tools → Aspid 🐍 → FastTools → Settings** и выберите **Build / CI gate → Fail**. Теперь потерянные типы прерывают сборку плеера. По умолчанию стоит **Warn** — он сообщает о проблемах и продолжает сборку.

Проверка сообщает о нарушениях; исправление описано на странице [Восстановление SerializeReference](04-serialize-reference-tooling.md).

## Проверка перед сборкой

Строгость проверки задаёт настройка **Build / CI gate**:

| Режим | Сборка плеера | Отдельный CI-запуск |
|---|---|---|
| `Off` | Проверка пропускается | Ни поиска, ни отчёта, старый отчёт остаётся; код `0` |
| `Warn` | Предупреждение, сборка продолжается | Отчёт и нарушения в журнале; код `0` |
| `Fail` | Потерянные типы прерывают сборку | Отчёт; код `1` при нарушениях |

Сборка проверяет все ассеты под `Assets/`, а не только попадающие в неё: в режиме `Fail` её остановит и неиспользуемый префаб — исключите такие папки в [**Excluded scan folders**](#область-проверки).

## Что проверяет каждый запуск

| Запуск | Потерянные типы | Пустые поля с <code lang="csharp">Required = true</code> |
|---|---|---|
| **Project References → Scan Project** | Да, вместе с ожидающими миграциями | Если режим не `Off`, отдельной группой **Required violations** |
| **Asset References** | Да | Да, в любом режиме |
| Сборка плеера | Если режим не `Off` | Нет |
| CI без `-srGateRequired` | Если режим не `Off` | Нет |
| CI с `-srGateRequired` | Если режим не `Off` | Если режим не `Off` |

![Группа Required violations: пустое поле _primary в двух префабах](../Images/aspid_fasttools_serialize_reference_required_violations.png)

Обязательное поле задаёт <code lang="csharp">[TypeSelector(Required = true)]</code>, подробнее — в разделе [Обязательное поле](12-type-selector.md#обязательное-поле). В сценах не проверяются обязательные поля внутри managed-ссылок, в коллекциях и в override префабов.

## Область проверки

Проверяются сохранённые `.prefab`, `.asset` и `.unity` внутри `Assets/`. Ожидающие [миграции с MovedFrom](04-serialize-reference-tooling.md#миграции-с-movedfrom) не считаются потерянными типами. Проверка потерянных типов относится к <code lang="csharp">[SerializeReference]</code>; она не ищет неразрешимые имена в <code lang="class-name">SerializableType</code> или строках.

**Excluded scan folders** исключает папки из Project References, проверки сборки, CI и обнаружения новых поломок. По умолчанию исключений нет.

Двоичные ассеты и нескачанные файлы Git LFS не проверяются; CI перечисляет их в отчёте. Для полного сканирования используйте **Asset Serialization → Mode → Force Text** и скачайте файлы LFS.

**Build / CI gate** и **Excluded scan folders** доступны и в **Project Settings → Aspid.FastTools → SerializeReference** и хранятся в `ProjectSettings/SerializeReferenceSharedSettings.asset`, общем для команды и CI.

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

## Отчёт

Отчёт содержит потерянные типы, незаполненные обязательные поля и пропущенные файлы. Пропущенные файлы не меняют код выхода.

<details>
<summary>Формат отчёта</summary>

Отчёт начинается с заголовка:

```text
# SerializeReference Gate Report
# Violations: 2
# Not scanned (not text YAML): 2
#   Binary	Assets/Legacy/OldLoadout.prefab
#   LfsPointer	Assets/Levels/Arena.unity
```

Дальше — по строке на нарушение, поля разделены табуляцией:

```text
KIND    assetPath    fileId    rid    className    fieldPath    origin
```

| Поле | Содержимое |
|---|---|
| `KIND` | `MissingType` или `RequiredUnset` |
| `assetPath` | Путь файла |
| `fileId` | Идентификатор объекта-владельца внутри файла; для override экземпляра префаба — идентификатор экземпляра |
| `rid` | Идентификатор managed-ссылки; в строках `RequiredUnset` — `-2` для пустого <code lang="csharp">[SerializeReference]</code> и `0` для <code lang="csharp">string</code> и <code lang="class-name">SerializableType</code> |
| `className` | Сохранённое имя класса для `MissingType` |
| `fieldPath` | Путь обязательного поля; для `MissingType` из override — переопределённое поле; иначе пусто |
| `origin` | `override` для типа, заданного override экземпляра префаба; иначе пусто |

В Asset References запись находится по `rid`, строка `RequiredUnset` с `rid` `0` — по `fieldPath`; строка с origin `override` — в карточке **Prefab instance overrides** в Project References.

</details>

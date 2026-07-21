# SerializeReference Selector

Стандартный Inspector не умеет заполнять поля `[SerializeReference]`: managed-ссылку нельзя
создать из UI, а при переименовании или удалении типа Unity молча очищает данные.
SerializeReference Selector закрывает оба пробела: выпадающий выбор реализации прямо
в Инспекторе плюс инструменты поиска и починки сломанных ссылок — от одного поля
до всего проекта и CI-гейта.

**Разделы справочника:**

* [`Inspector type dropdown`](#inspector-type-dropdown) — дропдаун `[TypeSelector]`
  на полях `[SerializeReference]`: выбор реализации, вложенный inspector, generics,
  copy/paste;
* [`Repairing broken references`](#repairing-broken-references) — жёлтое предупреждение
  вместо молчаливой очистки, **Fix** / **Smart Fix** / **Make unique**;
* [`Bulk repair tabs`](#bulk-repair-tabs) — вкладки **Asset References** и
  **Project References** для аудита и массовой починки по всему проекту;
* [`Project settings & the build/CI gate`](#project-settings--the-buildci-gate) —
  настройки в Project Settings, их scope и гейт на сборку плеера;
* [`Headless CI`](#headless-ci) — `SerializeReferenceCiGate.RunCheck` для batchmode-пайплайнов.

**Краткая версия с теми же примерами — в** [README](README.md#serializereference-selector).

## Inspector type dropdown

Добавьте `[TypeSelector]` рядом с `[SerializeReference]` — Inspector заменит стандартный
UI managed-ссылки иерархическим [окном выбора типа](Types.md#typeselectorwindow) с поиском.
Вы прямо в инспекторе выбираете, какая конкретная реализация типа поля будет создана;
`<None>` очищает ссылку.

```csharp
using System;
using UnityEngine;
using System.Collections.Generic;
using Aspid.FastTools.Types;

public interface IWeapon
{
    void Fire();
}

[Serializable]
public sealed class Pistol : IWeapon
{
    [SerializeField] [Min(0)] private int _damage = 10;

    public void Fire() => Debug.Log($"Pistol: {_damage} dmg");
}

public sealed class Loadout : MonoBehaviour
{
    [TypeSelector]
    [SerializeReference] private IWeapon _primary;

    [TypeSelector]
    [SerializeReference] private List<IWeapon> _sidearms;
}
```

Атрибут существует только в редакторе (`[Conditional("UNITY_EDITOR")]`) и не несёт
стоимости в рантайме. Работает с одиночными полями, массивами и `List<T>`, в инспекторах
IMGUI и UIToolkit. Тот же атрибут работает и с полями `string` и `SerializableType` —
см. [TypeSelectorAttribute](Types.md#typeselectorattribute).

![Выбор реализации в managed-ссылке: пикер и вложенный inspector выбранного экземпляра](../Images/aspid_fasttools_serialize_reference_selector.gif)

| Возможность | Что делает |
|---|---|
| **Выбор реализации** | В списке — конкретные не-`UnityEngine.Object` классы, совместимые с типом поля. `[TypeSelector(typeof(IMelee))]` сужает его до реализаций `IMelee`. |
| **Open generics** | `Modifier<T>` и подобные: аргументы выводятся из закрытого generic-поля либо выбираются на второй странице селектора. |
| **Сохранение данных** | При смене типа поля, совпадающие по имени и сериализуемой форме, переносятся, а не сбрасываются в значения по умолчанию. |
| **Copy / Paste** | Правый клик по заголовку копирует значение и вставляет его независимым экземпляром в любое совместимое поле. |
| **Мультивыделение** | Смешанное выделение показывает смешанное состояние dropdown; выбор или вставка применяется к каждому объекту в одной группе Undo. |
| **Проверка компилятором** | Анализатор Roslyn: `AFT0004` (ошибка) — тип наследует `UnityEngine.Object`; `AFT0005` (предупреждение) — селектор оказался бы пустым. |

Пустое поле с `[TypeSelector(Required = true)]` показывает предупреждение «required»
в инспекторе и считается нарушением для [build/CI-гейта](#project-settings--the-buildci-gate) —
см. свойство `Required` в [TypeSelectorAttribute](Types.md#typeselectorattribute).

## Repairing broken references

Когда сохранённый в ассете тип перестаёт резолвиться или два поля незаметно делят
один экземпляр, селектор не молчит — каждая проблема получает заметку в инспекторе
и кнопку починки рядом:

| Случай | Решение |
|---|---|
| **Потерянный тип** (переименован или удалён) | Жёлтое предупреждение вместо молчаливой очистки. Подчёркнутое **Fix** открывает селектор и переназначает тип с сохранением данных — на любой глубине, в сохранённых ассетах и прямо в Prefab Mode. |
| **Smart Fix** | Рядом с **Fix** предлагает наиболее вероятную замену (`[MovedFrom]`, другой namespace/сборка, регистр, близкое имя) и применяет в один клик — никогда не автоматически. |
| **Общая ссылка** (два поля делят экземпляр) | Помечается лейблом; **Make unique** расщепляет её в независимую копию. Дублирование элемента списка (Ctrl+D, `+`) больше не создаёт алиас. |

![Заметка Missing type с кнопками Fix и Smart Fix на сломанной managed-ссылке](../Images/aspid_fasttools_serialize_reference_repair.png)

![Заметка Shared reference с действием Make unique на двух полях, делящих один экземпляр](../Images/aspid_fasttools_serialize_reference_make_unique.png)

## Bulk repair tabs

Чинить ссылки по одной необязательно: аудит и массовая починка вынесены в отдельные
вкладки окна FastTools.

| Вкладка | Назначение |
|---|---|
| **Asset References** (`Tools → Aspid 🐍 → FastTools → Asset References`) | Строит весь граф managed-ссылок ассета прямо из YAML — дерево по компонентам с путями полей, общими и осиротевшими ссылками, значками `MISSING` / `SHARED` и инлайн-выбором типа на каждой карточке. Достаёт потерянные ссылки, которые инспектор не показывает. |
| **Project References** (`Tools → Aspid 🐍 → FastTools → Project References`) | `Scan Project` обходит каждый `.prefab` / `.asset` / `.unity` под `Assets/`, группирует сломанные ссылки по сохранённому типу и чинит всю группу одним `Fix all` (плюс Smart Fix). Группа, чей сохранённый тип совпадает с объявленным переименованием `[MovedFrom]`, читается как ожидающая миграция, а не поломка — один клик **Migrate all** запекает переименование в файлы, после чего атрибут можно удалить из кода. |

Вкладка **Asset References** раскладывает граф managed-ссылок одного ассета по карточкам
со значками `MISSING` / `SHARED` и инлайн-починкой:

![Вкладка Asset References: граф ссылок ассета с карточкой Fix Missing](../Images/aspid_fasttools_serialize_reference_asset_references.png)

Вкладка **Project References** группирует находки всего проекта по сохранённому типу —
одна группа чинится целиком одним `Fix all`:

![Вкладка Project References: группа сломанных ссылок с Fix all и Smart Fix](../Images/aspid_fasttools_serialize_reference_project_references.png)

## Project settings & the build/CI gate

**`Project Settings → Aspid FastTools → SerializeReference`** содержит:

| Настройка | Scope | Что делает |
|---|---|---|
| **Breakage detection** | per-user | Проактивный тост + предупреждение в Console, когда ссылки заново становятся потерянными после рекомпиляции / импорта. |
| **Auto de-alias duplicated list elements** | коммитимая | Дублированный элемент списка получает собственный экземпляр вместо совместного использования id оригинала. |
| **Build / CI gate** | коммитимая | `Off` / `Warn` / `Fail`: при сборке плеера логировать или прерывать сборку на потерянных (а для CI — и на незаданных обязательных) managed-ссылках. |
| **Excluded scan folders** | коммитимая | Пути, пропускаемые при всех проектных сканах. |

- Коммитимые значения хранятся в `ProjectSettings/SerializeReferenceSharedSettings.asset` — закоммитьте его, чтобы команда и CI вели себя одинаково; breakage detection остаётся per-machine (`EditorPrefs`).
- Rid colours — не настройка: общая ссылка всегда раскрашивается по id — совпадающий цвет и показывает, какие поля делят один экземпляр.

Те же опции продублированы во вкладке **Settings** окна (`Tools → Aspid 🐍 → FastTools → Settings`) и на странице **`Preferences → Aspid FastTools`**, рядом с индивидуальными настройками пикера:

- **Favorites** — переключатель секции.
- **Recent items** — слайдер ёмкости (0–20; 0 скрывает секцию и приостанавливает запись, не стирая историю).
- **Saved lists** — очищает сохранённые Favorites / Recent.
- **Welcome** — переключатель автопоказа.

Каждая строка помечена полоской scope (зелёная — коммитимые, синяя — индивидуальные); закреплённый футер предлагает **Reset to defaults** отдельно для каждого scope (сохранённые списки Favorites / Recent сброс переживают). Все поверхности зеркалят друг друга живьём.

## Headless CI

Для headless-CI та же проверка запускается методом `SerializeReferenceCiGate.RunCheck`:
он сканирует проект, пишет отчёт, логирует каждое нарушение и учитывает коммитимую
строгость гейта — `Off` пропускает проверку, `Warn` логирует, но завершается с кодом 0,
`Fail` завершается с кодом 1 при нарушениях (код 2 — внутренняя ошибка самой проверки).

```bash
Unity -batchmode -quit -projectPath . \
  -executeMethod Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceCiGate.RunCheck \
  -srGateReport SerializeReferenceGateReport.txt -srGateRequired
```

| Флаг | Описание |
|---|---|
| `-srGateReport <path>` | Путь файла отчёта; по умолчанию `SerializeReferenceGateReport.txt` в корне проекта. Каждое нарушение — машиночитаемая строка с типом нарушения, путём ассета и путём поля. |
| `-srGateRequired` | Дополнительно проверяет незаданные поля `[TypeSelector(Required = true)]` в префабах, ScriptableObject и сценах (required-поля верхнего уровня, чистый YAML-проход). |
| `-srGateWarnOnly` | Переопределяет коммитимую строгость на `Warn` для этого запуска: нарушения логируются, но код выхода 0. Выигрывает у `-srGateFail`, если переданы оба. |
| `-srGateFail` | Переопределяет коммитимую строгость на `Fail` для этого запуска: код выхода 1 при нарушениях. |

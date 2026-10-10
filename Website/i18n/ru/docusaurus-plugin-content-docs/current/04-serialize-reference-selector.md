# SerializeReference Selector

Класс поля выбирают в инспекторе, а данные не теряются при его смене.

## Быстрый старт

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>[SerializeReference]&#10;private IWeapon _primary =&#10;    new Pistol();</code></pre> | <pre lang="csharp"><code>[TypeSelector]&#10;[SerializeReference]&#10;private IWeapon _primary;</code></pre> |

## Какие классы в списке

Поле <code lang="class-name">IWeapon</code> предлагает конкретные реализации этого интерфейса, например <code lang="class-name">Pistol</code> и <code lang="class-name">Shotgun</code>. Выбор создаёт экземпляр класса; `<None>` очищает поле.

Дополнительные ограничения, обязательные поля и оформление списка — на странице [TypeSelector](03-type-selector.md).

- <code lang="csharp">Allow</code> здесь не действует: нужны типы, экземпляры которых можно создать.
- Аргументы generic-класса выводятся из типа поля и типов <code lang="csharp">[TypeSelector]</code>; если вывести их нельзя, окно предлагает выбрать типы, которые Unity умеет сериализовать.

## Списки

В списке с <code lang="csharp">[TypeSelector]</code> кнопка «+» открывает выбор класса и добавляет новый экземпляр, а `<None>` — пустой элемент. При нескольких выбранных объектах каждый получает свой экземпляр.

![«+» у Sidearms открывает выбор класса и добавляет Shotgun](../../../../docs/Images/aspid_fasttools_serialize_reference_list.gif)

## Смена класса

При смене класса FastTools пытается перенести совместимые значения полей с теми же именами. Поля, которые перенести не удалось, остаются со значениями нового экземпляра:

| Поле | <code lang="class-name">Pistol</code> | → <code lang="class-name">Shotgun</code> |
|---|---|---|
| **Damage** | 37 | 37 |
| **Magazine Size** | 12 | — |
| **Pellets** | — | 8, начальное значение |

![Смена Pistol на Shotgun сохраняет Damage = 37](../../../../docs/Images/aspid_fasttools_serialize_reference_selector.gif)

Вложенная ссылка переносится тем же экземпляром, если имя и тип поля совместимы.

> [!TIP]
> Класс можно выбрать, перетащив `.cs` из **Project** на заголовок поля.

## Меню заголовка

Правый клик по заголовку поля:

| Пункт | Что делает |
|---|---|
| **Copy Serialize Reference** | Копирует класс и данные поля; копия живёт до перезагрузки домена |
| **Paste Serialize Reference** | Вставляет копию в поле подходящего типа; копия пустого поля очищает его |
| **Find Usages of Pistol** | Ищет класс в проекте через Unity Search |
| **Create New Script…** | Создаёт <code lang="csharp">[Serializable]</code> класс под тип поля и назначает его после компиляции |
| **Save as Template…** | Сохраняет значение под именем; шаблоны хранятся в настройках редактора на этом компьютере, не в проекте |
| **Paste Template → …** | Создаёт экземпляр из шаблона; в списке только подходящие полю |
| **Paste Template → Remove Missing (N)…** | Удаляет шаблоны, класс которых больше не загружается; появляется, только если такие есть |

> [!WARNING]
> Copy/Paste и шаблоны не переносят вложенные поля <code lang="csharp">[SerializeReference]</code>: вставленный объект потеряет такие ссылки.

## Общие ссылки

**Link to Existing → …** в меню заголовка связывает поле с экземпляром из другого поля того же объекта.

Поля, которые указывают на один экземпляр, помечены **Shared reference #N**, а **Make unique** под полем или **Make Unique Reference** в меню заголовка даёт полю собственную копию вместе с вложенными ссылками.

![Make unique создаёт независимую копию общей ссылки](../../../../docs/Images/aspid_fasttools_serialize_reference_make_unique.png)

Продублированный элемент списка получает собственный экземпляр, а не ссылку на тот же. За это отвечает настройка **Auto de-alias duplicated list elements** в **Tools → Aspid 🐍 → FastTools → Settings**, по умолчанию она включена и [общая для команды](07-serialize-reference-validation.md#область-проверки).

## Потерянный тип

После переименования, переноса или удаления класса поле показывает `<Missing …>`, а под ним появляется **Missing type**. Данные поля при этом остаются в ассете.

![Потерянная ссылка Game.Gear.Pistoll с кнопками Fix и → Pistol](../../../../docs/Images/aspid_fasttools_serialize_reference_repair.png)

**Fix** открывает выбор класса, и выбранный класс заменяет потерянный. Уведомление может сразу предложить подходящий класс, например **→ Pistol**; причину показывает подсказка. Что Fix сохраняет на ассете и в сцене, описано в разделе [Fix в инспекторе](06-serialize-reference-tooling.md#fix-в-инспекторе).

Все потерянные ссылки в проекте находит [Project References](06-serialize-reference-tooling.md#project-references-восстановить-группу) и восстанавливает их группами. О новых потерях сообщают [проверка перед сборкой](07-serialize-reference-validation.md) и [обнаружение новых поломок](07-serialize-reference-validation.md#обнаружение-новых-поломок).

## Собственный инспектор

Поле с <code lang="csharp">[TypeSelector]</code> в своём редакторе рисует обычный <code lang="class-name">PropertyField</code> — в UI Toolkit и в IMGUI: выбор класса и «+» списка появляются сами. Если элемент списка рисуется отдельно, используйте [SerializeReferenceEditorGUI](https://vpdpersonal.github.io/Aspid.FastTools/ru/api/Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceEditorGUI) или [SerializeReferenceIMGUIList](https://vpdpersonal.github.io/Aspid.FastTools/ru/api/Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceIMGUIList).

## Пример в пакете

Выбор оружия, списки и общие ссылки показаны в примере [SerializeReferences](../../docusaurus-plugin-content-docs-tutorials/current/SerializeReferences/README.md).

![Манекен получает урон в сцене SerializeReferences](../../../../tutorials/SerializeReferences/Images/demo.gif)

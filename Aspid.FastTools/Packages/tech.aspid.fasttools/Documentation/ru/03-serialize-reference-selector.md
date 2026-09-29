# SerializeReference Selector

Реализацию выбирают прямо в инспекторе — из списка с поиском, без своего редактора.

<a id="inspector-type-dropdown"></a>

## Быстрый старт

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>[SerializeReference]&#10;private IWeapon _primary =&#10;    new Pistol();</code></pre> | <pre lang="csharp"><code>[TypeSelector]&#10;[SerializeReference]&#10;private IWeapon _primary;</code></pre> |

## Какие классы в списке

Список строится по типу поля; атрибут может сузить его дополнительными типами.

```csharp
public interface IMelee : IWeapon { }
public interface IRanged : IWeapon { }
public sealed class Sword : IMelee { }

public abstract class StatusEffect { }

public class Modifier<T> : IModifier { }
public sealed class DamageModifier : Modifier<float> { }
```

| Поле <code lang="class-name">Loadout</code> с <code lang="csharp">[TypeSelector]</code> | Классы в списке |
|---|---|
| <code lang="csharp">IWeapon _primary;</code> | <code lang="class-name">Crossbow</code>, <code lang="class-name">Pistol</code>, <code lang="class-name">Railgun</code>, <code lang="class-name">Shotgun</code>, <code lang="class-name">Sword</code> |
| <code lang="csharp">IWeapon _meleeBackup;</code> и <code lang="csharp">typeof(IMelee)</code> | <code lang="class-name">Sword</code> |
| <code lang="csharp">StatusEffect _onHit;</code>, абстрактный класс | <code lang="class-name">BurnEffect</code>, <code lang="class-name">FreezeEffect</code> |
| <code lang="csharp">Modifier&lt;float&gt; _damageModifier;</code> | <code lang="class-name">DamageModifier</code>, <code lang="class-name">Modifier&lt;Single&gt;</code> |
| <code lang="csharp">List&lt;IModifier&gt; _perks;</code> | <code lang="class-name">AmmoModifier</code>, <code lang="class-name">DamageModifier</code>, <code lang="class-name">NameModifier</code>, <code lang="class-name">Modifier&lt;T&gt;</code> с выбором <code lang="class-name">T</code> |

- В списке только конкретные классы, не наследующие <code lang="class-name">UnityEngine.Object</code>.
- В инспекторе runtime-объекта нет классов из editor-only сборок (`UnityEditor`, asmdef только для Editor, папки `Editor`): билд плеера не сможет их создать.
- Аргументы generic-класса выводятся из типа поля; если вывести их нельзя, окно спрашивает каждый и предлагает только типы, которые Unity умеет сериализовать.
- Ограничение можно взять и [из другого поля](02-serializable-types.md#ограничение-из-другого-поля).
- Строку класса в списке настраивает [`[TypeSelectorDisplay]`](02-serializable-types.md#typeselectordisplay).

## Обязательное поле

```csharp
[TypeSelector(Required = true)]
[SerializeReference] private IWeapon _primary;
```

С <code lang="csharp">Required = true</code> пустое поле показывает **Required reference is not set**; подробнее — в разделе [Обязательное поле](02-serializable-types.md#обязательное-поле).

## Списки

В списке с <code lang="csharp">[TypeSelector]</code> кнопка «+» открывает выбор класса и добавляет новый экземпляр, а `<None>` — пустой элемент. При нескольких выбранных объектах каждый получает свой экземпляр, всё в одной группе Undo.

## Смена класса

Новый экземпляр получает значения полей с теми же именами:

| Поле | <code lang="class-name">Pistol</code> | → <code lang="class-name">Shotgun</code> |
|---|---|---|
| **Damage** | 37 | 37 |
| **Magazine Size** | 12 | — |
| **Pellets** | — | 8, начальное значение |

![Смена Pistol на Shotgun сохраняет Damage = 37](../Images/aspid_fasttools_serialize_reference_selector.gif)

Вложенная ссылка с тем же именем переходит в новый класс тем же экземпляром, а не копией.

## Меню заголовка

Правый клик по заголовку поля:

| Пункт | Что делает |
|---|---|
| **Copy Serialize Reference** | Копирует класс и данные поля; копия живёт до перезагрузки домена |
| **Paste Serialize Reference** | Вставляет копию в поле подходящего типа; копия пустого поля очищает его |
| **Make Unique Reference** | Даёт полю собственную копию [общей ссылки](#общие-ссылки); у необщей ссылки пункта нет |
| **Find Usages of Pistol** | Ищет класс в проекте через Unity Search |
| **Link to Existing → …** | Указывает поле на экземпляр из другого поля того же объекта |
| **Create New Script…** | Создаёт <code lang="csharp">[Serializable]</code> класс под тип поля и назначает его после компиляции |
| **Save as Template…** | Сохраняет значение под именем; шаблоны хранятся в настройках редактора на этом компьютере, не в проекте |
| **Paste Template → …** | Создаёт экземпляр из шаблона; в списке только подходящие полю |
| **Paste Template → Remove Missing (N)…** | Удаляет шаблоны, класс которых не загружается; есть, только когда такие шаблоны есть |

Скрипт `.cs`, перетащенный из Project на заголовок, назначает свой класс с переносом данных, как при выборе из списка.

> [!WARNING]
> Copy/Paste и шаблоны не переносят вложенные поля <code lang="csharp">[SerializeReference]</code>: скопированный <code lang="class-name">Railgun</code> вставится без <code lang="csharp">_chargeEffect</code>.

## Общие ссылки

Два поля объекта могут указывать на один экземпляр: правка через одно видна в другом. Такие поля помечены **Shared reference #N**, а **Make unique** даёт полю собственную копию вместе с вложенными ссылками.

![Make unique создаёт независимую копию общей ссылки](../Images/aspid_fasttools_serialize_reference_make_unique.png)

Продублированный элемент списка получает собственный экземпляр, а не ссылку на тот же. За это отвечает настройка **Auto de-alias duplicated list elements** в [общих настройках](04-serialize-reference-tooling.md#общие-и-личные-настройки), по умолчанию она включена.

<a id="repairing-broken-references"></a>

## Восстановление потерянного типа

После переименования, переноса или удаления класса у поля появляется **Missing type**, а данные остаются в ассете.

![Потерянная ссылка с Fix и подсказкой → Pistol в инспекторе](../Images/aspid_fasttools_serialize_reference_repair.png)

| Действие | Что делает |
|---|---|
| **Fix** | Открывает выбор класса, включая скрытые через <code lang="csharp">Hidden</code> |
| **→ Pistol** | Назначает предложенный класс; причина в подсказке: [`[MovedFrom]`](04-serialize-reference-tooling.md#миграции-с-movedfrom), то же имя, то же имя в другом регистре или похожее имя |

На ассете Fix переписывает файл, и Undo его не отменит. В сцене и Prefab Mode исправление остаётся в памяти: Undo его отменяет, а сохранение делает окончательным и очищает историю Undo объекта.

> [!WARNING]
> Исправление в памяти возвращает только простые поля верхнего уровня: вложенные объекты, массивы, списки, векторы и цвета получают значения по умолчанию.

### Когда Fix нет

| Случай | Что делать |
|---|---|
| Выбрано несколько объектов | Выберите один |
| В сцене или Prefab Mode есть несохранённые изменения | Сохраните — Fix появится |
| Экземпляр префаба, класс хранится в исходном префабе | Исправьте исходный префаб, его имя в подсказке |
| Экземпляр префаба, класс задан override | Выберите новый класс на экземпляре или отмените override |
| Ассет префаба в Project, пока префаб открыт в Prefab Mode | Исправьте поле в Prefab Mode |

Остальное исправляет [SerializeReference Tooling](04-serialize-reference-tooling.md).

## Собственный инспектор

Поле с <code lang="csharp">[TypeSelector]</code> в своём редакторе рисует обычный <code lang="class-name">PropertyField</code>: выбор класса и «+» списка появляются сами, вызывать пакет не нужно.

| UI Toolkit — CreateInspectorGUI | IMGUI — OnInspectorGUI |
|---|---|
| <pre lang="csharp"><code>new PropertyField(&#10;    serializedObject&#10;        .FindProperty("_sidearms"))</code></pre> | <pre lang="csharp"><code>EditorGUILayout.PropertyField(&#10;    serializedObject&#10;        .FindProperty("_sidearms"));</code></pre> |

Если у поля <code lang="csharp">[SerializeReference]</code> нет атрибута <code lang="csharp">[TypeSelector]</code> или элемент списка рисуется отдельно через <code lang="function">GetArrayElementAtIndex</code>, <code lang="class-name">PropertyField</code> выбора класса не покажет. Его можно нарисовать в своём редакторе, вызвав один из методов:

| Метод | Что рисует |
|---|---|
| <code lang="csharp">SerializeReferenceEditorGUI.CreateField()</code> | Поле в <code lang="function">CreateInspectorGUI</code> |
| <code lang="csharp">SerializeReferenceEditorGUI.CreateList()</code> | Список в <code lang="function">CreateInspectorGUI</code> |
| <code lang="csharp">SerializeReferenceEditorGUI.DrawFieldLayout()</code> | Поле в <code lang="function">OnInspectorGUI</code> |
| <code lang="csharp">SerializeReferenceIMGUIList.Draw()</code> | Список в <code lang="function">OnInspectorGUI</code> |

Ограничения поверх типа поля передаются аргументом <code lang="csharp">baseTypes</code>, как типы в <code lang="csharp">[TypeSelector(...)]</code>.

## Ограничения

- **Конструктор.** Экземпляр создаётся конструктором без параметров, в том числе непубличным; если его нет — без инициализаторов полей.
- **<code lang="csharp">Allow</code> на <code lang="csharp">[SerializeReference]</code>** не действует — анализатор `AFT0002` предупредит об этом.
- **Пустой выбор.** Если ограничениям не отвечает ни один класс, анализаторы `AFT0003`, `AFT0005` и `AFT0009` предупредят об этом, а на поле с типом из <code lang="class-name">UnityEngine.Object</code> — ошибка `AFT0004`.

## Пример в пакете

Поля <code lang="class-name">Loadout</code> с этой страницы и ассеты с потерянными типами для **Fix** есть в примере [SerializeReferences](../../Samples~/SerializeReferences/Documentation/README.ru.md).

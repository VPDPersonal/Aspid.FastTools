# Ids — пошаговый туториал

Трио Ids даёт человекочитаемые имена в Inspector, за которыми на диске стоят стабильные целочисленные ID:

- `IId` — маркерный интерфейс с единственным свойством `int Id { get; }`; объявление
  `partial struct : IId` запускает `IdStructGenerator`, который генерирует весь сериализационный код.
- `IdRegistry` — `ScriptableObject`, привязывающий один struct-тип к списку строк `(Id, Name)`; он питает
  дропдаун в Inspector и рантайм-поиск `int ↔ string`.
- `[UniqueId]` — edit-time-валидация того, что никакие два ассета не делят один и тот же ID.

Каждый урок соответствует одной секции `STEP` компонента `IdsTutorial`.

## Как открыть туториал

1. Откройте окно Welcome (**Tools → Aspid 🐍 → FastTools → Welcome**) и импортируйте сэмпл **Ids**.
2. Откройте **`Scenes/IdsTutorial.unity`** и выделите GameObject **Ids Tutorial**.

---

## Урок 1 — Объявляем id-struct и выбираем значение

**Поле:** `EnemyId _step1EnemyId` — предустановлено в `walk_enemy_goblin`.

Всё пользовательское объявление — одна строка, см. `Scripts/EnemyId.cs`:

```csharp
[Serializable]
public partial struct EnemyId : IId { }
```

`IdStructGenerator` генерирует `__stringId`, `_id` и свойство `Id` во второй половине partial-типа.

В Inspector поле рисуется как **дропдаун имён**, а не int-поле:

1. Раскройте дропдаун у `_step1EnemyId` — четыре имени врагов приходят из `Data/IdRegistry_EnemyId.asset`.
2. Выберите другую запись, например `fly_enemy_dragon`.

- Сериализуется **стабильный int** (плюс строка имени для миграции) — последующее переименование записи в
  реестре не ломает ссылающиеся на неё ассеты.
- Каждый `IId`-struct привязан ровно к **одному** реестру; `IdRegistryResolver` находит его для дровера.

---

## Урок 2 — Реестр, стоящий за дропдауном

**Поле:** `IdRegistry _step2Registry` — ссылается на `Data/IdRegistry_EnemyId.asset`.

1. Кликните по полю — ассет реестра подсветится в окне Project и автоматически откроется в Inspector.
2. Inspector показывает строки `(Id, Name)`: `1 = fly_enemy_dragon` … `4 = walk_enemy_skeleton`.
3. Добавьте строку, например `swim_enemy_shark` — свежий ID назначится автоматически.
4. Вернитесь к tutorial-объекту: дропдаун из урока 1 теперь предлагает новое имя.

- Реестр создаётся через **Assets → Create → Aspid → Id Registry** и один раз привязывается к struct-типу.
- ID выдаются внутренним счётчиком и никогда не переиспользуются — удаление строки не возвращает её int в оборот.

---

## Урок 3 — Защита от коллизий `[UniqueId]`

**Поле:** `EnemyDefinition _step3Definition` — ссылается на `Data/walk_enemy_goblin.asset`.

`EnemyDefinition._id` помечено `[UniqueId]` (см. `Scripts/EnemyDefinition.cs`), поэтому два ассета
`EnemyDefinition` не могут разрешаться в один ID:

1. Выделите `Data/walk_enemy_goblin.asset` и продублируйте его (**Cmd/Ctrl+D**).
2. Выделите дубликат — Inspector помечает поле `Id`: этот ID уже занят оригиналом.
3. Исправьте, выбрав свободное имя в дропдауне (например, добавленный в уроке 2 `swim_enemy_shark`) —
   предупреждение исчезнет.
4. Закончив, удалите дубликат.

- Проверка работает только в редакторе — в рантайме она ничего не стоит.

---

## Урок 4 — Рантайм-поиск

**Поля:** `string _step4NameToResolve`, `EnemyDefinition[] _step4Catalog`.

Правый клик по заголовку компонента → **Log Tutorial Lookups** (работает в Edit Mode). Console показывает
весь рантайм-API в действии:

```csharp
_registry.TryGetName(_step1EnemyId.Id, out var name); // int → имя
_registry.TryGetId("walk_enemy_orc", out var id);     // имя → int
_registry.Contains(999);                              // проверка членства → false

foreach (var entry in _registry)                      // обход строк (Id, Name) в порядке ассета
    Debug.Log($"{entry.Key} = {entry.Value}");
```

Последняя строка лога разрешает id из урока 1 по `_step4Catalog` — ровно тот же паттерн, что использует
`Scripts/EnemySpawner.cs` в демо-сцене: сравнить `enemy.Id.Id` с целью и обработать совпадение.

- Поменяйте `_step4NameToResolve` на несуществующее имя, чтобы увидеть, как `TryGetId` аккуратно фейлится.
- Поиск идёт по словарям; кэш лениво перестраивается после изменения ассета (`InvalidateCache` /
  `EnsureCache`).

---

## Когда имена или ID меняются

- **Переименование записи реестра** сохраняет её int — все поля, ссылающиеся на этот ID, остаются валидными
  и просто показывают новое имя.
- **Удаление записи реестра** оставляет ссылающиеся поля с int, который больше не разрешается; `TryGetName`
  для него возвращает `false`. Добавьте строку заново или перевыберите значение в поле.
- Сериализованная строка имени в поле — вспомогательный механизм миграции; источник истины — int.

---

## Куда смотреть в коде

| Файл | Показывает |
|---|---|
| `Scripts/Tutorial/IdsTutorial.cs` | Все четыре урока как нумерованные поля + контекстное меню `Log Tutorial Lookups` |
| `Scripts/EnemyId.cs` | Однострочное объявление `partial struct : IId` |
| `Scripts/EnemyDefinition.cs` | `[UniqueId]` на сериализованном поле `EnemyId` |
| `Scripts/EnemySpawner.cs` | Поиск по каталогу по id в реальном компоненте (демо-сцена `Ids.unity`) |
| [README_RU.md](README_RU.md) | Компактный разбор демо-сцены |

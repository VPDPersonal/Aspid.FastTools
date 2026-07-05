# Пример Ids

Демонстрирует связку `IId` / `IdRegistry` / `[UniqueId]`: поля показывают человекочитаемую строку в Inspector, а сериализуются как стабильное целое число, и Inspector ловит коллизии прямо при редактировании.

> **Впервые здесь? Начните с [TUTORIAL_RU.md](TUTORIAL_RU.md)** ([EN](TUTORIAL.md)) — пошаговый гид (уроки 1–4) вокруг `Scripts/Tutorial/IdsTutorial.cs` и `Scenes/IdsTutorial.unity`. Эта страница — разбор демо-сцены; туториал учит самому процессу.

## Как это работает

- `IId` — маркерный интерфейс, объявляющий свойство `int Id { get; }`.
- `IdRegistry` — `ScriptableObject`, связывающий тип-структуру со списком записей `(Id, Name)` и сохраняющий отображение имя ↔ int доступным во рантайме. Property drawer отрисовывает выпадающий список, источником которого является этот реестр.
- `[UniqueId]` — валидирует во время редактирования, что ни два `ScriptableObject`-актива не имеют одинакового результирующего целочисленного ID.

## Сценарий

Каталог врагов. Каждый актив `EnemyDefinition` хранит уникальный `EnemyId` плюс данные для отображения (`_displayName`, `_maxHealth`, `_moveSpeed`). `EnemySpawner` выбирает целевой `EnemyId` через выпадающий список и ищет соответствующий актив в своём каталоге в `Start()`.

Смотрите:

- `Scripts/EnemyId.cs` — `partial struct : IId`. `IdStructGenerator` генерирует `__stringId`, `_id` и свойство `Id`.
- `Scripts/EnemyDefinition.cs:10` — `[UniqueId]` на сериализованном поле `EnemyId` предотвращает дублирование ID между активами.
- `Data/IdRegistry_EnemyId.asset` — реестр, связывающий имена (`fly_enemy_dragon`, `walk_enemy_goblin`, `walk_enemy_orc`, `walk_enemy_skeleton`) со стабильными целочисленными значениями.
- `Scripts/EnemySpawner.cs:9` — выбранный из списка `EnemyId`, преобразуется в `int` во время выполнения через `.Id`.

## Как запустить

Откройте `Scenes/Ids.unity` и войдите в Play Mode — в сцене уже стоит настроенный `EnemySpawner` (из `Prefabs/Ids.prefab`): все четыре актива `Data/*_enemy_*.asset` лежат в его `Catalog`, а `Spawn Target` выставлен в `walk_enemy_orc`. В Console появится лог найденного `EnemyDefinition` (display name, HP, move speed). Меняйте значение в списке `Spawn Target` — источник списка `IdRegistry_EnemyId`, — чтобы увидеть другие варианты поиска.

Чтобы добавить новые записи, откройте `Data/IdRegistry_EnemyId.asset` для строк реестра и `Assets > Create > Aspid > FastTools > Samples > Enemy Definition` — для соответствующих ассетов.

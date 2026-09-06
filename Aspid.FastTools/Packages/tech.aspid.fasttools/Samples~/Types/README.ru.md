# Пример Types

Спавнер врагов, который хранит в одном компоненте три `System.Type` и в Play Mode делает с каждым что-то видимое: какой подкласс `Enemy` спавнить, какой элитный вариант подмешивать и по какому паттерну расставлять волну. Справочник по API — [Serializable Type System](../../Documentation/ru/02-serializable-types.md); эта страница — практический тур.

## Как открыть

1. Импортируйте пример (**Package Manager → Aspid.FastTools → Samples** или **Tools → Aspid 🐍 → FastTools → Welcome**).
2. Откройте `Scenes/Types.unity` и выберите **Enemy Spawner**.
3. Войдите в Play Mode: каждые шесть секунд по кругу появляются восемь капсул, каждая четвёртая — `ArmoredGrunt`, и идут к центру.

## Попробуйте

1. **Тип компонента, переживающий переименование.** `Enemy Type` — это `SerializableMonoScript<Enemy>`: поле ссылается на ассет скрипта, а не на имя класса. Переименуйте класс в `Scripts/Enemies/Grunt.cs` в `Footman` (вместе с файлом), дождитесь компиляции — поле по-прежнему показывает `Footman`. `SerializableType` в той же ситуации стал бы `<Missing>`.
2. **Зависимый пикер.** `Elite Type` — обычная `string` с `[TypeSelector(nameof(_enemyType))]`, поэтому её пикер предлагает только подтипы того, что сейчас лежит в `Enemy Type`. Переключите `Enemy Type` на `Archer` и откройте `Elite Type`: появился `Sniper`, исчез `ArmoredGrunt`.
3. **Внешний вид в пикере.** Откройте `Pattern`. Кандидаты лежат в одной группе **Spawn Patterns** с понятными именами, подсказками и иконками — всё это `[TypeSelectorDisplay]` на классах паттернов. `OriginPattern` не показан, потому что он `Hidden`; `Allow = TypeAllow.None` на поле убирает из списка и сам интерфейс `ISpawnPattern`. Выберите **Grid** и заспавните волну.
4. **Required.** Поставьте `Enemy Type` в `<None>`: появится встроенное предупреждение, а поле станет нарушением для build/CI-гейта из [SerializeReference Tooling](../../Documentation/ru/04-serialize-reference-tooling.md).
5. **Замена компонента на месте.** Выберите **Placed Enemy (swap its type)**. Выпадающий список вверху инспектора даёт поле `ComponentTypeSelector` в `Enemy`. Переключите `Archer` на `Brute`: `Health` и `Speed`, объявленные в общей базе, сохранят значения, а `Keep Distance` (только у Archer) исчезнет.

## Куда смотреть

| Файл | Что показывает |
|---|---|
| `Scripts/EnemySpawner.cs` | `SerializableMonoScript<T>` с `Required`, `[TypeSelector]` со ссылкой на член, `SerializableType<T>` с `Allow = TypeAllow.None`, разрешение каждого через `.Type` / `Type.GetType` |
| `Scripts/Enemies/Enemy.cs` | Поле `ComponentTypeSelector` в базовом классе; подклассы в той же папке |
| `Scripts/Spawning/*.cs` | Обычные C#-стратегии с `[TypeSelectorDisplay]` (`Name`, `Group`, `Tooltip`, `Icon`, `Hidden`) |

См. также: [пример SerializeReferences](../SerializeReferences/README.ru.md) — `[TypeSelector]` на полях `[SerializeReference]`, [пример EditorTools](../EditorTools/README.ru.md) — тот же пикер из собственного editor-кода.

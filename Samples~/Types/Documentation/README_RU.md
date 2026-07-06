# Пример Types

Маленькая система способностей, демонстрирующая полиморфный выбор типа в Unity Inspector с помощью `SerializableType<T>`, `TypeSelectorAttribute` и `ComponentTypeSelector`. Игрок выбирает наследника `Ability` и список наследников `AbilityModifier`; для врагов используется `ComponentTypeSelector`, чтобы конкретный скрипт врага можно было заменить «на лету» из Inspector.

> **Впервые здесь? Начните с [TUTORIAL_RU.md](TUTORIAL_RU.md)** ([EN](TUTORIAL.md)) — пошаговый гид (уроки 1–6) на основе `Scripts/Tutorial/TypesTutorial.cs` и `Scenes/TypesTutorial.unity`. Эта страница — обзор демо-сцены; туториал учит рабочему процессу.

Смотрите:

- `Scripts/Abilities/AbilitySelector.cs:20` — поле `SerializableType<Ability>`, ограниченный выбор одного подтипа.
- `Scripts/Abilities/AbilitySelector.cs:25` — `[TypeSelector(typeof(AbilityModifier))]` на поле `string[]`.
- `Scripts/Enemies/EnemyBase.cs:18` — объявление `ComponentTypeSelector`, заменяющее прикреплённый скрипт по месту.

## Как запустить

Откройте `Scenes/Types.unity` — в сцене два prefab-инстанса:

- **AbilitySelector** (`Prefabs/AbilitySelector.prefab`) — `AbilitySelector` с предвыбранной способностью `Dash` и тремя заполненными модификаторами. Войдите в Play Mode, чтобы увидеть в Console лог активированной способности и каждого применённого модификатора.
- **Enemy** (`Prefabs/Enemy.prefab`) — `FastEnemy`, подключённый через `ComponentTypeSelector`. Выделите его в Hierarchy и используйте выпадающий список выбора типа в верхней части Inspector, чтобы переключиться между `FastEnemy` и `TankEnemy` по месту; значение `Health` сохраняется при замене.

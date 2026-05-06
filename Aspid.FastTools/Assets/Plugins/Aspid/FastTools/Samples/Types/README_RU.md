# Пример Types

Маленькая система способностей, демонстрирующая полиморфный выбор типа в Unity Inspector с помощью `SerializableType<T>`, `TypeSelectorAttribute` и `ComponentTypeSelector`. Игрок выбирает наследника `Ability` и список наследников `AbilityModifier`; для врагов используется `ComponentTypeSelector`, чтобы конкретный скрипт врага можно было заменить «на лету» из Inspector.

Смотрите:

- `Scripts/Abilities/AbilitySelector.cs:20` — поле `SerializableType<Ability>`, ограниченный выбор одного подтипа.
- `Scripts/Abilities/AbilitySelector.cs:26` — `[TypeSelector(typeof(AbilityModifier))]` на поле `string[]`.
- `Scripts/Enemies/EnemyBase.cs:18` — объявление `ComponentTypeSelector`, заменяющее прикреплённый скрипт по месту.

Оба Type-drawer’а поддерживают и UIToolkit, и IMGUI. Параллельные `IMGUI*`-варианты принудительно используют IMGUI-путь — удобно для сравнения или миграции IMGUI-проектов:

- `Scripts/Abilities/IMGUIAbilitySelector.cs` + `Scripts/Editor/IMGUIAbilityHolderEditor.cs` — те же поля `SerializableType<T>` / `[TypeSelector]`, отрисованные через `OnInspectorGUI`.
- `Scripts/Enemies/IMGUI/IMGUIEnemyBase.cs` (+ `IMGUIFastEnemy`, `IMGUITankEnemy`) + `Scripts/Editor/IMGUIEnemyBaseEditor.cs` — IMGUI-эквивалент потока подмены через `ComponentTypeSelector`.

## Как запустить

Откройте `Scenes/Types.unity` — в сцене два prefab-инстанса:

- **AbilitySelector** (`Prefabs/AbilitySelector.prefab`) — `AbilitySelector` с предвыбранной способностью `Heal` и тремя заполненными модификаторами. Войдите в Play Mode, чтобы увидеть в Console лог активированной способности и каждого применённого модификатора.
- **Enemy** (`Prefabs/Enemy.prefab`) — `FastEnemy`, подключённый через `ComponentTypeSelector`. Выделите его в Hierarchy и используйте выпадающий список выбора типа в верхней части Inspector, чтобы переключиться между `FastEnemy` и `TankEnemy` по месту; значение `Health` сохраняется при замене.

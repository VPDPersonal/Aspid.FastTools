# Пример Types

Маленькая система способностей, демонстрирующая полиморфный выбор типа в Unity Inspector с помощью `SerializableType<T>`, `TypeSelectorAttribute` и `ComponentTypeSelector`. Игрок выбирает наследника `Ability` и список наследников `AbilityModifier`; для врагов используется `ComponentTypeSelector`, чтобы конкретный скрипт врага можно было заменить «на лету» из Inspector.

Смотрите:

- `Scripts/AbilitySelector.cs:21` — поле `SerializableType<Ability>`, ограниченный выбор одного подтипа.
- `Scripts/AbilitySelector.cs:26` — `[TypeSelector(typeof(AbilityModifier), AllowAbstractTypes = false)]` на поле `string[]`.
- `Scripts/Enemies/EnemyBase.cs:18` — объявление `ComponentTypeSelector`, заменяющее прикреплённый скрипт по месту.

Для запуска: откройте `Scenes/Types.unity` (будет создана в Unity) и нажмите Play.

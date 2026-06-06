# Пример SerializeReferences

Маленькая система снаряжения, демонстрирующая `[SerializeReferenceSelector]` — иерархический выпадающий список с поиском для полей `[SerializeReference]`. Вы прямо в Inspector выбираете, какая конкретная реализация полиморфного поля будет создана.

Смотрите:

- `Scripts/Loadout.cs` — одиночное поле (`IWeapon`), `List<IWeapon>` и поле с абстрактным базовым типом (`StatusEffect`), каждое с `[SerializeReference]` и `[SerializeReferenceSelector]`.
- `Scripts/Weapons/` — интерфейс `IWeapon` и его реализации (`Pistol`, `Shotgun`, `Railgun`). `Railgun` вкладывает ещё одно поле `[SerializeReferenceSelector]` — показывает рекурсивное полиморфное редактирование.
- `Scripts/Effects/` — абстрактный базовый `StatusEffect` с `BurnEffect` / `FreezeEffect`. В списке предлагаются только конкретные подтипы; абстрактный базовый класс никогда не показывается.

Drawer поддерживает и UIToolkit, и IMGUI. Вариант `IMGUILoadout` принудительно использует IMGUI-путь — удобно для сравнения или миграции IMGUI-проектов:

- `Scripts/IMGUILoadout.cs` + `Scripts/Editor/IMGUILoadoutEditor.cs` — те же поля, отрисованные через `OnInspectorGUI` (`SerializeReferenceIMGUIPropertyDrawer`).

## Как запустить

В `Prefabs/` лежат два готовых префаба — дважды кликните, чтобы открыть в Prefab Mode, или перетащите любой в сцену:

- **Loadout** (`Prefabs/Loadout.prefab`) — путь UIToolkit. Предзаполнено: `Primary Weapon = Railgun` (с вложенным эффектом заряда `BurnEffect`), `Sidearms = [Pistol, Shotgun]`, `On Hit Effect = FreezeEffect`.
- **IMGUILoadout** (`Prefabs/IMGUILoadout.prefab`) — путь IMGUI. Предзаполнено: `Primary Weapon = Pistol`, `On Hit Effect = BurnEffect`.

Дальше поэкспериментируйте с выпадающими списками:

1. Кликните по любому дропдауну типа и выберите другую реализацию — экземпляр создастся, а его сериализуемые поля появятся вложенно под foldout.
2. Разверните `Railgun` и смените вложенный `Charge Effect` — увидите рекурсивное полиморфное редактирование.
3. Нажмите **+** на `Sidearms` и задайте каждому элементу свой тип оружия.
4. Откройте `On Hit Effect` — обратите внимание, что предлагаются только `BurnEffect` / `FreezeEffect` (абстрактный `StatusEffect` скрыт).
5. ПКМ по заголовку компонента → **Log Loadout**, чтобы вывести настроенное снаряжение в Console.

Хотите собрать с нуля? Добавьте пустой GameObject и прикрепите компонент **Loadout** (UIToolkit) или **IMGUILoadout** (IMGUI).

Переключение поля обратно на `<None>` очищает ссылку. Если сохранённый тип позже переименуют или удалят, в списке появится подпись `<Missing …>` и предупреждение, вместо тихой очистки.

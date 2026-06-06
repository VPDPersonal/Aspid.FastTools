# Пример SerializeReferences

Маленькая система снаряжения, демонстрирующая `[SerializeReferenceSelector]` — иерархический выпадающий список с поиском для полей `[SerializeReference]`. Вы прямо в Inspector выбираете, какая конкретная реализация полиморфного поля будет создана.

Смотрите:

- `Scripts/Loadout.cs` — одиночное поле (`IWeapon`), `List<IWeapon>` и поле с абстрактным базовым типом (`StatusEffect`), каждое с `[SerializeReference]` и `[SerializeReferenceSelector]`.
- `Scripts/Weapons/` — интерфейс `IWeapon` и его реализации (`Pistol`, `Shotgun`, `Railgun`). `Railgun` вкладывает ещё одно поле `[SerializeReferenceSelector]` — показывает рекурсивное полиморфное редактирование.
- `Scripts/Effects/` — абстрактный базовый `StatusEffect` с `BurnEffect` / `FreezeEffect`. В списке предлагаются только конкретные подтипы; абстрактный базовый класс никогда не показывается.

Drawer поддерживает и UIToolkit, и IMGUI. Вариант `IMGUILoadout` принудительно использует IMGUI-путь — удобно для сравнения или миграции IMGUI-проектов:

- `Scripts/IMGUILoadout.cs` + `Scripts/Editor/IMGUILoadoutEditor.cs` — те же поля, отрисованные через `OnInspectorGUI` (`SerializeReferenceIMGUIPropertyDrawer`).

## Как запустить

1. Создайте пустой GameObject в любой сцене и добавьте компонент **Loadout** (путь UIToolkit) или **IMGUILoadout** (путь IMGUI).
2. В Inspector кликните по выпадающему списку `<None>` и выберите реализацию — например, `Primary Weapon → Railgun`. Экземпляр создастся, а его сериализуемые поля появятся вложенно под foldout.
3. Выберите вложенный у `Railgun` `Charge Effect → BurnEffect` — увидите рекурсивное полиморфное редактирование.
4. Нажмите **+** на `Sidearms` и задайте каждому элементу свой тип оружия.
5. Задайте `On Hit Effect` — обратите внимание, что предлагаются только `BurnEffect` / `FreezeEffect` (абстрактный `StatusEffect` скрыт).
6. ПКМ по заголовку компонента → **Log Loadout**, чтобы вывести настроенное снаряжение в Console.

Переключение поля обратно на `<None>` очищает ссылку. Если сохранённый тип позже переименуют или удалят, в списке появится подпись `<Missing …>` и предупреждение, вместо тихой очистки.

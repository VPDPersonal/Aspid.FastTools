# Пример SerializeReferences

Маленькая система снаряжения, демонстрирующая `[SerializeReferenceSelector]` — иерархический выпадающий список с поиском для полей `[SerializeReference]`. Вы прямо в Inspector выбираете, какая конкретная реализация полиморфного поля будет создана.

Смотрите:

- `Scripts/Loadout.cs` — одиночное поле (`IWeapon`), `List<IWeapon>` и поле с абстрактным базовым типом (`StatusEffect`), каждое с `[SerializeReference]` и `[SerializeReferenceSelector]`.
- `Scripts/Weapons/` — интерфейс `IWeapon` и его реализации (`Pistol`, `Shotgun`, `Railgun`). `Railgun` вкладывает ещё одно поле `[SerializeReferenceSelector]` — показывает рекурсивное полиморфное редактирование.
- `Scripts/Effects/` — абстрактный базовый `StatusEffect` с `BurnEffect` / `FreezeEffect`. В списке предлагаются только конкретные подтипы; абстрактный базовый класс никогда не показывается.
- `Scripts/Modifiers/` — generic-иерархия: неабстрактный generic-класс `Modifier<T>` (`IModifier`) с закрытыми подтипами `DamageModifier : Modifier<float>`, `AmmoModifier : Modifier<int>`, `NameModifier : Modifier<string>`. Поле `IModifier` предлагает все три подтипа **и** сам открытый `Modifier<T>` — при его выборе открывается второе окно для выбора аргумента `T`. Поле `Modifier<float>` предлагает только присваиваемых кандидатов (`DamageModifier` и `Modifier<T>` с выведенным `T = float`).
- `Scripts/WeaponPreset.cs` + `Presets/BrokenWeaponPreset.asset` — `ScriptableObject`, у которого поле `_weapon` указывает на несуществующий тип; используется для демонстрации починки потерянного типа (см. *Сервисные функции* ниже).

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
5. Откройте `Modifier` — рядом с тремя конкретными подтипами (`DamageModifier`, `AmmoModifier`, `NameModifier`) предлагается и открытый `Modifier<T>`. Выберите `Modifier<T>` — откроется второе окно для выбора аргумента `T` (попробуйте `string`, затем `float`), и только потом создастся экземпляр. Откройте `Float Modifier` — предлагаются только присваиваемые к `Modifier<float>` кандидаты (`DamageModifier` и `Modifier<T>` с выведенным `T = float`, без дополнительного окна).
6. ПКМ по заголовку компонента → **Log Loadout**, чтобы вывести настроенное снаряжение в Console.

Хотите собрать с нуля? Добавьте пустой GameObject и прикрепите компонент **Loadout** (UIToolkit) или **IMGUILoadout** (IMGUI).

Переключение поля обратно на `<None>` очищает ссылку. Если сохранённый тип позже переименуют или удалят, в списке появится подпись `<Missing …>` и предупреждение, вместо тихой очистки.

## Сервисные функции

Drawer также помогает восстановиться после двух типичных поломок managed-ссылки.

### Copy / Paste и сохранение данных

- **ПКМ** по заголовку любого селектора → **Copy Serialize Reference** / **Paste Serialize Reference**. Вставка создаёт *независимый* экземпляр в целевом поле и неактивна, если скопированный тип не подходит полю.
- **Смена типа** сохраняет поля, общие у старой и новой реализации. Поставьте `Sidearms[0] = Pistol`, задайте урон, переключите на `Shotgun` и обратно — значение `Pistol` сохранится.

### Починка потерянного типа — `BrokenWeaponPreset.asset` и `LoadoutMissingType.prefab`

Два ассета поставляются заранее сломанными, со ссылками на несуществующие классы:

- `Presets/BrokenWeaponPreset.asset` — `ScriptableObject`, поле `Weapon` ссылается на потерянный `GhostWeapon`.
- `Prefabs/LoadoutMissingType.prefab` — префаб, `Sidearms → Element 0` ссылается на потерянный `GhostPistol`.

Выделите любой **в окне Project**. У потерянного поля будет подпись `<Missing …>`, предупреждение **Missing type** и кнопка **Fix**:

1. Нажмите **Fix** — откроется привычный селектор типов с поиском. Выберите `Pistol`.
2. Ссылка восстановится в `Pistol` с сохранёнными данными (префаб сохранит `_damage = 15`, `_magazineSize = 12`; ассет — `_damage = 25`, `_magazineSize = 8`). Выбор типа переписывает сохранённый тип в файле ассета, а не создаёт экземпляр заново — поэтому значения сохраняются.

> Починка читает и переписывает файл ассета напрямую — Unity не отдаёт потерянный тип через свой serialization API (а на GameObject/префабах ещё и обнуляет его в живом объекте, UUM-129100), поэтому осиротевшие тип и данные восстанавливаются прямо из YAML. Значит нужен **сохранённый файл ассета**: работает для ScriptableObject и префаб-ассетов, выделенных в Project, но не для объектов в Prefab Mode или экземпляров в сцене (нет файла ассета для перезаписи).

### Расцепление общей ссылки — `LoadoutSharedRef.prefab`

В `Prefabs/LoadoutSharedRef.prefab` оба элемента `Sidearms` ссылаются на **один и тот же** экземпляр (это же состояние получается дублированием элемента массива).

1. Откройте его — оба элемента показывают пометку **shared reference**; редактирование одного меняет другой.
2. **ПКМ** по элементу → **Make Unique Reference**. Он получит собственную копию данных, и поля станут независимыми.

# Пример SerializeReferences

Маленькая система снаряжения, демонстрирующая `[TypeSelector]` — иерархический выпадающий список с поиском для полей `[SerializeReference]`. Вы прямо в Inspector выбираете, какая конкретная реализация полиморфного поля будет создана.

> **Впервые здесь? Начните с [TUTORIAL_RU.md](TUTORIAL_RU.md)** ([EN](TUTORIAL.md)) — пошаговый разбор (уроки 1–8) вокруг `Scripts/Tutorial/TypeSelectorTutorial.cs` и `Scenes/TypeSelectorTutorial.unity`. Эта страница — справочник по фичам, а туториал — пошаговое прохождение.

Смотрите:

- `Scripts/Loadout.cs` — одиночное поле (`IWeapon`), `List<IWeapon>` и поле с абстрактным базовым типом (`StatusEffect`), каждое с `[SerializeReference]` и `[TypeSelector]`.
- `Scripts/Weapons/` — интерфейс `IWeapon` и его реализации (`Sword`, `Pistol`, `Shotgun`, `Railgun`). `Railgun` вкладывает ещё одно поле `[TypeSelector]` — показывает рекурсивное полиморфное редактирование.
- `Scripts/Effects/` — абстрактный базовый `StatusEffect` с `BurnEffect` / `FreezeEffect`. В списке предлагаются только конкретные подтипы; абстрактный базовый класс никогда не показывается.
- `Scripts/Modifiers/` — generic-иерархия: неабстрактный generic-класс `Modifier<T>` (`IModifier`) с закрытыми подтипами `DamageModifier : Modifier<float>`, `AmmoModifier : Modifier<int>`, `NameModifier : Modifier<string>`. Поле `IModifier` предлагает все три подтипа **и** сам открытый `Modifier<T>` — при его выборе открывается вторая страница внутри того же окна для выбора аргумента `T`. Поле `Modifier<float>` предлагает только присваиваемых кандидатов (`DamageModifier` и `Modifier<T>` с выведенным `T = float`).
- `Scripts/WeaponPreset.cs` + `Presets/BrokenWeaponPreset.asset` — `ScriptableObject`, у которого поле `_weapon` указывает на несуществующий тип; используется для демонстрации починки потерянного типа (см. *Сервисные функции* ниже).

Drawer поддерживает и UIToolkit, и IMGUI на полном паритете функций. **У каждого демо-префаба есть `IMGUI…`-двойник** с теми же данными, но принудительно на IMGUI-пути — так можно сравнить оба рендерера (или мигрировать IMGUI-проект) для любого сценария ниже:

| Префаб UIToolkit | IMGUI-двойник |
|---|---|
| `Loadout.prefab` | `IMGUILoadout.prefab` |
| `SlottedLoadout.prefab` | `IMGUISlottedLoadout.prefab` |
| `LoadoutMissingType.prefab` | `IMGUILoadoutMissingType.prefab` |
| `NestedLoadout.prefab` | `IMGUINestedLoadout.prefab` |
| `LoadoutSharedRef.prefab` | `IMGUILoadoutSharedRef.prefab` |

Двойник подставляет соседний компонент (`IMGUILoadout` / `IMGUISlottedLoadout`), чей companion-editor переопределяет `OnInspectorGUI` без `CreateInspectorGUI` — этого достаточно, чтобы каждое вложенное поле `[TypeSelector]` пошло через `SerializeReferenceIMGUIPropertyDrawer`, а не через UIToolkit-путь `CreatePropertyGUI`. Образец — `Scripts/IMGUILoadout.cs` + `Scripts/Editor/IMGUILoadoutEditor.cs`.

## Как запустить

В `Prefabs/` лежат готовые префабы — дважды кликните, чтобы открыть в Prefab Mode, или перетащите любой в сцену. Начните с пары **Loadout**:

- **Loadout** (`Prefabs/Loadout.prefab`) — путь UIToolkit. Предзаполнено: `Primary Weapon = Railgun` (с вложенным эффектом заряда `BurnEffect`), `Sidearms = [Pistol, Shotgun]`, `On Hit Effect = FreezeEffect`.
- **IMGUILoadout** (`Prefabs/IMGUILoadout.prefab`) — путь IMGUI. Те же данные, что у **Loadout**, но отрисованные через IMGUI-drawer — удобно сравнить оба пути бок о бок.

У каждого сценарного префаба из **Сервисных функций** ниже (`SlottedLoadout`, `LoadoutMissingType`, `NestedLoadout`, `LoadoutSharedRef`) тоже есть `IMGUI…`-двойник — откройте любой рендерер, чтобы увидеть те же данные обоими путями.

Дальше поэкспериментируйте с выпадающими списками:

1. Кликните по любому дропдауну типа и выберите другую реализацию — экземпляр создастся, а его сериализуемые поля появятся вложенно под foldout.
2. Разверните `Railgun` и смените вложенный `Charge Effect` — увидите рекурсивное полиморфное редактирование.
3. Нажмите **+** на `Sidearms` и задайте каждому элементу свой тип оружия.
4. Откройте `On Hit Effect` — обратите внимание, что предлагаются только `BurnEffect` / `FreezeEffect` (абстрактный `StatusEffect` скрыт).
5. Откройте `Modifier` — рядом с тремя конкретными подтипами (`DamageModifier`, `AmmoModifier`, `NameModifier`) предлагается и открытый `Modifier<T>`. Выберите `Modifier<T>` — откроется вторая страница внутри того же окна для выбора аргумента `T` (попробуйте `string`, затем `float`), и только потом создастся экземпляр. Откройте `Float Modifier` — предлагаются только присваиваемые к `Modifier<float>` кандидаты (`DamageModifier` и `Modifier<T>` с выведенным `T = float`, без дополнительной страницы).
6. ПКМ по заголовку компонента → **Log Loadout**, чтобы вывести настроенное снаряжение в Console.

Хотите собрать с нуля? Добавьте пустой GameObject и прикрепите компонент **Loadout** (UIToolkit) или **IMGUILoadout** (IMGUI).

Переключение поля обратно на `<None>` очищает ссылку. Если сохранённый тип позже переименуют или удалят, в списке появится подпись `<Missing …>` и предупреждение, вместо тихой очистки.

## Сервисные функции

Drawer также помогает восстановиться после двух типичных поломок managed-ссылки.

### Copy / Paste и сохранение данных

- **ПКМ** по заголовку любого селектора → **Copy Serialize Reference** / **Paste Serialize Reference**. Вставка создаёт *независимый* экземпляр в целевом поле и неактивна, если скопированный тип не подходит полю.
- **Смена типа** сохраняет поля, общие у старой и новой реализации. Поставьте `Sidearms[0] = Pistol`, задайте урон, переключите на `Shotgun` и обратно — значение `Pistol` сохранится.

### Починка потерянного типа — `BrokenWeaponPreset.asset` и `LoadoutMissingType.prefab`

Три ассета поставляются заранее сломанными, со ссылками на несуществующие классы:

- `Presets/BrokenWeaponPreset.asset` — `ScriptableObject`, поле `Weapon` ссылается на потерянный `GhostWeapon`.
- `Presets/BrokenArsenalPreset.asset` — второй `ScriptableObject`, который тоже ссылается на потерянный `GhostWeapon`, причём трижды (`Weapon` плюс два элемента `Alternates`), поэтому делит сломанный тип с `BrokenWeaponPreset.asset`.
- `Prefabs/LoadoutMissingType.prefab` — префаб, `Sidearms → Element 0` ссылается на потерянный `GhostPistol` (его IMGUI-двойник `Prefabs/IMGUILoadoutMissingType.prefab` ломает тот же слот через IMGUI-рендерер).

Выделите любой **в окне Project**. У потерянного поля будет подпись `<Missing …>`, предупреждение **Missing type** и кнопка **Fix**:

1. Нажмите **Fix** — откроется привычный селектор типов с поиском. Выберите `Pistol`.
2. Ссылка восстановится в `Pistol` с сохранёнными данными (префаб сохранит `_damage = 15`, `_magazineSize = 12`; ассет — `_damage = 25`, `_magazineSize = 8`). Выбор типа переписывает сохранённый тип в файле ассета, а не создаёт экземпляр заново — поэтому значения сохраняются.

> Починка читает и переписывает файл ассета напрямую — Unity не отдаёт потерянный тип через свой serialization API (а на GameObject/префабах ещё и обнуляет его в живом объекте, UUM-129100), поэтому осиротевшие тип и данные восстанавливаются прямо из YAML. Работает для ScriptableObject и префаб-ассетов, выделенных в Project (переписывается их YAML), для объектов в **Prefab Mode** (чинится на живом экземпляре) и для объектов в **сохранённой чистой сцене** (находятся через `GlobalObjectId`) — но не для **несохранённой/грязной сцены** или **переопределения на экземпляре префаба**, у которых нет зафиксированного файла ассета для маппинга ссылки.
>
> Если потерянная ссылка вложена в другое значение или лежит на дочернем объекте, до которого не добраться в инспекторе — используйте **`Tools → Aspid 🐍 → FastTools → Asset References`**: вкладка сканирует весь файл ассета и выводит все потерянные ссылки (любой глубины, на любом дочернем объекте), каждую со своим **Fix**.
>
> Вкладка **Project References** проходит по всем ассетам в `Assets/` и группирует потерянные ссылки по сохранённому типу — поэтому `BrokenWeaponPreset.asset` и `BrokenArsenalPreset.asset` сворачиваются в одну группу **GhostWeapon** (`4 entries · 2 files`). Одна кнопка **Fix all** выбирает единственную замену и перенаправляет все вхождения сразу в обоих файлах.

### Карта вложенного графа — `NestedLoadout.prefab`

`Prefabs/NestedLoadout.prefab` — трёхуровневая иерархия `NestedLoadout → WeaponSlot → BackupSlot`, на **каждом** объекте свой `Loadout`, так что на каждом дочернем объекте есть потерянная ссылка, до которой не добраться в инспекторе вне Prefab Mode:

- **NestedLoadout** (корень) — `Primary Weapon = Railgun` (с вложенным эффектом заряда `BurnEffect`), `Sidearms = [GhostPistol (потерян), <None> (пустой слот)]`, `On Hit Effect = FreezeEffect`.
- **WeaponSlot** (дочерний) — `Primary Weapon = GhostBlade` (потерян), `Sidearms[0] = Pistol`.
- **BackupSlot** (внук) — `On Hit Effect = GhostAura` (потерян), `Primary Weapon = Shotgun`.

Выделите его **в окне Project** и откройте вкладку **Asset References** — **`Tools → Aspid 🐍 → FastTools → Asset References`**. Граф строит сразу все три компонента (по документу на объект). Каждая ссылка — инлайн-дропдаун: выберите тип, чтобы присвоить / перенаправить её, или `<None>`, чтобы очистить; у потерянных `GhostPistol` / `GhostBlade` / `GhostAura` карточек — янтарное действие **Fix Missing**. Вложенность читается по пути поля (`_primaryWeapon._chargeEffect`), а не по отступу, поэтому плоский список карточек остаётся читаемым.

Его IMGUI-двойник `Prefabs/IMGUINestedLoadout.prefab` повторяет тот же трёхуровневый граф — окно **Asset References** не зависит от рендерера, поэтому строит их одинаково, тогда как корневой инспектор двойника принудительно идёт по IMGUI-пути.

### Расцепление общих ссылок и различение групп по цвету — `LoadoutSharedRef.prefab`

В `Prefabs/LoadoutSharedRef.prefab` на одном объекте — **две независимые** пары общих ссылок (каждую пару можно также получить дублированием элемента массива), поэтому цветовая полоска/нотис по rid здесь действительно полезна:

- `Sidearms[0]` и `Sidearms[1]` ссылаются на один и тот же `Pistol` — один цвет.
- `Primary Weapon → Charge Effect` и `On Hit Effect` ссылаются на один и тот же `BurnEffect` — другой цвет, хотя одно поле лежит на три уровня вложенности глубже, а другое — на верхнем уровне.

1. Откройте его — каждая пара показывает пометку **shared reference**, и редактирование одного элемента меняет его партнёра. Совпадающий цвет полоски/нотиса означает один и тот же экземпляр, независимо от места поля в иерархии, поэтому две пары читаются как два разных цвета.
2. **ПКМ** по элементу → **Make Unique Reference**. Он получит собственную копию данных, и поля станут независимыми — его пометка исчезнет, как и у бывшего партнёра, ведь ничего больше не общее.

Его IMGUI-двойник `Prefabs/IMGUILoadoutSharedRef.prefab` показывает те же две пары через IMGUI-рендерер.

# Пример SerializeReferences

Маленькая система снаряжения, демонстрирующая `[TypeSelector]` — иерархический выпадающий список с поиском для полей `[SerializeReference]`. Поставьте оба атрибута на одно поле — и прямо в Inspector выбираете, какая конкретная реализация будет создана; `<None>` очищает ссылку, а сериализуемые поля экземпляра появляются вложенно под foldout.

```csharp
[SerializeReference] [TypeSelector]
private IWeapon _weapon;
```

> **Впервые здесь? Начните с [TUTORIAL_RU.md](TUTORIAL_RU.md)** ([EN](TUTORIAL.md)) — пошаговый разбор (уроки 1–8) вокруг `Scripts/Tutorial/TypeSelectorTutorial.cs` и `Scenes/TypeSelectorTutorial.unity`. Эта страница — справочник по фичам, а туториал — пошаговое прохождение.

<!-- TODO(media): aspid_fasttools_type_selector_window.png is re-shot together with README (same shared file) -->
![Окно выбора типа](../../Documentation/Images/aspid_fasttools_type_selector_window.png)

*То же окно-селектор с поиском, здесь показано на другом списке кандидатов — ваши поля открывают его отфильтрованным под собственную иерархию типов.*

## Как запустить

В `Prefabs/` лежат готовые демо-префабы — дважды кликните, чтобы открыть в Prefab Mode, или перетащите любой в сцену. Начните с **`Loadout.prefab`** — он предзаполнен: `Primary Weapon = Railgun` (с вложенным эффектом заряда `BurnEffect`), `Sidearms = [Pistol, Shotgun]`, `On Hit Effect = FreezeEffect`. Дальше поэкспериментируйте:

1. Кликните по любому дропдауну типа и выберите другую реализацию — экземпляр создастся, а его сериализуемые поля появятся вложенно под foldout.
2. Разверните `Railgun` и смените вложенный `Charge Effect` — увидите рекурсивное полиморфное редактирование.
3. Нажмите **+** на `Sidearms` и задайте каждому элементу свой тип оружия.
4. Откройте `On Hit Effect` — обратите внимание, что предлагаются только `BurnEffect` / `FreezeEffect` (абстрактный `StatusEffect` скрыт).
5. Откройте `Modifier` — селектор предлагает конкретные подтипы **и** открытый `Modifier<T>`; при выборе `Modifier<T>` внутри того же окна открывается вторая страница для выбора `T`. Откройте `Float Modifier` — предлагаются только присваиваемые к `Modifier<float>` кандидаты, `T` выводится сам (без дополнительной страницы). Полный разбор — [TUTORIAL_RU.md, урок 6](TUTORIAL_RU.md#урок-6--generic-иерархии).
6. ПКМ по заголовку компонента → **Log Loadout**, чтобы вывести настроенное снаряжение в Console.

Хотите собрать с нуля? Добавьте пустой GameObject и прикрепите компонент **Loadout**.

Переключение поля обратно на `<None>` очищает ссылку. Если сохранённый тип позже переименуют или удалят, в списке появится подпись `<Missing …>` и предупреждение, вместо тихой очистки.

### Демо-префабы

| Префаб | Показывает |
|---|---|
| `Loadout.prefab` | Все виды полей: одиночное, список, абстрактная база, generics, вложенность |
| `SlottedLoadout.prefab` | Ссылки внутри обычных `[Serializable]`-контейнеров (урок 7) |
| `LoadoutMissingType.prefab` | Предупреждение о потерянном типе и инлайн-**Fix** |
| `NestedLoadout.prefab` | Трёхуровневая иерархия для графа **Asset References** |
| `LoadoutSharedRef.prefab` | Пары общих ссылок, цветовая кодировка, **Make Unique Reference** |
| `IMGUILoadout.prefab` | Те же данные, что у `Loadout.prefab`, но принудительно через IMGUI-рендерер (см. *Путь IMGUI* ниже) |

## Что в коде

- `Scripts/Loadout.cs` — одиночное поле (`IWeapon`), `List<IWeapon>`, поле с абстрактным базовым типом (`StatusEffect`) и generic-поля (`IModifier` / `Modifier<float>`), каждое с `[SerializeReference]` и `[TypeSelector]`.
- `Scripts/Weapons/` — интерфейс `IWeapon` с ветками `IMelee` / `IRanged` и реализациями (`Sword`, `Pistol`, `Shotgun`, `Railgun`, `Crossbow`). `Railgun` вкладывает ещё одно поле `[TypeSelector]`; `Crossbow` несёт `[MovedFrom]`, используемый демо миграции.
- `Scripts/Effects/` — абстрактный базовый `StatusEffect` с `BurnEffect` / `FreezeEffect`. В списке предлагаются только конкретные подтипы.
- `Scripts/Modifiers/` — generic-иерархия: открытый `Modifier<T>` и закрытые подтипы над `float` / `int` / `string` ([TUTORIAL_RU.md, урок 6](TUTORIAL_RU.md#урок-6--generic-иерархии)).
- `Scripts/WeaponSlot.cs` — обычный `[Serializable]`-контейнер, общий для `SlottedLoadout` и урока 7 туториала.
- `Scripts/WeaponPreset.cs` + `Presets/` — `ScriptableObject`-ы с намеренно сломанными или устаревшими идентичностями типов, используемые потоками починки ниже.

## Сервисные функции

Drawer также помогает восстановиться после типичных поломок managed-ссылки.

### Copy / Paste и сохранение данных

- **ПКМ** по заголовку любого селектора → **Copy Serialize Reference** / **Paste Serialize Reference**. Вставка создаёт *независимый* экземпляр в целевом поле и неактивна, если скопированный тип не подходит полю.
- **Смена типа** сохраняет поля, общие у старой и новой реализации. Поставьте `Sidearms[0] = Pistol`, задайте урон, переключите на `Shotgun` и обратно — значение `Pistol` сохранится.

### Починка потерянного типа — `BrokenWeaponPreset.asset` и `LoadoutMissingType.prefab`

Пять ассетов поставляются с сохранёнными идентичностями типов, которые больше не резолвятся напрямую:

- `Presets/BrokenWeaponPreset.asset` — `ScriptableObject`, поле `Weapon` ссылается на потерянный `GhostWeapon`.
- `Presets/BrokenArsenalPreset.asset` — второй `ScriptableObject`, который тоже ссылается на потерянный `GhostWeapon`, причём трижды (`Weapon` плюс два элемента `Alternates`), поэтому делит сломанный тип с `BrokenWeaponPreset.asset`.
- `Prefabs/LoadoutMissingType.prefab` — префаб, `Sidearms → Element 0` ссылается на потерянный `GhostPistol`.
- `Presets/MovedWeaponPreset.asset` — `ScriptableObject`, у которого `Weapon` хранит `Pistol` под старым namespace `…Samples.SerializeReferences.Legacy` — как будто класс перенесли без `[MovedFrom]`; именно он демонстрирует одно-кликовый **Smart Fix** ниже.
- `Presets/RenamedWeaponPreset.asset` — `ScriptableObject`, у которого `Weapon` хранит старое имя класса `CrossbowLauncher`; сам класс теперь называется `Crossbow` и несёт объявленный `[MovedFrom]`, поэтому инспектор показывает здоровое оружие и устарел только файл — именно он демонстрирует поток **Migrate all** в Project References.

Выделите любой из первых четырёх **в окне Project**. У потерянного поля будет подпись `<Missing …>`, предупреждение **Missing type** и кнопка **Fix**:

1. Нажмите **Fix** — откроется привычный селектор типов с поиском. Выберите `Pistol`.
2. Ссылка восстановится в `Pistol` с сохранёнными данными (префаб сохранит `_damage = 15`, `_magazineSize = 12`; ассет — `_damage = 25`, `_magazineSize = 8`). Выбор типа переписывает сохранённый тип в файле ассета, а не создаёт экземпляр заново — поэтому значения сохраняются.

Когда у сломанной идентичности есть правдоподобный преемник, предупреждение дополняется одно-кликовой подсказкой
**Smart Fix** — откройте `MovedWeaponPreset.asset`: его плашка заканчивается на **`→ Pistol?`** (наведите курсор,
чтобы увидеть полную идентичность и причину ранжирования). Клик перенаправляет ссылку без открытия селектора,
сохраняя `_damage = 21`, `_magazineSize = 6`.

- Ранжирование, от высшего балла: объявленное совпадение `[MovedFrom]`, одноимённый тип в другом namespace/сборке, переименование только по регистру, похожее имя, подкреплённое формой полей осиротевших данных.
- Никогда не применяется автоматически — клик всегда за вами.
- Перенос, сразу снабжённый `[MovedFrom]`, вообще не ломается (Unity мигрирует ссылку при загрузке); Smart Fix ловит переносы, где про атрибут забыли. У `GhostWeapon`/`GhostPistol`-ассетов преемника нет, поэтому подсказки у них не появляется — этот контраст намеренный.

> Починка читает и переписывает файл ассета напрямую — Unity не отдаёт потерянный тип через свой serialization API (а на GameObject/префабах ещё и обнуляет его в живом объекте, UUM-129100), поэтому осиротевшие тип и данные восстанавливаются прямо из YAML. Работает для ScriptableObject и префаб-ассетов, выделенных в Project (переписывается их YAML), для объектов в **Prefab Mode** (чинится на живом экземпляре) и для объектов в **сохранённой чистой сцене** (находятся через `GlobalObjectId`) — но не для **несохранённой/грязной сцены** или **переопределения на экземпляре префаба**, у которых нет зафиксированного файла ассета для маппинга ссылки.
>
> Если потерянная ссылка вложена в другое значение или лежит на дочернем объекте, до которого не добраться в инспекторе — используйте **`Tools → Aspid 🐍 → FastTools → Asset References`**: вкладка сканирует весь файл ассета и выводит все потерянные ссылки (любой глубины, на любом дочернем объекте), каждую со своим **Fix**.
>
> Вкладка **Project References** проходит по всем ассетам в `Assets/` и группирует потерянные ссылки по сохранённому типу — поэтому `BrokenWeaponPreset.asset` и `BrokenArsenalPreset.asset` сворачиваются в одну группу **GhostWeapon** (`4 entries · 2 files`). Одна кнопка **Fix all** выбирает единственную замену и перенаправляет все вхождения сразу в обоих файлах. А `RenamedWeaponPreset.asset` всплывает там как спокойная info-подсвеченная **ожидающая миграция**, а не предупреждение: его сохранённый `CrossbowLauncher` совпадает с `[MovedFrom]`, объявленным на `Crossbow`, поэтому карточка предлагает авторитетный **Migrate all (1) → Crossbow**, запекающий переименование в файл — после чего атрибут можно удалить из кода.

### Карта вложенного графа — `NestedLoadout.prefab`

`Prefabs/NestedLoadout.prefab` — трёхуровневая иерархия `NestedLoadout → WeaponSlot → BackupSlot`, на **каждом** объекте свой `Loadout`, так что на каждом дочернем объекте есть потерянная ссылка, до которой не добраться в инспекторе вне Prefab Mode:

- **NestedLoadout** (корень) — `Primary Weapon = Railgun` (с вложенным эффектом заряда `BurnEffect`), `Sidearms = [GhostPistol (потерян), <None> (пустой слот)]`, `On Hit Effect = FreezeEffect`.
- **WeaponSlot** (дочерний) — `Primary Weapon = GhostBlade` (потерян), `Sidearms[0] = Pistol`.
- **BackupSlot** (внук) — `On Hit Effect = GhostAura` (потерян), `Primary Weapon = Shotgun`.

Выделите его **в окне Project** и откройте вкладку **Asset References** — **`Tools → Aspid 🐍 → FastTools → Asset References`**. Граф строит сразу все три компонента (по документу на объект). Каждая ссылка — инлайн-дропдаун: выберите тип, чтобы присвоить / перенаправить её, или `<None>`, чтобы очистить; у потерянных `GhostPistol` / `GhostBlade` / `GhostAura` карточек — янтарное действие **Fix Missing**. Вложенность читается по пути поля (`_primaryWeapon._chargeEffect`), а не по отступу, поэтому плоский список карточек остаётся читаемым.

### Расцепление общих ссылок и различение групп по цвету — `LoadoutSharedRef.prefab`

В `Prefabs/LoadoutSharedRef.prefab` на одном объекте — **две независимые** пары общих ссылок (каждую пару можно также получить дублированием элемента массива), поэтому цветовая полоска/нотис по rid здесь действительно полезна:

- `Sidearms[0]` и `Sidearms[1]` ссылаются на один и тот же `Pistol` — один цвет.
- `Primary Weapon → Charge Effect` и `On Hit Effect` ссылаются на один и тот же `BurnEffect` — другой цвет, хотя одно поле лежит на три уровня вложенности глубже, а другое — на верхнем уровне.

1. Откройте его — каждая пара показывает пометку **shared reference**, и редактирование одного элемента меняет его партнёра. Совпадающий цвет полоски/нотиса означает один и тот же экземпляр, независимо от места поля в иерархии, поэтому две пары читаются как два разных цвета.
2. **ПКМ** по элементу → **Make Unique Reference**. Он получит собственную копию данных, и поля станут независимыми — его пометка исчезнет, как и у бывшего партнёра, ведь ничего больше не общее.

## Путь IMGUI

Drawer поддерживает и UIToolkit-, и IMGUI-рендеринг на полном паритете функций. **`Prefabs/IMGUILoadout.prefab`** несёт те же данные, что и `Loadout.prefab`, но принудительно идёт по IMGUI-пути — так можно сравнить оба рендерера бок о бок или скопировать паттерн в IMGUI-проект.

Секрет — в companion-editor: `IMGUILoadoutEditor` переопределяет `OnInspectorGUI` **без** `CreateInspectorGUI` — этого достаточно, чтобы каждое вложенное поле `[TypeSelector]` пошло через `SerializeReferenceIMGUIPropertyDrawer`, а не через UIToolkit-путь `CreatePropertyGUI`. Единственная IMGUI-особенность: Unity применяет drawer к каждому *элементу* списка, поэтому кнопка **+** `[SerializeReference]`-списка рисуется через `SerializeReferenceIMGUIList.Draw(listProperty, label, elementType)` — это сохраняет добавление через селектор без алиасинга. Образец — `Scripts/IMGUILoadout.cs` + `Scripts/Editor/IMGUILoadoutEditor.cs`.

Все сервисные окна (**Asset References**, **Project References**) не зависят от рендерера и работают одинаково для обоих путей.

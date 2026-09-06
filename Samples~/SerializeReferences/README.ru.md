# Пример SerializeReferences

Турель, стреляющая полиморфным оружием по тренировочному манекену. Каждое оружие, эффект и модификатор — поле `[SerializeReference]` с выпадающим списком `[TypeSelector]`, а в комплекте идут намеренно сломанные ассеты, на которых можно пройти инструменты ремонта. Справочник — [SerializeReference Selector](../../Documentation/ru/03-serialize-reference-selector.md) и [SerializeReference Tooling](../../Documentation/ru/04-serialize-reference-tooling.md).

```csharp
[TypeSelector]
[SerializeReference] private IWeapon _primary;
```

## Как открыть

1. Импортируйте пример и откройте `Scenes/SerializeReferences.unity`.
2. Выберите **Loadout**. Войдите в Play Mode: основное оружие и запасное по очереди бьют манекен раз в секунду; он уменьшается, окрашивается при горении и заморозке и сбрасывается после уничтожения. Каждый удар виден в Console.

## Попробуйте: настройка

1. **Выбор реализации.** Откройте список `Primary`: окно с поиском перечисляет все конкретные `IWeapon`, сгруппированные в **Weapons/Melee** и **Weapons/Ranged** через `[TypeSelectorDisplay]`. `DebugWeapon` там нет — он `Hidden` и предназначен только для кода. Выберите `Shotgun`; его поля появятся прямо под списком.
2. **Смена типа сохраняет общие данные.** Поставьте в `Sidearms` `Pistol`, измените `Damage`, переключите на `Shotgun` и обратно: значение осталось, потому что оба типа объявляют `_damage`.
3. **Списки.** Нажмите **+** на `Sidearms`: вместо дублирования последнего элемента открывается пикер, так что два элемента никогда не разделяют один экземпляр.
4. **Сужение.** `Melee Backup` объявлено как `IWeapon`, но несёт `[TypeSelector(typeof(IMelee))]`, поэтому предлагается только `Sword`. `Holster` делает то же самое внутри обычного `[Serializable]`-контейнера, на уровень глубже.
5. **Вложенность.** Разверните `Railgun` в `Primary`: его `Charge Effect` — собственный `[SerializeReference]` со своим списком. При попадании рейлгана манекен загорается.
6. **Абстрактная база.** `On Hit` — это `StatusEffect`; пикер предлагает `BurnEffect` и `FreezeEffect`, но никогда — абстрактную базу.
7. **Generics.** `Damage Modifier` — `Modifier<float>`: `T` зафиксирован, поэтому предлагаются `DamageModifier` и `Modifier<float>`, и создаются сразу. `Perks` — `List<IModifier>`: предлагает закрытые подклассы **и** открытый `Modifier<T>`, который запрашивает `T` на второй странице.
8. **Required.** Поставьте `Primary` в `<None>`: появится предупреждение, а поле станет нарушением для build/CI-гейта.
9. **Правый клик по любому списку** — Copy / Paste, Make Unique Reference, Save as Template, Find Usages и Create New Script.

## Попробуйте: ремонт

В папках `Presets/` и `Prefabs/` лежат ассеты, чьи сохранённые типы устарели или исчезли:

| Ассет | Что не так | Что делать |
|---|---|---|
| `Presets/BrokenWeaponPreset.asset` | `Weapon` хранит несуществующий `GhostWeapon` | Выберите его. Поле показывает `<Missing GhostWeapon>` и кнопку **Fix**; выберите `Pistol`. Урон и размер магазина сохранятся. |
| `Presets/BrokenArsenalPreset.asset` | Тот же `GhostWeapon`, трижды | Откройте **Tools → Aspid 🐍 → FastTools → Project References**, **Scan Project**: оба пресета схлопнутся в одну группу `GhostWeapon`. **Fix all** перенаправит все записи разом. |
| `Presets/MovedWeaponPreset.asset` | `Pistol` под старым namespace | Предупреждение заканчивается подсказкой **Smart Fix** в один клик (`→ Pistol?`). Smart Fix ранжирует совпадение по `[MovedFrom]`, одноимённый тип, смену регистра и близкое имя и никогда не применяется сам. |
| `Presets/RenamedWeaponPreset.asset` | Хранит `CrossbowLauncher`; класс теперь `Crossbow` с `[MovedFrom]` | Инспектор уже показывает здоровый `Crossbow`, устарел только файл. В **Project References** группа отображается как ожидающая миграция с кнопкой **Migrate all**, которая записывает переименование в файл. |
| `Prefabs/BrokenLoadout.prefab` | `Sidearms[2]` — отсутствующий `GhostCrossbow`; `Sidearms[0]` и `[1]` делят один `Pistol` | Выберите его в Project. Отсутствующий элемент предлагает **Fix**; общая пара помечена цветным уведомлением, а **Make Unique Reference** её разделяет. **Asset References** показывает весь граф префаба на одном экране. |

Ремонт переписывает файл ассета, поэтому ему нужен сохранённый ассет: ScriptableObject или префаб, выбранный в Project, Prefab Mode либо чистая сохранённая сцена.

## Путь IMGUI

У `WeaponPreset` инспектор на IMGUI (`Scripts/Editor/WeaponPresetEditor.cs`). Одного переопределения `OnInspectorGUI` достаточно, чтобы все вложенные drawer'ы пошли через IMGUI с полным паритетом возможностей. Единственное отличие — список: Unity применяет drawer к каждому элементу, поэтому его **+** клонировал бы последний элемент; `SerializeReferenceIMGUIList.Draw` возвращает добавление через пикер. Для кастомного редактора, который рисует managed reference без `[TypeSelector]` на поле, те же контролы строят `SerializeReferenceEditorGUI.CreateField` / `CreateList` / `DrawFieldLayout`.

## Куда смотреть

| Файл | Что показывает |
|---|---|
| `Scripts/Loadout.cs` | Все формы поля: одиночное, список, суженное, контейнер, абстрактная база, закрытый и открытый generic, `Required` |
| `Scripts/Weapons/` | Иерархия `IWeapon`, группы `[TypeSelectorDisplay]`, `Hidden`-тип, `[MovedFrom]` на `Crossbow`, вложенная ссылка в `Railgun` |
| `Scripts/Effects/`, `Scripts/Modifiers/` | Абстрактная база и конкретный открытый generic |
| `Scripts/TrainingDummy.cs` | Цель, на которую действуют оружие и эффекты |
| `Scripts/WeaponPreset.cs` + `Presets/` | Сценарии ремонта |
| `Scripts/Editor/WeaponPresetEditor.cs` | IMGUI-инспектор с `SerializeReferenceIMGUIList` |

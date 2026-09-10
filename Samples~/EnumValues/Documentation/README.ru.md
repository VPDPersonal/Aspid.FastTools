# Пример EnumValues

Персонаж ходит по ряду плиток. Цвет плитки, цвет следа, интервал обновления цвета и скорость — всё это `EnumValues`-поиски по enum поверхности или `[Flags]`-enum рельефа, настроенные в инспекторе со значением по умолчанию. Справочник — [EnumValues](../../../Documentation/ru/06-enum-values.md).

Например, в `SurfacePalette.cs` объявлены две таблицы цветов с типом `SurfaceType`, заданным в коде:

```csharp
[SerializeField] private EnumValues<SurfaceType, Color> _tileColors;
[SerializeField] private EnumValues<SurfaceType, Color> _footprintColors;
```

![Цвета поверхностей и следов; у каждой таблицы есть Default Value.](Images/surface-tables.png)

Те же поля в инспекторе `Data/SurfacePalette.asset`: `_tileColors` отображается как **Tile Colors**, а `_footprintColors` — как **Footprint Colors**. У каждой таблицы своё значение **Default Value**.

## Как открыть

1. Откройте **Welcome Window** через меню `Tools → Aspid 🐍 → FastTools → Welcome` и нажмите **Import** у примера **EnumValues**. Если вместо **Import** отображается **Remove**, пример уже импортирован — переходите к следующему шагу.
2. В папке импортированного примера откройте `Scenes/EnumValues.unity`.
3. Войдите в Play Mode: **Walker** проходит семь плиток, оставляя непрерывную цветную линию. Её старый конец сокращается через две секунды. На горячем металле персонаж движется быстрее, на мокрой траве — медленнее. Мягкий песок даёт множитель скорости `0.4`: ниже, чем у сухого камня (`1`) и мокрого скользкого камня (`0.5`).

![Персонаж проходит по разным поверхностям и оставляет непрерывную цветную линию.](Images/demo.gif)

Персонаж проходит по разным поверхностям и оставляет непрерывную цветную линию.

## Попробуйте

### 1. Типизированный вариант

Выберите `Data/SurfacePalette.asset`. Обе таблицы — `EnumValues<SurfaceType, Color>`: строка с типом enum только для чтения, потому что тип задан полем. Поменяйте цвет `Grass` — плитки перекрасятся сразу, без Play Mode.

Так `SurfacePalette.cs` читает цвет плитки из таблицы, показанной выше:

```csharp
[SerializeField] private EnumValues<SurfaceType, Color> _tileColors;

public Color GetTileColor(SurfaceType surface) =>
    _tileColors.GetValue(surface);
```

Аргумент `surface` определяет строку таблицы, а `GetValue` возвращает её цвет.

### 2. Значение по умолчанию

В `Footprint Colors` у каждой поверхности свой цвет следа: зелёный у травы, фиолетовый у камня, красный у металла, голубой у воды и оранжевый у песка. Чтобы проверить fallback, удалите строку `Stone`: камень начнёт использовать `Default Value`. Правый клик по полю → **Populate Missing Enum Members** восстановит строку с этим значением; через Undo верните исходную палитру.

Внутри `SurfacePalette` вызов остаётся тем же, даже если строка удалена:

```csharp
var color = _footprintColors.GetValue(SurfaceType.Stone);
```

При наличии строки `Stone` вы получите её цвет, после удаления — **Default Value**. Отдельная проверка наличия ключа не нужна.

### 3. Выбор enum в инспекторе

Выберите объект **Walker** в Hierarchy и найдите таблицу **Speed By Terrain** в инспекторе. В коде она объявлена как `EnumValues<float>`: задан только тип значений — `float`, а тип enum выбирается в инспекторе над списком строк. В этом примере уже выбран `TerrainFlags`. Каждая строка связывает флаг или сочетание флагов с множителем скорости. Например, `Wet, Slippery` означает, что в одной строке выбраны оба флага — `Wet` и `Slippery`; для этого сочетания задан множитель `0.5`.

Поле из `Walker.cs` и пример чтения множителя внутри этого компонента:

```csharp
[SerializeField] private EnumValues<float> _speedByTerrain;

private float GetSpeedMultiplier(TerrainFlags terrain) =>
    _speedByTerrain.GetValue(terrain);
```

`EnumValues` находится в пространстве имён `Aspid.FastTools.Enums`. Значения таблицы настраиваются в инспекторе.

### 4. Правила поиска для `[Flags]`

`GetValue` выбирает значение в таком порядке:

- сначала выигрывает **точный** ключ: плитка `Water (Wet, Slippery)` даёт строку `Wet, Slippery` (`0.5`), хотя строки `Wet` и `Slippery` тоже есть;
- иначе выигрывает **первая строка, все флаги которой содержатся** в значении: для `Wet | Hot` отдельной строки нет, поэтому результат — `0.8` из строки `Wet`, которая стоит раньше `Hot`;
- ничего не совпало, включая `None` — **Default Value** (`1`).

Эти вызовы внутри `Walker` показывают все три случая при исходных настройках **Speed By Terrain**:

```csharp
// Точная строка Wet, Slippery → 0.5
var exact = _speedByTerrain.GetValue(
    TerrainFlags.Wet | TerrainFlags.Slippery);

// Строки Wet, Hot нет; первая подходящая — Wet → 0.8
var partial = _speedByTerrain.GetValue(
    TerrainFlags.Wet | TerrainFlags.Hot);

// Подходящей строки нет → Default Value (1)
var fallback = _speedByTerrain.GetValue(TerrainFlags.None);
```

Оператор `|` объединяет флаги в одно значение. Множители не складываются и не перемножаются: поиск выбирает одну строку.

### 5. Перебор

Правый клик **Walker → Log Tables**. `foreach` выдаёт настроенные строки в порядке списка; значение по умолчанию в перебор не входит.

Фрагмент метода `LogTables` в `Walker.cs`:

```csharp
foreach (var (flags, multiplier) in _speedByTerrain)
{
    Debug.Log($"Speed x{multiplier:0.00} on [{flags}]", this);
}
```

### 6. Измените enum

Добавьте член в `SurfaceType` в коде: ничего не ломается, новая поверхность просто берёт значение по умолчанию, пока вы не добавите строку. Ключи хранятся по имени, так что и переставлять члены безопасно.

## Куда смотреть

| Файл | Что показывает |
|---|---|
| `Scripts/SurfacePalette.cs` | `EnumValues<TEnum, TValue>` на ScriptableObject |
| `Scripts/Walker.cs` | Оба варианта в компоненте, `GetValue` по обычному и `[Flags]`-ключу, `foreach` |
| `Scripts/SurfaceTrail.cs` | Рисует непрерывную линию и убирает старые точки |
| `Scripts/SurfaceTile.cs` | Следит за цветом из палитры через editor update; применяет изменения и Undo без Play Mode |
| `Scripts/TerrainFlags.cs` | `[Flags]`-enum с комбинируемыми членами |

# Пример EnumValues

Персонаж идёт по плиткам, а цвет, след и скорость на каждой поверхности берутся из таблиц в инспекторе.

![Персонаж проходит по разным поверхностям и оставляет непрерывную цветную линию.](../../../../../tutorials/EnumValues/Images/demo.gif)

Персонаж проходит по разным поверхностям и оставляет непрерывную цветную линию.

## Как открыть

1. Импортируйте пример: **Tools → Aspid 🐍 → FastTools → Welcome** → **Samples** → **Import** у **EnumValues**.
2. Откройте `Scenes/EnumValues.unity` и войдите в Play Mode: на металле персонаж ускоряется, на мокрых и мягких плитках замедляется, а след окрашивается в цвет поверхности.

## Попробуйте

### 1. Enum задан в коде

Выберите `Data/SurfacePalette.asset` и поменяйте цвет `Grass` в **Tile Colors** — плитки перекрасятся сразу.

![Tile Colors и Footprint Colors, у каждой таблицы своё Default Value.](../../../../../tutorials/EnumValues/Images/surface-tables.png)

Tile Colors и Footprint Colors, у каждой таблицы своё Default Value.

```csharp
[SerializeField] private EnumValues<SurfaceType, Color> _tileColors;

public Color GetTileColor(SurfaceType surface) =>
    _tileColors.GetValue(surface);
```

### 2. Значение по умолчанию

Удалите строку `Stone` из **Footprint Colors** и войдите в Play Mode: след на камне примет **Default Value** таблицы.

### 3. Enum выбран в инспекторе

Выберите **Walker**: для **Speed By Terrain** код задаёт только тип значения, а enum <code lang="class-name">TerrainFlags</code> выбран в шапке таблицы.

```csharp
[SerializeField] private EnumValues<float> _speedByTerrain;

var speed = _speed * (_tile == null
    ? 1f
    : _speedByTerrain.GetValue(_tile.Flags));
```

### 4. Ключи `[Flags]`

<code lang="function">GetValue</code> возвращает значение одной строки:

| Ключ | Строка | Множитель |
|---|---|---|
| `Wet, Slippery` | точная строка | `0.5` |
| `Wet, Hot` | первая строка внутри ключа — `Wet` | `0.8` |
| `None` | нет, **Default Value** | `1` |

### 5. Новый член enum

Добавьте `Ice` в <code lang="class-name">SurfaceType</code> и поставьте одной плитке **Surface** = `Ice`: плитка и её след возьмут значения по умолчанию.

## Куда смотреть

| Файл | Что показывает |
|---|---|
| `Scripts/SurfacePalette.cs` | <code lang="class-name">EnumValues&lt;TEnum, TValue&gt;</code> на ScriptableObject |
| `Scripts/Walker.cs` | <code lang="class-name">EnumValues&lt;TValue&gt;</code> с <code lang="csharp">[Flags]</code>-ключом |

Справочник — [EnumValues](../../../docusaurus-plugin-content-docs/current/08-enum-values.md).

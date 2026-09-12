# EnumValues Sample

A character pacing over a row of floor tiles. The tile color, the trail color, the color sampling interval and the walking speed are all `EnumValues` lookups keyed by a surface enum or a `[Flags]` terrain enum, configured in the Inspector with a default fallback. The API reference lives in [EnumValues](../../../Documentation/06-enum-values.md).

For example, `SurfacePalette.cs` declares two color tables with `SurfaceType` fixed in code:

```csharp
[SerializeField] private EnumValues<SurfaceType, Color> _tileColors;
[SerializeField] private EnumValues<SurfaceType, Color> _footprintColors;
```

![Surface colors and footprint colors, each with a Default Value.](Images/surface-tables.png)

The same fields in the Inspector for `Data/SurfacePalette.asset`: `_tileColors` appears as **Tile Colors**, and `_footprintColors` as **Footprint Colors**. Each table has its own **Default Value**.

## Open it

1. Open the **Welcome Window** from `Tools → Aspid 🐍 → FastTools → Welcome` and click **Import** for the **EnumValues** sample. If the button shows **Remove** instead of **Import**, the sample is already imported — continue to the next step.
2. Open `Scenes/EnumValues.unity` inside the imported sample folder.
3. Enter Play Mode: the **Walker** crosses seven tiles, leaving a continuous colored trail whose older end recedes after two seconds. Movement is faster on hot metal and slower on wet grass. Soft sand uses a `0.4` speed multiplier, below both dry stone (`1`) and wet, slippery stone (`0.5`).

![The walker crosses different surfaces and leaves a continuous colored trail.](Images/demo.gif)

The walker crosses different surfaces and leaves a continuous colored trail.

## Try

### 1. Typed variant

Select `Data/SurfacePalette.asset`. Both tables are `EnumValues<SurfaceType, Color>`: the enum row is read-only because the type is fixed by the field. Change the `Grass` color; the tiles recolor immediately, without Play Mode.

This is how `SurfacePalette.cs` reads a tile color from the table shown above:

```csharp
[SerializeField] private EnumValues<SurfaceType, Color> _tileColors;

public Color GetTileColor(SurfaceType surface) =>
    _tileColors.GetValue(surface);
```

The `surface` argument identifies the table row; `GetValue` returns its color.

### 2. Default value

`Footprint Colors` assigns a distinct trail color to every surface: green for grass, purple for stone, red for metal, cyan for water and orange for sand. To try the fallback, remove the `Stone` row: stone then uses `Default Value`. Right-click the field and choose **Populate Missing Enum Members** to restore the missing row with that default; use Undo to return to the original palette.

Inside `SurfacePalette`, the call stays the same even after removing a row:

```csharp
var color = _footprintColors.GetValue(SurfaceType.Stone);
```

With the `Stone` row present, this returns its color; after removing it, the result is **Default Value**. No separate key-existence check is needed.

### 3. Choose the enum in the Inspector

Select the **Walker** object in the Hierarchy and find the **Speed By Terrain** table in the Inspector. Its declaration, `EnumValues<float>`, specifies only the value type, `float`; the enum type is chosen in the Inspector above the list of rows. This sample already has `TerrainFlags` selected. Each row maps a flag or a combination of flags to a speed multiplier. For example, `Wet, Slippery` means both flags, `Wet` and `Slippery`, are selected in a single row; that combination has a multiplier of `0.5`.

The field from `Walker.cs` and an example of reading a multiplier inside that component:

```csharp
[SerializeField] private EnumValues<float> _speedByTerrain;

private float GetSpeedMultiplier(TerrainFlags terrain) =>
    _speedByTerrain.GetValue(terrain);
```

`EnumValues` belongs to the `Aspid.FastTools.Enums` namespace. Configure the table values in the Inspector.

### 4. `[Flags]` lookup rules

`GetValue` selects a value in this order:

- an **exact** key wins first: the `Water (Wet, Slippery)` tile resolves to the `Wet, Slippery` row (`0.5`), even though `Wet` and `Slippery` rows exist;
- otherwise the **first row whose flags are all contained** in the value wins: `Wet | Hot` has no row of its own, so it resolves to `0.8` from the `Wet` row, which appears before `Hot`;
- nothing matches, `None` included: **Default Value** (`1`).

These calls inside `Walker` demonstrate all three cases with the original **Speed By Terrain** settings:

```csharp
// Exact Wet, Slippery row → 0.5
var exact = _speedByTerrain.GetValue(
    TerrainFlags.Wet | TerrainFlags.Slippery);

// No Wet, Hot row; the first matching row is Wet → 0.8
var partial = _speedByTerrain.GetValue(
    TerrainFlags.Wet | TerrainFlags.Hot);

// No matching row → Default Value (1)
var fallback = _speedByTerrain.GetValue(TerrainFlags.None);
```

The `|` operator combines flags into one value. Multipliers are neither added nor multiplied together: the lookup selects one row.

### 5. Iterate

Right-click **Walker → Log Tables**. `foreach` yields the configured rows in list order; the default value is not part of the iteration.

An excerpt from `LogTables` in `Walker.cs`:

```csharp
foreach (var (flags, multiplier) in _speedByTerrain)
{
    Debug.Log($"Speed x{multiplier:0.00} on [{flags}]", this);
}
```

### 6. Change the enum

Add a member to `SurfaceType` in code: nothing breaks, the new surface simply falls back to the default until you add a row. Keys are stored by name, so reordering members is safe too.

## Where to look

| File | Shows |
|---|---|
| `Scripts/SurfacePalette.cs` | `EnumValues<TEnum, TValue>` on a ScriptableObject |
| `Scripts/Walker.cs` | Both variants in a component, `GetValue` on a plain and a `[Flags]` key, `foreach` |
| `Scripts/SurfaceTrail.cs` | Draws a continuous ribbon and trims points by age |
| `Scripts/SurfaceTile.cs` | Checks the palette color on editor updates, applying edits and Undo without Play Mode |
| `Scripts/TerrainFlags.cs` | The `[Flags]` enum with combinable members |

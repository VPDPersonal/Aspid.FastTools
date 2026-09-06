# EnumValues Sample

A character pacing over a row of floor tiles. The tile color, the footprint color, the step cadence and the walking speed are all `EnumValues` lookups keyed by a surface enum or a `[Flags]` terrain enum, configured in the Inspector with a default fallback. The API reference lives in [EnumValues](../../Documentation/06-enum-values.md).

```csharp
[SerializeField] private EnumValues<SurfaceType, float> _stepInterval; // enum fixed in code
[SerializeField] private EnumValues<float> _speedByTerrain;            // enum picked in the Inspector
```

## Open it

1. Import the sample and open `Scenes/EnumValues.unity`.
2. Enter Play Mode: the **Walker** crosses seven tiles, leaving colored footprints that fade, faster on hot metal and slower on wet grass.

## Try

1. **Typed variant.** Select `Data/SurfacePalette.asset`. Both tables are `EnumValues<SurfaceType, Color>`: the enum row is read-only because the type is fixed by the field. Change the `Grass` color; the tiles recolor immediately, without Play Mode.
2. **Default value.** `Footprint Colors` has rows only for `Grass`, `Sand` and `Water`. Stone and metal footprints use `Default Value`. Right-click the field and choose **Populate Missing Enum Members** to add the rest, seeded with that default.
3. **Untyped variant.** Select **Walker**. `Speed By Terrain` is an `EnumValues<float>` whose enum, `TerrainFlags`, was picked in the header row. Each key is a flags dropdown, and `Wet, Slippery` is a row of its own.
4. **`[Flags]` lookup rules**, visible as the walker crosses the tiles:
   - an **exact** key wins first: the `Water (Wet, Slippery)` tile resolves to the `Wet, Slippery` row (`0.5`), even though `Wet` and `Slippery` rows exist;
   - otherwise the **first row whose flags are all contained** in the value wins: a `Wet`-only tile gets `0.8`;
   - nothing matches, `None` included: **Default Value** (`1`).
5. **Iterate.** Right-click **Walker → Log Tables**. `foreach` yields the configured rows in list order; the default value is not part of the iteration.
6. **Change the enum.** Add a member to `SurfaceType` in code: nothing breaks, the new surface simply falls back to the default until you add a row. Keys are stored by name, so reordering members is safe too.

## Where to look

| File | Shows |
|---|---|
| `Scripts/SurfacePalette.cs` | `EnumValues<TEnum, TValue>` on a ScriptableObject |
| `Scripts/Walker.cs` | Both variants in a component, `GetValue` on a plain and a `[Flags]` key, `foreach` |
| `Scripts/SurfaceTile.cs` | Reads the palette in the editor (`[ExecuteAlways]`) so palette edits show without Play Mode |
| `Scripts/TerrainFlags.cs` | The `[Flags]` enum with combinable members |

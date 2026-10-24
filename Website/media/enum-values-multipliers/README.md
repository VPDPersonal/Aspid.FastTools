# EnumValues media

Recipes for the EnumValues page captures in `Website/docs/Images/`:

- `enum-values-multipliers-quick-start.png`: this folder, below.
- `enum-values-multipliers-populate.gif`: [populate](populate/README.md), then [`padding/pad.py`](padding/pad.py).
- `enum-values-type-selector.png`: [type-selector](type-selector/README.md).

`subject.cs.txt` is the temporary `DocumentationMedia.EnumMedia` subject that `populate` and `type-selector` are shot on: an in-memory `EnumValues<DamageType, float>` table (Default Value 1, one Fire row at 1.5) in a utility EditorWindow; nothing is saved. Run it with `unity command script run` from a `.cs` copy (`script run` takes only `.cs`): `DocumentationMedia.EnumMedia.Main` opens it, `Untyped` swaps in an `EnumValues<float>` table, `Cleanup` closes it.

# Quick-start capture — 2026-09-20

Real Aspid.FastTools UI Toolkit PropertyField in a temporary Unity 6000.4 EditorWindow. The temporary ScriptableObject has EnumValues<DamageType, float>: Physical, Fire, Ice, Poison. Default Value 1; one initial Fire row with value 1.5.

capture.cs.txt creates its own in-memory subject, separate from `subject.cs.txt`; no project asset or scene is saved. Render scale 2.5, property width 464, window 1200 × 700. Source window screenshots are 1200 × 728 JPEGs returned by computer-use; the final PNG is a lossless encoding of the cropped source, not a claim of lossless capture. Capture at enlarged UI scale preserves legibility at article width. Cursor stays in title bar, outside crop.

encode.sh crops x=10 y=30 width=1190, height=390 into `multipliers-quick-start.png` here; copy it over `Website/docs/Images/enum-values-multipliers-quick-start.png` and run `scripts/frame-doc-captures.sh`. No recolouring, compositing, resizing or field edits.

This recipe makes the dark capture only. The site's light theme shows `enum-values-multipliers-quick-start-light.png`, a separate capture in Unity's light Editor skin (the docs-site skill requires a light twin for every image); no recipe for it is kept here. `scripts/frame-doc-captures.sh` frames both files. Temporary windows and objects removed; the active scene stayed clean, outside Play Mode.

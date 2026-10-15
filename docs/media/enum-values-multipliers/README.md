# Current media

The populate GIF is superseded by [revision 3](v3/README.md), and its frames are removed. The quick-start PNG below is still current; `encode.sh` rebuilds it from `dark-01.jpg`.

# EnumValues quick-start captures — 2026-09-20

Real Aspid.FastTools UI Toolkit PropertyField in a temporary Unity 6000.4 EditorWindow. The temporary ScriptableObject has EnumValues<DamageType, float>: Physical, Fire, Ice, Poison. Default Value 1; one initial Fire row with value 1.5.

Capture.cs creates the in-memory subject; no project asset or scene is saved. Render scale 2.5, property width 464, window 1200 × 700. Source window screenshots are 1200 × 728 JPEGs returned by computer-use; the final PNG is a lossless encoding of the cropped source, not a claim of lossless capture. Capture at enlarged UI scale preserves legibility at article width. Cursor stays in title bar, outside crop.

The superseded populate GIF was made as follows. The native menu was observed through accessibility, but computer-use could not capture it or reliably activate its item. The actual package EnumValuesPropertyDrawerHelper.PopulateMissing handler was invoked via Unity CLI, followed by actual Undo.PerformUndo. Serialized data verified Fire=1.5, appended Physical/Ice/Poison=1, then one Fire row after Undo. The GIF showed genuine before / populated / Undo states, not a recording of menu navigation; its frames are removed.

encode.sh crops x=10 y=30 width=1190, height=390. No recolouring, compositing, resizing or field edits.

This recipe makes the dark capture only. The site's light theme shows `enum-values-multipliers-quick-start-light.png`, a separate capture in Unity's light Editor skin (the docs-site skill requires a light twin for every image); no recipe for it is kept here. `scripts/frame-doc-captures.sh` frames both files. Temporary windows and objects removed; the active scene stayed clean, outside Play Mode.

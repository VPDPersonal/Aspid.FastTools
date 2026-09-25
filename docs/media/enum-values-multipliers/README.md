# Current media

The populate GIF is superseded by [revision 2](v2/README.md), which includes a visible UI Toolkit menu. The quick-start PNG below is still current.

# EnumValues quick-start captures — 2026-09-20

Real Aspid.FastTools UI Toolkit PropertyField in a temporary Unity 6000.4 EditorWindow. The temporary ScriptableObject has EnumValues<DamageType, float>: Physical, Fire, Ice, Poison. Default Value 1; one initial Fire row with value 1.5.

Capture.cs creates the in-memory subject; no project asset or scene is saved. Render scale 2.5, property width 464, window 1200 × 700. Source window screenshots are 1200 × 728 JPEGs returned by computer-use; the final PNG is a lossless encoding of the cropped source, not a claim of lossless capture. Capture at enlarged UI scale preserves legibility at article width. Cursor stays in title bar, outside crop.

Native menu was observed through accessibility, but computer-use could not capture it or reliably activate its item. The actual package EnumValuesPropertyDrawerHelper.PopulateMissing handler was invoked via Unity CLI, followed by actual Undo.PerformUndo. Serialized data verified Fire=1.5, appended Physical/Ice/Poison=1, then one Fire row after Undo. GIF shows genuine before / populated / Undo states, not a recording of menu navigation.

encode.sh crops x=10 y=30 width=1190; static height=390, animation height=554 (JPEG chroma alignment rounds the requested 555 down). No recolouring, compositing, resizing or field edits. GIF duration is 9 seconds: initial 2 seconds, populated 3 seconds, then Undo through the loop seam, 10 fps, 256-color palette, sierra2_4a. All frames are cropped from originals.

Light Editor skin was checked: the current package uses a dark stylesheet even with light Editor text, making labels unreadable. Keep the original dark capture for both website themes; do not fabricate a light sibling. Original dark Unity skin was restored; temporary windows and objects removed. Active scene remained SerializeReferences, clean, outside Play Mode; selection originally empty.

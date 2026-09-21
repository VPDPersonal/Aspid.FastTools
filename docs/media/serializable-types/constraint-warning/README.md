# TypeSelector constraint warning

Captured from the ordinary Unity 6000.4.0f1 Inspector using the current Editor theme. One screenshot is used in both website themes, as requested. No custom window styling or imitation Inspector controls.

Import `TypeSelectionExample.cs.txt` temporarily as `Assets/__DocsConstraintCapture/TypeSelectionExample.cs`. The deliberate `_categroy` typo makes the package drawer show its real warning. Compile the project, save the previous selection in `Docs.NativeConstraint.PreviousSelection`, then run `capture.cs.txt` through Unity Pipeline `run_script`, entry `NativeConstraintInspector.Main`. It creates a temporary preview scene and a separate native Inspector window; the user's scene and layout are preserved.

Capture the Inspector window through computer use. The original `source-inspector.jpg` is 640 × 628. The final PNG crops one contiguous area with `ffmpeg -i source-inspector.jpg -vf "crop=640:110:0:110,pad=640:114:0:4:color=0x383838" -frames:v 1 type-selector-constraint-warning.png`. This keeps only the component header, Script reference, selector fields, and warning, matching the requested crop. The GameObject header, Transform, and Add Component button are excluded. A 4 px strip of matching Inspector background adds the requested breathing room above the header without bringing the Transform row back into view. No resizing or alteration of displayed values.

After capture, close the window and preview scene identified by `Docs.NativeConstraint.Window` and `Docs.NativeConstraint.Scene`, restore the previous selection, and delete the temporary script folder through AssetDatabase. Recompile to verify cleanup.

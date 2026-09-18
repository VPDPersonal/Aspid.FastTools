# Selector Inspector recording

Recorded on 2026-09-13 in Unity 6000.4.0f1 using Unity CLI and computer use.

The native UnityEditor.InspectorWindow displays the real sample Loadout component on a temporary GameObject in a preview scene, at 1.5× UI rendering scale. Pistol begins with Damage 37. The actual SerializeReferenceField dropdown handler creates the package picker, retaining the original selection callback. For capture, the same picker view is hosted over the Inspector root so the native window capture includes both; it is positioned below Primary with a 402×150 logical-pixel viewport. No screenshots are composited and no fields or results are painted or substituted. Select Shotgun and confirm: the native Inspector updates to Pellets 8 while preserving Damage 37.

`capture.cs.txt` creates the temporary scene and native Inspector; `picker.cs.txt` opens and positions its picker. Instance IDs are session-specific. Capture the initial state with the picker hidden, show it, select Shotgun, and confirm. Move the cursor to the title bar before every capture. Close the temporary Inspector and preview scene afterward, and restore the original selection. The active Types scene remained clean and the original selection was restored.

Original captures are 840×668 JPEG files. Run `sh docs/media/selector/encode.sh` from the repository root. The crop `820:570:0:28` excludes native title chrome, the right scrollbar, and the unrelated lower section without cutting any visible field in the recording. No padding, decorative border, image scaling, or compositing. `frames.ffconcat` holds the four states for 2 / 1.5 / 1 / 3 seconds. Encoding uses 10 fps, 256 palette colors and sierra2_4a dithering.

The superseded custom-window captures and their recipe are preserved in `archive-custom-window/` for provenance; the published GIF uses only the native Inspector captures above. Existing GIF Unity GUID is retained.

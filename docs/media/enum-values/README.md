# EnumValues native Inspector recording

Recorded on 2026-09-13 in Unity 6000.4.0f1. `capture.cs.txt` clones the imported SurfacePalette in memory, removes Stone and Sand from Tile Colors, and displays the actual property drawer in a temporary 640×600 Inspector. Footprint Colors stays unchanged. No UI scale or style override.

The real Tile Colors context menu command **Populate Missing Enum Members** was confirmed with Down and Return. SerializedObject verified five rows, appended Stone and Sand, and Stone's color equal to Default Value (0.5,0.5,0.5,1). Cmd+Z restored the three original rows. The final loop shows the clear before/after table states; the native macOS context menu was not visible in the screen capture and is not recreated or composited.

Lossless Retina originals: `/usr/sbin/screencapture -x -R124,100,632,564 <file.png>`, 1264×1128. Move the computer-use cursor onto the native Inspector title before capture. The region excludes title chrome and rounded corners while retaining all table content. No resizing, padding or pixel edits.

Run `sh docs/media/enum-values/encode.sh`. Three-second loop: missing rows for 1.2 seconds, populated rows for 1.8 seconds, then the same starting frame at the loop seam. 10 fps, shared 256-color palette, sierra2_4a dithering. `01-missing.png` is retained as provenance; the post-Undo frame is used as the opening state to preserve focus colors.

The temporary Inspector and cloned object were destroyed and selection restored. The existing dirty user scene was left unsaved; the source palette asset was not changed. Instance IDs are session-specific.

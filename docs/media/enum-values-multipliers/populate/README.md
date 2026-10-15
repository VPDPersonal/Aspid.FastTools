# Populate GIF: native macOS recording

Real macOS context menu opened by right-clicking the genuine EnumValues PropertyField. Computer-use selected Populate Missing Enum Members and sent Cmd+Z. macOS screencapture recorded the screen region and actual cursor, including the native menu. No UI Toolkit replacement menu, painted menu or composited cursor.

The in-memory DocumentationMedia.EnumMedia subject from [`../subject.cs.txt`](../subject.cs.txt) is shown with field width 475 and render scale 1.6, an 800×480 utility window. No scene or project assets were saved. The temporary window and object were removed after recording.

Source command: screencapture -x -v -C -k -V 35 -R190,104,780,352 native-menu.mov. Retina output is 1560×704. The crop excludes title bar and every other application. The original screen-region movie is in the private [Aspid.Media.Storage](https://github.com/VPDPersonal/Aspid.Media.Storage/tree/main/FastTools/Docs/enum-values-multipliers/v3) repository under `FastTools/Docs/enum-values-multipliers/v3/`; copy it next to `encode.sh` before you run it. Git ignores it here.

encode.sh retains source intervals 4.5–6.4, 11.4–15, and 24.7–27.5 seconds, removing idle time while retaining opening, selecting and Undo transitions. 8.3-second GIF, 20 fps, 256 colors, sierra2_4a. No spatial rescaling, recoloring or added cursor. Native macOS menu and cursor are intentionally included at the user's request.

This recipe makes the dark GIF only; `../padding/source.gif` is its output. The light GIF starts from `../padding/source-light.gif`, a separate capture in Unity's light Editor skin whose native menu is recoloured from the dark recording, because a native menu cannot be grabbed in the background; only that GIF is kept, not its recipe. `padding/pad.py` adds the bottom strip to both, writes `enum-values-multipliers-populate.gif` and its `-light` twin to `Website/docs/Images/` and runs `scripts/frame-doc-captures.sh`.

Timing refinement: the pause between opening the native menu and choosing its command is approximately 0.85 seconds, shortened from about 3 seconds. Pointer motion uses the recorded frames without interpolation.

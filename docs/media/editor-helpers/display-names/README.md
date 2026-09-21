# Display-name documentation capture

Captured in Unity 6000.4.0f1 from a temporary UI Toolkit EditorWindow. The three results come from actual `GetDisplayName()` and `GetDisplayNameWithIndex()` calls on two temporary `AbilityConfig` components. The preview scene is closed immediately after the calls, without changing the active scene. The window is closed after both captures. No assets or Editor theme preferences are changed.

`capture.cs.txt` is compiled in memory with Unity Pipeline `run_script` (copy to a temporary `.cs` file, entry `DocumentationCapture.DisplayNames.Main`). `light.cs.txt` repaints that same window in its light palette while preserving the results from its labels (entry `LightCapture.Main`). Neither script belongs in Assets.

The originals are native computer-use screenshots: JPEG, 820 × 318 pixels, not upscaled. Export each with FFmpeg `-vf crop=820:272:0:28 -frames:v 1`. This removes the title bar and rounded bottom corners, retaining the window's own content padding. No compositing, recoloring, or edits to displayed text.

Outputs: `Documentation/Images/editor-display-names.png` and `editor-display-names-light.png` in the package.

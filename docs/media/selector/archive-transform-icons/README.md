# Native Inspector and popup recording

Recorded on 2026-09-13 in Unity 6000.4.0f1. These frames replace the rejected embedded-picker recordings archived below.

The native InspectorWindow shows the sample Loadout on a temporary GameObject in a preview scene. Its window is 760×700 logical pixels at screen (120,100). No root transform, UI scale override, stylesheet override, popup resizing, reparenting, or replacement callbacks are used. Pistol begins with Damage 37.

`capture.cs.txt` creates the temporary object and ordinary Inspector. `open-picker.cs.txt` invokes the real field's dropdown handler and stops: the package creates its own unmodified TypeSelectorWindow. This permits opening the actual popup after moving the computer-use pointer to the Inspector title bar, outside the captured area. Two Down keys select Shotgun; Return confirms through the original callback. The actual Inspector then shows Damage 37 and Pellets 8.

Capture preparation: target the correct Editor explicitly with `unity command editor_focus --project-path ...`, focus the temporary Inspector, and keep Unity foreground while capturing. Every original PNG is one real screen-region capture using `/usr/sbin/screencapture -x -R120,100,760,570 <output.png>`. The Retina output is 1520×1140. Inspector and popup are captured together by macOS; no window screenshots are stitched, no cursor is painted out, and no app-window filtering is applied. The crop excludes title chrome and retains the complete native popup, including border and shadow.

Run `sh docs/media/selector/archive-transform-icons/encode.sh <output.gif>`; it never writes the published GIF. The loop uses the returned Pistol frame as its opening state: Pistol (1.2 s), forward picker (0.7 s), Shotgun selected (0.5 s), Shotgun Inspector (1 s), return picker (0.7 s), Pistol selected (0.5 s), then back to the same returned Pistol frame. `capture-return.cs.txt` and `open-return-picker.cs.txt` prepare the matching native Inspector for the reverse capture; two Up keys and Return select Pistol through the actual menu. Damage remains 37 in both directions. The original `01-pistol.png` is retained as provenance, but the loop starts with `07-pistol-returned.png` to avoid a focus-color discontinuity at the seam. Encoding uses 10 fps, a shared 256-color palette and sierra2_4a dithering, with no further cropping, scaling or compositing. Existing GIF Unity GUID is preserved.

After capture the temporary Inspector and preview scene were closed. The active user scene was neither saved nor reverted. Recreate instance IDs for a new session; do not reuse IDs from the scripts blindly.

`archive-custom-window/` and `archive-embedded-inspector/` contain rejected attempts for provenance only. They must not be used as publication sources.

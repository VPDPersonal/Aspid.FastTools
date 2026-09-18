# Tooling documentation media

Recorded in Unity 6000.4.0f1 on 2026-09-13 using Unity CLI and computer use. Previous JPEG sources and recipes are preserved in `archive-jpeg/`.

`capture.cs.txt` creates a temporary AssetDatabase.CopyAsset copy of BrokenWeaponPreset and an ordinary FastTools Asset References window at (100,100), 880×640 logical pixels. No UI scale override, stylesheet change or picker resizing. Open Fix Missing, search Pistol, blur the search, select the result, and press Return. A SerializedObject read verifies that the real repair preserves Damage 25 and Magazine Size 8.

Original PNGs are lossless Retina screen captures, 1744×1268 pixels: `/usr/sbin/screencapture -x -R104,100,872,634 <file.png>`. The crop excludes native title chrome and rounded bottom corners. Move the computer-use pointer to the title bar before each capture. Keep Unity foreground. No compositing, enlargement, cursor removal or artificial padding.

Run `sh docs/media/tooling/encode.sh`. States last 1.5 / 1 / 0.6 / 1 seconds; `-t 4.1` prevents the repeated terminal frame from adding an extra pause. The result holds No missing references for one second. Encoding uses 10 fps, a shared 256-color palette and sierra2_4a dithering at the original pixel dimensions. Keep the existing GIF Unity GUID.

After capture the temporary window and asset folder were removed, selection restored, and the already-dirty active scene left unsaved and outside Play Mode. Session-specific instance IDs must be discovered again before reuse.

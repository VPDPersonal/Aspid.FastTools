# Tooling documentation media

Captured in Unity 6000.4.0f1 on 2026-09-13 using Unity CLI and computer use.

A temporary AssetDatabase.CopyAsset copy of the sample BrokenWeaponPreset is opened in a separate real FastTools Asset References window, rendered at 1.25× UI scale. The inline picker height is reduced to 200 logical pixels for this capture only, so its complete card fits the window. Search Pistol, select it and confirm. The actual repair changes GhostWeapon to Pistol and removes the missing-reference warning. A SerializedObject read verified Damage 25 and Magazine Size 8 were preserved.

The original native captures are JPEG (`*.jpg`), 960×553 for Selector and 1100×708 for Tooling. No upscaling of captured pixels. Capture scripts are stored as `.cs.txt`; instance IDs are session-specific and must be replaced when recreating the temporary windows. The original scene and selection were restored, temporary assets and windows removed, and the active scene remained clean and outside Play Mode.

Run `sh docs/media/tooling/encode.sh` from the repository root. The ffconcat recipe holds the original frames for 2 / 1.5 / 1 / 3 seconds. Crop `1088:674:6:28` removes native window chrome and rounded corners; no padding, frame, cursor or screenshot compositing is added. FFmpeg encodes at 10 fps with a shared 256-color palette and sierra2_4a dithering. Final output is used by both locale pages and the generated root README.

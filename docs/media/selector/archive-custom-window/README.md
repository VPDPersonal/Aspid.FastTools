# Selector documentation media

Captured in Unity 6000.4.0f1 on 2026-09-13 using Unity CLI and computer use.

A temporary preview scene hosts the real sample Loadout, initialized to Pistol with Damage 37. The capture window renders native PropertyFields at 1.5× UI scale. The package TypeSelectorView is hosted inline to keep the picker and Inspector in the same native capture; its selection callback uses the package CreateInstancePreservingData helper. Selecting Shotgun preserves Damage 37 and exposes Pellets 8. The files capture actual rendered controls, without compositing or invented results.

The original native captures are JPEG (`*.jpg`), 960×553 for Selector and 1100×708 for Tooling. No upscaling of captured pixels. Capture scripts are stored as `.cs.txt`; instance IDs are session-specific and must be replaced when recreating the temporary windows. The original scene and selection were restored, temporary assets and windows removed, and the active scene remained clean and outside Play Mode.

Run `sh docs/media/selector/archive-custom-window/encode.sh <output.gif>`; it never writes the published GIF. The ffconcat recipe holds the original frames for 2 / 1.5 / 1 / 3 seconds. Crop `960:510:0:28` removes native window chrome and rounded corners; no padding, frame, cursor or screenshot compositing is added. FFmpeg encodes at 10 fps with a shared 256-color palette and sierra2_4a dithering.

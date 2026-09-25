# Light sample recordings

Real Unity 6000.4 camera recordings with the shared Light preview: neutral gray platform/background, mint and pale blue accents, subdued grid and lifted animated colors. Authored gameplay runs normally: Types shows regular/elite waves moving to the center (3 seconds), SerializeReferences shows real attacks and changing dummy health (6 seconds), ProfilerMarkers shows the flock simulation (6 seconds, deterministic seed 42).

Camera sources are lossless FFV1, 1440 × 810 at 20 fps. The recording helper hides scene title/footer TextMeshes only during each camera render to keep them outside the cropped gameplay illustration, then restores them immediately. Full scene PNGs preserve titles for tutorial/gallery previews. Crops: Types 1204×680 at (100,116), SerializeReferences 888×532 at (276,178), ProfilerMarkers 800×688 at (320,108). No image recoloring or compositing.

Catalog sources are native computer-use window screenshots (JPEG, 1124×768); they are not upscaled. The real Ability Catalog uses its new Light theme, with a temporary editable clone of Ward so no user asset changes. The real button changes 15 MP / 12 seconds to 20 MP / 6 seconds; Cmd-Z returns 15 / 12, verified with SerializedObject. Each state lasts 1.2 seconds. The body crop 1116×420 at (4,132) excludes native window chrome and cursor; the static preview includes the header and theme selector. The temporary clone/window were removed and previous theme restored.

Run `python3 docs/media/sample-themes/other-samples/encode.py` from any directory to reproduce all GIFs/previews. GIFs use 256 colors and sierra2_4a dithering. Existing media GUIDs are preserved. Dark media are unchanged.

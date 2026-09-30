# Samples gallery media

The cards of `/tutorials` (`Website/src/components/SamplesGallery`) use themed images without playback controls.

- **Scenes** (EnumValues, Types, SerializeReferences, ProfilerMarkers): `python3 docs/media/samples-gallery/scenes.py`
  writes looping `Website/static/img/samples/<slug>(-light).gif` pairs from the lossless recordings in
  `docs/media/sample-themes`. Each pair uses the same 16:9 crop and start frame, at 1024×576 and 20 fps,
  with 256 colors and sierra2_4a dithering. Backgrounds match `--venom-reading-surface`; rerun when that token changes.
  EnumValues uses `enum-values-dark.mkv` and `enum-values-light.mkv`, each with 355 frames (17.75 s).
  Both cropped recordings are placed at (169, 270) in camera space and start at frame 29 (zero-based).
  Pass `enum-values` to rebuild only that card. GIFs loop natively in the browser; there is no video layer or
  JavaScript playback management.
- **Ability Catalog** (EditorTools): the static idle grab of the real window, scaled to 1440×810 by
  `docs/media/sample-themes/other-samples/encode.py` (grabs and steps in its `catalog/`), stays a PNG.
  Its demo is a value change too small to read at card size.

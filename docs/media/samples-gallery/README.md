# Samples gallery previews

The cards of `/tutorials` (`Website/src/components/SamplesGallery`) show `Website/static/img/samples/<slug>.png` and
`<slug>-light.png`. Each pair has one size and one 16:9 frame, and no scene titles: the card names the sample.

- **Scenes** (EnumValues, Types, SerializeReferences, ProfilerMarkers): `python3 docs/media/samples-gallery/scenes.py`
  writes the previews and the clips that loop on the cards (`Website/src/components/SamplesGallery/media/`) from the
  lossless recordings of `docs/media/sample-themes`, one per theme and frame for frame, so both themes show the same
  moment. Pass a slug (for example `enum-values`) to rebuild only that sample. A clip is muted H.264 at 1024×576 and starts on its
  preview's frame. The camera background is repainted: to `--venom-reading-surface` in the previews, to black or white
  in the clips, which the card blends (`lighten` / `darken`) with a layer of that surface, since 8-bit YUV misses it by
  one level. Change the surface token → rerun the script. Reduced motion keeps the still.
- **Ability Catalog** (EditorTools): the idle grab of the real window that the sample's `demo.gif` is composed from,
  scaled to 1440×810 by `docs/media/sample-themes/other-samples/encode.py` (grabs and steps in its `catalog/`). No clip:
  its demo is a value change too small to read at card size.

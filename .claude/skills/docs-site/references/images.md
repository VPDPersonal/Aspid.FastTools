# Images: cropping, scenes and live versions

Details behind the Images rules of the `docs-site` skill. Paths starting with `src/` are relative to `Website/`;
`scripts/` is at the repository root, and the capture recipes are in `Website/media/`.

- **Inspector and picker captures are cropped in the file, not in CSS.** `scripts/frame-doc-captures.sh` crops each
  listed capture (both themes) to its component and adds an 8px margin that continues Unity's background, the
  Inspector header included; GIFs go through gifsicle, so pixels and timing stay exact. GitHub and the site
  then show the same image. After re-shooting a listed capture, run the script; it skips framed files and fails on a
  capture whose size no longer matches its row, which then needs new bounds. Only `enum-values-multipliers/padding/pad.py`
  and `enum-values-multipliers/type-selector/crop.sh` in `Website/media` still write a listed capture, and they call it
  themselves; the other listed captures come from the DocsMedia harnesses, so run the script after re-shooting them.
  On doc pages the site pads every listed capture on to 8px from its rounded frame, drawing it as a border image so
  the margin's bands meet the edge without a seam; `docusaurus.config.js` reads the list and `PAD` from the script
  (`customFields.framedCaptures`), so a new row needs nothing else. The introduction's cards do the same in `src/css/custom.css`.
  Whole-window captures (`aspid_fasttools_serialize_reference_*` from the SerializeReferences DocsMedia harness) keep
  the window's 24px margin; doc pages crop it to the same 8px (`WINDOW_CAPTURES` in `src/theme/MDXComponents/Img`), so a new
  window capture is added to that pattern.
- Which samples are scenes is the **hardcoded `SCENE_SAMPLES` list** in `src/remark/themedImages.js` (folder names under
  `Website/tutorials/`). It drives both `.sample-scene` (the background-recolouring filter on the sample's tutorial page)
  and `.scene-footage`; a sample missing from it just keeps its own background. Nothing fails the build.
- Click or Enter opens the image in a modal (Esc closes). Bare images are capped at 640×520;
  captures in `doc-image-panel` and `.sample-scene` media fill the article.
- Status badges (`Images/status-badge-*.svg`) are links, not captures: they keep their size and do not zoom.
- A static picture can have a live site version: `src/remark/liveDiagrams.js` maps the file name to a component
  (`profiler-markers-hierarchy.svg` → `ProfilerHierarchy`). Markdown
  keeps the picture for GitHub, dark only: the site never shows it, so it has no light sibling. The caption paragraph
  must still repeat the alt text as plain text.
- A table can have a live site version the same way: `TABLES` in `liveDiagrams.js` maps its first body cell to a
  component (`SetPadding(8)` → `StyleSides`, the Styles table of VisualElement Extensions), which gets every row as
  `<StyleSidesRow call="…">` with the second cell as children. Markdown keeps the table, and the rows and their
  translations stay in the page; changing that first cell detaches the component.
- An ordered list can have one too: `LISTS` in `liveDiagrams.js` maps the bold text of its first item to a component
  (`Tools → Aspid 🐍 → FastTools → Project References` → `ProjectReferencesPanel`, the SerializeReference repair quick
  start), which gets each item's text as a `<span>` child, so the steps and their translation stay in the page; changing
  that bold text detaches it. The panel reuses the install panel's card and steps and draws the Aspid FastTools window
  without its tab strip; in the light site theme it takes Unity's light skin and the window's light palette
  (`Aspid-FastTools-Default-Light.uss`), so update both when the package palette changes.

# README feature previews

The root README cannot run the introduction's animated feature previews (`Website/src/components/FeaturePreview`), so
`Website/scripts/github-readme.mjs` shows a recording of each one in its card:
`docs/images/readme-previews/<doc>[-light].webp`.

`record.mjs` opens the built introduction in headless Chrome on virtual time and records one loop of each preview
at 2x in the green accent, per theme: 20 fps, the Profiler's scrolling timeline 12.5 fps. `img2webp` encodes
it (lossy, q 82). Each loop length comes from the preview's `useLoop` steps (the Profiler's ticker for ProfilerMarkers);
update `PREVIEWS` when they change. The recording drops the card's frame, since on GitHub the table cell is the frame.

1. Build and serve the site: `Website/scripts/serve-all.sh`.
2. Record every preview, or only the named ones: `node docs/media/readme-previews/record.mjs http://localhost:<port>/Aspid.FastTools/ [doc …]`.
3. Regenerate the README: `npm --prefix Website run sync-readme`.

Needs Google Chrome and `img2webp` (`brew install webp`).

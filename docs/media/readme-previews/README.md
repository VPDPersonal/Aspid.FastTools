# README feature previews

The root READMEs cannot run the introduction's animated feature previews (`Website/src/components/FeaturePreview`), so
`Website/scripts/github-readme.mjs` shows a recording of each one in its card:
`docs/images/readme-previews/<doc>[-ru][-light].webp`. A preview with Russian text (`ru` in `record.mjs`) gets a `-ru`
recording for `README.ru.md`; the others share the English one.

`record.mjs` opens the built introduction in headless Chrome on virtual time and records one loop of each preview
at 2x in the green accent, per theme and language: 20 fps, the Profiler's scrolling timeline 12.5 fps. `img2webp` encodes
it (lossy, q 82). Each loop length comes from the preview's `useLoop` steps (the Profiler's ticker for ProfilerMarkers);
update `PREVIEWS` when they change. The recording drops the card's frame: the README shows previews without one.

1. Build and serve the site: `Website/scripts/serve-all.sh`.
2. Record every preview, or only the named ones: `node docs/media/readme-previews/record.mjs http://localhost:<port>/Aspid.FastTools/ [doc …]`.
3. Regenerate the READMEs: `npm --prefix Website run sync-readme`.

Needs Google Chrome and `img2webp` (`brew install webp`).

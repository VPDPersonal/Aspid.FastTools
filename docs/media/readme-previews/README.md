# README feature previews

The root README cannot run the introduction's animated feature previews (`Website/src/components/FeaturePreview`) or
the install panel's Package Manager walk-through (`Website/src/components/InstallPanel`), so
`Website/scripts/github-readme.mjs` shows a recording of each one in its card:
`docs/images/readme-previews/<name>[-light].webp`, `install` for the walk-through.

`record.mjs` opens the built introduction in headless Chrome on virtual time and records one loop of each preview
at 2x in the green accent, per theme: 20 fps, the Profiler's scrolling timeline 12.5 fps. `img2webp` encodes it
losslessly: lossy WebP leaves ghosts of earlier frames on these flat interfaces. The Profiler's timeline takes mixed
frames (q 90) instead, which keeps it near 2 MB. Each loop length comes from the preview's `useLoop` steps (the
Profiler's ticker for ProfilerMarkers, `DURATIONS` in `InstallPanel` for the walk-through); update `PREVIEWS` when they
change. The recording drops the frame of the card or panel, since on GitHub the table cell is the frame. The
walk-through hides the package version, but its typed URL names the channel (`#upm-preview`), so re-record `install`
when a release switches channels; `scripts/set-version.sh` says so.

1. Build and serve the site: `Website/scripts/serve-all.sh`.
2. Record every preview, or only the named ones: `node docs/media/readme-previews/record.mjs http://localhost:<port>/Aspid.FastTools/ [name …]`.
3. Regenerate the README: `npm --prefix Website run sync-readme`.

Needs Node 22+, Google Chrome and `img2webp` (`brew install webp`). Outside macOS, set `CHROME` to the Chrome binary.

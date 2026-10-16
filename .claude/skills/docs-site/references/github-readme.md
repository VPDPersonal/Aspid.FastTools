# GitHub README generator

`npm --prefix Website run sync-readme` gives the root README GitHub's version of the site's introduction blocks
(`Website/scripts/github-readme.mjs`, tested in `github-readme.test.mjs`):

- Doc-page and samples-overview links open the site; the link row's site link becomes `Documentation: EN, RU`.
- The install instruction becomes a card (the recorded InstallPanel walk-through beside its steps), and the note under
  the URL links the site's install panel for versions and channels.
- Each feature becomes a card: a one-row table with the preview, then title, summary and «Read more».
- The Resources list becomes a row of tiles, and the help section the SupportPanel call to action without the licence
  line.
- A code preview gives way to the recording of the site's animated preview, `docs/images/readme-previews/<name>.webp`.

The README shows only dark images: a `<picture>` per theme follows the system theme, not GitHub's, so the generator
drops the `-light` siblings and the recorder records the dark theme only.

After a change to `FeaturePreview`, the InstallPanel walk-through, the site's dark theme or the install channel (the
walk-through types its URL), rebuild the site and re-record:
`node docs/media/readme-previews/record.mjs <served URL> [name …]` (`docs/media/readme-previews/README.md`), then run
`sync-readme`.

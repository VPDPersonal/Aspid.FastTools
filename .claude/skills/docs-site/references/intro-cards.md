# Introduction feature cards

There is no landing page: `/` redirects to `/docs`. The README banner is a static 2560×768 PNG
(`docs/images/aspid_fasttools_readme_banner.png`, the green dark variant), designed on the «README banner» page of the
«FastTools Asset Store» Design canvas: one board per accent and theme. The plugin renders it as
`src/components/IntroBanner`: that layout rebuilt in HTML, the logo (`media/logo-<accent>.webp`, 640 px, picked from
`html[data-accent]`) and the live title and tagline, sized in `cqi` so it scales like the PNG. It has no background: on
wide screens the article panel paints its surface only below the banner (`custom.css`), so the dot canvas shows
through it, `DotAmbient` raises a glow from its bottom edge, and `DotRipple`'s `isCanvas` and `DotSpotlight` treat the
banner as canvas. Once per tab session the banner makes an entrance: `src/intro.js` sets `html[data-intro]` before the
first paint (never under reduced motion), `styles.css` draws the ring from the snake's head and wipes the name in, and
`IntroBanner` sends a `DotRipple` wave with the burst's shake (`sendWave`) from the logo when the ring closes and lets the glow rise; below
997 px the banner paints
the dots itself. A new accent in `src/accents.js` needs its logo WebP.
On the introduction, `src/remark/introBanner.js` also turns the README's
Features section into card grids. A card whose page has an entry in `src/components/FeaturePreview` (EnumValues and
the Editor & tooling features) shows that animated preview instead of the README capture; its code is hand-written
there, so update it when the page's quick start changes. The EnumValues clip is `FeaturePreview/media/enum.mp4`, with `enum-light.mp4` for the light theme. Both are the
`enum-values-multipliers-populate(-light).gif` padded to 1576×1080 (`-preset veryslow -crf 24
-vf "fps=20,pad=1576:1080:20:206:color=0x333333"`, `0xC8C8C8` for light).
The Inspector captures of the Serialization cards are cropped and framed in the file (`scripts/frame-doc-captures.sh`), so each card
takes the capture's own shape. The Serialization cards have their own captures (`*-card.gif`), shot in a 440 pt Inspector or
FastTools window so their text reads at the card's width, with the picker's empty rows above its footer cut out of every
frame; the doc pages keep the wider captures (their empty picker rows are cut too). A card stays about as tall as its text column, so cut a card
capture's empty UI too (a hint line the card text repeats, a state that only adds empty rows); over the FastTools
window's dot grid cut whole ~36.9 px steps so the seam does not show. Inside an animated preview the blocks keep the
preview inset between them as well as around them, the spare height goes to the code, centred, and no note line
explains the result (a preview list resets the article's `.markdown li + li` margin).

The same plugin replaces the Installation section's instruction, URL block and version note with
`src/components/InstallPanel`: a Package Manager walk-through beside the steps, and the URL to copy with Stable
and Preview dropdown tabs. Each offers Latest or a pinned version; a channel without versions is disabled.
The README's URL carries the channel — `#upm-preview` for a prerelease, `#upm` for a stable version — and
`scripts/set-version.sh` switches it together with the badge label. The README URL selects the initial channel;
each tab pins from its own branch out of `customFields.packageVersions` (the tags of both branches, per branch); `UPM_BRANCH` in `docusaurus.config.js`
follows the `package.json` version the same way and only decides where the working-tree version is offered.
Its text is written in the component per locale, so update it when the README's install steps change.

`static/img/logo.png` and `favicon.png` are copies of the package icon
`Editor/Resources/Icons/aspid_icon_medium_green_256x253.png`, and `logo-red.png`, `logo-blue.png`, `logo-yellow.png` of
its colour variants, `logo-mono.png` a greyscale copy of the green one (each also the favicon for its accent); re-copy them if the icons change. `src/theme/Logo` renders
all four and CSS shows the current accent's.

The page uses normal document scrolling with sticky navigation. The borderless article has an opaque reading
surface (graphite in dark mode, warm linen in light mode). The fixed dot texture is painted on `html`, not the
viewport-height `body`, so it remains visible in the margins throughout long articles. Both navigation columns
share `--venom-navigation-width` (260px). Below 1400px the right TOC becomes an in-article disclosure, and
below 997px the navigation uses Docusaurus' mobile menu. On desktop (≥997px) the navbar is hidden and
`src/theme/DocSidebar/Desktop` wraps the sidebar into a full-height panel: pinned header (brand, section
switcher built from the navbar's left items, search), scrolling document list, pinned footer (GitHub,
language, theme).

Blocks follow the admonitions: a frame on the article surface, no fill of their own. Admonitions carry a coloured
2px border and heading; code blocks, tables, `<details>`, cards, panels and the diagrams (`StyleSides`,
`ProfilerHierarchy`, the feature previews) a 2px `--venom-line-strong` frame with the 12px `--ifm-pre-border-radius`.
Their surface is `--venom-block-surface` (transparent), or `--venom-reading-surface` where it must stay opaque (sticky
cells). A fill is kept only where content needs it: inline code chips (`--ifm-code-background`), Unity mock-ups (their editor
skin) and the letterbox behind captures. Controls and popovers (section switcher, search, floating TOC, the install
version list) are opaque in the article's own `--venom-reading-surface`, so the page has one surface colour, with a
1px frame. Radii come in three steps: 12px for blocks, 10px for buttons, popovers, dialogs and selected items (sidebar,
search results), 6px for small plaques (the Esc/Close keys, `kbd`, status badges, accent swatches, items inside a
popover); inline code keeps 4px so it does not turn into a pill. Unity mock-ups keep the editor's own radii. Table rows have no zebra; the header is muted and closed by a 2px rule, its inline code without a chip.
Captures have no frame around them (see Images).
`DotRipple` draws the click ripple in the accent colour on the empty canvas around the article.

A selection mark sits on the frame line, not inside it, and matches its 2px: the picked call in `StyleSides`, the
selected row in `ProfilerHierarchy`, the marked code lines in `AgentSession` and the introduction's animated previews,
the install panel's tab underline (which runs the strip's side padding past its label on both sides, the first one
from the card's edge). At a rounded corner the mark stops where the straight edge ends. Where the block scrolls or
clips (Hierarchy, the feature cards), this needs `overflow: clip; overflow-clip-margin: border-box` inside
`@supports`; Safari lacks it and keeps the mark just inside the frame.

Colours in the **light theme** have roles, so nothing is darkened more than contrast needs (a darker tone reads muddy);
the dark theme's colours are bright enough to serve every role and are left alone:

- `--venom-accent`: text (links, the picked item) — the mark hue darkened only to 4.5:1 on white;
- `--venom-canvas-accent`: text on the grey canvas (the sidebar's active item, the TOC) — 4.5:1 there;
- `--venom-accent-mark`: marks and tints (frame bars, underlines, outlines, focus rings, dots, `--venom-glow*`) — 3:1;
- `--venom-accent-fill` / `-fill-ink`: filled surfaces with text (the primary button, the active install step) —
  the text accent with white ink, except yellow (bright amber with dark ink) and mono (a mid grey).

Light yellow leans amber, since a darkened true yellow turns olive. Status colours split the same way:
`--venom-{emerald,sapphire,ruby,amber}` for an admonition's heading and icon, `…-mark` for its frame. The light Prism
theme (`src/prism/venom.js`) is Ayu Light's hues darkened only to 4.6:1 on the inline-code chip. Static light SVGs that
draw code or marks (`Website/docs/Images/*-light.svg`) use the same values; update them when the palette changes.

`src/plugins/search` builds a locale-specific index from Docusaurus' resolved document sources and permalinks.
`src/theme/SearchBar` loads it on demand, searches Docs/Samples/API/Changelog, and supports Cmd/Ctrl+K, arrow
keys, Enter and Esc. `node --test scripts/*.test.mjs` in `Website/` (`npm test`) checks matching, Markdown extraction and index URLs.

`src/components/DotRipple` draws the click ripples on the dot background; the number of waves is unlimited, and
`waves.js` culls, per grid row, the waves whose ring misses it. `node --test scripts/dot-ripple.test.mjs` checks
that no dot of a ring is dropped.

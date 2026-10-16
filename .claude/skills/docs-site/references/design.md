# Site design: accents, fonts, Prism

Details behind the Design rules of the `docs-site` skill. Paths are relative to `Website/`.

## Accents

The reader can switch the accent from the default green to red, blue, yellow or mono in the sidebar footer appearance
menu, next to the theme (`src/components/NavigationPanel/AppearanceSwitcher.js`). The variants live in `src/css/accents.css` under
`html[data-accent]`, the list and the pre-paint boot script in `src/accents.js`. The accent's favicon is set on the plain
`<link rel="icon">` in `headTags`, not on the `favicon` field: React Helmet owns that field's tag and resets it on every
navigation.

## Fonts

IBM Plex Serif (headings), Plex Sans (text) and Plex Mono (code) are self-hosted: the site makes no request to Google.
`src/css/fonts.css` declares the woff2 files of the exact-pinned `@fontsource/ibm-plex-*` packages, Latin and Cyrillic
only, each with its `unicode-range` (the packages' own per-subset CSS has none and adds woff copies). A new language
outside those two needs `latin-ext` or `cyrillic-ext` rules there. `src/plugins/fonts` makes Docusaurus emit fonts as
files, because it inlines anything under 10 kB into the stylesheet of every page. iA Writer Quattro, the samples' editor
font, is self-hosted in `src/fonts/` (OFL, keep the licence file) for navigation labels only — sidebar and TOC,
`--venom-font-family-nav`.

## Prism

Prism themes are Ayu-based, in `src/prism/venom.js`, with a transparent background.

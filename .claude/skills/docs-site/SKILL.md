---
name: docs-site
description: How Aspid.FastTools documentation is authored and published — Markdown inside the UPM package (`Documentation/`, `Documentation/ru/`, and each sample's `Documentation/` folder) read by GitHub, Unity and the Docusaurus site in `Website/`, deployed to GitHub Pages. Use when adding or editing any documentation page, translation, sample README, or the site itself.
user-invocable: false
---

# Documentation site

One source of truth: Markdown files inside the UPM package. The same file is read by GitHub, by Unity
(as a `TextAsset` in the Inspector) and by the Docusaurus site in `Website/`. Nothing is copied by hand
and the root `README.md` is only an overview with links. Write GitHub Flavored Markdown; the site adapts
to it, never the other way round.

## Layout

| What | Where | Site route |
|---|---|---|
| Main docs | `Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/NN-*.md` | `/docs/<name>` (`02-serializable-types.md` → `/docs/serializable-types`) |
| Introduction | `Documentation/README.md` | `/docs` |
| Tutorials | `Samples~/<Sample>/Documentation/README.md` and `TUTORIAL.md` | `/tutorials/<slug>`, `/tutorials/<slug>/tutorial` (`SerializeReferences` → `serialize-references`; an optional `NN. ` folder prefix orders and is stripped) |
| Translations | `Documentation/ru/**` (same names), `Samples~/<Sample>/Documentation/README.ru.md`, `TUTORIAL.ru.md` | `/ru/...` |
| Images | `Documentation/Images/` | referenced relatively |
| Site config | `Website/docusaurus.config.js`, `sidebars.js`, `sidebarsTutorials.js` | |
| CI | `.github/workflows/docs.yml` → GitHub Pages `https://vpdpersonal.github.io/Aspid.FastTools/` | |

The site folder is `Website/`, not `Docs/`: the repo already has `docs/` (internal working documents) and
macOS treats the two names as one directory.

Two docs plugin instances: the default one (`docs`) points at `Documentation/` (translation folders and
`SUMMARY.md` excluded), while the second (`id: 'tutorials'`) reads the generated `Website/tutorials/` tree.
`sync-i18n.mjs` builds that tree from every sample's `Documentation/` folder so public routes stay unchanged.
Scripts, scenes and `.meta` files are never copied.

`Website/scripts/sync-i18n.mjs` (run by `prestart`/`prebuild`) copies the English sample documentation into
`Website/tutorials/`, plus `Documentation/<locale>/`, every sample-local `*.<locale>.md`, and images into the
`Website/i18n/` layout Docusaurus expects. `Website/tutorials/` and `Website/i18n/` are build artifacts and are
gitignored; never edit them by hand.

`SUMMARY.md` is the GitHub table of contents; `Website/sidebars.js` groups the main docs into Serialization
and Editor & tooling. Add each new page to the appropriate group.

## Writing rules (so all three renderers agree)

- **No front matter.** Unity and GitHub would show it as text. Title comes from the first `# H1`,
  slug and order come from the file name (`NN-` prefix orders, is stripped from the route).
- **Admonitions**: GitHub style only — `> [!NOTE]`, `TIP`, `IMPORTANT`, `WARNING`, `CAUTION`. Never `:::note`.
- **Links** are relative paths to the `.md` file: `[EnumValues](06-enum-values.md)`, from a sample
  `[Selector](../../../Documentation/03-serialize-reference-selector.md)`, from a doc
  `[Types sample](../Samples~/Types/Documentation/README.md)`. GitHub follows them as files; links that cross between
  the two plugin instances are rewritten to site routes by `Website/src/remark/crossInstanceLinks.js`.
  Never link by site URL.
- **One `# H1` per file.** Use `##` in the body.
- **Images** for main docs live in package-level `Documentation/Images/`; each sample keeps its own images in
  `Samples~/<Sample>/Documentation/Images/`. Reference sample images as `Images/x.png` from its README.
- **Every `.md` in the package needs a `.meta`** (`TextScriptImporter`) — Unity would otherwise generate one
  in the consumer's project. Copy an existing one and give it a fresh GUID.
- The package is English. A translation is a sibling file: `Documentation/ru/06-enum-values.md`,
  `README.ru.md` next to `README.md`. Missing pages fall back to English. A translated file links translated
  targets (`../../Samples~/Types/Documentation/README.ru.md`) so GitHub stays in the same language; the site drops the
  locale segment itself.
- Adding a language: create `Documentation/<locale>/` and `*.<locale>.md` files and add the locale to
  `LOCALES` in `docusaurus.config.js`. Nothing else: `sync-i18n.mjs` discovers locale folders by name, and
  `LOCALES` is what keeps them out of the English `docs` instance.

## Adding a main doc page

Drop `NN-name.md` into `Documentation/`, add its row to `Documentation/README.md` (feature table) and to
`SUMMARY.md`, add the `.meta`, optionally the translation at `Documentation/ru/NN-name.md`, and add its id to
`Website/sidebars.js`. Also update the feature table in the root `README.md`.

## Adding a sample

1. Create `Samples~/<Name>/Documentation/README.md` (+ `TUTORIAL.md` if there is a guided scene), with `.meta` files.
   Put that sample's documentation images in `Documentation/Images/`.
2. Add a category with `<slug>/readme` and `<slug>/tutorial` to `Website/sidebarsTutorials.js`; drop the
   `items` entry if there is no `TUTORIAL.md`.
3. Register the sample in `package.json` → `samples`, and add its row to `01-getting-started.md` (EN and `ru/`).

## Local run / check

```bash
cd Website
npm ci
npm start                # http://localhost:3000/Aspid.FastTools/ (prestart syncs translations)
npm run start:ru         # Russian locale (dev server serves one locale at a time)
npm run build            # what CI runs; fails on broken links
```

`onBrokenLinks` and `onBrokenMarkdownLinks` are `throw`: a bad relative link breaks the build on purpose
(`onBrokenAnchors` only warns — check the log for `#anchor` typos).
Do not run `npm run build` while a dev server is running from the same folder — they share `.docusaurus/`
and `i18n/`.

## Versioning

`npm run version 1.0.0` snapshots the main docs into `Website/versioned_docs/`; tutorials are not versioned.
The version dropdown reads `Website/versions.json`; until the first snapshot it shows the package version
from `package.json`.

## API reference (`/api`)

The **API** tab is generated from the XML doc comments by DocFX, not written by hand. `Website/api/` holds the
generated Markdown plus `sidebar.js` and **is committed**: DocFX compiles the assemblies against the local Unity
installation, which CI does not have.

```bash
dotnet tool install -g docfx            # once; the tool lands in ~/.dotnet/tools — make sure it is on PATH
cd Website && npm run api               # regenerate after public API or XML doc changes
```

`npm run api` runs three steps (`Website/scripts/docfx-*.mjs`, `Website/docfx/docfx.json`):

1. `docfx-projects.mjs` writes SDK-style projects for `Aspid.FastTools` and `Aspid.FastTools.Editor` under
   `Website/docfx/projects/` (gitignored, absolute paths). Sources come from each asmdef folder (so a stale
   csproj `<Compile>` list does not matter); references, defines and the source generators come from the
   Unity-generated `.csproj`, and other Unity assemblies are referenced from
   `Library/ScriptAssemblies` — open the project in Unity once so both exist and are fresh.
2. `docfx metadata` writes one Markdown page per type into `Website/api/`. A `warning: InvalidCref` here is a
   real bug in an XML comment — fix the `cref`, do not ignore it.
3. `docfx-postprocess.mjs` makes it MDX-safe: `<xref>` → links (own pages, learn.microsoft.com, Unity Scripting
   Reference), `<pre><code>` → fenced code, heading anchors as `{#id}`, escaped `<T`/`{}`, no "Inherited Members",
   front matter with a short `sidebar_label`, and `toc.yml` → `sidebar.js` (namespace → Classes/Interfaces/…
   groups). A type name that appears in two namespaces (`TypeExtensions`, `VisualElementExtensions`) gets a
   namespace suffix in its label — Docusaurus derives one translation key per label and the `ru` build fails
   on duplicates.

The pages are served by the third docs plugin instance (`id: 'api'`). `Website/sidebarsApi.js` adapts the generated
sidebar labels for display, omitting the repeated `Aspid.FastTools.` namespace prefix. Never edit files in `Website/api/` by hand;
fix the XML comment or the postprocess script and regenerate. Translations are not generated; the `ru` locale
falls back to the English pages. The Math satellite assembly is not documented — it compiles only when
`com.unity.mathematics` is installed, which this project does not.

## Design

The theme is shared with Aspid.MVVM: dark graphite with the Unity badge green as accent (`--venom-*` tokens in
`Website/src/css/custom.css`), IBM Plex Serif/Mono from Google Fonts, iA Writer Quattro body self-hosted in
`src/fonts/` (OFL, keep the licence file), Ayu-based Prism themes in `src/prism/venom.js`. `src/pages/index.js`
redirects `/` to `/docs` — there is no landing page yet.

`static/img/logo.png` and `favicon.png` are copies of the package icon
`Editor/Resources/Icons/aspid_icon_medium_green_256x253.png`; re-copy them if the icon changes.

The page uses normal document scrolling with sticky navigation. The borderless article has an opaque reading
surface (graphite in dark mode, warm linen in light mode). The fixed dot texture is painted on `html`, not the
viewport-height `body`, so it remains visible in the margins throughout long articles. Both navigation columns
share `--venom-navigation-width` (260px); group headings use distinct blocks, and the right TOC has no left rule.
Below 1400px the right TOC becomes an in-article disclosure, and below 997px the navigation uses Docusaurus'
mobile menu. Code blocks, images and other components share 4–6px corner radii.

`src/theme/MDXComponents/Img` opens documentation images in a native modal dialog (click/Enter to open, Esc to
close). Inspector screenshots are capped at 640px; `.sample-scene` media can fill the article. Repeated alt-text
captions receive a class from `remark/themedImages.js` without changing package Markdown.

`src/plugins/search` builds a locale-specific index from Docusaurus' resolved document sources and permalinks.
`src/theme/SearchBar` loads it on demand, searches Docs/Samples/API/Changelog, and supports Cmd/Ctrl+K, arrow keys,
Enter and Esc. `node --test scripts/search.test.mjs` checks matching and Markdown extraction.
Site interface translations live in `Website/translations/<locale>/`; `sync-i18n.mjs` copies them into generated
`i18n/` before each build.

On desktop (≥997px) the navbar is hidden and `src/theme/DocSidebar/Desktop` wraps the sidebar into a full-height
panel: pinned header (brand, section switcher built from the navbar's left items, search), scrolling document list,
pinned footer (GitHub, language, theme). Below 997px the stock navbar and mobile drawer remain. The changelog gets its
sidebar from `changelog/sidebars.json`, generated by `sync-i18n.mjs` from the `## [version]` headings, which also
receive `{#v…}` anchors.

## Deploy

`.github/workflows/docs.yml` builds on every push to `main` touching `Website/`, `Documentation/` or a sample
`README*.md` / `TUTORIAL*.md`, and on PRs (build only). Pages source must be set to "GitHub Actions" once in the
repository settings.

---
name: docs-site
description: How Aspid.FastTools documentation is authored and published — Markdown inside the UPM package (`Documentation/`, `Documentation/ru/`, each sample's `Documentation/`) plus the root `CHANGELOG*.md`, read by GitHub, Unity and the Docusaurus site in `Website/`, deployed to GitHub Pages. Use when adding or editing any documentation page, translation, sample README, image, or the site itself.
user-invocable: false
---

# Documentation site

One source of truth: Markdown inside the UPM package. The same file is read by GitHub, by Unity (as a
`TextAsset` in the Inspector) and by the Docusaurus site in `Website/`. Nothing is copied by hand — the root
`README.md`, the tutorials tree, the i18n tree, the changelog page and the API reference are all generated.
Write GitHub Flavored Markdown; the site adapts to it, never the other way round.

Package-relative paths below are rooted at `Aspid.FastTools/Packages/tech.aspid.fasttools/`.

## Layout

| What | Source | Site route |
|---|---|---|
| Introduction | `Documentation/README.md` | `/docs` |
| Main docs | `Documentation/NN-*.md` | `/docs/<name>` (`02-serializable-types.md` → `/docs/serializable-types`) |
| Samples | `Samples~/<Sample>/Documentation/README.md` | `/tutorials/<slug>` (`SerializeReferences` → `serialize-references`; an optional `NN. ` folder prefix orders and is stripped) |
| Samples overview | `Website/src/samples/index.mdx`, `index.ru.mdx` → `<SamplesGallery/>` | `/tutorials` |
| Changelog | root `CHANGELOG.md`, `CHANGELOG.ru.md` | `/changelog` |
| API reference | generated into `Website/api/` by DocFX (committed) | `/api` |
| Translations | `Documentation/ru/**` (same names), `Samples~/<Sample>/Documentation/README.ru.md` | `/ru/...` |
| Images | `Documentation/Images/`, `Samples~/<Sample>/Documentation/Images/` | referenced relatively |
| Root README | generated from `Documentation/README.md` (committed) | — |
| Site config | `Website/docusaurus.config.js`, `sidebars.js`, `sidebarsTutorials.js`, `sidebarsApi.js` | |
| CI | `.github/workflows/docs.yml` → GitHub Pages `https://vpdpersonal.github.io/Aspid.FastTools/` | |

The site folder is `Website/`, not `Docs/`: the repo already has `docs/` (internal working documents, plus
`docs/images/` which hosts the README banner GIF that GitHub serves over `raw.githubusercontent.com`), and
macOS treats the two names as one directory.

**Four docs plugin instances**, all in `docusaurus.config.js`:

- `docs` — reads the package `Documentation/` in place; locale folders are excluded through `LOCALES`.
- `tutorials` — reads the generated `Website/tutorials/` tree (`include: ['index.mdx', '*/README.md']`).
  One page per sample; the overview page comes from `src/samples/index.mdx`.
- `changelog` — reads the generated `Website/changelog/`, whose sidebar is built from the `## [version]`
  headings (each gets a `{#v…}` anchor).
- `api` — reads the committed `Website/api/`.

Generated and gitignored: `Website/tutorials/`, `Website/i18n/`, `Website/changelog/`, `Website/build/`,
`Website/docfx/projects/`. Never edit them by hand. Generated **and committed**: the root `README.md` and
`Website/api/`.

## Writing rules (so all three renderers agree)

- **No front matter.** Unity and GitHub would show it as text. Title comes from the first `# H1`, slug and
  order from the file name (`NN-` prefix orders, is stripped from the route).
- **One `# H1` per file.** Use `##` in the body. Exception: the introduction (`Documentation/README.md` and
  its translations) starts with the banner `<img>` and the status badges, without an H1. `parseFrontMatter`
  in `docusaurus.config.js` recognises that page by the banner's file name and supplies the title,
  description and `hide_title` — do not rename `aspid_fasttools_readme_banner.gif`.
- **Admonitions**: GitHub style only — `> [!NOTE]`, `TIP`, `IMPORTANT`, `WARNING`, `CAUTION`. Never `:::note`.
- **Links** are relative paths to the `.md` file: `[EnumValues](06-enum-values.md)`, from a sample
  `[Selector](../../../Documentation/03-serialize-reference-selector.md)`, from a doc
  `[Types sample](../Samples~/Types/Documentation/README.md)`. GitHub follows them as files; links that cross
  between plugin instances are rewritten to site routes by `Website/src/remark/crossInstanceLinks.js`.
  Never link by site URL.
- **Before/after comparisons**: a two-column table whose cells are `<pre lang="csharp">…</pre>` stays portable
  on GitHub and becomes real highlighted code blocks on the site (`src/remark/introBanner.js`).
- **Every `.md` and every image in the package needs a `.meta`** (`TextScriptImporter` for Markdown) — Unity
  would otherwise generate one in the consumer's project. Copy an existing one and give it a fresh GUID.
- The package is English. A translation is a sibling file: `Documentation/ru/06-enum-values.md`,
  `README.ru.md` next to `README.md`. Missing pages fall back to English. A translated file links translated
  targets (`../../Samples~/Types/Documentation/README.ru.md`) so GitHub stays in the same language; the site
  drops the locale segment itself.
- Adding a language: create `Documentation/<locale>/` and `*.<locale>.md` files, add `Website/translations/<locale>/`
  for the interface strings, and add the locale to `LOCALES` in `docusaurus.config.js`. Nothing else:
  `sync-i18n.mjs` discovers locale folders by name, and `LOCALES` is what keeps them out of the English
  `docs` instance.

### Images

- Main docs use `Documentation/Images/`; each sample keeps its own in `Samples~/<Sample>/Documentation/Images/`
  and references them as `Images/x.png`. A main doc may point at a sample image by path
  (`../Samples~/EnumValues/Documentation/Images/demo.gif`); `sync-i18n.mjs` mirrors those folders for i18n.
- **Scene footage and diagrams need a light-theme sibling**: `x.png` plus `x-light.png` in the same folder
  (`demo`/`scene` captures, SVG diagrams, gallery previews). `src/remark/themedImages.js` swaps them per theme.
  **Editor UI captures (Inspector, windows, pickers) do not** — they stay in the dark editor theme in both site
  themes; never report a missing `-light` for them.
- **Editor captures are framed automatically.** In `/docs` and `/tutorials` an image renders inside the
  window frame (`doc-image-panel`, `src/theme/MDXComponents/Img`). The exception is `demo`/`scene`
  (`.gif`/`.png`) on a *tutorial* page, which keeps the bare scene look; the same file on a doc page is framed.
  So name inspector captures anything but `demo`/`scene`, and name scene footage exactly that.
- `.sample-scene` (the background-recolouring filter) is applied by `themedImages.js` only to `demo`/`scene`
  files inside a **hardcoded list of sample folders** — a new sample must be added to that regex.
- A paragraph that repeats the image's alt text right below it becomes the caption (`doc-media-caption`).
- Click or Enter opens the image in a modal (Esc closes). Unframed images are capped at 640×520;
  framed and `.sample-scene` media fill the article.
- Status badges (`Images/status-badge-*.svg`) are links, not captures: they keep their size and do not zoom.

## Writing a feature page (docs/)

Rules the user confirmed while reworking `08-serialized-property-extensions.md` and `09-editor-helpers.md`; apply them
to every main doc page, always to the English file and its `ru/` twin together.

- **Lead paragraph = problem → solution → result, not a table of contents.** Two or three sentences a reader
  understands without knowing the API: the pain the feature removes (what the Unity way costs), what the feature
  does, ideally with the visible result (`FireAbility` → "Fire Ability", `FlockSimulation.Step (3)`), and what the
  reader gains. Every claim must be checkable against the code or Unity's behaviour — no "powerful", "easy",
  "seamless". No "for X, Y and Z" enumerations of sections, no abstract wording ("resolves the property back to its
  owner"). The ProfilerMarkers page is the reference.
- **One concrete example type per page**, declared in the quick start ("The examples on this page work with the
  `AbilityBook` component:") and reused by every section. Extend that type rather than inventing a second one.
- **Verify every claim against the source** (`Editor/Scripts/...`) before writing it; drop anything the code does
  not back (e.g. the "inherited attribute" note was removed from `GetDisplayName`).
- **Results go in tables**: property × method result tables and Unity-API-vs-FastTools before/after tables replace
  runs of small code blocks. Long method lists (setters) become a grouped table, not a comma list.
- **Say each fact once.** No repeat between a table's cell comments and the paragraph under it, and no repeat
  between quick start and a later section (`AndApply` is explained once).
- **Do not state what the context already implies** (no editor-only note under "in its custom `Editor`").
  Never stack two admonitions. A pitfall that silently loses data gets a `> [!WARNING]` (boxed struct copy).
- **Sample reference is minimal**: a closing `## Package sample` / `## Пример в пакете` with one sentence, the
  link to the sample README and, when the sample has one, its `demo.gif` with the caption paragraph — no
  "how to open" steps or experiments, those live on the sample's own page.

## Adding a main doc page

Drop `NN-name.md` into `Documentation/`, add its section to `Documentation/README.md` (and `ru/README.md`),
add the `.meta`, optionally the translation at `Documentation/ru/NN-name.md`, and add its id to the right
group in `Website/sidebars.js` (Serialization / Editor & tooling). Run `npm --prefix Website run sync-readme`
to refresh the root `README.md`.

## Adding a sample

1. `Samples~/<Name>/Documentation/README.md` (+ `README.ru.md`), with `.meta` files. Images go in that
   sample's `Documentation/Images/`; `demo`/`scene` captures get a `-light` sibling.
2. `Website/sidebarsTutorials.js`: add `{ type: 'doc', id: '<slug>/readme', label: '<Name>' }`.
3. `Website/src/components/SamplesGallery/index.js`: add an entry (id = slug, feature name, en/ru title and
   description) and put its preview at `Website/static/img/samples/<slug>.png` + `<slug>-light.png`.
4. If the sample ships `demo`/`scene` captures, add its folder to the sample regex in
   `Website/src/remark/themedImages.js`.
5. List it in the samples overview (`Samples~/README.md`, `README.ru.md`) and register it in the package
   `package.json` → `samples`.

## Local run / check

**Shared server (default).** The user works on the English and Russian versions at the same time, and other
agents work on the site in parallel, so everything is checked on **one shared production build** served on
port 3001 — never on per-agent dev servers:

```bash
Website/scripts/serve-all.sh          # kill the old server, `npm run build` (en + ru), serve detached on 3001
```

```bash
Website/scripts/serve-all.sh --stop
```

- English: `http://localhost:3001/Aspid.FastTools/`, Russian: `http://localhost:3001/Aspid.FastTools/ru/`.
- The server is detached (`nohup`, log in `Website/.serve-all.log`), so it outlives the session that started it
  and every agent and the user see the same site. Do not start it through `preview_start` — that ties it to one
  session.
- A static build does **not** pick up edits: after **every** change you want to verify (Markdown, config, remark
  plugins, CSS, sidebars), rerun `serve-all.sh` yourself and only then check in the browser. Never ask the user
  to restart it. The rebuild takes about a minute.
- If port 3001 is already answering when you start, another agent's build is up — rerun the script anyway after
  your edits; it replaces the server safely. Do not run `npm run build` or a dev server from `Website/` while the
  script is building (they share `.docusaurus/`, `build/` and `i18n/`).

Dev servers serve one locale at a time and are only for quick hot-reload iteration on a single page — they
don't reload config or remark plugins, and the user does not look at them: `npm start` / `npm run start:ru`, or
`website-dev` / `website-dev-ru` in `.claude/launch.json` (3100/3101). The `website-ru-3001` and
`website-serve-all` entries in that file both occupy port 3001 and would replace the shared build with a
session-bound server — do not launch them.

`onBrokenLinks` and `onBrokenMarkdownLinks` are `throw`: a bad relative link breaks the build on purpose
(`onBrokenAnchors` only warns — check the log for `#anchor` typos).

## Generated content

- `npm --prefix Website run sync-readme` regenerates the root `README.md` from `Documentation/README.md`,
  rebasing file links to the repository root. Never edit the root README by hand. `prestart`/`prebuild` refresh
  it automatically and CI runs `check-readme` before building to reject a stale copy.
- `Website/scripts/sync-i18n.mjs` (also run by `prestart`/`prebuild`) builds `Website/tutorials/`,
  `Website/changelog/` and `Website/i18n/` from the package: English sample READMEs and their images,
  `Documentation/<locale>/`, every sample-local `*.<locale>.md`, the root changelogs and
  `Website/translations/<locale>/`. Scripts, scenes and `.meta` files are never copied. Because the copies are
  untracked, each page's "Last updated" date is stamped from the **source file's last commit** — an
  uncommitted page shows no date.

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
```

```bash
cd Website && npm run api               # regenerate after public API or XML doc changes
```

`npm run api` deletes `Website/api/` and runs three steps (`Website/scripts/docfx-*.mjs`,
`Website/docfx/docfx.json`) — if DocFX fails midway, the directory stays empty and the site build breaks, so
regenerate or `git checkout Website/api` before building:

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

`Website/sidebarsApi.js` adapts the generated sidebar for display (drops the repeated `Aspid.FastTools.`
prefix, folds the `SetLabel` overloads). Never edit files in `Website/api/` by hand; fix the XML comment or the
postprocess script and regenerate. Translations are not generated; the `ru` locale falls back to the English
pages. The Math satellite assembly is not documented — it compiles only when `com.unity.mathematics` is
installed, which this project does not.

## Design

The theme is shared with Aspid.MVVM: dark graphite with the Unity badge green as accent (`--venom-*` tokens in
`Website/src/css/custom.css`), IBM Plex Serif/Mono from Google Fonts, iA Writer Quattro body self-hosted in
`src/fonts/` (OFL, keep the licence file), Ayu-based Prism themes in `src/prism/venom.js`.

### Introduction feature cards

There is no landing page: `/` redirects to `/docs`. On the introduction, `src/remark/introBanner.js` turns the README's
Features section into card grids. A card whose page has an entry in `src/components/FeaturePreview` (EnumValues and
the Editor & tooling features) shows that animated preview instead of the README capture; its code is hand-written
there, so update it when the page's quick start changes. The EnumValues clip is `FeaturePreview/media/enum.mp4`.
The Inspector GIFs of the first two cards are cropped in `custom.css` to hide the baked title bar and tab strip.

The same plugin replaces the Installation section's instruction, URL block and version note with
`src/components/InstallPanel`: a Package Manager walk-through beside the steps, and the URL to copy with a toggle
between the latest preview and `#upm-preview/<packageVersion>` (`customFields.packageVersion` from `package.json`).
Its text is written in the component per locale, so update it when the README's install steps change.

`static/img/logo.png` and `favicon.png` are copies of the package icon
`Editor/Resources/Icons/aspid_icon_medium_green_256x253.png`; re-copy them if the icon changes.

The page uses normal document scrolling with sticky navigation. The borderless article has an opaque reading
surface (graphite in dark mode, warm linen in light mode). The fixed dot texture is painted on `html`, not the
viewport-height `body`, so it remains visible in the margins throughout long articles. Both navigation columns
share `--venom-navigation-width` (260px). Below 1400px the right TOC becomes an in-article disclosure, and
below 997px the navigation uses Docusaurus' mobile menu. On desktop (≥997px) the navbar is hidden and
`src/theme/DocSidebar/Desktop` wraps the sidebar into a full-height panel: pinned header (brand, section
switcher built from the navbar's left items, search), scrolling document list, pinned footer (GitHub,
language, theme).

`src/plugins/search` builds a locale-specific index from Docusaurus' resolved document sources and permalinks.
`src/theme/SearchBar` loads it on demand, searches Docs/Samples/API/Changelog, and supports Cmd/Ctrl+K, arrow
keys, Enter and Esc. `node --test scripts/search.test.mjs` checks matching and Markdown extraction.

## Deploy

`.github/workflows/docs.yml` builds on every push to `main` touching `Website/`, the package `Documentation/`,
a sample's `Documentation/`, the root `README.md` or `CHANGELOG*.md`, and on PRs (build only); it runs
`check-readme` before the build. Pages source must be set to "GitHub Actions" once in the repository settings.

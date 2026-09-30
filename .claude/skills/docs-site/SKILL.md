---
name: docs-site
description: How Aspid.FastTools documentation is authored and published — Markdown inside the UPM package (`Documentation/`, `Documentation/ru/`, each sample's `Documentation/`) plus the root `CHANGELOG*.md`, read by GitHub, Unity and the Docusaurus site in `Website/`, deployed to GitHub Pages. Use when adding or editing any documentation page, translation, sample README, image, or the site itself.
user-invocable: false
metadata:
  internal: true
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
- **Highlighted inline code**: `<code lang="csharp">void Run&lt;T&gt;()</code>` is plain inline code on GitHub and is
  highlighted on the site (`introBanner.js` → `src/components/InlineCode`; sample pages get it from `remarkInlineCode`
  in the `tutorials` instance). Use it for every inline C# snippet, in prose
  and in tables — never for paths, flags, diagnostic IDs (`AFT0010`) or Profiler column names (`Calls`) — and escape
  `<`, `>`, `{`, `}` as in `<pre>` cells.
  `<code lang="string">`, `<code lang="class-name">` and `<code lang="function">` paint the text in that token's
  colour: Profiler marker names in a result column, a lone type (`T`; in `System.Type` only `Type`; in `List<Weapon>`
  both names, brackets plain), a bare method name (`Update`; in `styleSheets.Add` only `Add`). A namespace stays in the
  normal text colour, in code blocks too (`using Aspid.FastTools.Types;`, the `System.` of `System.Type`): write it as
  `<code lang="csharp">UnityEngine.Scripting</code>`. Prism leaves a lone generic type or `System.Type` in `csharp`
  uncoloured, so use `class-name` for them; a qualified method call reads best as `csharp` with its parentheses
  (`Type.GetType()`). Fields, properties and enum values stay plain, as in code blocks.
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
- **Every image needs a light-theme sibling**: `x.png` plus `x-light.png` in the same folder, with the same
  pixel size and, for a GIF, the same timeline. `src/remark/themedImages.js` swaps them per theme (it runs through
  the webpack cache: after adding a sibling to an unchanged page, `npx docusaurus clear` before the build). Editor
  UI captures (Inspector, windows, pickers) are re-shot in Unity's light editor skin with the same steps; the
  FastTools window (Project/Asset References) switches to its light palette there too. Status badges are links,
  not captures, and need none.
- **Editor captures have no frame around them.** In `/docs` and `/tutorials` an image renders in a plain
  full-width wrapper (`doc-image-panel`, `src/theme/MDXComponents/Img`): no window, padding or shadow, the
  capture's own 1px edge is the only frame. The exception is `demo`/`scene` (`.gif`/`.png`) on a *tutorial* page,
  which keeps the bare scene look. So name inspector captures anything but `demo`/`scene`, and name scene footage
  exactly that. A scene sample's `demo`/`scene` linked from a doc page gets `.scene-footage` (recoloured background).
- Which samples are scenes is the **hardcoded `SCENE_SAMPLES` list** in `themedImages.js` (folder names under
  `Samples~/`). It drives both `.sample-scene` (the background-recolouring filter on the sample's tutorial page)
  and `.scene-footage`; a sample missing from it just keeps its own background. Nothing fails the build.
- A paragraph that repeats the image's alt text right below it becomes the caption (`doc-media-caption`).
- Click or Enter opens the image in a modal (Esc closes). Bare images are capped at 640×520;
  captures in `doc-image-panel` and `.sample-scene` media fill the article.
- Status badges (`Images/status-badge-*.svg`) are links, not captures: they keep their size and do not zoom.
- A static picture can have a live site version: `src/remark/liveDiagrams.js` maps the file name to a component
  (`profiler-markers-hierarchy.svg` → `ProfilerHierarchy`). Markdown
  keeps the picture for GitHub and Unity; the caption paragraph must still repeat the alt text as plain text.
- A table can have a live site version the same way: `TABLES` in `liveDiagrams.js` maps its first body cell to a
  component (`SetPadding(8)` → `StyleSides`, the Styles table of VisualElement Extensions), which gets every row as
  `<StyleSidesRow call="…">` with the second cell as children. Markdown keeps the table, and the rows and their
  translations stay in the page; changing that first cell detaches the component.
- An ordered list can have one too: `LISTS` in `liveDiagrams.js` maps the bold text of its first item to a component
  (`Tools → Aspid 🐍 → FastTools → Project References` → `ProjectReferencesPanel`, the SerializeReference Tooling quick
  start), which gets each item's text as a `<span>` child, so the steps and their translation stay in the page; changing
  that bold text detaches it. The panel reuses the install panel's card and steps and draws the Aspid FastTools window
  without its tab strip; in the light site theme it takes Unity's light skin and the window's light palette
  (`Aspid-FastTools-Default-Light.uss`), so update both when the package palette changes.

## Writing a feature page (docs/)

Rules the user confirmed while reworking `05-profiler-markers.md`, `07-visual-element-extensions.md`,
`08-serialized-property-extensions.md` and `09-editor-helpers.md`; apply them
to every main doc page, always to the English file and its `ru/` twin together.

- **Lead = one short sentence that makes the reader interested**, not an explanation. No API names, code,
  `using`, name formats or mechanics — the quick start shows those right below. Plain wording a reader understands
  without knowing the package; the claim must still hold (no "powerful", "easy", "seamless"), and no "for X, Y and Z"
  enumerations of sections. References: the Introduction («Aspid.FastTools — пакет для Unity, который убирает рутину
  из сериализации, профилирования и редакторского кода.») and ProfilerMarkers («Маркеры профилировщика одной строкой,
  без полей и имён, которые приходится поддерживать вручную.»).
- **One concrete example type per page**, reused by every section; extend that type rather than inventing a second
  one. Do not announce it with a sentence ("The examples on this page work with…") — the before/after table
  already shows it, and the link to the sample lives only in the closing `## Package sample`.
- **Quick start is the before/after table**, plus at most one sentence on what the calls return. No `using` line, no
  type declaration, no "in your custom `Editor`, add…". A declaration the results cannot be read without goes into the
  section that needs it (SerializedProperty Extensions: `AbilityBook` sits above the reflection table).
- **A plain rename is a two-column `Unity | FastTools` table of inline code** (`AddToClassList` → `AddClass`). Keep
  `<pre>` before/after cells for calls where FastTools removes code (`AddChildIf`, a hex colour, `TryGetByEnum`).
- **Verify every claim against the source** (`Editor/Scripts/...`) before writing it; drop anything the code does
  not back (e.g. the "inherited attribute" note was removed from `GetDisplayName`).
- **Results go in tables**: property × method result tables and Unity-API-vs-FastTools before/after tables replace
  runs of small code blocks. Long method lists (setters) become a grouped table, not a comma list.
- **Say each fact once.** No repeat between a table's cell comments and the paragraph under it, and no repeat
  between quick start and a later section (`AndApply` is explained once).
- **Do not state what the context already implies** (no editor-only note under "in its custom `Editor`",
  no Assembly Definition reference note), do not list what is *not* required ("no attributes or `partial`") — a
  requirement would be stated — and do not state expected behaviour ("keeps the order", "finds private fields too",
  "call it again — the string does not update", "timings are illustrative").
- **Unity's own behaviour stays out**, even when it explains a FastTools detail: version limits of Unity types
  ("Unity 6.2+"), Unity applying a write with Undo itself, how long an inspector's `SerializedObject` lives, ordinary
  `SerializedObject` patterns (several writes before one apply).
- **Admonitions:** `> [!NOTE]` for a non-obvious mismatch that loses nothing (`HasFoldout()` vs the Inspector);
  `> [!WARNING]` only when data or measurements are lost silently (boxed struct copy, `partial` calls on one line).
  A mistake an analyzer reports is a plain bullet with its ID (`AFT0010`, `AFT0011`), not a warning. Never stack two.
- **Headings and labels name what the reader gets**: «Поле C# за свойством», not «Тип поля и объект-владелец»; a table
  row names the value type («Идентификаторы объектов»), never a constraint («Unity 6.2 и новее»).
- **Sample reference is minimal**: a closing `## Package sample` / `## Пример в пакете` with one sentence, the
  link to the sample README and, when the sample's `demo.gif` shows this page's feature, that gif with the caption
  paragraph — no "how to open" steps or experiments, those live on the sample's own page. Footage of a sample
  several pages share (EditorTools) stays on the sample's page unless it shows what the text cannot (it does
  for VisualElement Extensions, not for SerializedProperty Extensions or Editor Helpers). The sentence must match what the
  sample code really does — check the sample scripts, and fix its README (en + ru) when it disagrees; never promise
  more than the scene has («Все маркеры с этой страницы…» was wrong). A caption must be about this page's feature —
  otherwise drop it and keep a neutral alt text (the shared EditorTools gif on VisualElement Extensions).
- **The Introduction (`Documentation/README.md`) is the ideal** for tone, density and visuals; ProfilerMarkers and
  Editor Helpers were reworked from it. Only FastTools-specific behaviour: never explain Unity or UI Toolkit.
- **Check Unity's behaviour by decompiling, not from memory**:
  `~/.dotnet/tools/ilspycmd -t UnityEditor.ObjectNames /Applications/Unity/Hub/Editor/6000.0.64f1/Unity.app/Contents/Managed/UnityEngine/UnityEditor.CoreModule.dll`.
- **Code blocks fit the article width** without horizontal scrolling. Site table columns are equal and fixed, so
  long code in a cell breaks mid-word: keep cells short, move a long attribute into the column header.
- **No caption under a capture on a feature page**: the section text already says what it shows, so the image keeps
  only its alt text (Serializable Type System, ProfilerMarkers). The caption paragraph stays for live diagrams, tutorial
  pages and the `## Package sample` gif (see below).
- **A picture must show something the text does not.** A capture that repeats the lead or a table goes. Diagrams
  and previews follow the Introduction's feature cards: site tokens, one frame, no shadow, no frame in a frame.
- Text stays left-aligned (never justified) and fills the article width.
- A bug found in package code while writing docs is not fixed on the docs branch: report it and offer a separate
  task in its own worktree.

## Writing a sample page (tutorials/)

Rules the user confirmed while reworking the five sample READMEs; the feature-page rules above apply too.

- **Order:** `# <Name> Sample`, the lead, `demo.gif` with its caption, `## Open it`, `## Try`, optional sections for a
  second scenario (SerializeReferences: Repair, IMGUI inspector), `## Where to look`, then one closing line
  `Reference: [Feature](…)` (ru `Справочник — […](…)`). No code block or capture above the demo: an Inspector capture
  goes into the step that uses it.
- **Lead = the sample's card description** in `SamplesGallery/index.js`, word for word in both places; change them
  together.
- **Import step, same on every page:** «Import the sample: **Tools → Aspid 🐍 → FastTools → Welcome** → **Samples** →
  **Import** on **X**.» / «Импортируйте пример: … → **Import** у **X**.» The next step opens the scene and enters Play
  Mode with one sentence on what the reader sees, no mechanics.
- **No recording notes** (Sample Themes, the Ability Catalog Theme menu): they are for our captures, and the Light
  preview swaps the EnumValues palette, so edits to the asset stop showing.
- **Try steps run in Edit Mode** unless a step says «enter Play Mode»: open `## Try` with «Exit Play Mode and select
  **X**.» A step must be visible in the sample as shipped (change a value first when the scene holds the default);
  anything read from saved files (Scan Project, CI) says «Save the scene».
- **Code is the sample's own**: quote real lines (comments may go), never an invented call attributed to a sample
  file. A result that only a hypothetical call shows goes into a results table instead.
- **Inspector labels are bold** (**Enemy Type**, **Mana Cost**), class names `class-name`, marker names `string`,
  as on feature pages; picker in ru is «окно выбора».
- **Say it once:** a fact documented on the feature page (the header menu, the custom-inspector API) is a link to that
  section, not a copy. `## Where to look` lists only files that show FastTools.

## Adding a main doc page

Drop `NN-name.md` into `Documentation/`, add its section to `Documentation/README.md` (and `ru/README.md`),
add the `.meta`, optionally the translation at `Documentation/ru/NN-name.md`, and add its id to the right
group in `Website/sidebars.js` (Serialization / Editor & tooling). Run `npm --prefix Website run sync-readme`
to refresh the root `README.md`.

## Adding a sample

1. `Samples~/<Name>/Documentation/README.md` (+ `README.ru.md`), with `.meta` files. Images go in that
   sample's `Documentation/Images/`; every image gets a `-light` sibling.
2. `Website/sidebarsTutorials.js`: add `{ type: 'doc', id: '<slug>/readme', label: '<Name>' }`.
3. `Website/src/components/SamplesGallery/index.js`: add an entry (id = slug, feature name, en/ru title and
   description) and put its preview at `Website/static/img/samples/<slug>.png` + `<slug>-light.png` (one 16:9 size
   for both, no scene titles; a scene may add a looping clip — `docs/media/samples-gallery/README.md`).
4. If the sample's `demo`/`scene` captures show a scene (not an editor window), add its `Samples~/` folder name
   to `SCENE_SAMPLES` in `Website/src/remark/themedImages.js`.
5. List it in the samples overview (`Samples~/README.md`, `README.ru.md`) and register it in the package
   `package.json` → `samples`.

## Local run / check

**One production build per checkout (default).** The user checks English and Russian together, on a production
build, and other agents work on the site in parallel in their own worktrees. So every checkout serves **its own**
build, and nobody replaces anybody else's:

```bash
Website/scripts/serve-all.sh          # replace this checkout's server, `npm run build` (en + ru), serve detached
```

```bash
Website/scripts/serve-all.sh --stop   # stop this checkout's server only
```

- The main checkout serves on **3001**. A linked worktree gets its own port in 3200–3999, derived from its path (the
  next free one if taken), so it stays the same across rebuilds; the script prints the URLs and writes the port to
  `Website/.serve-all.port`. Give the user the printed English and Russian links.
- In a worktree the script links `Website/node_modules` to the main checkout's on first run — no `npm ci` needed.
- The server is detached (`nohup`, log in `Website/.serve-all.log`) and bound to localhost. Do not start it through
  `preview_start` — that ties it to one session. Each run also stops servers whose checkout was deleted
  (including worktrees the app moved to `.ccd-trash`), so abandoned builds do not pile up.
- A static build does **not** pick up edits: after **every** change you want to verify (Markdown, config, remark
  plugins, CSS, sidebars), rerun `serve-all.sh` yourself and only then check in the browser. Never ask the user
  to restart it. The rebuild takes about a minute.
- Do not run `npm run build` or a dev server from the same `Website/` while the script is building (they share
  `.docusaurus/`, `build/` and `i18n/`). Other checkouts are unaffected.
- Open the page with a fresh query (`?v=N`) after a rebuild: the browser otherwise shows the cached version.

Dev servers serve one locale at a time and are only for quick hot-reload iteration on a single page — they
don't reload config or remark plugins, and the user does not look at them: `website-dev` / `website-dev-ru` in
`.claude/launch.json`, started with `preview_start`. They use `autoPort`, so dev servers of different worktrees get
different ports. There is no launch entry for the production build on purpose: only `serve-all.sh` starts it.
Both run `npm run start` / `start:ru`, so `prestart` runs `sync-i18n` first: it creates `Website/tutorials/`,
`changelog/` and `i18n/`, without which a fresh worktree's `docusaurus start` fails. `Documentation/ru/**`, sample
READMEs and the changelogs reach the site as copies: after editing them run `npm --prefix Website run sync-i18n`
for a running dev server to see the change.

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
   groups). A type name that appears in two namespaces (`VisualElementExtensions` in `UIElements` and
   `UIElements.Editors`) gets a namespace suffix in its label — Docusaurus derives one translation key per label
   and the `ru` build fails on duplicates. Unity links point at the Scripting Reference of the Editor in
   `Aspid.FastTools/ProjectSettings/ProjectVersion.txt` (members move between versions);
   `node --test scripts/unity-script-reference.test.mjs` checks the URL builder.

`Website/sidebarsApi.js` adapts the generated sidebar for display (drops the repeated `Aspid.FastTools.`
prefix, folds the `SetLabel` overloads). Never edit files in `Website/api/` by hand; fix the XML comment or the
postprocess script and regenerate. Translations are not generated; the `ru` locale falls back to the English
pages. The Math satellite assembly (`Aspid.FastTools.VisualElements.Math`, `INotifyValueChangedMathExtensions`) is
not in `/api` only because `ASSEMBLIES` in `docfx-projects.mjs` does not list it; it compiles in this project
(`com.unity.mathematics` comes in transitively), so documenting it means adding it there.

## Design

The theme is shared with Aspid.MVVM: dark graphite with the Unity badge green as accent (`--venom-*` tokens in
`Website/src/css/custom.css`). Green is the default; the reader can switch the accent to red, blue, yellow or mono in the
sidebar footer appearance menu, next to the theme (`NavigationPanel/AppearanceSwitcher.js`). The variants live in `src/css/accents.css` under
`html[data-accent]`, the list and the pre-paint boot script in `src/accents.js`. Colour things with `--venom-accent*`
/ `--ifm-color-primary*`, never a literal green, unless it mimics Unity or means success (`--venom-emerald`).
Fonts: IBM Plex Serif (headings), Plex Sans (text) and Plex Mono (code) from Google Fonts; iA Writer Quattro, the
samples' editor font, is self-hosted in `src/fonts/` (OFL, keep the licence file) for navigation labels only — sidebar
and TOC, `--venom-font-family-nav`. Prism themes are Ayu-based, in `src/prism/venom.js`, with a transparent background.

### Introduction feature cards

There is no landing page: `/` redirects to `/docs`. The README banner GIF (6 MB) stays for GitHub only: the plugin
renders it as `src/components/IntroBanner`, which plays `media/banner.mp4` with the poster `media/banner-poster.jpg`.
Both are encoded from the GIF — re-encode them when it changes (`ffmpeg -i <gif> -movflags +faststart -c:v libx264
-pix_fmt yuv420p -preset veryslow -crf 22 -tune animation -an -vf fps=25 banner.mp4`, poster: `-frames:v 1 -q:v 4`).
On the introduction, `src/remark/introBanner.js` also turns the README's
Features section into card grids. A card whose page has an entry in `src/components/FeaturePreview` (EnumValues and
the Editor & tooling features) shows that animated preview instead of the README capture; its code is hand-written
there, so update it when the page's quick start changes. The EnumValues clip is `FeaturePreview/media/enum.mp4`, with `enum-light.mp4` for the light theme. Both are the
sample's `enum-values-multipliers-populate(-light).gif` padded on the right and bottom only, since the GIF's left margin is
already 16px wider (`-preset veryslow -crf 24 -vf "fps=20,pad=1576:1080:0:188:color=0x333333"`, `0xC8C8C8` for light).
The Inspector GIFs of the first two cards are cropped in `custom.css` to hide the baked title bar and tab strip.

The same plugin replaces the Installation section's instruction, URL block and version note with
`src/components/InstallPanel`: a Package Manager walk-through beside the steps, and the URL to copy with a toggle
between the latest version and `<url>/<packageVersion>` (`customFields.packageVersion` from `package.json`). The
README's URL carries the channel — `#upm-preview` for a prerelease, `#upm` for a stable version — and
`scripts/set-version.sh` switches it together with the badge label. The panel pins from the branch its own URL names,
out of `customFields.packageVersions` (the tags of both branches, per branch); `UPM_BRANCH` in `docusaurus.config.js`
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
draw code or marks (`Documentation/Images/*-light.svg`) use the same values; update them when the palette changes.

`src/plugins/search` builds a locale-specific index from Docusaurus' resolved document sources and permalinks.
`src/theme/SearchBar` loads it on demand, searches Docs/Samples/API/Changelog, and supports Cmd/Ctrl+K, arrow
keys, Enter and Esc. `node --test scripts/*.test.mjs` in `Website/` (`npm test`) checks matching, Markdown extraction and index URLs.

`src/components/DotRipple` draws the click ripples on the dot background; the number of waves is unlimited, and
`waves.js` culls, per grid row, the waves whose ring misses it. `node --test scripts/dot-ripple.test.mjs` checks
that no dot of a ring is dropped.

## Deploy

`.github/workflows/docs.yml` builds on every push to `main` touching `Website/`, the package `Documentation/`,
a sample's `Documentation/`, the root `README.md` or `CHANGELOG*.md`, and on PRs (build only); it runs
`check-readme` and the site tests (`node --test scripts/*.test.mjs`) before the build. Pages source must be set to "GitHub Actions" once in the repository settings.

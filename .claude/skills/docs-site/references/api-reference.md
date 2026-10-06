# API reference (`/api`)

The **API** tab is generated from the XML doc comments by DocFX, not written by hand. `Website/api/` holds the
generated Markdown plus `sidebar.js` and **is committed**: DocFX compiles the assemblies against the local Unity
installation, which CI does not have.

```bash
dotnet tool install -g docfx            # once; the tool lands in ~/.dotnet/tools — make sure it is on PATH
```

```bash
cd Website && npm run api               # regenerate after public API or XML doc changes
```

A worktree has no Unity-generated `.csproj`, no `Library/` and no `Website/node_modules`. Take them from the main
checkout before `npm run api`. Run these from the worktree root (macOS):

1. Clone the Library: `cp -c -R <main>/Aspid.FastTools/Library Aspid.FastTools/Library`. On APFS, `-c` makes a
   clone in seconds; GNU `cp` reads `-c` differently.
2. Copy `Aspid.FastTools.csproj` and `Aspid.FastTools.Editor.csproj` into `Aspid.FastTools/`. Replace the main
   checkout path in them with the worktree path, so the analyzers come from the worktree.
3. Link `Website/node_modules` to the main checkout's folder. Remove the link before you stage anything:
   `Website/.gitignore` ignores a `node_modules/` directory, not a symlink.

The cloned `Library/ScriptAssemblies` holds DLLs built from the main checkout. A PR that changes an asmdef outside
the documented assemblies (for example the Math satellite) needs a Unity compile of the worktree first.

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

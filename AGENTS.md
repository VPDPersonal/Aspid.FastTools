# Aspid.FastTools

Unity package `tech.aspid.fasttools` (`Aspid.FastTools/Packages/tech.aspid.fasttools/`) plus two standalone .NET solutions,
`Aspid.FastTools.Generators/` and `Aspid.FastTools.Analyzers/`, whose DLLs are committed into the package, and
Agent Skills for projects that consume the package in `skills/`.

## Never

- Use Unity APIs newer than **6000.0.53f1** without a `#if UNITY_6000_x_OR_NEWER` guard — the dev project runs
  6000.4, but `package.json` promises 6000.0.53f1. Unity has no define for a patch: an API from a later 6000.0 patch
  needs a higher `unityRelease`.
- Write to `Console` from generator or analyzer code, or reference `SourceGenerator.Foundations` —
  it deadlocks Unity's compilation server.

## Not obvious

- A change to generator or analyzer source reaches Unity **only** after `dotnet build -c Release` in that solution;
  `dotnet test` (Debug) deliberately does not copy the DLL, so it is safe to run.
- `Aspid.FastTools.YamlTests/` runs the package's SerializeReference YAML engine and its tests outside Unity: it compiles
  those package sources as-is (C# 9, Unity 6000.0's version) and stubs only `Debug.LogError` and
  `AssetDatabase.MakeEditable`, so a Unity API or a newer language feature added to that engine breaks this project
  first; tests that need the Editor are excluded in its csproj and run in the Unity job.
- The version lives in `package.json`, the badge SVG and the badge alt text, release link and install URLs of both
  READMEs, and the version's section of both CHANGELOGs; bump all of them with `scripts/set-version.sh <version>`,
  which the release workflow checks (`scripts/check-version.mjs`). The version also picks the channel, and the script
  switches it: a prerelease installs from `#upm-preview` under a Preview badge, a stable version from `#upm` under a
  Release one; the site derives `UPM_BRANCH` from the version. `/asp-ft-release <version>` runs the whole release.
- `skills/` is for **consumers** of the package and is installed with `npx skills add VPDPersonal/Aspid.FastTools`;
  skills for working on this repo live in `.claude/skills/` and carry `metadata.internal: true` so that command does
  not offer them; `scripts/check-skills.mjs` (CI) checks both. A skill that describes public API is updated in the same PR
  as that API.
- A change to public API or to its XML docs reaches `Website/api/` **only** after `npm run api` in `Website/`; commit the
  result in the same PR. CI cannot run it, because DocFX compiles against a local Unity install. The steps for a worktree
  are in `.claude/skills/docs-site/references/api-reference.md`.
- `Aspid.FastTools/Packages/com.unity.asset-store-tools/` is Unity's Asset Store Tools (the package validator and uploader),
  embedded in the dev project. It is third-party code with CRLF line endings, so do not edit, reformat or delete it. It
  stays outside `Aspid.FastTools/Packages/tech.aspid.fasttools/`, so the `upm` branches do not contain it.
- The dev project uses the legacy Input Manager on purpose (`activeInputHandler: 0`, no `com.unity.inputsystem`).
  `com.unity.pipeline` compiles its input commands only when `ENABLE_INPUT_SYSTEM` is defined, and then it needs the
  package. The CLI input commands (`simulate_key` and the pointer commands) report as unavailable.

## Checks

Run what applies before you open a PR. CI runs most of these (`tests.yml`, `docs.yml`, `skills.yml`).

- Generators and analyzers: `dotnet test` in `Aspid.FastTools.Generators/` or `Aspid.FastTools.Analyzers/`.
- YAML engine: `dotnet test Aspid.FastTools.YamlTests`.
- Compile in the dev project with its Editor open: `unity command recompile`, then `unity command get_console_logs`
  for the errors. It needs `com.unity.pipeline`, which the dev project manifest lists.
- EditMode tests on any Unity version: `scripts/make-unity-test-project.sh <dir> [unity-version]` builds a throwaway
  project, and its header shows the `-runTests` command. CI runs five versions.
- Site, in `Website/`: `npm ci` first (a fresh worktree has no `node_modules`), then `npm run check-readme`,
  `npm run check-translations`, `npm test` and `npm run build`.
- Skills: `npm install --no-save js-yaml@4 && node scripts/check-skills.mjs`.

## C# style beyond `.editorconfig`

- Named arguments, even for a single obvious argument: `Type.GetType(_aqn, throwOnError: false)`.
- `using` directives sorted by ascending line length, not alphabetically; `System` is not forced first.
- Leaf classes are `sealed`; leave one open only when it is designed for inheritance. `internal` for implementation,
  `public` for API.
- Public API of the package carries XML docs (`<summary>`, `<param>`, `<returns>`, `<see cref/>`): the site's API
  reference is generated from them.

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
  `dotnet test` (Debug) deliberately does not copy the DLL, so it is safe to run. Commit the rebuilt DLL:
  `ci.yml` fails when a committed DLL differs from a Release build of its sources. `PackagedAnalyzerTests` read the
  committed DLLs, so after a change to a generator or an analyzer run the Release build first: only then do they judge
  the new code.
- `Aspid.FastTools.YamlTests/` runs the package's SerializeReference YAML engine and its tests outside Unity: it compiles
  those package sources as-is (C# 9, Unity 6000.0's version; the engine against .NET Standard 2.1) and stubs only
  `Debug.LogError` and `AssetDatabase.MakeEditable`, so a Unity API, a newer language feature or a newer .NET API added
  to that engine breaks this project first. `Aspid.FastTools.TypeTests/` compiles `GenericTypeResolver`,
  `GenericArgumentFilter` and `TypeUtility` with the resolver tests in the same way. It stubs only `UnityEngine.Object`
  and `CompilationPipeline.GetAssemblies`, so a new Unity API in these three files breaks it. A test that needs the
  Editor goes into its fixture's `*.Unity.cs` part, which both projects leave out; the Unity job runs it.
- `Tests/Editor/PublicApi/PublicApi.txt` in the package is a snapshot of the public API, and `PublicApiSnapshotTests`
  fails on any difference, so a removed or renamed member cannot reach a release unnoticed. After an intended API
  change, rewrite the file on the Unity version that `WholeApiVersion` in the test names, or a newer one, and commit
  its diff in the same PR. Close the project in the Editor first, or run the command from a worktree:
  `ASPID_FASTTOOLS_WRITE_PUBLIC_API=1 <Unity> -batchmode -nographics -projectPath Aspid.FastTools -runTests -testPlatform EditMode -testFilter PublicApiSnapshotTests -testResults "${TMPDIR:-/tmp}/public-api.xml"`.
  An older Editor compiles only part of the API (the `UNITY_6000_x_OR_NEWER` guards), so there the test checks only
  for additions. The `#if` around `HasWholeApi` in the test names the newest guard: move it and `WholeApiVersion`
  when a public member gets a newer one.
- `Tests/Burst` (Burst jobs that call `this.Marker()`) compiles only where `com.unity.burst` is installed
  (`versionDefines`): the dev project has it through URP, the project from `scripts/make-unity-test-project.sh`
  does not, so CI skips it.
- The version lives in `package.json`, the badge SVG and the badge alt text, release link and install URLs of both
  READMEs, and the version's section of both CHANGELOGs; bump all of them with `scripts/set-version.sh <version>`,
  which `ci.yml` checks (`scripts/check-version.mjs`) in every PR and in the release. The version also picks the channel,
  and the script switches it: a prerelease installs from `#upm-preview` under a Preview badge, a stable version from
  `#upm` under a Release one; the site derives `UPM_BRANCH` from the version. `/asp-ft-release <version>` runs the whole
  release.
- `skills/` is for **consumers** of the package and is installed with `npx skills add VPDPersonal/Aspid.FastTools`;
  skills for working on this repo live in `.claude/skills/` and carry `metadata.internal: true` so that command does
  not offer them. `scripts/check-skills.mjs` (CI, on every PR) checks every `SKILL.md` in the repository: a file outside
  `skills/` must be internal, because the installer scans about 30 agent folders, an `AFT` code in a skill must be
  reported by an analyzer, and a namespace or assembly in a consumer skill must exist in the package. A skill that
  describes public API is updated in the same PR as that API.
- `scripts/check-package-files.mjs` (CI) fails on a package path longer than 123 characters (the Asset Store Validator
  limit is 140, counted from `Aspid/FastTools/`), on a `.cs.meta` outside `Samples~` without a `MonoImporter` block, and
  on a `.uss.meta` with importer id 12388. Unity writes the full block into every new `.cs.meta`, and a reserialize adds
  it to old ones, so the check keeps all of them in one format. Unity rewrites id 12388 to 12385 when it imports a sample.
  It also fails on a file or folder without a `.meta` (`Samples~` content too), a `.meta` without an asset, a GUID used
  twice, a sample path of `package.json` that is no folder, and an asmdef reference that is no asmdef of the package.
  Unity repairs these in a local package and ignores or breaks them in an installed one. An asmdef that needs an assembly
  from another package lists it in `EXTERNAL_ASSEMBLIES` of the script.
- The minimum Unity version is `unity` and `unityRelease` in `package.json`. It is also written by hand in this file,
  `.github/claude-review.md`, `.github/ISSUE_TEMPLATE/release_checklist.yml`, `skills/aspid-visual-element-fluent/SKILL.md`, the package `SETUP.md`
  and the first Unity version of the matrix in `.github/workflows/ci.yml`; `scripts/check-unity-minimum.mjs` (CI) fails
  when one differs.
- `.github/workflows/ci.yml` holds every required check except the PR title and CHANGELOG (`pr-checks.yml`), and
  `release.yml` runs it on the tagged commit. It has no path filter, so each of its jobs can be a required check. It runs
  `check-package-files.mjs`, `check-version.mjs`, `check-unity-minimum.mjs`, `check-skills.mjs` and
  `check-contrast.mjs`, compares the committed Roslyn DLLs with a Release build, runs `node --test scripts/*.test.mjs`,
  the .NET tests and the Unity EditMode tests.
  The script tests run `set-version.sh` on a copy of the repository and need `npm --prefix Website ci`.
- `.claude/settings.json` is also loaded by the Claude agent in `claude.yml`, which keeps `GITHUB_TOKEN` in its
  environment. Allow there only commands that cannot run code or read the environment; put the rest in
  `.claude/settings.local.json`.
- A change to public API or to its XML docs reaches `Website/api/` **only** after `npm run api` in `Website/`; commit the
  result in the same PR. CI cannot run it, because DocFX compiles against a local Unity install. The steps for a worktree
  are in `.claude/skills/docs-site/references/api-reference.md`.
- `Aspid.FastTools/Packages/com.unity.asset-store-tools/` is Unity's Asset Store Tools (the package validator and uploader),
  embedded in the dev project. It is third-party code with CRLF line endings, so do not edit, reformat or delete it. It
  stays outside `Aspid.FastTools/Packages/tech.aspid.fasttools/`, so the `upm` branches do not contain it.
- The dev project uses only the Input System (`activeInputHandler: 1`, `com.unity.inputsystem`): `UnityEngine.Input`
  throws there. `com.unity.pipeline` needs the package for its CLI input commands (`simulate_key`, pointer commands).

## Checks

Run what applies before you open a PR. CI runs most of these (`ci.yml`, `docs.yml`).

- Generators and analyzers: `dotnet test` in `Aspid.FastTools.Generators/` or `Aspid.FastTools.Analyzers/`.
- YAML engine: `dotnet test Aspid.FastTools.YamlTests`.
- Compile in the dev project with its Editor open: `unity command recompile`, then `unity command get_console_logs`
  for the errors. It needs `com.unity.pipeline`, which the dev project manifest lists.
- EditMode tests on any Unity version: `scripts/make-unity-test-project.sh <dir> [unity-version]` builds a throwaway
  project, and its header shows the `-runTests` command. CI runs every version of its matrix.
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

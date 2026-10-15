# Aspid.FastTools

Unity package `tech.aspid.fasttools` (`Aspid.FastTools/Packages/tech.aspid.fasttools/`) plus two standalone .NET solutions,
`Aspid.FastTools.Generators/` and `Aspid.FastTools.Analyzers/`, whose DLLs are committed into the package, and
Agent Skills for projects that consume the package in `skills/`.

## Never

- Use Unity APIs newer than **6000.0.53f1** without a `#if UNITY_6000_x_OR_NEWER` guard. An API from a later 6000.0
  patch needs a higher `unityRelease` instead.
- Write to `Console` from generator or analyzer code, or reference `SourceGenerator.Foundations`.
- Commit `com.unity.upm-publishing-tools` to the dev project manifest.
- Use `UnityEngine.Input` in the dev project: it uses only the Input System.

## Rules

- After a change to generator or analyzer source, run `dotnet build -c Release` in that solution and commit the
  rebuilt DLL. Run it before `PackagedAnalyzerTests` too: they test the committed DLL.
- The SerializeReference YAML engine (`Aspid.FastTools.YamlTests/`) and `GenericTypeResolver`, `GenericArgumentFilter`,
  `TypeUtility` (`Aspid.FastTools.TypeTests/`) compile outside Unity: C# 9, .NET Standard 2.1 for the engine, and no
  Unity API except the stubbed `Debug.LogError`, `AssetDatabase.MakeEditable`, `UnityEngine.Object` and
  `CompilationPipeline.GetAssemblies`. A test that needs the Editor goes into its fixture's `*.Unity.cs` part.
- `Tests/Editor/PublicApi/PublicApi.txt` in the package is the public API snapshot. After an intended API change,
  rewrite it on the Unity version that `WholeApiVersion` in `PublicApiSnapshotTests` names, or a newer one, and commit
  it in the same PR:
  `p="$(mktemp -d)/aspid-api"; v=$(scripts/make-unity-test-project.sh "$p" <version>)`, then
  `ASPID_FASTTOOLS_WRITE_PUBLIC_API=1 <Unity $v> -batchmode -nographics -projectPath "$p" -runTests -testPlatform EditMode -testFilter PublicApiSnapshotTests -testResults "$p/public-api.xml"`.
  When a public member gets a newer `UNITY_6000_x_OR_NEWER` guard, move the `#if` around `HasWholeApi` and
  `WholeApiVersion` to it.
- Bump the version only with `scripts/set-version.sh <version>`. `/asp-ft-release <version>` runs the whole release,
  the Asset Store upload included.
- `skills/` holds skills for consumers of the package (`npx skills add VPDPersonal/Aspid.FastTools`). Skills for
  working on this repo go into `.claude/skills/` with `metadata.internal: true`. A skill that describes public API is
  updated in the same PR as that API.
- The minimum Unity version is `unity` and `unityRelease` in `package.json`; `scripts/check-unity-minimum.mjs` names
  the other files to update by hand.
- In `.claude/settings.json` allow only commands that cannot run code or read the environment; put the rest in
  `.claude/settings.local.json`.
- After a change to public API or its XML docs, run `npm run api` in `Website/` and commit `Website/api/` in the same
  PR. Steps for a worktree: `.claude/skills/docs-site/references/api-reference.md`.

## Checks

Run what applies before you open a PR.

- Generators and analyzers: `dotnet test` in `Aspid.FastTools.Generators/` or `Aspid.FastTools.Analyzers/`.
- YAML engine: `dotnet test Aspid.FastTools.YamlTests`.
- Type resolver: `dotnet test Aspid.FastTools.TypeTests`.
- Compile in the dev project with its Editor open: `unity command recompile`, then `unity command get_console_logs`.
- EditMode tests on any Unity version: `scripts/make-unity-test-project.sh <dir> [unity-version]`; its header shows
  the `-runTests` command.
- Scripts: `npm --prefix Website ci && node --test scripts/*.test.mjs`.
- Site, in `Website/`: `npm ci`, then `npm run check-readme`, `npm run check-translations`, `npm test` and
  `npm run build`.
- Repository: `node scripts/check-package-files.mjs`, `node scripts/check-version.mjs`,
  `node scripts/check-unity-minimum.mjs`, `node scripts/check-contrast.mjs`, and
  `npm install --no-save js-yaml@4 && node scripts/check-skills.mjs`.

## C# style beyond `.editorconfig`

- Named arguments, even for a single obvious argument: `Type.GetType(_aqn, throwOnError: false)`.
- `using` directives sorted by ascending line length, not alphabetically; `System` is not forced first.
- Leaf classes are `sealed` unless designed for inheritance. `internal` for implementation, `public` for API.
- Public API of the package carries XML docs (`<summary>`, `<param>`, `<returns>`, `<see cref/>`).

# Unity project and package

The dev project; the package is in `Packages/tech.aspid.fasttools/`.

## Never

- Use Unity APIs newer than **6000.0.53f1** without a `#if UNITY_6000_x_OR_NEWER` guard. An API from a later 6000.0
  patch needs a higher `unityRelease` instead.
- Commit `com.unity.upm-publishing-tools` to `Packages/manifest.json`.
- Use `UnityEngine.Input` in the dev project: it uses only the Input System.

## Rules

- The SerializeReference YAML engine (`Aspid.FastTools.YamlTests/`) and `GenericTypeResolver`, `GenericArgumentFilter`,
  `TypeUtility` (`Aspid.FastTools.TypeTests/`) compile outside Unity: C# 9, .NET Standard 2.1 for the engine, and no
  Unity API except the stubbed `Debug.LogError`, `AssetDatabase.MakeEditable`, `UnityEngine.Object` and
  `CompilationPipeline.GetAssemblies`. A test that needs the Editor goes into its fixture's `*.Unity.cs` part.
- `Tests/Editor/PublicApi/PublicApi.txt` in the package is the public API snapshot. After an intended API change,
  rewrite it on the Unity version that `WholeApiVersion` in `PublicApiSnapshotTests` names, or a newer one, and commit
  it in the same PR. From the repository root:
  `p="$(mktemp -d)/aspid-api"; v=$(scripts/make-unity-test-project.sh "$p" <version>)`, then
  `ASPID_FASTTOOLS_WRITE_PUBLIC_API=1 <Unity $v> -batchmode -nographics -projectPath "$p" -runTests -testPlatform EditMode -testFilter PublicApiSnapshotTests -testResults "$p/public-api.xml"`.
  When a public member gets a newer `UNITY_6000_x_OR_NEWER` guard, move the `#if` around `HasWholeApi` and
  `WholeApiVersion` to it.
- After a change to public API or its XML docs, run `npm run api` in `Website/` and commit `Website/api/` in the same
  PR. Steps for a worktree: `.claude/skills/docs-site/references/api-reference.md`. A skill in `skills/` that
  describes that API is updated in the same PR too.

## Checks

From the repository root:

- Compile in the dev project with its Editor open: `unity command recompile`, then `unity command get_console_logs`.
- EditMode tests on any Unity version: `scripts/make-unity-test-project.sh <dir> [unity-version]`; its header shows
  the `-runTests` command.
- YAML engine: `dotnet test Aspid.FastTools.YamlTests`.
- Type resolver: `dotnet test Aspid.FastTools.TypeTests`.

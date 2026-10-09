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
  not offer them. `scripts/check-skills.mjs` (CI, on every PR) checks every `SKILL.md` in the repository: a file outside
  `skills/` must be internal, because the installer scans about 30 agent folders, and an `AFT` code in a skill must be
  reported by an analyzer. A skill that describes public API is updated in the same PR as that API.
- `.claude/settings.json` is also loaded by the Claude agent in `claude.yml`, which keeps `GITHUB_TOKEN` in its
  environment. Allow there only commands that cannot run code or read the environment; put the rest in
  `.claude/settings.local.json`.

## C# style beyond `.editorconfig`

- Named arguments, even for a single obvious argument: `Type.GetType(_aqn, throwOnError: false)`.
- `using` directives sorted by ascending line length, not alphabetically; `System` is not forced first.
- Leaf classes are `sealed`; leave one open only when it is designed for inheritance. `internal` for implementation,
  `public` for API.
- Public API of the package carries XML docs (`<summary>`, `<param>`, `<returns>`, `<see cref/>`): the site's API
  reference is generated from them.

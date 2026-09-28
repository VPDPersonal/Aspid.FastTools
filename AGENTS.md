# Aspid.FastTools

Unity package `tech.aspid.fasttools` (`Aspid.FastTools/Packages/tech.aspid.fasttools/`) plus two standalone .NET solutions,
`Aspid.FastTools.Generators/` and `Aspid.FastTools.Analyzers/`, whose DLLs are committed into the package, and
Agent Skills for projects that consume the package in `skills/`.

## Never

- Use Unity APIs newer than **6000.0** — the dev project runs 6000.4, but `package.json` promises 6000.0.
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
  READMEs; bump all of them with `scripts/set-version.sh <version>`, which the release workflow checks
  (`scripts/check-version.mjs`). The version also picks the channel, and the script switches it: a prerelease installs
  from `#upm-preview` under a Preview badge, a stable version from `#upm` under a Release one; the site derives
  `UPM_BRANCH` from the version.
- `skills/` is for **consumers** of the package and is installed with `npx skills add VPDPersonal/Aspid.FastTools`;
  skills for working on this repo live in `.claude/skills/` and carry `metadata.internal: true` so that command does
  not offer them; `scripts/check-skills.mjs` (CI) checks both. A skill that describes public API is updated in the same PR
  as that API.

## C# style beyond `.editorconfig`

- Named arguments, even for a single obvious argument: `Type.GetType(_aqn, throwOnError: false)`.
- `using` directives sorted by ascending line length, not alphabetically; `System` is not forced first.
- Leaf classes are `sealed`; leave one open only when it is designed for inheritance. `internal` for implementation,
  `public` for API.
- Public API of the package carries XML docs (`<summary>`, `<param>`, `<returns>`, `<see cref/>`): the site's API
  reference is generated from them.

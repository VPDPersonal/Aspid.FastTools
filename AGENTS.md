# Aspid.FastTools

Unity package `tech.aspid.fasttools` (`Aspid.FastTools/Packages/tech.aspid.fasttools/`) plus two standalone .NET solutions,
`Aspid.FastTools.Generators/` and `Aspid.FastTools.Analyzers/`, whose DLLs are committed into the package, and
Agent Skills for projects that consume the package in `skills/`. The rules of each area are in the `AGENTS.md` of
`Aspid.FastTools/` (Unity project and package), `Aspid.FastTools.Generators/`, `Aspid.FastTools.Analyzers/` and
`Website/`.

## Rules

- Bump the version only with `scripts/set-version.sh <version>`. `/asp-ft-release <version>` runs the whole release,
  the Asset Store upload included.
- In `.claude/settings.json` allow only commands that cannot run code or read the environment; put the rest in
  `.claude/settings.local.json`.

## Checks

Run what applies before you open a PR.

- Scripts: `npm --prefix Website ci && node --test scripts/*.test.mjs`.
- Repository: `node scripts/check-package-files.mjs`, `node scripts/check-version.mjs`,
  `node scripts/check-unity-minimum.mjs`, `node scripts/check-contrast.mjs`, and
  `npm install --no-save js-yaml@4 && node scripts/check-skills.mjs`.

## C# style beyond `.editorconfig`

- Named arguments, even for a single obvious argument: `Type.GetType(_aqn, throwOnError: false)`.
- `using` directives sorted by ascending line length, not alphabetically; `System` is not forced first.
- Leaf classes are `sealed` unless designed for inheritance. `internal` for implementation, `public` for API.
- Public API of the package carries XML docs (`<summary>`, `<param>`, `<returns>`, `<see cref/>`).
- `Samples~` is code a newcomer reads, not package API, and follows its own rules (`node scripts/check-samples.mjs`
  checks them):
  - No `//`, `///` or `/* */` comments, no ReSharper banner, `[Header]`, `Debug.Log` or XML docs; `[Tooltip]` only
    where it tells more than the field name.
  - Each asmdef sets `rootNamespace` to its namespace, and the `.cs` files sit flat beside it in `Scripts/` (an editor
    asmdef in `Scripts/Editor/`). Scenes go in `Scenes/`, and the visuals in `Materials/`, `Shaders/` (the sample's
    own `Studio` shader) and `Fonts/` with their licences. One top-level type per file, named after it.
  - Name every argument of a non-fluent call with two or more arguments, and a lone `bool`, number or `null`
    literal: `Instantiate(original: _enemyPrefab, parent: transform)`, `_list.SetSelection(index: 0)`. Keep
    single-argument fluent setters, profiler markers and attribute arguments positional: `SetWidth(220)`,
    `WithName("Steering")`, `[Range(10, 400)]`.
  - Scenes need no optional module: no lights, colliders or physics. No sample ships a broken asset.
  - A kept file keeps its `.meta` GUID. Every serialized field with a constant initializer is read in code (CS0414).

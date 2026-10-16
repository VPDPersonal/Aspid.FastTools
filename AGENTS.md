# Aspid.FastTools

Unity package `tech.aspid.fasttools` (`Aspid.FastTools/Packages/tech.aspid.fasttools/`) plus two standalone .NET solutions,
`Aspid.FastTools.Generators/` and `Aspid.FastTools.Analyzers/`, whose DLLs are committed into the package, and
Agent Skills for projects that consume the package in `skills/`. The rules of each area are in the `AGENTS.md` of
`Aspid.FastTools/` (Unity project and package), `Aspid.FastTools.Generators/`, `Aspid.FastTools.Analyzers/` and
`Website/`.

## Rules

- Bump the version only with `scripts/set-version.sh <version>`. `/asp-ft-release <version>` runs the whole release,
  the Asset Store upload included.
- `asp-branch`, `asp-commit` and `asp-pr` in `.claude/skills/` are copies from Aspid.AgentKit. Do not edit them;
  update them only with `scripts/sync-agentkit-skills.sh`.
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

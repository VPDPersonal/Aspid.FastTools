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

## C# style

`.editorconfig` holds the rules a formatter checks; [CODESTYLE.md](CODESTYLE.md) holds the rest: member order, named
arguments, attribute and blank-line layout, comments and XML docs. Read it before you write C#.

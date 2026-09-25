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
- The version lives in `package.json`, the badge SVG and the install URLs of both READMEs; bump all of them with
  `scripts/set-version.sh <version>`. A release is prepared with `scripts/prepare-release.mjs` (the `release` skill),
  and merging the PR that changes the `package.json` version publishes it.
- `CHANGELOG.ru.md` translates `CHANGELOG.md` bullet for bullet and the package ships a byte-for-byte copy of
  `CHANGELOG.md`; `scripts/check-changes.mjs` checks this and the rest of what CI requires. Run it before committing, or
  enable it as a pre-commit hook once per clone with `git config core.hooksPath .githooks`.
- `skills/` is for **consumers** of the package and is installed with `npx skills add VPDPersonal/Aspid.FastTools`;
  skills for working on this repo live in `.claude/skills/` and carry `metadata.internal: true` so that command does
  not offer them; `scripts/check-skills.mjs` (CI) checks both. A skill that describes public API is updated in the same PR
  as that API.

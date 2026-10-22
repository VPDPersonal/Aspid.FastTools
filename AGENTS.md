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
- Repository: `node scripts/check-package-files.mjs`, `node scripts/check-samples.mjs`,
  `node scripts/check-version.mjs`, `node scripts/check-unity-minimum.mjs`, `node scripts/check-contrast.mjs`, and
  `npm install --no-save js-yaml@4 && node scripts/check-skills.mjs`.

## C# style

`.editorconfig` holds the rules a formatter checks; the [asp-ft-code-style](.claude/skills/asp-ft-code-style/SKILL.md)
skill holds the rest: member order, named arguments, attribute and blank-line layout, comments and XML docs. Read it
before you write C#.

## Samples~

`Samples~` is code a newcomer reads, not package API. Beyond the code style skill (`node scripts/check-samples.mjs`
checks these):

- No comments except the `CheckNamespace` banner, and no `[Header]`, `Debug.Log` or XML docs; `[Tooltip]` only where
  it tells more than the field name.
- Each asmdef sets `rootNamespace`. Its `.cs` files sit in `Scripts/` (an editor asmdef in `Scripts/Editor/`); a
  subfolder of `Scripts/` groups related types and adds its name to the namespace: `Types/Scripts/Enemies/` holds
  `Aspid.FastTools.Samples.Types.Enemies`. Scenes go in `Scenes/`, assets in `Data/`, style sheets in `Styles/`, and
  the visuals in `Materials/`, `Shaders/` (the sample's own `Studio` shader) and `Fonts/` with their licences.
- Scenes need no optional module: no lights, colliders or physics. No sample ships a broken asset.
- The site GIFs show the scenes and the Ability Catalog window: change their look only together with the GIFs.
- A kept file keeps its `.meta` GUID. Every serialized field with a constant initializer is read in code (CS0414).

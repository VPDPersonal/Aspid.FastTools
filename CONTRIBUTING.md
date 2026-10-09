# Contributing to Aspid.FastTools

This page says how to set up the repository, what a pull request must contain and what CI checks.
[AGENTS.md](AGENTS.md) holds the same code rules in short form for coding agents.

## Before you start

- **A bug:** open an issue with the Bug report form. Give the Unity version, the package version and the steps to reproduce.
- **A new feature or a large change:** open a [discussion](https://github.com/VPDPersonal/Aspid.FastTools/discussions) first, so you do not write code the package will not take.
- **A small fix, a typo or a docs correction:** send a pull request directly.
- **A security problem:** do not open an issue. Follow [SECURITY.md](SECURITY.md).

By sending a pull request you agree to license your contribution under the [MIT License](LICENSE).

## Repository layout

| Path | What it holds |
|---|---|
| `Aspid.FastTools/Packages/tech.aspid.fasttools/` | The Unity package: `Runtime/`, `Editor/`, `Tests/`, `Samples~/` and the two committed Roslyn DLLs |
| `Aspid.FastTools/` | The Unity project that opens the package for development |
| `Aspid.FastTools.Generators/`, `Aspid.FastTools.Analyzers/` | .NET solutions whose Release build produces the DLLs in the package |
| `Aspid.FastTools.YamlTests/` | Tests of the SerializeReference YAML engine, run without Unity |
| `Website/` | The documentation site: pages, tutorials, Russian translations and the API reference |
| `skills/` | Agent Skills for projects that use the package |

## Set up

1. Install the Unity version in `Aspid.FastTools/ProjectSettings/ProjectVersion.txt` and open the `Aspid.FastTools/` folder as a project.
2. Install the .NET SDK that `global.json` pins. It allows no other version. The test projects target `net6.0`, so install the .NET 6 runtime too.
3. Install Node.js 22 and run `npm --prefix Website ci`. Only the documentation site needs it.

## Code rules

A pull request that breaks one of these does not merge:

- Use no Unity API newer than **6000.0.53f1** without an `#if UNITY_6000_x_OR_NEWER` guard. The development project runs a newer Unity than `package.json` promises. Unity has no define for a patch: an API from a later 6000.0 patch needs a higher `unityRelease`.
- Do not reach editor-only APIs from runtime code.
- Do not write to `Console` or reference `SourceGenerator.Foundations` in generator or analyzer code. It deadlocks Unity's compilation server.
- Keep the SerializeReference YAML engine (`Editor/Scripts/SerializeReferences/Yaml`) on C# 9 with no Unity API: `Aspid.FastTools.YamlTests` compiles it outside Unity.

Style is set by `.editorconfig` and the "C# style" section of [AGENTS.md](AGENTS.md): named arguments, `using` directives sorted by line length, `sealed` leaf classes, `internal` for implementation. Public API carries XML docs.

Unity writes a `.meta` file for every new file or folder under `Aspid.FastTools/`. Commit it with the file. Do not commit changes Unity makes to other `.meta` files, to `ProjectSettings/` or to `Packages/*.json`.

## Generators and analyzers

A change to generator or analyzer source reaches Unity only through the DLLs committed into the package.

1. Run the tests in the solution folder: `dotnet test`. This is a Debug build and does not copy the DLL.
2. Build in Release in the same folder: `dotnet build -c Release`. This copies the DLL into the package.
3. Commit the changed DLL together with the source.

Build with the SDK from `global.json`. The release workflow rebuilds both DLLs and fails when they differ from the committed ones.

## Tests

| Area | Command |
|---|---|
| Generators | `dotnet test` in `Aspid.FastTools.Generators/` |
| Analyzers | `dotnet test` in `Aspid.FastTools.Analyzers/` |
| YAML engine | `dotnet test Aspid.FastTools.YamlTests` in the repository root |
| Package | **Window → General → Test Runner → EditMode** in the development project |

Add or extend a test for every behavior change a test project can reach. EditMode tests live in `Aspid.FastTools/Packages/tech.aspid.fasttools/Tests/Editor/`. To run them on another Unity version, use the batch command in the header of `scripts/make-unity-test-project.sh`.

## CHANGELOG

A change to `Runtime/`, `Editor/`, `Samples~/` (except `Documentation`) or the package DLLs that users can see needs an entry under `[Unreleased]` in [CHANGELOG.md](CHANGELOG.md), under Added, Changed, Fixed or Removed.

- Put the entry next to the most related one, not at the end of the section. Parallel pull requests then do not conflict on the same line.
- Add the same entry in Russian to [CHANGELOG.ru.md](CHANGELOG.ru.md). If you do not write Russian, say so in the pull request.
- A breaking change goes under Changed and says how to migrate.
- Users see no change (a refactor, a test)? Ask for the `no-changelog` label instead.

## Documentation and skills

- Pages live in `Website/docs/` and `Website/tutorials/`. Each page has a Russian twin under `Website/i18n/ru/`. Change both: CI compares their headings, code blocks, images and links. If you do not write Russian, say so in the pull request.
- The root `README.md` is generated. Edit `Website/docs/README.md` and run `npm --prefix Website run sync-readme`.
- Build the site with `npm --prefix Website run build`.
- A change to public API or to its XML docs regenerates the API reference with `npm run api` in `Website/` and commits `Website/api/`. It needs a Unity install and DocFX. Without them, say in the pull request that the reference still needs regenerating.
- A skill in `skills/` that describes API you changed is updated in the same pull request. Check skill files with `node scripts/check-skills.mjs`.

## Pull request

1. Branch from `main`. Keep one logical change in one pull request.
2. Do not change the package version. A maintainer does it at release with `scripts/set-version.sh`.
3. Title the pull request `type(scope): summary`, in [Conventional Commits](https://www.conventionalcommits.org/) form. Pull requests are squash-merged, so the title becomes the commit subject.
4. Fill in the pull request template: why, what and how you checked it.

The title rules, which CI checks:

- At most **72 characters**.
- No period at the end.
- A type and a scope from [.claude/asp-pr.md](.claude/asp-pr.md). The scope is optional.
- `!` before the colon for a breaking change.

Examples:

```text
fix(types): stop the type picker selection refresh loop on Unity 6000.6
feat(visual-elements)!: add plural class and style sheet extensions
docs(enums): review the EnumValues page
```

CI sets the `type:`, `area:` and `breaking-change` labels from the title and the changed paths. Do not add them yourself.

### What CI checks

- **PR title** and **CHANGELOG**: the rules above.
- **Tests:** the generator, analyzer and YAML engine tests on every pull request. The Unity EditMode tests run on five Unity versions when a change can affect them. They need repository secrets, so a pull request from a fork skips them: say how you tested in Unity.
- **Docs:** the site builds when `Website/`, a README or a CHANGELOG changes.
- **Skills:** the `SKILL.md` front matter is valid when a skill changes.
- **Review:** a bot reviews non-draft pull requests opened from branches of this repository, by the rules in [.github/claude-review.md](.github/claude-review.md).

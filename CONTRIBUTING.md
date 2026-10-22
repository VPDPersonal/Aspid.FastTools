# Contributing to Aspid.FastTools

Contributions are licensed under the [MIT License](LICENSE). Report security problems as [SECURITY.md](SECURITY.md)
says.

## Set up

- Package: open `Aspid.FastTools/` in the Unity version from `Aspid.FastTools/ProjectSettings/ProjectVersion.txt`.
- Generators, analyzers, tests: the .NET SDK pinned in `global.json` (no other version runs) and the .NET 6 runtime.
- Docs and scripts: Node.js 22, then `npm --prefix Website ci`.

## Code

- No Unity API newer than **6000.0.53f1** without an `#if UNITY_6000_x_OR_NEWER` guard.
- The YAML engine (`Editor/Scripts/SerializeReferences/Yaml`), `GenericTypeResolver`, `GenericArgumentFilter` and
  `TypeUtility` stay on C# 9 with no new Unity API: test projects compile them outside Unity.
- Style: `.editorconfig` and [CODESTYLE.md](CODESTYLE.md); other rules: [AGENTS.md](AGENTS.md).
- Generator or analyzer change: run `dotnet test`, then `dotnet build -c Release` in its solution folder and commit
  the rebuilt DLL with the source. CI fails if they differ.

## Tests

`dotnet test` in `Aspid.FastTools.Generators/`, `Aspid.FastTools.Analyzers/`, `DotnetTests/Yaml` and
`DotnetTests/Types`; package tests in the Unity Test Runner (EditMode). A behavior change a test project can reach
needs a test.

## CHANGELOG and docs

- A user-visible package change goes under `[Unreleased]` in both [CHANGELOG.md](CHANGELOG.md) and
  [Website/i18n/ru/CHANGELOG.md](Website/i18n/ru/CHANGELOG.md); a breaking change says how to migrate. Nothing
  visible: ask for the `no-changelog` label.
- Docs in `Website/docs/` and `Website/tutorials/` have Russian twins in `Website/i18n/ru/`. Machine translation, or
  English text in the Russian file noted in the PR, is fine.
- Do not edit the root `README.md`: it is generated from `Website/docs/README.md` by
  `npm --prefix Website run sync-readme`.
- Changed API described by a skill in `skills/`? Update the skill.

## Pull request

- Branch from `main`, one logical change per PR, no package version bump.
- Title: `type(scope): summary` ([Conventional Commits](https://www.conventionalcommits.org/)), up to 72 characters,
  no period at the end, types and scopes from [.claude/asp-pr.md](.claude/asp-pr.md); `!` before the colon for a
  breaking change.
- On a PR from a fork the **Unity** check fails (it needs repository secrets): a maintainer runs it. Say how you
  tested in Unity.

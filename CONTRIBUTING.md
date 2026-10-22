# Contributing to Aspid.FastTools

## Before you start

Pull requests are welcome directly. To check that a change fits the package before you spend time on it, open a Bug
report or a Feature request first. Report security problems privately, as [SECURITY.md](SECURITY.md) says.

By sending a pull request you agree to license your contribution under the [MIT License](LICENSE).

## Set up

- **Package code:** open `Aspid.FastTools/` in Unity 6, ideally the version in
  `Aspid.FastTools/ProjectSettings/ProjectVersion.txt`.
- **Generators, analyzers and the test projects:** install the .NET SDK that `global.json` pins (no other version runs)
  and the .NET 6 runtime.
- **Docs and repository scripts:** install Node.js 22 and run `npm --prefix Website ci`.

## Code rules

- Use no Unity API newer than **6000.0.53f1** without an `#if UNITY_6000_x_OR_NEWER` guard.
- Keep the YAML engine (`Editor/Scripts/SerializeReferences/Yaml`) and `GenericTypeResolver`, `GenericArgumentFilter`,
  `TypeUtility` on C# 9 with no new Unity API: test projects compile them outside Unity.

C# style is in `.editorconfig` and [CODESTYLE.md](CODESTYLE.md); the other rules are in [AGENTS.md](AGENTS.md).

## Generators and analyzers

Unity sees a change to generator or analyzer source only through the DLLs committed into the package. In the solution
folder run `dotnet test`, then `dotnet build -c Release`, and commit the changed DLL with the source. CI fails when the
committed DLLs differ from a Release build.

## Tests

Run `dotnet test` in `Aspid.FastTools.Generators/` and `Aspid.FastTools.Analyzers/`, and
`dotnet test DotnetTests/Yaml` and `dotnet test DotnetTests/Types` in the repository root. Package
tests run in **Window → General → Test Runner → EditMode**. Add a test for every behavior change a test project can reach.

## CHANGELOG and docs

- A change users can see in the package goes under `[Unreleased]` in [CHANGELOG.md](CHANGELOG.md) and
  [Website/i18n/ru/CHANGELOG.md](Website/i18n/ru/CHANGELOG.md), next to the most related entry. A breaking change says
  how to migrate. No visible change? Ask for the `no-changelog` label.
- Docs pages live in `Website/docs/` and `Website/tutorials/`, each with a Russian twin under `Website/i18n/ru/`.
  The root `README.md` is generated from `Website/docs/README.md` by `npm --prefix Website run sync-readme`.
- Update a skill in `skills/` that describes API you changed.

If you do not write Russian, a machine translation is fine. Or put the English text in the Russian file and say so in
the pull request.

## Pull request

- Branch from `main`, one logical change per pull request. Do not change the package version.
- Title: `type(scope): summary` in [Conventional Commits](https://www.conventionalcommits.org/) form, up to 72
  characters, no period at the end, with types and scopes from [.claude/asp-pr.md](.claude/asp-pr.md). Add `!` before
  the colon for a breaking change.
- Fill in the pull request template: why, what and how you checked it.

The Unity tests in CI need repository secrets, so on a pull request from a fork the **Unity** check fails. This is
expected: a maintainer runs the tests and merges. Say how you tested in Unity.

The Unity tests run on every Unity 6000.x line, and only when a change reaches what they read: a change to Markdown,
license texts or files that the Unity jobs do not read skips them, and so does a commit whose content already
passed them. When a change cannot depend on the line, a maintainer can set the `unity-quick` label: the PR then tests
only the minimum and the newest Unity version, and `main` tests all of them after the merge. Adding or removing
`unity-quick` restarts the CI of the pull request; another label waits for the running CI and then skips the Unity tests
that passed there.

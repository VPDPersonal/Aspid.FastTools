# PR rules for Aspid.FastTools

Checked by `.github/workflows/pr-checks.yml`; keep this file, the Pull request section of `CONTRIBUTING.md` and the title hint in `.github/PULL_REQUEST_TEMPLATE.md` in sync with it.

- Types: `feat` `fix` `perf` `refactor` `docs` `test` `chore` `ci` `style`.
- Scopes (or none): `types` `ids` `enums` `serialize-references` `profiler-markers` `visual-elements` `serialized-property` · `runtime` `editor` `generators` `analyzers` `samples` `package` · `website` `docs` `changelog` `release` `ci` `github` `skills` `claude` `deps`.
- CI sets `type:*` and `breaking-change` from the title and `area:*` from the paths, and removes the ones that no longer apply; do not pass them.
- `!` before the colon marks a breaking change. The title check requires it when the PR removes or changes a line of
  `Aspid.FastTools/Packages/tech.aspid.fasttools/Tests/Editor/PublicApi/PublicApi.txt` that is not a `#` comment; the
  CHANGELOG entry then goes under Changed with a migration note. A line that only gains an interface or a base class
  (class and struct) or the `[Flags]` of an enum does not count. A PR that also changes `PublicApiSnapshot.cs` gets a
  warning instead of a failure: a maintainer judges the break by hand.
- Shipped package code (`Runtime/`, `Editor/`, `Samples~/`, package DLLs, the package `package.json`) changed → the same entry under `[Unreleased]` in `CHANGELOG.md` and `Website/i18n/ru/CHANGELOG.md` (one file alone fails the check), or the `no-changelog` label when users see no change.
- `unity-quick` label (set by hand, CI reruns): the Unity EditMode matrix tests only the minimum and the newest Unity
  version instead of every tested line; the push to `main` after the merge runs every version. Decide on it at every
  `gh pr create` and name the decision in the report. The Unity jobs run only when the PR changes the package (except
  its Markdown), `scripts/make-unity-test-project.sh` or `.github/workflows/ci.yml`. Pass `--label unity-quick` unless
  the diff can behave differently across Unity lines: a `#if UNITY_*` block, a Unity API that changed between 6000.x
  versions, the generator or analyzer DLLs, an `.asmdef`, the Unity jobs of `ci.yml` (`UNITY_VERSIONS` among them) or
  `make-unity-test-project.sh`. Text in the package, `package.json` descriptions, tests, the rest of `ci.yml`, and a
  local refactor or UI change on stable API get the label. Any label a person adds or removes (`no-changelog` too)
  cancels the running CI and starts it again, so set the labels in one go at `gh pr create`: added later,
  `unity-quick` restarts the jobs that are already halfway.

## Review loop

The bot review rules with the verdict format are in `.github/claude-review.md`.

Logic paths:

- package: `Aspid.FastTools/Packages/tech.aspid.fasttools/` (except `README.md`), `Aspid.FastTools/Packages/manifest.json`,
  `Aspid.FastTools/Packages/packages-lock.json`, `Aspid.FastTools/Assets/DevTests~/`,
  `Aspid.FastTools/Assets/DevTools/`;
- .NET: `Aspid.FastTools.Generators/`, `Aspid.FastTools.Analyzers/`, `DotnetTests/Yaml/`,
  `DotnetTests/Types/`, `global.json`;
- site: `Website/` except `docs/`, `i18n/`, `tutorials/`, `api/`, `static/`;
- CI: `scripts/`, `.github/workflows/`, `.github/claude-review.md`.

Other paths are text: `Website/docs/`, `Website/i18n/`, `Website/tutorials/`, `skills/`, other `*.md`.
A fix there needs no re-review.

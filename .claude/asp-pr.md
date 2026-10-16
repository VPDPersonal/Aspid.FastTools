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

## Review loop

The bot review rules with the verdict format are in `.github/claude-review.md`.

Logic paths:

- package: `Aspid.FastTools/Packages/tech.aspid.fasttools/` (except `README.md` and `SETUP.md`), `Aspid.FastTools/Packages/manifest.json`,
  `Aspid.FastTools/Packages/packages-lock.json`, `Aspid.FastTools/Assets/DevTests/`;
- .NET: `Aspid.FastTools.Generators/`, `Aspid.FastTools.Analyzers/`, `DotnetTests/Yaml/`,
  `DotnetTests/Types/`, `global.json`;
- site: `Website/` except `docs/`, `i18n/`, `tutorials/`, `api/`, `static/`;
- CI: `scripts/`, `.github/workflows/`, `.github/claude-review.md`.

Other paths are text: `Website/docs/`, `Website/i18n/`, `Website/tutorials/`, `skills/`, other `*.md`.
A fix there needs no re-review.

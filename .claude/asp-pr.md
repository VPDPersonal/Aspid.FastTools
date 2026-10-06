# PR rules for Aspid.FastTools

Checked by `.github/workflows/pr-checks.yml`; keep this file in sync with it.

- Types: `feat` `fix` `perf` `refactor` `docs` `test` `chore` `ci` `style`.
- Scopes (or none): `types` `ids` `enums` `serialize-references` `profiler-markers` `visual-elements` `serialized-property` · `runtime` `editor` `generators` `analyzers` `samples` `package` · `website` `docs` `changelog` `release` `ci` `github` `skills` `claude` `deps`.
- CI sets `type:*` and `breaking-change` from the title and `area:*` from the paths; do not pass them.
- Shipped package code (`Runtime/`, `Editor/`, `Samples~/`, package DLLs) changed → an entry under `[Unreleased]` in `CHANGELOG.md`, or the `no-changelog` label when users see no change.

## Review loop

The bot review rules with the verdict format are in `.github/claude-review.md`.

Logic paths:

- package: `Aspid.FastTools/Packages/tech.aspid.fasttools/` (except `README.md`), `Aspid.FastTools/Packages/manifest.json`,
  `Aspid.FastTools/Packages/packages-lock.json`, `Aspid.FastTools/Assets/DevTests/`;
- .NET: `Aspid.FastTools.Generators/`, `Aspid.FastTools.Analyzers/`, `Aspid.FastTools.YamlTests/`, `global.json`;
- site: `Website/` except `docs/`, `i18n/`, `tutorials/`, `api/`, `static/`;
- CI: `scripts/`, `.github/workflows/`, `.github/claude-review.md`.

Other paths are text: `Website/docs/`, `Website/i18n/`, `Website/tutorials/`, `skills/`, other `*.md`.
A fix there needs no re-review.

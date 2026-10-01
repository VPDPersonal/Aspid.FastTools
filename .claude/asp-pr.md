# PR rules for Aspid.FastTools

Checked by `.github/workflows/pr-checks.yml`; keep this file in sync with it.

- Types: `feat` `fix` `perf` `refactor` `docs` `test` `chore` `ci` `style`.
- Scopes (or none): `types` `ids` `enums` `serialize-references` `profiler-markers` `visual-elements` `serialized-property` · `runtime` `editor` `generators` `analyzers` `samples` `package` · `website` `docs` `changelog` `release` `ci` `github` `skills` `claude` `deps`.
- CI sets `type:*` and `breaking-change` from the title and `area:*` from the paths; do not pass them.
- Shipped package code (`Runtime/`, `Editor/`, `Samples~/`, package DLLs) changed → an entry under `[Unreleased]` in `CHANGELOG.md`, or the `no-changelog` label when users see no change.

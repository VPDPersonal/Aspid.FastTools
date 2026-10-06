# PR rules for Aspid.FastTools

Checked by `.github/workflows/pr-checks.yml`; keep this file in sync with it.

- Types: `feat` `fix` `perf` `refactor` `docs` `test` `chore` `ci` `style`.
- Scopes (or none): `types` `ids` `enums` `serialize-references` `profiler-markers` `visual-elements` `serialized-property` · `runtime` `editor` `generators` `analyzers` `samples` `package` · `website` `docs` `changelog` `release` `ci` `github` `skills` `claude` `deps`.
- CI sets `type:*` and `breaking-change` from the title and `area:*` from the paths; do not pass them.
- Shipped package code (`Runtime/`, `Editor/`, `Samples~/`, package DLLs) changed → an entry under `[Unreleased]` in `CHANGELOG.md`, or the `no-changelog` label when users see no change.

## Review loop

`.github/workflows/claude.yml` reviews a PR once, when it leaves draft. The review rules are in `.github/claude-review.md`.

- Verdict: the first line of the `claude[bot]` review comment, `Verdict: <N> blocking, <M> minor`.
  Each inline comment starts with `[blocking]` or `[minor]`.
- Re-review: post a PR comment `@claude review the commits after your last review`.
  The `mention` job runs it with the same rules.
- Logic paths: `Aspid.FastTools/Packages/tech.aspid.fasttools/` (except `README.md`), `Aspid.FastTools.Generators/`,
  `Aspid.FastTools.Analyzers/`, `Aspid.FastTools.YamlTests/`, `scripts/`, `.github/workflows/`.
  Other paths (`Website/`, `skills/`, `*.md`) are text: a fix there needs no re-review.

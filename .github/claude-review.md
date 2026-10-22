# Rules for Claude in this repository

## Always

- Follow the root `AGENTS.md` and the `AGENTS.md` of the area a file is in.
- Write in English. Keep code identifiers as they are.

## When you review a pull request

Check, in this order:

1. Correctness: bugs and missed edge cases, with the input that breaks.
2. Data and code in user projects:
   - a renamed or moved file keeps its `.meta` GUID;
   - a renamed serialized field gets `[FormerlySerializedAs]`, a renamed or moved type stored through
     `[SerializeReference]` gets `[MovedFrom]`: without them user projects lose saved data;
   - no Unity API newer than 6000.0.53f1 without a `#if UNITY_6000_x_OR_NEWER` guard;
   - no editor-only API reachable from runtime code: CI runs only Editor tests and misses it.
3. Generators and analyzers: no `Console` output, no reference to `SourceGenerator.Foundations`.
4. Public API: XML docs on each new or changed member, a CHANGELOG entry that describes the change as a user sees it,
   and an updated skill in `skills/` when one describes the API. A change to public API or XML docs without a
   regenerated `Website/api/` is a `[minor]`, unless the PR says the reference still needs regenerating.
5. Workflows: actions pinned to a full commit SHA, only the permissions a job uses, and no `pull_request_target` job
   that checks out or runs the code of the PR.

Do not report:

- What CI checks: build and test failures, the PR title, a missing CHANGELOG entry, Roslyn DLLs that differ from their
  sources, version files, missing `.meta` files, translations, SKILL.md frontmatter.
- Style and naming that `.editorconfig`, the `asp-ft-code-style` and `asp-ft-samples` skills and AGENTS.md do not
  cover.
- The content of generated files: `Website/api/`, the committed DLLs, `Tests/Editor/PublicApi/PublicApi.txt`.

On a review of the commits after your last review, check only those commits and whether your earlier findings are
fixed. Do not repeat a finding that is fixed or that the author answered with a reason.

## Format

- At most 8 findings, most severe first, each in an inline comment on its line that starts with a severity tag:
  - `[blocking]`: the PR must not merge until it is fixed;
  - `[minor]`: an improvement that can wait.
- Post the main comment last, as a new comment with `gh pr comment`, for a requested review too: an agent waits for
  it, and an edit of an earlier comment does not notify it.
- Start the main comment with exactly one line `Verdict: <N> blocking, <M> minor` (`Verdict: 0 blocking, 0 minor`
  without findings): the agent reads it. Put at most 2 short sentences after it.
- Do not praise.
- Do not write the literal trigger phrase (at-sign + "claude"): it starts the workflow again.

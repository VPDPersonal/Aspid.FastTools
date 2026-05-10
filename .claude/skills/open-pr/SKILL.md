---
name: open-pr
description: Open a pull request following Aspid.FastTools project conventions — title format, PULL_REQUEST_TEMPLATE body, label policy, commit-message rules, and scope hygiene. Use this skill whenever the user asks to open / create / draft a PR, when running `@claude` triggers a PR-creation flow, or before invoking `gh pr create` directly.
user-invocable: true
---

Use this skill any time a pull request is being opened in the Aspid.FastTools repository — manual, scripted, or via `@claude` automation.

## Title

- Short imperative sentence describing the change. Aim for under 70 characters.
- No auto-generated branch-name strings. `Claude/add onenable idregistry i bk jl` is **not** acceptable; rewrite it.
- Examples that pass: `Mark IdRegistry cache dirty on OnEnable`, `Fix this.Marker() generator for explicit interfaces, generics and field naming`, `Document PR conventions in CLAUDE.md`.

## Body

Fill out `.github/PULL_REQUEST_TEMPLATE.md` — three sections, all of which may be omitted only when truly empty.

### `## Summary`

- 1–3 bullets covering what changed and why.
- Code snippets are welcome here when they make the change concrete (e.g. the new method body, a small before/after).
- Do not paste a wall of release-notes prose; if the change really is that big, see the *Release-notes mega-PRs* carve-out below.

### `## Notes for review`

- Optional. Use it for trade-offs, risks, things you specifically want eyes on.
- Also use it to flag unrelated noise in the diff (e.g. `.github/` files that already landed on `main` showing up because the branch sits ahead of `Develop`) so reviewers know it is a no-op on merge.
- If there is nothing to say, delete the section — do not leave a placeholder.

### `## Linked issues`

- `Closes #N` for issues this PR fully resolves (auto-closes them on merge).
- `Refs #N` for related issues that the PR does not close.
- Delete the section when nothing applies.

## Labels

Pick from the existing label set; do **not** invent new labels in PRs. The label catalogue is fixed and visible via `gh label list --repo VPDPersonal/Aspid.FastTools`.

| Group | Rule |
|---|---|
| `type: *` | Exactly **one**: `feature`, `fix`, `refactor`, `documentation`, `test`, `chore`, `ci`, `style`, `performance`. |
| `area: *` | One or more for every part of the codebase the PR actually touches: `runtime`, `editor`, `generator`, `samples`. |
| `status: *` | `needs-review` once the PR is ready, `work-in-progress` while still drafting. |
| Special | `breaking-change`, `dependencies`, `needs-changelog` only when literally true. |

## Commit messages

- Short imperative sentences: `Add X`, `Fix Y`, `Mark Z`. Match the headline style already in `git log`.
- **Never** append `Co-Authored-By: Claude …` or any other Claude/Anthropic attribution trailer. Commits are authored as the human user only. This overrides default templates from any skill (e.g. `commit-commands:commit`, `commit-commands:commit-push-pr`).

## Scope

- One logical change per PR.
- If the diff drags in unrelated noise from a base-branch merge, do **not** delete it — just call it out under *Notes for review*.
- Templates (`bug_report.yml`, `feature_request.yml`, `config.yml`, `PULL_REQUEST_TEMPLATE.md`) and CI workflows (`.github/workflows/*.yml`) live on `main`; targeting them at `Develop` directly is wrong unless the change is meant to ride a release merge.

## Release-notes mega-PRs (carve-out)

`Develop` → `main` release cuts (e.g. #8) are **exempt** from the three-section template. Use feature-scoped `###` subsections instead — one per major area (e.g. `### Source generators`, `### ID System`, `### UIToolkit`, `### Layout & rename`, `### Docs & tooling`). Mirror the structure of the most recent release PR.

## Recipe

When invoked, walk through this checklist before reporting "PR opened":

1. **Branch** — confirm the head branch is named per existing patterns (`Feature/<name>`, `chore/<name>`, `claude/<auto>`). If the auto-name is ugly (e.g. `claude/add-onenable-idregistry-IBkJl`), live with it but compensate with a clean PR title.
2. **Diff** — run `git diff <base>...HEAD --stat` and skim what's actually changing. Identify the *one* logical change vs. accidental noise.
3. **Title** — draft per the rules above.
4. **Body** — fill the three template sections. Use `gh pr create --body "$(cat <<'EOF' ... EOF)"` with a HEREDOC; never inline backticks through zsh, they get eaten as command substitution and silently drop content (this happened on issue-comment 4415890039 — see the `gh api -X PATCH` recovery).
5. **Labels** — apply via `--add-label "type: X,area: Y,…"` either at create time or right after.
6. **Closes / Refs** — if any issues are involved, write them in *Linked issues*. Verify with `gh issue list --state open` what is actually linkable.
7. **Verify** — `gh pr view <N> --json title,labels,body` to confirm everything took.

## Common failure modes to avoid

- Empty body. Fix immediately if you see `body=""` on a PR you opened.
- Auto-generated title from branch name (`Claude/add-…`).
- Two `type: *` labels at once (pick one).
- Pasting unredacted Claude-attribution trailers in commit messages.
- Backtick-inside-`gh-pr-comment` shell injection (always heredoc).

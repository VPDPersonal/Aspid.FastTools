---
name: open-pr
description: Open a pull request following Aspid.FastTools project conventions — title format, PULL_REQUEST_TEMPLATE body, base-branch (default `Develop`), label policy with required `type:`/`area:`/`status:` groups, draft + `status: work-in-progress` default, SSH-key per-branch recovery, and commit-message rules. Use this skill whenever the user asks to open / create / draft a PR, when running `@claude` triggers a PR-creation flow, or before invoking `gh pr create` directly.
user-invocable: true
---

# open-pr (Aspid.FastTools overrides)

This skill is the project-scoped variant of `/open-pr`. The full universal procedure lives in `~/.claude/skills/open-pr/SKILL.md`. Follow it, then apply the project-specific rules below — they take precedence on any conflict.

## Title

- Short imperative sentence describing the change. Aim for under 70 characters.
- No auto-generated branch-name strings. `claude/add-onenable-idregistry-IBkJl` is **not** acceptable; rewrite it before reporting "PR opened".
- Examples that pass: `Mark IdRegistry cache dirty on OnEnable`, `Fix this.Marker() generator for explicit interfaces, generics and field naming`, `Document PR conventions in CLAUDE.md`.

## Base branch

The active integration branch is `Develop`; `main` is reserved for release cuts.

- For ordinary feature/fix/refactor PRs, the base is **`Develop`**.
- For release-cut PRs (`Develop` → `main`, mega-PRs), the base is **`main`**.
- Run the universal merge-base detection (see user-level skill). If detection is ambiguous, fall back to `Develop` when HEAD is not `Develop` itself; otherwise fall back to `main`.
- Persist via `git config branch.<name>.aspidBase "<base>"` so the next open/edit on the same branch skips the question.

## Body

Fill out `.github/PULL_REQUEST_TEMPLATE.md` — three sections, omitted only when truly empty.

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

Pick from the existing label set; do **not** invent new labels silently. The label catalogue is visible via `gh label list --repo VPDPersonal/Aspid.FastTools`.

Three groups are required on every PR:

| Group | Values | Rule |
|---|---|---|
| `type: *` | `feature`, `fix`, `refactor`, `documentation`, `test`, `chore`, `ci`, `style`, `performance` | Exactly **one**. |
| `area: *` | `runtime`, `editor`, `generator`, `samples` | **One or more** — one per area the PR actually touches. |
| `status: *` | `work-in-progress`, `needs-review`, `needs-info`, `blocked`, `do-not-merge` | Exactly **one**. Default `work-in-progress`. |

Special labels — `breaking-change`, `dependencies`, `needs-changelog` — only when literally true.

### Draft + status default

Unless the user explicitly says the PR is ready for review (`готово`, `на ревью`, `ready for review`), the PR opens as **draft** with `status: work-in-progress`:

```sh
gh pr create --draft --label "type: <X>,area: <Y>,status: work-in-progress" ...
```

When the user later flips the PR to ready:

```sh
gh pr edit <N> --remove-label "status: work-in-progress" --add-label "status: needs-review"
gh pr ready <N>
```

### Missing-label flow

If for any required group **no** existing label fits, use the universal create-new-label flow from the user-level skill:

- Suggest a name in the project's style — `<group>: <kebab-slug>`.
- Pick a colour from the palette already used in that group. Current palette (from `gh label list`):
  - `type: *` — `#2369B4` (documentation), `#1E783C` (feature), `#A53232` (fix), `#F5B955` (performance), `#646464` (refactor/test/chore/ci/style).
  - `area: *` — `#1E783C` (runtime), `#2369B4` (editor), `#646464` (generator/samples). Reuse a colour or pick from this same palette for new areas.
  - `status: *` — `#F5B955` (in-progress/needs-info), `#2369B4` (needs-review), `#A53232` (blocked), `#000000` (do-not-merge).
- Confirm with the user via `AskUserQuestion` before running `gh label create` — name, colour, description.
- Then `gh pr edit <N> --add-label "<new-label>"`.

## SSH on push

Follow the universal SSH recovery flow from the user-level skill. Notes for this repo:

- Remote is `git@github.com:VPDPersonal/Aspid.FastTools.git`. Default key per `~/.ssh/config` is `~/.ssh/vpd_personal_my_macbook`. If a push fails with `Permission denied (publickey)`, that usually means ssh-agent offered a different key first.
- Always read `git config --get branch.<name>.sshCommand` before any push/fetch on a branch. Apply via `GIT_SSH_COMMAND` when set.

## Commit messages

- Short imperative sentences in **English** (the chat may be in Russian; GitHub content is English per the `feedback_github_content_english` rule): `Add X`, `Fix Y`, `Mark Z`. Match the headline style already in `git log`.
- **Never** append `Co-Authored-By: Claude …` or any other Claude/Anthropic attribution trailer. Commits are authored as the human user only.

## Scope

- One logical change per PR.
- If the diff drags in unrelated noise from a base-branch merge, do **not** delete it — just call it out under *Notes for review*.
- Templates (`bug_report.yml`, `feature_request.yml`, `config.yml`, `PULL_REQUEST_TEMPLATE.md`) and CI workflows (`.github/workflows/*.yml`) live on `main`; targeting them at `Develop` directly is wrong unless the change is meant to ride a release merge.

## Release-notes mega-PRs (carve-out)

`Develop` → `main` release cuts (e.g. #8) are **exempt** from the three-section template. Use feature-scoped `###` subsections instead — one per major area (e.g. `### Source generators`, `### ID System`, `### UIToolkit`, `### Layout & rename`, `### Docs & tooling`). Mirror the structure of the most recent release PR.

## Recipe

When invoked, walk through this checklist before reporting "PR opened":

1. **Branch** — `git rev-parse --abbrev-ref HEAD`. Confirm the head branch is named per existing patterns (`Feature/<name>`, `chore/<name>`, `claude/<auto>`). If the auto-name is ugly, live with it but compensate with a clean title.
2. **Base** — run merge-base detection; fall back to `Develop` (or `main` for release-cut PRs); persist via `branch.<name>.aspidBase`.
3. **Diff** — `git diff <base>...HEAD --stat`; identify the *one* logical change.
4. **Push** — read `branch.<name>.sshCommand`; push with `GIT_SSH_COMMAND` if set. On SSH failure run the SSH recovery flow and persist the chosen key.
5. **Ready-state** — `--draft` + `status: work-in-progress` unless the user explicitly said ready.
6. **Title / Body** — draft per rules above; HEREDOC for the body.
7. **Create** — `gh pr create --base "<base>" [--draft] --title "..." --body "..." --label "type: X,area: Y,status: work-in-progress"`.
8. **Clarify labels** — for any required group still missing, run `AskUserQuestion` and `gh pr edit <N> --add-label "..."`.
9. **Missing-label flow** — for any group where nothing existing fits, propose creating a new label and only run `gh label create` after explicit confirmation.
10. **Verify** — `gh pr view <N> --json title,labels,body,isDraft,baseRefName`. Report URL, base, draft state, labels.

## Common failure modes to avoid

- Empty body. Fix immediately if you see `body=""` on a PR you opened.
- Opening a feature PR against `main` instead of `Develop`.
- Marking a PR ready when the user only asked to "open" it.
- Auto-generated title from branch name (`claude/add-…`).
- Two `type: *` labels at once (pick one).
- Pasting unredacted Claude-attribution trailers in commit messages.
- Creating a label without explicit user confirmation of name + colour.
- Backtick-inside-`gh-pr-comment` shell injection (always HEREDOC).

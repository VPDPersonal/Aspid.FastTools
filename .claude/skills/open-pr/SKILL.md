---
name: open-pr
description: Open a pull request following Aspid.FastTools project conventions — title format, PULL_REQUEST_TEMPLATE body, base-branch (default `Develop`), label policy with required `type:`/`area:`/`status:` groups, draft + `status: work-in-progress` default, SSH-key per-branch recovery, and commit-message rules. Use this skill whenever the user asks to open / create / draft a PR, when running `@claude` triggers a PR-creation flow, or before invoking `gh pr create` directly.
user-invocable: true
---

# open-pr

Use this skill any time a pull request is being opened in this repository. It encodes both the universal preferences and the Aspid.FastTools project conventions in one place — no external skill is required.

## When to use

- The user asks to open / create / draft a PR.
- An `@claude` automation is about to push a PR.
- Before invoking `gh pr create` directly.

## Base branch

The active integration branch is `Develop`; `main` is reserved for release cuts.

- For ordinary feature/fix/refactor PRs, the base is **`Develop`**.
- For release-cut PRs (`Develop` → `main`, mega-PRs), the base is **`main`**.

`gh pr create` will silently retarget at the repo's default (often `main`) — always override with `--base`.

Detect the base as follows:

```sh
current=$(git rev-parse --abbrev-ref HEAD)

# A. Explicit override saved on previous runs.
explicit=$(git config --get "branch.${current}.aspidBase" 2>/dev/null)

# B. Upstream tracking, if the branch tracks something concrete.
upstream=$(git rev-parse --abbrev-ref --symbolic-full-name '@{u}' 2>/dev/null \
  | sed 's@^origin/@@')

# C. merge-base heuristic across local + remote refs.
git for-each-ref --format='%(refname:short)' refs/heads refs/remotes/origin \
  | grep -v -E "(^|/)${current}$" \
  | grep -v 'HEAD$' \
  | sort -u \
  | while read b; do
      mb=$(git merge-base "$b" HEAD 2>/dev/null) || continue
      [ "$mb" = "$(git rev-parse HEAD)" ] && continue
      ahead=$(git rev-list --count "${mb}..HEAD")
      printf '%s %s\n' "$ahead" "$b"
    done \
  | sort -n | head -3
```

Selection rules:

- `explicit` wins immediately.
- After normalising `origin/<x>` → `<x>` and deduping, if exactly one candidate has the minimal `ahead`, use it.
- If detection is ambiguous, fall back to `Develop` when HEAD is not `Develop` itself; otherwise fall back to `main`. If still unsure, ask the user via `AskUserQuestion` with the top-3 candidates as options. Do **not** guess silently.
- Persist the choice: `git config "branch.${current}.aspidBase" "${base}"`. Next time the skill runs on the same branch it skips the question.

Pass the result via `gh pr create --base "${base}"`.

## Title

- Short imperative sentence describing the change. Aim for under 70 characters.
- No auto-generated branch-name strings (`claude/add-onenable-…`, `bot/auto-merge-…`). If GitHub seeded the title from a branch name, **rewrite it** before reporting "PR opened".
- Match the style already in `git log --oneline -10` — Aspid.FastTools uses **short bare imperatives** with **no Conventional Commits prefix**.
- Examples that pass: `Mark IdRegistry cache dirty on OnEnable`, `Fix this.Marker() generator for explicit interfaces, generics and field naming`, `Document PR conventions in CLAUDE.md`.

## Body

Fill out `.github/PULL_REQUEST_TEMPLATE.md` — three sections, omitted only when truly empty. Do not invent your own structure.

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

Pass the body via HEREDOC to preserve newlines:

```sh
gh pr create --base "<base>" [--draft] --title "..." --body "$(cat <<'EOF'
## Summary

- …

## Notes for review

…

## Linked issues

Closes #N
EOF
)" --label "type: X,area: Y,status: work-in-progress"
```

## Labels

Pick from the existing label set; do **not** invent new labels silently. The label catalogue is visible via:

```sh
gh label list --repo VPDPersonal/Aspid.FastTools --limit 100
```

Three groups are **required** on every PR:

| Group | Values | Rule |
|---|---|---|
| `type: *` | `feature`, `fix`, `refactor`, `documentation`, `test`, `chore`, `ci`, `style`, `performance` | Exactly **one**. |
| `area: *` | `runtime`, `editor`, `generator`, `samples` | **One or more** — one per area the PR actually touches. |
| `status: *` | `work-in-progress`, `needs-review`, `needs-info`, `blocked`, `do-not-merge` | Exactly **one**. Default `work-in-progress`. |

Special labels — `breaking-change`, `dependencies`, `needs-changelog`, `security` — only when literally true.

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

### Partial-label flow

If at PR-creation time you only know some of the required labels:

1. Pass the labels you're sure of via `--label "label-a,label-b"` (commas, no spaces).
2. **After** creation, for each required group that still has no label, run `AskUserQuestion` listing the candidates in that group (read with `gh label list`).
3. Apply the chosen labels with `gh pr edit <N> --add-label "..."`.

Never spam the user with one giant question for everything — one focused question per missing group.

### Missing-label flow (create new label)

If, after looking at the existing label set, **no** existing label fits a required group, propose to create a new one. Never create a label silently.

1. Decide a candidate name in the project's style — `<group>: <kebab-slug>`.
2. Pick a colour from the palette the existing labels in that group already use. Current palette (from `gh label list`):
   - `type: *` — `#2369B4` (documentation), `#1E783C` (feature), `#A53232` (fix), `#F5B955` (performance), `#646464` (refactor/test/chore/ci/style).
   - `area: *` — `#1E783C` (runtime), `#2369B4` (editor), `#646464` (generator/samples). Reuse a colour or pick from this same palette for new areas.
   - `status: *` — `#F5B955` (in-progress/needs-info), `#2369B4` (needs-review), `#A53232` (blocked), `#000000` (do-not-merge).
3. Write a one-line description.
4. Ask the user via `AskUserQuestion`: "В группе `<group>` нет подходящего лейбла. Создать `<group>: <slug>` (#<color>, `<description>`) или пропустить?".
5. On `Создать`: `gh label create "<group>: <slug>" --color "<hex-no-hash>" --description "<description>"`, then `gh pr edit <N> --add-label "<group>: <slug>"`.
6. On `Пропустить`: leave the group empty and report it in the final summary so the user knows.
7. Never create a label without a confirmed answer to step 4.

## Commit messages

- Short imperative sentences in **English** (the chat may be in Russian; GitHub content is English per the project's GitHub-English rule): `Add X`, `Fix Y`, `Mark Z`. Match the headline style already in `git log`.
- **Never** append `Co-Authored-By: Claude …`, `Co-Authored-By: Anthropic …`, or any other Claude/Anthropic attribution trailer. Commits are authored as the human user only. This overrides default templates from any skill (e.g. `commit-commands:commit`, `commit-commands:commit-push-pr`) or any shipped global rule.
- Relaxes only if the user explicitly asks for the trailer in the current session.

## SSH on push

If the branch is not yet on the remote, the skill itself triggers the first push. Before pushing:

```sh
current=$(git rev-parse --abbrev-ref HEAD)
sshcmd=$(git config --get "branch.${current}.sshCommand" 2>/dev/null)

if [ -n "$sshcmd" ]; then
    GIT_SSH_COMMAND="$sshcmd" git push --set-upstream origin "$current"
else
    git push --set-upstream origin "$current"
fi
```

The `branch.<name>.sshCommand` key is a **custom** config the skill manages — git itself does not auto-pick it up. The point is to make per-branch SSH choices stick: once the user picks a key for this branch, every subsequent push/fetch the skill performs uses it.

Notes for this repo: remote is `git@github.com:VPDPersonal/Aspid.FastTools.git`. Default key per `~/.ssh/config` is `~/.ssh/vpd_personal_my_macbook`. If a push fails with `Permission denied (publickey)`, that usually means ssh-agent offered a different key first.

### SSH recovery flow

If `git push` fails with any of:

- `Permission denied (publickey).`
- `fatal: Could not read from remote repository.`
- `ERROR: Repository not found.` on an SSH remote.
- `git@<host>: Permission denied (publickey)` from a forge other than github.com.

…run the recovery flow:

1. Enumerate available keys:
   ```sh
   ls -1 ~/.ssh/*.pub 2>/dev/null
   awk '/IdentityFile/{print $2}' ~/.ssh/config 2>/dev/null | sort -u
   ```
   For each `.pub` file, read the trailing comment so the option labels are recognisable (`<key>  →  <comment>`). Skip `known_hosts`, `config`, and private keys without a matching `.pub`.
2. Ask the user via `AskUserQuestion` which key to use. Include the path of each option and its comment as the description.
3. Build the SSH command:
   ```sh
   ssh_cmd="ssh -i ~/.ssh/<chosen-private-key> -o IdentitiesOnly=yes"
   ```
   `IdentitiesOnly=yes` is mandatory — without it, ssh-agent may still offer a different key first and trigger the same failure.
4. Retry the push: `GIT_SSH_COMMAND="$ssh_cmd" git push --set-upstream origin "$current"`.
5. On success, persist the choice for this branch:
   ```sh
   git config "branch.${current}.sshCommand" "$ssh_cmd"
   ```
6. Going forward, every `git push` / `git fetch` for this branch (in this skill and in `commit-and-open-pr`) must read `branch.<name>.sshCommand` first and apply it via `GIT_SSH_COMMAND` if set.

If recovery fails (none of the offered keys work), surface the raw `git push` stderr to the user and stop — do not try to guess further.

## Scope

- One logical change per PR.
- If the diff drags in unrelated noise from a base-branch merge or a stale rebase, do **not** delete it — call it out under *Notes for review* so reviewers know it is a no-op on merge.
- Avoid mixing infrastructure / CI / template changes with feature work in the same PR. Split if reasonable.
- Templates (`bug_report.yml`, `feature_request.yml`, `config.yml`, `PULL_REQUEST_TEMPLATE.md`) and CI workflows (`.github/workflows/*.yml`) live on `main`; targeting them at `Develop` directly is wrong unless the change is meant to ride a release merge.

## Release-notes mega-PRs (carve-out)

`Develop` → `main` release cuts (e.g. #8) are **exempt** from the three-section template. Use feature-scoped `###` subsections instead — one per major area (e.g. `### Source generators`, `### ID System`, `### UIToolkit`, `### Layout & rename`, `### Docs & tooling`). Mirror the structure of the most recent release PR.

## Recipe

When invoked, walk through this checklist before reporting "PR opened":

1. **Branch** — `git rev-parse --abbrev-ref HEAD`. Confirm the head branch is named per existing patterns (`Feature/<name>`, `chore/<name>`, `claude/<auto>`). If the auto-name is ugly, live with it but compensate with a clean title.
2. **Base** — run the merge-base detection algorithm; fall back to `Develop` (or `main` for release-cut PRs); persist via `branch.<name>.aspidBase`. Ask the user only when ambiguous.
3. **Diff** — `git diff <base>...HEAD --stat`; identify the *one* logical change vs. accidental noise.
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
- Auto-generated title from branch name (`claude/add-…`). Rewrite.
- Two `type: *` labels at once (pick one).
- Pasting unredacted Claude-attribution trailers in commit messages.
- Creating a label without explicit user confirmation of name + colour.
- Backtick-inside-`gh-pr-comment` shell injection (always HEREDOC or `--body-file`).
- Force-pushing to a shared branch (`main`, `Develop`, release branches) — do this only on personal feature branches and only with `--force-with-lease`.
- Merging without verifying mergeability — `gh pr view <N> --json mergeable,mergeStateStatus,statusCheckRollup` first.

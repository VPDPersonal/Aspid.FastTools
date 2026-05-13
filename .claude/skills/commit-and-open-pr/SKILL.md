---
name: commit-and-open-pr
description: One-shot orchestrator that commits the current session's changes and then opens a pull request in the Aspid.FastTools repo. Delegates the commit step to the project `commit` skill (Samples~ caveats, generator-DLL handling, English-only content, short bare imperative titles) and the PR step to the project `open-pr` skill (base `Develop`, draft + `status: work-in-progress` default, required `type:`/`area:`/`status:` labels, SSH recovery per branch). Use whenever the user asks to "commit and open PR", "закоммить и открой пр", "commit + PR", `/commit-and-open-pr`, or otherwise wants a single command that goes from working tree to published PR.
user-invocable: true
---

# commit-and-open-pr

This is a **composite** skill. It does not re-implement commit-message logic or PR-creation logic — it sequences the project's `commit` and `open-pr` skills with one consistent push step in between.

## When to use

- The user types `/commit-and-open-pr`.
- The user says "commit and open a PR", "закоммить и открой пр", "commit + push + PR", or any natural-language equivalent.
- Do **not** invoke when the user only said "commit" (use `commit`) or only "open a PR" (use `open-pr`).
- Do **not** confuse with the built-in `commit-commands:commit-push-pr` plugin skill — that one has its own contract; this orchestrator follows the project conventions (no Co-Authored-By trailer, per-branch SSH, draft-by-default, English-only GitHub content, base `Develop`, etc.).

## Process

Run the steps in order. Stop at the first hard failure; do not press on.

### 1. Pre-flight

```sh
git status --porcelain
git diff --stat
git diff --staged --stat
git rev-parse --abbrev-ref HEAD
```

- If the working tree is **completely** clean (no staged, no unstaged, no untracked in the session's `IN_CONTEXT`) and the branch is already pushed and a PR already exists, ask the user what they wanted — probably an `open-pr` update or `gh pr ready`.
- If there is **no** staged content but there are unstaged session-edits, `commit` will pick them up automatically (it inspects both index and working tree). Do not pre-`git add` anything yourself.
- If the diff mixes several unrelated logical changes (e.g. a fix + an unrelated refactor + a docs tweak), surface it the same way `commit` would — ask the user via `AskUserQuestion` whether to combine, split into several PRs, or commit only one group. Splitting "commit one logical change → push → open PR" repeatedly across multiple PRs is acceptable in a single invocation when the user says so.

Project-specific pre-flight checks (on top of the universal ones):

- **`Samples~/`.** Files under `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Samples~/` routinely appear / disappear because Unity Package Manager imports samples into the working tree. They are local artefacts. The `commit` skill already filters them out unless the user said the commit is about samples — trust that filter; do not pre-stage `Samples~/*` here.
- **Generator DLL.** If `git status` shows changes to `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Aspid.FastTools.Generators.dll` but **no** `*.cs` change under `Aspid.FastTools.Generators/Aspid.FastTools.Generators/`, that means a previous unrelated generator edit deployed the DLL. Surface this to the user before committing — usually the right call is to leave the DLL out of this PR.
- **README sync.** If the diff touches a feature that has a page in `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Documentation/EN/` or `…/RU/` (e.g. namespace rename, public API change, new `CreateAssetMenu` path) and the four README files (`README.md`, `README_RU.md`, `Documentation/EN/README.md`, `Documentation/RU/README.md`) are unchanged, run `AskUserQuestion` offering "запустить /sync-readmes сейчас?". If the user agrees, invoke the `sync-readmes` skill before continuing.

### 2. Commit

Delegate to the project `commit` skill (`.claude/skills/commit/SKILL.md`). Pass through the user's session context. The composite skill does **not**:

- compose its own commit title/body;
- bypass `commit`'s "ask before mixing logical groups" gate;
- `git add -A` or `git add .`.

Project-specific reminders that `commit` already enforces but worth repeating here:

- Title style: short bare imperative, no Conventional Commits prefix, under 70 characters.
- Content language: English, even when the chat is in Russian.
- No `Samples~/` paths and no orphan generator-DLL deploy in the commit.
- No `Co-Authored-By: Claude …` / `Anthropic …` trailers.

After `commit` returns, capture the new SHA: `git rev-parse HEAD`.

### 3. Push

This is the only step the composite skill performs itself.

```sh
current=$(git rev-parse --abbrev-ref HEAD)
sshcmd=$(git config --get "branch.${current}.sshCommand" 2>/dev/null)

if [ -n "$sshcmd" ]; then
    GIT_SSH_COMMAND="$sshcmd" git push --set-upstream origin "$current"
else
    git push --set-upstream origin "$current"
fi
```

Persisted `branch.<name>.sshCommand` for this repo is typically `ssh -i ~/.ssh/vpd_personal_my_macbook -o IdentitiesOnly=yes` (matches `~/.ssh/config` `IdentityFile` for `github.com`). If recovery picks a different key for a specific branch (e.g. a fork or co-authored remote), persist it per the recovery flow — do not change the global `~/.ssh/config`.

On failure with an SSH error (`Permission denied (publickey)`, `Could not read from remote repository`, `Repository not found.` on an SSH remote), run the SSH recovery flow:

1. Enumerate available keys:
   ```sh
   ls -1 ~/.ssh/*.pub 2>/dev/null
   awk '/IdentityFile/{print $2}' ~/.ssh/config 2>/dev/null | sort -u
   ```
   For each `.pub` file, read the trailing comment so option labels are recognisable.
2. `AskUserQuestion` — which key to use.
3. Build `ssh_cmd="ssh -i ~/.ssh/<chosen-private-key> -o IdentitiesOnly=yes"` (`IdentitiesOnly=yes` is mandatory).
4. Retry: `GIT_SSH_COMMAND="$ssh_cmd" git push --set-upstream origin "$current"`.
5. On success, persist: `git config "branch.${current}.sshCommand" "$ssh_cmd"`.

On any non-SSH push failure (rejected, non-fast-forward, hook failure), surface the raw stderr and stop. Do not auto-`--force` / `--force-with-lease`.

### 4. Open PR

Delegate to the project `open-pr` skill (`.claude/skills/open-pr/SKILL.md`). The composite skill does **not**:

- guess a base branch — `open-pr` runs its own merge-base detection;
- decide draft / non-draft — `open-pr` defaults to draft + `status: work-in-progress` unless the user said the PR is ready for review;
- pick or invent labels — `open-pr` runs the partial-label and missing-label-creation flows.

Pass through any signals the user gave in the original message (e.g. "ready for review" → tells `open-pr` to skip `--draft` and use `status: needs-review`).

Project-specific reminders that `open-pr` already enforces:

- Base defaults to `Develop` (or `main` for release-cut PRs).
- Required labels: exactly one `type: *`, ≥1 `area: *` (`runtime` / `editor` / `generator` / `samples`), one `status: *` (default `work-in-progress`).
- Default is `--draft` + `status: work-in-progress` unless the user said the PR is ready.
- Missing-label flow: propose creating new labels via `gh label create`, but only after user confirmation of name + colour + description. Reuse the palette listed in the project `open-pr` skill.

### 5. Report

One final summary suitable for skim:

- New commit SHA(s) and titles.
- PR URL.
- Base branch the PR targets.
- Draft state (`draft` / `ready`).
- Final label list as shown by `gh pr view <N> --json labels`.
- Any group still missing a label (because the user declined the missing-label flow).
- SSH key used, if recovery happened (so the user knows what got persisted to `branch.<name>.sshCommand`).
- Whether the generator DLL was part of the commit (`yes` / `no — kept out`).
- Whether the README sync prompt was offered and what the user picked.

## Hard rules

Inherit and respect:

- All hard rules from the project `commit` skill (no `-A` staging, no `--amend`, no `--no-verify`, no Claude/Anthropic Co-Authored-By trailer, no secret-file commits, no `Samples~/` paths, no orphan generator-DLL deploy).
- All hard rules from the project `open-pr` skill (HEREDOC body, no auto-generated titles, no invented labels without confirmation, correct base via merge-base or persisted `branch.<name>.aspidBase`).

Plus this composite's own:

- **Never push before `commit` has finished cleanly.** A failed commit aborts the pipeline; do not try to "push what's already there" as a recovery.
- **Never open a PR before `git push` has succeeded.** `gh pr create` against an unpushed branch fails confusingly.
- **Never `--force` push** as part of this skill. If the push is rejected, stop and ask.
- **Never bypass either delegated skill's "ask" gates** by inlining the answer yourself. If `commit` would ask "which group?" let it ask.
- **English on GitHub.** Commit messages, PR title, PR body, PR labels — all English. The chat may be in Russian; persisted GitHub content is not.
- **Never include `Samples~/` paths in a feature PR** unless the user explicitly opted in.
- **Never run `dotnet build` manually here.** The PostToolUse hook `.claude/hooks/rebuild-generators-on-change.sh` already rebuilds the generator DLL on every Edit/Write to `*.cs` under `Aspid.FastTools.Generators/Aspid.FastTools.Generators/`. If a generator-source edit happened in this session, the DLL deploy is already done — verify with `git status`, do not duplicate it.
- **README sync** is a separate skill (`sync-readmes`). Do not inline README updates into this skill.

## Common failure modes to avoid

- Composing the commit message inside this skill instead of delegating to `commit`.
- Pushing before the commit lands (especially on a fresh `commit` failure).
- Opening the PR against the repo default branch (`main`) when the head branch was forked from `Develop`.
- Forgetting to apply the persisted `branch.<name>.sshCommand` and getting a `Permission denied` on the second invocation.
- Treating a partial label set as "good enough" and closing out without asking the user about the missing groups.
- Reporting "PR opened" when only `gh pr create` ran but the post-creation label/draft fix-ups never happened.
- Writing PR title / body / commit message in Russian instead of English.

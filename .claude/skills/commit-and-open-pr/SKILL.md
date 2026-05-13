---
name: commit-and-open-pr
description: Aspid.FastTools-specific overrides for `/commit-and-open-pr` — Samples~ caveats, generator-DLL handling, README-sync prompt, base-branch default `Develop`, English-only PR/commit content, and SSH-key persistence per branch. Inherits the universal recipe from `~/.claude/skills/commit-and-open-pr/SKILL.md`. Use whenever the user asks to "commit and open PR", "закоммить и открой пр", `/commit-and-open-pr`, or otherwise wants the one-shot commit-push-PR flow inside this repo.
user-invocable: true
---

# commit-and-open-pr (Aspid.FastTools overrides)

This is the project-scoped variant. The full universal procedure lives in `~/.claude/skills/commit-and-open-pr/SKILL.md`. Follow it, then apply the project-specific rules below — they take precedence on any conflict.

## Pre-flight

In addition to the universal pre-flight (`git status --porcelain`, `git diff --stat`, `git diff --staged --stat`):

- **Samples~/.** Files under `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Samples~/` routinely appear / disappear because Unity Package Manager imports samples into the working tree. They are local artefacts. The `commit` skill already filters them out unless the user said the commit is about samples — trust that filter; do not pre-stage `Samples~/*` here.
- **Generator DLL.** If `git status` shows changes to `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Aspid.FastTools.Generators.dll` but **no** `*.cs` change under `Aspid.FastTools.Generators/Aspid.FastTools.Generators/`, that means a previous unrelated generator edit deployed the DLL. Surface this to the user before committing — usually the right call is to leave the DLL out of this PR.
- **README sync.** If the diff touches a feature that has a page in `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Documentation/EN/` or `…/RU/` (e.g. namespace rename, public API change, new `CreateAssetMenu` path) and the four README files are unchanged, run `AskUserQuestion` offering "запустить /sync-readmes сейчас?". If the user agrees, invoke the `sync-readmes` skill before continuing.

## Commit

Delegate to the project-scoped `commit` skill — title style "short bare imperatives, no Conventional Commits prefix", **English-only** content (even when chat is in Russian, per `feedback_github_content_english`), Samples~ caveat, no Claude/Anthropic Co-Authored-By.

## Push

Universal push step. Persisted `branch.<name>.sshCommand` for this repo is typically `ssh -i ~/.ssh/vpd_personal_my_macbook -o IdentitiesOnly=yes` (matches `~/.ssh/config` `IdentityFile` for `github.com`). If recovery picks a different key for a specific branch (e.g. a fork or co-authored remote), persist it per the universal flow — do not change the global `~/.ssh/config`.

## Open PR

Delegate to the project-scoped `open-pr` skill:

- Base defaults to `Develop` (or `main` for release-cut PRs).
- Required labels: exactly one `type: *`, ≥1 `area: *` (`runtime` / `editor` / `generator` / `samples`), one `status: *` (default `work-in-progress`).
- Default is `--draft` + `status: work-in-progress` unless the user said the PR is ready.
- Missing-label flow: propose creating new labels via `gh label create`, but only after user confirmation of name + colour + description. Reuse the palette listed in the project `open-pr` skill.

## Report

In addition to the universal report fields, include:

- Whether the generator DLL was part of the commit (`yes` / `no — kept out`).
- Whether the README sync prompt was offered and what the user picked.
- The persisted `branch.<name>.sshCommand` if any new value was written.

## Hard rules (project additions)

- **English on GitHub.** Commit messages, PR title, PR body, PR labels — all English. The chat may be in Russian; persisted GitHub content is not.
- **Never include `Samples~/` paths in a feature PR** unless the user explicitly opted in.
- **Never run `dotnet build` manually here.** The PostToolUse hook `.claude/hooks/rebuild-generators-on-change.sh` already rebuilds the generator DLL on every Edit/Write to `*.cs` under `Aspid.FastTools.Generators/Aspid.FastTools.Generators/`. If a generator-source edit happened in this session, the DLL deploy is already done — verify with `git status`, do not duplicate it.
- **README sync** is a separate skill (`sync-readmes`). Do not inline README updates into this skill.

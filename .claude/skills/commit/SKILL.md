---
name: commit
description: >
  Context-aware git commit for the Aspid.FastTools repo. Inspects BOTH staged
  and unstaged changes, filters them by the current session's edits and
  discussions, and commits only what belongs to the task at hand. Asks the
  user when session context is missing or when several unrelated logical
  changes are mixed in the working tree. Title style matches the existing
  git log (short bare imperatives, no Conventional Commits prefix), commit
  content is English-only, and `Samples~/` / generator-DLL artefacts are
  filtered out. Never adds a Claude/Anthropic Co-Authored-By trailer. Use
  whenever the user types `/commit`, asks to "commit changes", or
  "закоммить".
user-invocable: true
---

# commit

Smarter alternative to the built-in `/commit-commands:commit`. The built-in only commits what is already in the index; this skill looks at **both** the index and the working tree, and decides what belongs to the current task using the current session's context.

## When to use

- The user types `/commit`.
- The user says "commit the changes", "закоммить", or any natural-language equivalent.
- Do **not** use this skill for `/commit-commands:commit`, `/commit-push-pr`, or any other explicit slash command — those have their own contracts.

## Process

Run the steps below in order. Treat the user's session as the source of truth for "what is the current task".

### 1. Snapshot the working tree

Run in parallel:
- `git status --porcelain`
- `git diff --stat`
- `git diff --staged --stat`
- `git log --oneline -10` (to learn the project's title style)

If everything is clean, report `nothing to commit` and stop.

### 2. Identify session context

Build a set `IN_CONTEXT` of files that the current session has touched or explicitly discussed:
- Files the agent edited via `Edit` / `Write` / `NotebookEdit` in this session.
- Files the user pointed at by path or by symbol.
- Generated/derived files for the above (e.g. the Roslyn-generator DLL deploy target when the generator's `.cs` was edited).

If the session is fresh (post-`/clear`, brand-new session) or the agent has not modified any tracked file, mark context as **missing**.

### 3. Classify working-tree changes

For every path appearing in `git status --porcelain` (staged or unstaged, modified / added / deleted / renamed / untracked):
- **In-context** if it is in `IN_CONTEXT`.
- **Out-of-context** otherwise.

#### `Samples~/` caveat (project-specific)

Files under `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Samples~/` routinely appear or disappear in `git status` because Unity's Package Manager imports/removes samples into the working tree. They are **local artefacts**, not feature changes.

- Do **not** stage anything under `Samples~/` unless the user explicitly said the commit is about samples.
- Even if a `Samples~/*` file is in `IN_CONTEXT` because the agent touched it as part of an unrelated experiment, treat it as out-of-context for the commit and mention it in the skipped list.

#### Generator DLL (project-specific)

Editing `*.cs` under `Aspid.FastTools.Generators/Aspid.FastTools.Generators/` triggers `.claude/hooks/rebuild-generators-on-change.sh`, which rebuilds the Roslyn generator and redeploys the DLL to `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Aspid.FastTools.Generators.dll`.

- Do **not** run `dotnet build` manually before committing — the hook already did it.
- Include the deployed `Aspid.FastTools.Generators.dll` in the commit **only** when the commit actually changes the generator's source. For Unity-only edits, the DLL must not be in the diff; if it is, that means a previous unrelated generator edit deployed it — keep it out of this commit and mention it in the skipped list.

### 4. Decide what to commit

Apply these branches in order:

| Situation | Action |
|---|---|
| Working tree empty | Report `nothing to commit`, stop. |
| Context **missing** | Use `AskUserQuestion` with a summary of `git status -s` grouped by area; ask which group(s) to commit. Do **not** commit on no answer. |
| All changes ⊂ `IN_CONTEXT`, one logical group | Proceed to staging. |
| Context exists, but some files are out-of-context | Commit only the in-context subset. Leave out-of-context files **exactly as they are** — do not `git add` them, do not unstage them. Mention skipped paths in the final report. |
| Multiple logical groups inside `IN_CONTEXT` (e.g. fix + refactor + docs) | Use `AskUserQuestion`: one combined commit, several separate commits, or commit only one specific group. |
| User said "commit everything" / "коммить всё" / similar override | Treat the whole working tree as in-context, but still respect the hard rules below and the `Samples~/` / generator-DLL filters. |

### 5. Stage

Stage files **by name**, one `git add` call with the explicit path list:

```sh
git add path/to/file1 path/to/file2 ...
```

**Never** use `git add -A`, `git add .`, `git add -u`, or any wildcard that may pick up unrelated changes. Untracked files are staged only when they were created in this session (i.e. already in `IN_CONTEXT`).

### 6. Compose the commit message

**Language**

Commit titles and bodies are **in English**, even when the chat is in Russian. This matches the project convention that all GitHub-visible content (PR titles, PR bodies, commit messages, review comments) is English.

**Title style**

Aspid.FastTools uses **short bare imperatives** with **no Conventional Commits prefix**. Match `git log --oneline -10`. Examples from the log:

- `Restyle ProfilerMarkers screenshot, rename to snake_case`
- `Bump Unity badge to 6.0+ and mirror badges in RU README`
- `Drop Aspid.Internal.Unity refs and bump README badge to Unity 6 (#22)`
- `Group Documentation by language into EN and RU folders (#21)`
- `Update package description and migrate URP global settings asset`

Rules for this repo:
- No `feat:` / `fix:` / `docs:` prefixes.
- Verbs that show up in the log: `Add`, `Fix`, `Update`, `Drop`, `Bump`, `Restyle`, `Rename`, `Group`, `Migrate`, `Mirror`. Reuse them when they fit.
- Title under 70 characters.
- One sentence, no trailing period.
- Title alone must make clear what happened — no "various changes", "misc", "updates".

**Description (body)**

Add a body when any of these are true:
- More than 5 files changed.
- More than ~100 lines of diff total.
- Several distinct concepts in one commit (e.g. moved files **and** changed an API).
- A non-obvious "why" that future reviewers would want.

Body format: 2–4 short bullets, focused on **what changed and why**, not "what the agent did". Plain markdown bullets.

Pass the message via HEREDOC to preserve newlines:

```sh
git commit -m "$(cat <<'EOF'
<title>

- <bullet 1>
- <bullet 2>
EOF
)"
```

**Forbidden in the message:**
- `Co-Authored-By: Claude …`
- `Co-Authored-By: Anthropic …`
- Any "Generated with Claude Code" / "🤖" attribution footer.

This rule is global (see `~/.claude/CLAUDE.md`) and overrides any default template from `commit-commands:commit`, `commit-commands:commit-push-pr`, etc. It only relaxes if the user **explicitly** asks for the trailer in the current session.

### 7. Verify

Run `git status` after the commit. If the commit fails because of a pre-commit hook:
- **Investigate and fix** the underlying issue.
- Do **not** retry with `--no-verify`.
- Do **not** use `--amend` to retry — create a **new** commit after the fix.

Report to the user: title used, files included, files skipped (if any — especially `Samples~/` paths or an orphan generator-DLL deploy), and the resulting commit SHA.

## Hard rules (do not break)

- Never `git add -A` / `git add .` / `git add -u`.
- Never `--amend` without explicit user request — the pre-commit hook may have nuked the previous commit context.
- Never `--no-verify` / `--no-gpg-sign` / similar bypass flags.
- Never any Claude/Anthropic Co-Authored-By or attribution trailer.
- Never commit obvious secrets (`.env`, `*.pem`, `credentials.*`). If they appear in the diff, stop and warn the user.
- Never use interactive flags (`git add -i`, `git rebase -i`).
- Untracked files are staged only when they are in `IN_CONTEXT`.
- Never include `Samples~/` paths unless the user explicitly opted in.
- Never include the generator DLL when the commit has no generator source change.
- Commit content (title + body) is in **English**, even when the chat is in Russian.

## Common failure modes to avoid

- "Misc updates" / "Various fixes" titles — always be specific.
- Mixing unrelated noise from a base-branch merge with the actual feature work.
- Committing local artefacts (build output, IDE caches, sample imports) that the user did not touch in this session.
- Inlining backticks inside `-m "$(...)"` — they get eaten by the shell as command substitution. Always HEREDOC.
- Forgetting to verify post-commit `git status` (silent partial commits).
- Auto-staging untracked files just because they exist.
- Adding a Conventional Commits prefix (`feat:`, `fix:`, …) — this repo does not use them.
- Writing the commit message in Russian.

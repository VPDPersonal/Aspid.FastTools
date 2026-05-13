---
name: commit
description: >
  Aspid.FastTools-specific overrides for the /commit skill — title style
  matching this repo's git log, Samples~ caveats, generator-DLL rebuild
  note, and English-only commit content. Inherits the universal recipe
  from `~/.claude/skills/commit/SKILL.md`. Use whenever the user types
  `/commit`, asks to "commit changes", or "закоммить" inside this repo.
user-invocable: true
---

# commit (Aspid.FastTools overrides)

This is the project-scoped variant of the `/commit` skill. The full procedure lives in `~/.claude/skills/commit/SKILL.md`. Follow it, then apply the project-specific rules below — they take precedence on any conflict.

## Title style

Match `git log --oneline -10`. Aspid.FastTools uses **short bare imperatives** with **no Conventional Commits prefix**. Examples from the current log:

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

## Language

Commit titles and bodies are **in English**, even when the chat is in Russian. This matches `feedback_github_content_english` for everything that lands on GitHub (PR titles, PR bodies, commit messages, review comments).

## `Samples~/` caveat

Files under `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Samples~/` routinely appear or disappear in `git status` because Unity's Package Manager imports/removes samples into the working tree. They are **local artefacts**, not feature changes.

- Do **not** stage anything under `Samples~/` unless the user explicitly said the commit is about samples.
- Even if a `Samples~/*` file is in `IN_CONTEXT` because the agent touched it as part of an unrelated experiment, treat it as out-of-context for the commit and mention it in the skipped list.

(At the time of writing, the working tree on `docs/profiler-markers-screenshot` carries dozens of deleted `Samples~/EnumValues/...` and `Samples~/Ids/...` entries that must stay out of commits on this branch.)

## Generators DLL

Editing `*.cs` under `Aspid.FastTools.Generators/Aspid.FastTools.Generators/` triggers `.claude/hooks/rebuild-generators-on-change.sh`, which rebuilds the Roslyn generator and redeploys the DLL to `Aspid.FastTools/Assets/Plugins/Aspid/FastTools/Aspid.FastTools.Generators.dll`.

- Do **not** run `dotnet build` manually before committing — the hook already did it.
- Include the deployed `Aspid.FastTools.Generators.dll` in the commit **only** when the commit actually changes the generator's source. For Unity-only edits, the DLL must not be in the diff; if it is, that means a previous unrelated generator edit deployed it — keep it out of this commit.

## Co-Authored-By

Do not append `Co-Authored-By: Claude …` or any other Claude/Anthropic attribution to commit messages. This is a global rule (`~/.claude/CLAUDE.md`) but it is critical for this project and repeated here for emphasis.

## Pre-commit hook

If a pre-commit hook fails, fix the underlying issue and create a **new** commit. Do not retry with `--amend` or `--no-verify`.

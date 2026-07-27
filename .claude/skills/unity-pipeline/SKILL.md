---
name: unity-pipeline
description: Drive this repo's running Unity Editor from the shell via the official `unity` CLI and the `com.unity.pipeline` package — recompile/test loop, C# eval, console logs, Game/Scene screenshots, the `sr_gate` SerializeReference check, and authoring new `[CliCommand]` commands. Use whenever a task needs to compile, test, inspect, or script the live Editor instead of inferring behaviour from source.
user-invocable: false
allowed-tools:
  - Bash
---

# Unity Pipeline (live Editor control)

Stack: `unity` CLI (`~/.unity/bin/unity`, normally already on `PATH`) plus the UPM package
`com.unity.pipeline`, pinned in `Aspid.FastTools/Packages/manifest.json`. The Editor exposes
~460 commands; the CLI just forwards to them, so no CLI update is needed when the package adds one.

Generic CLI surface (editors, licenses, headless `build`/`test`, `mcp`) is covered by the vendor
`unity-cli` skill. **This file covers only what is specific to Aspid.FastTools.**

## 1. Always target a project explicitly

Several Editors run at once here — the main checkout plus `.claude/worktrees/shared-*`. A bare
`unity command …` fails with `COMMAND_FAILED / Multiple Unity Editor instances found`.

```bash
P="$PWD/Aspid.FastTools"            # from the repo (or worktree) root
unity command <name> --project-path "$P" --format json
```

## 2. Health check — `status`, not `pipeline list`

```bash
unity status --format json          # port, project, version, PID, state per Editor
```

Reads the per-Editor lockfile instead of probing over HTTP: faster, and stale instances are
reported as `unreachable`. `unity pipeline list` is the wrong tool for this — a *closed* Editor
can still show `Running=true` there from a leftover lock. Use `pipeline list` only for package
install/version questions.

`unity list --project-path "$P"` prints every command the Editor exposes with its parameter
schema — use it instead of guessing argument names.

## 3. Edit → verify loop

```bash
unity command set_autotick --enable true --project-path "$P"   # do this first
unity command recompile --project-path "$P"
unity command recompile_status --project-path "$P"             # poll: idle|triggered|compiling|completed|up_to_date
unity command run_tests --mode editor --filter <Fixture> --project-path "$P"
```

- **`set_autotick` first.** Unfocused Unity throttles its update loop, so a recompile or test run
  started from the shell can hang indefinitely without it.
- **The server goes silent during domain reload** — commands return empty. Poll with retries; the
  connection restores itself afterwards (no reinstall needed).
- `run_tests` right after another command can return `result=null` (runner still busy) — retry.
- Long runs: `--async_tests true`, then poll `test_status`; abort with `cancel_tests`.

## 4. C# eval

```bash
unity command eval 'return UnityEditor.EditorUtility.scriptCompilationFailed ? "COMPILE_FAILED" : "OK";' \
  --project-path "$P" --format json
```

- **The trailing `;` is mandatory** — without it the snippet fails to compile with `CS1002`.
- **The JSON response echoes your source back** inside `parameters`. Grep only the result field
  (`grep -E '"result": *"[^"]*"'`), otherwise a string literal in your own code matches and, for
  the snippet above, "COMPILE_FAILED" is reported even on success.
- `eval_file <path>` runs a `.cs` file through the same path.

Compilation ground truth is `scriptCompilationFailed`, not the absence of console errors.

## 5. Console and screenshots

```bash
unity command get_console_logs --severity error --limit 50 --project-path "$P"
unity command screenshot --view game --output ./shot.png --project-path "$P"
```

`severity`: `all | log | warning | error`. Without `--output`, `screenshot` (and
`capture_game_view` / `capture_scene_view`) writes a timestamped PNG under
`<project>/Temp/pipeline-screenshots/`.

For docs media beyond a plain Game/Scene frame — floating Inspectors, `TypeSelectorWindow`
dropdowns, GIFs — use the `editor-media-capture` skill instead; those need window-ID capture and
focus tricks the pipeline commands do not cover.

## 6. `sr_gate` — SerializeReference gate without a batchmode relaunch

```bash
unity command sr_gate --scope full --project-path "$P" --format json
```

| Arg | Values | Default |
|---|---|---|
| `scope` | `missing` \| `required` \| `full` | `missing` |
| `warn_only` | force Warn severity — reports, `exitCode` 0 | `false` |
| `fail` | force Fail severity — `exitCode` 1 on violations | `false` |

Returns `{success, scope, severity, violationCount, exitCode, violations[]}`, each violation
carrying `kind`, `assetPath`, `fieldPath`, `storedType`, `fileId`, `rid`. Source:
`Aspid.FastTools/Assets/DevTests/CliCommands/Editor/SerializeReferenceGateCommands.cs` — a thin
wrapper over `SerializeReferenceGateScanner`, so it reflects the same rules as the CI gate.

## 7. Adding a command

```csharp
using Unity.Pipeline.Commands;

internal static class MyCommands
{
    [CliCommand("my_command", "What this does")]   // MainThreadRequired=true by default
    internal static object Run(
        [CliArg("text", "Input", Required = true)] string text,
        [CliArg("count", "Repeat count")] int count = 1)
        => new { echoed = text, count };            // any object is serialised into `result`
}
```

- `[CliArg]` is optional (the C# parameter name is used); defaults come from the C# default value.
- Mutating commands take `confirm` + `dry_run` parameters by convention. Multi-field input goes
  through a class implementing `IStructuredCommandInput`.
- `MainThreadRequired=false` only for thread-safe read-only work; `RuntimeOnly=true` marks a
  Player-runtime command (hidden from the Editor listing).
- Discovery is via `TypeCache` — **a new command appears only after a recompile**.

**Placement rule:** never inside `Packages/tech.aspid.fasttools` — it would ship to users and drag
the experimental `com.unity.pipeline` dependency with it. Put it in
`Aspid.FastTools/Assets/DevTests/CliCommands/Editor/` (asmdef
`Aspid.FastTools.DevTests.CliCommands.Editor`, referencing `Unity.Pipeline` plus the package
assemblies it needs). Package internals are already opened to it via `InternalsVisibleTo`.

## Maintenance

```bash
unity upgrade --channel beta --check      # CLI; --rollback restores the previous binary
unity pipeline list-versions --format json # package versions in the registry
unity pipeline upgrade --project-path "$P" # bumps manifest.json — a committed file, so review the diff
```

The package is experimental (`*-exp.*`); treat a version bump as a real change, not a chore.

---
name: unity-pipeline
description: Drive a running Unity Editor or development Player from the command line via the unity-pipeline package — install the package, keep the editor ticking while unfocused, run the edit→recompile→run_tests loop, evaluate C#, and hot-reload files at runtime. Use when an agent needs to control a live Unity instance, automate Unity tests, recompile scripts headlessly, or apply runtime hot reloads. Assumes the `unity` CLI is on PATH.
---

# Unity Pipeline (agent control)

Invoke commands with `unity command <name> [args]`. Run `unity command` with no name to
list what an instance exposes. Two servers exist: **Editor** (`7800-7849`, auto-starts with
the editor) and **Runtime** (`7900-7949`, only in a dev Player build). See the package
`README.md` "Commands" reference for the full list and parameters.

## 1. Install & verify

```bash
unity pipeline install            # install into current project (or --project-path)
unity pipeline list               # confirm the editor instance + server are reachable
unity command editor_status       # confirm the editor server answers
```

## 2. Autonomous edit loop (Editor)

This is the core agent workflow: keep the editor alive, change code, recompile, test.

```bash
# 1. Keep the editor ticking even when unfocused/minimized. REQUIRED before headless work —
#    Unity otherwise throttles or stalls update/compile when it isn't the active app.
unity command set_autotick --enable true

# 2. Edit C# source files on disk normally.

# 3. Recompile (async: triggers a domain reload, then poll until done).
unity command recompile
unity command recompile_status    # repeat until "completed" or "up_to_date"
#   Tolerate connection errors while the domain reload is in flight — that is expected.
#   If recompile_status reports failed=true, read its "errors" array and fix before testing.

# 4. (Optional) List available tests without running them.
unity command list_tests --mode editor            # mode: all | editor | playmode

# 5. Run tests (filter to keep it fast).
unity command run_tests --mode editor --filter MyFixture.MyTest
```

`run_tests` modes: `all` | `editor` | `playmode`. `filter_type`: `testName` | `assembly` |
`category`. For long runs use `--async_tests true` and poll `unity command test_status`
(abort with `unity command cancel_tests`).

> **Known caveat:** when any selected test *fails*, `run_tests` may surface an opaque
> result instead of the failure details. Re-run a narrower `--filter`, or inspect the
> editor's Test Runner / logs to get the real failure.

## 3. Runtime hot reload

Change gameplay code in a **running** game with no domain reload. The game must be live:
enter Editor Play Mode (`unity command editor_play`) or run a dev Player. A
`RuntimePipelineManager` in the scene auto-discovers tagged methods on `Awake` (no manual
registration). Mono only — Editor Play Mode and Mono desktop dev builds, not IL2CPP. The token
is auto-injected for local requests.

### Prefer: in-place — `reload_file`

Edit the method body directly; no separate file, no boilerplate.

1. Tag the method `[HotReload]` on the MonoBehaviour.
2. Edit its body on disk.
3. Apply (re-run to iterate):
   ```bash
   unity command reload_file --filename Assets/Spinner.cs
   ```
   Add `--pdb` to make it debuggable — emits a portable PDB mapped to your source so breakpoints in
   the original file bind (attach the IDE + enable Editor Attaching; compiles unoptimized):
   ```bash
   unity command reload_file --filename Assets/Spinner.cs --pdb
   ```

Constraints: `void` instance methods only; **public** members only; debugging requires `--pdb`
(the default emits no symbols).

### Alternative: with helper — `reload_file_override`

Keep the original method; put the tweak in a **separate** override file.

1. Tag the method `[HotReloadWithOverrides]` and route it via
   `HotReloadHelper.ExecuteWithHotReload(this, "Update", OriginalUpdate)` (original body in `OriginalUpdate`).
2. In a **separate file that does not redeclare the target type**, add a `public static`
   method tagged `[HotReloadOverrideMethod("BossController.Update")]` taking the instance first.
3. Apply: `unity command reload_file_override --filename Assets/HotReload/BossOverrides.cs`

Constraints: **public** members only; you cannot break into the override.

`unity command hotreload_status` shows active overrides. `reload_file_override*` options: `--timeout <ms>`
(default 30000), `--assemblyDir <dir>` (persist DLLs instead of in-memory);
`cleanup_hotreload --assemblyDir <dir>` clears old DLLs. Both commands validate the file up front
and return a clear error on misconfiguration (no override found, target type redeclared, bad signature).

## 4. Quick C# eval (Runtime)

```bash
unity command eval "return 2 + 2;"
# Or evaluate a .cs file on disk (contents run through the same path):
unity command eval_file Assets/Scratch.cs
```

## Gotchas

- **`set_autotick` first.** Without it, recompile and tests can hang while the editor is
  unfocused. The package's watchdog relies on the tick loop staying alive.
- **Hot reload needs the game running.** `reload_file_override` / `reload_file` apply to a live
  game — enter Editor Play Mode (`unity command editor_play`) first, or run a dev Player.
- **Player-only commands need a dev Player.** `log`, `set_timescale`, `runtime_status`, etc. hit
  the Runtime server, which does not run in the Editor.
- **Async commands poll.** `recompile`→`recompile_status`, `run_tests --async_tests`→
  `test_status`. Never assume completion from the trigger call's response.
- **Target a specific instance** with `--instance host:port` or `--project-path <path>`
  when more than one is running.

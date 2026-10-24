---
name: asp-ft-player-builds
description: Build and run release players of Aspid.FastTools on macOS in one command - Mac Mono, Mac IL2CPP (Managed Stripping Level Minimal and High) and the iOS Simulator - and check that SerializableType resolves its stored names and that ENABLE_PROFILER is off. Covers the "player builds" items of the release checklist. - `/asp-ft-player-builds`, `/asp-ft-player-builds 6000.3.26f1`.
argument-hint: "[unity-version]"
disable-model-invocation: true
---

# Player builds

The argument is the Unity version. No argument → the minimum: the first version of `UNITY_VERSIONS` in
`.github/workflows/ci.yml`.

## When

- The manual QA of a **stable** release, before the merge of the release PR: the items "player builds on Mono and
  IL2CPP", "non-development build", "IL2CPP stripping and `SerializableType`" and "one standalone and one AOT target"
  of `.github/ISSUE_TEMPLATE/release_checklist.yml`.
- A change to `SerializableType`, the type lookup, the profiler marker generator or `ENABLE_PROFILER` code.

## Run

Run it on the branch under test: for a release, a worktree of `chore/release-<version>`. The script builds the
package of its own checkout.

```bash
scripts/player-builds.sh <scratch>/player-builds [unity-version]
```

- `<scratch>/player-builds` must not exist or be empty. It gets `Project/`, `Builds/` and `Logs/`.
- Run it in the background: all four targets take about 3–4 minutes.
- `--targets mac-mono,ios-sim` limits the run to the listed targets.
- It needs an Apple Silicon Mac (the players are arm64), the Unity Editor of the version, its modules `mac-il2cpp`
  and `ios`, and Xcode for `ios-sim`. A missing module skips its target and prints the Unity Hub command that installs
  it. Ask the user before you run an install: it downloads gigabytes.
- The simulator runs without its window. The script shuts it down after the run when it booted it.

## Read the table

```
TARGET               BUILD      ERR/WARN  AOTCHECK   PROFILER  REFERENCE MISSING
mac-mono             Succeeded  0/0       5/5        off dev=off none
```

- `BUILD`: the `BuildReport` result. `xcodebuild-failed`: Unity made the Xcode project, Xcode did not build it.
- `ERR/WARN`: the errors and warnings of the `BuildReport`.
- `AOTCHECK`: the required fields that resolved in the player, out of all. Required are a `[Preserve]` class,
  `Transform`, `List<int>`, and `LinePattern` of the Types sample (`samplePattern`, and `samplePatternInstance` for
  its instance): each must resolve on every target. The sample patterns carry `[Preserve]`, so a `MISSING` there means
  that the sample lost it. `no-run`: the player did not print `AOTCHECK DONE`.
- `PROFILER`: `ENABLE_PROFILER` and `DEVELOPMENT_BUILD` in the player. Both must be `off`.
- `REFERENCE MISSING`: reference fields that did not resolve. They are informational and do not fail the run:
  - `plain`, a class referenced only by its stored name: the linker removes it from Managed Stripping Level Medium up.
    The XML docs of `SerializableTypeBase` say so. `plain` that turns `MISSING` on `mac-mono` or
    `mac-il2cpp-minimal` is a regression.
  - `strippedEngine`, `BoxCollider`: on every IL2CPP target Strip Engine Code removes the physics module, which the
    check scene does not use.

The exit code is 1 when a target is skipped or fails. The logs are in `Logs/`: `unity-*.log` for the builds,
`run-*.log` for the players, `xcodebuild-ios-sim.log`. The iOS Simulator player is always a development engine build,
so its log says "Build type 'Development'" and `debug=True`: the `PROFILER` column still checks the defines.

## Report

Write the report in Russian. Keep it short:

1. The first line: the verdict, the Unity version and the commit.
2. The table as the script printed it.
3. Each failure: the target and the first error lines from its log.
4. The release checklist items that the run closes.

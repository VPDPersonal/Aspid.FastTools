---
name: asp-ft-release
description: Release an Aspid.FastTools version end to end. Phase 1 opens the release PR (CHANGELOG section, version files, local tests, Unity EditMode); the user merges it. Phase 2 pushes the v<version> tag after the user's yes and checks what the Release workflow published. The version alone picks the channel. - `/asp-ft-release 1.0.0-rc.9`, `/asp-ft-release 1.0.0`.
argument-hint: <version>
disable-model-invocation: true
metadata:
  internal: true
---

# Release

The argument is the version, for example `1.0.0-rc.9`. Remove a leading `v`. No argument → ask for the version.

## Channel

The version picks the channel. It is the same rule as in `scripts/set-version.sh` and `.github/workflows/release.yml`.

- A version with a hyphen (`1.0.0-rc.9`) → `upm-preview`: a Preview badge, a GitHub prerelease.
- A version without a hyphen (`1.0.0`) → `upm`: a stable release. Only it has the steps marked **Stable**.

## Which phase

Run `git fetch origin`. Then pick the phase:

1. The tag `v<version>` is on origin (`git ls-remote --tags origin v<version>`) → go to "Phase 3".
2. The `package.json` on `origin/main` has the version → go to "Phase 2".
3. The PR from `chore/release-<version>` is open (`gh pr view chore/release-<version> --json state,url`) →
   report its link and its checks, then stop.
4. Else → go to "Phase 1".

## Phase 1: release PR

Do every step without questions. Stop at the first failure and report it.

1. Check that the working tree is clean.
2. Check that the version is greater than the `package.json` version on `origin/main`. Use SemVer precedence:
   `1.0.0-rc.10` > `1.0.0-rc.9`, and `1.0.0` > `1.0.0-rc.9`.
3. Create the branch: `git switch -c chore/release-<version> origin/main`.
4. Check that `Website/node_modules` exists. If not, run `npm --prefix Website ci`.
5. Run `scripts/set-version.sh <version>`. Keep the "channel changed" line if the script prints it.
6. Start two `unity-verify` agents in the background, with EditMode tests and no filter:
   - a project from `scripts/make-unity-test-project.sh <scratchpad>/unity-min <minimum>`, where `<minimum>` is the
     `minimum` entry of `.github/workflows/tests.yml`;
   - the dev project `Aspid.FastTools/` of this checkout.

   The `unity-verify` agent is user-level. Without it, run the batch command from the header of
   `scripts/make-unity-test-project.sh` for both projects, in the background.
7. Run the checks that the release runs:
   - from `.github/workflows/tests.yml`: `dotnet test --nologo` in `Aspid.FastTools.Generators/` and in
     `Aspid.FastTools.Analyzers/`, and `dotnet test Aspid.FastTools.YamlTests --nologo` in the repository root;
   - from the `preflight` job of `.github/workflows/release.yml`: the two Release builds of the step
     "Verify committed Roslyn DLLs match sources", then its `git diff`.
8. A DLL diff in step 7 means stale DLLs on `main`. Run `git checkout --` on both DLLs. Stop and report.
9. Run `npm --prefix Website run check-readme`.
10. Find the old version in prose:
    `git grep -n -F '<old>' -- . ':!CHANGELOG*' ':!scripts/'`.
    Do not edit the hits. Put them into the report.
11. Wait for both `unity-verify` agents. A failed test stops the release.
12. Commit with the `asp-commit` skill. Message: `chore(release): v<version>`.
13. Open the PR with the `asp-pr` skill. Title: `chore(release): v<version>`. Body: the channel and the version's
    CHANGELOG section in one line.

    The `asp-commit` and `asp-pr` skills are user-level. Without them, use `git commit`, `git push` and
    `gh pr create`, and follow `.claude/asp-pr.md`.
14. Send the report (format below). The user merges the PR.

## Phase 2: tag

1. Find the merge commit: `gh pr view chore/release-<version> --json state,mergeCommit`.
2. The PR is not merged → say so and stop.
3. Ask the user in one message: the version, the channel, the commit SHA and subject. Say that the tag starts the
   Release workflow and that a published tag cannot be replaced.
4. Wait for a clear yes. Any other answer → stop.
5. Create the tag: `git tag -a v<version> -m "Release v<version>" <merge-sha>`.
6. Push it: `git push origin v<version>`.
7. Find the run of the tag:
   `gh run list --workflow release.yml --branch v<version> --limit 1 --json databaseId,status,conclusion`.
   No run yet → wait 10 seconds and try again. After 6 tries, stop and report.
8. Run `gh run watch <id> --exit-status` in the background. Go to "Phase 3" when it ends.

## Phase 3: check the publication

1. Find the run of the tag as in "Phase 2" step 7.
2. `status` is not `completed` → go to "Phase 2" step 8.
3. `conclusion` is not `success` → go to "Release failed".
4. Check the GitHub release: `gh release view v<version> --json url,isPrerelease`.
   `isPrerelease` must be true on `upm-preview` and false on `upm`.
5. Check that `refs/heads/<channel>` and `refs/tags/<channel>/<version>` point to one commit:
   `git ls-remote origin refs/heads/<channel> refs/tags/<channel>/<version>`.
6. Check the package version on the channel:
   `gh api 'repos/VPDPersonal/Aspid.FastTools/contents/package.json?ref=<channel>' --jq .content | base64 -d`.
7. Check that the package CHANGELOG on the channel has the version's section. The release generates that copy:
   `gh api 'repos/VPDPersonal/Aspid.FastTools/contents/CHANGELOG.md?ref=<channel>' --jq .content | base64 -d`.
8. **Stable:** ask before you close the milestone `v<version>`. Then close it with `gh api -X PATCH`.
9. Send the final report: the release URL, the channel, the open items from the Phase 1 report.

## Release failed

Read the failed step: `gh run view <id> --log-failed`. Then:

- **The job `Preflight`, `Tests` or `release` failed.** Only the `v<version>` tag is out: the push of the tags is atomic.
  1. Report the cause.
  2. A transient cause (network, Unity license): ask, then run `gh run rerun <id> --failed`.
     It keeps the finished jobs. Go to "Phase 3" when the run ends.
  3. Preflight says the commit is not in `main`: there is nothing to fix in `main`. The tag is on the wrong commit.
     Merge that commit into `main` with a PR. Then go to step 5 with the merge commit.
  4. Any other cause: the fix goes to `main` in a separate PR.
  5. After the fix, ask before you delete the tag: `git push origin :refs/tags/v<version>`, `git tag -d v<version>`.
  6. Go to "Phase 2" step 3 with the merge commit of that PR.
- **Only the job `GitHub release` failed.** The tags and the channel branch are out.
  1. Run `gh run rerun <id> --failed`. It runs only that job. Go to "Phase 3" when the run ends.
     Do not rerun the whole run: preflight would refuse the UPM tag that the first run already published.
  2. It fails again → ask, then create the release by hand.
     The command is in the comment above the job in `release.yml`.

## Not done by this skill

Put these items into the Phase 1 report. Do not do them without a request.

- The API reference on the site. Each PR that changes public API regenerates it (`docs-site` skill). A PR made
  without Unity says that the reference still needs regenerating. Ask if such a PR is merged since the last release.
- "The channel changed" from `set-version.sh` → re-record the README install walk-through in a separate PR
  (`docs/media/readme-previews/README.md`).
- **Stable:** the manual QA of `.github/ISSUE_TEMPLATE/release_checklist.yml`: player builds, Asset Store.

## Report format

Write the report in Russian. Keep it short:

1. The first line: the PR link (Phase 1) or the release link (Phase 3), and the channel.
2. The checks, one line each: `dotnet test`, DLLs, `check-readme`, Unity minimum, Unity dev project.
3. The old-version hits from Phase 1 step 10: the file and the line.
4. The open items from "Not done by this skill" that apply to this version.
5. The last line: what the user does next.

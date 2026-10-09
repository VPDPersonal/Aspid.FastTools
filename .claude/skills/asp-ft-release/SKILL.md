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

   The Editor of `<minimum>` may not be installed (`ls /Applications/Unity/Hub/Editor`). Then use the oldest
   installed 6000.0.x as `<minimum>`. Write that version in the report. The `minimum` job of CI covers `<minimum>`.
   No 6000.0.x Editor is installed → skip the minimum project. Write that in the report. The `minimum` job of CI
   covers it.
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
3. Check the consumer skills in the merge commit. The docs tell users to install them from the tag
   (`npx skills add VPDPersonal/Aspid.FastTools#v<version>`), and a published tag cannot be replaced.
   - `git ls-tree --name-only <merge-sha>:skills` lists the skill folders. An error or an empty list means that the
     tag has no skills.
   - Every folder has a `SKILL.md`: `git ls-tree -r --name-only <merge-sha>:skills | grep -c '/SKILL.md$'` gives the
     number of folders.
   - `git ls-tree --name-only origin/main:skills` lists more folders → a skill was merged after the release PR.
     It is not in this tag.
4. Ask the user in one message: the version, the channel, the commit SHA and subject, and the result of step 3.
   Say that the tag starts the Release workflow and that a published tag cannot be replaced.
5. Wait for a clear yes. Any other answer → stop.
6. Create the tag: `git tag -a v<version> -m "Release v<version>" <merge-sha>`.
7. Push it: `git push origin v<version>`.
8. Find the run of the tag:
   `gh run list --workflow release.yml --branch v<version> --limit 1 --json databaseId,status,conclusion`.
   No run yet → wait 10 seconds and try again. After 6 tries, stop and report.
9. Run `gh run watch <id> --exit-status` in the background. Go to "Phase 3" when it ends.

## Phase 3: check the publication

1. Find the run of the tag as in "Phase 2" step 8.
2. `status` is not `completed` → go to "Phase 2" step 9.
3. `conclusion` is not `success` → go to "Release failed".
4. Check the GitHub release: `gh release view v<version> --json url,isPrerelease`.
   `isPrerelease` must be true on `upm-preview` and false on `upm`.
5. Check that `refs/heads/<channel>` and `refs/tags/<channel>/<version>` point to one commit:
   `git ls-remote origin refs/heads/<channel> refs/tags/<channel>/<version>`.
6. Check the package version on the channel:
   `gh api 'repos/VPDPersonal/Aspid.FastTools/contents/package.json?ref=<channel>' --jq .content | base64 -d`.
7. Check that the package CHANGELOG on the channel has the version's section. The release generates that copy:
   `gh api 'repos/VPDPersonal/Aspid.FastTools/contents/CHANGELOG.md?ref=<channel>' --jq .content | base64 -d`.
8. Confirm that the published tag has the consumer skills that "Phase 2" step 3 listed:
   `git fetch origin tag v<version>`, then `git ls-tree --name-only v<version>:skills`.
   An error or an empty list means that `npx skills add` finds no skills in the tag. Put it into the report.
   This step only reports: a published tag cannot be replaced.
9. **Stable:** find the milestone. Its title is `<version>` or `v<version>`:
   `gh api 'repos/VPDPersonal/Aspid.FastTools/milestones?state=open&per_page=100' --jq '.[] | select(.title == "<version>" or .title == "v<version>") | .number'`.
   No milestone → say so in the report and skip step 10.
10. **Stable:** ask before you close the milestone. Then close it:
    `gh api -X PATCH repos/VPDPersonal/Aspid.FastTools/milestones/<number> -f state=closed`.
11. Send the final report: the release URL, the channel, the open items from the Phase 1 report.

## Release failed

Read the failed step: `gh run view <id> --log-failed`. Then:

- **The infrastructure failed.** Examples: the Unity process was killed (exit code 137), a runner was lost, a download
  timed out. The log shows no error in the repository, and "Publish release tag and UPM subtree" did not run.
  1. Run `gh run rerun <id> --failed`. Keep the tag.
  2. Go to "Phase 2" step 9.
  3. The same step fails again → use the next case.
- **A step before "Publish release tag and UPM subtree" failed.** Only the `v<version>` tag is out.
  1. Report the cause. A fix goes to `main` in a separate PR.
  2. After the fix, ask before you delete the tag: `git push origin :refs/tags/v<version>`, `git tag -d v<version>`.
  3. Go to "Phase 2" step 3 with the merge commit of the fix PR.
- **Only "Create GitHub release" failed.** The tags and the channel branch are out.
  1. Ask, then create the release by hand. The command is in the comment above that step in `release.yml`.

## Asset Store upload

The upload is manual and belongs to a **Stable** release. The user signs in and uploads. These decisions are fixed.

- **Upload form: Local UPM Package** of Asset Store Tools (`Tools/Asset Store/Uploader`, "Upload type").
  - The package stays a UPM package in `Packages/`. Its tests compile only for a package in `Assets/` or in
    `testables`, so a buyer does not run them.
  - Do not use "From Assets Folder". The package would land in `Assets/`, and about 30 tests would run in the buyer's
    project, some of them changing its `ProjectSettings`.
  - "Pre-exported .unitypackage" is the fallback if Asset Store Tools removes the Local UPM Package entry.
- **Symbol:** the entry exists only with `UNITY_ASTOOLS_EXPERIMENTAL`. The dev project sets it for the Standalone group
  only (`Aspid.FastTools/ProjectSettings/ProjectSettings.asset`). Keep it there, so that the Uploader and the
  Validator open in the dev project for a look. Never upload from the dev project: see "Editor". The upload project
  needs the same symbol, and its build target must stay Standalone: on Android or iOS the entry disappears without a
  message.
- **Editor: 6000.0.53f1**, the minimum of `package.json` and `tests.yml`. Asset Store Tools sends the Editor version
  with every request, and the store can record it as the minimum Unity version. An upload from the dev project (6000.4)
  can list buyers on 6000.0–6000.3 as unsupported. Do not open the dev project in 6000.0.53f1: it is a 6000.4
  project, and the downgrade rewrites its files.
- **Upload project:** a new project on 6000.0.53f1.
  1. Put the package into `Packages/tech.aspid.fasttools`. Take it from the `upm/<version>` tag after "Phase 3":
     `git clone --depth 1 --branch upm/<version> https://github.com/VPDPersonal/Aspid.FastTools <upload project>/Packages/tech.aspid.fasttools`,
     then delete the `.git` folder in it. The tag has the generated `CHANGELOG.md`. Before the tag, run
     `node scripts/package-changelog.mjs v<version>` and copy the package folder.
  2. Copy `Aspid.FastTools/Packages/com.unity.asset-store-tools` into `Packages/`.
  3. Add `UNITY_ASTOOLS_EXPERIMENTAL` to the Standalone scripting define symbols.
  4. In the Uploader, pick the draft, "Local UPM Package" and `Packages/tech.aspid.fasttools/package.json`.
- **Validator:** run `Tools/Asset Store/Validator` on the package before the upload. Fix every failure first.
  One warning is expected and stays: "The following scripts contain types not nested under a namespace".
  - It lists `ProfilerMarkerExtensionsForGenerator`. The type has no namespace on purpose, because the generator
    finds it by name. Do not move it.
  - The Validator does not report the types of the Roslyn DLLs. It reads only the DLLs that the Editor has loaded,
    and it skips compiler-generated names.

  Put the warning into the submission notes and name that type.
- **Check the export:** use "Export" and import the `.unitypackage` into a clean 6000.0.53f1 project. The package
  lands in `Packages/tech.aspid.fasttools`, and the Test Runner lists no FastTools tests.

## Not done by this skill

Put these items into the Phase 1 report. Do not do them without a request.

- The API reference on the site. Each PR that changes public API regenerates it (`docs-site` skill). A PR made
  without Unity says that the reference still needs regenerating. Ask if such a PR is merged since the last release.
- "The channel changed" from `set-version.sh` → re-record the README install walk-through in a separate PR
  (`docs/media/readme-previews/README.md`).
- **Stable:** the manual QA of `.github/ISSUE_TEMPLATE/release_checklist.yml`: player builds, and the Asset Store
  upload as in "Asset Store upload".

## Report format

Write the report in Russian. Keep it short:

1. The first line: the PR link (Phase 1) or the release link (Phase 3), and the channel.
2. The checks, one line each: `dotnet test`, DLLs, `check-readme`, Unity minimum, Unity dev project.
3. The old-version hits from Phase 1 step 10: the file and the line.
4. The open items from "Not done by this skill" that apply to this version.
5. The last line: what the user does next.

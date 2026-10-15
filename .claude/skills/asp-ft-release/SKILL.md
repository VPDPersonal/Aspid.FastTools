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
   report its link and its checks, then stop. **Stable:** add the steps of "Asset Store upload".
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
     first version of the `unity` matrix in `.github/workflows/ci.yml`;
   - the dev project `Aspid.FastTools/` of this checkout.

   The `unity-verify` agent is user-level. Without it, run the batch command from the header of
   `scripts/make-unity-test-project.sh` for both projects, in the background.

   The Editor of `<minimum>` may not be installed (`ls /Applications/Unity/Hub/Editor`). Then use the oldest
   installed 6000.0.x as `<minimum>`. Write that version in the report. The `minimum` job of CI covers `<minimum>`.
   No 6000.0.x Editor is installed → skip the minimum project. Write that in the report. The `minimum` job of CI
   covers it.
7. Run the .NET checks of `.github/workflows/ci.yml`. The release PR runs the other `ci.yml` checks:
   - the `dotnet-tests` jobs: `dotnet test --nologo` in `Aspid.FastTools.Generators/` and in
     `Aspid.FastTools.Analyzers/`, and `dotnet test Aspid.FastTools.YamlTests --nologo` and
     `dotnet test Aspid.FastTools.TypeTests --nologo` in the repository root;
   - the `roslyn-dlls` job: the two Release builds of the step
     "Rebuild both DLLs in Release", then the `git diff` of the next step.
8. A DLL diff in step 7 means stale DLLs on `main`. Run `git checkout --` on both DLLs. Stop and report.
9. Run `npm --prefix Website run check-readme`.
10. Find the old version in prose:
    `git grep -n -F '<old>' -- . ':!CHANGELOG*' ':!scripts/'`.
    Do not edit the hits. Put them into the report.
11. Wait for both `unity-verify` agents. A failed test stops the release.
12. Commit with the `asp-commit` skill. Message: `chore(release): v<version>`.
13. Open the PR with the `asp-pr` skill. Title: `chore(release): v<version>`. Body: the channel and the version's
    CHANGELOG section in one line.

    The `asp-commit` and `asp-pr` skills come from the `asp-git` plugin that `.claude/settings.json` enables.
    Without them, use `git commit`, `git push` and `gh pr create`, and follow `.claude/asp-pr.md`.
14. Send the report (format below). The user merges the PR. **Stable:** the merge waits for the Asset Store
    publication, see "Asset Store upload".

## Phase 2: tag

1. Find the merge commit: `gh pr view chore/release-<version> --json state,mergeCommit`.
2. The PR is not merged → say so and stop.
   **Stable:** ask the user whether this version is live on the Asset Store. Not yet → the PR was merged too
   early: the READMEs, the site and the badge point to `#upm` and `v<version>`, and neither exists before the tag.
   Ask the user which way to recover, then act:
   - tag now: GitHub goes first, and the install URLs work at once → go on with step 3;
   - revert the release commit on `main` in a PR: `[Unreleased]` gets its notes back, and "Phase 1" runs again
     after the store publishes, as "Asset Store upload" describes → stop.
3. Check the consumer skills in the merge commit. The docs tell users to install them from the tag
   (`npx skills add VPDPersonal/Aspid.FastTools#v<version>`), and a published tag cannot be replaced.
   - `git ls-tree --name-only <merge-sha>:skills` lists the skill folders. An error or an empty list means that the
     tag has no skills.
   - Every folder has a `SKILL.md`: `git ls-tree -r --name-only <merge-sha>:skills | grep -c '/SKILL.md$'` gives the
     number of folders.
   - Run `git fetch origin main`, then
     `diff <(git ls-tree --name-only <merge-sha>:skills) <(git ls-tree --name-only origin/main:skills)`.
     A folder only on `main` → a skill was merged, renamed or replaced after the release PR. It is not in this tag.
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

- **The job `Preflight`, a `CI / …` job or `release` failed.** Only the `v<version>` tag is out: the push of the tags is atomic.
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

## Asset Store upload

The upload is manual and belongs to a **Stable** release. The user signs in, uploads and submits. These decisions are
fixed.

- **Order:** the stable version reaches GitHub only after the Asset Store publishes it. A merge of the release PR
  already switches the READMEs and the site to `#upm` and the Release badge (`set-version.sh`, `docs.yml`), while
  `upm` gets the version only with the tag. So the upload runs from the head of `chore/release-<version>` while the PR
  is open. The user merges the PR after the store publishes the version, and "Phase 2" tags it at once.
- **Hold:** until the merge, no other PR that changes `Aspid.FastTools/Packages/tech.aspid.fasttools/`, `CHANGELOG.md`
  or `CHANGELOG.ru.md` merges into `main`. Otherwise the merge commit, and so `upm/<version>` with the package
  `CHANGELOG.md` that `release.yml` generates from the root one, differs from the uploaded package.
- **Portal:** the UPM Publisher Portal (`https://cloud.unity.com/assetstore/publisher/products`), product `fastTools`
  (id 2333465, store page `https://assetstore.unity.com/packages/package/2333465`). The package technical name
  `tech.aspid.fasttools` is reserved there and must stay the `name` of `package.json`. The old portal
  (`publisher.unity.com`) serves `.unitypackage` assets only: do not upload FastTools there.
- **Tool:** Asset Store UPM Publishing Tools, `com.unity.upm-publishing-tools`. It is free, but it installs only for an
  account that has it in **My Assets** (`https://assetstore.unity.com/packages/package/5368745`): the registry refuses
  the download otherwise. It lives only in the upload project: never commit it to the dev project manifest, where it
  would fail the package resolve of every clone without that entitlement. Do not use the old Asset Store Tools
  (`com.unity.asset-store-tools`): their "Local UPM Package" uploads a `.unitypackage` to the old portal.
- **Editor: 6000.0.53f1**, the minimum of `package.json` and of the `ci.yml` matrix. The portal records the Editor
  version of every uploaded package. An upload from the dev project (6000.6) can list buyers on 6000.0–6000.5 as
  unsupported. Do not open the dev project in 6000.0.53f1: it is a 6000.6 project, and the downgrade rewrites its files.
- **Upload project:** a new project on 6000.0.53f1.
  1. Check out the head of `chore/release-<version>` in a worktree, run `node scripts/package-changelog.mjs v<version>`
     there, and copy `Aspid.FastTools/Packages/tech.aspid.fasttools` into `Packages/tech.aspid.fasttools` of the
     upload project. With the hold above, it is the content that the Release workflow later publishes to
     `upm/<version>`.
  2. Install the Publishing Tools: **Window → Package Manager → My Assets → Asset Store UPM Publishing Tools → Install**.
- **Validator:** Validation Type **UPM**. Fix every failure first. A warning about types outside a namespace that names
  `ProfilerMarkerExtensionsForGenerator` stays: the generator finds that type by name. Put it into the submission
  notes and name that type.
- **Uploader:** the **UPM Packages** tab, **Upload** on `tech.aspid.fasttools`.
- **Portal, package metadata:** the compatible Unity versions (every `6000.x` from 6000.0 that the `ci.yml` matrix
  covers), the release notes (the version's CHANGELOG section) and the package information.
- **Submit** with **Manually publish**. Unity reviews in about five business days.
- **Rejected:** close the release PR without a merge. The fixes go to `main` in ordinary PRs, then "Phase 1" runs
  again: its notes are still under `[Unreleased]` on `main`. Keep the version if the portal takes a new upload of it,
  else take the next one. The rejected version never reaches `main` or a tag. With the same version, delete the old
  branch first, local and remote (`git branch -D chore/release-<version>`,
  `git push origin --delete chore/release-<version>`): "Phase 1" creates it again, and `gh pr view` by the branch
  name could find the closed PR. Use the number of the new PR from then on.
- **Approved:** the user clicks publish in the portal and checks the store page. Then, on the release branch, the date
  in the `## [<version>] — <date>` heading of both CHANGELOGs becomes the publication day. That date is the one
  expected difference between `upm/<version>` and the uploaded package, whose `CHANGELOG.md` keeps the upload day.
  The user merges the PR, and "Phase 2" tags it.
- **Check the install:** in a clean 6000.0.53f1 project, **My Assets → Aspid.FastTools → Install**. The package lands
  in the **Packages** list (not in `Assets/`), its **Samples** tab imports the samples, and the Test Runner lists no
  FastTools tests.

## Not done by this skill

Put these items into the Phase 1 report. Do not do them without a request.

- The API reference on the site. Each PR that changes public API regenerates it (`docs-site` skill). A PR made
  without Unity says that the reference still needs regenerating. Ask if such a PR is merged since the last release.
- "The channel changed" from `set-version.sh` → re-record the README install walk-through in a separate PR
  (`docs/media/readme-previews/README.md`).
- **Stable:** the manual QA of `.github/ISSUE_TEMPLATE/release_checklist.yml`: player builds, and the Asset Store
  upload as in "Asset Store upload". It comes before the merge of the release PR.

## Report format

Write the report in Russian. Keep it short:

1. The first line: the PR link (Phase 1) or the release link (Phase 3), and the channel.
2. The checks, one line each: `dotnet test`, DLLs, `check-readme`, Unity minimum, Unity dev project.
3. The old-version hits from Phase 1 step 10: the file and the line.
4. The open items from "Not done by this skill" that apply to this version.
5. The last line: what the user does next.

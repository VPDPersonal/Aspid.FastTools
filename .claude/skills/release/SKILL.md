---
name: release
description: Prepare and ship an Aspid.FastTools release — turn [Unreleased] of both changelogs into the version, bump the version everywhere, open the release PR whose merge publishes the tag, the upm branches and the GitHub release. Use on «подготовь релиз», «выпусти 1.0.0», "release 1.0.0-rc.9", "cut a release".
metadata:
  internal: true
---

# Release

Merging a PR into `main` that changes the `package.json` version runs `.github/workflows/release.yml`: it checks the
changelogs, the version files and the Roslyn DLLs, tags `v<version>`, publishes `upm`/`upm/<version>` (or
`upm-preview` for a prerelease) and creates the GitHub release from the changelog section. Everything below only
prepares that PR.

1. **Version.** Take it from the user; never guess. A `-suffix` (`1.0.0-rc.9`) is a prerelease.
2. **Branch** from a fresh `main`: `git fetch origin && git switch -c release/v<version> origin/main`.
3. **Prepare:** `[ -d Website/node_modules ] || npm --prefix Website ci`, then
   `node scripts/prepare-release.mjs <version>`. It refuses an empty `[Unreleased]` or an existing version.
4. **Check** with `node scripts/check-changes.mjs` and read the diff: both changelogs got
   `## [<version>] — <today>` under an empty `[Unreleased]` and a link reference, the package `CHANGELOG.md` is a
   copy of the root one, the version is bumped in `package.json`, both READMEs, the badge and the root `README.md`.
   A stable version also turns the badge into `Release` and the install URLs into `#upm`; a prerelease turns them
   back into `Preview` and `#upm-preview`.
5. **Stable release only:** ask whether to snapshot the docs (`npm --prefix Website run version <version>`, see the
   `docs-site` skill) in the same PR.
6. **Commit and PR:** `chore(release): prepare <version>` with explicit paths, then a ready PR with `type: chore`,
   `area: docs` and the changelog section as the body (`asp-pr`), and `gh pr merge <N> --auto --merge`.
7. **After the merge:** `gh run list --workflow release.yml --limit 1` and report the release URL.

A failed release run publishes nothing before its "Publish release tag and UPM subtree" step, which pushes the tags
and the upm branch in one atomic push: fix it in a new PR that keeps the version (the next merge re-runs the
release), or rerun the workflow. If only "Create GitHub release" failed, the tags are already out: create the release
by hand with `gh release create v<version> --verify-tag --notes-file <section>` (plus `--prerelease` for a
prerelease).

#!/bin/sh
# Set the package version everywhere it is written by hand: package.json, the version badge and the install URLs
# in both README translations. The root README is regenerated from the package one. The version also picks the
# channel .github/workflows/release.yml publishes to: a prerelease (1.0.0-rc.9) installs from `upm-preview` under
# a "Preview" badge, a stable version (1.0.0) from `upm` under a "Release" one.
#   scripts/set-version.sh 1.0.0-rc.9
set -eu
cd "$(dirname "$0")/.."
NEW="${1:?usage: scripts/set-version.sh <version>}"
echo "$NEW" | grep -Eq '^[0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z.-]+)?$' || { echo "$NEW is not a SemVer version" >&2; exit 1; }
# Checked before any file changes, so a missing install never leaves the root README out of step.
[ -d Website/node_modules ] || { echo "Website/node_modules is missing; run: npm --prefix Website ci" >&2; exit 1; }
PKG=Aspid.FastTools/Packages/tech.aspid.fasttools
OLD=$(sed -n 's/^  "version": "\(.*\)",$/\1/p' "$PKG/package.json")
[ -n "$OLD" ] || { echo "package.json version not found" >&2; exit 1; }
case "$NEW" in
  *-*) BRANCH=upm-preview LABEL=Preview EN='latest preview' RU='последнюю preview-версию' ;;
  *) BRANCH=upm LABEL=Release EN='latest release' RU='последний релиз' ;;
esac
export OLD NEW BRANCH LABEL EN RU
# Only the "version" line: a dependency can be at the same number. Elsewhere OLD is matched whole, so 1.0.0 does not
# rewrite 1.0.0-rc.8 or 11.0.0.
perl -pi -e 's/^(  "version": ")\Q$ENV{OLD}\E(",)$/$1$ENV{NEW}$2/' "$PKG/package.json"
WHOLE='s/(?<![0-9.])\Q$ENV{OLD}\E(?![0-9A-Za-z.-])/$ENV{NEW}/g'
for f in "$PKG/Documentation/README.md" "$PKG/Documentation/ru/README.md"; do
  perl -pi -e "$WHOLE"';
    s/\[!\[(?:Preview|Release) /[![$ENV{LABEL} /g;
    s/\.git#upm(?:-preview)?\b/.git#$ENV{BRANCH}/g;
    s/the latest (?:preview|release);/the $ENV{EN};/;
    s/на (?:последнюю preview-версию|последний релиз);/на $ENV{RU};/' "$f"
done
perl -pi -e "$WHOLE"'; s/(?:Preview|Release)( \Q$ENV{NEW}\E|<\/text>)/$ENV{LABEL}$1/g' \
  "$PKG/Documentation/Images/status-badge-preview.svg"
npm --prefix Website run --silent sync-readme
echo "$OLD -> $NEW ($BRANCH)"
git status --short

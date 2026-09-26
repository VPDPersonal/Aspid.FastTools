#!/bin/sh
# Set the package version everywhere it is written by hand: package.json, the badge SVG and the badge alt text and
# release link in both README translations. The root README is regenerated from the package one. The install URLs
# name the upm branch, not the version, so they are left alone.
#   scripts/set-version.sh 1.0.0-rc.9
set -eu
cd "$(dirname "$0")/.."
NEW="${1:?usage: scripts/set-version.sh <version>}"
PKG=Aspid.FastTools/Packages/tech.aspid.fasttools
OLD=$(sed -n 's/^  "version": "\(.*\)",$/\1/p' "$PKG/package.json")
[ -n "$OLD" ] || { echo "package.json version not found" >&2; exit 1; }
[ "$OLD" != "$NEW" ] || { echo "already at $NEW"; exit 0; }
for f in "$PKG/package.json" "$PKG/Documentation/README.md" "$PKG/Documentation/ru/README.md" \
         "$PKG/Documentation/Images/status-badge-preview.svg"; do
  perl -pi -e "s/\Q$OLD\E/$NEW/g" "$f"
done
npm --prefix Website run --silent sync-readme
echo "$OLD -> $NEW"
git status --short

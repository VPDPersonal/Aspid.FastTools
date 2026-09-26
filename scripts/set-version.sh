#!/bin/sh
# Set the package version everywhere it is written by hand: package.json, the version badge and the install URLs
# in both README translations. The root README is regenerated from the package one; on a stable version the unshipped
# analyzer rules move to AnalyzerReleases.Shipped.md.
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
  sed -i '' "s/$OLD/$NEW/g" "$f"
done
# Analyzer release tracking: the unshipped rules ship with a stable version. Its release headers accept only
# System.Version numbers (RS2007), so pre-releases keep them unshipped.
REL=Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers/AnalyzerReleases
case "$NEW" in *-*) ;; *)
  if grep -q '^AFT[0-9]' "$REL.Unshipped.md"; then
    { printf '\n## Release %s\n\n' "$NEW"
      awk '/^;/ { next } /^$/ { if (body) blank++; next } { body = 1; for (; blank; blank--) print ""; print }' "$REL.Unshipped.md"
    } >> "$REL.Shipped.md"
    sed -n '/^;/p' "$REL.Unshipped.md" > "$REL.Unshipped.md.tmp"
    mv "$REL.Unshipped.md.tmp" "$REL.Unshipped.md"
  fi ;;
esac
npm --prefix Website run --silent sync-readme
echo "$OLD -> $NEW"
git status --short

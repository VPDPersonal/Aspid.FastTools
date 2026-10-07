#!/bin/sh
# Set the package version everywhere it is written by hand: package.json, the badge SVG and the badge alt text,
# release link and install URLs in both README translations (Website/docs, Website/i18n/ru), the install URL in the
# package README. The root README is regenerated from the English one; on a
# stable version the unshipped analyzer rules move to AnalyzerReleases.Shipped.md. The [Unreleased] notes of
# CHANGELOG.md and CHANGELOG.ru.md become the version's section, dated today, with its release link.
# The version also picks the channel .github/workflows/release.yml publishes to: a prerelease (1.0.0-rc.9) installs
# from `upm-preview` under a "Preview" badge, a stable version (1.0.0) from `upm` under a "Release" one.
# Running it again with the version already set repairs files that drifted (a hand-edited package.json, say).
#   scripts/set-version.sh 1.0.0-rc.9
set -eu
cd "$(dirname "$0")/.."
NEW="${1:?usage: scripts/set-version.sh <version>}"
# \A..\z on the whole argument: a line-by-line grep would let "1.0.0<newline>x" through into package.json.
perl -e 'exit($ARGV[0] !~ /\A[0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z.-]+)?\z/)' "$NEW" || { echo "$NEW is not a SemVer version" >&2; exit 1; }
# Checked before any file changes, so a missing install never leaves the root README out of step.
[ -d Website/node_modules ] || { echo "Website/node_modules is missing; run: npm --prefix Website ci" >&2; exit 1; }
# A CHANGELOG that already has the version's section keeps it. Otherwise its [Unreleased] must have notes to release.
# The heading is matched whole, as the move below needs it: a trailing space or a CRLF fails here, before any change.
has_section() { perl -ne 'BEGIN { $v = shift } $f = 1 if /^## \[\Q$v\E\]/; END { exit !$f }' "$NEW" "$1"; }
for f in CHANGELOG.md CHANGELOG.ru.md; do
  has_section "$f" && continue
  awk '/^## \[Unreleased\]$/ { on = 1; next } /^## \[/ { exit } on && NF && !/^#/ { notes = 1 } END { exit !notes }' "$f" \
    || { echo "$f: no \"## [Unreleased]\" line with notes to release as $NEW" >&2; exit 1; }
done
PKG=Aspid.FastTools/Packages/tech.aspid.fasttools
DOCS=Website/docs
RU_DOCS=Website/i18n/ru/docusaurus-plugin-content-docs/current
OLD=$(sed -n 's/^  "version": "\(.*\)",$/\1/p' "$PKG/package.json")
[ -n "$OLD" ] || { echo "package.json version not found" >&2; exit 1; }
case "$NEW" in
  *-*) BRANCH=upm-preview LABEL=Preview EN='latest preview' RU='последнюю preview-версию' ;;
  *) BRANCH=upm LABEL=Release EN='latest release' RU='последнюю версию' ;;
esac
export NEW BRANCH LABEL EN RU
# Only the "version" line: a dependency can be at the same number.
OLD=$OLD perl -pi -e 's/^(  "version": ")\Q$ENV{OLD}\E(",)$/$1$ENV{NEW}$2/' "$PKG/package.json"
# Each file replaces the version its own badge carries, whole: 1.0.0 does not rewrite 1.0.0-rc.8 or 11.0.0.
# shellcheck disable=SC2016 # perl code, expanded by perl
WHOLE='s/(?<![0-9.])\Q$old\E(?![0-9A-Za-z-]|\.[0-9A-Za-z])/$ENV{NEW}/g if defined $old'
for f in "$DOCS/README.md" "$RU_DOCS/README.md"; do
  perl -0pi -e 'my ($old) = /\[!\[(?:Preview|Release) ([^\]]+)\]/; '"$WHOLE"';
    s/\[!\[(?:Preview|Release) /[![$ENV{LABEL} /g;
    s/\.git#upm(?:-preview)?\b/.git#$ENV{BRANCH}/g;
    s/the latest (?:preview|release);/the $ENV{EN};/;
    s/на последнюю (?:preview-)?версию;/на $ENV{RU};/' "$f"
done
# The package README carries only the install URL.
perl -pi -e 's/\.git#upm(?:-preview)?\b/.git#$ENV{BRANCH}/g' "$PKG/README.md"
# The badge is as wide as its version text: an estimated 13px glyph width per character, plus 14px of padding,
# which keeps 1.0.0-rc.8 at the original 162.
perl -0pi -e 'my ($old) = /aria-label="(?:Preview|Release) ([^"]+)"/; '"$WHOLE"';
  s/(?:Preview|Release)( \Q$ENV{NEW}\E|<\/text>)/$ENV{LABEL}$1/g;
  if (my ($t) = /class="text" x="90" y="20\.5">([^<]*)</) {
    my $e = 0; $e += /[0-9]/ ? 7.4 : /\./ ? 3.6 : /-/ ? 4.6 : /[A-Z]/ ? 8.5 : 6.6 for split //, $t;
    my $w = 90 + int($e + 0.5) + 14; my $r = $w - 1;
    s/width="\d+" height="32" viewBox="0 0 \d+ 32"/width="$w" height="32" viewBox="0 0 $w 32"/;
    s/(<rect class="outline"[^>]* width=")\d+/$1$r/;
  }' "$DOCS/Images/status-badge-preview.svg"
# Analyzer release tracking: the unshipped rules ship with a stable version. Its release headers accept only
# System.Version numbers (RS2007), so pre-releases keep them unshipped. A version that already has its header is
# never given a second one.
REL=Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers/AnalyzerReleases
case "$NEW" in *-*) ;; *)
  if grep -q '^AFT[0-9]' "$REL.Unshipped.md" && ! grep -qxF "## Release $NEW" "$REL.Shipped.md"; then
    { printf '\n## Release %s\n\n' "$NEW"
      awk '/^;/ { next } /^$/ { if (body) blank++; next } { body = 1; for (; blank; blank--) print ""; print }' "$REL.Unshipped.md"
    } >> "$REL.Shipped.md"
    sed -n '/^;/p' "$REL.Unshipped.md" > "$REL.Unshipped.md.tmp"
    mv "$REL.Unshipped.md.tmp" "$REL.Unshipped.md"
  fi ;;
esac
# The new section goes under an empty [Unreleased], and its release link above the older ones. The package copy of
# CHANGELOG.md is generated at release (scripts/package-changelog.mjs).
DATE=$(date +%Y-%m-%d) REPO=https://github.com/VPDPersonal/Aspid.FastTools
export DATE REPO
for f in CHANGELOG.md CHANGELOG.ru.md; do
  has_section "$f" && continue
  perl -0pi -e 's/^## \[Unreleased\]\n/## [Unreleased]\n\n## [$ENV{NEW}] — $ENV{DATE}\n/m or die "$ARGV: no [Unreleased] line\n";
    my $link = "[$ENV{NEW}]: $ENV{REPO}/releases/tag/v$ENV{NEW}\n";
    s/^(?=\[[0-9][^\]]*\]: )/$link/m or $_ .= "\n$link"' "$f"
done
npm --prefix Website run --silent sync-readme
# Whatever a pattern above missed (a reworded badge, say) fails here instead of at the release.
node scripts/check-version.mjs
echo "$OLD -> $NEW ($BRANCH)"
# The GitHub README's install walk-through types the channel's URL; a recording is not a text file to rewrite.
case "$OLD" in *-*) OLD_BRANCH=upm-preview ;; *) OLD_BRANCH=upm ;; esac
[ "$OLD_BRANCH" = "$BRANCH" ] || echo "The channel changed: re-record the README install walk-through (docs/media/readme-previews/README.md)."
git status --short

#!/bin/sh
# Copies the asp-git skills from Aspid.AgentKit into .claude/skills/ and marks them internal.
# Run it from the repository root to add or update them; `npx skills update` alone drops the internal mark.
#
# The copies are committed, so every clone and every worktree has them without installing anything.
# The mark keeps `npx skills add VPDPersonal/Aspid.FastTools` from offering them to package users.
set -eu

skills="asp-branch asp-commit asp-pr"

args=""
for s in $skills; do args="$args -s $s"; done
# shellcheck disable=SC2086
DISABLE_TELEMETRY=1 DO_NOT_TRACK=1 npx -y skills@latest add VPDPersonal/Aspid.AgentKit -a claude-code $args --copy -y

for s in $skills; do
  f=".claude/skills/$s/SKILL.md"
  grep -q '^  internal: true$' "$f" && continue
  # Put the mark right before the line that closes the frontmatter.
  awk 'NR > 1 && !done && $0 == "---" { print "metadata:"; print "  internal: true"; done = 1 } { print }' "$f" > "$f.tmp"
  mv "$f.tmp" "$f"
done

#!/usr/bin/env bash
# PostToolUse hook: rebuild the Roslyn analyzer after edits inside the analyzer
# project. Deployment into the Unity package is handled by
# Aspid.FastTools.Analyzers/Directory.Build.targets, which copies the DLL on a
# Release build — hence the -c Release below.
#
# Path-scoped on purpose:
#   - Triggers ONLY for *.cs under Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers/
#   - Skips the Tests and Sample projects.
#
# Build success -> exit 0 (silent).
# Path mismatch -> exit 0 (silent).
# Build failure -> exit 2 with stderr piped through, so the assistant sees it.

set -uo pipefail

command -v jq >/dev/null || { echo "rebuild-analyzers hook: jq not found — hook cannot parse tool input" >&2; exit 2; }

file_path=$(jq -r '.tool_input.file_path // empty' 2>/dev/null)

case "$file_path" in
  */Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers/*.cs) ;;
  *) exit 0 ;;
esac

cd "$CLAUDE_PROJECT_DIR" || exit 0

csproj=Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers.csproj

# --no-restore: the hook only fires on .cs edits, so dependencies cannot have changed.
dotnet build "$csproj" \
  --no-restore -c Release --nologo -v quiet 1>&2 || exit 2

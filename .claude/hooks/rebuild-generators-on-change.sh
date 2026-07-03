#!/usr/bin/env bash
# PostToolUse hook: rebuild Roslyn source generators after edits inside the
# main generator project, then redeploy the DLL into the Unity package.
#
# Path-scoped on purpose:
#   - Triggers ONLY for *.cs under Aspid.FastTools.Generators/Aspid.FastTools.Generators/
#   - Skips Unity-side edits (Aspid.FastTools/Assets/...), tests, and the Sample project.
#   - Skipping Unity edits matches the rule "do not run dotnet build for Unity-only edits".
#
# Build success -> exit 0 (silent).
# Path mismatch -> exit 0 (silent).
# Build failure -> exit 2 with stderr piped through, so the assistant sees it.

set -uo pipefail

command -v jq >/dev/null || { echo "rebuild-generators hook: jq not found — hook cannot parse tool input" >&2; exit 2; }

file_path=$(jq -r '.tool_input.file_path // empty' 2>/dev/null)

case "$file_path" in
  */Aspid.FastTools.Generators/Aspid.FastTools.Generators/*.cs) ;;
  *) exit 0 ;;
esac

cd "$CLAUDE_PROJECT_DIR" || exit 0

# --no-restore: the hook only fires on .cs edits, so dependencies cannot have changed.
dotnet build \
  Aspid.FastTools.Generators/Aspid.FastTools.Generators/Aspid.FastTools.Generators.csproj \
  --no-restore -c Release --nologo -v quiet 1>&2 || exit 2

#!/usr/bin/env bash
# PostToolUse hook: rebuild the Roslyn analyzer after edits inside the analyzer
# project (a git submodule), then redeploy the DLL into the Unity package.
# The submodule has no Directory.Build.targets on purpose (it stays independent
# of this repo's layout), so the copy step lives here.
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
[ -f "$csproj" ] || { echo "rebuild-analyzers hook: submodule not initialized — run 'git submodule update --init'" >&2; exit 2; }

# --no-restore: the hook only fires on .cs edits, so dependencies cannot have changed.
dotnet build "$csproj" \
  --no-restore -c Release --nologo -v quiet 1>&2 || exit 2

cp Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers/bin/Release/netstandard2.0/Aspid.FastTools.Analyzers.dll \
   Aspid.FastTools/Packages/tech.aspid.fasttools/Aspid.FastTools.Analyzers.dll 1>&2 || exit 2

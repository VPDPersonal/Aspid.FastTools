#!/usr/bin/env bash
# Creates a throwaway Unity project that compiles the package, its tests and every sample, for running the package's
# EditMode tests on any Unity version (the dev project itself is pinned to one). CI (tests.yml) and local runs use it.
#
# Usage: scripts/make-unity-test-project.sh <project-dir> [unity-version | project]
#   unity-version  e.g. 6000.0.64f1; "project" (the default) takes the dev project's version.
# Prints the resolved Unity version to stdout.
#
# Local run:
#   v=$(scripts/make-unity-test-project.sh /tmp/aspid-ci 6000.0.64f1)
#   "/Applications/Unity/Hub/Editor/$v/Unity.app/Contents/MacOS/Unity" -batchmode -projectPath /tmp/aspid-ci \
#     -runTests -testPlatform EditMode -testResults /tmp/aspid-ci/results.xml
set -euo pipefail

if [ $# -lt 1 ] || [ $# -gt 2 ]; then
  echo "usage: $0 <project-dir> [unity-version | project]" >&2
  exit 2
fi

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PACKAGE="$ROOT/Aspid.FastTools/Packages/tech.aspid.fasttools"
PROJECT="$1"
VERSION="${2:-project}"

if [ "$VERSION" = "project" ]; then
  VERSION="$(sed -n 's/^m_EditorVersion: //p' "$ROOT/Aspid.FastTools/ProjectSettings/ProjectVersion.txt")"
fi
if [ -z "$VERSION" ]; then
  echo "error: could not resolve the Unity version" >&2
  exit 1
fi

if [ -e "$PROJECT" ] && [ -n "$(ls -A "$PROJECT")" ]; then
  echo "error: $PROJECT exists and is not empty" >&2
  exit 1
fi

mkdir -p "$PROJECT/Assets" "$PROJECT/Packages" "$PROJECT/ProjectSettings"
PROJECT="$(cd "$PROJECT" && pwd)"

# Relative, so the path still resolves when the workspace is mounted elsewhere (the GameCI container).
PACKAGE_REF="$(python3 -c 'import os, sys; print(os.path.relpath(sys.argv[1], sys.argv[2]))' "$PACKAGE" "$PROJECT/Packages")"

# test-framework 1.6.0 is what every Unity 6000.x bundles; the modules cover what the samples use.
cat > "$PROJECT/Packages/manifest.json" <<EOF
{
  "dependencies": {
    "tech.aspid.fasttools": "file:$PACKAGE_REF",
    "com.unity.test-framework": "1.6.0",
    "com.unity.modules.animation": "1.0.0",
    "com.unity.modules.audio": "1.0.0",
    "com.unity.modules.imageconversion": "1.0.0",
    "com.unity.modules.imgui": "1.0.0",
    "com.unity.modules.jsonserialize": "1.0.0",
    "com.unity.modules.particlesystem": "1.0.0",
    "com.unity.modules.physics": "1.0.0",
    "com.unity.modules.physics2d": "1.0.0",
    "com.unity.modules.terrain": "1.0.0",
    "com.unity.modules.ui": "1.0.0",
    "com.unity.modules.uielements": "1.0.0"
  },
  "testables": [
    "tech.aspid.fasttools"
  ]
}
EOF

printf 'm_EditorVersion: %s\n' "$VERSION" > "$PROJECT/ProjectSettings/ProjectVersion.txt"

# Samples are imported the way Package Manager imports them: copied into Assets/ with their .meta files.
mkdir -p "$PROJECT/Assets/Samples"
cp -R "$PACKAGE/Samples~/." "$PROJECT/Assets/Samples/"

echo "$VERSION"

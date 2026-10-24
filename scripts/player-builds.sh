#!/usr/bin/env bash
# Builds release players of the package on macOS and runs them: the manual "player builds" QA of
# .github/ISSUE_TEMPLATE/release_checklist.yml in one command. The asp-ft-player-builds skill reads the result.
#
# Usage: scripts/player-builds.sh [--targets <list>] <project-dir> [unity-version]
#   --targets      comma-separated; default: mac-mono,mac-il2cpp-minimal,mac-il2cpp-high,ios-sim
#                  mac-mono            StandaloneOSX, Mono, no stripping
#                  mac-il2cpp-minimal  StandaloneOSX, IL2CPP, Managed Stripping Level Minimal
#                  mac-il2cpp-high     StandaloneOSX, IL2CPP, Managed Stripping Level High
#                  ios-sim             iOS Simulator (arm64), IL2CPP, High: Xcode project, xcodebuild, simctl run
#   project-dir    must not exist or be empty; it gets the test project, Builds/ and Logs/.
#   unity-version  default: the minimum, the first version of UNITY_VERSIONS in .github/workflows/ci.yml.
#
# Needs an Apple Silicon Mac: the Mac players and the simulator player are arm64.
#
# Every player is built without BuildOptions.Development and runs Aspid.FastTools/Assets/PlayerBuildCheck~: its
# AOTCHECK lines tell whether SerializableType resolves the stored names, and whether ENABLE_PROFILER is off.
# A target whose Unity module is missing is skipped with the install command. Exits with 1 when a target fails.
set -euo pipefail

ALL_TARGETS="mac-mono,mac-il2cpp-minimal,mac-il2cpp-high,ios-sim"
TARGETS="$ALL_TARGETS"
if [ "${1:-}" = "--targets" ]; then
  TARGETS="${2:?--targets needs a list}"
  shift 2
fi
if [ $# -lt 1 ] || [ $# -gt 2 ]; then
  echo "usage: $0 [--targets <list>] <project-dir> [unity-version]" >&2
  exit 2
fi

IFS=',' read -r -a requested <<< "$TARGETS"
for target in "${requested[@]}"; do
  case ",$ALL_TARGETS," in
    *",$target,"*) ;;
    *)
      echo "error: unknown target $target; known: $ALL_TARGETS" >&2
      exit 2 ;;
  esac
done

if [ "$(uname -m)" != "arm64" ]; then
  echo "error: needs an Apple Silicon Mac: the players are built for arm64" >&2
  exit 1
fi

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
VERSION="${2:-$(sed -n 's/^ *UNITY_VERSIONS: \([^ ]*\).*/\1/p' "$ROOT/.github/workflows/ci.yml")}"
EDITOR_DIR="/Applications/Unity/Hub/Editor/$VERSION"
UNITY="$EDITOR_DIR/Unity.app/Contents/MacOS/Unity"
HUB="/Applications/Unity Hub.app/Contents/MacOS/Unity Hub"
PRODUCT="PlayerBuildCheck"
BUNDLE_ID="tech.aspid.fasttools.playerbuildcheck"
PLAYER_TIMEOUT=120

if [ ! -x "$UNITY" ]; then
  echo "error: Unity $VERSION is not installed: \"$HUB\" -- --headless install --version $VERSION" >&2
  exit 1
fi

if [ -e "$1" ] && [ -n "$(ls -A "$1")" ]; then
  echo "error: $1 exists and is not empty" >&2
  exit 1
fi
mkdir -p "$1"
# A physical path keeps the relative file: reference of the test project valid where /var or /tmp is a symlink.
PROJECT="$(cd "$1" && pwd -P)"
"$ROOT/scripts/make-unity-test-project.sh" "$PROJECT/Project" "$VERSION" >/dev/null
cp -R "$ROOT/Aspid.FastTools/Assets/PlayerBuildCheck~" "$PROJECT/Project/Assets/PlayerBuildCheck"
BUILDS="$PROJECT/Builds"
LOGS="$PROJECT/Logs"
mkdir -p "$BUILDS" "$LOGS"

# The module a target needs, or nothing when it is installed.
missing_module() {
  case "$1" in
    mac-mono) ;;
    mac-il2cpp-minimal | mac-il2cpp-high)
      ls -d "$EDITOR_DIR/Unity.app/Contents/PlaybackEngines/MacStandaloneSupport/Variations/"*il2cpp* >/dev/null 2>&1 ||
        echo "mac-il2cpp" ;;
    ios-sim)
      [ -d "$EDITOR_DIR/PlaybackEngines/iOSSupport" ] || [ -d "$EDITOR_DIR/Unity.app/Contents/PlaybackEngines/iOSSupport" ] ||
        echo "ios" ;;
  esac
}

declare -a MAC=() IOS=() SKIPPED=()
for target in "${requested[@]}"; do
  module="$(missing_module "$target")"
  if [ -n "$module" ]; then
    SKIPPED+=("$target")
    echo "skip $target: needs the Unity module $module: \"$HUB\" -- --headless install-modules --version $VERSION --module $module" >&2
  elif [ "$target" = "ios-sim" ]; then
    if xcrun simctl help >/dev/null 2>&1; then IOS+=("$target"); else SKIPPED+=("$target"); echo "skip $target: needs Xcode" >&2; fi
  else
    MAC+=("$target")
  fi
done

# One Editor run per build target group: the Mac targets, then iOS.
build() {
  local build_target="$1" log="$2"
  shift 2
  local list
  list="$(IFS=','; echo "$*")"
  "$UNITY" -batchmode -nographics -projectPath "$PROJECT/Project" -buildTarget "$build_target" \
    -executeMethod Aspid.FastTools.PlayerBuildCheck.PlayerBuilds.Build \
    -playerBuildTargets "$list" -playerBuildOutput "$BUILDS" -logFile "$LOGS/$log" || true
}

run_with_timeout() {
  perl -e 'alarm shift; exec @ARGV' "$PLAYER_TIMEOUT" "$@"
}

# ${a[@]+"${a[@]}"}: bash 3.2, the macOS bash, treats an empty array as unset under set -u.
[ ${#MAC[@]} -gt 0 ] && build OSXUniversal unity-mac.log "${MAC[@]}"

for target in ${MAC[@]+"${MAC[@]}"}; do
  app="$BUILDS/$target/$PRODUCT.app/Contents/MacOS/$PRODUCT"
  [ -x "$app" ] && { run_with_timeout "$app" -batchmode -nographics -logFile "$LOGS/run-$target.log" >/dev/null 2>&1 || true; }
done

if [ ${#IOS[@]} -gt 0 ]; then
  build iOS unity-ios.log "${IOS[@]}"
  if [ -d "$BUILDS/ios-sim/Unity-iPhone.xcodeproj" ]; then
    xcodebuild -project "$BUILDS/ios-sim/Unity-iPhone.xcodeproj" -scheme Unity-iPhone -sdk iphonesimulator \
      -configuration Release -derivedDataPath "$BUILDS/ios-sim-DerivedData" CODE_SIGNING_ALLOWED=NO ARCHS=arm64 \
      ONLY_ACTIVE_ARCH=YES build > "$LOGS/xcodebuild-ios-sim.log" 2>&1 || true
  fi
  app="$BUILDS/ios-sim-DerivedData/Build/Products/Release-iphonesimulator/$PRODUCT.app"
  if [ -d "$app" ]; then
    # The first available iPhone; a booted one first. The simulator runs without its window.
    read -r device state < <(xcrun simctl list devices available --json | python3 -c '
import json, sys
devices = [d for runtime in json.load(sys.stdin)["devices"].values() for d in runtime if d["name"].startswith("iPhone")]
devices.sort(key=lambda d: d["state"] != "Booted")
print(devices[0]["udid"], devices[0]["state"]) if devices else print("", "")')
    if [ -n "$device" ]; then
      # A failure here leaves ios-sim at no-run in the table instead of ending the script under set -e.
      if [ "$state" = "Booted" ] || xcrun simctl boot "$device"; then
        xcrun simctl bootstatus "$device" -b >/dev/null 2>&1 || true
        if xcrun simctl install "$device" "$app"; then
          run_with_timeout xcrun simctl launch --console-pty --terminate-running-process "$device" "$BUNDLE_ID" \
            > "$LOGS/run-ios-sim.log" 2>&1 || true
        else
          echo "ios-sim: simctl install failed" >&2
        fi
        [ "$state" = "Booted" ] || xcrun simctl shutdown "$device" || true
      else
        echo "ios-sim: simctl boot failed" >&2
      fi
    else
      echo "ios-sim: no available iPhone simulator" >&2
    fi
  fi
fi

# The table: one row per requested target.
status=0
printf '%-20s %-10s %-9s %-10s %-9s %s\n' TARGET BUILD ERR/WARN AOTCHECK PROFILER "REFERENCE MISSING"
for target in "${requested[@]}"; do
  if [[ " ${SKIPPED[*]+"${SKIPPED[*]}"} " == *" $target "* ]]; then
    printf '%-20s %s\n' "$target" "skipped: missing module or Xcode"
    status=1
    continue
  fi

  row="$(awk -F '\t' -v target="$target" '$1 == target' "$BUILDS/builds.tsv" 2>/dev/null || true)"
  result="$(cut -f2 <<< "$row")"
  counts="$(cut -f3 <<< "$row")/$(cut -f4 <<< "$row")"
  [ -n "$row" ] || { result="none"; counts="-"; }

  log="$LOGS/run-$target.log"
  if [ "$target" = "ios-sim" ] && ! grep -q '\*\* BUILD SUCCEEDED \*\*' "$LOGS/xcodebuild-ios-sim.log" 2>/dev/null; then
    result="xcodebuild-failed"
  fi

  if grep -q 'AOTCHECK DONE' "$log" 2>/dev/null; then
    required="$(grep -c 'AOTCHECK required .* ok ' "$log" || true)"
    required_total="$(grep -c 'AOTCHECK required ' "$log" || true)"
    aotcheck="$required/$required_total"
    profiler="$(sed -n 's/.*AOTCHECK profiler=\([a-z]*\) development=\([a-z]*\).*/\1 dev=\2/p' "$log" | head -n 1)"
    reference="$(sed -n 's/.*AOTCHECK reference \([A-Za-z]*\) MISSING.*/\1/p' "$log" | sort -u | paste -sd, -)"
  else
    required=0; required_total=1; aotcheck="no-run"; profiler="-"; reference="-"
  fi

  printf '%-20s %-10s %-9s %-10s %-9s %s\n' "$target" "$result" "$counts" "$aotcheck" "$profiler" "${reference:-none}"

  if [ "$result" != "Succeeded" ] || [ "$required" != "$required_total" ] || [ "$profiler" != "off dev=off" ]; then
    status=1
  fi
done

echo "logs: $LOGS"
exit $status

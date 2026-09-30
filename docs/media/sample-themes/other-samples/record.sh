#!/bin/sh
# Records one sample scene into <slug>-<dark|light>.mkv next to this script, in a batch-mode Editor started with the
# asp-unity-capture skill (`editor.sh open <project>`), so no window reaches the screen.
# Usage: record.sh <project> <Types|SerializeReferences|ProfilerMarkers> <Dark|Light>
# The project needs the samples imported for the package version; prepare.cs.txt opens the scene in that theme preview,
# record.cs.txt writes the frames in Play Mode. Types starts right after Play Mode so its placed Archer (12 s lifetime)
# is on screen: record both themes of a sample with this script back to back.
set -e
here=$(cd "$(dirname "$0")" && pwd)
proj=$(cd "$1" && pwd); sample=$2; theme=$3
cloak_cs=${CAPTURE_SCRIPTS:-$HOME/.claude/skills/asp-unity-capture/scripts}/cloak.cs
slug=$(echo "$sample" | sed 's/\([a-z]\)\([A-Z]\)/\1-\2/g' | tr '[:upper:]' '[:lower:]')
out=$here/$slug-$(echo "$theme" | tr '[:upper:]' '[:lower:]').mkv
frames=$proj/Temp/SampleThemes/other-samples/clean-frames/$sample

cmd() { unity command --project-path "$proj" --no-banner --timeout 60 --json "$@"; }
cloak() { cmd run_script --file "$cloak_cs" --entry CaptureCloak.Run >/dev/null 2>&1 || true; }
mode() {
  cmd editor_status 2>/dev/null | python3 -c 'import json, sys
r = json.load(sys.stdin)["data"]["result"]
print("busy" if r["compiling"] or r["domainReloadInProgress"] else r["playMode"])' 2>/dev/null || echo busy
}
wait_mode() { i=0; until [ "$(mode)" = "$1" ]; do i=$((i+1)); [ $i -gt 60 ] && { echo "Editor never reached $1"; exit 1; }; sleep 1; done; }
# run_script takes only .cs files.
run() {
  tmp=$(mktemp -t sample-theme) && mv "$tmp" "$tmp.cs" && tmp="$tmp.cs" && cp "$1" "$tmp"
  status=0
  cmd run_script --file "$tmp" --entry "$2" ${3:+--args "$3"} | python3 -c 'import json, sys
r = (json.load(sys.stdin).get("data") or {}).get("result") or {}
print(r.get("result") if r.get("success") else r.get("errorDetails", r))
sys.exit(0 if r.get("success") else 1)' || status=$?
  rm -f "$tmp"; return $status
}

[ "$(mode)" = playing ] && { cmd editor_stop >/dev/null; wait_mode stopped; }
wait_mode stopped
run "$here/prepare.cs.txt" PrepareSamples.Main "[\"$sample\",\"$theme\"]"
cmd editor_play >/dev/null
wait_mode playing
cloak
rm -rf "${frames:?}"
run "$here/record.cs.txt" SampleThemeRecorder.Main >/dev/null
i=0; until [ -f "$frames/done.txt" ]; do i=$((i+1)); [ $i -gt 300 ] && { echo "recorder did not finish"; exit 1; }; sleep 1; done
cmd editor_stop >/dev/null
wait_mode stopped
cloak
ffmpeg -y -hide_banner -loglevel error -framerate 20 -i "$frames/frame-%03d.png" -c:v ffv1 -pix_fmt bgr0 "$out"
echo "$out: $(cat "$frames/done.txt")"

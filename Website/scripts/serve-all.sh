#!/bin/sh
# Build the site (all locales) and serve it detached on port 3001 so every agent and the user share one server.
# Re-run after any change to verify it: it kills the previous server, rebuilds and serves again.
#   Website/scripts/serve-all.sh            # build + serve
#   Website/scripts/serve-all.sh --stop     # stop only
# Log: Website/.serve-all.log
set -e
cd "$(dirname "$0")/.."
PORT=3001
LOG="$PWD/.serve-all.log"

pids=$(lsof -tiTCP:$PORT -sTCP:LISTEN 2>/dev/null || true)
[ -n "$pids" ] && kill $pids 2>/dev/null && sleep 1
[ "$1" = "--stop" ] && { echo "stopped"; exit 0; }

npm run build
nohup npx docusaurus serve --port $PORT --no-open --host 0.0.0.0 >"$LOG" 2>&1 &
for i in $(seq 1 30); do
  curl -sf "http://localhost:$PORT/Aspid.FastTools/" >/dev/null 2>&1 && break
  sleep 1
done
echo "serving: http://localhost:$PORT/Aspid.FastTools/  (ru: http://localhost:$PORT/Aspid.FastTools/ru/)"

#!/bin/sh
set -eu
cd "$(dirname "$0")"
ffmpeg -hide_banner -loglevel error -y -i dark-01.jpg -vf 'crop=1190:390:10:30' multipliers-quick-start.png
for name in dark-01 dark-03 dark-04-undo; do
    ffmpeg -hide_banner -loglevel error -y -i "$name.jpg" -vf 'crop=1190:555:10:30' "$name.png"
done
cat > frames.ffconcat <<'FRAMES'
ffconcat version 1.0
file dark-01.png
duration 2
file dark-03.png
duration 3
file dark-04-undo.png
duration 2
file dark-04-undo.png
FRAMES
ffmpeg -hide_banner -loglevel error -y -f concat -safe 0 -i frames.ffconcat -filter_complex 'fps=10,split[a][b];[a]palettegen=max_colors=256[p];[b][p]paletteuse=dither=sierra2_4a' -loop 0 multipliers-populate.gif

#!/bin/sh
set -eu
cd "$(dirname "$0")"
for name in 02-menu 03-populated 04-undo; do
  ffmpeg -hide_banner -loglevel error -y -i "$name.jpg" -vf 'crop=1190:554:10:30' "$name.png"
done
ffmpeg -hide_banner -loglevel error -y -i 05-selector.jpg -vf 'crop=1190:390:10:30' enum-values-type-selector.png
cat > frames.ffconcat <<'FRAMES'
ffconcat version 1.0
file 04-undo.png
duration 1.5
file 02-menu.png
duration 2.5
file 03-populated.png
duration 3
file 04-undo.png
duration 2
file 04-undo.png
FRAMES
ffmpeg -hide_banner -loglevel error -y -f concat -safe 0 -i frames.ffconcat -filter_complex 'fps=10,split[a][b];[a]palettegen=max_colors=256[p];[b][p]paletteuse=dither=sierra2_4a' -t 9 -loop 0 enum-values-multipliers-populate.gif

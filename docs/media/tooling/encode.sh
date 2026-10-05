#!/bin/sh
# Rebuilds the 2026-09-13 recording as tooling.gif next to this script. It never writes the published GIF.
set -eu
cd "$(dirname "$0")"
ffmpeg -v error -y -safe 0 -i frames.ffconcat -filter_complex 'fps=10,split[a][b];[a]palettegen=max_colors=256[p];[b][p]paletteuse=dither=sierra2_4a' -t 4.1 -loop 0 tooling.gif

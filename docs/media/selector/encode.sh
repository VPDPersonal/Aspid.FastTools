#!/bin/sh
set -eu
cd "$(dirname "$0")/../../.."
ffmpeg -v error -y -safe 0 -i docs/media/selector/frames.ffconcat -filter_complex 'fps=10,split[a][b];[a]palettegen=max_colors=256[p];[b][p]paletteuse=dither=sierra2_4a' -loop 0 docs/media/selector/selector.gif

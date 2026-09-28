#!/bin/sh
# Archived attempt, kept for provenance: encodes this folder's own frames into <output.gif>.
# The published GIF is built by ../encode.sh; do not point this one at the package.
#   sh docs/media/tooling/archive-jpeg/encode.sh <output.gif>
set -eu
OUT="${1:?usage: encode.sh <output.gif>}"
ffmpeg -v error -y -safe 0 -i "$(dirname "$0")/frames.ffconcat" -filter_complex 'crop=1088:674:6:28,fps=10,split[a][b];[a]palettegen=max_colors=256[p];[b][p]paletteuse=dither=sierra2_4a' -loop 0 "$OUT"

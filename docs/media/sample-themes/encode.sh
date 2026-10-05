#!/bin/sh
# EnumValues demo.gif / demo-light.gif from the dark and light recordings: one recorder, crop and 355-frame timeline.
set -eu
cd "$(dirname "$0")"
images=../../../Website/tutorials/EnumValues/Images
for pair in dark:demo.gif light:demo-light.gif; do
  ffmpeg -y -hide_banner -loglevel error -i "enum-values-${pair%%:*}.mkv" -filter_complex 'split[v][p];[p]palettegen=max_colors=256[pal];[v][pal]paletteuse=dither=sierra2_4a' -loop 0 "$images/${pair#*:}"
done

#!/bin/sh
set -eu
cd "$(dirname "$0")"
ffmpeg -y -hide_banner -loglevel error -i source-light.mkv -filter_complex 'crop=1101:331:169:270,split[v][p];[p]palettegen=max_colors=256[pal];[v][pal]paletteuse=dither=sierra2_4a' -loop 0 ../../../Aspid.FastTools/Packages/tech.aspid.fasttools/Samples~/EnumValues/Documentation/Images/demo-light.gif

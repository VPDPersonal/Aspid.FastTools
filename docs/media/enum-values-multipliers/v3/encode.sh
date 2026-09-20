#!/bin/sh
set -eu
cd "$(dirname "$0")"
ffmpeg -y -hide_banner -loglevel error -i native-menu.mov -filter_complex "[0:v]split=3[a][b][c];[a]trim=start=4.5:end=6.4,setpts=PTS-STARTPTS[a1];[b]trim=start=11.4:end=15,setpts=PTS-STARTPTS[b1];[c]trim=start=24.7:end=27.5,setpts=PTS-STARTPTS[c1];[a1][b1][c1]concat=n=3:v=1:a=0,fps=20,split[p][v];[p]palettegen=max_colors=256[pal];[v][pal]paletteuse=dither=sierra2_4a[out]" -map '[out]' -loop 0 enum-values-multipliers-populate.gif

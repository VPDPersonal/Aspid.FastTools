#!/bin/sh
set -eu
cd "$(dirname "$0")"
ffmpeg -y -hide_banner -loglevel error -i original-region.png -filter_complex '[0:v]split[top][bottom];[top]crop=1080:328:0:0[t];[bottom]crop=1080:100:0:680[b];[t][b]vstack=inputs=2[out]' -map '[out]' -frames:v 1 ../../../../Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/Images/enum-values-type-selector.png

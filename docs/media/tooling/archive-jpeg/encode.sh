#!/bin/sh
set -eu
cd "$(dirname "$0")/../../.."
ffmpeg -v error -y -safe 0 -i docs/media/tooling/frames.ffconcat -filter_complex 'crop=1088:674:6:28,fps=10,split[a][b];[a]palettegen=max_colors=256[p];[b][p]paletteuse=dither=sierra2_4a' -loop 0 Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/Images/aspid_fasttools_serialize_reference_tooling.gif

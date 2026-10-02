#!/bin/sh
set -eu
cd "$(dirname "$0")/../../.."
ffmpeg -v error -y -safe 0 -i docs/media/selector/frames.ffconcat -filter_complex 'fps=10,split[a][b];[a]palettegen=max_colors=256[p];[b][p]paletteuse=dither=sierra2_4a' -loop 0 Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/Images/aspid_fasttools_serialize_reference_selector.gif
# Crop to the component and add the 8px margin the site shows (see the script).
scripts/frame-doc-captures.sh

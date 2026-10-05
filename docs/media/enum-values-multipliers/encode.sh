#!/bin/sh
set -eu
cd "$(dirname "$0")"
ffmpeg -hide_banner -loglevel error -y -i dark-01.jpg -vf 'crop=1190:390:10:30' multipliers-quick-start.png

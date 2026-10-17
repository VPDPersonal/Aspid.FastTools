#!/usr/bin/env bash
# Turns the manual fixtures of the dev project on and off. They live in Aspid.FastTools/Assets/DevTests~, which Unity
# does not import (a folder name that ends with "~"): some of them are broken on purpose and log warnings on every
# import, which would bury the warnings of the package under test.
# "on" links Assets/DevTests to DevTests~, so Unity imports the fixtures and every change it writes there lands in
# DevTests~; "off" removes the link. The link and its .meta are in .gitignore. The Editor picks the change up on its
# next asset refresh (focus it, or Assets > Refresh).
#
# Usage: scripts/devtests.sh [on | off | status]
set -euo pipefail

LINK="$(cd "$(dirname "$0")/.." && pwd)/Aspid.FastTools/Assets/DevTests"
# The guid the folder had before the fixtures moved to DevTests~, so a reference to the folder still resolves.
GUID=6b9501a56f3149f38f66f517f00527d0

if [ -e "$LINK" ] && [ ! -L "$LINK" ]; then
  echo "error: $LINK is a folder, not the link to DevTests~; move its content into DevTests~ and delete it" >&2
  exit 1
fi

case "${1:-status}" in
  on)
    if [ ! -L "$LINK" ]; then
      ln -s DevTests~ "$LINK"
      printf 'fileFormatVersion: 2\nguid: %s\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n' \
        "$GUID" > "$LINK.meta"
    fi
    echo "DevTests: on"
    ;;
  off)
    rm -f "$LINK" "$LINK.meta"
    echo "DevTests: off"
    ;;
  status)
    if [ -L "$LINK" ]; then echo "DevTests: on"; else echo "DevTests: off"; fi
    ;;
  *)
    echo "usage: $0 [on | off | status]" >&2
    exit 2
    ;;
esac

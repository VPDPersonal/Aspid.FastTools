#!/bin/sh
# Crop raw Unity editor captures in Documentation/Images to their content and add an 8px margin that continues
# Unity's own background, so GitHub, Unity and the site all show the same framed image and the site needs no
# per-capture CSS. Run it after re-shooting a capture listed below; both the dark file and its -light twin are framed.
# A file already at its framed size is skipped, so running it again is safe; a file of any other size is an error,
# because the bounds below would no longer match it: measure the new capture and update its row.
# Needs ImageMagick 6 (`convert`, `identify`) and gifsicle.
#   scripts/frame-doc-captures.sh
set -eu
cd "$(dirname "$0")/../Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/Images"
PAD=8
TMP=$(mktemp -d)
trap 'rm -rf "$TMP"' EXIT
FAILED=0

# A canvas the size of the framed image. `inspector[:<dark body>]`: Unity's Inspector header (a 2px separator 34px below
# the crop's top edge, the row at source y=40) continued to both sides, over the dark body #383838 unless the row names
# its own; `flat:<dark>:<light>`: a single background colour.
canvas() { # <out> <width> <height> <style> <light?>
  case "$4" in
    inspector|inspector:*)
      if [ "$5" = 1 ]; then HEADER='#cbcbcb' LINE='#bababa' BODY='#cbcbcb'
      else HEADER='#3e3e3e' LINE='#303030' BODY='#383838'; [ "$4" = inspector ] || BODY=${4#inspector:}; fi
      SEPARATOR=$((PAD + 34))
      convert -size "$2x$3" "xc:$BODY" -fill "$HEADER" -draw "rectangle 0,0 $(($2 - 1)),$((SEPARATOR - 1))" \
        -fill "$LINE" -draw "rectangle 0,$SEPARATOR $(($2 - 1)),$((SEPARATOR + 1))" "$1" ;;
    flat:*)
      COLORS=${4#flat:}
      if [ "$5" = 1 ]; then COLOR=${COLORS#*:}; else COLOR=${COLORS%%:*}; fi
      convert -size "$2x$3" "xc:$COLOR" "$1" ;;
    *) echo "unknown style: $4" >&2; exit 1 ;;
  esac
}

# <name> <source size> <left> <top> <right> <bottom> <style>; right and bottom are exclusive.
while read -r NAME SIZE LEFT TOP RIGHT BOTTOM STYLE; do
  case "$NAME" in ''|'#'*) continue ;; esac
  W=$((RIGHT - LEFT)) H=$((BOTTOM - TOP))
  FW=$((W + 2 * PAD)) FH=$((H + 2 * PAD))
  for LIGHT in 0 1; do
    SUFFIX=''; [ "$LIGHT" = 1 ] && SUFFIX='-light'
    FILE=$(ls "$NAME$SUFFIX".png "$NAME$SUFFIX".gif 2>/dev/null | head -n 1 || true)
    [ -n "$FILE" ] || { echo "$NAME$SUFFIX: not found" >&2; FAILED=1; continue; }
    ACTUAL=$(identify -format '%wx%h\n' "$FILE" | head -n 1)
    if [ "$ACTUAL" = "${FW}x$FH" ]; then continue; fi
    [ "$ACTUAL" = "$SIZE" ] || { echo "$FILE: $ACTUAL, expected $SIZE (raw) or ${FW}x$FH (framed)" >&2; FAILED=1; continue; }
    canvas "$TMP/canvas.png" "$FW" "$FH" "$STYLE" "$LIGHT"
    case "$FILE" in
      *.gif)
        # gifsicle crops every frame without re-quantizing; ImageMagick would rebuild the palettes and shift colours.
        # Frame 0 is laid over the canvas, every later frame keeps its own palette, delay and disposal, moved by PAD.
        gifsicle --no-warnings --crop "$LEFT,$TOP+${W}x$H" "$FILE" -o "$TMP/crop.gif"
        gifsicle --info "$TMP/crop.gif" | grep '+ image' >"$TMP/info"
        head -n 1 "$TMP/info" | grep -q "^  + image #0 ${W}x$H" || { echo "$FILE: frame 0 is not a full frame" >&2; exit 1; }
        DELAY=$(gifsicle --info "$TMP/crop.gif" '#0' | sed -n 's/.*delay \([0-9.]*\)s.*/\1/p' | awk '{ print int($1 * 100 + 0.5) }')
        LOOP=''; gifsicle --info "$FILE" | grep -q '^  loop forever' && LOOP='--loopcount=forever'
        gifsicle --no-warnings "$TMP/crop.gif" '#0' -o "$TMP/first.gif"
        convert "$TMP/canvas.png" "$TMP/first.gif" -geometry "+$PAD+$PAD" -composite "$TMP/first.png"
        convert "$TMP/first.png" "$TMP/first.gif"
        set --
        while read -r _ _ INDEX _ AT POSITION _; do
          [ "$INDEX" = '#0' ] && continue
          X=0 Y=0
          [ "$AT" = at ] && X=${POSITION%,*} Y=${POSITION#*,}
          set -- "$@" --position "$((X + PAD)),$((Y + PAD))" "$TMP/crop.gif" "$INDEX"
        done <"$TMP/info"
        gifsicle --no-warnings --logical-screen "${FW}x$FH" $LOOP ${DELAY:+-d"$DELAY"} "$TMP/first.gif" --same-delay "$@" \
          -O3 -o "$TMP/out.gif"
        mv "$TMP/out.gif" "$FILE" ;;
      *.png)
        convert "$TMP/canvas.png" \( "$FILE" -crop "${W}x$H+$LEFT+$TOP" +repage \) -geometry "+$PAD+$PAD" -composite \
          "$TMP/out.png"
        mv "$TMP/out.png" "$FILE" ;;
    esac
    echo "$FILE: $SIZE -> ${FW}x$FH"
  done
done <<'EOF'
# Type picker captures: labels start at x=38; warnings keep their icon from x=34; the missing-type stripe from x=30.
serializable-type-missing                     1384x176  30  6 1377  159 inspector:#3c3c3c
serializable-type-quick-start                 1393x400  38  6 1385  392 inspector
type-selector-display                         1393x314  38  6 1385  306 inspector
type-selector-window                          1393x583  38  6 1385  575 inspector
type-selector-generic                         1393x435  38  6 1385  427 inspector
type-selector-member-constraint               1393x565  38  6 1385  557 inspector
type-selector-required                        1393x176  34  6 1385  160 inspector
type-selector-constraint-warning              1393x176  34  6 1385  160 inspector
# TypeSelector's quick start compares two pickers side by side, so they come from a 440 pt Inspector like the cards.
type-selector-quick-start-before              880x793   38 170  872  681 inspector:#3c3c3c
type-selector-quick-start-after               880x793   38 170  872  681 inspector:#3c3c3c
component-type-selector                       1300x764  34  6 1292  724 inspector
# The introduction's cards: the same pickers in a 440 pt Inspector, so their text reads at the card's width. The empty
# rows between the picker's last item and its footer are cut out of every frame before encoding, for a shorter card.
serializable-type-card                        880x546   38 170  872  534 inspector:#3c3c3c
component-type-selector-card                  880x520   34 170  872  468 inspector:#3c3c3c
aspid_fasttools_serialize_reference_selector_card 880x734 34 170 872  624 inspector:#3c3c3c
# x=10 keeps the foldouts and the coloured shared and missing-reference markers, the same on every capture of the page.
aspid_fasttools_serialize_reference_list         1340x670  10  6 1332  662 inspector:#3c3c3c
aspid_fasttools_serialize_reference_selector     1340x520  10  6 1332  480 inspector
aspid_fasttools_serialize_reference_make_unique  1340x448  10  6 1332  434 inspector
aspid_fasttools_serialize_reference_repair       1340x226  10  6 1332  208 inspector
# EnumValues: the four-row state of every frame, and the picker's lower edge.
enum-values-multipliers-populate              1560x726  28 26 1548  700 flat:#333333:#c8c8c8
enum-values-type-selector                     1080x428  26 26 1064  404 flat:#3c3c3c:#c8c8c8
# The quick start's table, from its outer border.
enum-values-multipliers-quick-start           1190x390  17 16 1175  377 flat:#3c3c3c:#c8c8c8
EOF
exit "$FAILED"

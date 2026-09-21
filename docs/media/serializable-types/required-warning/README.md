# Required type warning crop

`source-inspector.png` preserves the original documentation Inspector capture (1418 × 858, 2× pixel density).

Keep the Weapon Mount component header, Script reference, both selector fields, and Required warning. Remove the window chrome, GameObject header, Transform, and Add Component button. Add 8 source pixels of matching Inspector background above the header, equivalent to 4 px at the capture's logical scale.

```sh
ffmpeg -i source-inspector.png -vf 'crop=1414:216:2:274,pad=1414:224:0:8:color=0x383838' -frames:v 1 type-selector-required.png
```

No scaling or changes to Inspector text or values. The final image is shared by both documentation languages.

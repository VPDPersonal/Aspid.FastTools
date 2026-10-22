# Tooling documentation media

> Provenance only. These are the four states of the repair that the published
> `aspid_fasttools_serialize_reference_tooling.gif` (and its `-light` twin) shows, without a cursor; the published GIF is
> built separately, with a composited cursor. `encode.sh` rebuilds them as `tooling.gif` here.

Re-shot after the window redesign (#442) at commit `4dc4beaf` in Unity 6000.6.5f1, in a hidden batch-mode Editor (no
screen capture). Asset: `Presets/BrokenWeaponPreset.asset` of the SerializeReferences sample (`GhostWeapon` at
`_weapon`, rid 7000). The FastTools window opens on the Asset References tab at 872×634 logical pixels and is grabbed
whole, tab strip and footer included: 1744×1268 PNGs, dark skin, no compositing or padding.

| Frame | State |
|---|---|
| `01-missing.png` | GhostWeapon card with **Fix Missing** |
| `02-picker.png` | Picker open, `Pistol` searched, search field blurred |
| `03-selection.png` | The Pistol result row selected |
| `04-repaired.png` | **No missing references**, the card shows Pistol with **Change** |

At this window size the open picker does not fit: frames 02 and 03 may show a thin scrollbar, and the window footer
covers the card's bottom border. That is how the real window looks at 872×634.

Run `sh Website/media/tooling/encode.sh`; it never writes the published GIF. States last 1.5 / 1 / 0.6 / 1 seconds; `-t 4.1`
prevents the repeated terminal frame from adding an extra pause. Encoding uses 10 fps, a shared 256-color palette and
sierra2_4a dithering at the original pixel dimensions.

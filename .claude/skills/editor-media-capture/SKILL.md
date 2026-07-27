---
name: editor-media-capture
description: Shoot Unity Editor screenshots and GIFs for this package's documentation without the user touching Unity — floating Inspectors, TypeSelectorWindow dropdowns, prefab-mode states, and ffmpeg GIF assembly. Use when a task asks for docs media (PNG/GIF) of editor UI under Documentation/Images, or when scripting EditorWindow interaction headlessly.
user-invocable: false
allowed-tools:
  - Bash
---

# Editor media capture

Driving the Editor is the `unity-pipeline` skill's job; this file covers what it cannot do —
capturing *editor windows* (not the Game/Scene view) and scripting UI states.

For a plain Game or Scene frame, stop here and use
`unity command screenshot --view game|scene` instead. Everything below is for editor chrome.

## Capture without stealing focus

```bash
screencapture -x -o -l <windowID> out.png     # specific window, works while Unity is inactive/occluded
```

- Find window IDs with a small Swift script over `CGWindowListCopyWindowInfo` (no `pyobjc` on this
  machine). Re-query **before every shot** and pick the **highest** id: closed editor windows leave
  blank native ghosts with the same size and title, which `screencapture` will happily capture
  (~13 KB of nothing).
- `-o` drops the drop shadow. Crop the 28 pt (56 px @2x) macOS title bar with `ffmpeg`; `sips -c`
  cannot crop with an offset.
- `screencapture -R x,y,w,h` takes **points** and outputs @2x Retina pixels. It composites by
  z-order, so a floating `EditorWindow` sitting under the main window yields a gray rectangle.
  `win.Focus()` raises it but activates the app once — avoid while the user is typing; window-ID
  capture needs no focus.

## Floating windows

- `CreateInstance<InspectorWindow>()` + `Show()`, then set `position` a **second** time — the first
  assignment is ignored, and the getter returns stale values. Read real bounds from the window list;
  do not trust the setter (macOS cascades floating windows).
- Title bar ≈ 28–30 pt: the capture rect is the content rect grown upward.

## TypeSelectorWindow

- It **survives without app focus**, and
  `EditorWindow.SendEvent(Event.KeyboardEvent("down"|"return"))` drives its keyboard navigation
  headlessly — the full root → namespace → select flow works with zero `cliclick`.
- The root page auto-shows a **Recent** section that breaks scripted down/return navigation. Clear
  `TypeSelectorPreferences.RecentsKey` first and restore the user's entries afterwards — scripted
  selections pollute Recents. Keys: `Aspid.FastTools.TypeSelector.{Favorites|Recents}.<productGUID>`,
  JSON `{"Entries":[aqn,...]}`.
- A non-empty `currentAqn` opens the window already inside that type's namespace page (flat list,
  breadcrumbs); empty starts at root. Favorites/Recent exist only on the root page and open
  collapsed (in-memory state, not persisted).
- Anchor `ShowAsDropDown(screenRect, size)` to the **exact field rect** — it drops flush under
  `screenRect`'s bottom, and a hand-guessed anchor leaves a visible gap. In a 430 pt floating
  Inspector (GO header + Transform + component): value column `x+176`, row `y+154` from content
  top, `248×17`.
- A dropdown opened without OS focus dies non-deterministically 0.5–4 s after opening. Capture each
  state in its **own short eval** (Show → SendEvents fast → hold ≤2.5 s) with an outside burst loop
  (5 shots @ 0.45 s), then composite the states onto the inspector capture with `ffmpeg overlay`.
  `ShowPopup()` clones do not auto-close but usually render **blank** when the app is inactive —
  dead end.
- If you must use `cliclick`: a single synthetic `c:` click on a picker row only hovers, it does
  **not** activate — use `dc:` (double-click). `kp:esc` closes the dropdown. A picker anchored under
  a field overlaps the rows below it, so clicks aimed there land on the picker's search bar.

## Scene / asset staging

- `AssetDatabase.ImportAsset` does **not** recurse into folders (children get no `.meta`) — pass
  `ImportAssetOptions.ImportRecursive | ForceUpdate`.
- A scene instance of a prefab with a missing managed-reference type shows `<None>`, not the notice.
  Shoot missing-type UI in **Prefab Mode** (`PrefabStageUtility.OpenPrefab`).
- `SerializeReferenceSettings.ExcludedFolders` writes through to the **committed**
  `ProjectSettings/SerializeReferenceSharedSettings.asset`. Restore it and check `git diff` after
  using it to de-clutter Project References scans.
- Demo types for docs media live in `Aspid.FastTools/Assets/DevTests/Types/Scripts/DocsMedia/` under
  the user-facing namespace `Game.Combat` — the namespace shows up in picker breadcrumbs, so no
  `DevTests` naming there.

## GIF assembly

`ffmpeg` concat with a per-frame `duration`, then two-pass `palettegen`/`paletteuse`.
**Do not use `-fps_mode vfr`** — it silently collapsed a sequence into 6 identical frames.

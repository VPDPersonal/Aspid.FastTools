# Native selector capture

Replaces the v2 embedded TypeSelectorView screenshot. Genuine TypeSelectorWindow popup anchored by the package TypeField.OnDropdownClicked handler. No manual popup positioning or embedded substitute. The source is a genuine full popup; the final image has its empty middle shortened at the user’s explicit request (see below).

Uses the temporary EnumMedia.Untyped subject from v2: field width 520 at native scale 1, utility window 560×450, positioned at screen (180,100) content origin. capture.cs.txt invokes the actual field click handler, seeds the real search with DamageType and focuses the results list after layout to remove the search caret. The popup retains its native 320-point height and field-aligned width.

macOS screencapture PNG captured the real screen without cursor. Original Retina crop x=380,y=208,width=1080,height=780 from 5120×2880 is preserved as original-region.png; The final output preserves source rows 0–327 and 680–779, joining them to remove only 352 pixels of empty middle space at the user’s explicit request. The native footer, bottom border and shadow are retained; all underlying inspector controls remain intact. The join lies in uniform empty space below the inspector, so it does not cut any controls. Final size 1080×428. crop.sh reproduces the edit from the original. No text, fields or UI values were redrawn. The original full desktop image remains in scratch only. Existing .meta GUID preserved. No changes to the Unity package implementation, scene or assets; temporary popup, window and object closed afterward.

The authentic dark capture is reused for both website themes, as with earlier media.

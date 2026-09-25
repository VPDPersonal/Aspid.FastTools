# EnumValues media, revision 2 — 2026-09-20

Replaces the original populate GIF with visible menu selection. All UI is rendered by Unity 6000.4.0f1; original app-window screenshots are JPEGs from computer-use at 1200×728. Export uses cropping only, no compositing or recolouring. PNG encoding does not restore lossless source pixels.

Capture.cs creates a temporary in-memory object and EditorWindow. It shows the genuine package EnumValues PropertyField at render scale 2.5 (464 logical px width). No scene/assets are saved. The window is shown once; there are no repeated Window.Focus calls.

The genuine ContextualMenuPopulateEvent is dispatched through a capture-only ContextualMenuManager, which displays the package-provided DropdownMenu actions with Unity's stock GenericDropdownMenu inside the window. This uses the UI Toolkit menu presentation instead of the native macOS menu, which the capture tool cannot see. Menu labels render at 24 px. No menu labels/actions are recreated manually. The visible Populate Missing Enum Members row was clicked via computer-use and the real callback added Physical, Ice and Poison, leaving Fire=1.5. Actual Undo.PerformUndo restored Fire alone. This is a paused sequence of genuine captured states, not a continuous pointer recording.

The type-selector image uses EnumValues<float> and the package's actual TypeSelectorView embedded directly below the header in the same temporary window so both appear in one capture. Its normal enum filter plus the transient DamageType type is supplied; search is DamageType, the current row is selected, and focus is moved away from the search text. The capture-only namespace DocumentationMedia is visible. Selector content renders at 2×, width 530, height 120; host background matches Unity's window background. The image is a documentation arrangement of the real controls, not a claim that the popup window is being captured by the OS.

encode.sh crops x=10,y=30,width=1190. GIF height=554; selector image height=390. 9-second loop: initial 1.5s, menu 2.5s, populated 3s, Undo 2s. 10 fps, 256 colours, sierra2_4a. Title-bar cursor is outside every crop.

Dark captures are reused in both website themes because the package's dark-only palette makes a native light capture unreadable (see prior recipe). Temporary windows/objects removed; scene and assets remain untouched. No package implementation is modified.

# Shared sample themes and neutral light EnumValues media

Unity menu: Tools > Aspid > FastTools > Sample Themes. Shared editable Light/Dark palettes are saved in ProjectSettings/AspidFastToolsSampleThemes.asset. Authored removes the preview. The editor helper recognizes the four shipped camera scenes by their SampleFrame component. It previews camera, presentation materials and scene text without changing material assets or storing preview colors in saved scenes. In Light mode, EnumValues uses a temporary cloned SurfacePalette with editable tile/trail colors. Original palette references are restored before saving, changing scenes or entering/exiting Play Mode. Other semantic overrides are left alone. EditorTools is an editor-UI sample and uses the editor/package UI theme instead.

Light palette follows the site's neutral tokens: background #EEF0F3, platform #D3D5DB, edge #B4B7C0, porcelain #CFD9DF, muted #5F636B, accent #A6D8A8, text #2A2C31. Edit these once in the palette window for future recordings.

`source-dark.mkv` and `source-light.mkv` are paired lossless FFV1 recordings of 100 actual camera-rendered PNG frames (1440×810, 4× MSAA, 20 fps). Both start Walker at (-8, 1, 0), reset its direction, tile and trail state, and warm up for 60 frames. `positions.txt` from each recording was compared: all 100 positions match exactly. `encode.sh` crops both at x=169, y=270, width=1101, height=331 and encodes five seconds with 256 colors and sierra2_4a. Existing `.meta` GUIDs are retained.

To repeat in a hidden capture Editor, import EnumValues for the current package version. Copy `prepare-enum.cs.txt` and `record.cs.txt` to temporary `.cs` files for `unity command run_script`. While stopped on a clean scene, call `PrepareEnumCapture.Main` with `["Dark"]` or `["Light"]`, enter Play Mode, then call `SampleThemeRecorder.Main` with that same argument. Wait for `Temp/SampleThemes/enum-<theme>/done.txt`, stop Play Mode, and encode `frame-%03d.png` with `ffmpeg -framerate 20 -i ... -c:v ffv1 -pix_fmt bgr0 source-<theme>.mkv`. Use a fresh output folder for each recording. Restore the original scene and Authored preview after capture.

Validation: Unity compilation completed without errors. validate.cs.txt exercised Light, Dark and Authored on EnumValues, Types, SerializeReferences and ProfilerMarkers, verifying camera restoration, no dirty scenes and unchanged material asset colors. Original SerializeReferences scene restored clean outside Play Mode; capture framerate restored to 0. The temporary recording object and render textures were disposed.

EnumValues light surfaces: tiles Grass #9ABD9F, Stone #BBC0CB, Metal #93A9B5, Water #91BFD5, Sand #DBCAA5; trails #78B795, #B29BD4, #D998A8, #61B4CE, #D4A662 respectively. Captions #455363. The latest source-light.mkv is a fresh Unity recording with these colors, not a recolored image. Original neutral recording retained as source-light-neutral.mkv.

The light capsule and platform accent now use mint #A6D8A8; trail colors are lifted to softer midtones. Previous muted-color footage is retained as source-light-muted.mkv.

Other light samples use Cyan #97C7D8, Grid #B4BDC8 and Animated Color Lift 0.45. Animated property blocks are softened only while the camera renders, then restored; gameplay and authored material colors remain unchanged. New recordings and recipes for Types, SerializeReferences, ProfilerMarkers and EditorTools are in other-samples/.

The recipes open the samples imported for the current package version (Assets/Samples/Aspid.FastTools/<version>/) and write frames and the saved scene path to the Unity project's Temp/SampleThemes/.

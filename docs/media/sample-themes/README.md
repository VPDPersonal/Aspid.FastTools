# Shared sample themes and neutral light EnumValues media

Unity menu: Tools > Aspid > FastTools > Sample Themes. Shared editable Light/Dark palettes are saved in ProjectSettings/AspidFastToolsSampleThemes.asset. Authored removes the preview. The editor helper recognizes the four shipped camera scenes by their SampleFrame component. It previews camera, presentation materials and scene text without changing material assets or storing preview colors in saved scenes. In Light mode, EnumValues uses a temporary cloned SurfacePalette with editable tile/trail colors. Original palette references are restored before saving, changing scenes or entering/exiting Play Mode. Other semantic overrides are left alone. EditorTools is an editor-UI sample and uses the editor/package UI theme instead.

Light palette follows the site's neutral tokens: background #EEF0F3, platform #D3D5DB, edge #B4B7C0, porcelain #CFD9DF, muted #5F636B, accent #A6D8A8, text #2A2C31. Edit these once in the palette window for future recordings.

source-light.mkv is a lossless FFV1 archive of 100 actual camera-rendered PNG frames (1440x810, 4x MSAA, 20 fps), captured by record.cs.txt in the imported EnumValues scene with Light preview. There is a 60-frame warmup before capture; movement, speeds and trail geometry are the live sample behavior. Cropping x=169,y=270,width=1101,height=331 matches the earlier GIF framing. Five seconds, 256 colors, sierra2_4a; no recoloring after rendering. The static scene and gallery previews use full frame 30. Dark media and all .meta GUIDs are retained.

Validation: Unity compilation completed without errors. validate.cs.txt exercised Light, Dark and Authored on EnumValues, Types, SerializeReferences and ProfilerMarkers, verifying camera restoration, no dirty scenes and unchanged material asset colors. Original SerializeReferences scene restored clean outside Play Mode; capture framerate restored to 0. The temporary recording object and render textures were disposed.

EnumValues light surfaces: tiles Grass #9ABD9F, Stone #BBC0CB, Metal #93A9B5, Water #91BFD5, Sand #DBCAA5; trails #78B795, #B29BD4, #D998A8, #61B4CE, #D4A662 respectively. Captions #455363. The latest source-light.mkv is a fresh Unity recording with these colors, not a recolored image. Original neutral recording retained as source-light-neutral.mkv.

The light capsule and platform accent now use mint #A6D8A8; trail colors are lifted to softer midtones. Previous muted-color footage is retained as source-light-muted.mkv.

Other light samples use Cyan #97C7D8, Grid #B4BDC8 and Animated Color Lift 0.45. Animated property blocks are softened only while the camera renders, then restored; gameplay and authored material colors remain unchanged. New recordings and recipes for Types, SerializeReferences, ProfilerMarkers and EditorTools are in other-samples/.

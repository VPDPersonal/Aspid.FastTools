# Editor Helpers documentation capture

Captured from Unity 6000.4.0f1 using a temporary EditorWindow and real calls to `EditorExtensions.GetScriptName` and `GetScriptNameWithIndex`. The AudioSource and BoxCollider components were created in a preview scene, which was closed without changing the active scene. No results were painted or edited into the image.

`capture.cs.txt` is the Unity CLI eval source. After ShowUtility, set the window minSize, maxSize and position to 880×270 logical pixels, then repaint. Capture with computer use. `source.jpg` is the original 880×298 capture returned by the tool; the PNG does not add detail absent from this source.

Export recipe: `ffmpeg -i source.jpg -vf crop=880:240:0:28 -frames:v 1 aspid_fasttools_editor_helpers.png`. This removes the window title bar and bottom rounded corners while retaining the content padding. Do not resize or replace the displayed values.

Final asset: `Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/Images/aspid_fasttools_editor_helpers.png`.

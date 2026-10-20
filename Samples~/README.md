# Samples

Each feature ships with a sample: a small scene or editor tool that does something visible with the feature. Its full tutorial is on the [site](https://vpdpersonal.github.io/Aspid.FastTools/tutorials); the sample description in the Package Manager and the sample card on the **Welcome** tab link to it. Import them from the Package Manager (**Aspid.FastTools → Samples**) or open the **Welcome** tab (`Tools → Aspid 🐍 → FastTools → Welcome`).

The samples are listed in the recommended learning order; each can also be explored independently.

| Sample | What it shows |
|---|---|
| [EnumValues](https://vpdpersonal.github.io/Aspid.FastTools/tutorials/enum-values) | A walker over surface tiles: both `EnumValues` variants, default values, `[Flags]` lookup rules |
| [Types](https://vpdpersonal.github.io/Aspid.FastTools/tutorials/types) | An enemy spawner: `SerializableMonoScript<T>`, `SerializableType<T>`, `[TypeSelectorDisplay]`, a member-referenced `[TypeSelector]`, `ComponentTypeSelector` |
| [SerializeReferences](https://vpdpersonal.github.io/Aspid.FastTools/tutorials/serialize-references) | A turret with polymorphic weapons: the `[SerializeReference]` picker in every field shape, broken assets for the repair tools, an IMGUI inspector |
| [EditorTools](https://vpdpersonal.github.io/Aspid.FastTools/tutorials/editor-tools) | An editor window: fluent `VisualElement` extensions, `SerializedProperty` setters, editor helpers, `TypeSelectorWindow` |
| [ProfilerMarkers](https://vpdpersonal.github.io/Aspid.FastTools/tutorials/profiler-markers) | A flock simulation: the generated marker tree in the Profiler |

## Theme preview

Open `Tools → Aspid 🐍 → FastTools → Sample Themes` and choose **Light**, **Dark**, or **Authored**. The preview works in the EnumValues, Types, SerializeReferences and ProfilerMarkers scenes, including Play Mode; **Authored** restores the scene’s original colors.

Edit the shared palettes in that window before recording. They are saved in `ProjectSettings/AspidFastToolsSampleThemes.asset` and reused across all four samples. For EnumValues, **EnumValues light surfaces** also controls tile, trail and caption colors. The preview uses a temporary copy of the surface palette; saving a scene keeps its original references and material assets.

The light palette also includes **Cyan**, **Grid** and **Animated Color Lift** for softer effect, enemy and flock colors. The Ability Catalog has its own **Theme → Editor / Dark / Light** selector in its header.

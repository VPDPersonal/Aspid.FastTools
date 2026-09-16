# DevTests

Manual harnesses for `tech.aspid.fasttools`, kept in the development project only: nothing here is part
of the package or of its samples, and nothing here ships to users.

The package's own `Tests/Editor` assembly covers the logic, and `Samples~` covers the ordinary,
user-facing flows in UI Toolkit. What is left for this folder is what neither can do: render a drawer
through the IMGUI path, and hold assets that are deliberately broken.

## Types

`TypesIMGUITest` and `TypesUIToolkitTest` carry the same fields — `SerializableType`, its generic and
`SerializableMonoScript` variants, `[TypeSelector]` on strings and arrays, every `TypeAllow` category,
`Required`, and a `nameof` member reference. `TypesIMGUITestEditor` forces the first one through IMGUI,
so `Prefabs/TypesDevTest.prefab` shows both renderings of one field list side by side.

The prefab's `FastEnemy` child covers `ComponentTypeSelector`: `EnemyBaseEditor` is registered with
`editorForChildClasses: true`, so the inspector stays in IMGUI after the component swaps its own type.

`Scripts/DocsMedia` holds the types that appear in the published screenshots of
`Documentation/02-serializable-types.md`. Their namespaces are neutral and game-like because picker
breadcrumbs are visible in the captures; changing a name or a group there invalidates an image.

## SerializeReferences

`SerializeReferencesIMGUITest` and `SerializeReferencesUIToolkitTest` cover the notice states on
`Prefabs/SerializeReferencesDevTest.prefab`: an unset `Required` reference, one instance shared by two
fields, a missing type with Fix and Smart Fix, and a missing element inside a list.

`RequiredViolationsDevTest` leaves a managed reference and a type name unset on purpose, which is what
the Project References window groups under required violations and the Asset References window badges.

This folder is listed under **Excluded folders** in the project's SerializeReference settings, so the
project-wide scan and the `sr_gate` command pass over everything here; the samples carry their own broken
assets for that. Remove the entry to see these fixtures in Project References, and put it back afterwards.

`Scripts/Fixtures` backs three prefabs that exist as files rather than inspectors, because the audit
windows scan assets on disk: `WeaponPreset` (two ordinary references), `SharedWeaponPreset` (one
instance behind both fields) and `BrokenWeaponPreset` (a stored class name that resolves to nothing).

## Enums

`EnumValuesIMGUITest` and `EnumValuesUIToolkitTest` pair the same tables on
`Prefabs/EnumValuesDevTest.prefab`: the untyped variant with its type-picker row, single-line and
multi-field values, `[Flags]` keys, object-reference values, and tables inside an array.

## CliCommands

`sr_gate` exposes the SerializeReference gate scanner to `unity command`, so the scan returns JSON from
the running Editor instead of a batchmode relaunch. It is the only assembly here, because it needs the
package's internals.

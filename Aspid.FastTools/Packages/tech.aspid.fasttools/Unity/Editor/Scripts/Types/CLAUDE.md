# Types — Editor

Editor side of `SerializableType` and `[TypeSelector]`: the drawers, the picker window, and the
constraint resolution behind them. Runtime contracts live in `Unity/Runtime/Types/`.

## `[TypeSelector]` drives two different field shapes

| Field shape | Meaning | Drawn by |
|---|---|---|
| `string` | Assembly-qualified name; also what backs `SerializableType` | this folder (`Drawers/`) |
| `[SerializeReference]` managed reference | Picking a type **instantiates** it | `../SerializeReferences/` |

`TypeSelectorPropertyDrawer` dispatches on `SerializedProperty.propertyType`, so the same attribute
lands in two different code paths. **The managed-reference path is not in this folder** — look under
`Unity/Editor/Scripts/SerializeReferences/` for it.

The candidate list defaults to the field's declared type; a base type narrows it —
`[TypeSelector(typeof(IMelee))]`. Correct usage is enforced at compile time by the analyzer's
`AFT*` rules, so a wrong constraint is a build error, not a silent empty picker.

The `Drawers/` + `Selectors/` + `VisualElements/` split mirrors the sibling `Ids/` feature — see
`../Ids/CLAUDE.md`, which follows the same shape.

---
name: sync-readmes
description: Verify and update Aspid.FastTools README files against the actual codebase — namespaces, public API, CreateAssetMenu paths — keeping EN/RU and root/Documentation copies in sync
user-invocable: true
---

The package ships **eight** README files that drift from the code easily. Use this skill whenever the user asks to "check / update / sync READMEs", or after any change that touches: namespaces of public types, public API surface, `[CreateAssetMenu]` paths, source generator output, or sample structure.

## Files in scope

**Main READMEs (mirror each other 1:1 except for image paths and one heading):**

| Path | Locale | Image base path |
|---|---|---|
| `README.md` | EN | `Aspid.FastTools/Assets/Aspid/FastTools/Documentation/Images/` |
| `README_RU.md` | RU | `Aspid.FastTools/Assets/Aspid/FastTools/Documentation/Images/` |
| `Aspid.FastTools/Assets/Aspid/FastTools/Documentation/README.md` | EN | `Images/` |
| `Aspid.FastTools/Assets/Aspid/FastTools/Documentation/README_RU.md` | RU | `Images/` |

The Documentation copies have an extra `## Source Code` / `## Исходный код` block linking to the GitHub repo — the root copies don't. Otherwise the body is identical character-for-character.

**Sample READMEs (one EN + one RU per sample):**

- `Aspid.FastTools/Assets/Aspid/FastTools/Samples/Types/`
- `Aspid.FastTools/Assets/Aspid/FastTools/Samples/Ids/`
- `Aspid.FastTools/Assets/Aspid/FastTools/Samples/EnumValues/`
- `Aspid.FastTools/Assets/Aspid/FastTools/Samples/ProfilerMarkers/`
- `Aspid.FastTools/Assets/Aspid/FastTools/Samples/VisualElements/`

## Workflow

### 1. Verify against source before editing

For every fact the README states, prove it from the code:

| Claim | Verify with |
|---|---|
| Namespace of a public type | `grep -rn "namespace " <runtime-or-editor-dir> --include="*.cs"` then locate the file declaring the type |
| `[CreateAssetMenu]` menu path | `grep -rn "CreateAssetMenu\|menuName" <dir> --include="*.cs"` |
| Public method signature / return value | Read the source file directly; do not infer from name |
| Generated code shape (`IdStructGenerator`, `ProfilerMarkersGenerator`) | Read `Aspid.FastTools.Generators/Aspid.FastTools.Generators/Generators/.../*Body.cs` |
| Class actually exists | `find … -name "<ClassName>.cs"` — not all helper classes documented in old READMEs still exist (e.g. there is **no** `AspidEditorGUILayout`) |

Common drift points discovered historically:

- **Namespaces split per feature.** Public types live in `Aspid.FastTools` (root: `IId`, `UniqueIdAttribute`, `StringIdRegistry`), `Aspid.FastTools.Types`, `Aspid.FastTools.Enums`, `Aspid.FastTools.Ids`, `Aspid.FastTools.UIElements`. Editor helpers split similarly: `Aspid.FastTools.Editors` for `SerializedProperty` extensions / IMGUI scopes / `GetScriptName`, but per-feature editor code lives in `Aspid.FastTools.{Feature}.Editors`. A `using Aspid.FastTools;` line in a `SerializableType` example is wrong — it must be `using Aspid.FastTools.Types;`.
- **Two ID registries.** `StringIdRegistry` (in `Aspid.FastTools`) keeps int↔string at runtime; `IdRegistry` (in `Aspid.FastTools.Ids`) is int-only at runtime with names stripped from player builds. Don't conflate them. Their menu paths differ: `Aspid/FastTools/String Id Registry` vs `Aspid/FastTools/Id Registry`. `StringIdRegistry.GetId` returns `-1` (not `0`) when not found; the lookup-by-id method is `GetNameId(int)`, not `GetName(int)`. Neither registry exposes public `Add`, `Remove`, or `Rename` — those live behind the registry inspector / `RegistryEditorCore`.
- **Sample asset menu order.** Samples use `Aspid/Samples/FastTools/<Thing>` (Samples first), not `Aspid/FastTools/Samples/<Thing>`. Always re-grep `[CreateAssetMenu]` instead of trusting the existing README.

### 2. Apply edits to all matching files

Most edits hit all four main READMEs (EN root, EN Documentation, RU root, RU Documentation). Apply the same change to each — they must stay textually identical inside their respective body except for the image paths and the `## Source Code` heading.

For RU edits, follow the existing RU translation conventions in the file: `Namespace` → `Пространство имён`, `Description` → `Описание`, code identifiers and English technical terms like `runtime`, `partial struct`, `Inspector` stay in English.

When updating sample READMEs, edit both the EN and RU copy in the same sample folder.

### 3. Sanity-check after editing

Run these commands from the repo root and skim the output:

```bash
# All using statements in samples — these are ground truth for namespaces
grep -rn "^using Aspid" Aspid.FastTools/Assets/Aspid/FastTools/Samples --include="*.cs" | sort -u

# All CreateAssetMenu paths in the package
grep -rn "menuName" Aspid.FastTools/Assets/Aspid/FastTools --include="*.cs"

# Confirm both READMEs of a pair stay aligned (count sections)
grep -c "^## " README.md Aspid.FastTools/Assets/Aspid/FastTools/Documentation/README.md
grep -c "^## " README_RU.md Aspid.FastTools/Assets/Aspid/FastTools/Documentation/README_RU.md
```

Then visually diff each pair of matched files (EN root vs EN Documentation; RU root vs RU Documentation) — only image paths and the `## Source Code` block should differ.

## Arguments

`$ARGUMENTS` (optional):
- empty — full audit and update of all eight READMEs;
- `--check` — audit only, report findings without editing;
- a feature name (`ids`, `types`, `enums`, `visualelements`, `profilermarkers`, `imgui`, `serializedproperty`) — narrow the audit/update to that section.

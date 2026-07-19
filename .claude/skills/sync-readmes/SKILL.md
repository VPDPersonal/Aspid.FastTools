---
name: sync-readmes
description: Verify and update Aspid.FastTools README files against the actual codebase — namespaces, public API, CreateAssetMenu paths — keeping EN/RU and root/Documentation copies in sync
user-invocable: false
---

The package ships **fourteen** README files (4 main + 10 sample) that drift from the code easily. Use after any change touching namespaces of public types, public API surface, `[CreateAssetMenu]` paths, source generator output, or sample structure.

## Files in scope

**Main READMEs (mirror each other 1:1 except for the structural differences below):**

| Path | Locale | Image base path |
|---|---|---|
| `README.md` | EN | `Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/Images/` |
| `README_RU.md` | RU | `Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/Images/` |
| `Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/EN/README.md` | EN | `../Images/` |
| `Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/RU/README.md` | RU | `../Images/` |

Per-feature references (`Types.md`, `SerializeReferences.md`, `Ids.md`, `SerializedPropertyExtensions.md`, `VisualElementExtensions.md`) live alongside the Documentation copies inside `Documentation/EN/` and `Documentation/RU/`.

Expected structural differences between root and Documentation copies (not drift):

- **Badge block** (Unity / Stable / Preview / License shields) — only in the root copies.
- **Language switcher** under the banner — root links `README.md` / `README_RU.md`; Documentation copies link the sibling locale as `../RU/README.md` / `../EN/README.md`.
- **Source-code link row** (`[Source Code](…) · [Unity Asset Store](…) · [Releases](…)`, RU: `Исходный код`) under the one-liner — only in the Documentation copies.
- **LICENSE / CHANGELOG links** (in `## License` / `## Лицензия`) — root uses repo-relative paths; Documentation copies use absolute `github.com/.../blob/main/...` URLs.
- **Image paths** — see table above.
- **Feature-reference links** — root uses the full `Documentation/EN/...` path; Documentation copies link to the bare filename (same folder).

**Sample READMEs** — one EN + one RU per sample folder under `Aspid.FastTools/Packages/tech.aspid.fasttools/Samples~/` (`Types/`, `Ids/`, `EnumValues/`, `ProfilerMarkers/`, `VisualElements/`).

## Workflow

### 1. Verify against source before editing

For every fact the README states, prove it from the code:

| Claim | Verify with |
|---|---|
| Namespace of a public type | `grep -rn "^using Aspid" Aspid.FastTools/Packages/tech.aspid.fasttools/Samples~ --include="*.cs"` (samples are ground truth), or locate the declaring file |
| `[CreateAssetMenu]` menu path | `grep -rn "menuName" <dir> --include="*.cs"` |
| Public method signature / return value | Read the source file directly; do not infer from name |
| Generated code shape | Read `Aspid.FastTools.Generators/Aspid.FastTools.Generators/Generators/.../*Body.cs` |
| Class actually exists | `find … -name "<ClassName>.cs"` — old READMEs document helpers that no longer exist |

Where READMEs historically lie (re-verify these even if the text looks plausible):

- **Namespaces** — they are split per feature (runtime and editor alike); never assume a type sits in the root `Aspid.FastTools`. Check the `using` lines in samples.
- **The two ID registries** — `StringIdRegistry` and `IdRegistry` have different APIs, menu paths, and runtime behavior; don't conflate them — read both sources.
- **Sample asset menu paths** — samples use their own menu segment order; always re-grep `[CreateAssetMenu]` instead of trusting the existing README.

### 2. Apply edits to all matching files

Most edits hit all four main READMEs. Apply the same change to each — bodies must stay textually identical within a locale except for the structural differences listed above. When updating sample READMEs, edit both EN and RU copies in the folder.

RU conventions: follow the file's existing translations (`Namespace` → `Пространство имён`, `Description` → `Описание`); code identifiers and terms like `runtime`, `partial struct`, `Inspector` stay in English.

### 3. Sanity-check after editing

From the repo root:

```bash
# Heading structure of each pair must align (compare # markers, not text)
diff <(grep -oE "^#{1,4} " README.md) <(grep -oE "^#{1,4} " Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/EN/README.md)
diff <(grep -oE "^#{1,4} " README_RU.md) <(grep -oE "^#{1,4} " Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/RU/README.md)
diff <(grep -oE "^#{1,4} " README.md) <(grep -oE "^#{1,4} " README_RU.md)
```

Then diff each pair (EN root vs EN Documentation; RU root vs RU Documentation) — only the expected structural differences should appear.

## Arguments

`$ARGUMENTS` (optional):
- empty — full audit and update of all fourteen READMEs;
- `--check` — audit only, report findings without editing;
- a feature name (`ids`, `types`, `enums`, `visualelements`, `profilermarkers`, `imgui`, `serializedproperty`) — narrow to that section.

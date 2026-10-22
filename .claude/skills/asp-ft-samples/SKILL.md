---
name: asp-ft-samples
description: "Rules for the package samples in Samples~ - what a sample contains, comments and inspector attributes, asmdef namespaces and folder layout, scenes without optional modules, and the site GIFs that show them. Use before adding or changing anything under Samples~ (scripts, asmdefs, scenes, materials, shaders, data assets, style sheets) and when reviewing a sample. For work on this repository only."
---

# Samples

`Samples~` is code a newcomer reads, not package API. These rules come on top of the asp-ft-code-style skill and win
where the two differ. `node scripts/check-samples.mjs` checks the comment, namespace, folder and scene rules; the rest
needs a review.

## Code

- A sample shows one feature and contains only what that feature needs.
- No comments except the `CheckNamespace` banner, and no `[Header]`, `Debug.Log` or XML docs; `[Tooltip]` only where
  it tells more than the field name.
- No `[HideInInspector]` or `[AddComponentMenu("")]`. A value the user should not tune is a `const`.
- Every serialized field with a constant initializer is read in code (CS0414).

## Layout

- Each asmdef sets `rootNamespace`. Its `.cs` files sit in `Scripts/` (an editor asmdef in `Scripts/Editor/`); a
  subfolder of `Scripts/` groups related types and adds its name to the namespace: `Types/Scripts/Enemies/` holds
  `Aspid.FastTools.Samples.Types.Enemies`. Scenes go in `Scenes/`, assets in `Data/`, style sheets in `Styles/`, and
  the visuals in `Materials/`, `Shaders/` (the sample's own `Studio` shader) and `Fonts/` with their licences.

## Scenes and assets

- Scenes need no optional module: no lights, colliders or physics. No sample ships a broken asset.
- The site GIFs show the scenes and the Ability Catalog window: change their look only together with the GIFs.

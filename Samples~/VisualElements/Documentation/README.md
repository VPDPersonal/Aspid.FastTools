# VisualElements Sample

A custom UIToolkit Inspector for an `AbilityConfig` ScriptableObject that showcases the fluent `VisualElement` extension API — building a card-style inspector entirely in code, with a status badge that reacts to Mana Cost edits.

> **New here? Start with [TUTORIAL.md](TUTORIAL.md)** ([RU](TUTORIAL_RU.md)) — a guided, step-by-step tour (Lessons 1–5) built around `Scripts/Editor/VisualElementsTutorialEditor.cs` and `Scenes/VisualElementsTutorial.unity`, covering fluent styling, font presets, layout & composition, reactive fields, and the per-element extensions. This page is the demo-inspector walkthrough; the tutorial teaches the fluent API itself.

Look at:

- `Scripts/Editor/AbilityConfigEditor.cs:23` — `CreateInspectorGUI` composes the card from plain `VisualElement` / `Label` / `HelpBox` chained with the Aspid fluent extensions (`.SetPadding`, `.SetBorderRadius`, `.SetBorderColor`, `.SetFlexDirection`, `.AddChild`).
- `Scripts/Editor/AbilityConfigEditor.cs:38` — `target.GetScriptName()` (from `Aspid.FastTools.Editors`) is used as the header title.
- `Scripts/Editor/AbilityConfigEditor.cs:65` — `PropertyField(...).AddValueChanged(_ => UpdateState())` re-runs the badge / help-box logic on every Mana Cost edit.
- `Scripts/Editor/AbilityConfigEditor.cs:88` — `UpdateState()` flips the badge text/color and toggles `helpBox.SetDisplay(...)` whenever `ManaCost is 0`.

To try it:

1. Select `ScriptableObjects/fireball_1.asset` (paid) or `ScriptableObjects/fireball_free.asset` in the Project window — the custom inspector appears in the Inspector panel.
2. Edit the fields. Set `Mana Cost` to `0` to see the status badge switch to "FREE" and the warning help box appear inline.
3. Or create your own via `Assets > Create > Aspid > FastTools > Samples > Ability Config`.

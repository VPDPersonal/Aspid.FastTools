# Internal editor components

Shared UIToolkit building blocks for this package's own editor UI. Everything here is `internal` —
in an `internal` class members are declared `internal` (or narrower), never `public`.

## Component convention

One subfolder per component under `Components/`, always the same four parts:

```
Components/AspidGradientButton/
├── AspidGradientButton.cs             ← the VisualElement
├── AspidGradientButtonPreset.cs       ← {Name}Preset
├── AspidGradientButtonExtensions.cs   ← fluent extensions
└── Styles/                            ← structs binding USS custom properties
```

- **Every component loads `AspidStyles.DefaultStyleSheet` first**, before its own sheet.
- Enums belong on their `Style` struct as a nested type named `Type` — not at namespace level.
- Styling goes in USS; code only calls `.AddClass()`. Class-name and `--aspid-*` variable grammar
  lives in `../../../Resources/UI/CLAUDE.md` — read it before touching either.

`ICustomStyleExtensions` is **not** here — it ships in runtime, at
`Unity/Runtime/VisualElements/Extensions/ICustomStyle/`.
